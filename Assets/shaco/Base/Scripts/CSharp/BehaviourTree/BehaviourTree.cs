using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class BehaviourTree : BaseTree
    {
        public BehaviourRootTree GetRoot()
        {
            return _root as BehaviourRootTree;
        }

        public void SetParameter(IBehaviourParam param) { GetRootTree().SetParameter(param);  }
        public T GetParameter<T>() where T : IBehaviourParam { return GetRootTree().GetParameter<T>();  }
        public void SetParameters(ICollection<IBehaviourParam> param) { GetRootTree().SetParameters(param); }
        public ICollection<IBehaviourParam> GetParameters() { return GetRootTree().GetParameters(); }
        public bool HasParameter<T>() where T : IBehaviourParam { return GetRootTree().HasParameter<T>(); }
        public bool HasParameters(IBehaviourParam[] keys) { return GetRootTree().HasParameters(keys); }
        public void RemoveParameter<T>() where T : IBehaviourParam { GetRootTree().RemoveParameter<T>();  }
        public void RemoveParameters(IBehaviourParam[] keys) { GetRootTree().RemoveParameters(keys); }

        /// <summary>
        /// 是否要延迟刷新当前任务
        /// 因为可能存在同一帧同时销毁和添加一个任务造成死循环(例如shaco.Base.BehaviourRepeatTree)
        /// 所以允许部分任务类型自动延时刷新
        /// </summary>
        public bool shouldDelayUpdate = false;

        //实行类型名字
        public string processTypeName = string.Empty;

        //显示描述文字
        virtual public string displayName { get { return string.IsNullOrEmpty(_displayName) ? this.GetType().Name : _displayName; } set { _displayName = value; } }
        private string _displayName = string.Empty;

        protected System.Action<BehaviourProcessState> _onProcessResultCallBack = null;
        protected System.Action<BehaviourProcessState> _onAllProcessResultCallBack = null;
        protected System.Action<BehaviourProcessState> _onAllProcessResultCallBackCache = null;
        private IBehaviourProcess _processInterface = null;
        private bool _isRunning = false;
#if UNITY_EDITOR
        private int _runnintRecentFrameCount = 0;
#endif

#if UNITY_EDITOR
        public UnityEngine.Rect editorDrawPosition = new UnityEngine.Rect();
        public UnityEngine.TextAsset editorAssetProcess = null;
        public UnityEngine.TextAsset editorAssetTree
        {
            get
            {
                if (null == _editorAssetTree && !string.IsNullOrEmpty(editorAssetPathTree))
                    _editorAssetTree = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(editorAssetPathTree);
                return _editorAssetTree;
            }
        }
        private UnityEngine.TextAsset _editorAssetTree = null;

        public string editorAssetPathProcess = string.Empty;
        public string editorAssetPathTree = string.Empty;
        public bool editorHasInValidParam = false;
#endif

        /// <summary>
        /// 将节点数据序列化为json数据数组
        /// </summary>
        virtual public shaco.LitJson.JsonData ToJson()
        {
            var retValue = new shaco.LitJson.JsonData();
            retValue.SetJsonType(shaco.LitJson.JsonType.Object);
            retValue["base.FullTypeName"] = GetType().FullName;
            retValue["base.processTypeName"] = processTypeName;
            retValue["base.displayName"] = displayName;
#if UNITY_EDITOR
            retValue["base.editorPath"] = UnityEditor.AssetDatabase.GetAssetPath(this.editorAssetProcess);
            retValue["base.editorTreePath"] = UnityEditor.AssetDatabase.GetAssetPath(this.editorAssetTree);
#endif
            return retValue;
        }

        /// <summary>
        /// 从json数组数组中依次获取节点数据，与ToJson的顺序相对应
        /// </summary>
        virtual public void FromJson(shaco.LitJson.JsonData jsonData)
        {
            if (!jsonData.IsObject)
            {
                Log.Error("BehaviourTree ToJson error: not json object !");
                return;
            }
            this.processTypeName = jsonData["base.processTypeName"].ToString();
            this.displayName = jsonData["base.displayName"].ToString();
#if UNITY_EDITOR
            this.editorAssetPathProcess = jsonData["base.editorPath"].ToString();
            this.editorAssetProcess = UnityEditor.AssetDatabase.LoadAssetAtPath(this.editorAssetPathProcess, typeof(UnityEngine.TextAsset)) as UnityEngine.TextAsset;
            this.editorAssetPathTree = jsonData["base.editorTreePath"].ToString();
#endif
        }

        virtual public void OnGUIDraw()
        {

        }
        
        virtual public bool ShowDefaultDisplayName()
        {
            return true;
        }

        virtual public bool Process()
        {
            var retValue = GetRoot().StartProcess(_processInterface, this);
            return retValue;
        }

        virtual public bool IsRunning()
        {
            return _isRunning;
        }

        virtual public void AddOnAllProcessResultCallBack(System.Action<BehaviourProcessState> callback)
        {
            _onAllProcessResultCallBackCache += callback;
        }

        virtual public void OnAllProcessResult(BehaviourProcessState state)
        {
            if (null != _onAllProcessResultCallBack)
            {
                try
                {
                    _onAllProcessResultCallBack(state);
                }
                catch (System.Exception e)
                {
                    Log.Error("BehaviourRootTree OnAllProcessResult error: callback=" + _onAllProcessResultCallBack + " e=" + e);
                }
                _onAllProcessResultCallBack = null;
            }
            else if (this.Count > 0)
                Log.Error("BehaviourTree OnAllProcessResult error: missing callback, tree=" + this + " process=" + this.processTypeName + " display=" + this.displayName);
        }

        virtual public void OnProcessResult(BehaviourProcessState state)
        {
            if (null != _onProcessResultCallBack)
            {
                try
                {
                    _onProcessResultCallBack(state);
                }
                catch (System.Exception e)
                {
                    Log.Error("BehaviourRootTree OnProcessResult error: callback=" + _onProcessResultCallBack + " e=" + e);
                }
            }
            else if (this.Count > 0)
                Log.Error("BehaviourTree OnProcessResult error: missing callback, tree=" + this + " process=" + this.processTypeName + " display=" + this.displayName);

        }

        public void StartRunning()
        {
            _isRunning = true;
#if UNITY_EDITOR
            _runnintRecentFrameCount = 10;
#endif
            UpdateAllProcessResultCallBackCache();
        }

        public void StopRunning()
        {
            _isRunning = false;
            _onProcessResultCallBack = null;
        }

#if UNITY_EDITOR
        //最近几帧率有运行过，只在编辑器上有使用到
        public bool IsRunningRecentInEditor()
        {
            if (!_isRunning && _runnintRecentFrameCount > 0)
            {
                --_runnintRecentFrameCount;
            }
            return _runnintRecentFrameCount > 0;
        }
#endif

        public void ForeachChildren(System.Func<BehaviourTree, bool> callback)
        {
            ForeachChildren((BaseTree tree) =>
            {
                return callback(tree as BehaviourTree);
            });
        }

        public void ForeachAllChildren(System.Func<BehaviourTree, int, int, bool> callback)
        {
            ForeachAllChildren((BaseTree tree, int index, int level) =>
            {
                return callback(tree as BehaviourTree, index, level);
            });
        }

        public void BindProcess<T>() where T : IBehaviourProcess, new()
        {
            this._processInterface = new T();
            this.processTypeName = this._processInterface.ToTypeString();
        }

        public bool InitProcees()
        {
            if (!string.IsNullOrEmpty(this.processTypeName))
            {
                var process = (IBehaviourProcess)shaco.Base.Utility.Instantiate(this.processTypeName);
                if (null == process)
                {
                    Log.Error("BehaviourTree InitProcees erorr: can't find script name=" + this.processTypeName + " display name=" + this.displayName);
                    return false;
                }
                else
                    this._processInterface = process;
                return true;
            }
            else
                return false;
        }

        protected void AddOnProcessResultCallBack(System.Action<BehaviourProcessState> callback)
        {
            _onProcessResultCallBack += callback;
        }

        /// <summary>
        /// 从回调缓存中取出之前添加的回调方法，以防止在Process递归中再被添加回调导致回调意外丢失的问题
        /// </summary>
        protected void UpdateAllProcessResultCallBackCache()
        {
            _onAllProcessResultCallBack = _onAllProcessResultCallBackCache;
            _onAllProcessResultCallBackCache = null;
        }

        protected BehaviourTree()
        {

        }

        private BehaviourRootTree GetRootTree()
        {
            var retValue = this as BehaviourRootTree;
            return null == retValue ? this.GetRoot() : retValue;
        }
    }
}
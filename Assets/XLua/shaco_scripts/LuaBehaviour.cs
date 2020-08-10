using System;
using UnityEngine;
using XLua;

namespace shaco
{
    public class LuaBehaviour : MonoBehaviour, shaco.IUIAnimation
    {
        [System.Serializable]
        public class ScriptAssetPath
        {
#if UNITY_EDITOR
            public TextAsset luaScriptAsset = null;
#endif
            public string luaScriptAssetPath = string.Empty;

            public string multiVersionControlRelativePath = string.Empty;

            public string luaScriptAssetGUID = string.Empty;
        }

        public LuaTable lua { get { return _classTable; } }

        [SerializeField]
        private ScriptAssetPath _scriptAssetPath = new ScriptAssetPath();

        //Unity life circle
        private System.Action _luaUpdate;
        private System.Action _luaLateUpdate;
        private System.Action _luaFixedUpdate;
        private System.Action _luaOnEnable;
        private System.Action _luaOnDisable;
        private System.Action _luaOnGUI;

        //shacoGameFramework life circle
        private System.Action _luaOnUIPreLoad;
        private System.Action<shaco.Base.BaseEventArg> _luaOnUIInit;
        private System.Action<shaco.Base.BaseEventArg> _luaOnUIOpen;
        private System.Action _luaOnUIHide;
        private System.Action _luaOnUIResume;
        private System.Action _luaOnUIClose;
        private System.Action _luaOnUIBringToFront;
        private System.Action<shaco.Base.BaseEventArg> _luaOnUIRefresh;
        private System.Action<System.Action> _luaRunOpenAnimation;
        private System.Action<System.Action> _luaRunCloseAnimation;

        private LuaTable _scriptEnv;
        private LuaTable _classTable;
        private LuaEnv _luaenv;
        private bool _isWillCloseWhenDoLuaStringError = false;

        virtual public void OnUIPreLoad()
        {
            ExecuteCallBack(_luaOnUIPreLoad);
        }

        virtual public void OnUIInit(shaco.Base.BaseEventArg arg)
        {
            ExecuteCallBack(_luaOnUIInit, arg);
        }

        virtual public void OnUIOpen(shaco.Base.BaseEventArg arg)
        {
            if (_isWillCloseWhenDoLuaStringError)
            {
                _isWillCloseWhenDoLuaStringError = false;
                this.CloseMe();
                return;
            }
            ExecuteCallBack(_luaOnUIOpen, arg);
        }

        virtual public void OnUIHide()
        {
            ExecuteCallBack(_luaOnUIHide);
        }

        virtual public void OnUIResume()
        {
            ExecuteCallBack(_luaOnUIResume);
        }

        virtual public void OnUIClose()
        {
            ExecuteCallBack(_luaOnUIClose);
        }

        virtual public void OnUIRefresh(shaco.Base.BaseEventArg arg)
        {
            ExecuteCallBack(_luaOnUIRefresh, arg);
        }

        virtual public void OnUIBringToFront()
        {
            ExecuteCallBack(_luaOnUIBringToFront);
        }

        public void SetPath(string path)
        {
#if UNITY_EDITOR
            _scriptAssetPath.luaScriptAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (null == _scriptAssetPath.luaScriptAsset)
            {
                Log.Error("LuaBehaviour SetPath error: can't load 'TextAsset' at path=" + path);
                return;
            }

            var newGUID = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(newGUID))
            {
                Log.Error("LuaBehaviour SetPath error: can't get guid at path=" + path);
                _scriptAssetPath.luaScriptAsset = null;
                return;
            }

            _scriptAssetPath.luaScriptAssetGUID = newGUID;
#endif
            _scriptAssetPath.luaScriptAssetPath = path;
        }

        public string GetPath()
        {
            return _scriptAssetPath.luaScriptAssetPath;
        }

        public string GetGUID()
        {
            return _scriptAssetPath.luaScriptAssetGUID;
        }

        virtual protected void Awake()
        {
            if (string.IsNullOrEmpty(_scriptAssetPath.luaScriptAssetPath))
            {
                Log.Error("LuaBehaviour Awake error: invalid script path", this);
                return;
            }

            _luaenv = XLuaManager.luaenv;
            _scriptEnv = _luaenv.NewTable();

            //为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
            LuaTable meta = _luaenv.NewTable();
            meta.Set("__index", _luaenv.Global);
            _scriptEnv.SetMetaTable(meta);
            meta.Dispose();

            _scriptEnv.Set("self", this);

            var loadAsset = shaco.GameHelper.res.LoadResourcesOrLocal<UnityEngine.Object>(_scriptAssetPath.luaScriptAssetPath, _scriptAssetPath.multiVersionControlRelativePath);
            if (null == loadAsset)
            {
                Log.Error("LuaBehaviour Awake erorr: Lua script was not found. the name of script may have been modified or its location has been moved\nPlease right-click on Lua file or folder to open the menu and click 'FixLuaBehaviourScriptPath' button to fix it");
                return;
            }

            var luaScript = loadAsset.ToString();

            try
            {
                _luaenv.DoString(luaScript, this.GetType().FullName, _scriptEnv);
            }
            catch (System.Exception e)
            {
                //如果加载lua脚本发生错误，则立即关闭当前的界面
                var errorString = e.ToString();
                var findFileLineStr = errorString.Substring("]:", ":");
                if (!string.IsNullOrEmpty(findFileLineStr))
                {
                    var findLine = findFileLineStr.ToInt();
                    var locationString = XLuaManager.GetLuaScriptPath(_scriptAssetPath.luaScriptAssetPath, findLine);
                    Log.Error("LuaBehaviour lua script error: " + locationString + " e=" + e);
                }
                else
                    Log.Error("LuaBehaviour lua script error: " + _scriptAssetPath.luaScriptAssetPath + " e=" + e);
                _isWillCloseWhenDoLuaStringError = true;
                return;
            }

            var typeName = this.GetType().Name;
            _scriptEnv.Get(typeName, out _classTable);

            if (null == _classTable)
            {
                Log.Error(string.Format("LuaBehaviour Awake error: not foud global tabel '{0}' in path '{1}'", typeName, _scriptAssetPath.luaScriptAssetPath));
                return;
            }

            _luaUpdate = _classTable.Get<System.Action>("Update");
            _luaFixedUpdate = _classTable.Get<System.Action>("FixedUpdate");
            _luaLateUpdate = _classTable.Get<System.Action>("LateUpdate");
            _luaOnEnable = _classTable.Get<System.Action>("OnEnable");
            _luaOnDisable = _classTable.Get<System.Action>("OnDisable");
            _luaOnGUI = _classTable.Get<System.Action>("OnGUI");

            _luaOnUIPreLoad = _classTable.Get<System.Action>("OnUIPreLoad");
            _luaOnUIInit = _classTable.Get<System.Action<shaco.Base.BaseEventArg>>("OnUIInit");
            _luaOnUIOpen = _classTable.Get<System.Action<shaco.Base.BaseEventArg>>("OnUIOpen");
            _luaOnUIHide = _classTable.Get<System.Action>("OnUIHide");
            _luaOnUIResume = _classTable.Get<System.Action>("OnUIResume");
            _luaOnUIClose = _classTable.Get<System.Action>("OnUIClose");
            _luaOnUIRefresh = _classTable.Get<System.Action<shaco.Base.BaseEventArg>>("OnUIRefresh");
            _luaOnUIBringToFront = _classTable.Get<System.Action>("OnUIBringToFront");
            _luaRunOpenAnimation = _classTable.Get<System.Action<System.Action>>("RunOpenAnimation");
            _luaRunCloseAnimation = _classTable.Get<System.Action<System.Action>>("RunCloseAnimation");

            var luaAwake = _classTable.Get<System.Action>("Awake");
            ExecuteCallBack(luaAwake);
        }

        virtual protected void Start()
        {
            if (_isWillCloseWhenDoLuaStringError)
            {
                _isWillCloseWhenDoLuaStringError = false;
                MonoBehaviour.Destroy(this.gameObject);
                return;
            }

            if (null != _classTable)
            {
                var luaStart = _classTable.Get<System.Action>("Start");
                ExecuteCallBack(luaStart);
            }
        }

        virtual protected void OnEnable()
        {
            ExecuteCallBack(_luaOnEnable);
        }

        virtual protected void OnDisable()
        {
            ExecuteCallBack(_luaOnDisable);
        }

        virtual protected void Update()
        {
            ExecuteCallBack(_luaUpdate);
            _luaenv.Tick();
        }

        virtual protected void FixedUpdate()
        {
            ExecuteCallBack(_luaFixedUpdate);
        }


        virtual protected void LateUpdate()
        {
            ExecuteCallBack(_luaLateUpdate);
        }

        virtual protected void OnGUI()
        {
            ExecuteCallBack(_luaOnGUI);
        }

        virtual protected void OnDestroy()
        {
            if (null != _classTable)
            {
                var luaOnDestroy = _classTable.Get<System.Action>("OnDestroy");
                ExecuteCallBack(luaOnDestroy);
            }

            _luaUpdate = null;
            _luaLateUpdate = null;
            _luaFixedUpdate = null;
            _luaOnEnable = null;
            _luaOnDisable = null;
            _luaOnGUI = null;

            _luaOnUIPreLoad = null;
            _luaOnUIInit = null;
            _luaOnUIOpen = null;
            _luaOnUIHide = null;
            _luaOnUIResume = null;
            _luaOnUIClose = null;
            _luaOnUIRefresh = null;
            _luaOnUIBringToFront = null;
            if (null != _classTable)
                _classTable.Dispose();
            if (null != _scriptEnv)
                _scriptEnv.Dispose();

            //销毁加载的lua脚本
            if (shaco.GameHelper.resCache.loadedCount > 0)
                shaco.GameHelper.res.UnloadAssetBundleLocal(_scriptAssetPath.luaScriptAssetPath, true, _scriptAssetPath.multiVersionControlRelativePath);
        }

        private void ExecuteCallBack(System.Action callback)
        {
            try
            {
                if (null != callback)
                    callback();
            }
            catch (System.Exception e)
            {
                Log.Error("LuaBehaviour callback erorr1: e=" + e + "\n" + GetLuaScriptFullPath(_scriptAssetPath.luaScriptAssetPath, e));
            }
        }

        private void ExecuteCallBack(System.Action<shaco.Base.BaseEventArg> callback, shaco.Base.BaseEventArg arg)
        {
            try
            {
                if (null != callback)
                    callback(arg);
            }
            catch (System.Exception e)
            {
                Log.Error("LuaBehaviour callback erorr2: e=" + e + "\n" + GetLuaScriptFullPath(_scriptAssetPath.luaScriptAssetPath, e));
            }
        }

        /// <summary>
        /// 获取lua脚本的全路径
        /// </summary>
        private string GetLuaScriptFullPath(string replativePath, System.Exception e)
        {
            var retValue = string.Empty;
#if UNITY_EDITOR
            var indexFind = Application.dataPath.LastIndexOf("Assets");
            var errorString = e.ToString();
            indexFind = errorString.IndexOf("XLua.LuaException:") + "XLua.LuaException:".Length;
            indexFind = errorString.IndexOf(":", indexFind) + 1;

            var lineAppendStr = new System.Text.StringBuilder();
            for (int i = indexFind; i < errorString.Length; ++i)
            {
                var cTmp = errorString[i];
                if (cTmp >= '0' && cTmp <= '9')
                    lineAppendStr.Append(cTmp);
                else
                    break;
            }

            //没找到堆栈信息
            if (lineAppendStr.Length == 0)
                return replativePath;

            var line = int.Parse(lineAppendStr.ToString());
            retValue = replativePath + ":" + line;
            return string.Format("<color=#{0}>({1} at {2})</color>", shaco.Base.GlobalParams.FORCE_LOG_LOCATION_COLOR, shaco.Base.GlobalParams.FORCE_LOG_LOCATION_TAG, retValue);
#else
            return replativePath;
#endif
        }

        public void RunOpenAnimation(System.Action callbackEnd)
        {
            if (null != _luaRunOpenAnimation)
                _luaRunOpenAnimation(callbackEnd);
            else
                callbackEnd();
        }

        public void RunCloseAnimation(System.Action callbackEnd)
        {
            if (null != _luaRunCloseAnimation)
                _luaRunCloseAnimation(callbackEnd);
            else
                callbackEnd();
        }
    }
}
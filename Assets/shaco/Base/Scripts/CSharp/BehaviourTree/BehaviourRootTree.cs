using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public enum BehaviourProcessState
    {
        Normal,
        Continue,
        Stop
    }

    [BehaviourProcessTree(typeof(BehaviourRootTree))]
    public partial class BehaviourRootTree : BehaviourTree
    {
        public enum UpdateEnumeratorResult
        {
            AllProcessPause,
            AllProcessResume,
            Processing,
            ProceessEnd,
            StopAllProcess,
        }

        private class EnumeratorInfo : shaco.Base.IObjectPoolData
        {
            public IBehaviourProcess process = null;
            public IEnumerator enumerator = null;
            public BehaviourTree tree = null;

            public void Dispose()
            {
                process = null;
                enumerator = null;
                tree = null;
            }
        }

        private Dictionary<int, IBehaviourParam> _parameters = new Dictionary<int, IBehaviourParam>();
        private List<EnumeratorInfo> _enumerators = new List<EnumeratorInfo>();
        private List<EnumeratorInfo> _willAddEnumerators = new List<EnumeratorInfo>();
        private List<EnumeratorInfo> _willRemoveEnumerators = new List<EnumeratorInfo>();
        private bool _isRequestStopProcess = false;
        private bool _isPaused = false;

        public bool LoadFromJsonPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            bool retValue = BehaviourTreeConfig.LoadFromJsonPath(path, this);
            this.InitProcesses();
            return retValue;
        }

        public bool LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return false;

            bool retValue = BehaviourTreeConfig.LoadFromJson(json, this);
            this.InitProcesses();
            return retValue;
        }

        public void SaveToJson(string path)
        {
            BehaviourTreeConfig.SaveToJson(this, path);
        }

        public BehaviourRootTree()
        {

        }

        public void Update(float elapseSeconds)
        {
            if (!IsRunning() || _isPaused)
                return;

            UpdateEnumerator(elapseSeconds);

            if (_isRequestStopProcess)
            {
                _isRequestStopProcess = false;
                ClearCache();
                this.StopUpdateSelf();
            }
            else 
            {
                if (_enumerators.Count == 0 && _willAddEnumerators.Count == 0)
                {
                    this.Process();

                    //execute children process
                    ForeachChildren((BehaviourTree tree) =>
                    {
                        tree.Process();
                        return true;
                    });
                }
            }
        }

        public void Stop()
        {
            this._isRequestStopProcess = true;
        }

        override public bool Process()
        {
            //根节点不做执行任务处理
            // this.AddOnProcessResultCallBack((bool isStoped) =>
            // {
            //     if (!isStoped)
            //     {
            //         ForeachChildren((shaco.Base.BehaviourTree tree) =>
            //         {
            //             tree.Process();
            //             return true;
            //         });
            //     }
            // });
            return base.Process();
        }

        override public bool IsRunning()
        {
            return base.IsRunning() && !_isPaused;
        }

        // override public void AddOnAllProcessResultCallBack(System.Action<BehaviourProcessState> callback)
        // {
        //     _onAllProcessResultCallBack += callback;
        // }

        override public void OnAllProcessResult(BehaviourProcessState state)
        {
            Log.Error("BehaviourRootTree OnAllProcessResult error: not support function");
        }

        override public void OnProcessResult(BehaviourProcessState state)
        {
            Log.Error("BehaviourRootTree OnProcessResult error: not support function");
        }

        new public void SetParameter(IBehaviourParam param)
        {
            if (null == param)
            {
                Log.Error("BehaviourRootTree SetParameter error: param is null");
                return;
            }
            _parameters[param.GetType().GetHashCode()] = param;
        }

        new public T GetParameter<T>() where T : IBehaviourParam
        {
            var key = typeof(T).GetHashCode();
            if (_parameters.ContainsKey(key))
            {
                return (T)_parameters[key];
            }
            else
            {
                return default(T);
            }
        }

        new public ICollection<IBehaviourParam> GetParameters()
        {
            return _parameters.Values;
        }

        new public void SetParameters(ICollection<IBehaviourParam> param)
        {
            foreach (var p in param)
            {
                SetParameter(p);
            }
        }

        new public bool HasParameter<T>() where T : IBehaviourParam
        {
            var key = typeof(T).GetHashCode();
            return _parameters.ContainsKey(key);
        }

        new public bool HasParameters(IBehaviourParam[] keys)
        {
            bool retValue = false;
            for (int i = keys.Length - 1; i >= 0; --i)
            {
                var key = keys[i].GetType().GetHashCode();
                if (_parameters.ContainsKey(key))
                {
                    retValue = true;
                    break;
                }
            }
            return retValue;
        }

        new public void RemoveParameter<T>() where T : IBehaviourParam
        {
            var key = typeof(T).GetHashCode();
            if (!_parameters.ContainsKey(key))
            {
                Log.Error("BehaviourRootTree RemoveParameter error: not found parameter by key=" + shaco.Base.Utility.ToTypeString<T>());
            }
            else
            {
                _parameters.Remove(key);
            }
        }

        new public void RemoveParameters(IBehaviourParam[] keys)
        {
            for (int i = keys.Length - 1; i >= 0; --i)
            {
                var key = keys[i].GetType().GetHashCode();
                if (!_parameters.ContainsKey(key))
                {
                    Log.Error("BehaviourRootTree RemoveParameters error: not found parameter by key=" + key);
                }
                else
                {
                    _parameters.Remove(key);
                }
            }
        }

        public void ClearParameter()
        {
            _parameters.Clear();
        }

        public bool StartProcess(IBehaviourProcess process, BehaviourTree tree)
        {
            bool retValue = false;

            //根节点不执行任何逻辑
            if (tree.IsRoot())
            {
                return retValue;
            }

            //当执行任务为空时候，使用默认任务
            if (null == process)
            {
                process = shaco.BehaviourProcess.BehaviourDefaultProcess.Default;
            }

            var infoNew = shaco.Base.GameHelper.objectpool.Instantiate(() => new EnumeratorInfo());
            infoNew.process = process;

            infoNew.enumerator = process.Process(tree);
            infoNew.tree = tree;

            _willAddEnumerators.Add(infoNew);
            retValue = true;
            return retValue;
        }
        private void InitProcesses()
        {
            this.ForeachAllChildren((BehaviourTree tree, int index, int level) =>
            {
                tree.InitProcees();
                return true;
            });
        }

        //计算迭代器任务(IBehaviourEnumerator)
        private void UpdateEnumerator(float elapseSeconds)
        {
            int countTmp = _enumerators.Count;
            bool shouldBreak = false;
            for (int i = 0; i < countTmp; ++i)
            {
                UpdateEnumeratorResult resultTmp = UpdateEnumeratorResult.Processing;
                GameHelper.profiler.BeginSample("UpdateEnumerator: name=" 
                                    + (null == _enumerators[i] || null == _enumerators[i].tree ? "null" : _enumerators[i].tree.displayName) 
                                    + " type=" + (null == _enumerators[i] || null == _enumerators[i].tree ? "null" : _enumerators[i].tree.processTypeName));
                {
                    resultTmp = UpdateEnumerator(elapseSeconds, _enumerators[i]);
                }
                GameHelper.profiler.EndSample();

                switch (resultTmp)
                {
                    case UpdateEnumeratorResult.AllProcessPause:
                        {
                            _isPaused = true;
                            shouldBreak = true;
                            break;
                        }
                    case UpdateEnumeratorResult.AllProcessResume:
                        {
                            _isPaused = false;
                            shouldBreak = true;
                            break;
                        }
                    case UpdateEnumeratorResult.StopAllProcess:
                        {
                            _isRequestStopProcess = true;
                            shouldBreak = true;
                            break;
                        }
                    case UpdateEnumeratorResult.Processing:
                    case UpdateEnumeratorResult.ProceessEnd:
                        {
                            break;
                        }
                    default: Log.Error("BehaviourRootTree UpdateEnumerator error: unsupport type=" + resultTmp); break;
                }
                if (shouldBreak)
                    break;
            }

            if (_isRequestStopProcess) 
                return;

            //执行完毕即将被移除执行队列的任务
            if (_willRemoveEnumerators.Count > 0)
            {
                for (int i = _willRemoveEnumerators.Count - 1; i >= 0; --i)
                {
                    var enumeratorInfo = _willRemoveEnumerators[i];
                    var currentEnumerator = null != enumeratorInfo.enumerator ? enumeratorInfo.enumerator.Current as IBehaviourEnumerator : null;

                    if (currentEnumerator == null)
                    {
                        //丢失了任务，可能是yield return null
                        //这里需要手动通知当前任务任务，并继续执行后续任务
                        enumeratorInfo.tree.OnProcessResult(BehaviourProcessState.Normal);
                        if (enumeratorInfo.tree.Count == 0)
                            enumeratorInfo.tree.OnAllProcessResult(BehaviourProcessState.Normal);
                    }
                    enumeratorInfo.tree.StopRunning();

                    enumeratorInfo.process.Dispose();
                    _enumerators.Remove(enumeratorInfo);
                    shaco.Base.GameHelper.objectpool.RecyclingObject(enumeratorInfo);
                }
                _willRemoveEnumerators.Clear();
            }

            //检查需要添加的任务
            if (_willAddEnumerators.Count > 0)
            {
                bool shouldDelayUpdate = false;
                for (int i = _willAddEnumerators.Count - 1; i >= 0; --i)
                {
                    var iterValue = _willAddEnumerators[i];
                    if (!_enumerators.Contains(iterValue))
                    {
                        _enumerators.Add(iterValue);
                        shouldDelayUpdate |= iterValue.tree.shouldDelayUpdate;
                    }
                }
                _willAddEnumerators.Clear();

                if (!shouldDelayUpdate)
                    UpdateEnumerator(elapseSeconds);
            }
        }

        /// <summary>
        /// 自动重置结束的任务枚举，并自动跳转到下一个枚举
        /// <param name="enumerator">枚举对象</param>
        /// </summary>
        public bool AutoResetAndMoveNext(IEnumerator enumerator)
        {
            if (null != enumerator)
            {
                var behaviourEnumerator = (IBehaviourEnumerator)(enumerator.Current);
                if (null != behaviourEnumerator)
                {
                    behaviourEnumerator.Reset();
                }
            }

            try
            {
                return null != enumerator && enumerator.MoveNext();
            }
            catch (System.Exception e)
            {
                Log.Error("BehavirourRootTree AutoResetAndMoveNext erorr: e=" + e);
                return false;
            }
        }

        private UpdateEnumeratorResult UpdateEnumerator(float elapseSeconds, EnumeratorInfo enumeratorInfo)
        {
            var currentEnumerator = enumeratorInfo.enumerator;
            UpdateEnumeratorResult retValue = UpdateEnumeratorResult.Processing;

            //处理首次任务
            if (null == currentEnumerator.Current)
            {
                AutoResetAndMoveNext(currentEnumerator);
                enumeratorInfo.tree.StartRunning();
            }

            //处理被动和主动停止任务
            if (currentEnumerator.Current == null || !enumeratorInfo.tree.IsRunning())
            {
                retValue = HanldeWillRemoveEnumerator(enumeratorInfo.enumerator, enumeratorInfo);
                _willRemoveEnumerators.Add(enumeratorInfo);
            }
            else
            {
                //处理任务进度
                var behaviourEnumerator = (IBehaviourEnumerator)(currentEnumerator.Current);
                if (!behaviourEnumerator.IsRunning())
                {
                    //处理任务进度，自动跳转
                    retValue = MoveNextProcess(enumeratorInfo);
                }
                else
                {
                    behaviourEnumerator.Update(elapseSeconds);
                }
            }
            return retValue;
        }

        /// <summary>
        /// 清理咋用的缓存和内存池信息
        /// </summary>
        private void ClearCache()
        {
            for (int i = _enumerators.Count - 1; i >= 0; --i)
            {
                var behaviourEnumerator = (IBehaviourEnumerator)(_enumerators[i].enumerator.Current);
                if (null != behaviourEnumerator)
                {
                    behaviourEnumerator.Reset();
                }
                _enumerators[i].process.Dispose();
                shaco.Base.GameHelper.objectpool.DestroyObject(_enumerators[i]);
            }

            for (int i = _willAddEnumerators.Count - 1; i >= 0; --i)
            {
                shaco.Base.GameHelper.objectpool.DestroyObject(_willAddEnumerators[i]);
            }

            ForeachAllChildren((child, index, level) =>
            {
                child.StopRunning();
                return true;
            });

            _enumerators.Clear();
            _willAddEnumerators.Clear();
            _willRemoveEnumerators.Clear();
            _parameters.Clear();
            this.StopRunning();

            if (null == _onAllProcessResultCallBack && null != _onAllProcessResultCallBackCache)
                _onAllProcessResultCallBack = _onAllProcessResultCallBackCache;
            if (null != _onAllProcessResultCallBack)
            {
                try
                {
                    _onAllProcessResultCallBack(BehaviourProcessState.Stop);
                }
                catch (System.Exception e)
                {
                    Log.Error("BehaviourRootTree ClearCache error: callback=" + _onAllProcessResultCallBack + " e=" + e);
                }
                _onAllProcessResultCallBack = null;
            }
        }

        private UpdateEnumeratorResult HanldeWillRemoveEnumerator(IEnumerator enumerator, EnumeratorInfo value)
        {
            UpdateEnumeratorResult retValue = UpdateEnumeratorResult.ProceessEnd;
            var currentEnumerator = null != enumerator ? enumerator.Current as IBehaviourEnumerator : null;
            if (currentEnumerator != null)
            {
                if (value.tree.IsRunning())
                {
                    retValue = currentEnumerator.OnRunningEnd(value.tree);
                }
                currentEnumerator.Dispose();
            }
            return retValue;
        }

        //自动跳转任务进度
        private UpdateEnumeratorResult MoveNextProcess(EnumeratorInfo iterValue)
        {
            UpdateEnumeratorResult retValue = UpdateEnumeratorResult.Processing;
            var nextEnumerator = iterValue.enumerator;
            var oldEnumerator = nextEnumerator.Current as IBehaviourEnumerator;

            //case 1:创建时候就马上停止的逻辑点
            //case 2:自动跳转任务，如果跳转失败，则继续下阶段逻辑点
            var currentEnumerator = nextEnumerator.Current as IBehaviourEnumerator;
            if ((null != currentEnumerator && currentEnumerator.IsIgnoreNextEnumerator()) || !AutoResetAndMoveNext(nextEnumerator))
            {
                retValue = HanldeWillRemoveEnumerator(iterValue.enumerator, iterValue);
                _willRemoveEnumerators.Add(iterValue);
            }
            else
            {
                //任务步骤变化了表示跳转到下个逻辑点，所以要回收上一个逻辑点资源
                if (null != oldEnumerator && oldEnumerator != nextEnumerator.Current)
                {
                    oldEnumerator.Dispose();
                }
            }
            return retValue;
        }
    }
}
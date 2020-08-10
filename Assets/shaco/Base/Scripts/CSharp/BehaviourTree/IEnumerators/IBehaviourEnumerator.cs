namespace shaco.Base
{
    public abstract class IBehaviourEnumerator : System.Collections.IEnumerator
    {
        public virtual object Current { get { return this; } }

        public virtual void Update(float elapseSeconds) { }

        public virtual bool MoveNext() { return true; }

        public abstract bool IsRunning();

        public abstract void Reset();

        /// <summary>
        /// 任务逻辑点结束时候回调，其他逻辑点可以继承该方法实现不同的逻辑执行方案
        /// <param name="tree">逻辑树节点</param>
        /// </summary>
        public virtual BehaviourRootTree.UpdateEnumeratorResult OnRunningEnd(BehaviourTree tree)
        {
            tree.OnProcessResult(BehaviourProcessState.Normal);
            if (tree.Count == 0)
            {
                tree.OnAllProcessResult(BehaviourProcessState.Normal);
            }
            return BehaviourRootTree.UpdateEnumeratorResult.ProceessEnd;
        }

        /// <summary>
        /// 是否忽略后续的逻辑点，强制结束当前逻辑
        /// </summary>
        public virtual bool IsIgnoreNextEnumerator()
        {
            return false;
        }

        public void Dispose()
        {
            shaco.Base.GameHelper.objectpool.RecyclingObject(this);
        }

        static public implicit operator IBehaviourEnumerator(int frameCount)
        {
            return WaitForFrame.Create(frameCount);
        }

        static public implicit operator IBehaviourEnumerator(float seconds)
        {
            return WaitforSeconds.Create(seconds);
        }

        static public implicit operator IBehaviourEnumerator(double seconds)
        {
            return WaitforSeconds.Create((float)seconds);
        }

        static public implicit operator IBehaviourEnumerator(bool isContinueProcess)
        {
            return (isContinueProcess ? (IBehaviourEnumerator)ContinueProcess.Create() : (IBehaviourEnumerator)RestartProcess.Create());
        }

        static protected T CreateWithPool<T>(System.Func<T> callbackCreate) where T : IBehaviourEnumerator
        {
            T retValue = shaco.Base.GameHelper.objectpool.Instantiate(typeof(T).FullName, callbackCreate);
            return retValue;
        }
    }
}
namespace shaco.Base
{
    public class ContinueProcess : shaco.Base.IBehaviourEnumerator
    {
        static public ContinueProcess Create()
        {
            var retValue = CreateWithPool(() => new ContinueProcess() );
            return retValue;
        }

        private ContinueProcess()
        {

        }

        public override bool IsRunning()
        {
            return false;
        }

        public override void Reset()
        {

        }

        override public BehaviourRootTree.UpdateEnumeratorResult OnRunningEnd(BehaviourTree tree)
        {
            //停止所有子逻辑
            tree.ForeachChildren((child) =>
            {
                child.StopRunning();
                return true;
            });

            //模拟当前逻辑结束
            tree.OnAllProcessResult(BehaviourProcessState.Continue);
            return BehaviourRootTree.UpdateEnumeratorResult.ProceessEnd;
        }

        override public bool IsIgnoreNextEnumerator()
        {
            return true;
        }
    }
}


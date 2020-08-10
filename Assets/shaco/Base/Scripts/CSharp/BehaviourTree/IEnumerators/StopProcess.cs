namespace shaco.Base
{
    public class StopProcess : shaco.Base.IBehaviourEnumerator
    {
        static public StopProcess Create()
        {
            var retValue = CreateWithPool(() => new StopProcess());
            return retValue;
        }

        private StopProcess()
        {

        }

        public override bool IsRunning()
        {
            return false;
        }

        public override void Reset()
        {

        }

        override public bool IsIgnoreNextEnumerator()
        {
            return true;
        }

        override public BehaviourRootTree.UpdateEnumeratorResult OnRunningEnd(BehaviourTree tree)
        {
            //停止逻辑点时候不做任何回调
            return BehaviourRootTree.UpdateEnumeratorResult.StopAllProcess;
        }
    }
}


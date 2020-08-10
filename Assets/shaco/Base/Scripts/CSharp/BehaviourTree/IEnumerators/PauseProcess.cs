namespace shaco.Base
{
    public class PauseProcess : shaco.Base.IBehaviourEnumerator
    {
        static public PauseProcess Create()
        {
            var retValue = CreateWithPool(() => new PauseProcess() );
            return retValue;
        }

        private PauseProcess()
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
            return BehaviourRootTree.UpdateEnumeratorResult.AllProcessPause;
        }

        override public bool IsIgnoreNextEnumerator()
        {
            return true;
        }
    }
}


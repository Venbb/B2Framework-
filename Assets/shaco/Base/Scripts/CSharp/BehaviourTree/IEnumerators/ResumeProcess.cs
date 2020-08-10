namespace shaco.Base
{
    public class ResumeProcess : shaco.Base.IBehaviourEnumerator
    {
        static public ResumeProcess Create()
        {
            var retValue = CreateWithPool(() => new ResumeProcess() );
            return retValue;
        }

        private ResumeProcess()
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
            return BehaviourRootTree.UpdateEnumeratorResult.AllProcessResume;
        }

        override public bool IsIgnoreNextEnumerator()
        {
            return true;
        }
    }
}


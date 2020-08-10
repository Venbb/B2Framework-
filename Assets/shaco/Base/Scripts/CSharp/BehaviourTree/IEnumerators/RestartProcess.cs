namespace shaco.Base
{
    public class RestartProcess : shaco.Base.IBehaviourEnumerator
    {
        static public RestartProcess Create()
        {
            var retValue = CreateWithPool(() => new RestartProcess());
            return retValue;
        }

        private RestartProcess()
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
            //重启逻辑点时候不做任何回调
            return BehaviourRootTree.UpdateEnumeratorResult.ProceessEnd;
        }

        override public bool IsIgnoreNextEnumerator()
        {
            return true;
        }
    }
}


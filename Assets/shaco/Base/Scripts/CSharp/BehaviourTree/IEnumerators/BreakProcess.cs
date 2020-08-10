namespace shaco.Base
{
    public class BreakProcess : shaco.Base.IBehaviourEnumerator
    {
        static public BreakProcess Create()
        {
            var retValue = CreateWithPool(() => new BreakProcess() );
            return retValue;
        }

        private BreakProcess()
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

            //停止所有兄弟逻辑
            tree.ForeachSibling((sibling) =>
            {
                ((BehaviourTree)sibling).StopRunning();
                return true;
            });

            //直接模拟最后一个节点逻辑结束事件
            ((BehaviourTree)tree.last).OnAllProcessResult(BehaviourProcessState.Normal);
            return BehaviourRootTree.UpdateEnumeratorResult.ProceessEnd;
        }

        override public bool IsIgnoreNextEnumerator()
        {
            return true;
        }
    }
}


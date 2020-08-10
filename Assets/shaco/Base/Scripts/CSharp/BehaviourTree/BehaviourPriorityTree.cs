using System.Collections;

namespace shaco.Base
{
    [BehaviourProcessTree(typeof(BehaviourPriorityTree))]
    public class BehaviourPriorityTree : BehaviourTree
    {
        override public string displayName { get { return _displayName; } set { _displayName = value; } }
        private string _displayName = "Priority";
        private BehaviourTree _currentChildTree = null;

        public override bool Process()
        {
            _currentChildTree = this._child as BehaviourTree;

            this.AddOnProcessResultCallBack((state) =>
            {
                ProcessNextChild();
            });
            return base.Process();
        }

        private void ProcessNextChild()
        {
            if (null != _currentChildTree)
            {
                _currentChildTree.Process();
                _currentChildTree.AddOnAllProcessResultCallBack((state) =>
                {
                    if (state == BehaviourProcessState.Continue)
                    {
                        _currentChildTree = _currentChildTree.next as BehaviourTree;
                        ProcessNextChild();
                    }
                    else 
                    {
                        _currentChildTree.AddOnAllProcessResultCallBack((stateSub)=>
                        {
                            OnAllProcessResult(stateSub);
                        });
                    }
                });
            }
        }
    }
}
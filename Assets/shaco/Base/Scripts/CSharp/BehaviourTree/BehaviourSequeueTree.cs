using System.Collections;

namespace shaco.Base
{
    [BehaviourProcessTree(typeof(BehaviourSequeueTree))]
    public class BehaviourSequeueTree : BehaviourTree
    {
        override public string displayName { get { return _displayName; } set { _displayName = value; } }
        private string _displayName = "Sequeue";

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
                    if (_currentChildTree.IsLastChild())
                    {
                        _currentChildTree = null;
                    }
                    else 
                    {
                        _currentChildTree = _currentChildTree.next as BehaviourTree;
                        if (_currentChildTree.IsLastChild())
                        {
                            _currentChildTree.AddOnAllProcessResultCallBack((stateSub)=>
                            {
                                OnAllProcessResult(stateSub);
                            });
                        }
                    }
                    ProcessNextChild();
                });
            }
        }
    }
}
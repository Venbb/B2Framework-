using System.Collections;

namespace shaco.Base
{
    [BehaviourProcessTree(typeof(BehaviourRepeatTree))]
    public class BehaviourRepeatTree : BehaviourTree
    {
        override public string displayName { get { return _displayName; } set { _displayName = value; } }
        private string _displayName = "Repeat";
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
                //最后一节点表示本次递归循环结束，需要延迟一帧update
                if (_currentChildTree.IsLastChild())
                    _currentChildTree.shouldDelayUpdate = true;
                _currentChildTree.Process();
                _currentChildTree.AddOnAllProcessResultCallBack((state) =>
                {
                    bool isLastChild = _currentChildTree.IsLastChild();
                    if (isLastChild)
                    {
                        _currentChildTree = _currentChildTree.first as BehaviourTree;
                    }
                    else
                    {
                        _currentChildTree = _currentChildTree.next as BehaviourTree;
                    }
                    ProcessNextChild();
                });
            }
        }
    }
}
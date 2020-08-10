using System.Collections;

namespace shaco.Base
{
    [BehaviourProcessTree(typeof(BehaviourDefaultTree))]
    public class BehaviourDefaultTree : BehaviourTree
    {
        override public string displayName { get { return _displayName; } set { _displayName = value; } }
        private string _displayName = "Default";

        public override bool Process()
        {
            this.AddOnProcessResultCallBack((state) =>
            {
                ForeachChildren((shaco.Base.BehaviourTree tree) =>
                {
                    tree.Process();
                    if (tree.IsLastChild())
                    {
                        tree.AddOnAllProcessResultCallBack(OnAllProcessResult);
                    }
                    return true;
                });
            });
            return base.Process();
        }
    }
}
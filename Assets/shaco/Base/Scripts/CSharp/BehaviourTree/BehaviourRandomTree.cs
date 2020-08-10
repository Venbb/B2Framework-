using System.Collections;

namespace shaco.Base
{
    [BehaviourProcessTree(typeof(BehaviourRandomTree))]
    public class BehaviourRandomTree : BehaviourTree
    {
        override public string displayName { get { return _displayName; } set { _displayName = value; } }
        private string _displayName = "Random";

        public override bool Process()
        {
            this.AddOnProcessResultCallBack((state) =>
            {
                int currentIndex = 0;
                int randSelectIndex = shaco.Base.Utility.Random(0, Count);
                ForeachChildren((shaco.Base.BehaviourTree tree) =>
                {
                    if (currentIndex++ == randSelectIndex)
                    {
                        tree.Process();
                        tree.AddOnAllProcessResultCallBack(OnAllProcessResult);
                        return false;
                    }
                    else
                        return true;
                });
            });
            return base.Process();
        }
    }
}
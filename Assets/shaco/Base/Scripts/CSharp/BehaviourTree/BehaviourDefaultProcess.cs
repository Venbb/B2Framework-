using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.BehaviourProcess
{
    public class BehaviourDefaultProcess : shaco.Base.IBehaviourProcess
    {
        static public BehaviourDefaultProcess Default
        {
            get
            {
                if (_default == null)
                    _default = new BehaviourDefaultProcess();
                return _default;
            }
        }
        static private BehaviourDefaultProcess _default = null;
        public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
		{
			yield return null;
        }

        public void Dispose()
        {

        }
    }
}
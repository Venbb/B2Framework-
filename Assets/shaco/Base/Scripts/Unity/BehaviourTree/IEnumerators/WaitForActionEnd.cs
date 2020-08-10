using UnityEngine;

namespace shaco
{
    public class WaitForActionEnd : shaco.Base.IBehaviourEnumerator
    {
        private shaco.ActionBase _action = null;
        static public WaitForActionEnd Create(shaco.ActionBase action)
        {
            if (null == action)
            {
                shaco.Log.Error("WaitForActionEnd Create error: action is null");
                return null;
            }
            
            var retValue = CreateWithPool(() => new WaitForActionEnd());
            retValue._action = action;
            return retValue;
        }

        private WaitForActionEnd()
        {
        }

        public override bool IsRunning()
        {
            return _action.elapsed < _action.duration;
        }

        public override void Reset()
        {
            _action.Reset(false);
            _action = null;
        }
    }
}


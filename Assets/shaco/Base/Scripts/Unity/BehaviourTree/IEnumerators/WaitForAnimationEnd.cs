using UnityEngine;

namespace shaco
{
    public class WaitForAnimationEnd : shaco.Base.IBehaviourEnumerator
    {
        private Animation _animation = null;

        static public WaitForAnimationEnd Create(Animation animation)
        {
            if (null == animation)
            {
                shaco.Log.Error("WaitForAnimationEnd Create error: Animation is null");
                return null;
            }
            var retValue = CreateWithPool(() => new WaitForAnimationEnd());
            retValue._animation = animation;
            return retValue;
        }

        public override bool IsRunning()
        {
            return _animation.isPlaying;
        }

        public override void Reset()
        {
            _animation = null;
        }

        private WaitForAnimationEnd() { }
    }
}


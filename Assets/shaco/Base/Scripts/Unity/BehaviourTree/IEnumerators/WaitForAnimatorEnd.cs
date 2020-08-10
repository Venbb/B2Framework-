using UnityEngine;

namespace shaco
{
    public class WaitForAnimatorEnd : shaco.Base.IBehaviourEnumerator
    {
        private Animator _animator = null;
        private int _animationIndex = 0;

        static public WaitForAnimatorEnd Create(Animator animator, int animationIndex)
        {
            if (null == animator)
            {
                shaco.Log.Error("WaitForAnimatorEnd Create error: animator is null");
                return null;
            }

            var retValue = CreateWithPool(() => new WaitForAnimatorEnd());
            retValue._animator = animator;
            retValue._animationIndex = animationIndex;
            return retValue;
        }

        private WaitForAnimatorEnd()
        {
        }

        public override bool IsRunning()
        {
            var animationTmp = _animator.GetCurrentAnimatorStateInfo(_animationIndex);
            return animationTmp.normalizedTime < 1.0f;
        }

        public override void Reset()
        {
            _animator = null;
            _animationIndex = -1;
        }
    }
}


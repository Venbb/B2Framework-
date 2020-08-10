using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class TransparentTo : TransparentBy
    {
        static public new TransparentTo Create(float endAlpha, float duration)
        {
            TransparentTo ret = new TransparentTo();
            ret._alphaEnd = endAlpha; 
            ret.duration = duration;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            _alpha = _alphaEnd - GetCurrentAlpha(target);
            base.RunAction(target);
        }

        public override ActionBase Clone()
        {
            return TransparentTo.Create(_alphaEnd, duration);
        }

        public override ActionBase Reverse(GameObject target)
        {
            return TransparentTo.Create(GetCurrentAlpha(target), duration);
        }

        private TransparentTo() { }
    }
}

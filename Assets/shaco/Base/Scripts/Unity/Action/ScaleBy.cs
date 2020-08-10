using UnityEngine;
using System.Collections;

namespace shaco
{
    public class ScaleBy : ActionBase
    {
        protected Vector3 _vec3Scale;
        protected Vector3 _vec3ScaleEnd;
        static public ScaleBy Create(Vector3 scale, float duration)
        {
            ScaleBy ret = new ScaleBy();
            ret._vec3Scale = scale;
            ret.duration = duration;

            return ret;
        }
        public override void RunAction(GameObject target)
        {
            _vec3ScaleEnd = target.transform.localScale + _vec3Scale;

            this.onCompleteFunc += (shaco.ActionBase action) =>
            {
                if (_vec3ScaleEnd != target.transform.localScale)
                {
                    target.transform.localScale = _vec3ScaleEnd;
                }
            };
            base.RunAction(target);
        }

        public override float UpdateAction(float prePercent)
        {
            target.transform.localScale += _vec3Scale * prePercent;

            return base.UpdateAction(prePercent); ;
        }

        public override ActionBase Clone()
        {
            return ScaleBy.Create(_vec3Scale, duration);
        }

        public override ActionBase Reverse(GameObject target)
        {
            return ScaleBy.Create(-_vec3Scale, duration);
        }

        protected ScaleBy() { }
    }
}
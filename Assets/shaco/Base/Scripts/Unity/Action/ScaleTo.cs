using UnityEngine;
using System.Collections;

namespace shaco
{
    public class ScaleTo : ScaleBy
    {  
        static public new ScaleTo Create(Vector3 endScale, float duration)
        {
            ScaleTo ret = new ScaleTo();
            ret._vec3ScaleEnd = endScale;
            ret.duration = duration;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            _vec3Scale = _vec3ScaleEnd - target.transform.localScale;
            base.RunAction(target);
        }

        public override ActionBase Clone()
        {
            return ScaleTo.Create(_vec3ScaleEnd, duration);
        }

        public override ActionBase Reverse(GameObject target)
        {
            return ScaleTo.Create(target.transform.localScale, duration);
        }

        private ScaleTo() { }
    }
}
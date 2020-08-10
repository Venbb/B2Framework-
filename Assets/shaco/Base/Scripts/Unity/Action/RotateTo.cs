using UnityEngine;
using System.Collections;

namespace shaco
{
    public class RotateTo : RotateBy
    {
        protected Vector3 _vec3EulerAngleSrc;

        static public new RotateTo Create(Vector3 endAngle, float duration, bool isWorldAngle = true)
        {
            RotateTo ret = new RotateTo();
            ret._vec3EulerAngleEnd = endAngle; 
            ret._isWorldAngle = isWorldAngle;
            ret.duration = duration;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            _vec3EulerAngleSrc = target.transform.eulerAngles;
            _vec3EulerAngle = _vec3EulerAngleEnd - target.transform.eulerAngles;

            base.RunAction(target);
        }

        public override ActionBase Clone()
        {
            return RotateTo.Create(_vec3EulerAngleEnd, duration);
        }

        
        public override ActionBase Reverse(GameObject target)
        {
            return RotateTo.Create(target.transform.eulerAngles, duration, this._isWorldAngle);
        }

        private RotateTo() { }
    }
}
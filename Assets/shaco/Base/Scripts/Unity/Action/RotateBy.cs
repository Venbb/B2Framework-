using UnityEngine;
using System.Collections;

namespace shaco
{
    public class RotateBy : ActionBase
    {
        protected Vector3 _vec3EulerAngle = Vector3.zero;
        protected Vector3 _vec3EulerAngleEnd = Vector3.zero;
        protected Vector3 _vec3Current = Vector3.zero;

        protected bool _isWorldAngle = false;

        static public RotateBy Create(Vector3 angle, float duration, bool isWorldAngle = true)
        {
            if (RotateBy.isZero(angle))
            {
                angle.x = 1; angle.y = 1; angle.z = 1;
            }
            RotateBy ret = new RotateBy();
            ret._vec3EulerAngle = angle;
            ret.duration = duration;
            ret._isWorldAngle = isWorldAngle;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            var currentAngle = GetAngle(target);
            _vec3Current = currentAngle;

            _vec3EulerAngleEnd = currentAngle + _vec3EulerAngle;

            this.onCompleteFunc += (shaco.ActionBase action) =>
            {
                if (_vec3EulerAngleEnd != GetAngle(target))
                {
                    if (_isWorldAngle)
                        target.transform.eulerAngles = _vec3EulerAngleEnd;
                    else
                        target.transform.localEulerAngles = _vec3EulerAngleEnd;
                }
            };
            base.RunAction(target);
        }

        public override float UpdateAction(float prePercent)
        {
            _vec3Current += _vec3EulerAngle * prePercent;
            _vec3Current.x %= 360;
            _vec3Current.y %= 360;
            _vec3Current.z %= 360;

            target.transform.rotation = Quaternion.Euler(_vec3Current);

            return base.UpdateAction(prePercent);
        }

        public override ActionBase Clone()
        {
            return RotateBy.Create(_vec3EulerAngle, duration);
        }

        public override ActionBase Reverse(GameObject target)
        {
            return RotateBy.Create(-_vec3EulerAngle, duration);
        }

        static bool isZero(Vector3 value)
        {
            return value.x == 0 && value.y == 0 && value.z == 0;
        }

        private Vector3 GetAngle(GameObject target)
        {
            return _isWorldAngle ? target.transform.eulerAngles : target.transform.localEulerAngles;
        }

        protected RotateBy() { }
    }
}
using UnityEngine;
using System.Collections;

namespace shaco
{
    public class MoveBy : ActionBase
    {
        protected Vector3 _vec3Position;
        protected Vector3 _vec3PositionEnd;
        protected bool _isRelativeMove = true;
        protected bool _isWorldPosition = true;

        static public MoveBy Create(Vector3 offsetPosition, float duration, bool isWorldPosition = true)
        {
            MoveBy ret = new MoveBy();
            ret._vec3Position = offsetPosition;
            ret.duration = duration;
            ret._isWorldPosition = isWorldPosition;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            if (_isRelativeMove)
            {
                var vec3DirectionOffset = target.transform.TransformDirection(_vec3Position);
                _vec3PositionEnd = GetRealPosition(target) + vec3DirectionOffset;
            }
            else
            {
                _vec3PositionEnd = GetRealPosition(target) + _vec3Position;
            }

            this.onCompleteFunc += (shaco.ActionBase action) =>
            {
                var realPos = GetRealPosition(target);
                if (_vec3PositionEnd != GetRealPosition(target))
                {
                    SetRealPosition(target, _vec3PositionEnd);
                }
            };
            base.RunAction(target);
        }

        public override float UpdateAction(float prePercent)
        {
            var moveOffset = prePercent * _vec3Position;
            if (_isRelativeMove && _isWorldPosition)
            {
                target.transform.Translate(moveOffset);
            }
            else
            {
                SetRealPosition(this.target, GetRealPosition(this.target) + moveOffset);
            }

            return base.UpdateAction(prePercent);
        }

        public override ActionBase Clone()
        {
            return MoveBy.Create(_vec3Position, duration);
        }

        public override ActionBase Reverse(GameObject target)
        {
            return MoveBy.Create(-_vec3Position, duration);
        }

        private Vector3 GetRealPosition(GameObject target)
        {
            return _isWorldPosition ? target.transform.position : target.transform.localPosition;
        }

        private void SetRealPosition(GameObject target, Vector3 realPosition)
        {
            if (_isWorldPosition)
            {
                target.transform.position = realPosition;
            }
            else
            {
                target.transform.localPosition = realPosition;
            }
        }

        protected MoveBy() { }
    }
}
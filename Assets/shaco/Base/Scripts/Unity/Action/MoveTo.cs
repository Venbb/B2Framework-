using UnityEngine;
using System.Collections;

namespace shaco
{
    public class MoveTo : MoveBy
    {
        static public new MoveTo Create(Vector3 endPosition, float duration, bool isWorldPosition = true)
        {
            MoveTo ret = new MoveTo();
            ret._vec3PositionEnd = endPosition;
            ret.duration = duration;
            ret._isRelativeMove = false;
            ret._isWorldPosition = isWorldPosition;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            _vec3Position = _vec3PositionEnd - (_isWorldPosition ? target.transform.position : target.transform.localPosition);
            base.RunAction(target);
        }

        public override ActionBase Clone()
        {
            return MoveTo.Create(_vec3PositionEnd, duration);
        }

        public override ActionBase Reverse(GameObject target)
        {
            return MoveTo.Create((_isWorldPosition ? target.transform.position : target.transform.localPosition), duration, this._isWorldPosition);
        }

        private MoveTo() { }
    }
}
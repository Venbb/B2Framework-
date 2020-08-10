using UnityEngine;
using System.Collections;

namespace shaco
{
    public class ShakeRepeat : ActionBase
    {
        private Vector3 _shakeRepeatDistance = Vector3.zero;
        private int _loop = 1;
        private shaco.ActionBase _actionTarget = null;

        static public ShakeRepeat Create(Vector3 shakeRepeatDistance, int loop, float duration)
        {
            ShakeRepeat ret = new ShakeRepeat();
            ret._shakeRepeatDistance = shakeRepeatDistance;
            ret._loop = loop;
            ret.duration = duration;

            return ret;
        }

        static public ShakeRepeat CreateShakeForever(Vector3 shakeRepeatDistance, float duration)
        {
            return Create(shakeRepeatDistance, -1, duration);
        }

        public override void RunAction(GameObject target)
        {
            var move1 = shaco.MoveTo.Create(target.transform.position + _shakeRepeatDistance, this.duration / this._loop / 2);
            var move2 = shaco.MoveTo.Create(target.transform.position, this.duration / this._loop / 2);
            var sequeue = shaco.Sequeue.Create(move1, move2);

            if (_loop == 1)
                _actionTarget = sequeue;
            else if (_loop > 1)
                _actionTarget = shaco.Repeat.Create(sequeue, _loop);
            else
            {
                _actionTarget = shaco.Repeat.CreateRepeatForever(sequeue);
            }

            _actionTarget.RunActionWithoutPlay(target);
            base.RunAction(target);
        }

        public override float UpdateAction(float prePercent)
        {
            SetActionAlive(true);
            prePercent = base.UpdateAction(prePercent);
            var newPercent = _actionTarget.UpdateAction(prePercent);

            if (!_actionTarget.IsActionAlive())
            {
                SetActionAlive(false);
            }

            return newPercent;
        }

        public override ActionBase Clone()
        {
            return ShakeRepeat.Create(_shakeRepeatDistance, _loop, duration);
        }

        public override ActionBase Reverse(GameObject target)
        {
            return ShakeRepeat.Create(-_shakeRepeatDistance, _loop, duration);
        }

        public override void PlayEndDirectly()
        {
            base.PlayEndDirectly();
            if (_actionTarget != null)
                _actionTarget.PlayEndDirectly();
        }

        private ShakeRepeat() { }
    }
}
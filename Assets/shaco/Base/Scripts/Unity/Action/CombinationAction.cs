using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class CombinationAction : ActionBase
    {
        protected List<ActionBase> _listActions = new List<ActionBase>();

        static public CombinationAction Create(float duration, params ActionBase[] actions)
        {
            var ret = new CombinationAction();
            ret.duration = duration;

            for (int i = 0; i < actions.Length; ++i)
            {
                ret._listActions.Add(actions[i]);
            }

            return ret;
        }

        public override void RunAction(GameObject target)
        {
            if (_listActions.Count == 0)
            {
                Log.Error("CombinationAction.RunAction erorr: not have any Action !", target);
                return;
            }

            for (int i = 0; i < _listActions.Count; ++i)
            {
                _listActions[i].RunAction(target);
            }
            base.RunAction(target);
        }

        public override float UpdateAction(float prePercent)
        {
            var ret = base.UpdateAction(prePercent);

            for (int i = 0; i < _listActions.Count; ++i)
            {
                _listActions[i].UpdateAction(prePercent);
            }

            return ret;
        }

        public override ActionBase Clone()
        {
            var ret = new CombinationAction();

            for (int i = 0; i < _listActions.Count; ++i)
            {
                ret._listActions.Add(_listActions[i]);
            }

            return ret;
        }

        public override ActionBase Reverse(GameObject target)
        {
            var ret = new CombinationAction();

            for (int i = _listActions.Count - 1; i >= 0; --i)
            {
                ret._listActions.Add(_listActions[i]);
            }

            return ret;
        }

        public override void PlayEndDirectly()
        {
            base.PlayEndDirectly();

            for (int i = 0; i < _listActions.Count; ++i)
            {
                _listActions[i].PlayEndDirectly();
            }
        }

        public override void Reset(bool isAutoPlay)
        {
            base.Reset(isAutoPlay);

            for (int i = 0; i < _listActions.Count; ++i)
            {
                _listActions[i].Reset(isAutoPlay);
            }
        }

        private CombinationAction() { }
    }
}
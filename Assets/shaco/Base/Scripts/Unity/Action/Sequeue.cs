using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class Sequeue : ActionBase
    {
        private class ActionInfo
        {
            public ActionBase action;
            public float percentWeight;
        }

        private List<ActionInfo> _listActions = new List<ActionInfo>();
        private int _actionIndex = 0;
        private ActionInfo _currentActionInfo = null;

        public int Count { get { return _listActions.Count; }}

        static public Sequeue Create(params ActionBase[] actions)
        {
            return Create((IEnumerable<ActionBase>)actions);
        }

        static public Sequeue Create(IEnumerable<ActionBase> actions)
        {
            Sequeue ret = new Sequeue();
            int index = 0;
            float totalDuration = 0;

            foreach (var iter in actions)
            {
                if (null == iter)
                {
                    Log.Error("Sequeue Create error: action is null, index=" + index);
                    continue;
                }

                totalDuration += iter.duration;

                var newInfo = new ActionInfo();
                newInfo.action = iter;

                ret._listActions.Add(newInfo);
                ++index;
            }
            ret.duration = totalDuration;

            //设置每个动画的时间权重比
            for (int i = ret._listActions.Count - 1; i >= 0; --i)
            {
                var actionInfo = ret._listActions[i];
                actionInfo.percentWeight = actionInfo.action.duration / totalDuration;
            }
            return ret;
        }

        public void Clear()
        {
            _listActions.Clear();
        }

        public override void RunAction(GameObject target)
        {
            this._actionIndex = 0;
            _currentActionInfo = _listActions[_actionIndex];
            _currentActionInfo.action.RunActionWithoutPlay(target);
            base.RunAction(target);
        }

        public override float UpdateAction(float prePercent)
        {
            var retValue = base.UpdateAction(prePercent);
            if (null == _currentActionInfo)
            {
                // Log.Error("Sequeue UpdateAction error: no playing action");
                return retValue;
            }

            var subPerPercent = prePercent / _currentActionInfo.percentWeight;
            _currentActionInfo.action.UpdateAction(subPerPercent);
            CheckJumpToNextAction();
            return retValue;
        }

        public override ActionBase Clone()
        {
            ActionBase[] actions = new ActionBase[_listActions.Count];
            for (int i = 0; i < _listActions.Count; ++i)
            {
                actions[i] = _listActions[i].action;
            }
            return Sequeue.Create(actions);
        }

        public override ActionBase Reverse(GameObject target)
        {
            ActionBase[] actions = new ActionBase[_listActions.Count];
            for (int i = _listActions.Count - 1; i >= 0; --i)
            {
                actions[i] = _listActions[i].action;
            }
            return Sequeue.Create(actions);
        }

        public override void Reset(bool isAutoPlay)
        {
            base.Reset(isAutoPlay);
            _actionIndex = 0;

            foreach (var iter in _listActions)
            {
                iter.action.Reset(isAutoPlay);
            }
        }

		public override void PlayEndDirectly ()
		{
			base.PlayEndDirectly ();

            for (int i = _actionIndex; i < _listActions.Count; ++i)
			{
				var actionTarget = _listActions[i].action;
				if (actionTarget != null)
				{
                    if (actionTarget.isPaused)
                        actionTarget.RunActionWithoutPlay(this.target);

                    actionTarget.PlayEndDirectly();
				}
			}
            _currentActionInfo = null;
            _actionIndex = _listActions.Count - 1;
		}

        private void CheckJumpToNextAction()
        {
            if (null == _currentActionInfo || null == _currentActionInfo.action)
            {
                Log.Error("Sequeue CheckJumpToNextAction error: action is null", target);
                return;
            }

            if (!_currentActionInfo.action.IsActionAlive())
            {
                ++_actionIndex;
                if (_actionIndex < 0 || _actionIndex >= _listActions.Count)
                {
                    _currentActionInfo = null;
                }
                else
                {
                    _currentActionInfo = _listActions[_actionIndex];
                    _currentActionInfo.action.RunActionWithoutPlay(target);
                }
            }
        }

        private Sequeue() { }
    }
}
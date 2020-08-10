using UnityEngine;
using System.Collections;

namespace shaco
{
    public class Repeat : ActionBase
    {
        public delegate void SS_CallFUNC_LOOP_COMPLETE(int loop);

        public SS_CallFUNC_LOOP_COMPLETE onLoopCompleteFunc = null;

        protected ActionBase ActionTarget;
        protected int _iLoop = 1;
        protected int _iCurrentTimes = 1;

        private bool _isPlayEndDirectlyIntercept = false;
        
        static public Repeat Create(ActionBase action, int loop)
        {
            Repeat ret = new Repeat();
            ret.ActionTarget = action;
            ret._iLoop = loop;
            ret.AddActionTargetCompletedCallBack(action);
            return ret;
        }

        static public Repeat CreateRepeatForever(ActionBase action)
        {
            Repeat ret = new Repeat();
            ret.ActionTarget = action;
            ret._iLoop = -1;
            ret.AddActionTargetCompletedCallBack(action);
            return ret;
        }

        public override void RunAction(GameObject target)
        {
            if (ActionTarget != null)
                ActionTarget.RunActionWithoutPlay(target);
            base.RunAction(target);
        }

        public override float UpdateAction(float prePercent)
        {
            if (elapsed >= duration)
            {
                SetActionAlive(false);
                return base.UpdateAction(prePercent);
            }

            SetActionAlive(true);
            prePercent = ActionTarget.GetCurrentPercent();

            //当无限循环的时候，需要在UpdateAction前重新设置prePercent，否则在一帧等待时间太长的时候，会导致自动AddRemove销毁了无限循环动作
            if (_iLoop < 0)
            {
                prePercent = 0;
            }
            prePercent = base.UpdateAction(prePercent);

            if (ActionTarget != null)
            {
                prePercent = ActionTarget.GetCurrentPercent();
                ActionTarget.UpdateAction(prePercent);
            }
            return prePercent;
        }

        public override ActionBase Clone()
        {
            return Repeat.Create(ActionTarget, _iLoop);
        }

        public override ActionBase Reverse(GameObject target)
        {
            return Repeat.Create(ActionTarget, _iLoop);
        }

        public override void PlayEndDirectly()
        {
            base.PlayEndDirectly();

            if (ActionTarget != null)
            {
                ActionTarget.PlayEndDirectly();
            }
        }

        public override void Reset(bool isAutoPlay)
        {
            base.Reset(isAutoPlay);

            _iCurrentTimes = 1;

            if (ActionTarget != null)
            {
                ActionTarget.Reset(isAutoPlay);
            }
        }

        public int GetCurrentLoop()
        {
            return _iCurrentTimes;
        }

        public void SetActionTarget(ActionBase action)
        {
            ActionTarget = action;
        }

        public ActionBase GetActionTarget()
        {
            return ActionTarget;
        }

        private void AddActionTargetCompletedCallBack(ActionBase target)
        {
            target.onCompleteFunc -= OnTargetCompletedCallBack;
            target.onCompleteFunc += OnTargetCompletedCallBack;
        }

        private void OnTargetCompletedCallBack(ActionBase action)
        {
            if (_isPlayEndDirectlyIntercept)
                return;
                
            if (_iCurrentTimes < _iLoop || _iLoop < 0)
            {
                if (onLoopCompleteFunc != null)
                    onLoopCompleteFunc(_iCurrentTimes);

                _isPlayEndDirectlyIntercept = true;
                ActionTarget.PlayEndDirectly();
                _isPlayEndDirectlyIntercept = false;

                ActionTarget.Reset(true);
                ActionTarget.RunActionWithoutPlay(ActionTarget.target);
                this.SetActionAlive(true);
            }
            else
                SetActionAlive(false);
                
            if (_iCurrentTimes > int.MaxValue - 1)
                _iCurrentTimes = 0;
            else
                ++_iCurrentTimes;
        }

        private Repeat() { }
    }
}
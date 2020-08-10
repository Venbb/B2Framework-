using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//--------------------------------
//ActionBase Using parameters is All global parameters, For example, do not use localEulerAngles
//--------------------------------
namespace shaco
{
    public abstract class ActionBase
    {
        //-------------------------------
        //Action params
        //-------------------------------
        public GameObject target { get { return _target; } protected set { _target = value; } }
        private GameObject _target = null;

        public float elapsed { get { return _eplapsed; } protected set { _eplapsed = value; } }
        private float _eplapsed = 0;

        public float duration { get { return _duration; } protected set { _duration = value; } }
        private float _duration = 1;

        public string actionName { get { return _actionName; } protected set { _actionName = value; } }
        private string _actionName = "ActionBase";
        
        public int tag { get { return _tag; } protected set { _tag = value; } }
        private int _tag = 0;

        public bool isAdded { get { return _isAdded; } protected set { _isAdded = value; } }
        private bool _isAdded = false;

        public bool isRemoved { get { return _isRemoved; } protected set { _isRemoved = value; } }
        private bool _isRemoved = false;
        
        public bool isPaused { get { return _isPaused; } protected set { _isPaused = value; } }
        private bool _isPaused = false;

        public ActionBase prevAction { get { return _prevAction; } protected set { _prevAction = value; } }
        private ActionBase _prevAction = null;

        public ActionBase nextAction { get { return _nextAction; } protected set { _nextAction = value; } }
        private ActionBase _nextAction = null;

        //-------------------------------
        //Callback Functions
        //-------------------------------
        public System.Action<ActionBase> onCompleteFunc = null;
        public System.Action<float> onFrameFunc = null;

        //-------------------------------
        //Instance params
        //-------------------------------
        private bool _isAutoPlay = true;

        public ActionBase()
        {
            actionName = this.ToTypeString();
        }

        public void RunActionWithoutPlay(GameObject target)
        {
            _isAutoPlay = false;
            RunAction(target);
            _isAutoPlay = true;
        }

        public void StopMe(bool isPlayEndWithDirectly = false)
        {
            if (isPlayEndWithDirectly)
            {
                this.PlayEndDirectly();
            }
            shaco.GameHelper.action.AddRemove(target, this);
        }

        public void Pause()
        {
            this._isPaused = true;
        }

        public void Resume()
        {
            this._isPaused = false;
        }

        virtual public float UpdateAction(float prePercent)
        {
            return UpdateActionBase(prePercent);
        }

        abstract public ActionBase Clone();

        abstract public ActionBase Reverse(GameObject target);

        virtual public void Reset(bool isAutoPlay)
        {
            if (isAutoPlay)
                elapsed = this.GetCurrentPercent() * duration;
            else
                elapsed = 0;
        }

        virtual public void PlayEndDirectly()
        {
            NotifyActionCompleted();
            elapsed = duration;
        }

        virtual public void RunAction(GameObject target)
        {
            if (null == target)
            {
                Log.Error("ActionBase RunAction erorr: target is null");
                return;
            }

            shaco.GameHelper.action.CheckInit();
            if (shaco.GameHelper.action.HasAction(target, this))
            {
                Log.Warning("ActionBase RunAction warning: Have been to add the action", target);
            }

            this.target = target;
            elapsed = 0;
            isRemoved = false;

            if (actionName == "ActionBase")
                actionName = GetType().FullName;

            if (_isAutoPlay)
            {
                shaco.GameHelper.action.AddAction(target, this);
                Resume();
            }

            //run action by directly
            if (duration <= 0.0f)
            {
                _isAutoPlay = false;
                UpdateAction(1.0f);
                _isAutoPlay = true;
            }
        }

        public float GetCurrentPercent()
        {
            if (this.duration <= 0)
                return 1.0f;
            else
                return shaco.GameHelper.action.currentDeltaTime / this.duration;
        }

        public float GetElapsedPercent()
        {
            if (this.duration <= 0)
                return 1.0f;
            else
                return this.elapsed / this.duration + GetCurrentPercent();
        }

        public float GetRemainPercent()
        {
            if (this.duration <= 0)
                return 0;
            else
                return (this.duration - this.elapsed) / this.duration;
        }

        public void SetTag(int tag)
        {
            _tag = tag;
        }

        public void MarkAsAdded()
        {
            this._isAdded = true;
        }

        public void MarkAsRemoved()
        {
            this._isRemoved = true;
            this._isPaused = true;
            this._isAdded = false;
        }

        public void SetNextAction(ActionBase next)
        {
            if (null == next)
            {
                this._prevAction = null;
            }
            else
            {
                next._prevAction = this;
            }
            this._nextAction = next;
        }

        public void SetDuration(float duration)
        {
            this._duration = duration;
        }

        public bool IsActionAlive()
        {
            return this.elapsed < this.duration;
        }

        public void NotifyActionCompleted()
        {
            try
            {
                if (null != onCompleteFunc)
                {
                    GameHelper.profiler.BeginSample("ActionBase OnActionComplete: target=" + onCompleteFunc.Target + " method=" + onCompleteFunc.Method);
                    onCompleteFunc(this);
                    GameHelper.profiler.EndSample();
                }

                //如果还有下个动画则自动开始它
                if (null != _nextAction)
                {
                    _nextAction.RunAction(this._target);
                }
            }
            catch (System.Exception e)
            {
                Log.Error("ActionBase OnActionComplete error: onCompleteFunc=" + onCompleteFunc + " e=" + e, target);
            }
        }

        

        protected void SetActionAlive(bool isAlive)
        {
            if (isAlive)
            {
                elapsed = 0;
                duration = 9999999;
            }
            else
            {
                elapsed = 1;
                duration = 1;
            }
        }

        protected float UpdateActionBase(float prePercent)
        {
            if (!_isAutoPlay)
                return 1.0f;

            if (isRemoved)
                return 1.0f;

            float remainPercent = this.GetRemainPercent();

            if (prePercent > remainPercent)
            {
                prePercent = remainPercent < 0 ? 0 : remainPercent;
            }

            elapsed += duration * prePercent;

            if (!this.IsActionAlive())
            {
                if (onFrameFunc != null)
                {
                    try
                    {
                        GameHelper.profiler.BeginSample("ActionBase UpdateActionBase onFrameFunc: target=" + onFrameFunc.Target + " method=" + onFrameFunc.Method);
                        onFrameFunc(1.0f);
                        GameHelper.profiler.EndSample();
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("ActionBase UpdateActionBase error: onFrameFunc=" + onFrameFunc + " e=" + e, target);
                    }
                }

                NotifyActionCompleted();

                if (!this.IsActionAlive())
                    shaco.GameHelper.action.AddRemove(this.target, this);
            }
            else if (onFrameFunc != null)
            {
                if (onFrameFunc != null)
                {
                    try
                    {
                        onFrameFunc(elapsed / duration);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("ActionBase UpdateActionBase error: onFrameFunc2=" + onFrameFunc + " e=" + e, target);
                    }
                }
            }
            return prePercent;
        }
    }
}
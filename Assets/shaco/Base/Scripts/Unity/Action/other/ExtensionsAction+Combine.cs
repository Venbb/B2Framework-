using UnityEngine;
using System.Collections;

static public partial class shaco_ExtensionsAction
{
    static public shaco.ActionBase OnCompleted(this shaco.ActionBase action, System.Action<shaco.ActionBase> callblacKEnd)
    {
        action.onCompleteFunc += callblacKEnd;
        return action;
    }

    static public shaco.MoveBy MoveBy(this shaco.ActionBase action, Vector3 endPositon, float duration, bool isWorldPosition = true)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine MoveBy erorr: action is null");
            return null;
        }
        
        var ret = shaco.MoveBy.Create(endPositon, duration, isWorldPosition);
        action.SetNextAction(ret);
        return ret;
    }

    static public shaco.MoveTo MoveTo(this shaco.ActionBase action, Vector3 endPositon, float duration, bool isWorldPosition = true)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine MoveTo erorr: action is null");
            return null;
        }

        var ret = shaco.MoveTo.Create(endPositon, duration, isWorldPosition);
        action.SetNextAction(ret);
        return ret;
    }

    static public shaco.CurveMoveBy CurveMoveBy(this shaco.ActionBase action, Vector3 beginPoint, Vector3 endPoint, float duration, params Vector3[] controlPoints)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine CurveMoveBy erorr: action is null");
            return null;
        }

        var ret = shaco.CurveMoveBy.Create(beginPoint, endPoint, duration, controlPoints);
        action.SetNextAction(ret);
        return ret;
    }

    static public shaco.CurveMoveTo CurveMoveTo(this shaco.ActionBase action, Vector3 beginPoint, Vector3 endPoint, float duration, params Vector3[] controlPoints)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine CurveMoveTo erorr: action is null");
            return null;
        }

        var ret = shaco.CurveMoveTo.Create(beginPoint, endPoint, duration, controlPoints);
        action.SetNextAction(ret);
        return ret;
    }

    static public shaco.RotateBy RotateBy(this shaco.ActionBase action, Vector3 angle, float duration, bool isWorldAngle = true)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine RotateBy erorr: action is null");
            return null;
        }

        var ret = shaco.RotateBy.Create(angle, duration, isWorldAngle);
        action.SetNextAction(ret);
        return ret;
    }

    static public shaco.RotateTo RotateTo(this shaco.ActionBase action, Vector3 angle, float duration, bool isWorldAngle = true)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine RotateTo erorr: action is null");
            return null;
        }

        var ret = shaco.RotateTo.Create(angle, duration, isWorldAngle);
        action.SetNextAction(ret);
        return ret;
    }

    static public shaco.ScaleBy ScaleBy(this shaco.ActionBase action, Vector3 scale, float duration)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine ScaleBy erorr: action is null");
            return null;
        }

        var ret = shaco.ScaleBy.Create(scale, duration);
        action.SetNextAction(ret);
        return ret;
    }

    static public shaco.ScaleTo ScaleTo(this shaco.ActionBase action, Vector3 scale, float duration)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine ScaleTo erorr: action is null");
            return null;
        }

        var ret = shaco.ScaleTo.Create(scale, duration);
        action.SetNextAction(ret);
        return ret;
    }

    static public shaco.ShakeRepeat ShakeRepeat(this shaco.ActionBase action, Vector3 shakeValue, int loop, float duration)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine ShakeRepeat erorr: action is null");
            return null;
        }

        var ret = shaco.ShakeRepeat.Create(shakeValue, loop, duration);
        action.SetNextAction(ret);
        return ret;
    }

    static public shaco.TransparentBy TransparentBy(this shaco.ActionBase action, float alpha, float duration)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine TransparentBy erorr: action is null");
            return null;
        }

        var ret = shaco.TransparentBy.Create(alpha, duration);
        action.SetNextAction(ret);
        return ret;
    }

    static public shaco.TransparentTo TransparentTo(this shaco.ActionBase action, float alpha, float duration)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine TransparentTo erorr: action is null");
            return null;
        }

        var ret = shaco.TransparentTo.Create(alpha, duration);
        action.SetNextAction(ret);
        return ret;
    }

    static public shaco.Repeat Repeat(this shaco.ActionBase action, int loop)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine Repeat erorr: action is null");
            return null;
        }

        var allActions = CollectionPreviousActions(action);
        if (allActions.IsNullOrEmpty())
        {
            shaco.Log.Error("ExtensionsAction+Combine Repeat erorr: not found action");
            return null;
        }

        var firstAction = allActions[allActions.Count - 1];
        var firstActionTarget = firstAction.target;
        firstAction.StopMe();
        
        shaco.Repeat ret = null;
        if (allActions.Count == 1)
        {
            ret = shaco.Repeat.Create(action, loop);
        }
        else
        {
            var sequeueAction = shaco.Sequeue.Create(allActions);
            ret = shaco.Repeat.Create(sequeueAction, loop);
        }

        ret.RunAction(firstActionTarget);
        return ret;
    }

    static public shaco.Repeat RepeatForever(this shaco.ActionBase action)
    {
        return action.Repeat(-1);
    }

    static public shaco.DelayTime Delay(this shaco.ActionBase action, float duration)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine Delay erorr: action is null");
            return null;
        }

        var ret = shaco.DelayTime.Create(duration);
        action.SetNextAction(ret);
        return ret;
    }

    static public shaco.Sequeue Pingpong(this shaco.ActionBase action)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine Pingpong erorr: action is null");
            return null;
        }

        var allActions = CollectionPreviousActions(action);
        if (allActions.IsNullOrEmpty())
        {
            shaco.Log.Error("ExtensionsAction+Combine Pingpong erorr: not found action");
            return null;
        }

        var firstAction = allActions[allActions.Count - 1];
        var firstActionTarget = firstAction.target;
        firstAction.StopMe();

        var reverseActions = new System.Collections.Generic.List<shaco.ActionBase>();
        for (int i = allActions.Count - 1; i >= 0; --i)
        {
            reverseActions.Add(allActions[i].Reverse(firstActionTarget));
        }
        allActions.AddRange(reverseActions);

        var ret = shaco.Sequeue.Create(allActions);
        ret.RunAction(firstActionTarget);
        return ret;
    }

    static public shaco.Accelerate Accelerate(this shaco.ActionBase action, 
                                                shaco.Accelerate.ControlPoint begin, 
                                                shaco.Accelerate.ControlPoint middle, 
                                                shaco.Accelerate.ControlPoint end, 
                                                shaco.Accelerate.AccelerateMode mode = shaco.Accelerate.AccelerateMode.Straight)
    {
        if (null == action)
        {
            shaco.Log.Error("ExtensionsAction+Combine Accelerate erorr: action is null");
            return null;
        }

        var allActions = CollectionPreviousActions(action);
        if (allActions.IsNullOrEmpty())
        {
            shaco.Log.Error("ExtensionsAction+Combine Accelerate erorr: not found action");
            return null;
        }

        var firstAction = allActions[allActions.Count - 1];
        var firstActionTarget = firstAction.target;
        firstAction.StopMe();

        shaco.Accelerate ret = null;
        if (allActions.Count == 1)
        {
            ret = shaco.Accelerate.Create(action, begin, middle, end, mode);
        }
        else
        {
            var sequeueAction = shaco.Sequeue.Create(allActions);
            ret = shaco.Accelerate.Create(sequeueAction, begin, middle, end, mode);
        }
        ret.RunAction(firstActionTarget);
        return ret;
    }

    static private System.Collections.Generic.List<shaco.ActionBase> CollectionPreviousActions(shaco.ActionBase action)
    {
        var ret = new System.Collections.Generic.List<shaco.ActionBase>();
        var prevAction = action;
        while (null != prevAction)
        {
            ret.Add(prevAction);
            var currentAction = prevAction;
            prevAction = prevAction.prevAction;
            currentAction.SetNextAction(null);
        }
        return ret;
    }
}

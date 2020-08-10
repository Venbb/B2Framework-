using UnityEngine;
using System.Collections;

static public partial class shaco_ExtensionsAction
{
    static public shaco.MoveBy MoveBy(this Component target, Vector3 endPosition, float duration, bool isWorldPosition = true)
    {
        return MoveBy(target.gameObject, endPosition, duration, isWorldPosition);
    }
    static public shaco.MoveBy MoveBy(this GameObject target, Vector3 endPosition, float duration, bool isWorldPosition = true)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction MoveBy erorr: target is null");
            return null;
        }
        var ret = shaco.MoveBy.Create(endPosition, duration, isWorldPosition);
        ret.RunAction(target);
        return ret;
    }

    static public shaco.MoveTo MoveTo(this Component target, Vector3 endPosition, float duration, bool isWorldPosition = true)
    {
        return MoveTo(target.gameObject, endPosition, duration, isWorldPosition);
    }
    static public shaco.MoveTo MoveTo(this GameObject target, Vector3 endPosition, float duration, bool isWorldPosition = true)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction MoveTo erorr: target is null");
            return null;
        }
        var ret = shaco.MoveTo.Create(endPosition, duration, isWorldPosition);
        ret.RunAction(target);
        return ret;
    }

    static public shaco.CurveMoveBy CurveMoveBy(this Component target, Vector3 beginPoint, Vector3 endPoint, float duration, params Vector3[] controlPoints)
    {
        return CurveMoveBy(target.gameObject, beginPoint, endPoint, duration, controlPoints);
    }
    static public shaco.CurveMoveBy CurveMoveBy(this GameObject target, Vector3 beginPoint, Vector3 endPoint, float duration, params Vector3[] controlPoints)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction CurveMoveBy erorr: target is null");
            return null;
        }
        var ret = shaco.CurveMoveBy.Create(beginPoint, endPoint, duration, controlPoints);
        ret.RunAction(target);
        return ret;
    }

    static public shaco.CurveMoveTo CurveMoveTo(this Component target, Vector3 beginPoint, Vector3 endPoint, float duration, params Vector3[] controlPoints)
    {
        return CurveMoveTo(target.gameObject, beginPoint, endPoint, duration, controlPoints);
    }
    static public shaco.CurveMoveTo CurveMoveTo(this GameObject target, Vector3 beginPoint, Vector3 endPoint, float duration, params Vector3[] controlPoints)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction CurveMoveTo erorr: target is null");
            return null;
        }
        var ret = shaco.CurveMoveTo.Create(beginPoint, endPoint, duration, controlPoints);
        ret.RunAction(target);
        return ret;
    }

    static public shaco.RotateBy RotateBy(this Component target, Vector3 angle, float duration)
    {
        return RotateBy(target.gameObject, angle, duration);
    }
    static public shaco.RotateBy RotateBy(this GameObject target, Vector3 angle, float duration)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction RotateBy erorr: target is null");
            return null;
        }
        var ret = shaco.RotateBy.Create(angle, duration);
        ret.RunAction(target);
        return ret;
    }

    static public shaco.RotateTo RotateTo(this Component target, Vector3 angle, float duration)
    {
        return RotateTo(target.gameObject, angle, duration);
    }
    static public shaco.RotateTo RotateTo(this GameObject target, Vector3 angle, float duration)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction RotateTo erorr: target is null");
            return null;
        }
        var ret = shaco.RotateTo.Create(angle, duration);
        ret.RunAction(target);
        return ret;
    }

    static public shaco.ScaleBy ScaleBy(this Component target, Vector3 scale, float duration)
    {
        return ScaleBy(target.gameObject, scale, duration);
    }
    static public shaco.ScaleBy ScaleBy(this GameObject target, Vector3 scale, float duration)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction ScaleBy erorr: target is null");
            return null;
        }
        var ret = shaco.ScaleBy.Create(scale, duration);
        ret.RunAction(target);
        return ret;
    }

    static public shaco.ScaleTo ScaleTo(this Component target, Vector3 scale, float duration)
    {
        return ScaleTo(target.gameObject, scale, duration);
    }
    static public shaco.ScaleTo ScaleTo(this GameObject target, Vector3 scale, float duration)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction ScaleTo erorr: target is null");
            return null;
        }
        var ret = shaco.ScaleTo.Create(scale, duration);
        ret.RunAction(target);
        return ret;
    }

    static public shaco.ShakeRepeat ShakeRepeat(this Component target, Vector3 shakeValue, int loop, float duration)
    {
        return ShakeRepeat(target.gameObject, shakeValue, loop, duration);
    }
    static public shaco.ShakeRepeat ShakeRepeat(this GameObject target, Vector3 shakeValue, int loop, float duration)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction ShakeRepeat erorr: target is null");
            return null;
        }
        var ret = shaco.ShakeRepeat.Create(shakeValue, loop, duration);
        ret.RunAction(target);
        return ret;
    }

    static public shaco.TransparentBy TransparentBy(this Component target, float alpha, float duration)
    {
        return TransparentBy(target.gameObject, alpha, duration);
    }
    static public shaco.TransparentBy TransparentBy(this GameObject target, float alpha, float duration)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction TransparentBy erorr: target is null");
            return null;
        }
        var ret = shaco.TransparentBy.Create(alpha, duration);
        ret.RunAction(target);
        return ret;
    }

    static public shaco.TransparentTo TransparentTo(this Component target, float alpha, float duration)
    {
        return TransparentTo(target.gameObject, alpha, duration);
    }
    static public shaco.TransparentTo TransparentTo(this GameObject target, float alpha, float duration)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction TransparentTo erorr: target is null");
            return null;
        }
        var ret = shaco.TransparentTo.Create(alpha, duration);
        ret.RunAction(target);
        return ret;
    }

    static public void StopAllAction(this Component target, bool isPlayEndWithDirectly = false)
    {
        StopAllAction(target.gameObject, isPlayEndWithDirectly);
    }
    static public void StopAllAction(this GameObject target, bool isPlayEndWithDirectly = false)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction StopActions erorr: target is null");
            return;
        }
        shaco.GameHelper.action.StopAllAction(target, isPlayEndWithDirectly);
    }

    static public void StopActionByTag(this Component target, int tag, bool isPlayEndWithDirectly = false)
    {
        StopActionByTag(target.gameObject, tag, isPlayEndWithDirectly);   
    }
    static public void StopActionByTag(this GameObject target, int tag, bool isPlayEndWithDirectly = false)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction StopActionByTag erorr: target is null");
            return;
        }
        shaco.GameHelper.action.StopActionByTag(target, tag, isPlayEndWithDirectly);
    }

    static public void StopActionByType<T>(this Component target, bool isPlayEndWithDirectly = false) where T : shaco.ActionBase
    {
        StopActionByType<T>(target.gameObject, isPlayEndWithDirectly);
    }
    static public void StopActionByType<T>(this GameObject target, bool isPlayEndWithDirectly = false) where T : shaco.ActionBase
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction StopAction<T> erorr: target is null");
            return;
        }
        shaco.GameHelper.action.StopActionByType<T>(target, isPlayEndWithDirectly);
    }

    static public void StopAction(this Component target, shaco.ActionBase action, bool isPlayEndWithDirectly = false)
    {
        StopAction(target.gameObject, action, isPlayEndWithDirectly);
    }
    static public void StopAction(this GameObject target, shaco.ActionBase action, bool isPlayEndWithDirectly = false)
    {
        if (null == target)
        {
            shaco.Log.Error("ExtensionsAction StopAction erorr: target is null");
            return;
        }
        shaco.GameHelper.action.StopAction(target, action);
    }
}

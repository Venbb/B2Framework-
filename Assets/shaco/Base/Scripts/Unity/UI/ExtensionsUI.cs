using UnityEngine;
using System.Collections;

static public class shaco_ExtensionsUI
{
    static public void HideMe(this UnityEngine.Component target)
    {
        shaco.GameHelper.ui.HideUITarget(target);
    }

    static public void CloseMe(this UnityEngine.Component target)
    {
        shaco.GameHelper.ui.CloseUITarget(target);
    }

    static public void BringToFront<T>(this UnityEngine.Component target)
    {
        shaco.GameHelper.ui.BringToFrontTarget(target);
    }
}
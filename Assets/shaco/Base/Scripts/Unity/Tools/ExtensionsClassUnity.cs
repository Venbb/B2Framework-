using UnityEngine;
using System.Collections;

static public class shaco_ExtensionsClassUnity
{
    static public bool Intersect(this Rect rect, Rect other)
    {
        return !(rect.xMax < other.xMin || rect.xMin > other.xMax
            || rect.yMax < other.yMin || rect.yMin > other.yMax);
    }

    static public bool Contains(this Rect rect, Rect other)
    {
        return rect.xMin <= other.xMin && rect.xMax >= other.xMax 
            && rect.yMin <= other.yMin && rect.yMax >= other.yMax;
    }

    static public TextAnchor ToTextAnchor(this shaco.Anchor anchor)
    {
        var retValue = TextAnchor.UpperLeft;
        switch (anchor)
        {
            case shaco.Anchor.UpperLeft: retValue = TextAnchor.UpperLeft; break;
            case shaco.Anchor.UpperCenter: retValue = TextAnchor.UpperCenter; break;
            case shaco.Anchor.UpperRight: retValue = TextAnchor.UpperRight; break;
            case shaco.Anchor.MiddleLeft: retValue = TextAnchor.MiddleLeft; break;
            case shaco.Anchor.MiddleCenter: retValue = TextAnchor.MiddleCenter; break;
            case shaco.Anchor.MiddleRight: retValue = TextAnchor.MiddleRight; break;
            case shaco.Anchor.LowerLeft: retValue = TextAnchor.LowerLeft; break;
            case shaco.Anchor.LowerCenter: retValue = TextAnchor.LowerCenter; break;
            case shaco.Anchor.LowerRight: retValue = TextAnchor.LowerRight; break;
            default: shaco.Log.Error("ToTextAnchor error: unsupport anchor=" + anchor); break;
        }
        return retValue;
    }

    static public shaco.Anchor ToAnchor(this TextAnchor anchor)
    {
        var retValue = shaco.Anchor.UpperLeft;
        switch (anchor)
        {
            case TextAnchor.UpperLeft: retValue = shaco.Anchor.UpperLeft; break;
            case TextAnchor.UpperCenter: retValue = shaco.Anchor.UpperCenter; break;
            case TextAnchor.UpperRight: retValue = shaco.Anchor.UpperRight; break;
            case TextAnchor.MiddleLeft: retValue = shaco.Anchor.MiddleLeft; break;
            case TextAnchor.MiddleCenter: retValue = shaco.Anchor.MiddleCenter; break;
            case TextAnchor.MiddleRight: retValue = shaco.Anchor.MiddleRight; break;
            case TextAnchor.LowerLeft: retValue = shaco.Anchor.LowerLeft; break;
            case TextAnchor.LowerCenter: retValue = shaco.Anchor.LowerCenter; break;
            case TextAnchor.LowerRight: retValue = shaco.Anchor.LowerRight; break;
            default: shaco.Log.Error("ToAnchor error: unsupport anchor=" + anchor); break;
        }
        return retValue;
    }

    static public Vector3 ToPivot(this TextAnchor anchor)
    {
        var retValue = shaco.Pivot.UpperLeft;
        switch (anchor)
        {
            case TextAnchor.UpperLeft: retValue = shaco.Pivot.UpperLeft; break;
            case TextAnchor.UpperCenter: retValue = shaco.Pivot.UpperCenter; break;
            case TextAnchor.UpperRight: retValue = shaco.Pivot.UpperRight; break;
            case TextAnchor.MiddleLeft: retValue = shaco.Pivot.MiddleLeft; break;
            case TextAnchor.MiddleCenter: retValue = shaco.Pivot.MiddleCenter; break;
            case TextAnchor.MiddleRight: retValue = shaco.Pivot.MiddleRight; break;
            case TextAnchor.LowerLeft: retValue = shaco.Pivot.LowerLeft; break;
            case TextAnchor.LowerCenter: retValue = shaco.Pivot.LowerCenter; break;
            case TextAnchor.LowerRight: retValue = shaco.Pivot.LowerRight; break;
            default: shaco.Log.Error("ToPivot error: unsupport anchor=" + anchor); break;
        }
        return retValue;
    }

    static public Vector3 ToPivot(this shaco.Anchor anchor)
    {
        var retValue = shaco.Pivot.UpperLeft;
        switch (anchor)
        {
            case shaco.Anchor.UpperLeft: retValue = shaco.Pivot.UpperLeft; break;
            case shaco.Anchor.UpperCenter: retValue = shaco.Pivot.UpperCenter; break;
            case shaco.Anchor.UpperRight: retValue = shaco.Pivot.UpperRight; break;
            case shaco.Anchor.MiddleLeft: retValue = shaco.Pivot.MiddleLeft; break;
            case shaco.Anchor.MiddleCenter: retValue = shaco.Pivot.MiddleCenter; break;
            case shaco.Anchor.MiddleRight: retValue = shaco.Pivot.MiddleRight; break;
            case shaco.Anchor.LowerLeft: retValue = shaco.Pivot.LowerLeft; break;
            case shaco.Anchor.LowerCenter: retValue = shaco.Pivot.LowerCenter; break;
            case shaco.Anchor.LowerRight: retValue = shaco.Pivot.LowerRight; break;
            default: shaco.Log.Error("ToPivot error: unsupport anchor=" + anchor); break;
        }
        return retValue;
    }

    static public Vector3 ToNegativePivot(this TextAnchor anchor)
    {
        var retValue = shaco.Pivot.UpperLeft;
        switch (anchor)
        {
            case TextAnchor.UpperLeft: retValue = shaco.Pivot.LowerRight; break;
            case TextAnchor.UpperCenter: retValue = shaco.Pivot.LowerCenter; break;
            case TextAnchor.UpperRight: retValue = shaco.Pivot.LowerLeft; break;
            case TextAnchor.MiddleLeft: retValue = shaco.Pivot.MiddleRight; break;
            case TextAnchor.MiddleCenter: retValue = shaco.Pivot.MiddleCenter; break;
            case TextAnchor.MiddleRight: retValue = shaco.Pivot.MiddleLeft; break;
            case TextAnchor.LowerLeft: retValue = shaco.Pivot.UpperRight; break;
            case TextAnchor.LowerCenter: retValue = shaco.Pivot.UpperCenter; break;
            case TextAnchor.LowerRight: retValue = shaco.Pivot.UpperLeft; break;
            default: shaco.Log.Error("ToNegativePivot error: unsupport anchor=" + anchor); break;
        }
        return retValue;
    }

    static public Vector3 ToNegativePivot(this shaco.Anchor anchor)
    {
        var retValue = shaco.Pivot.UpperLeft;
        switch (anchor)
        {
            case shaco.Anchor.UpperLeft: retValue = shaco.Pivot.LowerRight; break;
            case shaco.Anchor.UpperCenter: retValue = shaco.Pivot.LowerCenter; break;
            case shaco.Anchor.UpperRight: retValue = shaco.Pivot.LowerLeft; break;
            case shaco.Anchor.MiddleLeft: retValue = shaco.Pivot.MiddleRight; break;
            case shaco.Anchor.MiddleCenter: retValue = shaco.Pivot.MiddleCenter; break;
            case shaco.Anchor.MiddleRight: retValue = shaco.Pivot.MiddleLeft; break;
            case shaco.Anchor.LowerLeft: retValue = shaco.Pivot.UpperRight; break;
            case shaco.Anchor.LowerCenter: retValue = shaco.Pivot.UpperCenter; break;
            case shaco.Anchor.LowerRight: retValue = shaco.Pivot.UpperLeft; break;
            default: shaco.Log.Error("ToNegativePivot error: unsupport anchor=" + anchor); break;
        }
        return retValue;
    }

    static public TextAnchor ToNegativeAnchor(this TextAnchor anchor)
    {
        var retValue = TextAnchor.MiddleCenter;
        switch (anchor)
        {
            case TextAnchor.UpperLeft: retValue = TextAnchor.LowerRight; break;
            case TextAnchor.UpperCenter: retValue = TextAnchor.LowerCenter; break;
            case TextAnchor.UpperRight: retValue = TextAnchor.LowerLeft; break;
            case TextAnchor.MiddleLeft: retValue = TextAnchor.MiddleRight; break;
            case TextAnchor.MiddleCenter: retValue = TextAnchor.MiddleCenter; break;
            case TextAnchor.MiddleRight: retValue = TextAnchor.MiddleLeft; break;
            case TextAnchor.LowerLeft: retValue = TextAnchor.UpperRight; break;
            case TextAnchor.LowerCenter: retValue = TextAnchor.UpperCenter; break;
            case TextAnchor.LowerRight: retValue = TextAnchor.UpperLeft; break;
            default: shaco.Log.Error("ToNegativePivot error: unsupport anchor=" + anchor); break;
        }
        return retValue;
    }

    static public T FindComponent<T>(this GameObject target, bool ignoreActive = false) where T : UnityEngine.Component
    {
        T retValue = default(T);
        if (null == target)
            return retValue;

        bool shouldReturn = false;

        if (!target.activeSelf)
        {
            if (!ignoreActive)
            {
                shouldReturn = true;
            }
        }

        if (!shouldReturn)
        {
            retValue = target.GetComponent<T>();
            if (null != retValue)
            {
                shouldReturn = true;
            }
        }

        if (!shouldReturn)
        {
            for (int i = 0; i < target.transform.childCount; ++i)
            {
                var childTmp = target.transform.GetChild(i);
                retValue = FindComponent<T>(childTmp.gameObject, ignoreActive);

                if (null != retValue)
                {
                    break;
                }
            }
        }
        return retValue;
    }

    static public bool LoadFromResourcesOrLocal(this shaco.Base.BehaviourRootTree tree, string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
    {
        return tree.LoadFromJson(shaco.GameHelper.res.LoadResourcesOrLocal<UnityEngine.Object>(path, multiVersionControlRelativePath).ToString());
    }

    static public void LoadFromResourcesOrLocalAsync(this shaco.Base.BehaviourRootTree tree, string path, System.Action<bool> callbackEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
    {
        shaco.GameHelper.res.LoadResourcesOrLocalAsync<UnityEngine.Object>(path, (obj) =>
        {
            var result = tree.LoadFromJson(obj.ToString());

            try
            {
                callbackEnd(result);
            }
            catch (System.Exception e)
            {
                shaco.Base.Log.Error("ExtensionsClassUnity LoadFromResourcesOrLocalAsync exception: path=" + path + " e=" + e);
            }
        }, null, multiVersionControlRelativePath);
    }

    static public byte[] ToBytes(this UnityEngine.Object value)
    {
        if (value is UnityEngine.TextAsset)
        {
            return ((UnityEngine.TextAsset)value).bytes;
        }
        else if (value is shaco.TextOrigin)
        {
            return ((shaco.TextOrigin)value).bytes;
        }
        else
        {
            shaco.Log.Error("ExtensionsClassUnity ToBytes error: target not have byte data, value=" + value);
            return null;
        }
    }

    static public T GetOrAddComponent<T>(this UnityEngine.Component target) where T : UnityEngine.Component
    {
        return GetOrAddComponent<T>(target.gameObject);
    }

    static public T GetOrAddComponent<T>(this UnityEngine.GameObject gameObject) where T : UnityEngine.Component
    {
        var retValue = gameObject.GetComponent<T>();
        if (null == retValue)
            retValue = gameObject.AddComponent<T>();
        return retValue;
    }

    static public void RemoveComponentIfHave<T>(this UnityEngine.Component target) where T : UnityEngine.Component
    {
        RemoveComponentIfHave<T>(target.gameObject);
    }

    static public void RemoveComponentIfHave<T>(this UnityEngine.GameObject gameObject) where T : UnityEngine.Component
    {
        var component = gameObject.GetComponent<T>();
        if (null != component)
        {
            MonoBehaviour.Destroy(component);
        }
    }

    /// <summary>
    /// 设置世界缩放比例大小
    /// <param name="target">要设置缩放的对象</param>
    /// <param name="worldScale">世界缩放比率</param>
    /// </summary>
    static public void SetWorldScale(this Transform target, Vector3 worldScale)
    {
        var parentScale = Vector3.one;
        var parentTmp = target.parent;
        while (null != parentTmp)
        {
            parentScale.x *= parentTmp.transform.localScale.x;
            parentScale.y *= parentTmp.transform.localScale.y;
            parentScale.z *= parentTmp.transform.localScale.z;

            parentTmp = parentTmp.parent;
        }
        target.transform.localScale = new Vector3(worldScale.x / parentScale.x, worldScale.y / parentScale.y, worldScale.z / parentScale.z);
    }

    /// <summary>
    /// 删除所有子节点
    /// </summary>
    /// <param name="target">父节点对象</param>
    /// <param name="dontDestroyChildren">不希望被删除的子节点</param>
    static public void DestroyChildren(this GameObject target, params GameObject[] dontDestroyChildren)
    {
        DestroyChildren(target.transform, dontDestroyChildren.Convert(v =>  v.transform));
    }

    /// <summary>
    /// 删除所有子节点
    /// </summary>
    /// <param name="target">父节点对象</param>
    /// <param name="dontDestroyChildren">不希望被删除的子节点</param>
    static public void DestroyChildren(this Transform target, params Transform[] dontDestroyChildren)
    {
        bool hasIgnoreChild = !dontDestroyChildren.IsNullOrEmpty();
        for (int i = target.transform.childCount - 1; i >= 0; --i)
        {
            var childTmp = target.transform.GetChild(i);
            bool shouldRemoveChild = true;
            if (hasIgnoreChild)
            {
                for (int j = dontDestroyChildren.Length - 1; j >= 0; --j)
                {
                    if (dontDestroyChildren[j] == childTmp.transform)
                    {
                        shouldRemoveChild = false;
                        break;
                    }
                }
            }

            if (shouldRemoveChild)
            {
                if (!Application.isPlaying)
                    MonoBehaviour.DestroyImmediate(childTmp.gameObject);
                else
                    MonoBehaviour.Destroy(childTmp.gameObject);
            }
        }
    }

    static public void ChangeParent(this Transform target, Transform parent)
    {
        shaco.UnityHelper.ChangeParent(target.gameObject, parent.gameObject);
    }

    static public void ChangeParentLocalPosition(this Transform target, Transform parent)
    {
        shaco.UnityHelper.ChangeParentLocalPosition(target.gameObject, parent.gameObject);
    }

    static public void SetPersistentEventsState(this UnityEngine.Events.UnityEventBase e, UnityEngine.Events.UnityEventCallState state)
    {
        var countTmp = e.GetPersistentEventCount();
        for (int i = 0; i < countTmp; ++i)
        {
            e.SetPersistentListenerState(i, state);
        }
    }

    static public void SetLayerRecursive(this GameObject target, int layer)
    {
        target.layer = layer;
        shaco.UnityHelper.ForeachChildren(target, (index, child) =>
        {
            child.layer = layer;
            return true;
        });
    }

    /// <summary>
    /// 递归查找所有子节点名字
    /// </summary>
    static public Transform FindRecursive(this Transform parent, string targetName)
    {
        if (null == parent)
            return null;

        Transform retValue = null;
        retValue = parent.Find(targetName);
        if (null == retValue)
        {
            foreach (Transform child in parent)
            {
                retValue = FindRecursive(child, targetName);
                if (null != retValue)
                {
                    return retValue;
                }
            }
        }
        return retValue;
    }

    static public void LoadFromResourcesOrLocal(this shaco.Base.BadWordsFilter badWordsFilter, string path, System.Action<float> callbackProgress = null, string splitFlag = "\n")
    {
        if (path.IndexOf('\\') >= 0)
            path = path.Replace('\\', shaco.Base.FileDefine.PATH_FLAG_SPLIT);

        if (null == callbackProgress)
            badWordsFilter.LoadFromString(shaco.GameHelper.res.LoadResourcesOrLocal<UnityEngine.Object>(path).ToString(), callbackProgress, splitFlag);
        else
        {
            shaco.GameHelper.res.LoadResourcesOrLocalAsync<UnityEngine.Object>(path, (loadData) =>
            {
                if (null == loadData)
                {
                    callbackProgress(1);
                }
                else
                {
                    badWordsFilter.LoadFromString(loadData.ToString(), (float percent2) =>
                    {
                        callbackProgress(percent2 * 0.1f + 0.9f);
                    }, splitFlag);
                }
            }, (float percent) =>
            {
                callbackProgress(percent * 0.9f);
            });
        }
    }
}
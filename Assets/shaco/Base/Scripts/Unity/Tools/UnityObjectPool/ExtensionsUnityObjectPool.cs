using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class shaco_ExtensionsUnityObjectPool
{
    static public T InstantiateWithPool<T>(this T obj, string key) where T : UnityEngine.Object
	{
		return shaco.GameHelper.objectpool.Instantiate(key, ()=>
        {
            return MonoBehaviour.Instantiate(obj);
        });
	}

	static public T InstantiateWithPool<T>(this T obj, string key, System.Func<T> callbackCreate) where T : UnityEngine.Object
	{
		return shaco.GameHelper.objectpool.Instantiate(key, callbackCreate);
	}

	static public object RecyclingWithPool(this UnityEngine.Object obj)
	{
		return shaco.GameHelper.objectpool.RecyclingObject(obj);
	}

	static public object DestroyWithPool(this UnityEngine.Object obj)
	{
		return shaco.GameHelper.objectpool.DestroyObject(obj);
	}

    #region Object Pool with object
    static public T AutoRecyclingWithPool<T>(this T obj, UnityEngine.GameObject bindTarget)
    {
        var component = GetOrCreateAutoReleaseComponent(bindTarget);
        if (null != component)
        {
            component.AddOnDestroyCallBack(obj, () =>
            {
                if (shaco.GameHelper.objectpool.IsInPool(obj))
                    obj.RecyclingWithPool();
            });
        }
        return obj;
    }

    static public T AutoRecyclingWithPool<T>(this T obj, UnityEngine.Component bindTarget)
    {
        return AutoRecyclingWithPool(obj, bindTarget.gameObject);
    }

    static public T AutoDestroyWithPool<T>(this T obj, UnityEngine.GameObject bindTarget)
    {
        var component = GetOrCreateAutoReleaseComponent(bindTarget);
        if (null != component)
        {
            component.AddOnDestroyCallBack(obj, () =>
            {
                if (shaco.GameHelper.objectpool.IsInPool(obj))
                    obj.DestroyWithPool();
            });
        }
        return obj;
    }

    static public T AutoDestroyWithPool<T>(this T obj, UnityEngine.Component bindTarget)
    {
        return AutoDestroyWithPool(obj, bindTarget.gameObject);
    }

    #endregion //Object Pool with object

    #region Assetbundle
    static public string AutoUnLoad(this string pathAssetBundle, UnityEngine.GameObject bindTarget, string multiVersionControlRelativePath, bool unloadAllLoadedObjects = true)
    {
        var component = GetOrCreateAutoReleaseComponent(bindTarget);
        if (null != component)
        {
            component.AddOnDestroyCallBack(pathAssetBundle, () =>
            {
                if (shaco.GameHelper.resCache.IsLoadedAssetBundle(pathAssetBundle, multiVersionControlRelativePath) || shaco.GameHelper.resCache.IsLoadingAssetBundle(pathAssetBundle, multiVersionControlRelativePath))
                    shaco.GameHelper.resCache.UnloadAssetBundle(pathAssetBundle, unloadAllLoadedObjects, multiVersionControlRelativePath);
            });
        }
        return pathAssetBundle;
    }

    static public string AutoUnLoad(this string pathAssetBundle, UnityEngine.Component bindTarget, string multiVersionControlRelativePath, bool unloadAllLoadedObjects = true)
    {
        return AutoUnLoad(pathAssetBundle, bindTarget.gameObject, multiVersionControlRelativePath, unloadAllLoadedObjects);
    }

    static public string AutoUnLoad(this string pathAssetBundle, UnityEngine.GameObject bindTarget, bool unloadAllLoadedObjects = true)
    {
        return AutoUnLoad(pathAssetBundle, bindTarget, string.Empty, unloadAllLoadedObjects);
    }

    static public string AutoUnLoad(this string pathAssetBundle, UnityEngine.Component bindTarget, bool unloadAllLoadedObjects = true)
    {
        return AutoUnLoad(pathAssetBundle, bindTarget.gameObject, string.Empty, unloadAllLoadedObjects);
    }

    static public void CancelAutoRelease(this object target, UnityEngine.Component bindTarget)
    {
        CancelAutoRelease(target, bindTarget.gameObject);
    }

    static public void CancelAutoRelease(this object target, UnityEngine.GameObject bindTarget)
    {
        var component = bindTarget.GetComponent<shaco.UnityObjectAutoReleaseComponent>();
        if (null == component)
        {
            shaco.Log.Error("ExtensionsUnityObjectPool CancelAutoRelease error: not found 'shaco.UnityObjectAutoReleaseComponent' bindTarget=" + bindTarget + " target=" + target);
            return;
        }
        component.RemoveOnDestroyCallBacks(target);
    }

    static public void CancelAutoRelease(this object target, UnityEngine.Component bindTarget, System.Action callback)
    {
        CancelAutoRelease(target, bindTarget.gameObject, callback);
    }

    static public void CancelAutoRelease(this object target, UnityEngine.GameObject bindTarget, System.Action callback)
    {
        var component = bindTarget.GetComponent<shaco.UnityObjectAutoReleaseComponent>();
        if (null == component)
        {
            shaco.Log.Error("ExtensionsUnityObjectPool CancelAutoRelease error: not found 'shaco.UnityObjectAutoReleaseComponent' bindTarget=" + bindTarget + " target=" + target);
            return;
        }
        component.RemoveOnDestroyCallBack(target, callback);
    }

    static private shaco.UnityObjectAutoReleaseComponent GetOrCreateAutoReleaseComponent(UnityEngine.GameObject bindTarget)
    {
        if (null == bindTarget)
        {
            shaco.Log.Error("ExtensionsUnityObjectPool GetOrCreateAutoReleaseComponent erorr: target is null");
            return null;
        }

        var retValue = bindTarget.GetComponent<shaco.UnityObjectAutoReleaseComponent>();
        if (null == retValue)
        {
            retValue = bindTarget.AddComponent<shaco.UnityObjectAutoReleaseComponent>();
        }
        return retValue;
    }
    #endregion //Assetbundle

    static public void AutoStopUpLoad(this shaco.IHttpUpLoader uploader, UnityEngine.GameObject bindTarget)
    {
        if (null == bindTarget)
            return;
            
        var component = GetOrCreateAutoReleaseComponent(bindTarget);
        if (null != component)
        {
            System.Action callbackEnd = () =>
            {
                uploader.Stop();
            };

            component.AddOnDestroyCallBack(bindTarget, callbackEnd);
            uploader.AddEndCallBack((bytes, error) =>
            {
                component.RemoveOnDestroyCallBack(bindTarget, callbackEnd);
            });
        }
    }

    static public void AutoStopUpLoad(this shaco.IHttpUpLoader uploader, UnityEngine.Component bindTarget)
    {
        if (null == bindTarget)
            return;

        AutoStopUpLoad(uploader, bindTarget.gameObject);
    }

    static public void AutoStopDownload(this shaco.IHttpDownLoader downloader, UnityEngine.GameObject bindTarget)
    {
        var component = GetOrCreateAutoReleaseComponent(bindTarget);
        if (null != component)
        {
            System.Action callbackEnd = () =>
            {
                downloader.Stop();
            };

            component.AddOnDestroyCallBack(bindTarget, callbackEnd);
            downloader.AddEndCallBack((bytes, error) =>
            {
                component.RemoveOnDestroyCallBack(bindTarget, callbackEnd);
            });
        }
    }

    static public void AutoStopDownload(this shaco.IHttpDownLoader downloader, UnityEngine.Component bindTarget)
    {
        AutoStopDownload(downloader, bindTarget.gameObject);
    }
}

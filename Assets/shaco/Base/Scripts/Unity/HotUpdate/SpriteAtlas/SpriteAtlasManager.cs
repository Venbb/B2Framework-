using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class SpriteAtlasManager : ISpriteAtlasManager, shaco.Base.IGameInstanceCreator
    {
        private int _checkLoadAtlasCount = 0;
        private bool _isLoadWithAsync = false;

        static public object Create()
        {
            // shaco.Log.Info("SpriteAtlasManager Create");

            var retValue = new SpriteAtlasManager();
#if UNITY_2017_1_OR_NEWER
            UnityEngine.U2D.SpriteAtlasManager.atlasRequested -= retValue.OnAtlasRequested;
            UnityEngine.U2D.SpriteAtlasManager.atlasRequested += retValue.OnAtlasRequested;
#else
            shaco.Log.Error("SpriteAtlasManager Create error: only support on Unity2017 or upper !");
#endif
            return retValue;
        }

        private void OnAtlasRequested(string atlasName, System.Action<UnityEngine.U2D.SpriteAtlas> callbackLoadEnd)
        {
            var atlasPath = shaco.GameHelper.atlasSettings.GetAtlasPath(atlasName);
            var multiVersionControlRelativePath = shaco.GameHelper.atlasSettings.GetMultiVersionControlRelativePath(atlasName);

#if UNITY_2017_1_OR_NEWER
            if (_isLoadWithAsync)
            {
#if !UNITY_2018_2_OR_NEWER
                _isLoadWithAsync = false;
                shaco.Log.Error("SpriteAtlasManager OnAtlasRequested error: load with async only support on Unity2018.2.1f1 or upper !");
                return;
#endif

                ++_checkLoadAtlasCount;

                //异步加载图集需要2018.2.1f1之后的版本才支持
                shaco.Log.Info("SpriteAtlasManager OnAtlasRequested: async atlasName=" + atlasName);
                shaco.GameHelper.res.LoadResourcesOrLocalAsync<UnityEngine.U2D.SpriteAtlas>(atlasPath, (loadAtlas) =>
                {
                    try
                    {
                        //关闭多余的引用计数
                        callbackLoadEnd(loadAtlas);
                        shaco.GameHelper.resCache.UnloadAssetBundle(atlasPath, false);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("SpriteAtlasManager OnAtlasRequested 1 erorr: atlasName=" + atlasName + " callbackLoadEnd=" + callbackLoadEnd + " e=" + e);
                    }
                    --_checkLoadAtlasCount;
                }, null, multiVersionControlRelativePath, false);
            }
            else
            {
                //旧版本的图集必须使用同步加载
                shaco.Log.Info("SpriteAtlasManager OnAtlasRequested: atlasName=" + atlasName);
                var loadAtlas = shaco.GameHelper.res.LoadResourcesOrLocal<UnityEngine.U2D.SpriteAtlas>(atlasPath, multiVersionControlRelativePath, false);

                try
                {
                    //关闭多余的引用计数
                    shaco.GameHelper.resCache.UnloadAssetBundle(atlasPath, false);
                    callbackLoadEnd(loadAtlas);
                }
                catch (System.Exception e)
                {
                    Log.Error("SpriteAtlasManager OnAtlasRequested 2 erorr: atlasName=" + atlasName + " callbackLoadEnd=" + callbackLoadEnd + " e=" + e);
                }
            }
#endif
        }

        /// <summary>
        /// 开始检查图集是否被请求同步加载
        /// </summary>
        public void StartCheckLoadAtlas()
        {
            _isLoadWithAsync = false;
        }

        /// <summary>
        /// 等图集同步加载完毕
        /// </summary>
        public void EndCheckLoadAtlas(System.Type type)
        {
        }

        /// <summary>
        /// 开始检查图集是否被请求加载
        /// </summary>
        public void StartCheckLoadAtlasAysnc()
        {
            _isLoadWithAsync = true;
        }

        /// <summary>
        /// 等图集加载完毕
        /// </summary>
        public void EndCheckLoadAtlasAsync(System.Type type, System.Action callbackEnd)
        {
            //当没有图集在加载，或者资源类型本身就是图集的时候都直接返回
            if (0 >= _checkLoadAtlasCount || type == typeof(UnityEngine.U2D.SpriteAtlas))
            {
                //没有请求图集，直接返回了
                _isLoadWithAsync = false;
                if (null != callbackEnd)
                {
                    try
                    {
                        callbackEnd();
                    }
                    catch (System.Exception e)
                    {
                        shaco.Log.Error("SpriteAtlasManager EndCheckLoadAtlasAsync exception: e=" + e);
                    }
                }
            }
            else
            {
                //等待图集加载完毕
                shaco.Base.WaitFor.Run(() =>
                {
                    return 0 >= _checkLoadAtlasCount;
                }, () =>
                {
                    _isLoadWithAsync = false;
                    if (null != callbackEnd)
                    {
                        try
                        {
                            callbackEnd();
                        }
                        catch (System.Exception e)
                        {
                            shaco.Log.Error("SpriteAtlasManager EndCheckLoadAtlasAsync exception: e=" + e);
                        }
                    }
                });
            }
        }
    }
}
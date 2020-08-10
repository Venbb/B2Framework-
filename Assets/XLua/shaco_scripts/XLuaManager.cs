using UnityEngine;
using XLua;

namespace shaco
{
    //不生成热更新脚本过滤黑名单(过滤相关的所有域名空间)
    public class BlackListNamespaceAttribute : System.Attribute
    {

    }

    public class XLuaManager
    {
        //lua状态机
        static public LuaEnv luaenv
        {
            get
            {
                if (null == _luaenv)
                {
                    _luaenv = new LuaEnv();
                    var luaExtensions = shaco.GameHelper.res.LoadResourcesOrLocal<UnityEngine.Object>("shaco/xlua/FrameworkHeader" + shaco.Base.GlobalParams.LUA_FILE_EXTENSIONS);
                    if (null != luaExtensions)
                        _luaenv.DoString(luaExtensions.ToBytes());
                }
                return _luaenv;
            }
        }
        static private LuaEnv _luaenv = null;

        /// <summary>
        /// 运行目录下所有lua脚本
        /// </summary>
        /// <param name="path">目录路径，支持从Unity的Resources目录，或者下载到本地的asset/resources_hotupdate目录</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <param name="callbackProgress">加载lua进度</param>
        /// <param name="callbackEnd">加载lua脚本完毕后回调</param>
        /// <param name="extension">资源后缀名，如果为None则默认以AssetBundle资源进行加载</param>
        static public void RunWithFolder(string path, System.Action callbackEnd, System.Action<float> callbackProgress, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            shaco.GameHelper.res.LoadResourcesOrLocalAsyncAll(path, (Object[] readObjs) =>
            {
                Log.Info("XLuaManager RunWithFolder: path=" + path);

                //如果存在下载目录，则自动销毁ab包资源
                var fullPath = shaco.HotUpdateHelper.GetAssetBundleFullPath(path, multiVersionControlRelativePath);

                //Unity部分方法限制了函数必须从协程加载
                RunWithFolderCoroutine(fullPath, multiVersionControlRelativePath, callbackEnd, readObjs);

            }, (float progressPercent) =>
            {
                if (null != callbackProgress)
                {
                    try
                    {
                        callbackProgress(progressPercent);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("XLuaManager RunWithFolder error: path=" + path + " callbackEnd=" + callbackEnd + " callbackProgress=" + callbackProgress + " e=" + e);
                    }
                }
            }, multiVersionControlRelativePath);
        }

        /// <summary>
        /// 运行目录下所有lua脚本
        /// </summary>
        /// <param name="path">目录路径，支持从Unity的Resources目录，或者下载到本地的asset/resources_hotupdate目录</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <param name="callbackEnd">加载lua脚本完毕后回调</param>
        /// <param name="extension">资源后缀名，如果为None则默认以AssetBundle资源进行加载</param>
        static public void RunWithFolder(string path, System.Action callbackEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            RunWithFolder(path, callbackEnd, null, multiVersionControlRelativePath);
        }

        /// <summary>
        /// 运行目录下所有lua脚本
        /// </summary>
        /// <param name="path">目录路径，支持从Unity的Resources目录，或者下载到本地的asset/resources_hotupdate目录</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        static public void RunWithFolder(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            RunWithFolder(path, null, null, multiVersionControlRelativePath);
        }

        /// <summary>
        /// 运行一个lua脚本
        /// </summary>
        /// <param name="path">文件路径，支持从Unity的Resources目录，或者下载到本地的asset/resources_hotupdate目录</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        static public void RunWithFile(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            var readObjTmp = shaco.GameHelper.res.LoadResourcesOrLocal<UnityEngine.Object>(path, multiVersionControlRelativePath);
            if (null == readObjTmp)
                return;

            try
            {
                Log.Info("XLuaManager RunWithFile: path=" + path);
                RunWithString(readObjTmp.ToString());

                //如果存在已加载的缓存文件，则自动销毁ab包资源
                shaco.GameHelper.res.UnloadAssetBundleLocal(path, true, multiVersionControlRelativePath);
            }
            catch (System.Exception e)
            {
                Log.Error("XLuaManager RunWithFile error=" + e);
            }
        }

        /// <summary>
        /// 直接运行lua脚本
        /// </summary>
        /// <param name="luaScript">lua脚本内容</param>
        static public void RunWithString(string luaScript)
        {
            try
            {
                luaenv.DoString(luaScript);
            }
            catch (System.Exception e)
            {
                Log.Error("XLuaManager RunWithString error=" + e);
            }
        }

        /// <summary>
        /// 根据Lua文件描述字段获取可能对应的脚本名字
        /// 如果存在重名的情况，则获取第一个脚本
        /// </summary>
        static public string GetLuaScriptPath(string source, int line)
        {
#if UNITY_EDITOR
            if (System.IO.File.Exists(source))
            {
                return string.Format("<color=#{0}>({1} at {2}:{3})</color>", shaco.Base.GlobalParams.FORCE_LOG_LOCATION_COLOR, shaco.Base.GlobalParams.FORCE_LOG_LOCATION_TAG, source, line);
            }

            var lastFileName = string.Empty;
            if (source.EndsWith(".lua"))
            {
                lastFileName = System.IO.Path.GetFileName(source);
            }
            else
            {
                var splirtNames = source.Split('.');
                lastFileName = splirtNames[splirtNames.Length - 1];
                lastFileName = System.IO.Path.ChangeExtension(lastFileName, ".lua");
            }

            var filter = lastFileName + " t:TextAsset";
            var result = UnityEditor.AssetDatabase.FindAssets(filter, new string[] { "Assets" });
            if (null == result || result.Length == 0)
                return source;

            var paths = result.Convert(v => UnityEditor.AssetDatabase.GUIDToAssetPath(v));
            if (paths.Length > 1)
            {
                Log.Warning("XLuaManager GetLuaScriptPath warning: have duplicate source path\n" + paths.ToContactString("\n"));
            }
            return string.Format("<color=#{0}>({1} at {2}:{3})</color>", shaco.Base.GlobalParams.FORCE_LOG_LOCATION_COLOR, shaco.Base.GlobalParams.FORCE_LOG_LOCATION_TAG, paths[0], line);
#else
            return source + ":" + line;
#endif
        }

        /// <summary>
        /// 从本地Resources目录读取所有lua文件
        /// <param name="fullPath">目录绝对路径</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <param name="callbackEnd">加载lua脚本完毕后回调</param>
        /// <param name="readObjs">读取到的所有文件</param>
        /// </summary>
        static private void RunWithFolderCoroutine(string fullPath, string multiVersionControlRelativePath, System.Action callbackEnd, System.Object[] readObjs)
        {
            if (readObjs.IsNullOrEmpty())
            {
                if (null != callbackEnd)
                {
                    try
                    {
                        callbackEnd();
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("XLuaManager RunWithFolderCoroutine error: fullPath=" + fullPath + " callbackEnd=" + callbackEnd + " e=" + e);
                    }
                }
                return;
            }
            shaco.Base.Coroutine.Foreach(readObjs, (object obj) =>
            {
                var pathOneFile = obj.ToString();
                shaco.XLuaManager.RunWithString(pathOneFile);
                return true;
            }, (float percent) =>
            {
                //加载完毕
                if (percent >= 1.0f)
                {
                    //如果存在下载目录，则自动销毁ab包资源
                    var extensionsLua = shaco.Base.FileHelper.GetExtension(shaco.Base.FileDefine.FileExtension.Lua);
                    shaco.GameHelper.resCache.UnloadAssetBundlesWithNameFlag(extensionsLua, true, multiVersionControlRelativePath);

                    if (null != callbackEnd)
                    {
                        System.GC.Collect();

                        try
                        {
                            callbackEnd();
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("XLuaManager RunWithFolderCoroutine error: fullPath=" + fullPath + " callbackEnd=" + callbackEnd + " e=" + e);
                        }
                    }
                }
            });
        }
    }
}
using UnityEngine;
using System.Collections;

namespace shaco
{
    static public class shaco_ExtensionsLocalization
    {
        private const string LOCALIZATION_LANGUAGE_KEY = "localizaiton_language";
        private const string LOCALIZATION_LOAD_PATH = "localizaiton_load_path";
        private const string LOCALIZATION_MULTI_VERSION_PATH = "localizaiton_multi_version_path";

        static private readonly SystemLanguage[] DEFAULT_LANGUAGES = new SystemLanguage[] { SystemLanguage.Japanese, SystemLanguage.English, SystemLanguage.Chinese };

        // /// <summary>
        // /// 资源加载路径
        // /// </summary>
        // public string path { get; set; }

        // /// <summary>
        // /// 资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源
        // /// </summary>
        // public string multiVersionControlRelativePath { get; set; }

        /// <summary>
        /// 从本地目录加载本地化语言配置
        /// <param name="localization">本地化对象</param>
        /// <param name="path">资源路径</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        static public bool LoadWithJsonResourcesOrLocalPath(this shaco.Base.ILocalization localization, string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            var convertPath = GetJsonPathWithLanguage(path, GetCurrentLanuageString());
            if (shaco.Base.FileHelper.HasFileNameExtension(convertPath))
                convertPath = shaco.Base.FileHelper.RemoveLastExtension(convertPath);

            string loadString = shaco.GameHelper.res.LoadResourcesOrLocal<Object>(convertPath, multiVersionControlRelativePath).ToString();

            if (string.IsNullOrEmpty(loadString))
            {
                Log.Error("ExtensionsLocalization LoadWithJsonResourcesPath error: not find json, convertPath=" + convertPath);
                return false;
            }

            shaco.GameHelper.datasave.WriteString(LOCALIZATION_LOAD_PATH, path);
            shaco.GameHelper.datasave.WriteString(LOCALIZATION_MULTI_VERSION_PATH, multiVersionControlRelativePath);
            return localization.LoadWithJsonString(loadString);
        }

        /// <summary>
        /// 从本地目录异步加载本地化语言配置
        /// <param name="localization">本地化对象</param>
        /// <param name="path">资源路径</param>
        /// <param name="callbackProgress">加载进度回调</param>
        /// <param name="callbackEnd">加载完毕回调</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        static public void LoadWithJsonResourcesOrLocalPathAsync(this shaco.Base.ILocalization localization, string path, System.Action<float> callbackProgress = null, System.Action callbackEnd = null, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            var convertPath = GetJsonPathWithLanguage(path, GetCurrentLanuageString());
            if (shaco.Base.FileHelper.HasFileNameExtension(convertPath))
                convertPath = shaco.Base.FileHelper.RemoveLastExtension(convertPath);

            shaco.GameHelper.res.LoadResourcesOrLocalAsync<Object>(convertPath, (loadObj)=>
            {
                var loadString = null != loadObj ? loadObj.ToString() : null;
                if (!string.IsNullOrEmpty(loadString))
                {
                    shaco.GameHelper.datasave.WriteString(LOCALIZATION_LOAD_PATH, path);
                    shaco.GameHelper.datasave.WriteString(LOCALIZATION_MULTI_VERSION_PATH, multiVersionControlRelativePath);
                    localization.LoadWithJsonString(loadString);
                }

                if (null != callbackEnd)
                {
                    try
                    {
                        callbackEnd();
                    }
                    catch (System.Exception e)
                    {
                        shaco.Log.Error("ExtensionsLocalization LoadWithJsonResourcesOrLocalPathAsync exception: e=" + e);
                    }
                }
            }, (percent)=>
            {
                if (null != callbackProgress)
                    callbackProgress(percent);
            }, multiVersionControlRelativePath);
        }

        /// <summary>
        /// 获取本地化语言资源
        /// <param name="localization">本地化对象</param>
        /// <param name="key">键值</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <return>加载的资源</return>
        /// </summary>
        static public T GetResource<T>(this shaco.Base.ILocalization localization, string key, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString) where T : UnityEngine.Object
        {
            var pathTmp = localization.GetText(key, string.Empty);
            
            if (!string.IsNullOrEmpty(pathTmp))
            {
                return shaco.GameHelper.res.LoadResourcesOrLocal<T>(pathTmp, multiVersionControlRelativePath);
            }
            else
            {
                Log.Error("ExtensionsLocalization GetResource error: not found value by key=" + key);
                return null;
            }
        }

        /// <summary>
        /// 获取本地化语言资源
        /// <param name="localization">本地化对象</param>
        /// <param name="key">键值</param>
        /// <param name="callbackProgress">加载进度回调</param>
        /// <param name="callbackEnd">加载完毕回调</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <return></return>
        /// </summary>
        static public void GetResourceAsync<T>(this shaco.Base.ILocalization localization, string key, System.Action<float> callbackProgress = null, System.Action<T> callbackEnd = null, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString) where T : UnityEngine.Object
        {
            var pathTmp = localization.GetText(key, "null");

            if (pathTmp != "null")
            {
                shaco.GameHelper.res.LoadResourcesOrLocalAsync<T>(pathTmp, (loadObj)=>
                {
                    if (null != callbackEnd)
                    {
                        try
                        {
                            callbackEnd((T)loadObj);
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("RichText GetResourceAsync exception: key=" + key + " e=" + e);
                        }
                    }
                }, (percent)=>
                {
                    if (null != callbackProgress)
                        callbackProgress(percent);
                }, multiVersionControlRelativePath);
            }
            else
            {
                Log.Error("ExtensionsLocalization GetResource error: not found value by key=" + key);

                if (null != callbackProgress)
                    callbackProgress(1);
                if (null != callbackEnd)
                {
                    try
                    {
                        callbackEnd(null);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("RichText GetResourceAsync exception: e=" + e);
                    }
                }
            }
        }

        /// <summary>
        /// 设置当前显示的语言，会自动刷新当前界面所有本地化组件
        /// <param name="localization">本地化对象</param>
        /// <param name="language">语言类型</param>
        /// </summary>
        static public void SetCurrentLanguageAsync(this shaco.Base.ILocalization localization, SystemLanguage language, System.Action callbackEnd = null)
        {
            var path = shaco.GameHelper.datasave.ReadString(LOCALIZATION_LOAD_PATH);
            var multiVersionControlRelativePath = shaco.GameHelper.datasave.ReadString(LOCALIZATION_MULTI_VERSION_PATH);

            if (string.IsNullOrEmpty(path))
            {
                Log.Error("ExtensionsLocalization SetCurrentLanguage error: not found loaded localization configuration path, please call LoadWithJsonResourcesOrLocalPath at first");
                return;
            }

            //清空原来设置
            localization.Clear();

            //设置当前语言
            shaco.GameHelper.datasave.WriteString(LOCALIZATION_LANGUAGE_KEY, language.ToString());

            //根据上次设定的路径重新加载资源
            localization.LoadWithJsonResourcesOrLocalPathAsync(path, null, ()=>
            {
                //刷新本地显示内容
                RealoadAllLocalizationComponentsInScene();

                if (null != callbackEnd)
                {
                    try
                    {
                        callbackEnd();
                    }
                    catch (System.Exception e)
                    {
                        shaco.Log.Error("ExtensionsLocalization SetCurrentLanguageAsync exception: e=" + e);
                    }
                }
            }, multiVersionControlRelativePath);
        }

        /// <summary>
        /// 刷新界面中所有本地化语言组件，一般来说在语言切换后调用
        /// </summary>
        static private void RealoadAllLocalizationComponentsInScene()
        {
            var allCompnents = Resources.FindObjectsOfTypeAll<shaco.LocalizationComponent>();
            for (int i = allCompnents.Length - 1; i >= 0; --i)
            {
                allCompnents[i].UpdateLocalization();
            }
        }

        /// <summary>
        /// 获取当前语言字符串
        /// <param name="language">语言类型，如果设置为未知则会自动匹配之前设定过的语言或者系统默认语言</param>
        /// </summary>
        static private string GetCurrentLanuageString()
        {
            //获取上一次设定的语言key
            var retValue = shaco.GameHelper.datasave.ReadString(LOCALIZATION_LANGUAGE_KEY);
        
            //当没有设定过语言时候，自动根据系统语言来设定语言key
            if (string.IsNullOrEmpty(retValue) && DEFAULT_LANGUAGES.Length > 0)
            {
                bool isSupportLanguage = false;
                string setLanguage = string.Empty;

                for (int i = 0; i < DEFAULT_LANGUAGES.Length; ++i)
                {
                    if (DEFAULT_LANGUAGES[i] == Application.systemLanguage)
                    {
                        isSupportLanguage = true;
                        setLanguage = Application.systemLanguage.ToString();
                        break;
                    }
                }

                if (!isSupportLanguage)
                {
                    Log.Error("Localizaiton AutoSetCurrentLanuageWithLocalSave error: no support language set as default, Application.systemLanguage=" + Application.systemLanguage);
                    setLanguage = DEFAULT_LANGUAGES[0].ToString();
                }

                retValue = setLanguage;
            }
            
            if (string.IsNullOrEmpty(retValue))
                Log.Error("Localizaiton AutoSetCurrentLanuageWithLocalSave error: language settings is empty");

            //保存当前设定的语言
            shaco.GameHelper.datasave.WriteString(LOCALIZATION_LANGUAGE_KEY, retValue);
            return retValue;
        }

        /// <summary>
        /// 根据当前设定的语言字符串获取对应的json文件路径
        /// <param name="path">原文件路径</param>
        /// <param name="languageName">语言名称</param>
        /// <return>转换后的文件路径</return>
        /// </summary>
        static private string GetJsonPathWithLanguage(string path, string languageName)
        {
            if (string.IsNullOrEmpty(languageName))
                return path;

            string retValue = string.Empty;
            string folderTmp = shaco.Base.FileHelper.GetFolderNameByPath(path);
            string fileNameTmp = shaco.Base.FileHelper.GetLastFileName(path);
            bool hasExtensionsTmp = shaco.Base.FileHelper.HasFileNameExtension(fileNameTmp);
            string fileNameWithNoExtensionsTmp = hasExtensionsTmp ? shaco.Base.FileHelper.RemoveLastExtension(fileNameTmp) : fileNameTmp;
            string extensionsTmp = hasExtensionsTmp ? shaco.Base.FileHelper.GetFilNameExtension(fileNameTmp) : string.Empty;

            if (!string.IsNullOrEmpty(fileNameWithNoExtensionsTmp))
            {
                fileNameWithNoExtensionsTmp += "_";
            }

            if (!hasExtensionsTmp)
            {
                extensionsTmp = "json";
            }

            retValue = folderTmp + fileNameWithNoExtensionsTmp + languageName + (!hasExtensionsTmp ? shaco.Base.FileDefine.DOT_SPLIT_STRING : string.Empty) + extensionsTmp;
            return retValue;
        }
    }
}


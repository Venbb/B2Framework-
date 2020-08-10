using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    /// <summary>
    /// 图集配置类
    /// </summary>
    public class SpriteAtlasSettings : ScriptableObject, ISpriteAtlasSettings, shaco.Base.IGameInstanceCreator
    {
        public const string SETTINGS_PATH_KEY = "shaco.RuntimeSpriteAtlasSettings";

        [SerializeField]
        private List<SpriteAtlasSettingsInfo> _atlasInfo = new List<SpriteAtlasSettingsInfo>();

        private Dictionary<string, SpriteAtlasSettingsInfo> _atlasNameDic = new Dictionary<string, SpriteAtlasSettingsInfo>();
        private Dictionary<string, SpriteAtlasSettingsInfo> _atlasPathDic = new Dictionary<string, SpriteAtlasSettingsInfo>();

        private Dictionary<string, string> _atlasNameToFolder = null;
        private System.Text.StringBuilder _appendStringCache = new System.Text.StringBuilder();

        static public object Create()
        {
            return ScriptableObject.CreateInstance<SpriteAtlasSettings>();
        }

        public void Clear()
        {
            _atlasInfo.Clear();
            _atlasNameDic.Clear();
            _atlasPathDic.Clear();
        }

        public void AddAtlasInfo(SpriteAtlasSettingsInfo info)
        {
            if (_atlasNameDic.ContainsKey(info.atlasName))
            {
                Log.Error("SpriteAtlasSettings AddAtlasInfo error: duplicate atlas name=" + info.atlasName + "\npath=" + info.atlasFolder);
                return;
            }

            if (_atlasPathDic.ContainsKey(info.atlasFolder))
            {
                Log.Error("SpriteAtlasSettings AddAtlasInfo error: duplicate atlas path=" + info.atlasFolder + "\nname=" + info.atlasName);
                return;
            }
            _atlasInfo.Add(info);
            _atlasNameDic.Add(info.atlasName, info);
            _atlasPathDic.Add(info.atlasFolder, info);
        }

        public bool ContainsAtlasName(string atlasName)
        {
            return _atlasNameDic.ContainsKey(atlasName);
        }

        public bool ContainsAtlasPath(string atlasPath)
        {
            return _atlasPathDic.ContainsKey(atlasPath);
        }

        public string GetAtlasPath(string atlasName)
        {
            var folderPath = GetAtlasFolderPath(atlasName);
            string retValue = string.Empty;

            lock (_appendStringCache)
            {
                _appendStringCache.Length = 0;

                if (string.IsNullOrEmpty(folderPath))
                {
                    _appendStringCache.Append(atlasName);
                }
                else
                {
                    _appendStringCache.Append(folderPath);
                    _appendStringCache.Append(shaco.Base.FileDefine.PATH_FLAG_SPLIT);
                    _appendStringCache.Append(atlasName);
                }
                retValue = _appendStringCache.ToString();
            }
            return retValue;
        }

        public string GetMultiVersionControlRelativePath(string atlasName)
        {
            return string.Empty;//@@@待实现功能，目前所有图集暂时仅支持全局版本路径
        }

        public string GetAtlasFolderPath(string atlasName)
        {
            CheckInit();

            string findFolder = null;
            if (!_atlasNameToFolder.TryGetValue(atlasName, out findFolder))
            {
                //如果没有获取到图集文件夹名字，则视为没有开启自动化图集配置
                // Log.Error("SpriteAtlasSettings GetAtlasFolderPath error: not found altasName=" + atlasName);
                return string.Empty;
            }
            return findFolder;
        }

        private void CheckInit()
        {
            if (null != _atlasNameToFolder)
                return;

            _atlasNameToFolder = new Dictionary<string, string>();

            var settingPath = shaco.GameHelper.gameConfig.ReadString(shaco.SpriteAtlasSettings.SETTINGS_PATH_KEY);
            if (string.IsNullOrEmpty(settingPath))
            {
                //如果想开启自动化图集优化功能，需要使用SpriteAtlasSettingsWindow窗口创建一个运行时图集配置文件
                // Log.Error("SpriteAtlasSettings CheckInit erorr: not found setting asset, please use 'SpriteAtlasSettingsWindow' to create it");
                return;
            }

            var resourcesLoadPath = settingPath.RemoveFront("Resources/");
            if (string.IsNullOrEmpty(resourcesLoadPath))
            {
                Log.Error("SpriteAtlasSettings CheckInit erorr: invalid setting path=" + settingPath);
                return;
            }

            this.Clear();

            var loadSettings = GameHelper.res.LoadResourcesOrLocal<SpriteAtlasSettings>(resourcesLoadPath);
            for (int i = loadSettings._atlasInfo.Count - 1; i >= 0; --i)
                AddAtlasInfo(loadSettings._atlasInfo[i]);

            if (_atlasInfo.IsNullOrEmpty())
            {
                Log.Error("SpriteAtlasSettings CheckInit erorr: altas info is empty, make sure it correct, path=" + resourcesLoadPath);
                return;
            }

            for (int i = 0; i < _atlasInfo.Count; ++i)
            {
                var info = _atlasInfo[i];
                if (_atlasNameToFolder.ContainsKey(info.atlasName))
                {
                    Log.Error("SpriteAtlasSettings CheckInit error: duplicate key=" + info.atlasName + " path=" + resourcesLoadPath);
                    continue;
                }
                _atlasNameToFolder.Add(info.atlasName, info.atlasFolder);
            }
        }
    }
}
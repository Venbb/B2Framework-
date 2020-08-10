using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    /// <summary>
    /// excel附加参数的配置文件
    /// </summary>
    public class ExcelDataSettingUnity : ScriptableObject, shaco.Base.IExcelSetting, shaco.Base.IGameInstanceCreator
    {
        [System.Serializable]
        public class ExcelSettingInfo
        {
            [System.Serializable]
            public class ExcelDataInfo
            {
                public string guid = string.Empty;

                //excel路径无逻辑意义，仅仅是为了方便识别配置的是哪个文件
                public string path
                {
                    get
                    {
#if UNITY_EDITOR
                        _path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
#endif
                        return _path;
                    }
                    set
                    {
                        _path = value;
#if UNITY_EDITOR
                        guid = UnityEditor.AssetDatabase.AssetPathToGUID(value);
#endif
                    }
                }

                private string _path = string.Empty;

                //数据类型 
                public shaco.Base.ExcelData.TabelDataType dataType = shaco.Base.ExcelData.TabelDataType.List;

                //获取线程安全的路径
                public bool EqualsPath(string other)
                {
                    return _path == other;
                }
            }

            //自定义导出文本路径
            public string customExportTextPath = string.Empty;
            //自定义导出脚本路径
            public string customExportScriptPath = string.Empty;
            //自定义导出asset资源目录
            public string customExportAssetPath = string.Empty;
            //自定义代码命名空间
            public string customNamespace = string.Empty;
            //默认的配置类型
            public shaco.Base.ExcelData.TabelDataType defalutDataType = shaco.Base.ExcelData.TabelDataType.List;

            //一组excel数据配置
            public List<ExcelDataInfo> excelDatasInfo = new List<ExcelDataInfo>();
        }

        /// <summary> 
        /// 字符串类型对应的c#类型表
        /// </summary>
        public List<ExcelSettingInfo> settingsInfo
        {
            get
            {
                CheckInit();
                return _settingsInfo;
            }
        }
        [SerializeField]
        private List<ExcelSettingInfo> _settingsInfo = null;

        //运行时刻数据加载类型
        public shaco.Base.ExcelData.RuntimeLoadFileType runtimeLoadFileType
        {
            get
            {
                _runtimeLoadFileType = shaco.Base.GameHelper.gameConfig.ReadEnum(_runtimeLoadFileType.ToTypeString(), shaco.Base.ExcelData.RuntimeLoadFileType.UnityAsset);
                return _runtimeLoadFileType;
            }
            set
            {
                _runtimeLoadFileType = value;
                shaco.Base.GameHelper.gameConfig.WriteEnum(_runtimeLoadFileType.ToTypeString(), _runtimeLoadFileType);
                shaco.Base.GameHelper.gameConfig.CheckSaveModifyData();
            }
        }
        [SerializeField]
        private shaco.Base.ExcelData.RuntimeLoadFileType _runtimeLoadFileType = shaco.Base.ExcelData.RuntimeLoadFileType.UnityAsset;

        //excel数据加载类，支持自定义
        public shaco.Base.IExcelDataLoader dataLoader
        {
            get
            {
                CheckInit();
                if (null == _dataLoader && !string.IsNullOrEmpty(_dataLoaderTypeName))
                    _dataLoader = shaco.Base.Utility.Instantiate(_dataLoaderTypeName) as shaco.Base.IExcelDataLoader;
                return _dataLoader;
            }
            set
            {
                _dataLoader = value;
                _dataLoaderTypeName = null == value ? string.Empty : _dataLoader.ToTypeString();
            }
        }
        [SerializeField]
        private string _dataLoaderTypeName = string.Empty;
        private shaco.Base.IExcelDataLoader _dataLoader = null;

        /// <summary> 
        /// 默认excel表配置
        /// </summary>
        private readonly ExcelSettingInfo _defaultSettingInfo = new ExcelSettingInfo();
        private readonly ExcelSettingInfo.ExcelDataInfo _defaultExcelDataInfo = new ExcelSettingInfo.ExcelDataInfo();

        /// <summary>
        /// 替换new
        /// </summary>
        static public object Create()
        {
            return ScriptableObject.CreateInstance<ExcelDataSettingUnity>();
        }

        /// <summary>
        /// 获取表格式类型
        /// <param name="path">excel文件路径</param>
        /// <return>格式类型</return>
        /// </summary>
        public shaco.Base.ExcelData.TabelDataType GetDataType(string path)
        {
            var dataInfo = GetExcelDataInfo(path);
            return dataInfo.dataType;
        }

        /// <summary>
        /// 获取自定义加载相对路径
        /// <param name="path">excel文件路径</param>
        /// <return>加载相对路径</return>
        /// </summary>
        public string GetCustomLoadPath(string path)
        {
            var settingsInfo = GetSettingInfo(path);
            var retValue = string.Empty;
            switch (this.runtimeLoadFileType)
            {
                case shaco.Base.ExcelData.RuntimeLoadFileType.CSV: retValue = settingsInfo.customExportTextPath; break;
                case shaco.Base.ExcelData.RuntimeLoadFileType.UnityAsset: retValue = settingsInfo.customExportAssetPath; break;
                default: Log.Error("ExcelDataSettingUnity GetCustomLoadPath error: unsupport load type=" + this.runtimeLoadFileType); retValue = settingsInfo.customExportAssetPath; break;
            }
            return retValue;
        }

        /// <summary>
        /// 获取自定义导出文本相对路径
        /// <param name="path">excel文件路径</param>
        /// <return>文本相对路径</return>
        /// </summary>
        public string GetCustomExportTextPath(string path)
        {
            var retValue = GetSettingInfo(path).customExportTextPath;
            retValue = UnityHelper.UnityPathToFullPath(retValue);
            return retValue;
        }

        /// <summary>
        /// 获取自定义导出脚本相对路径
        /// <param name="path">excel文件路径</param>
        /// <return>脚本相对路径</return>
        /// </summary>
        public string GetCustomExportScriptPath(string path)
        {
            var retValue = GetSettingInfo(path).customExportScriptPath;
            retValue = UnityHelper.UnityPathToFullPath(retValue);
            return retValue;
        }

        /// <summary>
        /// 获取自定义导出asset相对路径
        /// <param name="path">excel文件路径</param>
        /// <return>asset相对路径</return>
        /// </summary>
        public string GetCustomExportAssetPath(string path)
        {
            var retValue = GetSettingInfo(path).customExportAssetPath;

            if (string.IsNullOrEmpty(retValue))
                retValue = Application.dataPath;
            retValue = UnityHelper.UnityPathToFullPath(retValue);
            return retValue;
        }

        /// <summary>
        /// 获取自定义命名空间
        /// <param name="path">excel文件路径</param>
        /// <return>命名空间</return>
        /// </summary>
        public string GetCustomNamespace(string path)
        {
            return GetSettingInfo(path).customNamespace;
        }

        /// <summary>
        /// 获取所有文本文件导出目录
        /// </summary>
        public string[] GetAllTextExportPath()
        {
            CheckInit();
            var retValue = new string[_settingsInfo.Count];
            for (int i = 0; i < _settingsInfo.Count; ++i)
            {
                retValue[i] = _settingsInfo[i].customExportTextPath;
            }
            return retValue;
        }

        /// <summary>
        /// 根据文件名字获取对应的excel表配置信息
        /// <param name="path">excel文件路径</param>
        /// <return>如果获取失败返回默认配置</return>
        /// </summary>
        private ExcelSettingInfo.ExcelDataInfo GetExcelDataInfo(string path)
        {
            CheckInit();

            if (string.IsNullOrEmpty(path))
                return _defaultExcelDataInfo;

            ExcelSettingInfo.ExcelDataInfo retValue = null;
            ExcelSettingInfo findInfo = null;
            this._settingsInfo.Find(v =>
            {
                var folderName = shaco.Base.FileHelper.GetFolderNameByPath(path).RemoveLastFlag();
                if (folderName == v.customExportTextPath)
                {
                    findInfo = v;
                    retValue = findInfo.excelDatasInfo.Find(v2 => v2.EqualsPath(path));
                    return true;
                }
                else
                    return false;
            });

            //使用找到的默认数据类型
            if (null == retValue && null != findInfo)
            {
                _defaultExcelDataInfo.dataType = findInfo.defalutDataType;
            }
            return null != retValue ? retValue : _defaultExcelDataInfo;
        }

        /// <summary>
        /// 根据文件名字获取对应的excel表配置信息
        /// <param name="path">excel文件路径</param>
        /// <return>如果获取失败返回默认配置</return>
        /// </summary>
        private ExcelSettingInfo GetSettingInfo(string path)
        {
            CheckInit();

            if (string.IsNullOrEmpty(path))
                return _defaultSettingInfo;

            ExcelSettingInfo retValue = null;
            var folderName = shaco.Base.FileHelper.GetFolderNameByPath(path).RemoveLastFlag();
            this._settingsInfo.Find(v =>
            {
                //不能直接用==判断，可能folderName是全路径，而customXXPath是相对路径，所以要用EndsWith判断
                if (folderName.EndsWith(v.customExportTextPath) || folderName.EndsWith(v.customExportScriptPath) || folderName.EndsWith(v.customExportAssetPath))
                {
                    retValue = v;
                    return true;
                }
                else
                    return false;
            });

            if (null == retValue)
            {
                Log.Error("ExcelDataSettingUnity GetSettingInfo error: Configuration not found, default configuration will be used instead, path=" + path);
                return _defaultSettingInfo;
            }

            return retValue;
        }

        /// <summary>
        /// 检查数据初始化
        /// </summary>
        private void CheckInit()
        {
            if (null == _settingsInfo)
            {
                //读取动态配置
                var typeName = this.GetType().Name;
                if (shaco.GameHelper.res.ExistsResourcesOrLocal(typeName))
                {
                    var loadScript = shaco.GameHelper.res.LoadResourcesOrLocal<ExcelDataSettingUnity>(typeName);
                    if (null != loadScript)
                    {
                        this._settingsInfo = loadScript._settingsInfo;
                        this._dataLoaderTypeName = loadScript._dataLoaderTypeName;
                        this._runtimeLoadFileType = loadScript._runtimeLoadFileType;
                    }
                    else
                    {
                        Log.Error("ExcelDataSettingUnity CheckInit erorr: not found 'DataTypesTemplate' ");
                    }
                }
                else
                {
                    Log.Error("ExcelDataSettingUnity CheckInit erorr: not found 'ExcelDataSettingUnity.asset' ");
                }
            }

            //读取动态资源失败，使用空配置
            if (null == _settingsInfo)
            {
                _settingsInfo = new List<ExcelSettingInfo>() { new ExcelSettingInfo() };
            }
        }
    }
}
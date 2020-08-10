using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    /// <summary>
    /// 自定义excel数据加载类
    /// </summary>
    public class ExcelDataLoaderDefault : shaco.Base.IExcelDataLoader
    {
        private string _multiVersionControlRelativePath = string.Empty;
        private string _prefixPath = string.Empty;

        virtual public shaco.Base.ExcelData Load(string path)
        {
            shaco.Base.ExcelData retValue = null;
            path = GetCurrentEnviromentExcelPath(path);
            var readTmp = shaco.GameHelper.res.LoadResourcesOrLocal<UnityEngine.Object>(path, _multiVersionControlRelativePath);
            if (null != readTmp)
            {
                retValue = LoadExcelDataFromResource(readTmp, path);
            }
            return retValue;
        }

        virtual public void LoadAsync(string path, System.Action<shaco.Base.ExcelData> callbackEnd, System.Action<float> callbackProgress = null)
        {
            if (null == callbackEnd)
            {
                Log.Error("ExcelHelper OpenResourcesOrLocalAsync error: callback function is invalid");
                return;
            }

            shaco.Base.ExcelData retValue = null;

            path = GetCurrentEnviromentExcelPath(path);
            shaco.GameHelper.res.LoadResourcesOrLocalAsync<UnityEngine.Object>(path, (obj) =>
            {
                if (null != obj)
                {
                    retValue = LoadExcelDataFromResource(obj, path);

                    if (null != retValue && null != callbackEnd)
                        callbackEnd(retValue);
                }
                else
                {
                    if (null != callbackEnd)
                        callbackEnd(null);
                }
            }, callbackProgress, _multiVersionControlRelativePath);
        }

        virtual public void UnLoad(shaco.Base.ExcelData data, string path)
        {
            path = GetCurrentEnviromentExcelPath(path);
            shaco.GameHelper.resCache.UnloadAssetBundle(path, true, _multiVersionControlRelativePath);
            if (data.GetTabelDataRowCount() > 1000)
            {
                data.Clear();
                System.GC.Collect();
            }
        }

        /// <summary>
        /// 设置全局excel资源目录前缀
        /// <param name="prefix">前缀路径</param>
        /// </summary>
        virtual public void SetPrefixPath(string prefix)
        {
            _prefixPath = prefix;
        }

        /// <summary>
        /// 设置全局excel资源版本
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        virtual public void SetMultiVersionControlRelativePath(string multiVersionControlRelativePath)
        {
            _multiVersionControlRelativePath = multiVersionControlRelativePath;
        }

        virtual protected shaco.Base.ExcelData LoadExcelDataFromResource(UnityEngine.Object readTmp, string pathTag)
        {
            shaco.Base.ExcelData retValue = null;

            //如果是新版asset文件，则从asset文件来初始化excelData数据
            if (readTmp.GetType() == typeof(shaco.ExcelDefaultAsset))
            {
                retValue = new shaco.Base.ExcelData();
                retValue.InitWithRowDatas(((shaco.ExcelDefaultAsset)readTmp).datas, pathTag);
            }
            else
            {
                //从csv文件初始化数据
                var readBytes = readTmp.ToBytes();
                if (!readBytes.IsNullOrEmpty())
                {
                    retValue = new shaco.Base.ExcelData();
                    retValue.InitWithBinary(readBytes, pathTag);
                }
                else
                {
                    Log.Error("ExcelDataLoaderDefault LoadExcelDataFromResource erorr: no data, path=" + pathTag);
                }
            }
            return retValue;
        }

        /// <summary>
        /// 获取当前环境文件读取路径
        /// </summary>
        private string GetCurrentEnviromentExcelPath(string path)
        {
            var retValue = new System.Text.StringBuilder();
            var runtimeLoadFileExtension = string.Empty;
            var runtimeLoadFileType = shaco.Base.GameHelper.excelSetting.runtimeLoadFileType;

            //全局设定路径
            if (!string.IsNullOrEmpty(_prefixPath))
            {
                retValue.Append(_prefixPath);
                if (!_prefixPath.EndsWith(System.IO.Path.DirectorySeparatorChar))
                    retValue.Append(System.IO.Path.DirectorySeparatorChar);
            }

            //读取文件夹路径
            var customResourcePath = shaco.Base.GameHelper.excelSetting.GetCustomLoadPath(path);
            if (!string.IsNullOrEmpty(customResourcePath))
            {
                retValue.Append(customResourcePath);
                retValue.Append(System.IO.Path.DirectorySeparatorChar);
            }
            else
            {
                Log.Warning("ExcelHelper+Unity GetCurrentEnviromentExcelPath warning: custom load path is empty, excel path=" + path);
            }

            //后缀名
            switch (runtimeLoadFileType)
            {
                case shaco.Base.ExcelData.RuntimeLoadFileType.CSV: runtimeLoadFileExtension = shaco.Base.ExcelDefine.EXTENSION_TXT; break;
                case shaco.Base.ExcelData.RuntimeLoadFileType.UnityAsset: runtimeLoadFileExtension = shaco.Base.ExcelDefine.EXTENSION_ASSET; break;
                default: Log.Error("ExcelDataSerializable SerializableAsCSharpScript error: unsupport load type=" + runtimeLoadFileType); break;
            }

            var fileName = System.IO.Path.GetFileName(path);
            fileName = System.IO.Path.ChangeExtension(fileName, runtimeLoadFileExtension);
            retValue.Append(fileName);
            return retValue.ToString();
        }
    }
}
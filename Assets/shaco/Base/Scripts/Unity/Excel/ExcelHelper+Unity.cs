using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public partial class ExcelHelper
    {
        //webgl配置表异步加载遍历速度
        //因为webgl无法使用多线程，所以采用的协程加载，如参数0.4f表示同一帧遍历40%的数据表内容
#if UNITY_WEBGL
        public const float DEFAULT_WEBGL_LOAD_ASYNC_SPEED = 0.4f;
#endif

        /// <summary>
        /// 从本地热更新下载目录打开excel，仅支持excel导出的txt或者csv格式
        /// <param name="path">excel路径</param>
        /// <return>表字典，用于读写excel数据</return>
        /// </summary>
        static public shaco.Base.ExcelData OpenResourcesOrLocal(string path)
        {
            var dataLoader = shaco.GameHelper.excelSetting.dataLoader;
            if (null == dataLoader)
            {
                Log.Exception("ExcelHelper+Unity OpenResourcesOrLocal exception: dataLoader is null");
                return null;
            }

            return dataLoader.Load(path);
        }

        static public void CloseResourcesOrLocal(shaco.Base.ExcelData data, string path)
        {
            var dataLoader = shaco.GameHelper.excelSetting.dataLoader;
            if (null == dataLoader)
            {
                Log.Exception("ExcelHelper+Unity CloseResourcesOrLocal exception: dataLoader is null");
                return;
            }

            dataLoader.UnLoad(data, path);
        }

        /// <summary>
        /// 从Resources目录或者本地下载目录异步打开excel表，仅支持excel导出的txt或者csv格式
        /// <param name="path">excel路径</param>
        /// <param name="callbackEnd">打开excel完毕回调</param>
        /// <return>表字典，用于读写excel数据</return>
        /// </summary>
        static public void OpenResourcesOrLocalAsync(string path, System.Action<shaco.Base.ExcelData> callbackEnd, System.Action<float> callbackProgress = null)
        {
            var dataLoader = shaco.GameHelper.excelSetting.dataLoader;
            if (null == dataLoader)
            {
                Log.Exception("ExcelHelper+Unity OpenResourcesOrLocalAsync exception: dataLoader is null");
                return;
            }
            dataLoader.LoadAsync(path, callbackEnd, callbackProgress);
        }
    }
}
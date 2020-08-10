using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 自定义excel数据加载类
    /// </summary>
    public interface IExcelDataLoader
    {
        /// <summary>
        /// 同步加载数据
        /// <param name="path">文件路径</param>
        /// <return>数据，如果为null表示加载失败</return>
        /// </summary>
        ExcelData Load(string path);

        /// <summary>
        /// 异步加载数据
        /// <param name="path">文件路径</param>
        /// <param name="callbackEnd">加载完毕回调</param>
        /// <param name="callbackProgress">加载进度回调，可以为空</param>
        /// </summary>
        void LoadAsync(string path, System.Action<ExcelData> callbackEnd, System.Action<float> callbackProgress = null);

        void UnLoad(ExcelData data, string path);

        /// <summary>
        /// 设置全局excel资源目录前缀
        /// <param name="prefix">前缀路径</param>
        /// </summary>
        void SetPrefixPath(string prefix);

        /// <summary>
        /// 设置全局excel资源版本
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        void SetMultiVersionControlRelativePath(string multiVersionControlRelativePath);
    }
}
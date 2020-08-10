using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// excel附加参数的配置文件
    /// </summary>
    public interface IExcelSetting : IGameInstance
    {
        ExcelData.RuntimeLoadFileType runtimeLoadFileType { get; set; }

        //excel数据加载类，支持自定义
        shaco.Base.IExcelDataLoader dataLoader { get; set; }

        /// <summary>
        /// 获取表格式类型
        /// <param name="path">excel文件名字</param>
        /// <return>格式类型</return>
        /// </summary>
        ExcelData.TabelDataType GetDataType(string path);

        /// <summary>
        /// 获取自定义加载相对路径
        /// <param name="path">excel文件名字</param>
        /// <return>加载相对路径</return>
        /// </summary>
        string GetCustomLoadPath(string path);

        /// <summary>
        /// 获取自定义导出文本相对路径
        /// <param name="path">excel文件名字</param>
        /// <return>文本相对路径</return>
        /// </summary>
        string GetCustomExportTextPath(string path);

        /// <summary>
        /// 获取自定义导出asset相对路径
        /// <param name="path">excel文件名字</param>
        /// <return>asset相对路径</return>
        /// </summary>
        string GetCustomExportAssetPath(string patpathh);

        /// <summary>
        /// 获取自定义导出脚本相对路径
        /// <param name="path">excel文件名字</param>
        /// <return>脚本相对路径</return>
        /// </summary>
        string GetCustomExportScriptPath(string path);

        /// <summary>
        /// 获取自定义命名空间
        /// <param name="path">excel文件名字</param>
        /// <return>命名空间</return>
        /// </summary>
        string GetCustomNamespace(string path);

        /// <summary>
        /// 获取所有文本文件导出目录
        /// </summary>
        string[] GetAllTextExportPath();
    }
}
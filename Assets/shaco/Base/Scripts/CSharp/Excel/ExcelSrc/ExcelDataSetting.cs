using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// excel附加参数的配置文件
    /// </summary>
    public class ExcelDataSetting : IExcelSetting
    {
        //运行时刻数据加载类型
        public shaco.Base.ExcelData.RuntimeLoadFileType runtimeLoadFileType
        {
            get { return shaco.Base.ExcelData.RuntimeLoadFileType.CSV; }
            set { }
        }

        //excel数据加载类，支持自定义
        public shaco.Base.IExcelDataLoader dataLoader { get; set; }

        /// <summary>
        /// 获取表格式类型
        /// <param name="fileName">excel文件名字</param>
        /// <return>格式类型</return>
        /// </summary>
        public ExcelData.TabelDataType GetDataType(string fileName)
        {
            return ExcelData.TabelDataType.List;
        }

        /// <summary>
        /// 获取自定义加载相对路径
        /// <param name="fileName">excel文件名字</param>
        /// <return>加载相对路径</return>
        /// </summary>
        public string GetCustomLoadPath(string fileName)
        {
            return string.Empty;
        }

        /// <summary>
        /// 获取自定义导出文本相对路径
        /// <param name="fileName">excel文件名字</param>
        /// <return>文本相对路径</return>
        /// </summary>
        public string GetCustomExportTextPath(string fileName)
        {
            return string.Empty;
        }

        /// <summary>
        /// 获取自定义导出脚本相对路径
        /// <param name="fileName">excel文件名字</param>
        /// <return>脚本相对路径</return>
        /// </summary>
        public string GetCustomExportScriptPath(string fileName)
        {
            return string.Empty;
        }

        /// <summary>
        /// 获取自定义命名空间
        /// <param name="fileName">excel文件名字</param>
        /// <return>命名空间</return>
        /// </summary>
        public string GetCustomNamespace(string fileName)
        {
            return string.Empty;
        }

        public string GetCustomExportAssetPath(string path)
        {
            return string.Empty;
        }

        /// <summary>
        /// 获取所有文本文件导出目录
        /// </summary>
        public string[] GetAllTextExportPath()
        {
            return null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System;

namespace shaco.Base
{
    public partial class ExcelHelper
    {
        private Dictionary<string, IExcelData> _excelDatas = new Dictionary<string, IExcelData>();

        /// <summary>
        /// 获取excel数据管理类，用于获取excel数据
        /// <param name="T">数据类类型</param>
        /// <return>excel数据类</return>
        /// </summary>
        static public T GetExcelData<T>() where T : IExcelData, new()
        {
            var instanceTmp = GameEntry.GetInstance<ExcelHelper>();
            string key = typeof(T).FullName.ToString();
            T retValue = default(T);
            IExcelData findValue = null;

            if (instanceTmp._excelDatas.TryGetValue(key, out findValue))
            {
                retValue = (T)findValue;
            }
            else 
            {
                retValue = new T();
                instanceTmp._excelDatas.Add(key, retValue);
            }
           
            return retValue;
        }

        /// <summary>
        /// 销毁一个excel数据类
        /// <param name="T">数据类类型</param>
        /// </summary>
        static public void UnloadExcelData<T>()
        {
            var instanceTmp = GameEntry.GetInstance<ExcelHelper>();
            string key = typeof(T).FullName.ToString();

            if (!instanceTmp._excelDatas.ContainsKey(key))
            {
                Log.Error("ExcelHelper UnloadExcelData error: not found data, key=" + key);
            }
            else 
            {
                instanceTmp._excelDatas.Remove(key);
            }
        }

        /// <summary>
        /// 清理所有excel数据类，释放内存
        /// </summary>
        static public void ClearExcelData()
        {
            var instanceTmp = GameEntry.GetInstance<ExcelHelper>();
            instanceTmp._excelDatas.Clear();
        }

        /// <summary>
        /// 判断是否为excel文件
        /// </summary>
        static public bool IsExcelFile(string path)
        {
            if (!shaco.Base.FileHelper.ExistsFile(path))
                return false;

            var extensions = FileHelper.GetFilNameExtension(path);
            return "xlsx" == extensions || "xls" == extensions || "csv" == extensions;
        }
    }
}


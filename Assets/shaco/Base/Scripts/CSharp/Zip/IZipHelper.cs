using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public interface IZipHelper : IGameInstance
    {
        /// <summary>
		/// 解压过程中需要忽略的文件名标记
		/// </summary>
        string[] ignoreFiles { get; set; }

        /// <summary>   
        /// 压缩文件或文件夹
        /// </summary>   
        /// <param name="fileToZip">要压缩的路径，可以是文件夹或者文件</param>   
        /// <param name="zipedFile">压缩后的文件名</param>   
        /// <param name="password">密码，可以为空则不需要密码</param>
        /// <returns>如果失败返回失败信息，成功返回空字符串</returns>   
        string Zip(string fileToZip, string zipedFile, string password = null);

        /// <summary>   
        /// 解压功能(解压压缩文件到指定目录)
        /// </summary>   
        /// <param name="fileToUnZip">待解压的文件或者文件夹</param>   
        /// <param name="zipedFolder">指定解压目标目录</param>
        /// <param name="password">密码，可以为空则不需要密码</param> 
        /// <returns>如果失败返回失败信息，成功返回空字符串</returns>   
        string UnZip(string fileToUnZip, string zipedFolder, string password = null, System.Action<float> callbackProgress = null);
    }	
}
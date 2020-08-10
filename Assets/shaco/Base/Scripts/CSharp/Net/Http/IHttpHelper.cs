using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class HttpComponent
    {
        public string key = string.Empty;
        public string value = string.Empty;

        public HttpComponent() { }

        public HttpComponent(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public interface IHttpHelper : IGameInstance
    {
        //超时时间
        double timeoutSeconds { get; set; }

        //超时后重试次数
        short timeoutRetryTimes { get; set; }

        /// <summary>
        /// 清理占用的内存资源
        /// </summary>
        void UnloadUnusedMemory();

        /// <summary>
        /// 下载
        /// <param name="url">下载地址</param>
        /// </summary>
        void Download(string url);

        // /// <summary>
        // /// 上传
        // /// </summary>
        // /// <param name="url">下载地址</param>
        // /// <param name="header">资源头</param>
        // /// <param name="body">资源体</param>
        // void UploadForm(string url, HttpComponent[] header, HttpComponent[] body);

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="header">资源头</param>
        /// <param name="body">资源体</param>
        void Upload(string url, HttpComponent[] header, HttpComponent[] body);

        /// <summary>
        /// 获取下载完成了资源，如果还在下载中则返回null
        /// </summary>
        byte[] GetDownloadByte();
        
        /// <summary>
        /// 当前上传或者下载是否完成
        /// </summary>
        bool IsSuccess();

        /// <summary>
        /// 是否下载完毕，可能成功或者失败
        /// </summary>
        bool IsDone();

        /// <summary>
        /// 获取当前下载进度(范围:0~1)
        /// </summary>
        float GetDownloadProgress();

        /// <summary>
        /// 获取当前上传进度(范围:0~1)
        /// </summary>
        float GetUploadProgress();

        /// <summary>
        /// 获取当前下载或上传速度(单位: kb)
        /// </summary>
        long GetDownloadSpeed();

        //重置网络下载速度
        void ResetDownloadSpeed();

		/// <summary>
		/// 获取从下载开始到现在已经经过的时间(单位：秒)
		/// </summary>
        double GetEplaseTimeSeconds();

        /// <summary>
        /// 获取当前一次循环下载的数据大小
        /// </summary>
        long GetCurrentDownloadSize();

        //获取最新一次网络错误信息
        string GetLastError();

        //网址错误，没有找到对应地址
        bool IsNotFound404();

        //被服务器拒绝访问，可能是权限或者网址有误
        bool IsForbidden403();

		/// <summary>
		/// 下载是否发生错误
		/// </summary>
        bool HasError();

		/// <summary>
		/// 关闭下载请求
		/// </summary>
        void CloseClient();

        /// <summary>
		/// 开启并设定下载完毕后自动保存的文件路径
		/// <param name="pathSave">保存文件路径</param>
		/// </summary>
        void SetAutoSaveWhenCompleted(string pathSave);

        /// <summary>
		/// 关闭下载完成后自动保存文件功能
		/// </summary>
        void CloseAutoSaveWhenCompleted();
    }
}
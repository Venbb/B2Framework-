using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public interface IHotUpdateImportWWW : shaco.Base.IGameInstance
    {
        //检查需要的下载的版本完毕回掉
        shaco.Base.EventCallBack onCheckVersionEndCallBack { get; }
        //下载完毕单个文件回调<key：下载到本地的绝对路径>
        shaco.Base.EventCallBack<string> onDownloadedOnceCallBack { get; }
		//更新中回调
        shaco.Base.EventCallBack onUpdatingCallBack { get; }
		//更新完毕回调
        shaco.Base.EventCallBack onUpdateEndCallBack { get; }

        //超时时间
        double timeoutSeconds { get; set; }
        //超时后重试次数
        short timeoutRetryTimes { get; set; }
        //刷新下载速度的间隔时间
        float updateDownloadSpeedTime { get; set; }
        //同一时间下载队列中http请求数量
        int downloadSqueueCount { get; set; }

        /// <summary>
        /// 检查资源更新
        /// <param name="urlVersion">服务器资源根目录下载地址(例如VersionControl@@Android文件夹所在地址)</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        void CheckUpdate(string urlVersion, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

        /// <summary>
        /// 检查资源更新
        /// <param name="urlVersion">服务器资源根目录下载地址(例如VersionControl@@Android文件夹所在地址)</param>
        /// <param name="mainMD5">主md5，如果没有设定该值，则会从服务器更新md5</param>
        /// <param name="filterPrefixPaths">下载文件筛选文件列表，如果为空则会下载所有资源</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        void CheckUpdate(string urlVersion, string mainMD5, string[] filterPrefixPaths, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

        /// <summary>
        /// 检查资源更新
        /// <param name="urlVersion">服务器资源根目录下载地址(例如VersionControl@@Android文件夹所在地址)</param>
        /// <param name="packageVersion">安装包版本号，例如1.0.0，如果填写了该字段，则在无网情况下对本地资源做版本管理检测，如果本地资源版本低于服务器资源则要求联网更新</param>
        /// <param name="mainMD5">主md5，如果没有设定该值，则会从服务器更新md5</param>
        /// <param name="filterPrefixPaths">下载文件筛选文件列表，如果为空则会下载所有资源</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        void CheckUpdate(string urlVersion, string packageVersion, string mainMD5, string[] filterPrefixPaths, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

        /// <summary>
		/// 判断当前是否包含该下载状态
		/// <param name="status">需要被判断的下载状态</param>
		/// <return>false无该状态，true有该状态</return>
		/// </summary>
        bool HasStatus(HotUpdateDownloadStatus.Status status);

        /// <summary>
        /// 是否需要更新资源
        /// </summary>
        /// <returns>false不需要更新资源，true需要更新资源</returns>
        bool IsNeedUpdate();

        /// <summary>
        /// 重置所有回调
        /// </summary>
        void ResetAllCallBack();

        /// <summary>
		/// 是否下载完毕且没有发生错误
		/// <return>true下载成功，false下载失败，可以调用GetLastError获取具体报错信息</return>
		/// </summary>
        bool IsSuccess();

        //获取状态信息描述文本
        string GetStatusDescription();

        /// <summary>
        /// 获取当前已下载数据大小
        /// </summary>
        long GetDownloadedDataSize();

        /// <summary>
        /// 获取需要下载的总数据大小
        /// </summary>
        long GetTotalDownloadDataSize();

        /// <summary>
        /// 获取本次更新需要下载的总数据大小
        /// </summary>
        long GetCurrentNeedUpdateDataSize();

        //获取当前下载速度(单位: kb)
        long GetDownloadSpeed();

        /// <summary>
        /// 获取格式化后的下载速度字符串
        /// </summary>
        /// <returns>下载速度字符串</returns>
        string GetDownloadSpeedFormatString();

        /// <summary>
		/// 获取资源版本号
		/// </summary>
        string GetVersion();

        /// <summary>
        /// 获取当前下载成功完成文件数量
        /// </summary>
        int GetDownloadedCount();

        /// <summary>
        /// 获取总共需要下载的文件数量
        /// </summary>
        int GetTotalDownloadCount();

        /// <summary>
        /// 获取下载进度，范围0 ~ 1
        /// </summary>
        float GetDownloadResourceProgress();

        /// <summary>
        /// 获取更新发生错误具体内容
        /// </summary>
        string GetLastError();

        /// <summary>
        /// 是否更新发生了错误
        /// </summary>
        bool HasError();
    }
}
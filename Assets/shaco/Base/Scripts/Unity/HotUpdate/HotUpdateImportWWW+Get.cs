using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public partial class HotUpdateImportWWW : HotUpdateDownloader, IHotUpdateImportWWW
    {
        //获取状态信息描述文本
        public string GetStatusDescription()
        {
            return _updateStatus.GetStatusDescription();
        }

        /// <summary>
        /// 获取当前已下载数据大小
        /// </summary>
        public long GetDownloadedDataSize()
        {
            return _currentDownloadedDataSize;
        }

        /// <summary>
        /// 获取需要下载的总数据大小
        /// </summary>
        public long GetTotalDownloadDataSize()
        {
            return _versionControlClient.TotalDataSize;
        }

        /// <summary>
        /// 获取本次更新需要下载的总数据大小
        /// </summary>
        public long GetCurrentNeedUpdateDataSize()
        {
            return _currentNeedUpdateDataSize;
        }

        //获取当前下载速度(单位: kb)
        public long GetDownloadSpeed()
        {
            return _iDownloadSpeed;
        }

        /// <summary>
        /// 获取格式化后的下载速度字符串
        /// </summary>
        /// <returns>The download speed format string.</returns>
        public string GetDownloadSpeedFormatString()
        {
            return shaco.Base.Utility.GetSpeedFormatString(GetDownloadSpeed(), 2);
        }

        //获取资源版本号
        public string GetVersion()
        {
            return _versionControlClient.Version;
        }

        //获取当前下载成功完成文件数量
        public int GetDownloadedCount()
        {
            return _iCurrentDownloadCount;
        }

        //获取总共需要下载的文件数量
        public int GetTotalDownloadCount()
        {
            return _iTotalDownloadCount;
        }

        /// <summary>
        /// 获取更新发生错误具体内容
        /// </summary>
        public override string GetLastError()
        {
            return base.GetLastError();
        }

        /// <summary>
        /// 是否更新发生了错误
        /// </summary>
        public override bool HasError()
        {
            return base.HasError()
                || _updateStatus.HasStatus(HotUpdateDownloadStatus.Status.HasError)
                || _updateStatus.HasStatus(HotUpdateDownloadStatus.Status.ErrorNeedUpdateResourceWithNetWork)
                || _updateStatus.HasStatus(HotUpdateDownloadStatus.Status.ErrorNotFoundResource);
        }
	}
}

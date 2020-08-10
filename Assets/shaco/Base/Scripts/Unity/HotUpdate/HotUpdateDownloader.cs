using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class HotUpdateDownloader : HotUpdateImport, shaco.Base.IObjectPoolData
    {
        //超时计时器
        protected shaco.ActionBase _actionCheckTimeout = null;

        //http请求对象
        protected shaco.Base.IHttpHelper _httpHelper = null;

        //下载中回调
        public shaco.Base.EventCallBack onDownloadingCallBack = null;

        //下载完毕回调
        public shaco.Base.EventCallBack onDownloadEndCalback = null;

        public HotUpdateDownloader()
        {
            _httpHelper = (shaco.Base.IHttpHelper)shaco.Base.GameHelper.http.GetType().Instantiate();
            onDownloadingCallBack = new shaco.Base.EventCallBack();
            onDownloadEndCalback = new shaco.Base.EventCallBack();
        }

        new public void Dispose()
        {
            base.Dispose();
            _actionCheckTimeout = null;

            if (null != _httpHelper)
            {
                _httpHelper.CloseClient();
            }
            onDownloadingCallBack.ClearCallBack();
            onDownloadEndCalback.ClearCallBack();
        }

        /// <summary>
        /// 获取下载进度，范围0 ~ 1
        /// </summary>
        public float GetDownloadResourceProgress()
        {
            return _fCurrentProgress;
        }

        /// <summary>
        /// 获取下载委托对象
        /// </summary>
        public shaco.Base.IHttpHelper GetHttpHelper()
        {
            return _httpHelper;
        }

        /// <summary>
        /// 通过http下载文件
        /// <param name="url">请求地址</param>
        /// <return></return>
        /// </summary>
        public void DownloadByHttp(string url)
        {
            if (null != _actionCheckTimeout)
            {
                _actionCheckTimeout.StopMe();
                _actionCheckTimeout = null;
            }

            _httpHelper.Download(url);
            _actionCheckTimeout = shaco.WaitFor.Run(() =>
                                              {
                                                  _fCurrentProgress = _httpHelper.GetDownloadProgress();

                                                  onDownloadingCallBack.InvokeAllCallBack();
                                                  if (_httpHelper.IsSuccess() || _httpHelper.HasError())
                                                      return true;
                                                  else
                                                      return false;
                                              },
            () =>
            {
                _fCurrentProgress = 1;
                OnError(_httpHelper.GetLastError());
                onDownloadingCallBack.InvokeAllCallBack();
                onDownloadEndCalback.InvokeAllCallBack();
                OnCompleted();
            });
        }
    }
}


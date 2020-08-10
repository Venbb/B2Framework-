using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class HttpDownLoader : IHttpDownLoader
    {
        //等待动作对象
        private shaco.ActionBase _actionWaitFor = null;

		//下载器
		private shaco.Base.IHttpHelper _http = null;

        //下载完毕回调
        private System.Action<byte[], string> _callbackEnd = null;

        /// <summary>
        /// 开始下载
        /// <param name="url">地址</param>
        /// <param name="callbackProgress">下载进度回调</param>
        /// <param name="callbackEnd">下载完毕回调</param>
        /// </summary>
        override public IHttpDownLoader Start(string url, System.Action<float> callbackProgress, System.Action<byte[], string> callbackEnd)
        {
            var retValue = new HttpDownLoader();
            retValue.StartBase(url, callbackProgress, callbackEnd);
            return (IHttpDownLoader)retValue;
        }

        /// <summary>
        /// 额外添加结束回调方法
        /// </summary>
        override public void AddEndCallBack(System.Action<byte[], string> callbackEnd)
        {
            _callbackEnd -= callbackEnd;
            _callbackEnd += callbackEnd;
        }

        /// <summary>
        /// 强制中断下载
        /// </summary>
        override public void Stop()
		{
			if (null != _actionWaitFor)
			{
                _actionWaitFor.StopMe();
                _actionWaitFor = null;
			}

			if (null != _http)
			{
                _http.CloseClient();
                _http.RecyclingWithPool();
                _http = null;
			}
		}

        /// <summary>
        /// 开始下载
        /// <param name="url">地址</param>
        /// <param name="callbackProgress">下载进度回调</param>
        /// <param name="callbackEnd">下载完毕回调</param>
        /// </summary>
        private void StartBase(string url, System.Action<float> callbackProgress, System.Action<byte[], string> callbackEnd)
        {
            _http = shaco.Base.GameHelper.objectpool.Instantiate(typeof(shaco.Base.IHttpHelper).ToTypeString(), () =>
            {
                return (shaco.Base.IHttpHelper)shaco.Base.GameHelper.http.GetType().Instantiate();
            });

            _http.Download(url);
            _callbackEnd -= callbackEnd;
            _callbackEnd += callbackEnd;
            _actionWaitFor = shaco.WaitFor.Run(() =>
            {
                if (null == _http || _http.IsSuccess() || _http.HasError())
                    return true;
                else
                {
                    if (null != callbackProgress)
                    {
                        try
                        {
                            callbackProgress(_http.GetDownloadProgress());
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("HttpDownLoader StartBase error: 1 url=" + url + " callbackProgress=" + callbackProgress + " callbackEnd=" + callbackEnd + " e=" + e);
                        }
                    }
                    return false;
                }
            },
            () =>
            {
                if (null == _http)
                {
                    Log.Info("HttpDownLoader Start: user canel, url=" + url);
                }
                else
                {
                    if (null != callbackProgress)
                    {
                        try
                        {
                            callbackProgress(1);
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("HttpDownLoader StartBase error: 2 url=" + url + " callbackProgress=" + callbackProgress + " callbackEnd=" + callbackEnd + " e=" + e);
                        }
                    }

                    if (null != callbackEnd)
                    {
                        try
                        {
                            callbackEnd(_http.GetDownloadByte(), _http.GetLastError());
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("HttpDownLoader StartBase error: 3 url=" + url + " callbackProgress=" + callbackProgress + " callbackEnd=" + callbackEnd + " e=" + e);
                        }
                    }

                    if (null != _http)
                        _http.RecyclingWithPool();
                }
                _http = null;
                _actionWaitFor = null;
            });
        }

        private HttpDownLoader()
        {
            
        }
    }
}
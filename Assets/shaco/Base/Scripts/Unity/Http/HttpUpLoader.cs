using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class HttpUpLoader : IHttpUpLoader
    {
        //等待动作对象
        private shaco.ActionBase _actionWaitFor = null;

        //上传器
        private shaco.Base.IHttpHelper _http = null;

        //结束回调方法
        private System.Action<byte[], string> _callbackEnd = null;

        /// <summary>
        /// 开始上传
        /// <param name="url">地址</param>
        /// <param name="header">信息头，可以为空</param> 
        /// <param name="body">信息体，可以为空</param>
        /// <param name="callbackProgress">上传进度回调</param>
        /// <param name="callbackEnd">上传完毕回调</param>
        /// </summary>
        override public IHttpUpLoader Start(string url, shaco.Base.HttpComponent[] header, shaco.Base.HttpComponent[] body, System.Action<float> callbackProgress, System.Action<byte[], string> callbackEnd)
        {
            var retValue = new HttpUpLoader();
            retValue.StartBase(url, header, body, callbackProgress, callbackEnd);
            return (IHttpUpLoader)retValue;
        }

        /// <summary>
        /// 开始上传
        /// <param name="url">地址</param>
        /// <param name="form">表单</param> 
        /// <param name="callbackProgress">上传进度回调</param>
        /// <param name="callbackEnd">上传完毕回调</param>
        /// </summary>
        override public IHttpUpLoader StartWithForm(string url, shaco.Base.HttpComponent[] form, System.Action<float> callbackProgress, System.Action<byte[], string> callbackEnd)
        {
            var retValue = new HttpUpLoader();
            var urlConvert = shaco.Base.Utility.GetHttpRequestFullUrl(url, form.ToArray());
            retValue.StartBase(urlConvert, null, null, callbackProgress, callbackEnd);
            return (IHttpUpLoader)retValue;
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
        /// 强制中断上传
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
        /// 开始上传
        /// <param name="url">地址</param>
        /// <param name="header">信息头，可以为空</param> 
        /// <param name="body">信息体，可以为空</param>
        /// <param name="callbackProgress">上传进度回调</param>
        /// <param name="callbackEnd">上传完毕回调</param>
        /// </summary>
        private IHttpUpLoader StartBase(string url, shaco.Base.HttpComponent[] header, shaco.Base.HttpComponent[] body, System.Action<float> callbackProgress, System.Action<byte[], string> callbackEnd)
        {
            Log.Info("HttpUpLoader StartBase: url=" + url);
            _http = shaco.Base.GameHelper.objectpool.Instantiate(typeof(shaco.Base.IHttpHelper).ToTypeString(), () =>
            {
                return (shaco.Base.IHttpHelper)shaco.Base.GameHelper.http.GetType().Instantiate();
            });

            _http.Upload(url, header, body);
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
                            callbackProgress(_http.GetUploadProgress());
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("HttpUpLoader StartBase error: 1 url=" + url + " callbackProgress=" + callbackProgress + " callbackEnd=" + callbackEnd + " e=" + e);
                        }
                    }
                    return false;
                }
            },
            () =>
            {
                if (null == _http)
                {
                    Log.Info("HttpUpLoader Start: user canel, url=" + url);
                }
                else
                {
                    if (_http.HasError())
                    {
                        Log.Error("HttpUpLoader Start error: " + _http.GetLastError());
                    }

                    var downloadBytes = null != _callbackEnd ? _http.GetDownloadByte() : null;
                    var lastError = null != _callbackEnd ? _http.GetLastError() : null;
                    _http.RecyclingWithPool();
                    _http = null;

                    if (null != callbackProgress)
                    {
                        try
                        {
                            callbackProgress(1);
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("HttpUpLoader StartBase error: 2 url=" + url + " callbackProgress=" + callbackProgress + " callbackEnd=" + callbackEnd + " e=" + e);
                        }
                    }

                    if (null != _callbackEnd)
                    {
                        try
                        {
                            _callbackEnd(downloadBytes, lastError);
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("HttpUpLoader StartBase error: 3 url=" + url + " callbackProgress=" + callbackProgress + " callbackEnd=" + callbackEnd + " e=" + e);
                        }
                    }
                }
                _actionWaitFor = null;
            });
            return this;
        }

        private HttpUpLoader()
        {

        }
    }
}
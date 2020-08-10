using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    /// <summary>
    /// Unity网络管理类，只支持Unity5.3以上版本，暂时不会考虑更低版本兼容
    /// </summary>
    public class UnityHttpHelper : shaco.Base.IHttpHelper
    {
        //超时时间
        public double timeoutSeconds
        {
            get { return _timeoutSeconds; }
            set { _timeoutSeconds = value; }
        }
        private double _timeoutSeconds = 10.0f;

        //超时后重试次数
        public short timeoutRetryTimes
        {
            get { return _timeoutRetryTimes; }
            set { _timeoutRetryTimes = value; }
        }
        private short _timeoutRetryTimes = 5;

        //unity网络管理器
        private UnityEngine.Networking.UnityWebRequest _www = null;

        private string _lastError = string.Empty;
        private bool _isAutoSaveWhenCompleted = false;
        private string _strAutoSavePathWhenCompleted = string.Empty;
        private long _currentDownloadCountForComputerSpeed = 0;
        private bool _isClosed = false;
        private bool _isCompleted = false;
        private short _currentRetryTimes = 0;
        private string _url = string.Empty;
        private shaco.Base.CalculateTime _timerTimeout = null;
        private float _prevProgress;

#if DEBUG_LOG
        private System.DateTime _startTime;
        private System.DateTime _endTime;
#endif

        /// <summary>
        /// 清理占用的内存资源
        /// </summary>
        public void UnloadUnusedMemory()
        {
            //暂时没有需要回收的资源
        }

        /// <summary>
        /// 下载
        /// <param name="url">下载地址</param>
        /// </summary>
        public void Download(string url)
        {
            _www = UnityEngine.Networking.UnityWebRequest.Get(url);
            PrepareConnect(_www);
#if UNITY_2018_1_OR_NEWER
            _www.SendWebRequest();
#elif UNITY_5_3_OR_NEWER
            _www.Send();
#endif

            //开始下载
            long prevDownloadSize = 0;
            StartTimerCalculate();

            shaco.Base.WaitFor.Run(() =>
            {
                var oldPreDownloadSize = prevDownloadSize;
                UpdateWhenRunning(ref prevDownloadSize);

                //当有内容下载到的时候重新开始超时计算时间
                if (oldPreDownloadSize != prevDownloadSize)
                {
                    _timerTimeout.ResetTimeout();
                }
                return _isClosed || _www.isDone || !string.IsNullOrEmpty(_www.error) || _timerTimeout.IsTimeout();
            }, () =>
            {
                //用户强制取消
                if (_isClosed)
                {
                    _lastError = "UnityHttpHelper Download error: user cancel, url=" + _url;
                }
                //发生错误
                else if (!string.IsNullOrEmpty(_www.error))
                {
                    OnCompleted("UnityHttpHelper Download error: " + _www.error, () => Download(_url));
                }
                //检测超时
                else if (_timerTimeout.IsTimeout())
                {
                    OnCompleted("UnityHttpHelper Download error: timeout", () => Download(_url));
                }
                //下载成功
                else
                {
                    OnCompleted(string.Empty, null);
                }
            });
        }

        // /// <summary>
        // /// 上传
        // /// </summary>
        // /// <param name="url">下载地址</param>
        // /// <param name="header">资源头</param>
        // /// <param name="body">资源体</param>
        // public void UploadForm(string url, shaco.Base.HttpComponent[] header, shaco.Base.HttpComponent[] body)
        // {
        //     _www = UnityEngine.Networking.UnityWebRequest.Post(url, GetPostData(header, body, true));
        //     StartUpload(header, body, true, () => UploadForm(url, header, body));
        // }

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="header">资源头</param>
        /// <param name="body">资源体</param>
        public void Upload(string url, shaco.Base.HttpComponent[] header, shaco.Base.HttpComponent[] body)
        {
            _url = url;

            // List<UnityEngine.Networking.IMultipartFormSection> formDataConvert = null;

            //写入body
            // if (!body.IsNullOrEmpty())
            // {
            //     var formData = null == body ? null : body.ConvertList(v => (UnityEngine.Networking.IMultipartFormSection)new UnityEngine.Networking.MultipartFormDataSection(v.key, v.value));
            //     formDataConvert = (List<UnityEngine.Networking.IMultipartFormSection>)formData;
            // }

            var postData = GetPostData(body);
            _www = UnityEngine.Networking.UnityWebRequest.Post(url, postData);
            _www.chunkedTransfer = false;

            //写入header
            WriteHeader(_www, header);

            // if (!header.IsNullOrEmpty())
            // {
            //     for (int i = 0; i < header.Length; ++i)
            //     {
            //         _www.SetRequestHeader(header[i].key, header[i].value);
            //     }
            // }

            StartUpload(header, body, false, () => Upload(url, header, body));
        }

        /// <summary>
        /// 开始上传
        /// </summary>
        public void StartUpload(shaco.Base.HttpComponent[] header, shaco.Base.HttpComponent[] body, bool bForm, System.Action retryCallback)
        {
            PrepareConnect(_www);
#if UNITY_2018_1_OR_NEWER
            _www.SendWebRequest();
#elif UNITY_5_3_OR_NEWER
            _www.Send();
#endif

            //开始上传
            long prevUploadSize = 0;
            StartTimerCalculate();
            shaco.Base.WaitFor.Run(() =>
            {
                var oldPreUploadSize = prevUploadSize;
                UpdateWhenRunning(ref prevUploadSize);

                //当有内容下载到的时候重新开始超时计算时间
                if (oldPreUploadSize != prevUploadSize)
                {
                    _timerTimeout.ResetTimeout();
                }
                return _isClosed || _www.isDone || !string.IsNullOrEmpty(_www.error) || _timerTimeout.IsTimeout();
            }, () =>
            {
                //用户强制取消
                if (_isClosed)
                {
                    _lastError = "UnityHttpHelper Upload error: user cancel, url=" + _url;
                }
                //发生错误
                else if (!string.IsNullOrEmpty(_www.error))
                {
                    OnCompleted("UnityHttpHelper Upload error: " + _www.error, retryCallback);
                    // TODO:防止服务器请求峰值过高，暂时屏蔽
                    // if (bForm)
                    //     OnCompleted("UnityHttpHelper Upload error: ", () => UploadForm(_url, header, body));
                    // else
                    //     OnCompleted("UnityHttpHelper Upload error: ", () => Upload(_url, header, body));
                }
                //检测超时
                else if (_timerTimeout.IsTimeout())
                {
                    OnCompleted("UnityHttpHelper Upload error: timeout", retryCallback);
                }
                //上传成功
                else
                {
                    OnCompleted(string.Empty, null);
                }
            });
        }

        /// <summary>
        /// 获取下载完成了资源，如果还在下载中则返回null
        /// </summary>
        public byte[] GetDownloadByte()
        {
            return !IsValidWWW() || null == _www.downloadHandler ? null : _www.downloadHandler.data;
        }

        /// <summary>
        /// 当前上传或者下载是否完成
        /// </summary>
        public bool IsSuccess()
        {
            return _isCompleted && string.IsNullOrEmpty(_lastError);
        }

        /// <summary>
        /// 是否下载完毕，可能成功或者失败
        /// </summary>
        public bool IsDone()
        {
            return IsSuccess() || HasError();
        }

        /// <summary>
        /// 获取当前上传或者下载进度(范围:0~1)
        /// </summary>
        public float GetDownloadProgress()
        {
            if (!IsValidWWW())
                return _prevProgress;

            var retValue = _www.downloadProgress;
            retValue = System.Math.Max(_prevProgress, _www.downloadProgress);
            _prevProgress = retValue;
            return retValue;
        }

        /// <summary>
        /// 获取当前上传进度(范围:0~1)
        /// </summary>
        public float GetUploadProgress()
        {
            if (!IsValidWWW())
                return _prevProgress;

            var retValue = _www.uploadProgress;
            retValue = System.Math.Max(_prevProgress, _www.uploadProgress);
            _prevProgress = retValue;
            return retValue;
        }

        /// <summary>
        /// 获取当前下载或上传速度(单位: kb)
        /// </summary>
        public long GetDownloadSpeed()
        {
            double elapseTimeTmp = GetEplaseTimeSeconds();
            if (elapseTimeTmp <= 0)
                return 0;

            return (long)(_currentDownloadCountForComputerSpeed / elapseTimeTmp);
        }

        //重置网络下载速度
        public void ResetDownloadSpeed()
        {
            if (_timerTimeout != null)
                _timerTimeout.ResetEplaseTime();

            _currentDownloadCountForComputerSpeed = 0;
        }

        /// <summary>
        /// 获取从下载开始到现在已经经过的时间(单位：秒)
        /// </summary>
        public double GetEplaseTimeSeconds()
        {
            return _timerTimeout == null ? 1 : _timerTimeout.GetElapseTimeSeconds();
        }

        /// <summary>
        /// 获取当前一次循环下载的数据大小
        /// </summary>
        public long GetCurrentDownloadSize()
        {
            return (long)(!IsValidWWW() ? 0 : _www.downloadedBytes);
        }

        //获取最新一次网络错误信息
        public string GetLastError()
        {
            return _lastError;
        }

        //网址错误，没有找到对应地址
        public bool IsNotFound404()
        {
            return GetLastError().Contains("404");
        }


        //被服务器拒绝访问，可能是权限或者网址有误
        public bool IsForbidden403()
        {
            return GetLastError().Contains("403");
        }

        /// <summary>
        /// 下载是否发生错误
        /// </summary>
        public bool HasError()
        {
            return !string.IsNullOrEmpty(_lastError);
        }

        /// <summary>
        /// 关闭下载请求
        /// </summary>
        public void CloseClient()
        {
            CloseClientBase();
            CloseAutoSaveWhenCompleted();
        }

        private void CloseClientBase()
        {
            if (_isClosed)
                return;
            _isClosed = true;

            if (null != _www)
            {
                _www.Abort();
                _www.Dispose();
            }

            _isCompleted = false;
            _lastError = string.Empty;
            _currentDownloadCountForComputerSpeed = 0;

            if (_timerTimeout != null)
                _timerTimeout.Stop();
        }

        /// <summary>
		/// 开启并设定下载完毕后自动保存的文件路径
		/// <param name="pathSave">保存文件路径</param>
		/// </summary>
        public void SetAutoSaveWhenCompleted(string pathSave)
        {
            _isAutoSaveWhenCompleted = true;
            _strAutoSavePathWhenCompleted = pathSave;
        }

        /// <summary>
		/// 关闭下载完成后自动保存文件功能
		/// </summary>
        public void CloseAutoSaveWhenCompleted()
        {
            _isAutoSaveWhenCompleted = false;
            _strAutoSavePathWhenCompleted = string.Empty;
        }

        /// <summary>
        /// 准备建立链接
        /// </summary>
        private void PrepareConnect(UnityEngine.Networking.UnityWebRequest www)
        {
            _isClosed = false;
            _isCompleted = false;
            _currentRetryTimes = 0;
            _prevProgress = 0;
            if (_timerTimeout != null)
                _timerTimeout.ResetEplaseTime();
            _lastError = string.Empty;

            _url = www.url;
            _www.timeout = 0;

#if DEBUG_LOG
            _startTime = System.DateTime.Now;
#endif
        }

        /// <summary>
        /// 刷新当前数据
        /// </summary>
        private void UpdateWhenRunning(ref long prevDownloadSize)
        {
            //计算总下载时间
            var deltaTime = Time.deltaTime;

            //计算当前下载大小
            var currentDownloadSize = GetCurrentDownloadSize();
            var offsetDownloadSize = currentDownloadSize - prevDownloadSize;

            prevDownloadSize = currentDownloadSize;
            _currentDownloadCountForComputerSpeed += offsetDownloadSize;
        }

        /// <summary>
        /// 下载或者上传完毕
        /// <param name="error">错误信息，如果为空表示成功</param>
        /// <param name="retryCallback">重试方法</param>
        /// </summary>
        private void OnCompleted(string erorr, System.Action retryCallback)
        {
            if (!string.IsNullOrEmpty(erorr))
            {
                Log.Info(erorr + " url=" + _url + " retry times=" + _currentRetryTimes + " max times=" + _timeoutRetryTimes);

                //还未超出重试次数，关闭链接重新开始
                if (_currentRetryTimes++ < _timeoutRetryTimes)
                {
                    var oldRetryTimes = _currentRetryTimes;
                    CloseClientBase();

                    if (null != retryCallback)
                    {
                        retryCallback();
                    }
                    else
                    {
                        //丢失重试方法，异常结束
                        OnCompleted("missing retry callback function, url=" + _url);
                    }
                    _currentRetryTimes = oldRetryTimes;
                }
                //超出重试次数，异常结束
                else
                {
                    OnCompleted(erorr);
                }
            }
            else
            {
                //如果设置了自动保存却还没有设置保存地址，那么可能是因为下载速度太快，逻辑还没走到设置地址的地方就已经下载完毕了
                if (_isAutoSaveWhenCompleted)
                {
                    if (string.IsNullOrEmpty(_strAutoSavePathWhenCompleted))
                    {
                        //这里的错误几乎不会重现？可能会因为多线程导致的问题，还是提示一下吧
                        var errorMessage = "UnityHttpHelper OnCompleted error: wait set auto save path...url=" + _url;
                        Log.Error(errorMessage);
                        OnCompleted(errorMessage);
                    }
                    else
                    {
                        var downloadBytes = GetDownloadByte();
                        if (downloadBytes.IsNullOrEmpty())
                        {
                            var errorMessage = "UnityHttpHelper OnCompleted error: download bytes is empty, please check last error";
                            Log.Error(errorMessage);
                            OnCompleted(errorMessage);
                        }
                        else
                        {
                            //异步写入文件
                            shaco.Base.ThreadPool.RunThread(() =>
                            {
                                shaco.Base.FileHelper.WriteAllByteByUserPathWithoutLog(_strAutoSavePathWhenCompleted, downloadBytes);
#if DEBUG_LOG
                                _endTime = System.DateTime.Now;
                                var elapseTimeTmp = _endTime - _startTime;
                                var relativePath = shaco.Base.FileHelper.GetLastFileNameLevel(_strAutoSavePathWhenCompleted, true, 2);
                                var strAppend = new System.Text.StringBuilder();
                                strAppend.AppendFormat("time[{0}] UnityHttpHelper download success: path[{1}] start[{2}] end[{3}] retry[{4}] error[{5}]",
                                                                                            elapseTimeTmp.TotalSeconds.Round(2),
                                                                                            relativePath,
                                                                                            _startTime.ToString("HH:mm:ss"),
                                                                                            _endTime.ToString("HH:mm:ss"),
                                                                                            _currentRetryTimes,
                                                                                            _lastError);
                                Log.Info(strAppend.ToString());
#endif
                                if (!string.IsNullOrEmpty(_lastError))
                                {
                                    var errorMessage = "UnityHttpHelper OnCompleted: uncatched error=" + _lastError;
                                    Log.Error(errorMessage);
                                    OnCompleted(errorMessage);
                                    return;
                                }

                                //没有发生错误，正常结束
                                OnCompleted(string.Empty);
                            });
                        }
                    }
                }
                else
                {
                    //没有发生错误，正常结束
                    OnCompleted(string.Empty);
                }
            }
        }

        /// <summary>
        /// 任务结束
        /// <param name="lastError">最终的错误信息</param>
        /// </summary>
        private void OnCompleted(string lastError)
        {
            if (!string.IsNullOrEmpty(lastError))
                Log.Error("UnityHttpHelper OnCompleted error:" + _lastError);
            _lastError = lastError;
            _isCompleted = true;
        }

        /// <summary>
        /// 获取上传参数
        /// <param name="header">资源头</param>
        /// <param name="body">资源体</param>
        /// <return>上传参数</return>
        /// </summary>
        private WWWForm GetPostData(shaco.Base.HttpComponent[] body)
        {
            var retValue = new WWWForm();
            if (!body.IsNullOrEmpty())
            {
                for (int i = 0; i < body.Length; ++i)
                {
                    retValue.AddField(body[i].key, body[i].value);
                }
            }
            return retValue;
        }

        /// <summary>
        /// 获取上传参数
        /// <param name="www">请求对象</param>
        /// <param name="header">资源头</param>
        /// <return>上传参数</return>
        /// </summary>
        private void WriteHeader(UnityEngine.Networking.UnityWebRequest www, shaco.Base.HttpComponent[] header)
        {
            if (!header.IsNullOrEmpty())
            {
                for (int i = 0; i < header.Length; ++i)
                {
                    _www.SetRequestHeader(header[i].key, header[i].value);
                }
            }
        }

        // Dictionary<string, string> HttpComponentToDic(shaco.Base.HttpComponent[] component)
        // {
        //     if (component.IsNullOrEmpty()) return null;

        //     Dictionary<string, string> dic = new Dictionary<string, string>();
        //     for (int i = 0; i < component.Length; ++i)
        //     {
        //         dic[component[i].key] = component[i].value;
        //     }

        //     return dic;
        // }

        /// <summary>
        /// 当前网络接口是否可用
        /// </summary>
        private bool IsValidWWW()
        {
            return null != _www && !_isClosed;
        }

        private void StartTimerCalculate()
        {
            if (_timerTimeout != null)
            {
                _timerTimeout.Stop();
            }
            _timerTimeout = new shaco.Base.CalculateTime();
            _timerTimeout.timeoutSeconds = timeoutSeconds;
            _timerTimeout.Start();
        }
    }
}
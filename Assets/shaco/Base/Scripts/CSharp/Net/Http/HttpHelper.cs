using System.Collections;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;

namespace shaco.Base
{
    /// <summary>
    /// 网络管理类，不支持WebGL平台
    /// </summary>
    public class HttpHelper : IHttpHelper
    {
        private enum HttpState
        {
            None,
            RequestDownload,
            RequestUpload,
            Downloading,
            Uploading,
            Completed
        }

        private class DownloadInfo : shaco.Base.IObjectPoolData
        {
            public byte[] byteDownloadRequst = new byte[1024 * MaxDownloadSpeedKB];

            public void Dispose()
            {
                System.Array.Clear(byteDownloadRequst, 0, byteDownloadRequst.Length);
            }
        }

        //最大http链接限制数量
        public const int DefaultConnectionLimit = 10;
        //最大上传速度(单位kb)
        public const int MaxUploadSpeedKB = 1024;
        //最大下载速度(单位kb)
        public const int MaxDownloadSpeedKB = 1024;

        //超时时间
        public double timeoutSeconds
        {
            get { return _timeoutSeconds; }
            set { _timeoutSeconds = value; }
        }

        //超时后重试次数
        public short timeoutRetryTimes
        {
            get { return _timeoutRetryTimes; }
            set { _timeoutRetryTimes = value; }
        }

        private System.Collections.Generic.List<byte> _byteDownload = new System.Collections.Generic.List<byte>();

        private double _timeoutSeconds = 10.0f;
        private short _timeoutRetryTimes = 5;
        private HttpState _stateDownload = HttpState.None;
        private string _urlHttp = string.Empty;
        private HttpComponent[] _listHeader = null;
        private HttpComponent[] _listBody = null;
        private string _strBodyFormat = "{0}={1}";
        private string _strBodySplitFlag = "&";
        private string _strLastError = string.Empty;
        private bool _isAutoSaveWhenCompleted = false;
        private string _strAutoSavePathWhenCompleted = string.Empty;
        private bool _isClosed = false;
        private CalculateTime _timerTimeout = null;
        private long _lCurrentProgressCount = 0;
        private long _lCurrentCountForSpeedComputer = 0;
        private long _lAllProgressCount = 0;
        private bool _isThreadClosed = false;
        private int _iCurrentRetryTimes = 0;
        private DownloadInfo _downloadInfo = null;
        //是否支持断点续传
        private bool _isSupportBreakPoint = true;

        //是否已经设置过网络证书回调
        static private bool _isSettedServerCertificateCallBack = false;

        //是否已经清理过占用的资源
        static private bool _isUnloadedUnusedMemory = false;

        /// <summary>
        /// 清理占用的内存资源
        /// </summary>
        public void UnloadUnusedMemory()
        {
            if (shaco.Base.GameHelper.objectpool.isUnused(typeof(DownloadInfo).ToTypeString()))
            {
                shaco.Base.GameHelper.objectpool.DestroyAllObjects(typeof(DownloadInfo).ToTypeString());
            }
            _isUnloadedUnusedMemory = true;
        }

        /// <summary>
        /// 异步下载资源，IsSuccess返回true后
        /// </summary>
        /// <param name="url">URL.</param>服务器地址
        public void Download(string url)
        {
            InitConnect(url);
            ThreadPool.RunThread(LoadThread);
            // Log.Info("HttpHelper download url=" + url);
        }

        // /// <summary>
        // /// 异步上传资源
        // /// </summary>
        // /// <param name="url">URL.</param>服务器地址
        // /// <param name="header">Header.</param>资源头 
        // /// <param name="body">Body.</param>资源体
        // public void UploadForm(string url, HttpComponent[] header, HttpComponent[] body)
        // {
        //     this._listHeader = header;
        //     this._listBody = body;
        //     InitConnect(url);
        //     ThreadPool.RunThread(UploadThread);
        // }

        /// <summary>
        /// 异步上传资源
        /// </summary>
        /// <param name="url">URL.</param>服务器地址
        /// <param name="header">Header.</param>资源头 
        /// <param name="body">Body.</param>资源体
        public void Upload(string url, HttpComponent[] header, HttpComponent[] body)
        {
            this._listHeader = header;
            this._listBody = body;
            InitConnect(url);
            ThreadPool.RunThread(UploadThread);
        }

        /// <summary>
        /// 获取下载完成了资源
        /// </summary>
        /// <returns>The download byte.</returns>
        public byte[] GetDownloadByte()
        {
            if (_stateDownload == HttpState.Downloading || _stateDownload == HttpState.Uploading)
                return null;

            return _byteDownload.ToArray();
        }

        /// <summary>
        /// 当前上传或者下载是否完成
        /// </summary>
        public bool IsSuccess()
        {
            return _stateDownload == HttpState.Completed && !HasError();
        }

        /// <summary>
        /// 是否下载完毕，可能成功或者失败
        /// </summary>
        public bool IsDone()
        {
            return IsSuccess() || HasError();
        }

        /// <summary>
        /// 获取当前下载进度(范围:0~1)
        /// </summary>
        /// <returns>The progress.</returns>
        public float GetDownloadProgress()
        {
            if (_lAllProgressCount <= 0)
                return 0;
            else
                return (float)((double)_lCurrentProgressCount / (double)_lAllProgressCount);
        }

        /// <summary>
        /// 获取当前上传进度(范围:0~1)
        /// </summary>
        public float GetUploadProgress()
        {
            return GetDownloadProgress();
        }

        /// <summary>
        /// 获取当前下载或上传速度(单位: kb)
        /// </summary>
        /// <returns>The download speed.</returns>
        public long GetDownloadSpeed()
        {
            double elapseTimeTmp = GetEplaseTimeSeconds();
            if (_timerTimeout == null || elapseTimeTmp <= 0)
                return 0;

            long ret = (long)((double)_lCurrentCountForSpeedComputer / (double)elapseTimeTmp);
            return ret;
        }

        //重置网络下载速度
        public void ResetDownloadSpeed()
        {
            _lCurrentCountForSpeedComputer = 0;

            if (_timerTimeout != null)
                _timerTimeout.ResetEplaseTime();
        }

        public double GetEplaseTimeSeconds()
        {
            return _timerTimeout == null ? 1 : _timerTimeout.GetElapseTimeSeconds();
        }

        /// <summary>
        /// 获取当前一次循环下载的数据大小
        /// </summary>
        public long GetCurrentDownloadSize()
        {
            return _lCurrentProgressCount;
        }

        //获取最新一次网络错误信息
        public string GetLastError()
        {
            return _strLastError;
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

        public bool HasError()
        {
            return !string.IsNullOrEmpty(_strLastError);
        }

        public void CloseClient()
        {
            CloseClientBase();
            CloseAutoSaveWhenCompleted();
        }

        private void CloseClientBase()
        {
            _isThreadClosed = true;

            if (!_isClosed)
            {
                _isClosed = true;

                if (_timerTimeout != null)
                {
                    _timerTimeout.Stop();
                }

                if (null != _downloadInfo)
                {
                    if (!_isUnloadedUnusedMemory)
                    {
                        shaco.Base.GameHelper.objectpool.RecyclingObject(_downloadInfo);
                    }
                    _downloadInfo = null;
                }
                CloseAutoSaveWhenCompleted();
            }
        }

        //设置为当下载完毕后自动保存文件到本地
        //@pathSave: 要保存的文件路径
        public void SetAutoSaveWhenCompleted(string pathSave)
        {
            if (string.IsNullOrEmpty(pathSave))
            {
                Log.Error("HttpHelper SetAutoSaveWhenCompleted error: path is valid");
                return;
            }
            
            _isAutoSaveWhenCompleted = true;
            _strAutoSavePathWhenCompleted = pathSave;
        }

        //关闭下载完成后自动保存文件功能
        public void CloseAutoSaveWhenCompleted()
        {
            _isAutoSaveWhenCompleted = false;
            _strAutoSavePathWhenCompleted = string.Empty;
        }

        private void ResetParam()
        {
            _strLastError = string.Empty;
            _stateDownload = HttpState.None;
            _isClosed = false;
            _lCurrentProgressCount = 0;
            _lAllProgressCount = 0;
            _isThreadClosed = false;
            _iCurrentRetryTimes = 0;
            _lCurrentCountForSpeedComputer = 0;
            _byteDownload.Clear();

            if (System.Net.ServicePointManager.DefaultConnectionLimit != DefaultConnectionLimit)
            {
                System.Net.ServicePointManager.DefaultConnectionLimit = DefaultConnectionLimit;
            }
        }

        //检查url是否支持断点续传
        private bool CheckSupportBreakPoint(string url)
        {
            var request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);

            request.ServicePoint.ConnectionLimit = DefaultConnectionLimit;
            request.Proxy = null;
            request.KeepAlive = true;
            request.Timeout = (int)timeoutSeconds * 1000;
            request.AddRange(0, 1);
            System.Net.HttpWebResponse response;
            try
            {
                response = (System.Net.HttpWebResponse)request.GetResponse();
            }
            catch (System.Net.WebException ex)
            {
                response = (System.Net.HttpWebResponse)ex.Response;
            }

            //根据HTTP状态码判断是否支持断点续传
            return response != null && response.StatusCode == System.Net.HttpStatusCode.PartialContent;
        }

        private void InitConnect(string url)
        {
            CloseClientBase();
            ResetParam();
            _urlHttp = url;
            // HttpHelperCache.AddHelper(this);

            if (!_isSettedServerCertificateCallBack)
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback = CheckRemoteCertificateValidationCallback;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                _isSettedServerCertificateCallBack = true;
            }
            _isUnloadedUnusedMemory = false;
        }

        private void LoadThread()
        {
            if (_stateDownload != HttpState.None)
            {
                Log.Error("HttpHelper UploadThread error: invalid state=" + _stateDownload);
                return;
            }

            _stateDownload = HttpState.Downloading;

            if (!_isSupportBreakPoint)
            {
                try
                {
                    _isSupportBreakPoint = CheckSupportBreakPoint(_urlHttp);
                }
                catch (System.Exception e)
                {
                    var errorMsg = "HttpHelper LoadThread error: url=" + _urlHttp + "\ne=" + e;
                    Log.Error(errorMsg);
                    OnCompleted(errorMsg);
                    return;
                }
            }

            // System.Threading.Thread.Sleep(1);
            var httpRequest = CreateDefaultHttpRequest("GET");
            StartTimerCalculate();

            if (_isSupportBreakPoint)
            {
                if (_lCurrentProgressCount != _lAllProgressCount)
                {
                    Log.Info("will continue download, current=" + _lCurrentProgressCount + " total=" + _lAllProgressCount);
                    httpRequest.AddRange((int)_lCurrentProgressCount);
                }
            }
            else
            {
                _lCurrentProgressCount = 0;
                _byteDownload.Clear();
            }

            if (_lCurrentProgressCount > _lAllProgressCount)
            {
                Log.Error("HttpHelper download progress error: out of range! _lCurrentProgressCount=" + _lCurrentProgressCount + " _lAllProgressCount=" + _lAllProgressCount);
            }

            Downloading(httpRequest, LoadThread);
        }

        private HttpWebRequest CreateDefaultHttpRequest(string method)
        {
            int timeoutSeconds = (int)this.timeoutSeconds * 1000;
            try
            {
                var httpRequest = (HttpWebRequest)HttpWebRequest.Create(_urlHttp);
                // httpRequest.Proxy = null;
                httpRequest.ServicePoint.ConnectionLimit = DefaultConnectionLimit;
                httpRequest.Credentials = CredentialCache.DefaultCredentials;
                httpRequest.KeepAlive = true;
                httpRequest.Method = method;
                httpRequest.Timeout = timeoutSeconds;
                httpRequest.ReadWriteTimeout = timeoutSeconds;
                return httpRequest;
            }
            catch (System.Exception e)
            {
                Log.Error("HttpHelper CreateDefaultHttpRequest error: url=" + _urlHttp + "\n" + e.ToString());
                return null;
            }
        }

        private void Downloading(HttpWebRequest httpRequest, System.Action retryCallback)
        {
            bool HasErrorTmp = false;
            Stream readStream = null;
            WebResponse httpResponse = null;

            if (null == httpRequest)
            {
                OnCompleted("HttpHelper Downloading error: http request is null");
                return;
            }

            try
            {
                httpResponse = httpRequest.GetResponse();
                using (httpResponse)
                {
                    readStream = httpResponse.GetResponseStream();
                    readStream.ReadTimeout = (int)timeoutSeconds * 1000;

                    if (_downloadInfo == null)
                    {
                        _downloadInfo = shaco.Base.GameHelper.objectpool.Instantiate(typeof(DownloadInfo).ToTypeString(), () => new DownloadInfo());
                    }

                    if (_lAllProgressCount <= 0)
                    {
                        _lAllProgressCount = httpResponse.ContentLength;
                        _byteDownload.Clear();
                    }

                    try
                    {
                        do
                        {
                            long readSize = (long)readStream.Read(_downloadInfo.byteDownloadRequst, 0, _downloadInfo.byteDownloadRequst.Length);
                            if (_isThreadClosed)
                                break;

                            if (readSize > 0)
                            {
                                long remainSize = _lAllProgressCount - _lCurrentProgressCount;
                                if (readSize > remainSize)
                                {
                                    HasErrorTmp = true;
                                    _isThreadClosed = true;
                                    _lCurrentProgressCount = 0;
                                    if (null != retryCallback)
                                        RetryDownload("http read stream error: read size is out of range ! readSize=" + readSize + " remainSize=" + remainSize + " _lAllProgressCount=" + _lAllProgressCount, retryCallback);
                                    else
                                        SetLastError(string.Format("http readSize{0} > remainSize{1}", readSize, remainSize));
                                    break;
                                }

                                _iCurrentRetryTimes = 0;

                                if (_timerTimeout != null)
                                    _timerTimeout.ResetTimeout();

                                for (int i = 0; i < readSize; ++i)
                                {
                                    _byteDownload.Add(_downloadInfo.byteDownloadRequst[i]);
                                }
                                _lCurrentProgressCount += readSize;
                                _lCurrentCountForSpeedComputer += readSize;

                                // Log.Info("eplase time=" + GetEplaseTimeSeconds() + " download count=" + _lCurrentProgressCount + " all count=" + _lAllProgressCount);
                            }

                            if (_isThreadClosed)
                                break;

                            //下载完毕
                            if (_lCurrentProgressCount == _lAllProgressCount)
                            {
                                break;
                            }
                            //下载文件大小超出预期
                            else if (_lCurrentProgressCount > _lAllProgressCount)
                            {
                                HasErrorTmp = true;
                                _isThreadClosed = true;
                                OnCompleted("http error: download data is invalid ! current progress=" + _lCurrentProgressCount + " all progress=" + _lAllProgressCount);
                            }
                            else
                            {
                                if (_timerTimeout != null)
                                {
                                    if (_timerTimeout.IsTimeout())
                                    {
                                        HasErrorTmp = true;
                                        _isThreadClosed = true;
                                        if (null != retryCallback)
                                            RetryDownload("manual detection download timeout", retryCallback);
                                    }
                                }
                            }

                            System.Threading.Thread.Sleep(1);

                        } while (!_isThreadClosed);
                    }
                    catch (System.Exception e)
                    {
                        HasErrorTmp = true;

                        if (null != retryCallback)
                            RetryDownload(e.ToString(), retryCallback);
                    }
                }
            }
            catch (System.Exception e)
            {
                HasErrorTmp = true;
                if (null != retryCallback)
                    RetryDownload(e.ToString(), retryCallback);
            }


            if (!_isThreadClosed)
            {
                if (!HasErrorTmp)
                {
                    OnCompleted(GetLastError());
                }
            }
            else
            {
                Log.Info("HttpHelper download thread: User Cancel" + "\nurl=" + _urlHttp);
            }

            _isThreadClosed = false;

            if (readStream != null)
            {
                readStream.Close();
                readStream = null;
            }
            if (httpResponse != null)
            {
                httpResponse.Close();
                httpResponse = null;
            }
            if (httpRequest != null)
            {
                httpRequest.Abort();
                httpRequest = null;
            }
        }

        private void RetryDownload(string msg, System.Action retryCallback)
        {
            //如果之前就是关闭状态了，则不再重试
            if (_isClosed)
            {
                Log.Warning("HttpHelper RetryDownload warning: aleady closed");
                return;
            }
                
            Log.Info("HttpHelper retry download warning=" + msg + " read offset=" + _lCurrentProgressCount + " total count=" + _lAllProgressCount);
            CloseClientBase();

            if (_iCurrentRetryTimes++ < timeoutRetryTimes)
            {
                Log.Info("HttpHelper will retry again=" + _iCurrentRetryTimes + " total times=" + timeoutRetryTimes + "\nurl=" + _urlHttp);

                if (_timerTimeout != null)
                    _timerTimeout.ResetTimeout();

                _isClosed = false;
                _stateDownload = HttpState.None;

                // HttpHelperCache.AddHelper(this);
                ThreadPool.RunThread(retryCallback);
            }
            else
            {
                OnCompleted("http download error: " + msg + "\nurl=" + _urlHttp);
            }
        }

        private void OnCompleted(string error)
        {
            if (_lCurrentProgressCount == _lAllProgressCount)
            {
                if (_timerTimeout != null)
                    _timerTimeout.Stop();
                if (_isAutoSaveWhenCompleted)
                {
                    //如果设置了自动保存却还没有设置保存地址，那么可能是因为下载速度太快，逻辑还没走到设置地址的地方就已经下载完毕了
                    while (true)
                    {
                        if (string.IsNullOrEmpty(_strAutoSavePathWhenCompleted))
                        {
                            Log.Error("HttpHelper OnCompleted: wait set auto save path...url=" + _urlHttp);
                        }
                        else
                        {
                            FileHelper.WriteAllByteByUserPath(_strAutoSavePathWhenCompleted, _byteDownload.ToArray());
                            break;
                        }
                    }
                }
            }

            SetLastError(error);
            Log.Error(error);
            CloseClientBase();
            _stateDownload = HttpState.Completed;
        }

        private void UploadThread()
        {
            if (_stateDownload != HttpState.None)
            {
                Log.Error("HttpHelper UploadThread error: invalid state=" + _stateDownload);
                return;
            }

            _stateDownload = HttpState.Uploading;
            var httpRequest = CreateDefaultHttpRequest("POST");

            string strBodyTmp = string.Empty;

            if (!_listHeader.IsNullOrEmpty())
            {
                for (int i = 0; i < _listHeader.Length; ++i)
                {
                    httpRequest.Headers.Add(_listHeader[i].key, _listHeader[i].value);
                }
            }
            if (!_listBody.IsNullOrEmpty() && _listBody.Length > 0)
            {
                strBodyTmp += string.Format(_strBodyFormat, _listBody[0].key, _listBody[0].value);
                for (int i = 1; i < _listBody.Length; ++i)
                {
                    strBodyTmp += _strBodySplitFlag;
                    strBodyTmp += string.Format(_strBodyFormat, _listBody[i].key, _listBody[i].value);
                }
            }


            // httpRequest.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            httpRequest.ContentLength = strBodyTmp.Length;
            var writeBodyTmp = strBodyTmp.ToByteArray();
            int totalWritedLength = 0;
            int requestWriteCount = MaxUploadSpeedKB * 1024;
            _lAllProgressCount = writeBodyTmp.Length;

            //上传内容
            try
            {
                using (Stream reqStream = httpRequest.GetRequestStream())
                {
                    while (totalWritedLength < writeBodyTmp.Length)
                    {
                        if (_isThreadClosed)
                            break;

                        int remainSize = writeBodyTmp.Length - totalWritedLength;
                        int currentWriteLength = requestWriteCount > remainSize ? remainSize : requestWriteCount;
                        reqStream.Write(writeBodyTmp, totalWritedLength, currentWriteLength);
                        totalWritedLength += currentWriteLength;
                        _lCurrentCountForSpeedComputer += currentWriteLength;
                    }

                    reqStream.Close();
                }

                //获取返回信息
                Downloading(httpRequest, UploadThread);
            }
            catch (System.Exception e)
            {
                Log.Error("HttpHelper UploadThread exception: e=" + e + "\nurl=" + _urlHttp);
                OnCompleted(e.ToString());
            }
        }

        private void SetLastError(string errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg) && HasError())
            {
                Log.Error("HttpHelper SetLastError error: can't set error message with empty string when has error");
                return;
            }
            _strLastError = errorMsg;
        }

        private void StartTimerCalculate()
        {
            if (_timerTimeout != null)
            {
                _timerTimeout.Stop();
            }
            _timerTimeout = new CalculateTime();
            _timerTimeout.timeoutSeconds = timeoutSeconds;
            _timerTimeout.Start();
        }

        /// <summary>
        /// 检查网络证书有效性并自动设置
        /// <return>返回true表示证书有效</return>
        /// </summary>
        static private bool CheckRemoteCertificateValidationCallback(System.Object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;

            // If there are errors in the certificate chain,
            // look at each error to determine the cause.
            if (sslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        continue;
                    }
                    chain.ChainPolicy.RevocationFlag = System.Security.Cryptography.X509Certificates.X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new System.TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = System.Security.Cryptography.X509Certificates.X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((System.Security.Cryptography.X509Certificates.X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                        break;
                    }
                }
            }
            return isOk;
        }
    }
}

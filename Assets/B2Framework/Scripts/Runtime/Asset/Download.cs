using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace B2Framework
{
    public class Download : DownloadHandlerScript, ICloneable
    {
        public int id { get; set; }
        public long len { get; set; }
        public string hash { get; set; }
        public string url { get; set; }
        public string savePath { get; set; }
        public string name { get; set; }
        public string error { get; private set; }
        public long position { get; private set; }
        public bool finished { get; private set; }
        public Action<Download> completed { get; set; }
        public string tempPath
        {
            get
            {
                return Utility.Path.Combine(Path.GetDirectoryName(savePath), hash);
            }
        }
        private UnityWebRequest _request;
        private FileStream _stream;
        private bool _running;
        public void Start()
        {
            if (_running) return;
            error = null;
            finished = false;
            _running = true;
            _stream = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
            position = _stream.Length;
            if (position < len)
            {
                _stream.Seek(position, SeekOrigin.Begin);
                _request = UnityWebRequest.Get(url);
                _request.SetRequestHeader("Range", "bytes=" + position + "-");
                _request.downloadHandler = this;
                _request.SendWebRequest();
                Log.Debug("Start DownLoad: " + url);
            }
            else Complete();
        }
        public void ReStart()
        {
            Dispose();
            Start();
        }
        public void Update()
        {
            if (!_running) return;
            if (_request.isDone && _request.downloadedBytes < (ulong)len)
            {
                error = "unknown error: downloadedBytes < len";
            }
            if (!string.IsNullOrEmpty(_request.error)) error = _request.error;
        }
        public void Complete(bool stop = false)
        {
            Dispose();
            if (!stop) OnComplete();
        }
        private void OnComplete()
        {
            var path = tempPath;
            if (!File.Exists(path))
            {
                error = "temp file is not exist: " + path;
                return;
            }
            if (string.IsNullOrEmpty(error))
            {
                using (var fs = File.OpenRead(path))
                {
                    // check file length
                    if (fs.Length != len) error = "file length error: file length:" + fs.Length + ";target length:" + len;
                    // check hash
                    if (Versions.verifyBy == VerifyBy.Hash)
                    {
                        if (!hash.Equals(Utility.Verifier.GetCRC32(fs), StringComparison.OrdinalIgnoreCase))
                        {
                            error = "hash error :" + url;
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(error))
            {
                // copy to save path
                File.Copy(path, savePath, true);
                File.Delete(path);
                Log.Debug("Download Complete: " + url);
                completed?.Invoke(this);
                completed = null;
            }
            else File.Delete(path);
        }
        public new void Dispose()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
                _stream = null;
            }
            if (_request != null)
            {
                _request.Abort();
                _request.Dispose();
                _request = null;
            }
            base.Dispose();
            _running = false;
            finished = true;
        }
        public override string ToString()
        {
            return string.Format("{0}, size:{1}, hash:{2}", url, len, hash);
        }
        #region Override DownloadHandler
        protected override float GetProgress()
        {
            return position * 1f / len;
        }

        protected override byte[] GetData()
        {
            return null;
        }
        protected override void ReceiveContentLengthHeader(ulong contentLength) { }
        protected override bool ReceiveData(byte[] buffer, int dataLength)
        {
            if (!string.IsNullOrEmpty(_request.error))
            {
                error = _request.error;
                Complete();
                return true;
            }

            _stream.Write(buffer, 0, dataLength);
            position += dataLength;
            return _running;
        }
        protected override void CompleteContent()
        {
            Complete();
        }
        #endregion
        #region ICloneable implementation
        public object Clone()
        {
            return new Download()
            {
                id = id,
                hash = hash,
                url = url,
                len = len,
                savePath = savePath,
                completed = completed,
                name = name
            };
        }
        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace B2Framework
{
    public class Downloader : MonoBehaviour
    {
        public int maxDownloads = 3;
        [SerializeField] private float sampleTime = 0.5f;
        private readonly List<Download> _downloads = new List<Download>();
        public List<Download> downloads { get { return _downloads; } }
        private readonly Queue<Download> _waitings = new Queue<Download>();
        private readonly List<Download> _downloadings = new List<Download>();

        public Action<long, long, float> onUpdate;
        public Action completed;

        public long size { get; private set; }
        public long position { get; private set; }
        public float speed { get; private set; }

        private int _finishedIdx;
        private int _downloadIdx;
        private float _startTime;
        private float _lastTime;
        private long _lastSize;
        private bool _running;

        public void StartDownload()
        {
            _waitings.Clear();
            _finishedIdx = 0;
            _lastSize = 0L;
            Restart();
        }
        public void Restart()
        {
            _lastTime = 0f;
            _startTime = Time.realtimeSinceStartup;
            _running = true;
            _downloadIdx = _finishedIdx;
            var max = Math.Min(_downloads.Count, maxDownloads);
            for (var i = _finishedIdx; i < max; i++)
            {
                _waitings.Enqueue(_downloads[i]);
                _downloadIdx++;
            }
        }
        public void AddDownload(string url, string filename, string savePath, string hash, long len)
        {
            var download = new Download
            {
                id = _downloads.Count,
                url = url,
                name = filename,
                hash = hash,
                len = len,
                savePath = savePath,
                completed = OnFinished
            };
            _downloads.Add(download);
            var info = new System.IO.FileInfo(download.tempPath);
            if (info.Exists) size += len - info.Length; else size += len;
        }
        private long GetDownloadSize()
        {
            var len = 0L;
            var downloadSize = 0L;
            for (var i = 0; i < _downloads.Count; i++)
            {
                len += _downloads[i].len;
                downloadSize += _downloads[i].position;
            }
            return downloadSize - (len - size);
        }
        private void Update()
        {
            if (!_running) return;
            if (_waitings.Count > 0)
            {
                var max = Math.Min(maxDownloads, _waitings.Count);
                while (max > 0)
                {
                    var item = _waitings.Dequeue();
                    item.Start();
                    _downloadings.Add(item);
                    max--;
                }

                for (var i = 0; i < _downloadings.Count; i++)
                {
                    var download = _downloadings[i];
                    download.Update();
                    if (!download.finished) continue;
                    _downloadings.RemoveAt(i);
                    i--;
                }
                position = GetDownloadSize();

                var elapsed = Time.realtimeSinceStartup - _startTime;
                var deltaTime = elapsed - _lastTime;
                if (deltaTime < sampleTime) return;

                speed = (position - _lastSize) / deltaTime;
                if (onUpdate != null) onUpdate(position, size, speed);

                _lastTime = elapsed;
                _lastSize = position;
            }
        }
        private void OnFinished(Download download)
        {
            if (_downloadIdx < _downloads.Count)
            {
                _waitings.Enqueue(_downloads[_downloadIdx]);
                _downloadIdx++;
            }
            _finishedIdx++;
            Debug.Log(Utility.Text.Format("OnFinished:{0}, {1}", _finishedIdx, _downloads.Count));
            if (_finishedIdx != _downloads.Count) return;
            completed?.Invoke();
            _running = false;
        }
        public void Stop()
        {
            _waitings.Clear();
            foreach (var download in _downloadings)
            {
                download.Complete(true);
                _downloads[download.id] = download.Clone() as Download;
            }
            _downloadings.Clear();
            _running = false;
        }
        public void Clear()
        {
            size = 0;
            position = 0;

            _downloadIdx = 0;
            _finishedIdx = 0;
            _lastTime = 0;
            _lastSize = 0;
            _startTime = 0;
            _running = false;
            foreach (var item in _downloadings) item.Complete(true);
            _downloadings.Clear();
            _downloads.Clear();
            _waitings.Clear();
        }
        // private void OnApplicationFocus(bool focus)
        // {
        //     if (_downloads.Count <= 0) return;
        //     if (!The.IsEditor) if (focus) ReStart(); else Stop();
        // }
    }
}
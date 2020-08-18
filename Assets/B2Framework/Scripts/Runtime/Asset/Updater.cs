using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using B2Framework.Net;
using B2Framework;
using UnityEngine;
using UnityEngine.Networking;

namespace B2Framework
{
    public interface IUpdater
    {
        void OnStart();

        void OnMessage(string msg);

        void OnProgress(float progress);

        void OnVersion(string ver);

        void OnClear();
    }
    [RequireComponent(typeof(Downloader))]
    [RequireComponent(typeof(NetworkObserver))]
    public class Updater : MonoBehaviour, IUpdater, INetworkListener
    {
        public IUpdater listener { get; set; }
        private Downloader _downloader;
        private NetworkObserver _networker;
        private List<UnityWebRequest> _downloads = new List<UnityWebRequest>();
        private UStatus _status;
        private bool _netWorkChanged;
        private void Start()
        {
            _downloader = gameObject.GetComponent<Downloader>();
            _downloader.onUpdate = OnUpdate;
            _downloader.completed = OnComplete;

            _networker = gameObject.GetComponent<NetworkObserver>();
            _networker.listener = this;

            _status = UStatus.Idle;

            StartUpdate();
        }
        public void StartUpdate()
        {
            // 编辑器模式跳过资源更新
            if (!GameUtility.Assets.runtimeMode)
            {
                StartCoroutine(StartGame());
                return;
            }

            GameUtility.Assets.CreatePersistentDataDir();

            OnStart();
            Reset();
            _status = UStatus.Requesting;
        }
        public void Clear()
        {
            MessageBox.Show("提示", "清除数据后所有数据需要重新下载，请确认！", "清除").callBack = sure =>
            {
                if (sure) OnClear();
            };
        }
        #region IUpdater implementation
        public void OnClear()
        {
            OnMessage("数据清除完毕");
            OnProgress(0);

            Reset();

            _downloader.Clear();
            _status = UStatus.Idle;
            _netWorkChanged = false;

            AssetsManger.Clear();

            listener?.OnClear();

            Versions.Clear();
        }

        public void OnMessage(string msg)
        {
            listener?.OnMessage(msg);
        }

        public void OnProgress(float progress)
        {
            listener?.OnProgress(progress);
        }

        public void OnStart()
        {
            listener?.OnStart();
        }

        public void OnVersion(string ver)
        {
            listener?.OnVersion(ver);
        }
        #endregion
        private void Update()
        {
            switch (_status)
            {
                case UStatus.Idle: break;
                case UStatus.Requesting:
                    _status = UStatus.Idle;
                    RequestVersions();
                    break;
                case UStatus.Checking:
                    _status = UStatus.Idle;
                    CheckVersions();
                    break;
            }
        }
        private void RequestVersions()
        {
            OnMessage("正在获取版本信息...");
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                MessageBox.Show("提示", "请检查网络连接状态", "重试", "退出").callBack = OnError;
                return;
            }
            var url = GameUtility.Assets.GetDownloadURL(GameConst.RES_VER_FILE);
            Log.Debug(url);
            var request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerFile(Versions.versionFile);
            _downloads.Add(request);
            var oper = request.SendWebRequest();
            oper.completed += delegate
             {
                 if (!string.IsNullOrEmpty(request.error))
                 {
                     MessageBox.Show("提示", string.Format("获取服务器版本失败：{0}", request.error), "重试", "退出").callBack = OnError;
                 }
                 else
                 {
                     try
                     {
                         Versions.serverVersion = Versions.LoadVersion();
                         var newFiles = Versions.GetNewFiles();
                         if (newFiles.Count > 0)
                         {
                             foreach (var item in newFiles)
                             {
                                 var path = GetFilePath(item.name);
                                 _downloader.AddDownload(GameUtility.Assets.GetDownloadURL(item.name), item.name, path, item.hash, item.len);
                             }
                             _status = UStatus.Checking;
                         }
                         else OnComplete();
                     }
                     catch (Exception e)
                     {
                         Log.Error(e.Message);
                         MessageBox.Show("提示", "版本文件加载失败", "重试", "退出").callBack = OnError;
                     }
                 }
             };
        }
        private void CheckVersions()
        {
            OnMessage("正在检查版本信息...");
            var totalSize = _downloader.size;
            if (totalSize > 0)
            {
                var tips = string.Format("发现内容更新，总计需要下载 {0} 内容", GameUtility.FormatSize(totalSize));
                MessageBox.Show("提示", tips, "下载", "退出").callBack += delegate (bool sure)
                {
                    if (sure)
                    {
                        _downloader.StartDownload();
                        _status = UStatus.Downloading;
                    }
                    else Quit();
                };
            }
            else OnComplete();
        }
        private void OnUpdate(long progress, long size, float speed)
        {
            OnMessage(Utility.Text.Format("下载中...{0}/{1}, 速度：{2}", GameUtility.FormatSize(progress), GameUtility.FormatSize(size), GameUtility.FormatSize(speed, true)));
            OnProgress(progress * 1f / size);
        }
        private void OnComplete()
        {
            OnProgress(1);
            OnMessage("更新完成");
            OnVersion(Versions.localVersion.version);
            StartCoroutine(StartGame());
        }
        private IEnumerator StartGame()
        {
            OnMessage("正在初始化");
            var init = AssetsManger.Instance.Initialize();
            yield return init;
            if (string.IsNullOrEmpty(init.error))
            {
                init.Release();
                OnProgress(0);
                OnMessage("加载游戏场景");
                
                ScenesManager.Instance.UnloadSceneAsync(Scenes.Updater.ToString());

                var scene = ScenesManager.Instance.LoadSceneAsync(Scenes.Login.ToString());
                while (!scene.isDone)
                {
                    OnProgress(scene.progress);
                    yield return null;
                }
            }
            else
            {
                init.Release();
                var mb = MessageBox.Show("提示", "初始化失败");
                yield return mb;
                Quit();
            }
        }
        private string GetFilePath(string path)
        {
            return GameUtility.Assets.GetDataPath(path);
        }
        private void Restart()
        {
            switch (_status)
            {
                case UStatus.Downloading:
                    _downloader?.Restart();
                    break;
                default:
                    StartUpdate();
                    break;
            }
        }
        private void Reset()
        {
            foreach (var download in _downloads) download.Dispose();
            _downloads.Clear();
            MessageBox.Dispose();
        }
        private void OnApplicationFocus(bool focus)
        {
            if (Application.isEditor) return;
            if (_netWorkChanged || _status == UStatus.Idle) return;
            if (focus)
            {
                MessageBox.CloseAll();
                Restart();
            }
            else
            {
                if (_status == UStatus.Downloading) _downloader.Stop();
            }
        }
        public void OnReachablityChanged(NetworkReachability reachability)
        {
            if (_status == UStatus.Idle) return;
            _netWorkChanged = true;
            if (_status == UStatus.Downloading) _downloader?.Stop();
            if (reachability == NetworkReachability.NotReachable)
            {
                MessageBox.Show("提示", "请检查网络连接状态", "重试", "退出").callBack += sure =>
                  {
                      if (sure)
                      {
                          Restart();
                          _netWorkChanged = false;
                      }
                      else Quit();
                  };
            }
            else
            {
                MessageBox.CloseAll();
                Restart();
                _netWorkChanged = false;
            }
        }
        private void OnError(bool sure)
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
            // if (sure) StartUpdate(); else Quit();
        }
        private void OnDestroy()
        {
            Reset();
        }
        private void Quit()
        {
            if (Application.isEditor)
                UnityEditor.EditorApplication.isPlaying = false;
            else
                Application.Quit();
        }
    }
}
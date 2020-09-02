using UnityEngine;

namespace B2Framework
{
    public partial class Game : MonoBehaviour
    {
        protected float _pauseSpeed;
        protected virtual void Awake()
        {
            if (Instance != null)
            {
                if (Instance != this) DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            Application.lowMemory += OnLowMemory;
            Initialize();
        }
        protected virtual void Start()
        {
            // 从这里启动游戏
            ScenesManager.Instance.LoadSceneAsync(Scenes.Updater.ToString());
        }
        void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 100, 50), ""))
            {
                var size = string.Empty;
                var go = false;
                // GC
                Profiler.Sampling(() =>
                {
                    // for (var i = 0; i < 100; i++)

                });

                // 耗时
                Profiler.Watch(() =>
                {
                    for (var i = 0; i < 100; i++)
                    {
                        var ll = Game.luaMgr;
                    }
                });
                Log.Debug(size);
                Log.Debug(go);
            }
        }
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public virtual void Pause()
        {
            if (isGamePaused) return;

            _pauseSpeed = gameSpeed;
            gameSpeed = 0f;
        }
        /// <summary>
        /// 恢复暂停
        /// </summary>
        public virtual void Resume()
        {
            if (!isGamePaused) return;

            gameSpeed = _pauseSpeed;
        }
        /// <summary>
        /// 恢复游戏正常速度
        /// </summary>
        public virtual void NormalSpeed()
        {
            if (m_GameSpeed != 1f) gameSpeed = 1f;
        }
        /// <summary>
        /// 更新游戏逻辑
        /// </summary>
        protected virtual void Update()
        {
            // TODO:更新游戏逻辑
        }
        /// <summary>
        /// 重启游戏
        /// </summary>
        public virtual void Restart()
        {
            Dispose();
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
        }
        /// <summary>
        /// 释放
        /// </summary>
        public virtual void Dispose()
        {
            sceneMgr?.Dispose();
            luaMgr?.Dispose();
            netMgr?.Dispose();

            Destroy(gameObject);
        }
        protected virtual void OnDestroy()
        {

        }
        /// <summary>
        /// 游戏退出时调用
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            Application.lowMemory -= OnLowMemory;
            StopAllCoroutines();
        }
        /// <summary>
        /// 低内存时的处理逻辑
        /// </summary>
        protected virtual void OnLowMemory()
        {
            Log.Warning("Low memory reported...");
            // TODO:释放对象池
            // TODO:释放加载的资源
            AssetsManger.UnloadUnusedAssets();

            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    }
}
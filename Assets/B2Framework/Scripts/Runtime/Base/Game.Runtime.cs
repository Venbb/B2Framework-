using UnityEngine;

namespace B2Framework
{
    public partial class Game
    {
        private float _pauseSpeed;
        void Awake()
        {
            instance = this;

            Init();
            Application.lowMemory += OnLowMemory;
            Log.Debug("llllll");
        }
        void Start()
        {
            // 从这里启动游戏
            ScenesManager.Instance.LoadSceneAsync(Scenes.Updater.ToString());
        }
        // void OnGUI()
        // {
        //     if (GUI.Button(new Rect(100, 100, 100, 50), ""))
        //     {
        //         var size = string.Empty;
        //         GameUtility.Sampling(() =>
        //         {
        //             for (var i = 0; i < 100; i++)
        //                 size = GameUtility.FormatSize(10000);
        //         });
        //         Log.Debug(size);
        //     }
        // }
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void Pause()
        {
            if (isGamePaused) return;

            _pauseSpeed = gameSpeed;
            gameSpeed = 0f;
        }
        /// <summary>
        /// 恢复暂停
        /// </summary>
        public void Resume()
        {
            if (!isGamePaused) return;

            gameSpeed = _pauseSpeed;
        }
        /// <summary>
        /// 恢复游戏正常速度
        /// </summary>
        public void NormalSpeed()
        {
            if (m_GameSpeed != 1f) gameSpeed = 1f;
        }
        /// <summary>
        /// 更新游戏逻辑
        /// </summary>
        void Update()
        {
            // TODO:更新游戏逻辑
        }
        /// <summary>
        /// 重启游戏
        /// </summary>
        public void Restart()
        {
            Destroy(gameObject);

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
        }
        /// <summary>
        /// 退出游戏
        /// </summary>
        /// <param name="restart"></param>
        public void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        void OnDestroy()
        {
            Game.SceneMgr?.Dispose();
            instance = null;
        }
        /// <summary>
        /// 游戏退出时调用
        /// </summary>
        void OnApplicationQuit()
        {
            Application.lowMemory -= OnLowMemory;
            StopAllCoroutines();
        }
        /// <summary>
        /// 低内存时的处理逻辑
        /// </summary>
        private void OnLowMemory()
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
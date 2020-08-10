using UnityEngine;

namespace B2Framework
{
    public partial class Game : MonoBehaviour
    {
        private float _pauseSpeed;
        private void Awake()
        {
            var settings = Resources.Load<Settings>("GameSettings");

            frameRate = settings.frameRate;
            gameSpeed = settings.gameSpeed;
            runInBackground = settings.runInBackground;
            neverSleep = settings.neverSleep;

            // Application.targetFrameRate = _frameRate;
            // Time.timeScale = _gameSpeed;
            // Application.runInBackground = _runInBackground;
            // Screen.sleepTimeout = _neverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;

            Utility.Assets.runtimeMode = settings.runtimeMode;

            Application.lowMemory += OnLowMemory;
        }
        void Start()
        {
            // 从这里启动游戏
            The.SceneMgr.LoadSceneAsync(Scenes.Updater.ToString());
        }
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
            if (_gameSpeed != 1f) gameSpeed = 1f;
        }
        /// <summary>
        /// 更新游戏逻辑
        /// </summary>
        void Update()
        {
            // TODO:更新游戏逻辑
        }
        /// <summary>
        /// 退出游戏
        /// </summary>
        public void Quit()
        {
            Destroy(gameObject);
        }
        /// <summary>
        /// 被销毁时的处理
        /// </summary>
        private void OnDestroy()
        {

        }
        /// <summary>
        /// 游戏退出时调用
        /// </summary>
        private void OnApplicationQuit()
        {
            Application.lowMemory -= OnLowMemory;
            StopAllCoroutines();
        }
        /// <summary>
        /// 低内存时的处理逻辑
        /// </summary>
        private void OnLowMemory()
        {
            Debug.Log("Low memory reported...");
            // TODO:释放对象池
            // TODO:释放加载的资源
            Assets.UnloadUnusedAssets();
        }
    }
}

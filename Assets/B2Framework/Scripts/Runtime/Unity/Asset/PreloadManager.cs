using System;
using System.Collections.Generic;
using UnityEngine;

namespace B2Framework.Unity
{
    public interface ILoader
    {
        void Load();
        void Unload();
        bool IsDone();
        float Progress();
    }

    public class AssetLoader : ILoader
    {
        AssetRequest request;

        string url;
        Type type;

        public AssetLoader(string assetPath, Type assetType)
        {
            url = assetPath;
            type = assetType;
        }

        public bool IsDone()
        {
            if (request == null)
            {
                return false;
            }
            return request.isDone;
        }

        public void Load()
        {
            request = Assets.LoadAssetAsync(url, type);
        }

        public float Progress()
        {
            if (request == null)
            {
                return 0;
            }
            return request.progress;
        }

        public void Unload()
        {
            request.Release();
            request = null;
        }

        public UnityEngine.Object GetAsset() { return request.asset; }
    }

    public class SceneLoader : ILoader
    {
        SceneAssetRequest request;
        string url;
        bool isAdditive;

        public SceneLoader(string assetPath, bool additive)
        {
            url = assetPath;
            isAdditive = additive;
        }

        public bool IsDone()
        {
            return request.isDone;
        }

        public void Load()
        {
            request = Assets.LoadSceneAsync(url, isAdditive);
        }

        public float Progress()
        {
            return request.progress;
        }

        public void Unload()
        {
            request.Unload();
            request = null;
        }
    }

    public class GameObjectLoader : ILoader
    {
        public string url;
        AssetRequest request;
        public GameObject gameObject { get; private set; }
        public GameObjectLoader(string path)
        {
            url = path;
        }

        public bool IsDone()
        {
            return request.isDone;
        }

        public void Load()
        {
            request = Assets.LoadAssetAsync(url, typeof(GameObject));
            request.completed += OnAssetLoaded;
        }

        private void OnAssetLoaded(AssetRequest obj)
        {
            if (!string.IsNullOrEmpty(obj.error))
            {
                Log.Error(obj.error);
                return;
            }
            gameObject = GameObject.Instantiate((GameObject)obj.asset);
        }

        public float Progress()
        {
            return request.progress;
        }

        public void Unload()
        {
            if (gameObject != null)
            {
                GameObject.Destroy(gameObject);
                gameObject = null;
            }
            request.Release();
            request = null;
        }
    }

    public class PreloadManager : MonoSingleton<PreloadManager>, IDisposable
    {
        List<ILoader> loaders = new List<ILoader>();
        private bool loading = false;

        Dictionary<string, AssetLoader> assets = new Dictionary<string, AssetLoader>();
        Dictionary<string, GameObjectLoader> gameObjects = new Dictionary<string, GameObjectLoader>();

        private void AddLoader(ILoader loader)
        {
            loaders.Add(loader);
        }

        public AssetLoader AddAsset(string path, Type type)
        {
            var loader = new AssetLoader(path, type);
            AddLoader(loader);
            assets.Add(path, loader);
            return loader;
        }

        public SceneLoader AddScene(string path, bool additive)
        {
            var loader = new SceneLoader(path, additive);
            AddLoader(loader);
            return loader;
        }

        public GameObjectLoader AddGameObject(string path)
        {
            var loader = new GameObjectLoader(path);
            AddLoader(loader);
            gameObjects.Add(path, loader);
            return loader;
        }

        public GameObject GetGameObject(string path)
        {
            GameObjectLoader loader;
            if (gameObjects.TryGetValue(path, out loader))
            {
                return loader.gameObject;
            }
            return null;
        }

        public T GetAsset<T>(string path) where T : UnityEngine.Object
        {
            AssetLoader loader;
            if (assets.TryGetValue(path, out loader))
            {
                return (T)loader.GetAsset();
            }
            return null;
        }

        public void StartLoad()
        {
            loaders.ForEach((o) => o.Load());
            Loading = true;
        }

        public void Dispose()
        {
            loaders.ForEach((o) => o.Unload());
            loaders.Clear();

            assets.Clear();
            gameObjects.Clear();
        }

        readonly Predicate<ILoader> matchIsDone = delegate (ILoader o) { return o.IsDone(); };

        public bool Loading { get { return loading; } private set { loading = value; } }

        public bool IsDone()
        {
            return loaders.TrueForAll(matchIsDone);
        }

        public float Progress()
        {
            float progress = 0;

            Action<ILoader> action = delegate (ILoader o) { progress += o.Progress(); };

            loaders.ForEach(action);

            return progress / loaders.Count;
        }

        private void Update()
        {
            if (Loading)
            {
                if (IsDone())
                {
                    Loading = false;
                }
            }
        }
    }
}
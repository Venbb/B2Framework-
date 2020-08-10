using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace B2Framework
{
    /// <summary>
    /// 资源加载基类
    /// </summary>
    public class AssetRequest : Reference, IEnumerator
    {
        public Type assetType;
        public string url;
        public LoadState loadState { get; protected set; }
        public virtual bool isDone { get { return true; } }
        public virtual float progress { get { return 1; } }
        public virtual string error { get; protected set; }
        public string text { get; protected set; }
        public byte[] bytes { get; protected set; }
        public Object asset { get; internal set; }
        public Action<AssetRequest> completed;
        public AssetRequest()
        {
            asset = null;
            loadState = LoadState.Init;
        }
        internal virtual void Load()
        {
            // 在Editor模式下指定加载方式：AssetDatabase.LoadAssetAtPath或者Resources.Load
            if (!Utility.Assets.runtimeMode && Utility.Assets.loadHander != null)
                asset = Utility.Assets.loadHander(url, assetType);                
            if (asset == null) error = "error! file not exist:" + url;
        }
        internal bool Update()
        {
            if (!isDone) return true;
            if (completed == null) return false;
            try
            {
                completed.Invoke(this);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            completed = null;
            return false;
        }
        internal virtual void Unload()
        {
            if (asset == null) return;
            if (!Utility.Assets.runtimeMode)
            {
                if (!(asset is GameObject))
                    Resources.UnloadAsset(asset);
            }
            asset = null;
        }
        #region IEnumerator implementation
        public object Current { get { return null; } }
        public bool MoveNext()
        {
            return !isDone;
        }
        public void Reset() { }
        #endregion
    }
}
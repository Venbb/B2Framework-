using UnityEngine;
using UnityEngine.Networking;

namespace B2Framework.Unity
{
    public class WebAssetRequest : AssetRequest
    {
        private UnityWebRequest _request;
        public override bool isDone
        {
            get
            {
                if (loadState == LoadState.Init) return false;
                if (loadState == LoadState.Loaded) return true;
                if (loadState == LoadState.LoadAsset)
                {
                    if (_request == null || !string.IsNullOrEmpty(_request.error)) return true;
                    if (_request.isDone)
                    {
                        if (assetType != typeof(Texture2D))
                        {
                            if (assetType != typeof(TextAsset))
                            {
                                if (assetType != typeof(AudioClip))
                                    bytes = _request.downloadHandler.data;
                                else
                                    asset = DownloadHandlerAudioClip.GetContent(_request);
                            }
                            else
                            {
                                text = _request.downloadHandler.text;
                            }
                        }
                        else
                        {
                            asset = DownloadHandlerTexture.GetContent(_request);
                        }
                        loadState = LoadState.Loaded;
                        return true;
                    }
                    return false;
                }

                return true;
            }
        }
        public override string error
        {
            get { return _request.error; }
        }
        public override float progress
        {
            get { return _request.downloadProgress; }
        }
        internal override void Load()
        {
            if (assetType == typeof(AudioClip))
            {
                _request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
            }
            else if (assetType == typeof(Texture2D))
            {
                _request = UnityWebRequestTexture.GetTexture(url);
            }
            else
            {
                _request = new UnityWebRequest(url);
                _request.downloadHandler = new DownloadHandlerBuffer();
            }
            _request.SendWebRequest();
            loadState = LoadState.LoadAsset;
        }

        internal override void Unload()
        {
            if (asset != null)
            {
                Object.Destroy(asset);
                asset = null;
            }
            if (_request != null) _request.Dispose();

            bytes = null;
            text = null;
        }
    }
}
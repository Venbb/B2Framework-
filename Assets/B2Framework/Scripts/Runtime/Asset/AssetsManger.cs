using System;

namespace B2Framework
{
    public partial class AssetsManger : MonoSingleton<AssetsManger>, IDisposable
    {
        public ManifestRequest Initialize()
        {
            Log.Debug("Initialize with: runtimeMode={0}\nbasePath：{1}\nupdatePath={2}",
             GameUtility.Assets.runtimeMode, GameUtility.Assets.basePath, GameUtility.Assets.dataPath);

            var request = new ManifestRequest { url = GameUtility.Assets.manifestFilePath };
            AddAssetRequest(request);
            return request;
        }
        private void Update()
        {
            UpdateAssets();
            UpdateBundles();
        }
        public void Dispose()
        {
            Clear();
        }
    }
}
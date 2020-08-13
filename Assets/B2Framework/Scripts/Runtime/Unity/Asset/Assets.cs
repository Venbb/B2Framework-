﻿namespace B2Framework.Unity
{
    public partial class Assets : MonoSingleton<Assets>
    {
        public ManifestRequest Initialize()
        {
            Log.Debug(string.Format("Initialize with: runtimeMode={0}\nbasePath：{1}\nupdatePath={2}",
             GameUtility.Assets.runtimeMode, GameUtility.Assets.basePath, GameUtility.Assets.dataPath));

            var request = new ManifestRequest { url = GameUtility.Assets.manifestFilePath };
            AddAssetRequest(request);
            return request;
        }
        private void Update()
        {
            UpdateAssets();
            UpdateBundles();
        }
        public override void Dispose()
        {
           Clear();
        }
    }
}
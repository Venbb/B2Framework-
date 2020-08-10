namespace B2Framework
{
    public partial class Assets : MonoSingleton<Assets>
    {
        public ManifestRequest Initialize()
        {
            Debug.Log(string.Format("Initialize with: runtimeMode={0}\nbasePath：{1}\nupdatePath={2}",
             Utility.Assets.runtimeMode, Utility.Assets.basePath, Utility.Assets.dataPath));

            var request = new ManifestRequest { url = Utility.Assets.manifestFilePath };
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
using UnityEngine;

namespace B2Framework
{
    [System.Serializable]
    public class AssetInfo
    {
        public int bundle;
        public int dir;
        public string fileName;
    }
    [System.Serializable]
    public class BundleInfo
    {
        public string bundleName;
        public int[] deps;
        public string hash;
        public long len;
        public string crc;
    }
    public class BundleManifest : ScriptableObject
    {
        public string[] activeVariants = new string[0];
        public string[] dirs = new string[0];
        public BundleInfo[] bundles = new BundleInfo[0];
        public AssetInfo[] assets = new AssetInfo[0];
    }
}
using UnityEditor;

namespace B2Framework.Editor
{
    public static partial class MenuItems
    {
        [MenuItem("B2Framework/Build/Build All")]
        /// <summary>
        /// 一键打包
        /// </summary>
        static void BuildAll()
        {
            BuildHelper.BuildPlayer();
        }

        [MenuItem("B2Framework/Build/Build Player")]
        /// <summary>
        /// 一键出包
        /// </summary>
        static void BuildPlayer()
        {
            BuildHelper.BuildPlayer();
        }

        [MenuItem("B2Framework/Build/Build AssetBundles")]
        /// <summary>
        /// 一键构建AssetBundles
        /// </summary>
        static void BuildAssetBundles()
        {
            BuildHelper.BuildAssetBundles();
        }

        [MenuItem("Assets/Group AssetBundle/Filename")]
        private static void GroupByFilename()
        {
           BuildHelper.GroupAssets(BundleNameBy.Filename);
        }

        [MenuItem("Assets/Group AssetBundle/Directory")]
        private static void GroupByDirectory()
        {
            BuildHelper.GroupAssets(BundleNameBy.Directory);
        }

        [MenuItem("Assets/Group AssetBundle/Custom")]
        private static void GroupByCustom()
        {
            BuildHelper.GroupAssets(BundleNameBy.Custom);
        }
    }
}
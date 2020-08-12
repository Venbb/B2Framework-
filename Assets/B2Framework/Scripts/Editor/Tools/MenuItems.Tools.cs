using B2Framework.Unity;
using UnityEditor;
using UnityEngine;

namespace B2Framework.Editor
{
    public static partial class MenuItems
    {
        [MenuItem("B2Framework/ExplorerFolder/DataPath")]
        static void OpenDataPath()
        {
            EditorUtility.ExplorerFolder(Application.dataPath);
        }

        [MenuItem("B2Framework/ExplorerFolder/PersistentDataPath")]
        static void OpenPersistentDataPath()
        {
            EditorUtility.ExplorerFolder(Application.persistentDataPath);
        }

        [MenuItem("B2Framework/ExplorerFolder/StreamingAssetsPath")]
        static void OpenStreamingAssetsPath()
        {
            EditorUtility.ExplorerFolder(Application.streamingAssetsPath);
        }
        [MenuItem("B2Framework/ExplorerFolder/AssetBundlesPath")]
        static void OpenAssetBundlesPath()
        {
            EditorUtility.ExplorerFolder(AppConst.ASSETBUNDLES);
        }
    }
}
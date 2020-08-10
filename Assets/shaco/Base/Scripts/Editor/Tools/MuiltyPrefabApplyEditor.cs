using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class MuiltyPrefabApplyEditor
    {
        [MenuItem("GameObject/shaco/ApplyPrefabs", false, (int)ToolsGlobalDefine.HierachyMenuPriority.MUILTY_PREFABS_APPLY)]
        public static void ApplyPrefabsHierachy()
        {
            ApplyPrefabs();
        }

        [MenuItem("GameObject/shaco/ApplyPrefabs", true, (int)ToolsGlobalDefine.HierachyMenuPriority.MUILTY_PREFABS_APPLY)]
        public static bool ApplyPrefabsHierachyValid()
        {
            return !Selection.gameObjects.IsNullOrEmpty();
        }

        [MenuItem("shaco/Other/ApplyPrefabs %#&a", false, (int)ToolsGlobalDefine.MenuPriority.Other.MUILTY_PREFABS_APPLY)]
        public static void ApplyPrefabs()
        {
            for (int i = Selection.gameObjects.Length - 1; i >= 0; --i)
            {
                ApplyPrefab(Selection.gameObjects[i]);
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("shaco/Other/ApplyPrefabs %#&a", true, (int)ToolsGlobalDefine.MenuPriority.Other.MUILTY_PREFABS_APPLY)]
        public static bool ApplyPrefabsValid()
        {
            if (Selection.gameObjects.IsNullOrEmpty())
                return false;

            bool hasNotPrefabGameObject = false;

            for (int i = Selection.gameObjects.Length - 1; i >= 0; --i)
            {
                if (!IsPrefab(Selection.gameObjects[i]))
                {
                    hasNotPrefabGameObject = true;
                    break;
                }
            }
            return !hasNotPrefabGameObject;
        }

        //执行prefab的apply方法
        static public void ApplyPrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            if (!IsPrefab(prefab))
            {
                return;
            }

            //这里必须获取到prefab实例的根节点，否则ReplacePrefab保存不了
            GameObject prefabGo = GetPrefabInstanceParent(prefab);
            UnityEngine.Object prefabAsset = null;
            if (prefabGo != null)
            {
#if UNITY_2018_1_OR_NEWER
                prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(prefabGo);
#else
                prefabAsset = PrefabUtility.GetPrefabParent(prefabGo);
#endif

                //如果为固定资产prefab则需要调用SavePrefabAsset
                if (null == prefabAsset)
                {
                    PrefabUtility.SavePrefabAsset(prefabGo);
                    return;
                }

#if UNITY_2018_1_OR_NEWER
                //获取当前prefab编辑场景
                var currentPrefabState = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                if (null != currentPrefabState)
                {
                    //如果为实例化prefab直接使用prefab编辑器场景的保存
                    PrefabUtility.SaveAsPrefabAsset(currentPrefabState.prefabContentsRoot, currentPrefabState.prefabAssetPath);
                    currentPrefabState.ClearDirtiness();
                    return;
                }

                PrefabUtility.UnpackPrefabInstance(prefabGo, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                PrefabUtility.SaveAsPrefabAssetAndConnect(prefabGo, AssetDatabase.GetAssetPath(prefabAsset), InteractionMode.AutomatedAction);
#else
                PrefabUtility.ReplacePrefab(prefabGo, prefabAsset, ReplacePrefabOptions.ConnectToPrefab);
#endif
            }
        }

        //遍历获取prefab节点所在的根prefab节点
        static private GameObject GetPrefabInstanceParent(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            if (!IsPrefab(go))
                return null;

            if (go.transform.parent == null)
                return go;

            if (!IsPrefab(go.transform.parent.gameObject))
                return go;

            return GetPrefabInstanceParent(go.transform.parent.gameObject);
        }

        static private bool IsPrefab(GameObject target)
        {
#if UNITY_2018_1_OR_NEWER
            var pType = PrefabUtility.GetPrefabAssetType(target);
            return pType != PrefabAssetType.NotAPrefab;
#else
            var pType = PrefabUtility.GetPrefabType(target);
            return pType == PrefabType.PrefabInstance;
#endif
        }
    }
}
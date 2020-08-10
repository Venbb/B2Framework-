using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class ObjectPoolPreviewWindow : EditorWindow
	{
        static public bool isOpendWindow = false;

        private shaco.Instance.Editor.TreeView.WindowSplitter _dragLineSeparatorHorizontal = new shaco.Instance.Editor.TreeView.WindowSplitter();
        private shaco.Base.IObjectPool _currentObjectPool = null;
        private string _searchName = string.Empty;
        private Vector2 _scrollPosition = Vector2.zero;

        [MenuItem("shaco/Viewer/ObjectPoolPreview " + ToolsGlobalDefine.MenuPriority.ViewerShortcutKeys.OBJECT_POOL, false, (int)ToolsGlobalDefine.MenuPriority.Viewer.OBJECT_POOL)]
        static void Open()
        {
            EditorHelper.GetWindow<ObjectPoolPreviewWindow>(null, true, "ObjectPoolPreview");
        }

        void OnEnable()
        {
            EditorHelper.GetWindow<ObjectPoolPreviewWindow>(this, true, "ObjectPoolPreview").Init();
        }

        private void Init()
        {
            _currentObjectPool = shaco.GameHelper.objectpool;
            isOpendWindow = true;
            shaco.GameHelper.objectpool.isOpendStackLocation = true;
        }

        void OnDestroy()
        {
            isOpendWindow = false;
            shaco.GameHelper.objectpool.isOpendStackLocation = false;
        }

		void OnGUI()
		{
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            {
                _dragLineSeparatorHorizontal.SetSplitWindow(new Rect(0, 40, this.position.width, this.position.height), 0.5f, 0.5f);

                var objectPoolTypeName = _currentObjectPool.GetType().FullName;
                var newObjectPoolTypeName = EditorGUILayout.TextField("Object pool", objectPoolTypeName);
                if (newObjectPoolTypeName != objectPoolTypeName)
                {
                    GUI.FocusControl(string.Empty);
                }
                DrawSearchField();

                if (null != _currentObjectPool)
                {
                    _dragLineSeparatorHorizontal.BeginLayout(true);
                    {
                        if (GUILayoutHelper.DrawHeader("Instantiate Pool", "Instantiate Pool", true))
                        {
                            DrawObjectInstantiatePool(_currentObjectPool);
                        }
                    }
                    _dragLineSeparatorHorizontal.EndLayout();

                    _dragLineSeparatorHorizontal.BeginLayout();
                    {
                        if (GUILayoutHelper.DrawHeader("Cache Pool", "Cache Pool", true))
                        {
                            DrawObjectUnusedPool(_currentObjectPool);
                        }
                    }
                    _dragLineSeparatorHorizontal.EndLayout();
                }
                this.Repaint();
            }
            GUILayout.EndScrollView();
        }

        private void DrawSearchField()
        {
            _searchName = GUILayoutHelper.SearchField(_searchName);
        }

        private void DrawObjectInstantiatePool(shaco.Base.IObjectPool objectPool)
        {
            DrawObjectPoolItems(objectPool.ForeachInstantiatePool);
        }

        private void DrawObjectUnusedPool(shaco.Base.IObjectPool objectPool)
        {
            DrawObjectPoolItems(objectPool.ForeacUnusedPool);
        }

        private void DrawObjectPoolItems(System.Action<System.Func<string, List<shaco.Base.PoolDataInfo>, bool>> foreachFunction)
        {
            var searchNameLower = !string.IsNullOrEmpty(_searchName) ? _searchName.ToLower() : string.Empty;
            foreachFunction((string key, List<shaco.Base.PoolDataInfo> value) =>
            {
                if (!string.IsNullOrEmpty(_searchName) && !key.ToLower().Contains(searchNameLower))
                {
                    return true;
                }

                try
                {
                    GUILayout.BeginVertical("box");
                    {
                        if (GUILayoutHelper.DrawHeader("Key: " + key + "[Count: " + value.Count + "]", key, false))
                        {
                            for (int i = 0; i < value.Count; ++i)
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    EditorGUI.BeginDisabledGroup(true);
                                    {
                                        var valueType = value[i].value.GetType();
                                        GUILayoutHelper.DrawValue(string.Format("Item{0}", i), value[i].value, valueType);
                                        GUILayout.Label(valueType.ToString());
                                    }
                                    EditorGUI.EndDisabledGroup();
                                    DrawStackLocationButtons(value[i]);
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                catch
                {
                    //可能在foreach的时候，其他地方有子线程在对内存池操作，所以这里GUILayout的报错可以忽略不记
                }
                return true;
            });
        }

        private void DrawStackLocationButtons(shaco.Base.PoolDataInfo poolData)
        {
            if (null != poolData.stackLocationInstantiate && poolData.stackLocationInstantiate.HasStack() && GUILayout.Button("Instantiate" + poolData.stackLocationInstantiate.GetPerformanceDescription()))
            {
                shaco.Log.Info(poolData.stackLocationInstantiate.GetTotalStackInformation());
                EditorHelper.OpenAsset(poolData.stackLocationInstantiate.GetStackInformation(), poolData.stackLocationInstantiate.GetStackLine());
            }

            if (null != poolData.stackLocationRecycling && poolData.stackLocationRecycling.HasStack() && GUILayout.Button("Recycling" + poolData.stackLocationRecycling.GetPerformanceDescription()))
            {
                shaco.Log.Info(poolData.stackLocationRecycling.GetTotalStackInformation());
                EditorHelper.OpenAsset(poolData.stackLocationRecycling.GetStackInformation(), poolData.stackLocationRecycling.GetStackLine());
            }
        }
	}
}

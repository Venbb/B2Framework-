using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
	/// <summary>
	/// 用于查看和编辑shaco框架中缓存的所有assetbundle对象详细信息
	/// </summary>
    public class AssetbundleCachePreviewWindow : EditorWindow
    {
		private class AssetbundleDependencies
		{
			public class AssetInfo
			{
				public Object asset = null;
				public long memoryUsed = 0;
			}
            //当前搜集到引用到该资源的对象
            public Object assetTarget = null;
			//对象路径
            public string assetTargetPath = string.Empty;
			//ab包中读取出来的资源对象
			public List<AssetInfo> assetsInfo = new List<AssetInfo>();
        }

		private class AssetBundlesDependencies
		{
			public bool isUncapturedDependencie = false;
			public Dictionary<string, AssetbundleDependencies> assetbundleDependencies = new Dictionary<string, AssetbundleDependencies>();
		}

        //最大显示ab包信息数量　
        private readonly int MAX_SHOW_ASSETBUNDLE_COUNT = 50;

        //查询到的ab包引用关系表
        private Dictionary<string, AssetBundlesDependencies> _assetbundlesDependencies = new Dictionary<string, AssetBundlesDependencies>();

		//当前搜索的ab包名字
		private string _currentSearchAssetbundleName = string.Empty;

		//总计ab包使用内存大小
		private long _totalAssetbundleUsedMemory = 0;

		//滚动位置
		private Vector2 _scrollViewPosition = Vector2.zero;

        [MenuItem("shaco/Viewer/AssetbundleCachePreview " + ToolsGlobalDefine.MenuPriority.ViewerShortcutKeys.ASSETBUNDLE_CACHE, false, (int)ToolsGlobalDefine.MenuPriority.Viewer.ASSETBUNDLE_CACHE)]
        static void OpenAssetbundleCachePreviewWindow()
        {
            shacoEditor.EditorHelper.GetWindow<AssetbundleCachePreviewWindow>(null, true, "AssetbundleCachePreview");
        }

		void OnEnable()
		{
            shacoEditor.EditorHelper.GetWindow<AssetbundleCachePreviewWindow>(this, true, "AssetbundleCachePreview").Init();
		}

		void OnGUI()
		{
			this.Repaint();

            _scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition);
			{
                DrawSearchFilter();
				GUILayout.BeginHorizontal();
				{
					DrawCollectAllAssetbundleDependencies();
                    DrawUnloadUnusedAssetbundleButton();
				}
				GUILayout.EndHorizontal();
                DrawAllCachedAssetBundles();
			}
			GUILayout.EndScrollView();
		}

        private void Init()
        {
			this.name = "Uncaptured dependencie";
        }

		/// <summary>
		/// 绘制搜索筛选器
		/// </summary>
		private void DrawSearchFilter()
		{
			GUILayout.BeginHorizontal();
			{
                //缓存的ab包数量
                var allCachedAssetBundles = shaco.GameHelper.resCache.GetAllCachedAssetbundle();
				if (_totalAssetbundleUsedMemory > 0)
					GUILayout.Label("Count: " + allCachedAssetBundles.Count + "\nTotal Used Memory: " + shaco.Base.FileHelper.GetFileSizeFormatString(_totalAssetbundleUsedMemory));
				else
                    GUILayout.Label("Count: " + allCachedAssetBundles.Count);

                _currentSearchAssetbundleName = GUILayoutHelper.SearchField(_currentSearchAssetbundleName, GUILayout.Width(this.position.width / 3 * 1));
			}
			GUILayout.EndHorizontal();
		}

		/// <summary>
		/// 绘制收集所有ab包引用信息按钮
		/// </summary>
		private void DrawCollectAllAssetbundleDependencies()
		{
			if (GUILayout.Button("Collect All Assetbundle Dependencies"))
            {
                var allCachedAssetBundles = shaco.GameHelper.resCache.GetAllCachedAssetbundle();
				_assetbundlesDependencies.Clear();
                _totalAssetbundleUsedMemory = 0;
				var allObjectsDependencies = CollectAllSceneObjectDepencies();
                foreach (var iter in allCachedAssetBundles)
                {
                    CollectAssetBundleDependencies(allObjectsDependencies, iter.Key, iter.Value);
                }
            }
		}

		/// <summary>
		/// 绘制释放不再使用的ab包按钮
		/// </summary>
		private void DrawUnloadUnusedAssetbundleButton()
		{
			if (GUILayout.Button("Unload Unused"))
			{
				shaco.GameHelper.resCache.UnloadUnusedDatas(false);
			}
		}

		/// <summary>
		/// 绘制所有缓存的ab包信息
		/// </summary>
		private void DrawAllCachedAssetBundles()
		{
			var allCachedAssetBundles = shaco.GameHelper.resCache.GetAllCachedAssetbundle();

			//缓存的ab包信息
			int index = 0;
			var searchNameLower = _currentSearchAssetbundleName.ToLower();
			foreach (var iter in allCachedAssetBundles)
			{
				if (!string.IsNullOrEmpty(searchNameLower) && !iter.Key.ToLower().Contains(searchNameLower))
                    continue;
					
				GUILayout.BeginVertical("box");
				{
                    DrawCachedAssetbundle(iter.Key, iter.Value);
                }
				GUILayout.EndVertical();

                if (++index >= MAX_SHOW_ASSETBUNDLE_COUNT)
                    break;
			}
		}

        /// <summary>
        /// 绘制单个缓存的ab包信息
        /// <param name="key">ab包文件相对路径</param>
        /// <param name="dataCache">ab包信息</param>
        /// </summary>
        private void DrawCachedAssetbundle(string key, shaco.DataCache dataCache)
		{
            AssetBundlesDependencies assetbundleDependenciesTmp = null;
            _assetbundlesDependencies.TryGetValue(key, out assetbundleDependenciesTmp);

            var oldColor = GUI.color;
            if (null != assetbundleDependenciesTmp && assetbundleDependenciesTmp.isUncapturedDependencie)
                GUI.color = Color.yellow;

			if (GUILayoutHelper.DrawHeader(key + "[RefCount: " + dataCache.referenceCount + "]", key, false))
			{
				GUILayout.BeginHorizontal();
				{
					if (dataCache.stackLocationCreate.HasStack() && GUILayout.Button("Location(Create)" + dataCache.stackLocationCreate.GetPerformanceDescription()))
					{
						shaco.Log.Info(dataCache.stackLocationCreate.GetTotalStackInformation());
                        EditorHelper.OpenAsset(dataCache.stackLocationCreate.GetStackInformation(), dataCache.stackLocationCreate.GetStackLine());
					}
                    if (dataCache.stackLocationRead.HasStack() && GUILayout.Button("Location(Read)" + dataCache.stackLocationRead.GetPerformanceDescription()))
					{
						shaco.Log.Info(dataCache.stackLocationRead.GetTotalStackInformation());
                        EditorHelper.OpenAsset(dataCache.stackLocationRead.GetStackInformation(), dataCache.stackLocationRead.GetStackLine());
                    }
					if (GUILayout.Button("Collect Dependencie"))
					{
                        _assetbundlesDependencies.Clear();
                        _totalAssetbundleUsedMemory = 0;
                        CollectAssetBundleDependencies(CollectAllSceneObjectDepencies(), key, dataCache);
                    }
				}
				GUILayout.EndHorizontal();

                DrawAssetbundleDependenciesLayout(assetbundleDependenciesTmp);
            }

            if (null != assetbundleDependenciesTmp && assetbundleDependenciesTmp.isUncapturedDependencie)
                GUI.color = oldColor;
		}

        /// <summary>
        /// 绘制当前收集了ab的引用信息表布局
        /// <param name="assetPath">资源路径</param>
        /// </summary>
        private void DrawAssetbundleDependenciesLayout(AssetBundlesDependencies assetbundleDependencies)
		{
			if (null == assetbundleDependencies)
				return;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                GUILayout.BeginVertical();
                {
                    DrawAssetbundleDependencies(assetbundleDependencies);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
		}

        /// <summary>
        /// 绘制当前收集了ab的引用信息表
        /// <param name="assetbundleDependencies">ab包引用信息</param>
        /// </summary>
        private void DrawAssetbundleDependencies(AssetBundlesDependencies assetbundleDependencies)
		{
			foreach (var iter in assetbundleDependencies.assetbundleDependencies)
			{
				System.Action drawHeaderCallBack = () =>
				{
                    EditorGUI.BeginDisabledGroup(true);
					{
                        EditorGUILayout.ObjectField(iter.Value.assetTarget, iter.Key.GetType(), true);
                    }
                    EditorGUI.EndDisabledGroup();
                };


				if (GUILayoutHelper.DrawHeader(iter.Value.assetTargetPath, iter.Value.assetTargetPath, drawHeaderCallBack))
				{
					for (int i = iter.Value.assetsInfo.Count - 1; i >= 0; --i)
					{
						var infoTmp = iter.Value.assetsInfo[i];
						EditorGUI.BeginDisabledGroup(true);
						{
							GUILayout.BeginHorizontal();
							{
								GUILayout.Space(15);
								EditorGUILayout.ObjectField(infoTmp.asset, typeof(Object), true, GUILayout.Width(Screen.width / 4));
								GUILayout.Label(infoTmp.asset.GetType().ToTypeString() + " " + shaco.Base.FileHelper.GetFileSizeFormatString(infoTmp.memoryUsed));
							}
							GUILayout.EndHorizontal();
						}
						EditorGUI.EndDisabledGroup();
					}
				}
			}
		}

		private Dictionary<Object, List<Object>> CollectAllSceneObjectDepencies()
		{
            var allObjectsInScenes = Resources.FindObjectsOfTypeAll(typeof(MonoBehaviour));
            var allObjectsDependencies = new Dictionary<Object, List<Object>>();

            //收集所有引用信息
            for (int i = 0; i < allObjectsInScenes.Length; ++i)
            {
                var objectInScene = allObjectsInScenes[i];

				//过滤在Project窗口中的对象
				if (EditorUtility.IsPersistent(objectInScene))
				{
					continue;
				}

                var listDependencies = shaco.Base.Utility.CollectDependenciesEx<Object>(objectInScene);
                if (!listDependencies.IsNullOrEmpty())
                {
                    for (int j = 0; j < listDependencies.Length; ++j)
                    {
                        List<Object> objectsTmp = null;
                        if (!allObjectsDependencies.TryGetValue(listDependencies[j], out objectsTmp))
                        {
                            objectsTmp = new List<Object>();
                            allObjectsDependencies.Add(listDependencies[j], objectsTmp);
                        }
                        objectsTmp.Add(objectInScene);
                    }
                }
            }
			return allObjectsDependencies;
		}

        /// <summary>
        /// 收集ab引用信息
        /// <param name="key">ab包文件相对路径</param>
        /// <param name="dataCache">ab包信息</param>
        /// </summary>
        private void CollectAssetBundleDependencies(Dictionary<Object, List<Object>> allObjectsDependencies, string key, shaco.DataCache dataCache)
		{
			AssetBundlesDependencies newAssetbundleDependencies = null;
            if (!_assetbundlesDependencies.TryGetValue(key, out newAssetbundleDependencies))
			{
				newAssetbundleDependencies = new AssetBundlesDependencies();
			}

			//查找收集的引用信息中是否包含ab包读取出来的资源
			if (dataCache.readedAssets.IsNullOrEmpty() && null != dataCache.hotUpdateDelMemory)
			{
                var mainAsset = dataCache.hotUpdateDelMemory.ReadMainAsset();
				if (null != mainAsset)
					dataCache.readedAssets.Add(mainAsset);
			}

			foreach (var iter in dataCache.readedAssets)
			{
				//找到当前场景在引用该资源的对象
				List<Object> objectsTmp = null;
				if (allObjectsDependencies.TryGetValue(iter, out objectsTmp))
				{
					for (int i = 0; i < objectsTmp.Count; ++i)
					{
                        newAssetbundleDependencies.isUncapturedDependencie = false;
                        AddAssetbundleDepenencies(newAssetbundleDependencies, objectsTmp[i], iter);
                    }
				}
				else 
				{
                    newAssetbundleDependencies.isUncapturedDependencie = true;
                    AddAssetbundleDepenencies(newAssetbundleDependencies, this, iter);
				}
			}

			//只添加查找到引用的有效数据
			if (newAssetbundleDependencies.assetbundleDependencies.Count > 0)
			{
                _assetbundlesDependencies.Add(key, newAssetbundleDependencies);
            }
		}

        /// <summary>
        /// 添加ab包引用信息记录
        /// <param name="assetbundleDependencies">ab包引用信息集合</param>
		/// <param name="target">引用了资源的对象</param>
        /// <param name="asset">资源对象</param>
        /// <return></return>
        /// </summary>
        private void AddAssetbundleDepenencies(AssetBundlesDependencies assetbundleDependencies, Object target, Object asset)
		{
			AssetbundleDependencies newInfo = null;
			var hierarchyPath = shaco.UnityHelper.GetObjectPathInHierarchy(target);
            if (!assetbundleDependencies.assetbundleDependencies.TryGetValue(hierarchyPath, out newInfo))
            {
                newInfo = new AssetbundleDependencies();
                assetbundleDependencies.assetbundleDependencies.Add(hierarchyPath, newInfo);
            }
			else
				return;

            newInfo.assetTarget = target;
            newInfo.assetTargetPath = hierarchyPath;

			var memoryUsedTmp = this.GetRuntimeMemorySizeLong(asset);
            newInfo.assetsInfo.Add(new AssetbundleDependencies.AssetInfo()
            {
                asset = asset,
                memoryUsed = memoryUsedTmp
            });

            _totalAssetbundleUsedMemory += memoryUsedTmp;
		}

		/// <summary>
		/// 获取运行时刻对象占用内存大小
		/// <param name="target">对象</param>
		/// <return>内存大小</return>
		/// </summary>
		private long GetRuntimeMemorySizeLong(Object target)
		{
			long retValue = 0;
#if UNITY_4
			retValue = (long)UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(target);
#else
            retValue = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(target);
#endif
			//如果对象是Sprite需要加上其texture占用内存大小，才是使用的真实内存大小
			if (target is Sprite)
			{
				var spriteTmp = target as Sprite;
#if UNITY_4
				retValue += (long)UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(spriteTmp.texture);
#else
                retValue += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(spriteTmp.texture);
#endif
			}
			return retValue;
        }
    }
}
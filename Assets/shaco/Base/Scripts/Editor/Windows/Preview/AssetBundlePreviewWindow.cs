using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class AssetBundlePreviewWindow : EditorWindow
    {
        private class LoadObjectInfo
        {
            public Object obj;
            public long usedMemroy;
        }

		private string _selectAssetBundlePath = string.Empty;
        private string _selectAssetBundleName = string.Empty;
		private LoadObjectInfo[] _loadedObjects = null;
		private Dictionary<UnityEngine.Object, EditorHelper.AnalyseObjectResult[]> _analyseObjects = new Dictionary<UnityEngine.Object, EditorHelper.AnalyseObjectResult[]>();
		private bool _isEncryted = false;
        private bool _isOriginalFile = false;
        private bool _isCompressed = false;
		private Vector2 _scrollPosition = Vector2.zero;
		private GameObject _currentShowGameObject = null;
        private string _searchName = string.Empty;
        private string _searchNameLower = string.Empty;
        private bool _isAutoOpenFolderWhenSave = true;

        [MenuItem("shaco/Tools/AssetBundlePreview _F10", false, (int)ToolsGlobalDefine.MenuPriority.Tools.ASSETBUNDLE_VIEWER)]
        static public void OpenAssetbundleCachePreviewWindow()
        {
            shacoEditor.EditorHelper.GetWindow<AssetBundlePreviewWindow>(null, true, "AssetBundlePreview");
        }

        private void OnEnable()
        {
            shacoEditor.EditorHelper.GetWindow<AssetBundlePreviewWindow>(this, true, "AssetBundlePreview").Init();
        }

		private void OnDestroy() 
		{
            SaveSettings();
            SafeRemoveShowGameObject();
        }

		private void OnGUI() 
		{
			GUI.changed = false;
            _selectAssetBundlePath = GUILayoutHelper.PathField("AssetBundle Path", _selectAssetBundlePath, "ab,assetbundle");
			if (GUI.changed)
			{
                UpdateSelectedAssetBundle();
                UpdateSelectedAssetBundleName();
            }

            DrawSearchName();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
			{
                DrawUnityObjects(_selectAssetBundleName, _loadedObjects);
            }
			GUILayout.EndScrollView();
		}

        private void DrawSearchName()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(Screen.width / 3 * 2);

                GUI.changed = false;
                _searchName = GUILayoutHelper.SearchField(_searchName);
                if (GUI.changed)
                    _searchNameLower = _searchName.ToLower();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 重新拷贝一份纹理，因为原纹理可能关闭了可读性以保证访问速度
        /// <param name="source">原纹理</param>
        /// <param name="textureFormat">纹理格式</param>
        /// <return>拷贝的纹理</return>
        /// </summary>
        private Texture2D DuplicateTexture(Texture2D source, TextureFormat textureFormat)
        {
			if (source.width == 0 && source.height == 0)
			{
				throw new System.Exception("AssetBundlePreviewWindow DuplicateTexture error: no data in texture source=" + source);
			}
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height, textureFormat, true);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        private void DrawTextureToolsButton(System.Func<Texture2D> callbackGetTexture)
        {
			if (GUILayout.Button("Save png", GUILayout.Width(70)))
			{
                var texture = callbackGetTexture();
                OpenSavePathAndWrite(DuplicateTexture(texture, TextureFormat.RGBA32).EncodeToPNG(), texture.name, "png");
            }
            if (GUILayout.Button("Save jpg", GUILayout.Width(70)))
            {
                var texture = callbackGetTexture();
                OpenSavePathAndWrite(DuplicateTexture(texture, TextureFormat.RGBA32).EncodeToJPG(), texture.name, "jpg");
            }
            if (GUILayout.Button("Save tga", GUILayout.Width(70)))
            {
                var texture = callbackGetTexture();
                OpenSavePathAndWrite(DuplicateTexture(texture, TextureFormat.RGBA32).EncodeToTGA(), texture.name, "tga");
            }
        }

#if UNITY_2017_1_OR_NEWER
        private void DrawSpriteAtlas(UnityEngine.U2D.SpriteAtlas spriteAtlas)
        {
            var sprites = new Sprite[spriteAtlas.spriteCount];
            spriteAtlas.GetSprites(sprites);
            for (int i = 0; i < sprites.Length; ++i)
            {
                EditorGUILayout.ObjectField(sprites[i], typeof(Sprite), true);
            }
        }
#endif

        private void DrawUnityObjects(string assetbundleName, LoadObjectInfo[] loadedObjects)
		{
			if (loadedObjects.IsNullOrEmpty())
				return;

			GUILayout.Label("Count: " + loadedObjects.Length);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Audo Show In Folder When save", GUILayout.Width(180));
                _isAutoOpenFolderWhenSave = EditorGUILayout.Toggle(_isAutoOpenFolderWhenSave);
            }
            GUILayout.EndHorizontal();

			GUILayoutHelper.DrawHeaderText("Assetbundle Flags");
            EditorGUI.BeginDisabledGroup(true);
			{
				GUILayout.BeginVertical("box");
				{
                    EditorGUILayout.Toggle("Encrypt", _isEncryted);
                    EditorGUILayout.Toggle("Oringinal File", _isOriginalFile);
                    EditorGUILayout.Toggle("Compress", _isCompressed);
				}
				GUILayout.EndVertical();
			}
			EditorGUI.EndDisabledGroup();

			foreach (var iter in loadedObjects)
			{
                var objTmp = iter.obj;
                bool isOriginalFile = objTmp.GetType() == typeof(shaco.TextOrigin);
                var nameTmp = isOriginalFile ? assetbundleName : objTmp.name;
                if (!string.IsNullOrEmpty(_searchNameLower) && !nameTmp.ToLower().Contains(_searchNameLower))
                {
                    continue;
                }
                System.Action onDrawHeaderCallBack = () =>
				{
					var typeTmp = objTmp.GetType();

                    if (!isOriginalFile)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        {
                            EditorGUILayout.ObjectField(objTmp, typeTmp, false);
                        }
                        EditorGUI.EndDisabledGroup();
                    }

                    GUILayout.Label((_isOriginalFile ? "FileSize: " : "Memory: ") + shaco.Base.FileHelper.GetFileSizeFormatString(iter.usedMemroy));

					if (typeTmp == typeof(GameObject) && GUILayout.Button("Show In Hierarchy"))
					{
                        SafeRemoveShowGameObject();
                        _currentShowGameObject = objTmp as GameObject;
						var oldName = _currentShowGameObject.name;

                        _currentShowGameObject = MonoBehaviour.Instantiate(_currentShowGameObject) as GameObject;
						shaco.UnityHelper.ChangeParent(_currentShowGameObject, GameObject.FindObjectOfType<UnityEngine.GameObject>().gameObject);

                        _currentShowGameObject.name = "TempAssetBundle(" + oldName + ")";
                        _currentShowGameObject.SetActive(true);

						EditorGUIUtility.PingObject(_currentShowGameObject);
					}
					else if (typeTmp == typeof(TextAsset))
					{
                        if (GUILayout.Button("Save txt"))
						{
                            OpenSavePathAndWrite(objTmp.ToBytes(), nameTmp, "txt");
						}
                    }
                    else if (typeTmp == typeof(shaco.ExcelDefaultAsset))
                    {
                        if (GUILayout.Button("Save csv"))
                        {
                            var excelData = new shaco.Base.ExcelData();
                            excelData.InitWithRowDatas(((shaco.ExcelDefaultAsset)objTmp).datas, nameTmp);
                            var extensionsTmp = shaco.Base.ExcelDefine.EXTENSION_TXT;
                            if (extensionsTmp.StartsWith(shaco.Base.FileDefine.DOT_SPLIT))
                                extensionsTmp = extensionsTmp.Remove(0, 1);

                            foreach (var tableInfo in excelData.dataList)
                            {
                                OpenSavePathAndWrite(excelData.GetCSVString(tableInfo).ToByteArray(), excelData.dataList.Count == 1 ? nameTmp : tableInfo.tabelName, extensionsTmp);
                            }
                        }
                    }
                    else if (typeTmp.IsInherited(typeof(Texture2D)))
					{
                        DrawTextureToolsButton(() => objTmp as Texture2D);
                    }
                    else if (typeTmp.IsInherited(typeof(Sprite)))
                    {
                        DrawTextureToolsButton(() => (objTmp as Sprite).texture);
                    }
                    else if (typeTmp == (typeof(AudioClip)))
					{
						if (GUILayout.Button("Save wav"))
						{
							var audio = objTmp as AudioClip;
                            OpenSavePathAndWrite(audio.EncodeToWav(), audio.name, "wav");
                        }
					}
                    else if (typeTmp == typeof(shaco.TextOrigin))
                    {
                        if (GUILayout.Button("Save txt"))
                        {
                            OpenSavePathAndWrite(objTmp.ToBytes(), nameTmp, "txt");
                        }
                        DrawTextureToolsButton(() => 
                        {
                            var tex2D = new Texture2D(0, 0, TextureFormat.RGBA32, false);
                            tex2D.LoadImage(objTmp.ToBytes());
                            tex2D.name = assetbundleName;
                            return tex2D;
                        });
                    }
                };

				var headerText = nameTmp + "(" + iter.ToTypeString() + ")";
				bool isOpen = GUILayoutHelper.DrawHeader(headerText, headerText, false, onDrawHeaderCallBack);
						
				if (isOpen && !_isOriginalFile)
				{
                    EditorHelper.AnalyseObjectResult[] analyseObjectsFind = null;
					if (!_analyseObjects.TryGetValue(objTmp, out analyseObjectsFind))
					{
                        if (iter.GetType() == typeof(shaco.TextOrigin))
                        {
                            analyseObjectsFind = new EditorHelper.AnalyseObjectResult[]
                            {
                                new EditorHelper.AnalyseObjectResult()
                                {
                                    key = assetbundleName,
                                    value = iter.ToString()
                                }
                            };
                        }
#if UNITY_2017_1_OR_NEWER
                        else if (objTmp.GetType() == typeof(UnityEngine.U2D.SpriteAtlas))
                        {
                            analyseObjectsFind = new EditorHelper.AnalyseObjectResult[]
                            {
                                new EditorHelper.AnalyseObjectResult()
                                {
                                    key = assetbundleName,
                                    value = objTmp
                                }
                            };
                        }
#endif
                        else
                        {
                            analyseObjectsFind = EditorHelper.AnalyseObject(objTmp);
                        }
                        _analyseObjects.Add(objTmp, analyseObjectsFind);
                    }

					foreach (var analyseObjectTmp in analyseObjectsFind)
					{
						var typeTmp = analyseObjectTmp.value.GetType();
                        // GUILayout.Label(analyseObjectTmp.key, GUILayout.Width(Screen.width / 3));
						GUILayoutHelper.DrawValue(analyseObjectTmp.key, analyseObjectTmp.value, typeTmp);

#if UNITY_2017_1_OR_NEWER
                        if (typeTmp == typeof(UnityEngine.U2D.SpriteAtlas))
                        {
                            DrawSpriteAtlas(analyseObjectTmp.value as UnityEngine.U2D.SpriteAtlas);
                        }
#endif
                    }
                }
			}
		}

		private void Init()
		{
            LoadSettings();
		}

		private void SafeRemoveShowGameObject()
		{
			if (null != _currentShowGameObject)
			{
				shaco.UnityHelper.SafeDestroy(_currentShowGameObject);
                _currentShowGameObject = null;
			}
		}

		private void SaveSettings()
		{
			shaco.GameHelper.datasave.WriteString("AssetBundlePreviewWindow_selectAssetBundlePath", _selectAssetBundlePath);
            UpdateSelectedAssetBundleName();
            shaco.GameHelper.datasave.WriteBool("AssetBundlePreviewWindow_isAutoOpenFolderWhenSave", _isAutoOpenFolderWhenSave);
        }

		private void LoadSettings()
		{
            _selectAssetBundlePath = shaco.GameHelper.datasave.ReadString("AssetBundlePreviewWindow_selectAssetBundlePath");
            UpdateSelectedAssetBundleName();
            _isAutoOpenFolderWhenSave = shaco.GameHelper.datasave.ReadBool("AssetBundlePreviewWindow_isAutoOpenFolderWhenSave", _isAutoOpenFolderWhenSave);
            if (!string.IsNullOrEmpty(_selectAssetBundlePath))
			{
				UpdateSelectedAssetBundle();
			}
        }

        private void UpdateSelectedAssetBundleName()
        {
            if (string.IsNullOrEmpty(_selectAssetBundlePath))
            {
                _selectAssetBundleName = string.Empty;
                return;
            }
            _selectAssetBundleName = shaco.Base.FileHelper.GetLastFileName(_selectAssetBundlePath);
            _selectAssetBundleName = _selectAssetBundleName.RemoveBehind("@@");
        }

		private void OpenSavePathAndWrite(byte[] bytes, string defaultFileName, string defaultExtentsion)
		{
            var selectFilePath = EditorUtility.SaveFilePanel("Select a path", shaco.Base.FileHelper.GetFolderNameByPath(_selectAssetBundlePath), defaultFileName, defaultExtentsion);
            if (!string.IsNullOrEmpty(selectFilePath))
            {
                shaco.Base.FileHelper.WriteAllByteByUserPath(selectFilePath, shaco.Base.EncryptDecrypt.Decrypt(bytes));
                if (_isAutoOpenFolderWhenSave)
                    EditorHelper.ShowInFolder(selectFilePath);
            }
		}

        private void UpdateSelectedAssetBundle()
		{
			if (string.IsNullOrEmpty(_selectAssetBundlePath))
				return;

			if (!shaco.Base.FileHelper.ExistsFile(_selectAssetBundlePath))
			{
                _selectAssetBundlePath = string.Empty;
                SaveSettings();
				return;
			}

            var assetBundleLoad = new shaco.HotUpdateImportMemory();
            try
            {
                _isEncryted = shaco.Base.EncryptDecrypt.IsEncryptionPath(_selectAssetBundlePath);
                _isOriginalFile = shaco.HotUpdateHelper.IsKeepOriginalFile(_selectAssetBundlePath);
                _isCompressed = shaco.HotUpdateHelper.IsCompressedFile(_selectAssetBundlePath);

                assetBundleLoad.CreateByMemoryByUserPath(_selectAssetBundlePath);

                var loadObjecs = assetBundleLoad.ReadAll();
                if (loadObjecs.IsNullOrEmpty())
                {
                    _selectAssetBundlePath = string.Empty;
                }
                else
                {
                    _loadedObjects = new LoadObjectInfo[loadObjecs.Length];
                    for (int i = 0; i < loadObjecs.Length; ++i)
                    {
                        _loadedObjects[i] = new LoadObjectInfo();
                        _loadedObjects[i].obj = loadObjecs[i];

                        //从源文件读取的方式无法获取消耗内存，所以直接获取文件大小
                        _loadedObjects[i].usedMemroy = _isOriginalFile ? shaco.Base.FileHelper.GetFileSize(_selectAssetBundlePath) : EditorHelper.GetRuntimeMemorySizeLong(loadObjecs[i]);
                    }
                }
                _analyseObjects.Clear();
				SaveSettings();
            }
            catch (System.Exception e)
            {
                Debug.LogError(null == e ? "unknown error" : e.ToString());
            }
            assetBundleLoad.Close();
        }
    }
}
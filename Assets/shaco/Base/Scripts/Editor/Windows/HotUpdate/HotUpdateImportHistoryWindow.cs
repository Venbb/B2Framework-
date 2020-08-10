using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class HotUpdateImportHistoryWindow : EditorWindow
    {
		/// <summary>
		/// 导入过的资源目录地址记录
		/// </summary>
		private Dictionary<string, string> _importHistoryPaths = new Dictionary<string, string>();

		/// <summary>
		/// 选择导入目录回调，如果为空则表示没有选择目录
		/// </summary>
		private System.Func<string, bool> _callbackSelectPath = null;

		private List<string> _willDeleteHistoryPaths = new List<string>();

        /// <summary>
        /// 打开资源面板导入窗口，并自动停靠到目标窗口的下方
        /// <param name="targetWindow">停靠的窗口</param>
        /// <param name="callbackSelectPath">选择导入路径回调，如果回调内参数为空表示没有选择导入路径</param>
        /// </summary>
        static public void OpenHoUpdateImportHistoryWindow(EditorWindow targetWindow, System.Func<string, bool> callbackSelectPath)
        {
            var windowTarget = EditorHelper.GetWindow<HotUpdateImportHistoryWindow>(null, true);
            windowTarget.LoadSettings();
            windowTarget._callbackSelectPath = callbackSelectPath;
            windowTarget.position = new Rect(targetWindow.position.x, targetWindow.position.y + targetWindow.position.height, targetWindow.position.width, windowTarget.position.height);
        }

		static public void CloseWindow()
		{
			EditorHelper.CloseWindow<HotUpdateImportHistoryWindow>();
		}

		void OnEnable()
		{
            EditorHelper.GetWindow<HotUpdateImportHistoryWindow>(this, true, "HotUpdateImportHistory").LoadSettings();
        }

		void OnDestroy()
		{
			SaveSettings();			
		}

		void OnGUI()
		{
			if (null != _callbackSelectPath && GUILayout.Button("Browse"))
			{
                var pathFolder = EditorUtility.OpenFolderPanel("Select a version control folder", string.Empty, string.Empty);
                OnSelectPathAndCloseWindow(pathFolder);
            }
            DrawImportList();
		}

		private void DrawImportList()
		{
			foreach (var iter in _importHistoryPaths)
			{
                var pathTmp = iter.Key;
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.TextArea(pathTmp);
                    if (null != _callbackSelectPath && GUILayout.Button("Select Import", GUILayout.Width(100)))
                    {
                        OnSelectPathAndCloseWindow(pathTmp);
                    }
                    if (GUILayout.Button("Open", GUILayout.Width(100)))
                    {
						EditorHelper.ShowInFolder(pathTmp);
                    }
                    if (GUILayout.Button("Delete", GUILayout.Width(100)))
                    {
                        _willDeleteHistoryPaths.Add(iter.Key);
                    }
                }
                GUILayout.EndHorizontal();
			}

			if (_willDeleteHistoryPaths.Count > 0)
			{
				for (int i = _willDeleteHistoryPaths.Count - 1; i >= 0; --i)
				{
					_importHistoryPaths.Remove(_willDeleteHistoryPaths[i]);
				}
                _willDeleteHistoryPaths.Clear();
				SaveSettings();
			}
		}

		private void OnSelectPathAndCloseWindow(string selectPath)
		{
			if (string.IsNullOrEmpty(selectPath))
			{
				Debug.LogError("HotUpdateImportHistoryWindow OnSelectPathAndCloseWindow error: invalid path");
				return;
			}

			bool isImportSuccess = false;
			if (null != _callbackSelectPath)
                isImportSuccess = _callbackSelectPath(selectPath);
			else
                isImportSuccess = true;

			if (isImportSuccess)
			{
                if (!_importHistoryPaths.ContainsKey(selectPath))
                    _importHistoryPaths.Add(selectPath, selectPath);
			}
			else 
			{
				Debug.LogError("HotUpdateImportHistoryWindow OnSelectPathAndCloseWindow error: maybe not a 'VersionControl' Folder");
			}
            this.Close();
		}

		/// <summary>
		/// 加载配置
		/// </summary>
		private void LoadSettings()
		{
            _importHistoryPaths = shaco.GameHelper.datasave.ReadDictionary("HotUpdateImportHistoryWindow._importHistoryPaths", key => key, value => value, new Dictionary<string, string>());
		}

		/// <summary>
		/// 保存配置
		/// </summary>
		private void SaveSettings()
		{
			shaco.GameHelper.datasave.WriteDictionary("HotUpdateImportHistoryWindow._importHistoryPaths", _importHistoryPaths);
		}
    }

}
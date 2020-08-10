using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace shacoEditor
{
    public class HotUpdateVersionViewerWindow
    {
        private HotUpdateExportWindow _windowExport = null;
        private shaco.Instance.Editor.TreeView.PathTreeView _folderDrawer = null;
        private Color _colorDragPrompt = new Color(0, 1, 0, 0);
        private UnityEditor.AnimatedValues.AnimFloat aimationAlpha = new UnityEditor.AnimatedValues.AnimFloat(0);

        public void Init(HotUpdateExportWindow target)
        {
            if (Application.isBatchMode)
                return;

            _windowExport = target;
            _folderDrawer = new shaco.Instance.Editor.TreeView.PathTreeView();
            _folderDrawer.allowDeleteAndMoveToTrash = false;
            _folderDrawer.allowAutoDeleteEmptyDirectory = true;
            _folderDrawer.SetCustomIcon(".assetbundle", EditorGUIUtility.FindTexture("Prefab Icon"));
            _folderDrawer.AddHeader("ExpandAll", () => _folderDrawer.ExpandAll());
            _folderDrawer.AddHeader("CollapseAll", () => _folderDrawer.CollapseAll());

            _folderDrawer.callbackTopBarGUI = () =>
            {
                GUILayout.Label("AssetBundle Count:" + _windowExport.mapAssetbundlePath.Count);
                return 18;
            };

            _folderDrawer.callbackWillDelete += OnWillDeleteCallBack;
            _folderDrawer.callbackRenameSuccess += OnRenameSucessCallBack;
            _folderDrawer.callbackCanRename += (assetPath, customData) => assetPath.EndsWith(shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            _folderDrawer.callbackDragLocation += (dragtype, paths) =>
            {
                var hasDirectory = paths.Any(v => v.StartsWith("Assets") && System.IO.Directory.Exists((v)));
                int selectImportMode = 0;
                if (hasDirectory)
                {
                    var selectedObjectsPath = new System.Text.StringBuilder();
                    if (null != DragAndDrop.objectReferences)
                    {
                        for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                        {
                            selectedObjectsPath.Append("Path: ");
                            selectedObjectsPath.Append(AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[i]));
                            selectedObjectsPath.Append("\n");
                        }
                    }
                    selectImportMode = EditorUtility.DisplayDialogComplex("New AssetBundle", selectedObjectsPath.ToString(), "One", "Cancel", "Multiple");
                    if (selectImportMode == 1)
                    {
                        //user canel
                        return;
                    }
                }

                foreach (var iter in paths)
                {
                    if (!iter.StartsWith("Assets"))
                    {
                        Debug.LogError("HotUpdateVersionViewerWindow error: only support Unity asset path");
                        continue;
                    }

                    //如果包含文件夹，需要用户确认遍历文件夹还是直接对文件夹打包
                    if (selectImportMode == 0)
                    {
                        _windowExport.NewAssetBundle(new HotUpdateExportWindow.SelectFile.FileInfo(iter));
                    }
                    else if (selectImportMode == 2)
                    {
                        if (System.IO.File.Exists(iter))
                        {
                            _windowExport.NewAssetBundle(new HotUpdateExportWindow.SelectFile.FileInfo(iter));
                        }
                        else if (System.IO.Directory.Exists(iter))
                        {
                            var childrenPath = HotUpdateExportWindow.GetCanBuildAssetBundlesAssetPath(iter).ToArray();
                            _windowExport.NewAssetBundleDeepAssets(childrenPath.Convert((v) =>
                            {
                                return new HotUpdateExportWindow.SelectFile.FileInfo(EditorHelper.FullPathToUnityAssetPath(v));
                            }));
                        }
                        else
                        {
                            Debug.LogError("HotUpdateVersionViewerWindow error: not found import path=" + iter);
                        }
                    }
                }

                _windowExport.Repaint();
            };

            UpdateFolderData(target);
            aimationAlpha.speed = 1;
        }

        public void AddFile(string assetBundleName, HotUpdateExportWindow.SelectFile.FileInfo fileInfo)
        {
            if (Application.isBatchMode)
                return;

            _folderDrawer.AddPath(assetBundleName, fileInfo.Asset);
        }

        public void RemoveFile(string asssetBundleName)
        {
            if (Application.isBatchMode)
                return;
                
            _folderDrawer.RemovePath(asssetBundleName);
        }

        public void ClearDrawFolder()
        {
            if (Application.isBatchMode)
                return;

            if (null != _folderDrawer)
                _folderDrawer.Clear();
        }

        public void DrawInspector(Rect rect)
        {
            if (Application.isBatchMode)
                return;

            _folderDrawer.DrawTreeView(rect);

            if (!_folderDrawer.hasChildren && !Application.isPlaying)
            {
                var labelStyleTmp = EditorStyles.largeLabel;
                labelStyleTmp.fontSize = 20;
                labelStyleTmp.alignment = TextAnchor.MiddleRight;
                GUI.Label(new Rect(rect.x, rect.y, rect.width / 2, rect.height), "Drop asset", labelStyleTmp);

                labelStyleTmp.alignment = TextAnchor.MiddleLeft;
                GUI.Label(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height), "to here", labelStyleTmp);
                _colorDragPrompt.a = aimationAlpha.value;
                GUIHelper.DrawOutline(rect, 2, _colorDragPrompt);

                if (aimationAlpha.value == 0) aimationAlpha.target = 1;
                if (aimationAlpha.value == 1) aimationAlpha.target = 0;

                if (null != _windowExport)
                    _windowExport.Repaint();
            }
        }

        private string GetPathFolderByAsset(string assetbundleName, HotUpdateExportWindow.SelectFile.FileInfo asset)
        {
            var pathTmp = asset.Asset.ToLower();

            var pathSplit1 = assetbundleName.Split(shaco.Base.FileDefine.PATH_FLAG_SPLIT);
            var pathSplit2 = pathTmp.Split(shaco.Base.FileDefine.PATH_FLAG_SPLIT);
            var pathSplitReal = new List<string>();

            int index1 = 0;
            int index2 = 0;
            for (; index1 < pathSplit1.Length && index2 < pathSplit2.Length; ++index1, ++index2)
            {
                if (pathSplit1[index1] == pathSplit2[index2])
                {
                    pathSplitReal.Add(pathSplit2[index2]);
                }
                else
                {
                    ++index2;
                }
            }

            if (pathSplitReal.Count == 0)
            {
                Debug.LogError("can't get path folder by asset bundle name=" + assetbundleName + " asset=" + asset);
                return pathTmp;
            }

            pathTmp = string.Empty;
            for (int i = 0; i < pathSplitReal.Count; ++i)
            {
                pathTmp += pathSplitReal[i] + shaco.Base.FileDefine.PATH_FLAG_SPLIT;
            }
            pathTmp = pathTmp.Remove(pathTmp.Length - shaco.Base.FileDefine.PATH_FLAG_SPLIT_STRING.Length);

            var pathFolderTmp = shaco.Base.FileHelper.GetFolderNameByPath(pathTmp);
            var pathConvert = shaco.Base.FileHelper.ContactPath(pathFolderTmp, shaco.Base.FileHelper.GetLastFileName(assetbundleName, true));
            return pathConvert;
        }

        private void OnRenameSucessCallBack(string originalPath, string newPath, List<string> customData)
        {
            if (Application.isBatchMode)
                return;

            //保持后缀名为assetbundle固定格式
            if (!newPath.EndsWith(shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE))
            {
                var tmpName = newPath;
                newPath = System.IO.Path.ChangeExtension(newPath, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
                _folderDrawer.ReName(tmpName, newPath);
            }

            var oldAssetInfo = _windowExport.mapAssetbundlePath[originalPath];
            _windowExport.mapAssetbundlePath.Remove(originalPath);
            _windowExport.mapAssetbundlePath.Add(newPath, oldAssetInfo);
        }

        private bool OnWillDeleteCallBack(string path, List<string> customData)
        {
            if (Application.isBatchMode)
                return false;

            var deleteAssetName = new List<string>();

            var willDeleteAssetsPath = new List<string>();
            if (System.IO.Directory.Exists(path))
            {
                willDeleteAssetsPath.AddRange(_folderDrawer.GetChildrenPath(path, true));
            }
            else
                willDeleteAssetsPath.Add(path);

            var assetbundleName = string.Empty;
            foreach (var iter in willDeleteAssetsPath)
            {
                //如果是文件夹则可以不用报错，因为它不是assetbundle
                if (System.IO.Directory.Exists(iter))
                {
                    continue;
                }

                var assetPathLower = iter;
                var findValue = FindAssetBundleSelectInfo(assetPathLower, _windowExport.mapAssetbundlePath, ref assetbundleName);
                if (null == findValue)
                {
                    Debug.LogError("HotUpdateVersionViewerWindow OnWillDeleteCallBack erorr: not found path=" + iter);
                    continue;
                }

                foreach (var iter2 in findValue.ListAsset)
                {
                    _windowExport.mapAllExportAssetSameKeyCheck.Remove(iter2.Value.Asset);
                }

                _windowExport.mapAssetbundlePath.Remove(assetbundleName);
                deleteAssetName.Add(assetbundleName);
            }

            _windowExport.UpdateVersionControlWhenDelete(_windowExport.currentRootPath, _windowExport.versionControlConfig, deleteAssetName.ToArray());
            _windowExport.CheckRootPathValid();
            return true;
        }

        private HotUpdateExportWindow.SelectFile FindAssetBundleSelectInfo(
            string assetPath,
            Dictionary<string, HotUpdateExportWindow.SelectFile> mapAssetBundlePath,
            ref string assetbundleName)
        {
            HotUpdateExportWindow.SelectFile ret = null;
            if (string.IsNullOrEmpty(assetPath))
                return ret;

            assetbundleName = shaco.Base.FileHelper.ReplaceLastExtension(assetPath, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            if (_windowExport.mapAssetbundlePath.ContainsKey(assetbundleName))
            {
                ret = _windowExport.mapAssetbundlePath[assetbundleName];
            }
            return ret;
        }

        private void UpdateFolderData(HotUpdateExportWindow target)
        {
            ClearDrawFolder();
            foreach (var iter in target.mapAssetbundlePath)
            {
                foreach (var iter2 in iter.Value.ListAsset)
                {
                    AddFile(iter.Key, iter2.Value);
                }
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Linq;

namespace shacoEditor
{
    /// <summary>
    /// Spine丢失资源检测工具
    /// 自动识别丢失资源和丢失引用并输出报告
    /// </summary>
    public class SpineCheckMissingResourcesWindow : EditorWindow
    {
        private enum ReportType
        {
            None = 0x00,
            MissingAtlasTxt = 0x01,
            MissingJsonOrByte = 0x02,
            MissingPng = 0x04,
            MismatchedAtlasAndPngFileName = 0x08,
            EmptyFolder = 0x10,
            MissingJsonAnimationName = 0x20
        }

        private class ReportInfo
        {
            public ReportType reportType = ReportType.None;
            public string spineFolderPath;
            public List<string> missingAnimationNames = new List<string>();
        }

        private string importPath;
        private string exportPath;

        //检查报告
        private List<ReportInfo> _checkReportInfos = new List<ReportInfo>();
        //当前正在检查的文件夹名字
        private string _currentCheckFolderPath = string.Empty;
        //当前检查的百分比
        private float _currentCheckPercent = 0.0f;
        //是否为用户强制取消检查
        private bool _userCancel = false;
        //是否要开始检查了
        private bool _isStartCheck = false;
        //总检查文件数量
        private long _allCheckFilesCount = 0;
        //总检查文件夹数量
        private long _allCheckDirectoriesCount = 0;
        //需要检查的动画名字列表
        private List<string> _needCheckAnimationNames = new List<string>();

        [MenuItem("shaco/Spine/CheckMissingResources")]
        static void OpenWindow()
        {
            EditorWindow.GetWindow<SpineCheckMissingResourcesWindow>(true, "CheckMissingResources").Init();
        }

        void Update()
        {
            if (!_isStartCheck)
                return;

            _userCancel = EditorUtility.DisplayCancelableProgressBar("check missing resource", _currentCheckFolderPath, _currentCheckPercent);
            if (_userCancel || _currentCheckPercent >= 1.0f)
            {
                CheckMissingResourcesEnd();
            }
        }

        void OnDestroy()
        {
            shaco.GameHelper.datasave.WriteString("SpineCheckMissingResourcesWindow.importPath", importPath);
            EditorUtility.ClearProgressBar();
        }

        void OnGUI()
        {
            GUI.changed = false;
            importPath = GUILayoutHelper.PathField("Import Path", importPath, string.Empty, Application.dataPath);
            if (GUI.changed)
            {
                UpdateExportPath();
            }

            EditorGUI.BeginDisabledGroup(true);
            {
                EditorGUILayout.LabelField("Export Path", exportPath);
            }
            EditorGUI.EndDisabledGroup();

            GUILayoutHelper.DrawStringList(_needCheckAnimationNames, "Check Animation Names");

            EditorGUI.BeginDisabledGroup(!Directory.Exists(importPath));
            {
                if (GUILayout.Button("Start Check"))
                {
                    StartCheckMissingResources();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// 开始收集spine丢失资源信息，并输出报告
        /// </summary>
        private void StartCheckMissingResources()
        {
            if (!Directory.Exists(importPath))
            {
                Debug.LogError("SpineCheckMissingResourcesWindow StartCheckMissingResources error: import path not a folder, path=" + importPath);
                return;
            }

            //删除原输出目录，清理上一次的输出记录
            _checkReportInfos.Clear();
            if (Directory.Exists(exportPath))
                Directory.Delete(exportPath, true);

            //查找spine所在目录，以atlas.txt文件为标准，该文件在的目录则是spine目录
            var allAtlasFilesPath = Directory.GetFiles(importPath, "*.atlas.txt", SearchOption.AllDirectories);
            var allDirectories = Directory.GetDirectories(importPath, "*", SearchOption.AllDirectories);

            if (null != allAtlasFilesPath)
            {
                _isStartCheck = true;
                _userCancel = false;
                _currentCheckPercent = 0;

                shaco.Base.ThreadPool.RunThread(() =>
                {
                    //查找文件丢失
                    long checkIndex = 0;
                    _allCheckFilesCount = allAtlasFilesPath.Length;
                    _allCheckDirectoriesCount = allDirectories.Length;
                    var allCheckCount = allAtlasFilesPath.Length + allDirectories.Length;

                    for (int i = allAtlasFilesPath.Length - 1; i >= 0; --i)
                    {
                        if (_userCancel)
                            break;
                        var spineFolderPath = Directory.GetParent(allAtlasFilesPath[i]);
                        var spineName = Path.GetFileName(allAtlasFilesPath[i]);
                        spineName = spineName.Remove(spineName.LastIndexOf(".atlas.txt"));

                        _currentCheckFolderPath = spineFolderPath.FullName;
                        try
                        {
                            CheckMissingResources(spineFolderPath.FullName, spineName);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError(e);
                            _userCancel = true;
                        }
                        _currentCheckPercent = (float)((double)(++checkIndex) / (double)allCheckCount);
                    }

                    //查找空文件夹
                    for (int i = allDirectories.Length - 1; i >= 0; --i)
                    {
                        var allFilesTmp = Directory.GetFiles(allDirectories[i], "*", SearchOption.TopDirectoryOnly);
                        var allDirectoriesTmp = Directory.GetDirectories(allDirectories[i], "*", SearchOption.TopDirectoryOnly);

                        _currentCheckFolderPath = allDirectories[i];
                        _currentCheckPercent = (float)((double)(++checkIndex) / (double)allCheckCount);

                        //文件夹为空
                        if ((null == allFilesTmp || allFilesTmp.Length == 0) && (null == allDirectoriesTmp || allDirectoriesTmp.Length == 0))
                        {
                            var reportInfo = new ReportInfo();
                            reportInfo.spineFolderPath = allDirectories[i];
                            reportInfo.reportType |= ReportType.EmptyFolder;
                            _checkReportInfos.Add(reportInfo);
                        }
                    }
                });
            }
        }

        private void Init()
        {
            importPath = shaco.GameHelper.datasave.ReadString("SpineCheckMissingResourcesWindow.importPath");
            UpdateExportPath();
        }

        private void UpdateExportPath()
        {
            exportPath = shaco.Base.FileHelper.RemoveLastPathByLevel(importPath, 1).ContactPath("CheckSpineMissingResourcesReport");
        }

        private void CheckMissingResourcesEnd()
        {
            var outputMissingAtlasPath = Path.Combine(exportPath, "MissingAtlas.txt");
            var outputMissingJsonOrBytePath = Path.Combine(exportPath, "MissingJsonOrByte.txt");
            var outputMissingPngPath = Path.Combine(exportPath, "MissingPng.txt");
            var outputMismatchedAtlasAndPngPath = Path.Combine(exportPath, "MismatchedAtlasAndPng.txt");
            var outputEmptyFolderPath = Path.Combine(exportPath, "EmptyFolder.txt");
            var outputMissingAnimationNamePath = Path.Combine(exportPath, "MissingAnimationName.txt");

            if (!Directory.Exists(exportPath))
                Directory.CreateDirectory(exportPath);

            //清理上一次输出报告
            if (File.Exists(outputMissingAtlasPath)) File.Delete(outputMissingAtlasPath);
            if (File.Exists(outputMissingJsonOrBytePath)) File.Delete(outputMissingJsonOrBytePath);
            if (File.Exists(outputMissingPngPath)) File.Delete(outputMissingPngPath);
            if (File.Exists(outputMismatchedAtlasAndPngPath)) File.Delete(outputMismatchedAtlasAndPngPath);
            if (File.Exists(outputEmptyFolderPath)) File.Delete(outputEmptyFolderPath);
            if (File.Exists(outputMissingAnimationNamePath)) File.Delete(outputMissingAnimationNamePath);


            //记录错误类型所占总文件的百分比
            int missingAtlasCount = 0;
            int missingJsonOrByteCount = 0;
            int missingPngCount = 0;
            int mismatchedAtlasCount = 0;
            int emptyFolderCount = 0;
            int missingAnimationNameCount = 0;

            //分析丢失资源完毕，开始输出报告
            for (int i = _checkReportInfos.Count - 1; i >= 0; --i)
            {
                var reportInfo = _checkReportInfos[i];

                //输出丢失atlas信息
                if (((int)reportInfo.reportType & (int)ReportType.MissingAtlasTxt) != 0)
                {
                    File.AppendAllText(outputMissingAtlasPath, reportInfo.spineFolderPath + "\n");
                    ++missingAtlasCount;
                }

                //输出丢失json或者byte信息
                if (((int)reportInfo.reportType & (int)ReportType.MissingJsonOrByte) != 0)
                {
                    File.AppendAllText(outputMissingJsonOrBytePath, reportInfo.spineFolderPath + "\n");
                    ++missingJsonOrByteCount;
                }

                //输出丢失png信息
                if (((int)reportInfo.reportType & (int)ReportType.MissingPng) != 0)
                {
                    File.AppendAllText(outputMissingPngPath, reportInfo.spineFolderPath + "\n");
                    ++missingPngCount;
                }

                //输出atlas与png不匹配信息
                if (((int)reportInfo.reportType & (int)ReportType.MismatchedAtlasAndPngFileName) != 0)
                {
                    File.AppendAllText(outputMismatchedAtlasAndPngPath, reportInfo.spineFolderPath + "\n");
                    ++mismatchedAtlasCount;
                }

                //输出空文件夹信息
                if (((int)reportInfo.reportType & (int)ReportType.EmptyFolder) != 0)
                {
                    File.AppendAllText(outputEmptyFolderPath, reportInfo.spineFolderPath + "\n");
                    ++emptyFolderCount;
                }

                //输出丢失动画名字信息
                if (((int)reportInfo.reportType & (int)ReportType.MissingJsonAnimationName) != 0)
                {
                    var appendStringTmp = new System.Text.StringBuilder();
                    appendStringTmp.Append(reportInfo.spineFolderPath + "\n");
                    for (int j = 0; j < reportInfo.missingAnimationNames.Count; ++j)
                    {
                        appendStringTmp.Append("\t【Name】");
                        appendStringTmp.Append(reportInfo.missingAnimationNames[j]);
                        appendStringTmp.Append("\n");
                    }
                    File.AppendAllText(outputMissingAnimationNamePath, appendStringTmp.ToString());
                    ++missingAnimationNameCount;
                }
            }

            _currentCheckPercent = 0;
            _isStartCheck = false;
            _userCancel = false;
            EditorUtility.ClearProgressBar();

            if (_checkReportInfos.Count > 0)
            {
                //根据错误百分比修改原文件名字
                RenameFileWithErrorPercent(outputMissingAtlasPath, missingAtlasCount, _allCheckFilesCount);
                RenameFileWithErrorPercent(outputMissingJsonOrBytePath, missingJsonOrByteCount, _allCheckFilesCount);
                RenameFileWithErrorPercent(outputMissingPngPath, missingPngCount, _allCheckFilesCount);
                RenameFileWithErrorPercent(outputMismatchedAtlasAndPngPath, mismatchedAtlasCount, _allCheckFilesCount);
                RenameFileWithErrorPercent(outputEmptyFolderPath, emptyFolderCount, _allCheckDirectoriesCount);
                RenameFileWithErrorPercent(outputMissingAnimationNamePath, missingAnimationNameCount, _allCheckFilesCount);

                EditorHelper.ShowInFolder(exportPath);
            }
            else
            {
                Debug.Log("no missing report");
            }
        }

        /// <summary>
        /// 根据错误百分比对文件进行重命名
        /// </summary>
        private void RenameFileWithErrorPercent(string filePath, int errorCount, long totalCheckCount)
        {
            if (errorCount > 0)
            {
                var percentTmp = ((double)(errorCount * 100) / (double)totalCheckCount).ToString("0.00") + "%";
                var folderPath = Path.GetDirectoryName(filePath);
                var fileName = Path.GetFileName(filePath);
                var extension = Path.GetExtension(filePath);
                fileName = fileName.Remove(fileName.IndexOf('.')) + "(" + percentTmp + ")" + extension;

                File.Move(filePath, Path.Combine(folderPath, fileName));
            }
        }

        /// <summary>
        /// 检查一个spine丢失资源信息
        /// <param name="spineFolderPath">spine目录</param>
        /// <param name="spineName">spine名字</param>
        /// </summary>
        private void CheckMissingResources(string spineFolderPath, string spineName)
        {
            ReportInfo reportInfo = new ReportInfo();

            //检查atlas是否丢失
            var atlasPath = Path.Combine(spineFolderPath, spineName + ".atlas.txt");
            if (!File.Exists(atlasPath))
            {
                reportInfo.reportType |= ReportType.MissingAtlasTxt;
            }

            //检查bytes或者json文件是否丢失
            var bytesPath = Path.Combine(spineFolderPath, spineName + ".skel.bytes");
            var jsonPath = Path.Combine(spineFolderPath, spineName + ".json");
            if (!File.Exists(bytesPath) && !File.Exists(jsonPath))
            {
                reportInfo.reportType |= ReportType.MissingJsonOrByte;
            }
            else
            {
                //查看动画名字是否匹配
                if (_needCheckAnimationNames.Count > 0)
                {
                    var animationsNames = new List<string>();

                    //查看json文件
                    if (File.Exists(jsonPath))
                    {
                        var jsonData = shaco.LitJson.JsonMapper.ToObject(File.ReadAllText(jsonPath));
                        if (!jsonData.ContainsKey("animations"))
                        {
                            Debug.LogErrorFormat("SpineCheckMissingResourcesWindow CheckMissingResources error: not found animation json node");
                        }
                        else
                        {
                            var animationNode = jsonData["animations"];
                            foreach (KeyValuePair<string, shaco.LitJson.JsonData> iter in animationNode)
                            {
                                animationsNames.Add(iter.Key.ToString());
                            }
                        }
                    }
                    //查看二进制文件
                    else if (File.Exists(bytesPath))
                    {
                        var readBytes = File.ReadAllBytes(bytesPath);
                        var requiredPaths = shacoEditor.GameHelper.spineBinaryReader.ReadSpineAimationsFromBinary(readBytes);
                        if (!requiredPaths.IsNullOrEmpty())
                        {
                            for (int i = requiredPaths.Length - 1; i >= 0; --i)
                                animationsNames.Add(requiredPaths[i]);
                        }
                    }

                    for (int i = _needCheckAnimationNames.Count - 1; i >= 0; --i)
                    {
                        if (!animationsNames.Contains(_needCheckAnimationNames[i]))
                        {
                            reportInfo.missingAnimationNames.Add(_needCheckAnimationNames[i]);
                        }
                    }
                    if (reportInfo.missingAnimationNames.Count > 0)
                    {
                        reportInfo.reportType |= ReportType.MissingJsonAnimationName;
                    }
                }
            }

            //检查png是否丢失，只有有png图就不算丢失，如果是不匹配的话有另外的规则在计算
            // var pngPath = Path.Combine(spineFolderPath, spineName + ".png");
            var allFiles = Directory.GetFiles(spineFolderPath);
            if (!allFiles.Any(v => v.EndsWith(".png")))
            {
                reportInfo.reportType |= ReportType.MissingPng;
            }

            //当atlas存在的时候，检查atlas中记录的png名字是否匹配
            if (File.Exists(atlasPath))
            {
                //spine的atlas格式一般以第二行为当前sipne需要使用的图片名字
                var loadAtlasTxt = File.ReadAllText(atlasPath);
                loadAtlasTxt = loadAtlasTxt.Replace("\r\n", "\n");
                var splitLineText = loadAtlasTxt.Split('\n');

                var requirePngFileName = string.Empty;
                if (!string.IsNullOrEmpty(splitLineText[0]))
                    requirePngFileName = splitLineText[0];
                else
                    requirePngFileName = splitLineText[1];

                if (string.IsNullOrEmpty(requirePngFileName))
                {
                    Debug.LogError("SpineCheckMissingResourcesWindow read png file name error, path=" + spineFolderPath);
                }
                else
                {
                    //查看需求的图片是否和atlas一样放在同级目录
                    var requirePngFilePath = Path.Combine(spineFolderPath, requirePngFileName);
                    if (!File.Exists(requirePngFilePath))
                    {
                        reportInfo.reportType |= ReportType.MismatchedAtlasAndPngFileName;
                    }
                }
            }

            if (reportInfo.reportType != ReportType.None)
            {
                reportInfo.spineFolderPath = spineFolderPath;
                _checkReportInfos.Add(reportInfo);
            }
        }
    }
}
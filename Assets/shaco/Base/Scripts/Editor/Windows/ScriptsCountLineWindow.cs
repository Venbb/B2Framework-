using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace shacoEditor
{
    /// <summary>
    /// 代码行数统计窗口
    /// </summary>
    public class ScriptsCountLineWindow : EditorWindow
    {
        private class CountLineResult
        {
            public string filePath;
            public int lineCount;
        }

        //查找根目录所占进度条百分比
        private readonly float SEARCH_ROOT_PATH_PERCENT = 0.2f;

        //查找根目录路径
        [SerializeField]
        private List<string> _importRootPaths = new List<string>();

        //过滤路径标记
        [SerializeField]
        private List<string> _ignoreImportRootPathFlags = new List<string>();

        //脚本后缀名
        [SerializeField]
        private List<string> _scriptExtensions = new List<string>() { ".cs"};

        //计算行数结果
        [SerializeField]
        private List<CountLineResult> _countLineResults = new List<CountLineResult>();

        //代码行数
        [SerializeField]
        private long _totalScriptsLine = 0;

        //是否过滤空白行
        [SerializeField]
        private bool _isIgnoreEmptyLine = false;

        //需要过滤的前后关联符号
        [SerializeField]
        private List<shaco.Base.Utility.LocalizationCollectPairFlag> _ignoreCollectFlags = new List<shaco.Base.Utility.LocalizationCollectPairFlag>
        {
            new shaco.Base.Utility.LocalizationCollectPairFlag() { startFlag = "//", endFlag = string.Empty },
            new shaco.Base.Utility.LocalizationCollectPairFlag() { startFlag = "/*", endFlag = "*/" }
        };
        [SerializeField]
        private string[] _inputIgnoreCollectFlag = new string[2];

        [MenuItem("shaco/Tools/ScriptsCountLineWindow", false, (int)(int)ToolsGlobalDefine.MenuPriority.Tools.SCRIPTS_COUNT_LINE)]
        static private ScriptsCountLineWindow OpenScriptsCountLineWindow()
        {
            var retValue = EditorHelper.GetWindow<ScriptsCountLineWindow>(null, true, "ScriptsCountLineWindow");
            retValue.Init();
            return retValue;
        }

        private void Init()
        {
            var defaultPath = shaco.Base.FileHelper.GetLastFileName(Application.dataPath);
            _importRootPaths = shaco.GameHelper.datasave.ReadListString("ScriptsCountLineWindow._importRootPaths", new List<string>() { defaultPath });
            _ignoreImportRootPathFlags = shaco.GameHelper.datasave.ReadListString("ScriptsCountLineWindow._ignoreImportRootPathFlags", _ignoreImportRootPathFlags);
            _isIgnoreEmptyLine = shaco.GameHelper.datasave.ReadBool("ScriptsCountLineWindow._isIgnoreEmptyLine", _isIgnoreEmptyLine);
        }

        private void OnDestroy()
        {
            SaveSettings();
        }

        private void OnGUI()
        {
            EditorHelper.RecordObjectWindow(this);

            GUILayoutHelper.DrawListBase(_importRootPaths, "Import Paths", null, null, null, null, (index, value, callback) =>
            {
                GUI.changed = false;
                value = GUILayoutHelper.PathField("Item " + index, value, string.Empty);
                if (GUI.changed)
                {
                    callback(value);
                    SaveSettings();
                }
                return true;
            }, null);

            GUILayoutHelper.DrawStringList(_ignoreImportRootPathFlags, "Ignore Import Path Flags");
            GUILayoutHelper.DrawStringList(_scriptExtensions, "Script Extensions");

            _isIgnoreEmptyLine = EditorGUILayout.Toggle("Ignore Empty Line", _isIgnoreEmptyLine);

            LocalizationReplaceWindow.DrawFlags("Ignore Collect Flags", _ignoreCollectFlags, _inputIgnoreCollectFlag);

            if (GUILayout.Button("Start"))
            {
                if (!_importRootPaths.IsNullOrEmpty())
                {
                    CountLineStart(_importRootPaths);
                }
            }
        }

        private void CountLineStart(List<string> rootPaths)
        {
            var allFiles = new List<string>();
            var userCancel = false;
            var currentSearchRootPath = string.Empty;
            var currentSearchRootPathCount = 0;

            rootPaths = rootPaths.Distinct().ToList();
            shaco.Base.Coroutine.ForeachAsync(rootPaths, (path) =>
            {
                try
                {
                    var rootPath = path.ToString();
                    currentSearchRootPath = rootPath;
                    ++currentSearchRootPathCount;

                    shaco.Base.FileHelper.GetSeekPath(rootPath, ref allFiles, (value) =>
                    {
                        if (_ignoreImportRootPathFlags.Any(v => value.Contains(v)))
                            return false;

                        return _scriptExtensions.Any(v => value.EndsWith(v));
                    });
                }
                catch (System.Exception e)
                {
                    Debug.LogError("ScriptsCountLineWindow CountLineStart exception: e=" + e);
                }
                return !userCancel;
            }, (percent) =>
            {
                if (userCancel)
                {
                    Debug.Log("ScriptsCountLineWindow CountLineStart: user cancel...");
                    return;
                }
                
                if (percent >= 1.0f)
                {
                    EditorUtility.ClearProgressBar();
                    CountLineAllFiles(allFiles);
                }
                else
                {
                    userCancel = EditorUtility.DisplayCancelableProgressBar(string.Format("Search root path ({0}/{1})", currentSearchRootPathCount, rootPaths.Count), currentSearchRootPath, percent * SEARCH_ROOT_PATH_PERCENT);
                }
            });
        }

        private void CountLineAllFiles(List<string> allFiles)
        {
            if (allFiles.IsNullOrEmpty())
            {
                Debug.LogError("ScriptsCountLineWindow CountLineStart error: not found file");
                return;
            }

            _countLineResults.Clear();
            _totalScriptsLine = 0;

            var userCancel = false;
            var currentCountLineFilePath = string.Empty;
            var currentCountLineFileCount = 0;

            shaco.Base.Coroutine.ForeachAsync(allFiles, (path) =>
            {
                try
                {
                    var filePath = path.ToString();
                    currentCountLineFilePath = EditorHelper.FullPathToUnityAssetPath(filePath);
                    ++currentCountLineFileCount;
                    var fileString = shaco.Base.FileHelper.ReadAllByUserPath(filePath);

                    //去除多余的换行符
                    fileString = fileString.RemoveAll("\r");

                    //删除注释内容
                    if (!_ignoreCollectFlags.IsNullOrEmpty())
                    {
                        //删除注释
                        for (int i = _ignoreCollectFlags.Count - 1; i >= 0; --i)
                        {
                            fileString = fileString.RemoveAnnotation(_ignoreCollectFlags[i].startFlag, _ignoreCollectFlags[i].endFlag);
                        }
                    }

                    //获取计算结果
                    var resultNew = new CountLineResult();
                    var splitStrings = fileString.Split('\n').ToList();
                    if (_isIgnoreEmptyLine)
                    {
                        for (int i = splitStrings.Count - 1; i >= 0; --i)
                        {
                            var lineStr = splitStrings[i];

                            //判断是否为一行空白字符
                            if (!lineStr.Any(v => v != ' ' && v != '\t'))
                            {
                                splitStrings.RemoveAt(i);
                            }
                        }
                    }
                    resultNew.filePath = filePath;
                    resultNew.lineCount = splitStrings.Count;
                    _countLineResults.Add(resultNew);

                    _totalScriptsLine += resultNew.lineCount;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("ScriptsCountLineWindow CountLineAllFiles exception: e=" + e);
                }
                return !userCancel;
            }, (percent) =>
            {
                //用户手动取消
                if (userCancel)
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                if (percent >= 1.0f)
                {
                    EditorUtility.ClearProgressBar();

                    //写入计算结果
                    var strAppend = new System.Text.StringBuilder();
                    strAppend.Append("File Count:" + _countLineResults.Count + "\n");
                    strAppend.Append("All Line:" + _totalScriptsLine + "\n");

                    for (int i = 0; i < _countLineResults.Count; ++i)
                    {
                        var resultTmp = _countLineResults[i];
                        strAppend.Append(resultTmp.filePath);
                        strAppend.Append('\t');
                        strAppend.Append(resultTmp.lineCount);
                        strAppend.Append('\n');
                    }

                    //删除最后一个换行符
                    if (strAppend.Length > 0)
                    {
                        strAppend.Remove(strAppend.Length - 1, 1);
                    }

                    //写入结果到文件并打开所在目录
                    var saveFilePath = Application.dataPath.ContactPath("../ScriptsCountLineResult.txt");
                    shaco.Base.FileHelper.WriteAllByUserPath(saveFilePath, strAppend.ToString());
                    System.Diagnostics.Process.Start(saveFilePath);
                }
                else
                {
                    userCancel = EditorUtility.DisplayCancelableProgressBar(string.Format("Count line...({0}/{1})", currentCountLineFileCount, allFiles.Count), currentCountLineFilePath, percent * (1 - SEARCH_ROOT_PATH_PERCENT) + SEARCH_ROOT_PATH_PERCENT);
                }
            });
        }

        private void SaveSettings()
        {
            shaco.GameHelper.datasave.WriteList("ScriptsCountLineWindow._importRootPaths", _importRootPaths);
            shaco.GameHelper.datasave.WriteList("ScriptsCountLineWindow._ignoreImportRootPathFlags", _ignoreImportRootPathFlags);
            shaco.GameHelper.datasave.WriteBool("ScriptsCountLineWindow._isIgnoreEmptyLine", _isIgnoreEmptyLine);
        }
    }
}
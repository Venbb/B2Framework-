using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class CreateExcelScriptMenu : Editor
    {
        [MenuItem("Assets/Create/shaco/ExcelScript", false, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_EXCEL_SCRIPT)]
        static private void CreateExcelScript()
        {
            ConvertExcelFiles((excelData, exportPath, templates) =>
            {
                shaco.ExcelDataSerializable.SerializableAsCSharpScript(excelData.dataList, exportPath, templates);
            }, ".xlsx", ".xls");
        }

        [MenuItem("Assets/Create/shaco/ExcelText", false, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_EXCEL_TEXT)]
        static private void CreateExcelText()
        {
            ConvertExcelFiles((excelData, exportPath, templates)=>
            {
                excelData.SaveAsTxt(exportPath);
            }, shaco.Base.ExcelDefine.EXTENSION_TXT);
        }

        [MenuItem("Assets/Create/shaco/ExcelScript", true, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_EXCEL_SCRIPT)]
        static private bool CreateExcelScriptValidate()
        {
            return CheckExcelFile((filePath) =>
            {
                return IsExcelFileOrAsset(filePath) && !filePath.EndsWith(".xlsx") && !filePath.EndsWith(".xls");
            });
        }

        [MenuItem("Assets/Create/shaco/ExcelText", true, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_EXCEL_TEXT)]
        static private bool CreateExcelTextValidate()
        {
            return CheckExcelFile((filePath) =>
            {
                return !filePath.EndsWith(shaco.Base.ExcelDefine.EXTENSION_TXT) && shaco.Base.ExcelHelper.IsExcelFile(filePath);
            });
        }

        [MenuItem("Assets/Create/shaco/ExcelSerializableAsset", false, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_EXCEL_SERIALIZABLE_ASSET)]
        static private void CreateExcelSerializableAsset()
        {
            var selectGUIDs = Selection.assetGUIDs;
            if (null != selectGUIDs)
                CreateExcelSerializableAsset(selectGUIDs.Convert(v => AssetDatabase.GUIDToAssetPath(v)));
        }

        [MenuItem("Assets/Create/shaco/ExcelSerializableAsset", true, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_EXCEL_SERIALIZABLE_ASSET)]
        static private bool CreateExcelSerializableAssetValidate()
        {
            if (EditorApplication.isCompiling)
                return false;

            var selectGUIDs = Selection.assetGUIDs;
            if (null == selectGUIDs)
                return false;

            return !GetCurrentSelectExcelScripts(selectGUIDs.Convert(v => AssetDatabase.GUIDToAssetPath(v)), true).IsNullOrEmpty();
        }

        static private bool IsExcelFileOrAsset(string path)
        {
            return shaco.Base.ExcelHelper.IsExcelFile(path) || path.EndsWith(shaco.Base.ExcelDefine.EXTENSION_ASSET) || path.EndsWith(".cs");
        }

        static private TextAsset[] GetCurrentSelectExcelScripts(string[] paths, bool isCheckOnly)
        {
            List<TextAsset> retValue = new List<TextAsset>();
            var scriptsAsset = paths.FindAll(v =>
            {
                var fullPath = EditorHelper.GetFullPath(v);
                return fullPath.EndsWith(shaco.Base.ExcelDefine.EXTENSION_TXT) || shaco.Base.FileHelper.ExistsDirectory(fullPath);
            });
            
            if (scriptsAsset.IsNullOrEmpty())
            {
                return retValue.ToArray();
            }

            var typeName = typeof(shaco.Base.IExcelData).Name;
            var typeFullName = typeof(shaco.Base.IExcelData).FullName;
            var namespaceName = typeof(shaco.Base.IExcelData).Namespace;

            for (int i = scriptsAsset.Length - 1; i >= 0; --i)
            {
                var assetPath = scriptsAsset[i];
                var fullPath = EditorHelper.GetFullPath(assetPath);
                if (shaco.Base.FileHelper.ExistsDirectory(fullPath))
                {
                    var allFile = shaco.Base.FileHelper.GetFiles(fullPath, "*" + shaco.Base.ExcelDefine.EXTENSION_TXT, System.IO.SearchOption.TopDirectoryOnly);
                    foreach (var iter in allFile)
                    {
                        var assetTmp = AssetDatabase.LoadAssetAtPath<TextAsset>(EditorHelper.FullPathToUnityAssetPath(iter));
                        retValue.Add(assetTmp);
                        if (isCheckOnly)
                            break;
                    }
                }
                else
                {
                    var convertScript = ConvertCsvToExcelScript(assetPath);
                    if (null != convertScript)
                        retValue.Add(convertScript);
                }

                if (isCheckOnly && !retValue.IsNullOrEmpty())
                {
                    break;
                }
            }
            return retValue.ToArray();
        }

        //根据csv路径转换获取对应的脚本路径
        static private TextAsset ConvertCsvToExcelScript(string assetPath)
        {
            TextAsset retValue = null;
            if (!shaco.Base.ExcelHelper.IsExcelFile(assetPath))
                return retValue;

            //保证为相对路径
            assetPath = EditorHelper.FullPathToUnityAssetPath(assetPath);

            //查找脚本目录对应路径
            var customExportScriptPath = shaco.Base.GameHelper.excelSetting.GetCustomExportScriptPath(assetPath);
            var scriptFileName = shaco.Base.FileHelper.GetLastFileName(assetPath);
            scriptFileName = shaco.Base.FileHelper.ReplaceLastExtension(scriptFileName, ".cs");

            var scriptAssetPath = customExportScriptPath.ContactPath(scriptFileName);
            scriptAssetPath = EditorHelper.FullPathToUnityAssetPath(scriptAssetPath);
            retValue = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptAssetPath);
            return retValue;
        }

        static private void CreateExcelSerializableAsset(string[] paths)
        {
            if (EditorApplication.isCompiling)
            {
                shaco.Log.Error("CreateExcelScriptMenu CreateExcelSerializableAsset error: is compiling...");
                return;
            }

            var scriptsAsset = GetCurrentSelectExcelScripts(paths, false);
            if (scriptsAsset.IsNullOrEmpty())
            {
                shaco.Log.Error("CreateExcelScriptMenu CreateExcelSerializableAsset error: no selected excel script");
                return;
            }

            var currentExportPath = string.Empty;
            int index = 0;

            if (!Application.isBatchMode)
                EditorUtility.DisplayProgressBar("Create serializable asset start", "please wait...", 0);

            Debug.Log("CreateExcelScriptMenu CreateExcelSerializableAsset: count=" + scriptsAsset.Length);

            System.Action<TextAsset> updateOnce = (TextAsset asset) =>
            {
                if (null == asset)
                {
                    ++index;
                    return;
                }

                var assetTmp = asset;
                currentExportPath = AssetDatabase.GetAssetPath(assetTmp);

                var scriptPath = AssetDatabase.GetAssetPath(assetTmp);
                var customExportAssetPath = shaco.Base.GameHelper.excelSetting.GetCustomExportAssetPath(scriptPath);
                var assetFolderPath = shaco.Base.FileHelper.GetFolderNameByPath(customExportAssetPath);

                //获取asset文件路径
                var scriptFileName = shaco.Base.FileHelper.GetLastFileName(scriptPath);
                var assetPath = assetFolderPath.ContactPath(scriptFileName);
                assetPath = shaco.Base.FileHelper.ReplaceLastExtension(assetPath, ".asset");
                assetPath = EditorHelper.FullPathToUnityAssetPath(assetPath);

                //从数据表文本文件内容写入到asset文件中
                var customExportTextPath = shaco.Base.GameHelper.excelSetting.GetCustomExportTextPath(scriptPath);
                var csvFileName = shaco.Base.FileHelper.ReplaceLastExtension(shaco.Base.FileHelper.GetLastFileName(assetPath), shaco.Base.ExcelDefine.EXTENSION_TXT);
                var csvFullPath = customExportTextPath.ContactPath(csvFileName);

                //创建asset
                shaco.ExcelDefaultAsset currentAsset = AssetDatabase.LoadAssetAtPath<shaco.ExcelDefaultAsset>(assetPath);
                if (null == currentAsset)
                {
                    currentAsset = ScriptableObject.CreateInstance<shaco.ExcelDefaultAsset>();
                    AssetDatabase.CreateAsset(currentAsset, assetPath);
                }
                else
                    currentAsset.datas.Clear();

                Debug.Log("SerializableAsset asset path=" + assetPath, AssetDatabase.LoadAssetAtPath<Object>(EditorHelper.FullPathToUnityAssetPath(assetPath)));

                if (!shaco.Base.FileHelper.ExistsFile(csvFullPath))
                {
                    Debug.LogError("CreateUIScriptMenu CreateExcelSerializableAsset error: not found csv path=" + csvFullPath);
                    ++index;
                    return;
                }

                var streamReader = new System.IO.StreamReader(new System.IO.MemoryStream(shaco.Base.FileHelper.ReadAllByteByUserPath(csvFullPath)));
                var firstLineString = streamReader.ReadLine();
                var dataCountInRow = 0;
                if (!string.IsNullOrEmpty(firstLineString))
                {
                    var valuesTmp = firstLineString.Split('\t');
                    var newLineData = new shaco.Base.ExcelDefine.RowData();
                    newLineData.r = new string[valuesTmp.Length];
                    for (int i = 0; i < valuesTmp.Length; ++i)
                    {
                        newLineData.r[i] = valuesTmp[i];
                    }
                    currentAsset.datas.Add(newLineData);
                    dataCountInRow = valuesTmp.Length;
                }

                int currentLineIndex = 0;
                while (true)
                {
                    var lineString = streamReader.ReadLine();
                    if (string.IsNullOrEmpty(lineString))
                        break;
                        
                    var valuesTmp = lineString.Split('\t');
                    var newLineData = new shaco.Base.ExcelDefine.RowData();
                    newLineData.r = new string[dataCountInRow];

                    //超出首行数据时候默认丢弃，并报错提示
                    if (valuesTmp.Length > dataCountInRow)
                    {
                        Debug.LogErrorFormat("CreateExcelScriptMenu CreateExcelSerializableAsset error: line:{0} data count:{1} over than default count:{2}\n{3}", currentLineIndex, valuesTmp.Length, dataCountInRow, assetPath);
                        valuesTmp = valuesTmp.FixSize(dataCountInRow, string.Empty);
                    }

                    //行数据不足时候自动用空字符串补齐，并报错提示
                    if (valuesTmp.Length < dataCountInRow)
                    {
                        Debug.LogErrorFormat("CreateExcelScriptMenu CreateExcelSerializableAsset error: line:{0} data count:{1} less than default count:{2}\n{3}", currentLineIndex, valuesTmp.Length, dataCountInRow, assetPath);
                        valuesTmp = valuesTmp.FixSize(dataCountInRow, string.Empty);
                    }

                    for (int i = 0; i < dataCountInRow; ++i)
                    {
                        newLineData.r[i] = valuesTmp[i];
                    }

                    currentAsset.datas.Add(newLineData);
                    ++currentLineIndex;
                }
                ++index;

                EditorHelper.SetDirty(currentAsset);
            };

            if (Application.isBatchMode)
            {
                foreach (var iter in scriptsAsset)
                {
                    updateOnce(iter);
                }
                AssetDatabase.SaveAssets();
            }
            else
            {
                shaco.Base.Coroutine.Foreach(scriptsAsset, (o) =>
                {
                    updateOnce(o as TextAsset);
                    return true;
                }, (percent) =>
                {
                    if (percent < 1.0f)
                    {
                        if (!Application.isBatchMode)
                            EditorUtility.DisplayProgressBar("Create serializable asset: ", currentExportPath, (float)index / (float)scriptsAsset.Length);
                    }
                    else
                    {
                        if (!Application.isBatchMode)
                            EditorUtility.ClearProgressBar();
                        AssetDatabase.SaveAssets();
                    }
                }, 10.0f / (float)scriptsAsset.Length);
            }
        }

        static private bool CheckExcelFile(System.Func<string, bool> callbackIsValidFile)
        {
            var allSelectGUIDs = Selection.assetGUIDs;
            if (allSelectGUIDs == null || allSelectGUIDs.Length == 0)
                return false;

            bool hasValidFile = false;
            for (int i = 0; i < allSelectGUIDs.Length; ++i)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(allSelectGUIDs[i]);

                //如果是文件夹并且含有excel关键字，则遍历文件夹第一层查看是否excel文件
                if (shaco.Base.FileHelper.ExistsDirectory(assetPath))
                {
                    var filesTmp = System.IO.Directory.GetFiles(assetPath);
                    for (int j = 0; j < filesTmp.Length; ++j)
                    {
                        var filePathTmp = filesTmp[j].Replace("\\", "/");
                        if (callbackIsValidFile(filePathTmp))
                        {
                            hasValidFile = true;
                            break;
                        }
                    }
                }
                else
                {
                    if (callbackIsValidFile(assetPath))
                        hasValidFile = true;
                }

                if (hasValidFile)
                {
                    break;
                }
            }
            return hasValidFile;
        }

        static private void ConvertExcelFiles(System.Action<shaco.Base.ExcelData, string, shaco.Base.StringTypePair[]> onCreateCallBack, params string[] ignoreExtension)
        {
            var allSelectGUIDs = Selection.assetGUIDs;
            if (allSelectGUIDs == null || allSelectGUIDs.Length == 0)
                return;

            bool hasSavedExcel = false;
            bool userCancel = false;
            string exceptionMessage = string.Empty;
            string currentExcelPath = string.Empty;
            List<string> sharingViolationFilesPath = new List<string>();
            List<string> excelAssetsPaths = new List<string>();

            //获取当前选择所有对象
            int allObjectLength = allSelectGUIDs.Length;

            //筛选excel文件
            System.Action<string, int> onAddExcelAssetPathCallBack = (assetPath, index) =>
            {
                EditorUtility.DisplayProgressBar("Collect excel File: ", assetPath, (float)index / (float)allObjectLength);
                if (IsExcelFileOrAsset(assetPath))
                {
                    var fileName = shaco.Base.FileHelper.GetLastFileName(assetPath);

                    //如果是脚本文件默认替换为csv文件做查找
                    if (!shaco.Base.ExcelHelper.IsExcelFile(assetPath))
                    {
                        fileName = shaco.Base.FileHelper.ReplaceLastExtension(fileName, shaco.Base.ExcelDefine.EXTENSION_TXT);
                        assetPath = shaco.GameHelper.excelSetting.GetCustomExportTextPath(assetPath);
                        assetPath = assetPath.ContactPath(fileName);
                    }

                    if (!shaco.Base.FileHelper.ExistsFile(assetPath))
                    {
                        Debug.LogError("CreateExcelScriptMenu ConvertExcelFiles error: not foud path=" + assetPath);
                        return;
                    }

                    excelAssetsPaths.Add(assetPath);
                }
            };
            for (int i = 0; i < allObjectLength; ++i)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(allSelectGUIDs[i]);

                //如果是文件夹则需要遍历
                if (shaco.Base.FileHelper.ExistsDirectory(assetPath))
                {
                    var filesTmp = System.IO.Directory.GetFiles(assetPath);
                    for (int j = 0; j < filesTmp.Length; ++j)
                    {
                        onAddExcelAssetPathCallBack(EditorHelper.FullPathToUnityAssetPath(filesTmp[j]), i);
                    }
                }
                else
                {
                    onAddExcelAssetPathCallBack(assetPath, i);
                }
            }

            //没有发现excel文件
            if (excelAssetsPaths.IsNullOrEmpty())
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            //预加载一次excel配置，因为它不能在子线程中调用
            shaco.GameHelper.excelSetting.GetDataType(string.Empty);

            var exportFolderPath = shaco.Base.FileHelper.GetFolderNameByPath(excelAssetsPaths[0]);
            var typesTemplate = shaco.GameHelper.dateTypesTemplate.GetTypesTemplate();

            //移除需要被过滤后缀名当文件
            if (!ignoreExtension.IsNullOrEmpty())
            {
                for (int i = excelAssetsPaths.Count - 1; i >= 0; --i)
                {
                    for (int j = ignoreExtension.Length - 1; j >= 0; --j)
                    {
                        if (excelAssetsPaths[i].EndsWith(ignoreExtension[j]))
                        {
                            excelAssetsPaths.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            Debug.Log("Total excel files count=" + excelAssetsPaths.Count);

            shaco.Base.Coroutine.ForeachAsync(excelAssetsPaths, (data) =>
            {
                try
                {
                    var assetPath = data.ToString();
                    currentExcelPath = assetPath;

                    var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(assetPath);
                    var exportPathTmp = shaco.Base.FileHelper.ContactPath(exportFolderPath, fileNameTmp);
                    var excelDataTmp = new shaco.Base.ExcelData();
                    var initResult = excelDataTmp.Init(assetPath);

                    if ((int)initResult == (int)shaco.Base.ExcelDefine.ExcelInitResult.Suceess)
                    {
                        onCreateCallBack(excelDataTmp, exportPathTmp, typesTemplate);
                        hasSavedExcel = true;
                    }
                    //处理文件被占用的情况
                    else if ((int)initResult == (int)shaco.Base.ExcelDefine.ExcelInitResult.SharingViolation)
                    {
                        sharingViolationFilesPath.Add(assetPath);
                    }
                }
                catch (System.Exception e)
                {
                    exceptionMessage = e.ToString();
                }
                return !userCancel && string.IsNullOrEmpty(exceptionMessage);
            }, (progress) =>
            {
                if (!string.IsNullOrEmpty(exceptionMessage))
                {
                    Debug.LogError("CreateExcelScriptMenu CreateExcelScript exception=" + exceptionMessage);
                    EditorUtility.ClearProgressBar();
                    return;
                }

                if (progress >= 1.0f)
                {
                    if (!sharingViolationFilesPath.IsNullOrEmpty())
                    {
                        var strAppend = new System.Text.StringBuilder();
                        for (int i = 0; i < sharingViolationFilesPath.Count; ++i)
                        {
                            strAppend.Append("\n\n" + sharingViolationFilesPath[i]);
                        }
                        EditorUtility.DisplayDialog("Warning: File sharing violation", "Please close the open file and try again" + strAppend.ToString(), "OK");
                    }

                    if (hasSavedExcel)
                        AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    userCancel = EditorUtility.DisplayCancelableProgressBar("Creating excel script and text, please wait...", currentExcelPath, progress);
                    if (userCancel)
                    {
                        Debug.Log("CreateExcelScriptMenu CreateExcelScript info: user cancel");
                    }
                }
            });
        }
    }
}
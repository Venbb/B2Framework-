// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;

// namespace shacoEditor
// {
//     public class ExcelHelperWindow : EditorWindow
//     {
//         private ExcelHelperWindow _currentWindow = null;
// 		private string _importPath = string.Empty;
//         private string _exportPath = string.Empty;


//         [MenuItem("shaco/Tools/ExcelHelper", false, (int)ToolsGlobalDefine.MenuPriority.Tools.EXCEL_HELPER_INSPECTOR)]
//         static void OpenExcelHelperWindow()
//         {
//             shacoEditor.EditorHelper.GetWindow<ExcelHelperWindow>(null, true, "ExcelHelper");
//         }

// 		void OnEnable()
// 		{
//             _currentWindow = shacoEditor.EditorHelper.GetWindow<ExcelHelperWindow>(this, true, "ExcelHelper");
//             _currentWindow.LoadSettings();
//         }

// 		void OnDestroy()
// 		{
//             SaveSettings();
// 		}

// 		void OnGUI()
// 		{
// 			if (null == _currentWindow)
// 				return;

//             _importPath = GUILayoutHelper.PathField("Import Excel Path", _importPath, string.Empty);
//             _exportPath = GUILayoutHelper.PathField("Export Script Path", _exportPath, string.Empty);

// 			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_importPath) || string.IsNullOrEmpty(_importPath));
// 			{
//                 if (GUILayout.Button("Generate Code"))
//                 {
// 					//遍历导入目录中所有的excel相关文件
// 					var findFiles = new List<string>();
// 					shaco.Base.FileHelper.GetSeekPath(_importPath, ref findFiles, false, ".xlsx", ".xls", ".csv");

//                     //删除目标目录原有文件
//                     if (findFiles.Count > 0)
// 					{
// 						var deleteFiles = System.IO.Directory.GetFiles(_importPath, "cs", System.IO.SearchOption.TopDirectoryOnly);
// 						for (int i = 0; i < deleteFiles.Length; ++i)
// 						{
// 							shaco.Base.FileHelper.DeleteByUserPath(deleteFiles[i]);
// 						}
//                     }

// 					//自动生成脚本文件到导出目录
// 					for (int i = 0; i < findFiles.Count; ++i)
// 					{
// 						var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(findFiles[i]);
//                         fileNameTmp = shaco.Base.FileHelper.RemoveExtension(fileNameTmp);
// 						var exportPathTmp = shaco.Base.FileHelper.ContactPath(_exportPath, fileNameTmp);
// 						var excelDataTmp = shaco.Base.ExcelHelper.OpenWithFile(findFiles[i]);
// 						excelDataTmp.SerializableAsCSharpScript(exportPathTmp, shaco.GameHelper.dateTypesTemplate.GetTypesTemplate());
// 					}

// 					SaveSettings();

// 					if (findFiles.Count > 0)
// 					{
// 						AssetDatabase.Refresh();
// 					}
// 					else 
// 					{
// 						Debug.LogWarning("ExcelHelperWindow Generate Code warning: no thing need export");
// 					}
//                 }

// 				if (GUILayout.Button("Generate Txt File"))
// 				{
//                     //遍历导入目录中所有的excel相关文件
//                     var findFiles = new List<string>();
//                     shaco.Base.FileHelper.GetSeekPath(_importPath, ref findFiles, false, "xlsx", "xls", "csv");

//                     //删除目标目录原有文件
//                     if (findFiles.Count > 0)
//                     {
//                         var deleteFiles = new List<string>();
//                         shaco.Base.FileHelper.GetSeekPath(_exportPath, ref deleteFiles, false, "txt");
//                         for (int i = 0; i < deleteFiles.Count; ++i)
//                         {
//                             shaco.Base.FileHelper.DeleteByUserPath(deleteFiles[i]);
//                         }
//                     }

//                     //自动转换excel为txt到导出目录
//                     for (int i = 0; i < findFiles.Count; ++i)
//                     {
//                         var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(findFiles[i]);
//                         fileNameTmp = shaco.Base.FileHelper.RemoveExtension(fileNameTmp);
//                         var exportPathTmp = shaco.Base.FileHelper.ContactPath(_exportPath, fileNameTmp);
//                         var excelDataTmp = shaco.Base.ExcelHelper.OpenWithFile(findFiles[i]);
//                         excelDataTmp.SaveAsTxt(exportPathTmp);
//                     }

//                     SaveSettings();

//                     if (findFiles.Count > 0)
//                     {
//                         AssetDatabase.Refresh();
//                     }
//                     else
//                     {
//                         Debug.LogWarning("ExcelHelperWindow Generate Txt File warning: no thing need export");
//                     }
// 				}
// 			}
//             EditorGUI.EndDisabledGroup();
// 		}

// 		private void SaveSettings()
// 		{
//             shaco.GameHelper.datasave.Write("ExcelHelperWindow+_importPath", _importPath);
//             shaco.GameHelper.datasave.Write("ExcelHelperWindow+_exportPath", _exportPath);
// 		}

// 		private void LoadSettings()
// 		{
// 			_importPath = shaco.GameHelper.datasave.ReadString("ExcelHelperWindow+_importPath");
//             _exportPath = shaco.GameHelper.datasave.ReadString("ExcelHelperWindow+_exportPath");
// 		}
//     }
// }
//----------------------------------------------------------------------------------------------------
//该编辑器的作用已经完美被SpriteAtlasSettingsWindow替代了
//----------------------------------------------------------------------------------------------------
// using UnityEngine;
// using UnityEditor;
// using System.IO;
// using System.Linq;

// namespace shacoEditor
// {
//     /// <summary>
//     /// 图片大小分析窗口
//     /// </summary>
//     public class TextureSizeAnalyseWindow : EditorWindow
//     {
//         //判断条件
//         private enum JudgementCondition
//         {
//             GreaterThan,
//             LessThan,
//             Equal
//         }

//         //图片大小超出条件
//         private enum CheckOutOfRangeCondition
//         {
//             //只检查宽
//             WidthOnly,
//             //只检查高
//             HeightOnly,
//             //宽高都检查
//             Both,
//             //宽高满足其中一个 
//             Any
//         }

//         //操作类型
//         private enum OperationType
//         {
//             OutputLog,
//             CopyToExportPath
//         }

//         private Vector2 _maxPixel = new Vector2(512, 512);
//         private string _importPath = string.Empty;
//         private string _exportPath = string.Empty;
//         private bool _isOverwriteWhenFileExisted = false;
//         private JudgementCondition _judgementCondition = JudgementCondition.GreaterThan;
//         private CheckOutOfRangeCondition _checkOutOfRangeCondition = CheckOutOfRangeCondition.Both;
//         private OperationType _operationType = OperationType.OutputLog;

//         [MenuItem("shaco/Tools/TextureSizeAnalyse", false, (int)(int)ToolsGlobalDefine.MenuPriority.Tools.TEXTURE_SIZE_ANALYSE)]
//         static private TextureSizeAnalyseWindow OpenTextureSizeAnalyseWindow()
//         {
//             var retValue = EditorHelper.GetWindow<TextureSizeAnalyseWindow>(null, true, "TextureSizeAnalyseWindow");
//             retValue.Show();
//             retValue.Init();
//             return retValue;
//         }

//         private void Init()
//         {
//             _maxPixel = shaco.GameHelper.datasave.ReadVector2("TextureSizeAnalyseWindow._maxPixel", _maxPixel);
//             _importPath = shaco.GameHelper.datasave.ReadString("TextureSizeAnalyseWindow._importPath", shaco.Base.FileHelper.GetLastFileName(Application.dataPath));
//             _exportPath = shaco.GameHelper.datasave.ReadString("TextureSizeAnalyseWindow._exportPath", shaco.Base.FileHelper.GetLastFileName(Application.dataPath));
//             _isOverwriteWhenFileExisted = shaco.GameHelper.datasave.ReadBool("TextureSizeAnalyseWindow._isOverwriteWhenFileExisted", _isOverwriteWhenFileExisted);
//             _judgementCondition = shaco.GameHelper.datasave.ReadEnum("TextureSizeAnalyseWindow._judgementCondition", _judgementCondition);
//             _checkOutOfRangeCondition = shaco.GameHelper.datasave.ReadEnum("TextureSizeAnalyseWindow._checkOutOfRangeCondition", _checkOutOfRangeCondition);
//             _operationType = shaco.GameHelper.datasave.ReadEnum("TextureSizeAnalyseWindow._operationType", _operationType);
//         }

//         private void OnDestroy()
//         {
//             shaco.GameHelper.datasave.Write("TextureSizeAnalyseWindow._maxPixel", _maxPixel);
//             shaco.GameHelper.datasave.Write("TextureSizeAnalyseWindow._importPath", _importPath);
//             shaco.GameHelper.datasave.Write("TextureSizeAnalyseWindow._exportPath", _exportPath);
//             shaco.GameHelper.datasave.Write("TextureSizeAnalyseWindow._isOverwriteWhenFileExisted", _isOverwriteWhenFileExisted);
//             shaco.GameHelper.datasave.Write("TextureSizeAnalyseWindow._judgementCondition", _judgementCondition);
//             shaco.GameHelper.datasave.Write("TextureSizeAnalyseWindow._checkOutOfRangeCondition", _checkOutOfRangeCondition);
//             shaco.GameHelper.datasave.Write("TextureSizeAnalyseWindow._operationType", _operationType);
//         }

//         private void OnGUI()
//         {
//             _importPath = GUILayoutHelper.PathField("Import Path", _importPath, string.Empty, _importPath);

//             if (string.IsNullOrEmpty(_importPath))
//                 return;

//             if (_operationType == OperationType.CopyToExportPath)
//             {
//                 _exportPath = GUILayoutHelper.PathField("Export Path", _exportPath, string.Empty, _exportPath);
//                 _isOverwriteWhenFileExisted = EditorGUILayout.Toggle("Force overwrite", _isOverwriteWhenFileExisted);
//             }
//             _judgementCondition = (JudgementCondition)EditorGUILayout.EnumPopup("Judgement condition", _judgementCondition);
//             _checkOutOfRangeCondition = (CheckOutOfRangeCondition)EditorGUILayout.EnumPopup("Out of range condition", _checkOutOfRangeCondition);
//             _operationType = (OperationType)EditorGUILayout.EnumPopup("Operation type", _operationType);

//             switch (_checkOutOfRangeCondition)
//             {
//                 case CheckOutOfRangeCondition.WidthOnly: _maxPixel.x = EditorGUILayout.FloatField("Sprite Max Width", _maxPixel.x); break;
//                 case CheckOutOfRangeCondition.HeightOnly: _maxPixel.y = EditorGUILayout.FloatField("Sprite Max Height", _maxPixel.y); break;
//                 case CheckOutOfRangeCondition.Both:
//                 case CheckOutOfRangeCondition.Any:
//                     {
//                         _maxPixel.x = EditorGUILayout.FloatField("Sprite Max Width", _maxPixel.x);
//                         _maxPixel.y = EditorGUILayout.FloatField("Sprite Max Height", _maxPixel.y);
//                         break;
//                     }
//                 default: Debug.LogError("TextureSizeAnalyseWindow Analyse error: unsupport type" + _checkOutOfRangeCondition); break;
//             }

//             if (GUILayout.Button("Analyse"))
//             {
//                 if (_operationType == OperationType.CopyToExportPath && string.IsNullOrEmpty(_exportPath))
//                 {
//                     Debug.LogError("TextureSizeAnalyseWindow Analyse error: must set _exportPath when 'CopyToExportPath' operation");
//                     return;
//                 }

//                 //如果是查找项目根目录，直接提示是否继续
//                 if (_importPath == Application.dataPath || _importPath == "Assets")
//                 {
//                     if (!EditorUtility.DisplayDialog("Warning", "This is the root directory of the project. Confirm whether to continue", "Continue", "Cancel"))
//                     {
//                         return;
//                     }
//                 }

//                 var allFiles = shaco.Base.FileHelper.GetFiles(_importPath, "*", SearchOption.AllDirectories).Where(v => !v.EndsWith(".meta")).ToArray();

//                 //当查找目录文件数量过大时候，提示是否继续
//                 if (allFiles.Length > 1000)
//                 {
//                     if (!EditorUtility.DisplayDialog("Warning", "Operation folder is too large, confirm whether to continue", "Continue", "Cancel"))
//                     {
//                         return;
//                     }
//                 }

//                 var outOfSizeTexturesPath = new System.Collections.Generic.List<string>();

//                 for (int i = allFiles.Length - 1; i >= 0; --i)
//                 {
//                     var pathTmp = allFiles[i];
//                     var loadTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(EditorHelper.FullPathToUnityAssetPath(pathTmp));
//                     if (null == loadTexture)
//                         continue;

//                     bool isOutOfRange = false;
//                     switch (_checkOutOfRangeCondition)
//                     {
//                         case CheckOutOfRangeCondition.WidthOnly:
//                             {
//                                 switch (_judgementCondition)
//                                 {
//                                     case JudgementCondition.GreaterThan: isOutOfRange = loadTexture.width > _maxPixel.x; break;
//                                     case JudgementCondition.LessThan: isOutOfRange = loadTexture.width < _maxPixel.x; break;
//                                     case JudgementCondition.Equal: isOutOfRange = loadTexture.width == _maxPixel.x; break;
//                                 }
//                                 break;
//                             }
//                         case CheckOutOfRangeCondition.HeightOnly:
//                             {
//                                 switch (_judgementCondition)
//                                 {
//                                     case JudgementCondition.GreaterThan: isOutOfRange = loadTexture.height > _maxPixel.y; break;
//                                     case JudgementCondition.LessThan: isOutOfRange = loadTexture.height < _maxPixel.y; break;
//                                     case JudgementCondition.Equal: isOutOfRange = loadTexture.height == _maxPixel.y; break;
//                                 }
//                                 break;
//                             }
//                         case CheckOutOfRangeCondition.Both:
//                             {
//                                 bool condition1 = false;
//                                 bool condition2 = false;
//                                 switch (_judgementCondition)
//                                 {
//                                     case JudgementCondition.GreaterThan: condition1 = loadTexture.width > _maxPixel.x; break;
//                                     case JudgementCondition.LessThan: condition1 = loadTexture.width < _maxPixel.x; break;
//                                     case JudgementCondition.Equal: condition1 = loadTexture.width == _maxPixel.x; break;
//                                 }
//                                 switch (_judgementCondition)
//                                 {
//                                     case JudgementCondition.GreaterThan: condition2 = loadTexture.height > _maxPixel.y; break;
//                                     case JudgementCondition.LessThan: condition2 = loadTexture.height < _maxPixel.y; break;
//                                     case JudgementCondition.Equal: condition2 = loadTexture.height == _maxPixel.y; break;
//                                 }
//                                 isOutOfRange = condition1 && condition2;
//                                 break;
//                             }
//                         case CheckOutOfRangeCondition.Any:
//                             {
//                                 bool condition1 = false;
//                                 bool condition2 = false;
//                                 switch (_judgementCondition)
//                                 {
//                                     case JudgementCondition.GreaterThan: condition1 = loadTexture.width > _maxPixel.x; break;
//                                     case JudgementCondition.LessThan: condition1 = loadTexture.width < _maxPixel.x; break;
//                                     case JudgementCondition.Equal: condition1 = loadTexture.width == _maxPixel.x; break;
//                                 }
//                                 switch (_judgementCondition)
//                                 {
//                                     case JudgementCondition.GreaterThan: condition2 = loadTexture.height > _maxPixel.y; break;
//                                     case JudgementCondition.LessThan: condition2 = loadTexture.height < _maxPixel.y; break;
//                                     case JudgementCondition.Equal: condition2 = loadTexture.height == _maxPixel.y; break;
//                                 }
//                                 isOutOfRange = condition1 || condition2;
//                                 break;
//                             }
//                         default: Debug.LogError("TextureSizeAnalyseWindow Analyse error: unsupport type" + _checkOutOfRangeCondition); break;
//                     }

//                     if (!isOutOfRange)
//                         continue;

//                     outOfSizeTexturesPath.Add(pathTmp);
//                 }

//                 for (int i = outOfSizeTexturesPath.Count - 1; i >= 0; --i)
//                 {
//                     var pathTmp = outOfSizeTexturesPath[i];

//                     switch (_operationType)
//                     {
//                         case OperationType.OutputLog: Debug.Log("Analyse result path=" + pathTmp); break;
//                         case OperationType.CopyToExportPath:
//                             {
//                                 var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(pathTmp);
//                                 var newPathTmp = _exportPath.ContactPath(fileNameTmp);

//                                 if (!_isOverwriteWhenFileExisted && shaco.Base.FileHelper.ExistsFile(newPathTmp))
//                                 {
//                                     shaco.Log.Exception("SpriteAtlasAnalysisWindow Analyse error: file is existed, path=" + newPathTmp);
//                                     break;
//                                 }
//                                 AssetDatabase.MoveAsset(EditorHelper.FullPathToUnityAssetPath(pathTmp), EditorHelper.FullPathToUnityAssetPath(newPathTmp));
//                                 break;
//                             }
//                         default: Debug.LogError("TextureSizeAnalyseWindow Analyse error: unsupport operation type" + _operationType); break;
//                     }
//                 }

//                 if (outOfSizeTexturesPath.Count > 0 && _operationType == OperationType.CopyToExportPath)
//                 {
//                     if (_exportPath.StartsWith("Assets/"))
//                     {
//                         EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(_exportPath));
//                     }
//                     else
//                         System.Diagnostics.Process.Start(_exportPath);
//                 }
//             }
//         }
//     }
// }
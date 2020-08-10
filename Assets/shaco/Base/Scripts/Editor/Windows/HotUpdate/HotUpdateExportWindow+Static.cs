using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace shacoEditor
{
    public partial class HotUpdateExportWindow : EditorWindow
    {
        static public IEnumerable<string> GetCanBuildAssetBundlesAssetPath(string folderPath)
        {
            folderPath = EditorHelper.FullPathToUnityAssetPath(folderPath);
            var findGUIDs = AssetDatabase.FindAssets("t:Object", new string[] { folderPath });
            var findAssetPaths = findGUIDs.Select(v => AssetDatabase.GUIDToAssetPath(v));
            var retValue = findAssetPaths.Where(v => System.IO.File.Exists(v)).ToArray();
            return retValue;
        }

        static public void BuildAssetBundles(string pathFolder, Dictionary<string, shaco.HotUpdateDefine.ExportAssetBundle> allAssetBundles, BuildAssetBundleOptions options)
        {
            shaco.Base.FileHelper.CheckFolderPathWithAutoCreate(pathFolder);

            Debug.Log("HotUpdateExportWindow+Static BuildAssetBundles: set assetbundles information, options=" + options);
#if UNITY_5_3_OR_NEWER
            AssetBundleBuild[] allAssetBundleBuild = new AssetBundleBuild[allAssetBundles.Count];
            int index = 0;

            foreach (var key in allAssetBundles.Keys)
            {
                var value = allAssetBundles[key];

                var assetsTmp = new List<string>();
                foreach (var iter in value.ListFiles)
                {
                    assetsTmp.Add(iter.Key);
                }

                allAssetBundleBuild[index] = GetAssetBundleBuild(key, assetsTmp.ToArray());
                ++index;
            }

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;

            shaco.Base.FileHelper.CheckFolderPathWithAutoCreate(pathFolder.AddBehindNotContains(shaco.Base.FileDefine.PATH_FLAG_SPLIT_STRING));

            if (allAssetBundleBuild.Length > 0)
            {
                Debug.Log("HotUpdateExportWindow+Static BuildAssetBundles: build assetbundle start");
                BuildPipeline.BuildAssetBundles(pathFolder, allAssetBundleBuild, options, buildTarget);
            }
#else
            //每N个打包后需要回收一次内存，否则因为LoadAssetAtPath导致内存溢出
            int recoveryCount = 10;
            int buildCount = 0;
            
            Debug.Log("HotUpdateExportWindow+Static BuildAssetBundles: legacy build assetbundle start");
            foreach (var iter in allAssetBundles)
            {
                Object[] assetsTmp = new Object[iter.Value.ListFiles.Count];
                int index2 = 0;
                foreach (var iter2 in iter.Value.ListFiles)
                {
                    assetsTmp[index2++] = AssetDatabase.LoadAssetAtPath(iter2.Key, typeof(Object));
                    ++buildCount;
                }

                if (assetsTmp.Length > 0)
                {
                    var fullPath = shaco.Base.FileHelper.ContactPath(pathFolder, iter.Key);
                    var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                    shaco.Base.FileHelper.CheckFolderPathWithAutoCreate(shaco.Base.FileHelper.GetFolderNameByPath(fullPath));
                    BuildPipeline.BuildAssetBundle(assetsTmp[0], assetsTmp, fullPath, options, buildTarget);
                }

                if (buildCount >= recoveryCount)
                {
                    buildCount = 0;
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                }
            }
#endif       
        }

        // static public void CopyResourcesToResourcesHotUpdate(List<string> keepFolderPaths)
        // {
        //     foreach (var keepFolderPath in keepFolderPaths)
        //     {
        //         var keepFolderPathLower = keepFolderPath.ToLower();
        //         if (keepFolderPathLower.Contains(shaco.GameHelper.res.DEFAULT_PREFIX_PATH_LOWER))
        //         {
        //             var hotupdatePathTmp = shaco.Base.FileHelper.ContactPath(Application.dataPath, keepFolderPathLower.Remove("assets/"));
        //             var resourcesPathTmp = shaco.Base.FileHelper.ContactPath(Application.dataPath, keepFolderPathLower.Replace(shaco.GameHelper.res.DEFAULT_PREFIX_PATH_LOWER, "Resources/"));

        //             shaco.Base.FileHelper.CopyFileByUserPath(resourcesPathTmp, hotupdatePathTmp);
        //         }
        //     }
        // }

        static public Dictionary<string, string> BuildOriginalFiles(string pathFolder, Dictionary<string, shaco.HotUpdateDefine.ExportAssetBundle> orignialFiles, bool isAutoEncryptOriginalFile, bool isAutoCompress)
        {
            var retValue = new Dictionary<string, string>();
            var applicationPathTmp = Application.dataPath.Replace('\\', '/').Remove("Assets");

            foreach (var iter in orignialFiles)
            {
                foreach (var iter2 in iter.Value.ListFiles)
                {
                    var sourceFileNameTmp = iter2.Key.ToLower();
                    var assetBundleKey = shaco.Base.FileHelper.ReplaceLastExtension(sourceFileNameTmp, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
                    var sourcePathTmp = shaco.Base.FileHelper.ContactPath(applicationPathTmp, sourceFileNameTmp);
                    var destinationPathTmp = shaco.Base.FileHelper.ContactPath(pathFolder, assetBundleKey);

                    //查看带有md5的目标文件是否存在，如果存在，则不用再次导出原始文件了
                    var destinationPathWithMD5Tmp = shaco.HotUpdateHelper.AddAssetBundleNameTag(destinationPathTmp, iter.Value.AssetBundleMD5);
                    if (shaco.HotUpdateHelper.IsKeepOriginalFile(destinationPathWithMD5Tmp))
                    {
                        continue;
                    }
                    else
                    {
                        //因为现在在打包保留文件，如果存在非保留文件，则优先删除掉它
                        if (shaco.Base.FileHelper.ExistsFile(destinationPathWithMD5Tmp))
                        {
                            shaco.Base.FileHelper.DeleteByUserPath(destinationPathWithMD5Tmp);
                        }
                    }

                    //拷贝原始文件
                    shaco.Base.FileHelper.CopyFileByUserPath(sourcePathTmp, destinationPathTmp);

                    //压缩文件
                    var encryptTag = new List<string>();
                    encryptTag.Add(shaco.HotUpdateDefine.ORIGINAL_FILE_TAG);
                    if (isAutoCompress)
                    {
                        shaco.GameHelper.zip.Zip(destinationPathTmp, destinationPathTmp);
                        encryptTag.Add(shaco.HotUpdateDefine.COMPRESSED_FILE_TAG);
                    }

                    //原始文件加密，无论是否加密文件内容，都需要加密文件头，因为会写入是否为原始文件的标记，以方便在读取的时候判断
                    var secretRandomCode = shaco.Base.Utility.Random(int.MinValue, int.MaxValue);
                    shaco.Base.EncryptDecrypt.EncryptPath(destinationPathTmp, isAutoEncryptOriginalFile ? 3.0f : 1.0f, secretRandomCode, encryptTag.ToArray());

                    //删除manifest
                    shaco.HotUpdateHelper.DeleteManifest(destinationPathTmp);

                    //记录导出的源文件路径
                    if (!retValue.ContainsKey(assetBundleKey))
                        retValue.Add(assetBundleKey, null);
                }
            }
            return retValue;
        }

        //计算单个assetbundle文件的md5
        // static public string ComputerAssetBundleInfoMD5(shaco.HotUpdateDefine.ExportAssetBundle exportInfo, shaco.HotUpdateDefine.SerializeVersionControl versionControl, shaco.HotUpdateManifestInfo.ManifestInfo manifestInfo)
        // {
            // string retValue = string.Empty;

            // //computer all files to one md5
            // var strAllFilesMD5 = new System.Text.StringBuilder();
            // foreach (var iter2 in exportInfo.ListFiles)
            // {
                // var pathTmp = iter2.Key.ToLower();
                // var readFileTmp = shaco.Base.FileHelper.ReadAllByteByUserPath(pathTmp);
                // if (readFileTmp == null)
                // {
                //     DisplayDialogError("can't read string by user path=" + pathTmp);
                // }

                //computer file md5
                // strAllFilesMD5.Append(shaco.Base.FileHelper.MD5FromByte(readFileTmp));

                // //computer meta md5
                // var pathMetaTmp = shaco.Base.FileHelper.AddExtensions(pathTmp, shaco.HotUpdateDefine.EXTENSION_META);
                // pathMetaTmp = pathMetaTmp.Remove("assets");
                // pathMetaTmp = shaco.Base.FileHelper.ContactPath(Application.dataPath, pathMetaTmp);
                // strAllFilesMD5.Append(shaco.Base.FileHelper.MD5FromFile(pathMetaTmp));

                // //计算所有引用文件的md5
                // string[] listDependence = null;
                // var keyCheck = shaco.Base.FileHelper.ReplaceLastExtension(pathTmp, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
                // if (!manifestInfo.dependenies.TryGetValue(keyCheck, out listDependence))
                // {
                //     //没有引用就添加自己
                //     listDependence = new string[] { pathTmp };
                // }
                // else
                // {
                //     //补充自己
                //     var newDependece = new string[listDependence.Length + 1];
                //     for (int i = listDependence.Length - 1; i >= 0; --i)
                //         newDependece[i] = listDependence[i];
                //     newDependece[newDependece.Length - 1] = pathTmp;
                //     listDependence = newDependece;
                // }

                // for (int i = listDependence.Length - 1; i >= 0; --i)
                // {
                //     // //获取项目中源文件
                //     // var fullDependenceFilePath = EditorHelper.GetFullPath(listDependence[i]);
                //     // var dependenceMD5 = shaco.Base.FileHelper.MD5FromFile(fullDependenceFilePath);
                //     // strAllFilesMD5.Append(dependenceMD5);

                //     // //计算meta的md5
                //     // if (!fullDependenceFilePath.Contains(shaco.HotUpdateDefine.IGNORE_FILE_META_MD5_TAG))
                //     // {
                //     //     var readMetaMD5String = shaco.Base.FileHelper.ReadAllByUserPath(fullDependenceFilePath + ".meta");
                //     //     var dependenceMetaMD5 = shaco.Base.FileHelper.MD5FromString(readMetaMD5String);
                //     //     strAllFilesMD5.Append(dependenceMetaMD5);
                //     // }
                // }

                //如果是作为源文件打包，需要计算加密规则md5
                // if (versionControl.ExportFilesFormat == shaco.HotUpdateDefine.ExportFileFormat.OriginalFile)
                // {
                //     strAllFilesMD5.Append("_" + versionControl.AutoEncryt);
                //     strAllFilesMD5.Append("_" + versionControl.AutoCompressdFile);
                // }
        //     }

        //     retValue = shaco.Base.FileHelper.MD5FromString(strAllFilesMD5.ToString());
        //     return retValue;
        // }

#if UNITY_5_3_OR_NEWER
        static public AssetBundleBuild GetAssetBundleBuild(string filename, string[] assets)
        {
            AssetBundleBuild ret = new AssetBundleBuild();

            string[] assetPath = new string[assets.Length];
            for (int i = 0; i < assets.Length; ++i)
            {
                assetPath[i] = assets[i].ToLower();

                //this bug has fixed in unity version 5.3 or newer
                // for (int i = 0; i < assets.Length; ++i)
                // {
                //     assetPath[i] = EditorHelper.GetAssetPathLower(assets[i]);

                //     //if have spritePackingTag, we can't read texture object from assetbundle
                //     //but can read texture by 'Resource.Load', unknown reason now...
                //     //fixed: we force set spritePackingTag is empty string can fix it
                //     var texImportTmp = AssetImporter.GetAtPath(assetPath[i]) as TextureImporter;
                //     if (texImportTmp != null && !string.IsNullOrEmpty(texImportTmp.spritePackingTag))
                //     {
                //         texImportTmp.spritePackingTag = string.Empty;
                //         AssetDatabase.ImportAsset(assetPath[i]);
                //     }
                // }
            }

            ret.assetBundleName = filename;
            ret.assetNames = assetPath;

            return ret;
        }
#endif

        static public UnityEditor.BuildTarget GetBuildTargetByPlatForm(shaco.HotUpdateDefine.Platform platform)
        {
            UnityEditor.BuildTarget ret = UnityEditor.BuildTarget.NoTarget;

            switch (platform)
            {
                case shaco.HotUpdateDefine.Platform.Android:
                    {
                        ret = UnityEditor.BuildTarget.Android;
                        break;
                    }

                case shaco.HotUpdateDefine.Platform.iOS:
                    {
#if UNITY_5_3_OR_NEWER
                        ret = UnityEditor.BuildTarget.iOS;
#else
                        ret = UnityEditor.BuildTarget.iPhone;
#endif
                        break;
                    }
                case shaco.HotUpdateDefine.Platform.WebGL:
                    {
                        ret = UnityEditor.BuildTarget.WebGL;
                        break;
                    }
                default:
                    {
                        ret = UnityEditor.BuildTarget.Android;
                        DisplayDialogError("HoUpdateExportEditor GetBuildTargetByPlatForm error: unsupport platform=" + platform);
                        break;
                    }
            }

            return ret;
        }

        static public List<string> ObjectsToStrings(List<Object> listObjects)
        {
            List<string> ret = new List<string>();
            for (int i = 0; i < listObjects.Count; ++i)
            {
                if (listObjects[i] != null)
                {
                    var pathTmp = EditorHelper.GetAssetPathLower(listObjects[i]);
                    ret.Add(pathTmp);
                }
            }
            return ret;
        }

        /// <summary>
        /// 获取导出的assetbundle文件路径
        /// <param name="pathRoot">导出根目录，一般为asset</param>
        /// <param name="assetbundleInfo">导出文件信息</param>
        /// <return>导出的assetbundle文件路径</return>
        /// </summary>
        static private string GetAssetBundleFullPath(string pathRoot, shaco.HotUpdateDefine.ExportAssetBundle assetbundleInfo)
        {
            return GetAssetBundleFullPath(pathRoot, assetbundleInfo.AssetBundleName, assetbundleInfo.AssetBundleMD5);
        }

        /// <summary>
        /// 获取导出的assetbundle文件路径
        /// <param name="pathRoot">导出根目录，一般为asset</param>
        /// <param name="assetbundleName">assetbundle名字</param>
        /// <param name="assetbundleMD5">assetbundle的MD5</param>
        /// <return>导出的assetbundle文件路径</return>
        /// </summary>
        static private string GetAssetBundleFullPath(string pathRoot, string assetbundleName, string assetbundleMD5)
        {
            var fileNameTmp = shaco.HotUpdateHelper.AssetBundleKeyToPath(assetbundleName);

            int findIndexTmp = pathRoot.LastIndexOf("/assets");
            if (findIndexTmp >= 0)
            {
                pathRoot = pathRoot.Remove(findIndexTmp);
            }

            var fullPathTmp = pathRoot.ContactPath(fileNameTmp);

            if (!shaco.Base.FileHelper.ExistsFile(fullPathTmp))
            {
                fullPathTmp = shaco.HotUpdateHelper.AddAssetBundleNameTag(fullPathTmp, assetbundleMD5);
            }

            return fullPathTmp;
        }
    }
}
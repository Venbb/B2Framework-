using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class UpdateTool
    {
        static private readonly string DOWNLOAD_TEMPORY_PATH = Application.dataPath.RemoveLastPathByLevel(1) + "/shaco_download_tmp";
        static private readonly string DOWNLOAD_URL = "https://codeload.github.com/shaco-6/shaco/zip/master";

        //预估要下载的包体大小
        static private readonly double PACKAGE_SIZE = 6.82 * 1024 * 1024;

        static IEnumerator DownloadZip(string url, System.Func<float, bool> callbackProgress, System.Action<byte[], string> callbackCompleted)
        {
            bool forceStopDownload = false;
#if UNITY_5_3_OR_NEWER
            var www = UnityEngine.Networking.UnityWebRequest.Get(url);
    #if UNITY_2018_1_OR_NEWER
            www.SendWebRequest();
    #else
            www.Send();
    #endif
#else
            var www = new WWW(url);
#endif
            {
                shaco.Base.WaitFor.Run(() =>
                {
                    if (null != callbackProgress)
                    {
                        //下载过程中，强制提前停止下载
#if UNITY_5_3_OR_NEWER
                        float currentProgress = (float)((double)www.downloadedBytes / (double)PACKAGE_SIZE);
#else
                        float currentProgress = www.progress;
#endif
                        if (callbackProgress(currentProgress))
                        {
                            forceStopDownload = true;
                        }
                    }
                    return forceStopDownload || www.isDone || !string.IsNullOrEmpty(www.error);
                }, () =>
                {
                    if (null != callbackCompleted)
                    {
                        if (forceStopDownload)
                        {
                            callbackCompleted(null, string.Empty);
                        }
                        else
                        {
#if UNITY_5_3_OR_NEWER
                            callbackCompleted(www.downloadHandler.data, www.error);
#else
                            callbackCompleted(www.bytes, www.error);
#endif
                        }
                    }
                    www.Dispose();
                });
            }

            yield return 1;
        }

        [MenuItem("shaco/Tools/UpdateShacoGameFrameWork _F12", false, (int)ToolsGlobalDefine.MenuPriority.Tools.UPDATE_FRAMEWORK)]
        static private void OpenUpdateWindow()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogError("UpdateTool OpenUpdateWindow error: cannot be allowed when compiling code");
                return;
            }
            if (EditorUtility.DisplayDialog("Update", "will update 'shaco' GameFrameWork", "Continue", "Cancel"))
            {
                UpdatePackages();
            }
        }

        static private bool IsIgnoreFile(string[] ignoreFilesTmp, string filename)
        {
            for (int i = 0; i < ignoreFilesTmp.Length; ++i)
            {
                if (filename.Contains(ignoreFilesTmp[i]))
                {
                    return true;
                }
            }
            return false;
        }

        static private List<string> GetFilesWithoutGitHubDirectories(string rootPath)
        {
            List<string> retValue = new List<string>();
            shaco.Base.FileHelper.GetSeekPath(rootPath, ref retValue, (string value) =>
            {
                return !value.Contains(".git") && value.LastIndexOf("cs.meta") < 0;
            });
            return retValue;
        }

        static private void UpdatePackages()
        {
            //从github下载资源包
            // var httpTmp = new shaco.Base.HttpHelper();
            bool userCancel = false;

            try
            {
                //issue: 
                //c# http方法无法访问https的githube
                //因为github需要SecurityProtocolType.Tls1.2
                //而HttpHelper中使用的HttpWebRequest的.Net库需要4.5以上版本才支持，所以暂时用UnityWebRequest代替HttpHelper吧
                // httpTmp.SetAutoSaveWhenCompleted(downloadedZipPath);
                // httpTmp.Download(DOWNLOAD_URL);

                userCancel = EditorUtility.DisplayCancelableProgressBar("Update", "will start update", 0);

                shaco.GameHelper.StartCoroutine(DownloadZip(DOWNLOAD_URL, (float progress) =>
                {
                    userCancel = EditorUtility.DisplayCancelableProgressBar("Update From GitHub", "Please wait", progress);
                    return userCancel;

                }, (byte[] data, string error) =>
                {
                    OnDownloadedEnd(userCancel, data, error);
                }));
            }
            catch (System.Exception e)
            {
                Debug.LogError("Update error: " + e);
                // httpTmp.CloseClient();
                EditorUtility.ClearProgressBar();
            }
        }

        //删除xlua支持文件
        //ps:如果xlua想支持WebGL请在xlua的github官方地址下载WebGLPlugins文件夹放到Assets所在同级目录
        //   因为WebGLPlugins文件夹如果放到Assets目录中会导致WebGL打包编译失败，所以shaco框架不包含WebGLPlugins目录。需要手动下载
        //xlua github地址:https://github.com/Tencent/xLua
        static private void RemoveXluaPackage(string localPath)
        {
            var xluaFolderTmp = localPath.ContactPath("Base/Library/XLua");
            if (shaco.Base.FileHelper.ExistsDirectory(xluaFolderTmp))
            {
                shaco.Base.FileHelper.DeleteByUserPath(xluaFolderTmp);
            }
        }

        static private void OnDownloadedEnd(bool userCancel, byte[] data, string error)
        {
            if (null != data)
            {
                Debug.Log("data=" + data.Length);
            }
            if (userCancel)
            {
                Debug.Log("Manual Cancel Update");
                // httpTmp.CloseClient();
                EditorUtility.ClearProgressBar();
                return;
            }

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError("Update error: " + error);
                // httpTmp.CloseClient();
                EditorUtility.ClearProgressBar();
                return;
            }

            var downloadedZipPath = DOWNLOAD_TEMPORY_PATH + ".zip";
            shaco.Base.FileHelper.WriteAllByteByUserPath(downloadedZipPath, data);

            UnzipAndUpdateFramework();
            EditorUtility.ClearProgressBar();
        }

        static private void UnzipAndUpdateFramework()
        {
            var downloadedZipPath = DOWNLOAD_TEMPORY_PATH + ".zip";
            var downloadedUnZipPath = DOWNLOAD_TEMPORY_PATH;
            var localPath = shaco.Base.GlobalParams.GetShacoFrameworkRootPath();

            var downloadedPaths = new List<string>();
            var localPaths = new List<string>();
            var willRemovePaths = new List<string>();
            var downloadedPathsDic = new Dictionary<string, string>();

            //解压zip
            shaco.GameHelper.zip.UnZip(downloadedZipPath, downloadedUnZipPath);

            //过滤git自带的根目录
            downloadedUnZipPath = System.IO.Directory.GetDirectories(downloadedUnZipPath)[0].Replace('\\', '/');

            //重命名解压出来的文件夹名字
            if (downloadedUnZipPath.StartsWith("//"))
            {
                downloadedUnZipPath = downloadedUnZipPath.ReplaceFromBegin("//", "\\\\", 1);
            }

            Debug.Log("downloadedUnZipPath=" + downloadedUnZipPath + " localPath=" + localPath);

            //收集github上没有的文件，但是本地有的文件，准备进行删除
            if (downloadedUnZipPath.StartsWith("//"))
            {
                downloadedUnZipPath = downloadedUnZipPath.ReplaceFromBegin("//", "\\\\", 1);
            }
            downloadedPaths = GetFilesWithoutGitHubDirectories(downloadedUnZipPath);

            //收集本地的文件
            localPaths = GetFilesWithoutGitHubDirectories(localPath);

            Debug.Log("downloadedPaths=" + downloadedPaths.Count + " localPaths=" + localPaths.Count);

            for (int i = 0; i < downloadedPaths.Count; ++i)
            {
                var pathTmp = downloadedPaths[i].RemoveFront("shaco_download_tmp");
                pathTmp = pathTmp.Replace("shaco-master", "shaco");
                downloadedPathsDic.Add(pathTmp, pathTmp);
            }
            for (int i = 0; i < localPaths.Count; ++i)
            {
                var pathTmp = localPaths[i].Remove(Application.dataPath);
                if (!downloadedPathsDic.ContainsKey(pathTmp))
                {
                    willRemovePaths.Add(localPaths[i]);
                }
            }

            //删除本地多余文件
            for (int i = 0; i < willRemovePaths.Count; ++i)
            {
                //删除文件
                shaco.Base.FileHelper.DeleteByUserPath(willRemovePaths[i]);

                // //删除meta文件
                shaco.Base.FileHelper.DeleteByUserPath(willRemovePaths[i] + ".meta");

                // //删除空白文件夹 
                shaco.Base.FileHelper.DeleteEmptyFolder(willRemovePaths[i], ".meta");
            }

            //如果本地已经存在GameHelper的配置文件，则不再更新，默认使用项目自身的独立配置
            CheckNotReplaceTemporaryFile(shaco.Base.GameHelper.GameHelperConfigPath, downloadedUnZipPath);

#if UNITY_2018_4_OR_NEWER
            //在Unity2018.4.0以上版本已经自带了System.Data.dll，所以Shaco框架中的该dll不再使用
            var systemDataDllPath = shaco.Base.GlobalParams.GetShacoFrameworkRootPath().ContactPath("Base/Scripts/CSharp/Excel/System.Data.dll.meta");
            CheckNotReplaceTemporaryFile(systemDataDllPath, downloadedUnZipPath);
#endif

            //替换和新增本地文件
            Debug.Log("copy downloadedUnZipPath=" + downloadedUnZipPath + " des=" + localPath);
            shaco.Base.FileHelper.CopyFileByUserPath(downloadedUnZipPath, localPath, "cs.meta");

            //删除下载的资源包
            shaco.Base.FileHelper.DeleteByUserPath(downloadedZipPath);
            shaco.Base.FileHelper.DeleteByUserPath(DOWNLOAD_TEMPORY_PATH);

#if !UNITY_5_3_OR_NEWER
            //在Unity4.x版本不能过滤dll库的编译，如果存在xlua/tools目录中的部分dll库则会导致编译报错(System.dll)
            //xlua支持的版本在Unity5.3以上，所以5.x以下的xlua目录直接删掉吧
            Debug.Log("delete xlua directory, beacause not support in Unity5.2 or lower");
            RemoveXluaPackage(localPath);
#else
            //如果项目中已经包含Xlua文件夹，则删除shaco框架中重复的xlua文件夹
            if (shaco.Base.FileHelper.ExistsDirectory(Application.dataPath.ContactPath("XLua")))
            {
                Debug.Log("delete duplicate xlua directory");
                RemoveXluaPackage(localPath);
            }
#endif

            // httpTmp.CloseClient();
            AssetDatabase.Refresh();
        }

        static private void CheckNotReplaceTemporaryFile(string sourceFilePath, string downloadedUnZipPath)
        {
            Debug.Log("check not replace file=" + sourceFilePath);
            if (shaco.Base.FileHelper.ExistsFile(sourceFilePath))
            {
                var needDeleteTmpGameHelperPath = downloadedUnZipPath.ContactPath(sourceFilePath.Remove(shaco.Base.GlobalParams.GetShacoFrameworkRootPath()));
                shaco.Base.FileHelper.DeleteByUserPath(needDeleteTmpGameHelperPath);
            }
        }
    }
}
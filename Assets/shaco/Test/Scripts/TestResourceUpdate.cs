using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Reflection;

namespace shaco.Test
{
    public class TestResourceUpdate : MonoBehaviour
    {
        //资源的版本管理路径
        //1、如果为空字符串则表示为全局资源，则全路径为 则全路径自动拼接为 xxx/VersionControl@@Android/xxx
        //2、如果不为空则使用多版本资源，例如该值为version2，则全路径自动拼接为 xxx/version2/VersionControl@@Android/xxx
        public const string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString;

        public string urlVersion;
        public UnityEngine.UI.Text TextProgressImprecise;
        public UnityEngine.UI.Text TextProgressPrecision;
        public UnityEngine.UI.Text TextDownloadSpeed;
        public GameObject UIRootParent;
        public UnityEngine.UI.Image ImageTarget;

        void Awake()
        {
            // shaco.Localization.LoadWithJsonResourcesPathLanguage(string.Empty, SystemLanguage.Chinese);
        }

        void Start()
        {
            shaco.HotUpdateHelper.SetDynamicResourceAddress(urlVersion + multiVersionControlRelativePath);
        }

        void Update()
        {

        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                //检查并热更新资源
                if (TestMainMenu.DrawButton("CheckUpdate"))
                {
                    var updateHelper = shaco.GameHelper.hotupdate;
                    updateHelper.CheckUpdate(urlVersion, string.Empty, null, multiVersionControlRelativePath);

                    updateHelper.onCheckVersionEndCallBack.AddCallBack(this, (object sender) =>
                    {
                        shaco.Log.Info("Version=" + updateHelper.GetVersion() + " need update size=" + updateHelper.GetCurrentNeedUpdateDataSize() + " status=" + updateHelper.GetStatusDescription());
                    });

                    updateHelper.onUpdatingCallBack.AddCallBack(this, (object sender) =>
                    {
                        TextProgressPrecision.text = updateHelper.GetDownloadResourceProgress().ToString();
                        TextDownloadSpeed.text = "Speed:" + updateHelper.GetDownloadSpeedFormatString();

                        shaco.Log.Info("progress =" + updateHelper.GetDownloadResourceProgress());
                    });

                    updateHelper.onUpdateEndCallBack.AddCallBack(this, (object sender) =>
                    {
                        if (updateHelper.HasError())
                        {
                            shaco.Log.Info("ResourceUpdateTest error: msg=" + updateHelper.GetLastError(), Color.white);
                        }

                        if (updateHelper.IsSuccess())
                        {
                            shaco.Log.Info("check test end status=" + updateHelper.GetStatusDescription(), Color.white);
                        }
                    });
                }

                //仅检查本地资源是否需要更新
                if (TestMainMenu.DrawButton("CheckUpdateLocalOnly"))
                {
                    var isNeedUpdate = shaco.HotUpdateHelper.CheckUpdateLocalOnly("1.0", multiVersionControlRelativePath);
                    Debug.Log("update resource flag=" + isNeedUpdate);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("LoadAssetbundle"))
                {
                    ImageTarget.sprite = shaco.GameHelper.res.LoadResourcesOrLocal<Sprite>("2");
                }

                if (TestMainMenu.DrawButton("LoadAssetbundleAsync"))
                {
                    bool isloadstart = false;
                    shaco.GameHelper.res.LoadResourcesOrLocalAsync<Sprite>("icon-1024".AutoUnLoad(this), (obj) =>
                    {
                        Debug.Log("LoadAssetbundle=" + obj);
                        ImageTarget.sprite = obj;
                    }, (float percent) =>
                    {
                        Debug.Log("load progress=" + percent);

                        if (!isloadstart)
                        {
                            isloadstart = true;
                            shaco.GameHelper.res.LoadResourcesOrLocalAsync<Texture2D>("icon-1024".AutoUnLoad(this), (obj) =>
                            {
                                Debug.Log("LoadAssetbundle2=" + obj);
                            });
                        }
                    }, multiVersionControlRelativePath);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("LoadAssetbundleAsync(Sequeue)"))
                {
                    var resourceSequeue = new shaco.ResourcesOrLocalSequeue();

                    resourceSequeue.AddRequest<UnityEngine.Object>("TestBehaivourTree.json", (UnityEngine.Object obj) =>
                    {
                        Debug.Log("LoadAssetbundleAsync 1=" + obj);
                    }, multiVersionControlRelativePath);
                    resourceSequeue.AddRequest<UnityEngine.Object>("TestNewGuide.bytes", (UnityEngine.Object obj) =>
                    {
                        Debug.Log("LoadAssetbundleAsync 2=" + obj);
                    }, multiVersionControlRelativePath);

                    resourceSequeue.Start((float percent) =>
                    {
                        Debug.Log("load progress=" + percent);
                    });
                }

                if (TestMainMenu.DrawButton("LoadFolderAsync(dynamic)"))
                {
                    //先删除本地文件以模拟动态下载文件功能
                    var loadPath = "assets/resources_hotupdate2/config/cfg_text.csv";
                    var fullPath = HotUpdateHelper.GetAssetBundleFullPath(loadPath, multiVersionControlRelativePath);
                    fullPath = System.IO.Path.ChangeExtension(fullPath, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);

                    //如果事先有设置过动态下载地址(shaco.HotUpdateHelper.SetDynamicResourceAddress)
                    //则在加载文件夹失败的时候，会自动从动态地址中下载文件夹中的文件
                    shaco.GameHelper.res.LoadResourcesOrLocalAsync<UnityEngine.Object>(loadPath, (obj)=>
                    {
                        Debug.Log("load obj=" + obj);
                    }, (float percent)=>
                    {
                        Debug.Log("load percent=" + percent);       
                    });
                }

                if (TestMainMenu.DrawButton("LoadCoroutine"))
                {
                    StartCoroutine(TestCoroutine());
                }
            }
            GUILayout.EndHorizontal();

            if (TestMainMenu.DrawButton("ExistsResourcesOrLocal"))
            {
                bool exists = shaco.GameHelper.res.ExistsResourcesOrLocal("assets/resources_hotupdate2/config/cfg_text.csv", typeof(UnityEngine.Object), multiVersionControlRelativePath);
                Debug.Log("exists=" + exists);
            }

            if (TestMainMenu.DrawButton("UnloadAssetbundle"))
            {
                var result = shaco.GameHelper.res.UnloadAssetBundleLocal("assets/resources_hotupdate2/config/cfg_text.csv", true, multiVersionControlRelativePath);
                Debug.Log("unload assetbundle result=" + result);
            }

            TestMainMenu.DrawBackToMainMenuButton();
        }

        private IEnumerator TestCoroutine()
        {
            yield return 1;
            var loadRequest = shaco.GameHelper.res.LoadResourcesOrLocalCoroutine<TextAsset>("assets/resources_hotupdate2/config/cfg_activity.csv");
            while (!loadRequest.isDone)
            {
                Debug.Log("load progress=" + loadRequest.progress + " result=" + loadRequest.resultType);
                yield return 1;
            }
            Debug.Log("111 progress=" + loadRequest.progress + " result=" + loadRequest.resultType + " obj=" + loadRequest.value);

            var loadRequest2 = shaco.GameHelper.res.LoadResourcesOrLocalCoroutine<TextAsset>("assets/resources_hotupdate2/config/cfg_activity.csv");
            yield return loadRequest2;
            Debug.Log("222 progress=" + loadRequest.progress + " result=" + loadRequest.resultType + " obj=" + loadRequest.value);
        }
    }
}
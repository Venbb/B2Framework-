using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using B2Framework;

namespace B2Framework.Editor
{
    public static class InitializeOnLoad
    {
        /// <summary>
        /// Editor初始化
        /// </summary>
        [InitializeOnLoadMethod]
        private static void OnEditorInitialize()
        {
            UnityEditor.EditorUtility.ClearProgressBar();
            // 创建Settings到Resources目录
            var settings = BuildHelper.GetSettings();
            // 打包规则
            BuildHelper.GetBuildRules();
            // 打包设置
            BuildHelper.GetBuildSettings();
            // 创建Manifest配置
            BuildHelper.GetManifest();
            // 扩展工具配置
            BuildHelper.GetExTools();

            // Debug.Log("System.Environment.CurrentDirectory:"+System.Environment.CurrentDirectory);

            // Editor模式下，指定加载资源的方法
            GameUtility.Assets.loadHander = AssetDatabase.LoadAssetAtPath;
            // 设置当前语言
            Localization.SwitchLanguage(settings.language, false);
            // Debug.Log(Application.systemLanguage);
            // SystemLanguage.ChineseSimplified
            // Debug.Log(System.Threading.Thread.CurrentThread.CurrentCulture);
            // var cultures = System.Globalization.CultureInfo.GetCultureInfo("zh-HK").DisplayName;
            // var cultures = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
            // settings.cultureInfos = new string[cultures.Length];
            // for (int i = 0; i < cultures.Length; i++)
            //     settings.cultureInfos[i] = cultures[i].DisplayName + ":" + cultures[i].Name;

            // var str = "Editor模式下，指定加载资源的方法";
            // var watch = new Stopwatch();
            // watch.Start();
            // var source = Utility.Zip.Compress(str);
            // watch.Stop();
            // Debug.Log("Compress " + watch.ElapsedMilliseconds + " ms.");
            // Debug.Log(source);

            // source = Utility.Zip.Decompress(source);
            // Debug.Log("Decompress " + watch.ElapsedMilliseconds + " ms.");
            // Debug.Log(source);
        }
        /// <summary>
        /// 游戏开始运行
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void OnInitialize()
        {
            // var settings = BuildHelper.GetSettings();
            // if (!settings.debugEnable)
            // {
            //     var reporter = GameObject.FindObjectOfType<Reporter>();
            //     if (reporter != null)
            //         GameObject.DestroyImmediate(reporter.gameObject);
            // }
        }
    }
}

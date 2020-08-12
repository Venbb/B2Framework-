using UnityEngine;

namespace B2Framework.Unity
{
    public class Settings : ScriptableObject
    {
        [Header("Base")]
        [Tooltip("游戏帧率")] public int frameRate = 30;
        [Tooltip("游戏速度")] public float gameSpeed = 1f;
        [Tooltip("是否后台运行")] public bool runInBackground = true;
        [Tooltip("是否开启日志")] public bool debugEnable = true;
        [Tooltip("是否永不睡眠")] public bool neverSleep = true;
        [Tooltip("语言")] public GameLanguage language = GameLanguage.ChineseSimplified;



        [Header("Assets")]
        [Tooltip("是否开启运行时模式，默认从工程目录加载资源，开启后将从指定位置加载AB资源")] public bool runtimeMode;
        [ReadOnly] [Tooltip("（只读）资源构建平台，打包时选择")] public string buildPlatform;
        [Tooltip("资源服务器地址")] public string downloadURL = "http://127.0.0.1/b2";
    }
}
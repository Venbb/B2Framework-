// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace shaco
// {
//     public interface IXLuaManager : shaco.Base.IGameInstance
//     {
//         /// <summary>
//         /// 运行目录下所有lua脚本
//         /// </summary>
//         /// <param name="path">目录路径，支持从Unity的Resources目录，或者下载到本地的asset/resources_hotupdate目录</param>
//         /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
//         /// <param name="callbackProgress">加载lua进度</param>
//         /// <param name="callbackEnd">加载lua脚本完毕后回调</param>
//         /// <param name="extension">资源后缀名，如果为None则默认以AssetBundle资源进行加载</param>
//         void RunWithFolder(string path, System.Action callbackEnd, System.Action<float> callbackProgress, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

//         /// <summary>
//         /// 运行目录下所有lua脚本
//         /// </summary>
//         /// <param name="path">目录路径，支持从Unity的Resources目录，或者下载到本地的asset/resources_hotupdate目录</param>
//         /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
//         /// <param name="callbackEnd">加载lua脚本完毕后回调</param>
//         /// <param name="extension">资源后缀名，如果为None则默认以AssetBundle资源进行加载</param>
//         void RunWithFolder(string path, System.Action callbackEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

//         /// <summary>
//         /// 运行目录下所有lua脚本
//         /// </summary>
//         /// <param name="path">目录路径，支持从Unity的Resources目录，或者下载到本地的asset/resources_hotupdate目录</param>
//         /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
//         void RunWithFolder(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

//         /// <summary>
//         /// 运行一个lua脚本
//         /// </summary>
//         /// <param name="path">文件路径，支持从Unity的Resources目录，或者下载到本地的asset/resources_hotupdate目录</param>
//         /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
//         void RunWithFile(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

//         /// <summary>
//         /// 直接运行lua脚本
//         /// </summary>
//         /// <param name="luaScript">lua脚本内容</param>
//         void RunWithString(string luaScript);
//     }
// }
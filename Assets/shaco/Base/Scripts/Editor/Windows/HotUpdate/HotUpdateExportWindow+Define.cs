// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;

// namespace shacoEditor
// {
//     public partial class HotUpdateExportWindow : EditorWindow
//     {
//         //允许打包为assetbundle的文件后缀名，不在该列表中的无法打包
//         static private readonly string[] ALLOW_ASSETBUNDLE_EXTENSIONS = new string[]
//         {
//             //图片格式
// 			".bmp", ".jpg", ".png", ".tif", ".gif", ".tga",
//             //文本格式
// 			".txt", ".csv", ".json", ".md", ".xml", ".lua",
//             //声音
//             ".mp3", ".ogg", "wav", ".mixer",
//             //字体
//             ".fontsettings", ".ttf", ".otf", "eot", "woff", "woff2", "dfont",
//             //Unity支持格式
//             ".bytes", ".prefab", ".unity", ".shader", ".anim", ".skin", ".mat", ".FBX", ".spriteatlas", ".asset"
//         };

//         //Unity目前已知不支持的类型，以下类型的文件会自动强制转换为二进制文件打包
//         private readonly string[] AUTO_CONVERT_TO_BYTES_EXTENSIONS = new string[] { ".pack", ".bin", ".tmp", ".xlsx", ".xls", ".lua" };

//         //支持作为原始文件加载的文件后缀名列表
//         //不在此列表中的所有文件会自动打包为AssetBundle
//         private readonly string[] SUPPORT_ORIGINAL_FILE_FORMAT_EXTENSIONS = new string[]
//         { 
//             //图片格式
// 			".bmp",".jpg",".png",".tif",".gif",".pcx",".tga",".exif",".fpx",".svg",".psd",".cdr",".pcd",".dxf",".ufo",".eps",".ai",".raw",".WMF",".webp",
//             //文本格式
// 			".txt", ".csv", ".json", ".md", ".xml", ".lua",
//             //其他格式
//             ".bytes", ".pdf", ".meta", ".mdb", ".dat", ".log", ".db", ".prefs", ".res",
//             //自定义特殊格式
//             ".pack", ".bin", ".tmp"
//         };
//     }
// }
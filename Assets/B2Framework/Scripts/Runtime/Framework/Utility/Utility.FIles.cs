using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace B2Framework
{
    public static partial class Utility
    {
        /// <summary>
        /// 文件工具
        /// 正则表达式测试地址：https://www.regexpal.com
        /// </summary>
        public static partial class Files
        {
            /// <summary>
            /// 文件夹是否可写
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static bool HasWriteAccess(string path)
            {
                if (!Directory.Exists(path)) return false;
                try
                {
                    string tmpFilePath = Path.Combine(path, System.IO.Path.GetRandomFileName());
                    using (FileStream fs = new FileStream(tmpFilePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        StreamWriter writer = new StreamWriter(fs);
                        writer.Write("1");
                    }
                    File.Delete(tmpFilePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            /// <summary>
            /// 获取所有文件路径
            /// </summary>
            /// <param name="path"></param>
            /// <param name="extents"></param>
            /// <returns></returns>
            public static string[] GetFiles(string path, params string[] extents)
            {
                if (extents.Length > 0)
                {
                    return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(f => extents.Contains(Path.GetExtension(f))).ToArray();
                }
                else
                {
                    return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                }
            }
            /// <summary>
            /// 获取传入后缀以外的所有文件
            /// </summary>
            /// <param name="path"></param>
            /// <param name="extents"></param>
            /// <returns></returns>
            public static string[] GetFilesExcept(string path, params string[] extents)
            {
                if (extents.Length > 0)
                {
                    return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(f => !extents.Contains(Path.GetExtension(f))).ToArray();
                }
                else
                {
                    return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                }
            }
            /// <summary>
            /// 删除文件夹
            /// </summary>
            /// <param name="folderPath"></param>
            /// <returns></returns>
            public static bool DeleteDir(string path)
            {
                try
                {
                    if (string.IsNullOrEmpty(path)) return true;

                    if (!Directory.Exists(path)) return true;

                    Directory.Delete(path, true);
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(Utility.Text.Format("SafeDeleteDir failed! path = {0} with err: {1}", path, ex.Message));
                    return false;
                }
            }
            /// <summary>
            /// 删除文件
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static bool DeleteFile(string path)
            {
                try
                {
                    if (string.IsNullOrEmpty(path)) return true;

                    if (!File.Exists(path)) return true;

                    File.Delete(path);
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(Utility.Text.Format("DeleteFile failed! path = {0} with err: {1}", path, ex.Message));
                    return false;
                }
            }
            /// <summary>
            /// 删除指定文件夹下所有文件
            /// </summary>
            /// <param name="path"></param>
            /// <param name="extents"></param>
            public static void DeleteFiles(string path, params string[] extents)
            {
                var files = GetFiles(path, extents);
                for (var i = 0; i < files.Length; i++) DeleteFile(files[i]);
            }
            /// <summary>
            /// 删除指定文件夹下所有文件
            /// </summary>
            /// <param name="path"></param>
            /// <param name="extents"></param>
            public static void DeleteFilesExcept(string path, params string[] extents)
            {
                var files = GetFilesExcept(path, extents);
                for (var i = 0; i < files.Length; i++) DeleteFile(files[i]);
            }
            /// <summary>
            /// 清除空文件夹
            /// </summary>
            /// <param name="path"></param>
            /// <param name="self">根目录为空时，是否删除根目录</param>
            public static void DeleteEmptyDirs(string path, bool self = false)
            {
                if (!Directory.Exists(path)) return;
                // 获取所有文件夹和文件
                var entries = Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories);
                if (entries.Length > 0)
                {
                    foreach (var p in entries)
                    {
                        // 跳过文件，只考虑文件夹
                        if (File.Exists(p)) continue;
                        if (Directory.Exists(p))
                        {
                            // 如果文件夹里没有了文件，则删除这个文件夹
                            var files = Directory.GetFiles(p, "*", SearchOption.AllDirectories);
                            if (files.Length == 0) Directory.Delete(p, true);
                        }
                    }
                    // 删除自己
                    if (self)
                    {
                        entries = Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories);
                        if (entries.Length == 0) Directory.Delete(path);
                    }
                }
                else
                {
                    // 删除自己
                    if (self) Directory.Delete(path);
                }
            }
            /// <summary>
            /// 重命名文件
            /// </summary>
            /// <param name="sourceFileName"></param>
            /// <param name="destFileName"></param>
            /// <returns></returns>
            public static bool RenameFile(string sourceFileName, string destFileName)
            {
                try
                {
                    if (string.IsNullOrEmpty(sourceFileName)) return false;

                    if (!File.Exists(sourceFileName)) return false;

                    DeleteFile(destFileName);

                    File.Move(sourceFileName, destFileName);
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(Utility.Text.Format("RenameFile failed! path = {0} with err: {1}", sourceFileName, ex.Message));
                    return false;
                }
            }
            /// <summary>
            /// 读取字节数组
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static byte[] ReadAllBytes(string path)
            {
                try
                {
                    if (string.IsNullOrEmpty(path)) return null;

                    if (!File.Exists(path)) return null;
                    return Encoding.UTF8.GetBytes(File.ReadAllText(path));
                    // return File.ReadAllBytes(path);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(Utility.Text.Format("ReadAllBytes failed! path = {0} with err = {1}", path, ex.Message));
                    return null;
                }
            }
            /// <summary>
            /// 将字节数组写入指定文件
            /// </summary>
            /// <param name="path"></param>
            /// <param name="bytes"></param>
            /// <returns></returns>
            public static bool WriteAllBytes(string path, byte[] bytes)
            {
                try
                {
                    if (string.IsNullOrEmpty(path)) return false;

                    var dir = Path.GetDirectoryName(path);
                    if (Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    if (!File.Exists(path)) return false;
                    File.WriteAllBytes(path, bytes);
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(Utility.Text.Format("WriteAllBytes failed! path = {0} with err = {1}", path, ex.Message));
                    return false;
                }
            }
            /// <summary>
            /// 计算文件的MD5
            /// </summary>
            /// <param name="path">文件路径</param>
            /// <returns></returns>
            public static string GetMD5(string path)
            {
                try
                {
                    // return Utility.Verifier.GetMD5(ReadAllBytes(path));
                    using (var fs = new FileStream(path, FileMode.Open))
                    {
                        return Utility.Verifier.GetMD5(fs);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("GetMD5 fail, error:" + ex.Message);
                }
            }
        }
    }
}

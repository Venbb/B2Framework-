using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;
using System;
using B2Framework;

namespace B2Framework
{
    /// <summary>
    /// 版本管理
    /// 主版本号 . 子版本号 [ . 编译版本号 [ . 修正版本号 ] ]
    /// 
    /// Bundle Identifier:
    /// iOS、Android公用，字符串，一般格式为com.company.game，iOS里用于开发者证书
    /// 例：PlayerSettings.bundleIdentifier = "com.senlin.xuka";
    /// 
    /// Bundle Version:
    /// iOS、Android公用，字符串，一般格式为1.2.3，用于显示给用户的版本信息。
    /// 特别注意：如果iOS要提审AppStore，那么Bundle Version必须是3位，不可以4位，比如1.2.3.0这样是不行的
    /// 例：PlayerSettings.bundleVersion = "1.2.0";
    /// 
    /// Bundle Version Code:
    /// Android 特有，数字，anroid版本的内部版本号，可用于区别当前版本是否最新apk，进而整包更新
    /// 例：PlayerSettings.Andcroid.bundleVersionCode = 120;
    /// 
    /// Build Number：
    /// iOS特有，意义同上
    /// 
    /// 版本号解析：
    /// 第一位：主版本号；
    /// 第二位：子版本号；
    /// 第三位：编译版本号 = bundleVersionCode(android)/buildNumber(ios)
    /// 第四位：修正版本号（资源版本号）
    /// 在PlayerSettings设置中至少设置两位，主版本号和子版本号；推荐三位，将编译版本号加上，这个版本号最终会显示给用户
    /// 在游戏中显示的版本号：主版本号 . 子版本号 . 编译版本号 . 修正版本号
    /// 例：App v0.1.0.0 Res v0.1.0.0
    /// 打包时需要记录编译版本号，每次出包自动+1，并且设置bundleVersionCode(android)/buildNumber(ios)更新PlayerSettings.bundleVersion的第三位
    /// 构建资源时需要记录修正版本号（资源版本号），每次打资源包自动+1
    /// 最终在游戏中显示版本信息将PlayerSettings.bundleVersion加上资源版本号显示
    /// 低位版本号增加时，高位版本号重置
    /// </summary>
    public static class Versions
    {
        public static readonly VerifyBy verifyBy = VerifyBy.Hash;
        public static Version serverVersion { get; set; }
        public static Version localVersion { get; set; }

        public static string versionFile
        {
            get
            {
                return GameUtility.Assets.GetDataPath(GameConst.RES_VER_FILE);
            }
        }
        public static void Load()
        {
            localVersion = LoadVersion();
        }
        public static void Clear()
        {
            localVersion = serverVersion = null;
            // var path = versionFile;
            // if (File.Exists(path)) File.Delete(path);
            GameUtility.Assets.ClearPersistentData();
        }
        public static void Save()
        {
            using (var stream = File.OpenWrite(versionFile))
            {
                var writer = new BinaryWriter(stream);
                serverVersion.Serialize(writer);
            }
            localVersion = serverVersion = null;
        }
        public static Version LoadVersion()
        {
            using (var stream = File.OpenRead(versionFile))
            {
                var reader = new BinaryReader(stream);
                var ver = new Version();
                ver.Deserialize(reader);
                return ver;
            }
        }
        public static bool IsNew(string path, long len, string hash)
        {
            if (!File.Exists(path)) return true;

            if (localVersion != null)
            {
                var key = Path.GetFileName(path);
                var file = localVersion.GetFile(key);
                if (file != null && file.len == len && file.hash.Equals(hash, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            using (var stream = File.OpenRead(path))
            {
                if (stream.Length != len) return true;
                if (verifyBy != VerifyBy.Hash) return false;
                return !Utility.Verifier.GetCRC32(stream).Equals(hash, StringComparison.OrdinalIgnoreCase);
            }
        }
        public static List<VFile> GetNewFiles()
        {
            var list = new List<VFile>();
            foreach (var file in serverVersion.files)
            {
                var path = GameUtility.Assets.GetDataPath(file.name);
                if (IsNew(path, file.len, file.hash))
                {
                    list.Add(file);
                }
            }
            return list;
        }
    }
    public class Version
    {
        public int ver;
        public List<VFile> files = new List<VFile>();
        private Dictionary<string, VFile> _dataFiles = new Dictionary<string, VFile>();
        public string version
        {
            get
            {
                return Utility.Text.Format("{0}.{1}", Application.version, ver);
            }
        }
        public VFile GetFile(string path)
        {
            VFile file;
            _dataFiles.TryGetValue(path, out file);
            return file;
        }
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ver);

            writer.Write(files.Count);
            foreach (var file in files)
                file.Serialize(writer);
        }

        public void Deserialize(BinaryReader reader)
        {
            ver = reader.ReadInt32();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var file = new VFile();
                file.Deserialize(reader);
                files.Add(file);
                _dataFiles[file.name] = file;
            }
        }
    }
    public class VFile
    {
        public string hash { get; set; }
        public long len { get; set; }
        public string name { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(name);
            writer.Write(len);
            writer.Write(hash);
        }

        public void Deserialize(BinaryReader reader)
        {
            name = reader.ReadString();
            len = reader.ReadInt64();
            hash = reader.ReadString();
        }
    }
}
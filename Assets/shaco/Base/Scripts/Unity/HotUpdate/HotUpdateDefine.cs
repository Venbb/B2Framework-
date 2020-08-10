using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class HotUpdateDefine
    {
        public enum BaseVersionType
        {
            VER_1_0, //全json结构读取，效率最低
            VER_2_0, //文件列表使用StreamReader读取，依赖表使用json结构
            VER_2_1, //文件列表和依赖表均使用StreamReader读取
        }

        //导出assetbundle配置信息
        [SerializeField]
        public class ExportAssetBundle
        {
            public class ExportFile
            {
                //文件路径
                public string Key;
            }
            public shaco.HotUpdateDefine.ExportFileFormat exportFormat = shaco.HotUpdateDefine.ExportFileFormat.AssetBundle;
            public string AssetBundleName = string.Empty;
            public string AssetBundleMD5 = string.Empty;
            public string fullPathRuntime = string.Empty;
            public long fileSize = 0;
            public List<ExportAssetBundle.ExportFile> ListFiles = null;

            public ExportAssetBundle() { }
            public void CopyFrom(ExportAssetBundle other)
            {
                this.AssetBundleName = other.AssetBundleName;
                this.AssetBundleMD5 = other.AssetBundleMD5;
                this.fileSize = other.fileSize;
                this.ListFiles.Clear();
                this.ListFiles.AddRange(other.ListFiles);
                this.exportFormat = other.exportFormat;
            }
        }

        //下载的assetbundle信息
        [SerializeField]
        public class DownloadAssetBundle : shaco.Base.IObjectPoolData
        {
            public string url = string.Empty;
            public ExportAssetBundle ExportInfo = null;
            public shaco.Base.IHttpHelper HttpDel = null;
            public HotUpdateDownloader HotUpdateDel = null;

            public DownloadAssetBundle(ExportAssetBundle exportInfo)
            {
                this.ExportInfo = exportInfo;
            }
            public DownloadAssetBundle() { }

            public void Dispose()
            {
                url = string.Empty;
                ExportInfo = null;
                HttpDel = null;
                HotUpdateDel = null;
            }
        }

        //序列化的版本控制描述文件
        [SerializeField]
        public class SerializeVersionControl
        {
            //文件是否为自动加密状态
            public bool AutoEncryt = true;
            //是否自动输出资源更新日志
            public bool AutoOutputBuildLog = true;
            //是否自动压缩文件(一般来说Assetbundle自带有压缩了，所以该功能只会去压缩原始文件以减少下载资源大小)
            public bool AutoCompressdFile = true;
            //文件标记符(预留位置)
            public string FileTag = string.Empty;
            //Unity版本号
            public string UnityVersion = string.Empty;
            //版本控制文件版本
            public BaseVersionType baseVersion = BaseVersionType.VER_2_1;
            //资源版本号
            public string Version = string.Empty;
            //API字段，用于处理一些特殊逻辑
            public ExportFileAPI VersionControlAPI = ExportFileAPI.DeleteUnUseFiles;
            //所有Assetbundle和原始文件数据大小
            public long TotalDataSize = 0;
            //所有ab包数量
            public int TotalSize = 0;
            //版本中所有assetbundle
            public List<ExportAssetBundle> ListAssetBundles = new List<ExportAssetBundle>();
            public Dictionary<string, ExportAssetBundle> DicAssetBundles = new Dictionary<string, ExportAssetBundle>();

            public SerializeVersionControl() { }
        }

        // public delegate void CALL_FUNC_READ_STRING(string value);
        // public delegate void CALL_FUNC_READ_BYTE(byte[] value);
        // public delegate void CALL_FUNC_READ_PROGRESS(float percent);

        public enum ResourceCreateMode
        {
            Memory,
            MemoryAsync
        }

        public enum Platform
        {
            None = 0,
            Android = 1,
            iOS = 2,
            WebGL = 3,
        }

        public enum ExportFileAPI
        {
            None = 0,
            DeleteFile = 1,
            DeleteUnUseFiles = 2
        }

        //导出资源模式
        public enum ExportFileFormat
        {
            AssetBundle = 1,
            OriginalFile = 2,
        }

        //资源配置文件数量，用于计算下载百分比
        public const int ALL_VERSION_CONTROL_FILE_COUNT = 4;
        //计算下载百分比的时候，下载资源配置文件所占的百分比(范围0~1)
        public const float CHECK_VERSION_PROGRESS_RATIO = 0.2f;

        //当文件不存在时候，动态下载文件的所占百分比
        public const float DYNAMIC_DOWNLOAD_PROGRESS_PERCENT = 0.5f;

#if DEBUG_LOG && UNITY_EDITOR
        //最大保存的已读取资源数量上限
        public const int MAX_SAVE_READED_ASSET_COUNT = 10;
#else
        public const int MAX_SAVE_READED_ASSET_COUNT = 0;
#endif

        //file tag
        public const string INTERNAL_RESOURCE_PATH_TAG = "[pack]";
        public const string PATH_RELATIVE_FLAG = "##";
        public const string EXTENSION_DOT_FLAG = "##2";
        // public const string FILENAME_TAG_UNITY_VERSION = SIGN_FLAG;
        // public const string PATH_TAG_UNITY_VERSION = SIGN_FLAG;
        public const string FILE_DELETE_FLAG = "(Delete)";
        public const string FILE_UPDATE_FLAG = "(Update)";
        public const string EXTENSION_ASSETBUNDLE = ".assetbundle";
        public const string EXTENSION_VERSION_CONTROL = ".json";
        public const string EXTENSION_FILE_LIST = ".fdb";
        public const string EXTENSION_FILE_LIST_RUNTIME = ".fdbruntime";
        public const string EXTENSION_DEPEND = ".depencies";
        public const string DEPEND_TAG_2_1 = "ver:2.1"; //VER_2_1以上版本的版本标记
        public const short DEPEND_TAG_MAX_LENGTH = 8; //VER_2_1以上版本的版本标记最大检查长度，可以根据DEPEND_TAG_XXX最大长度修改它
        public const string EXTENSION_ASSETBUNDLE_MANIFEST = ".manifest";
        public const string EXTENSION_META = ".meta";
        public const string EXTENSION_MAIN_MD5 = ".txt";
        public const string VERSION_CONTROL = "VersionControl";
        public const string VERSION_CONTROL_MAIN_MD5 = VERSION_CONTROL + "_MainMD5";
        public const string PATH_TAG = VERSION_CONTROL + shaco.Base.GlobalParams.SIGN_FLAG;
        public const string BUILD_RESOURCES_LOG_FILE_NAME = "BuildResources.log";

        public const string ORIGINAL_FILE_TAG = "orignal";
        public const string COMPRESSED_FILE_TAG = "compressed_file";

        //本地version文件路径，主要用于在游戏内显示version
        public const string VERSION_PATH = "BuildVersion.txt";
        public const string RESOURCES_VERSION_PATH = "ResourcesVersion";
    }
}
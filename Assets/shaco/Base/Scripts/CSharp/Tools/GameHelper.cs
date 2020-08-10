using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 游戏类管理中心，负责c#使用类的管理
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public abstract class GameHelper
    {
        static public string GameHelperConfigPath
        {
            get { return shaco.Base.GlobalParams.GetShacoFrameworkRootPath() + "/Base/Resources/" + GameHelperConfigFileName; }
        }

        static public string GameHelperConfigFileName
        {
            get { return "GameHelperConfig.bytes"; }
        }

        static public shaco.Base.IDataSave gameConfig
        {
            get
            {
                if (null == _gameConfig)
                { 
                    var configBytes = LoadConfigFromResources();
                    if (null != configBytes)
                    {
                        _gameConfig = new shaco.Base.DataSave();
                        _gameConfig.savePath = GameHelperConfigPath;
                        _gameConfig.ReloadFromBytes(configBytes, false);
                    }
                }
                return _gameConfig;
            }
        }
        static private shaco.Base.IDataSave _gameConfig = null;

        /// <summary>
        /// 打印日志类
        /// </summary>
        static public shaco.Base.ILog log
        {
            get { return GetOrSetDefaultInterface<shaco.Base.ILog, shaco.Base.Log>(); }
            set { BindInterface<shaco.Base.ILog>(value); }
        }

        /// <summary>
        /// c#事件，便于添加、删除、派发事件，具有穿透性和易管理性
        /// </summary>
        static public shaco.Base.IEventManager Event
        {
            get { return GetOrSetDefaultInterface<shaco.Base.IEventManager, shaco.Base.EventManager>(); }
            set { BindInterface<shaco.Base.IEventManager>(value); }
        }

        /// <summary>
        /// 便捷的数据储存管理，支持自动加密和解密功能
        /// UnityEngine.PlayerPrefs更好的替代方案
        /// </summary>
        static public shaco.Base.IDataSave datasave
        {
            get { return GetOrSetDefaultInterface<shaco.Base.IDataSave, shaco.Base.DataSave>(); }
            set { BindInterface<shaco.Base.IDataSave>(value); }
        }

        /// <summary>
        /// c#内存池
        /// </summary>
        static public shaco.Base.IObjectPool objectpool
        {
            get { return GetOrSetDefaultInterface<shaco.Base.IObjectPool, shaco.Base.ObjectPool>(); }
            set { BindInterface<shaco.Base.IObjectPool>(value); }
        }

        /// <summary>
        /// c#内存池对象生成器
        /// </summary>
        static public shaco.Base.IObjectSpawn objectpoolSpawn
        {
            get { return GetOrSetDefaultInterface<shaco.Base.IObjectSpawn, shaco.Base.ObjectSpwan>(); }
            set { BindInterface<shaco.Base.IObjectSpawn>(value); }
        }

        /// <summary>
        /// http网络类单例，用于上传和下载http请求数据
        /// </summary>
        static public shaco.Base.IHttpHelper http
        {
            get { return GetOrSetDefaultInterface<shaco.Base.IHttpHelper, shaco.Base.HttpHelper>(); }
            set { BindInterface<shaco.Base.IHttpHelper>(value); }
        }

        /// <summary>
        /// 新手引导通用类
        /// </summary>
        static public shaco.Base.IGuideManager newguide
        {
            get { return GetOrSetDefaultInterface<shaco.Base.IGuideManager, shaco.Base.GuideManager>(); }
            set { BindInterface<shaco.Base.IGuideManager>(value); }
        }

        /// <summary>
        /// 引导数据配置帮助类
        /// </summary>
        static public shaco.Base.IGuideSettingHelper guideSettingHelper
        {
            get { return GetOrSetDefaultInterface<shaco.Base.IGuideSettingHelper, shaco.Base.GuideSettingHelper>(); }
            set { BindInterface<shaco.Base.IGuideSettingHelper>(value); }
        }

        /// <summary>
        /// 数据配置帮助类
        /// </summary>
        static public shaco.Base.ISettingHelper settinghelper
        {
            get { return GetOrSetDefaultInterface<shaco.Base.ISettingHelper, shaco.Base.SettingsHelper>(); }
            set { BindInterface<shaco.Base.ISettingHelper>(value); }
        }

        /// <summary>
        /// zip格式压缩解压
        /// </summary>
        static public shaco.Base.IZipHelper zip
        {
            get { return GetOrSetDefaultInterface<shaco.Base.IZipHelper, shaco.Base.ZipHelper>(); }
            set { BindInterface<shaco.Base.IZipHelper>(value); }
        }

        /// <summary>
        /// 模拟平台事件类
        /// </summary>
        static public shaco.Base.ISimulatePlatformEvent simulatePlatformEvent
        {
            get { return GetOrSetDefaultInterface<shaco.Base.ISimulatePlatformEvent, shaco.SimulatePlatformEventDefault>(); }
            set { BindInterface<shaco.Base.ISimulatePlatformEvent>(value); }
        }

        /// <summary>
        /// 数据类型与字符串相关转换类
        /// </summary>
        static public shaco.Base.IDataTypesTemplate dateTypesTemplate
        {
            get { return GetOrSetDefaultInterface<shaco.Base.IDataTypesTemplate, shaco.Base.DataTypesTemplate>(); }
            set { BindInterface<shaco.Base.IDataTypesTemplate>(value); }
        }

        /// <summary>
        /// 本地化语言类
        /// </summary>
        static public shaco.Base.ILocalization localization
        {
            get { return GetOrSetDefaultInterface<shaco.Base.ILocalization, shaco.Base.Localization>(); }
            set { BindInterface<shaco.Base.ILocalization>(value); }
        }

        /// <summary>
        /// excel附加参数配置类
        /// </summary>
        static public shaco.Base.IExcelSetting excelSetting
        {
            get { return GetOrSetDefaultInterface<shaco.Base.IExcelSetting, shaco.Base.ExcelDataSetting>(); }
            set { BindInterface<shaco.Base.IExcelSetting>(value); }
        }

        /// <summary>
        /// 性能分析类
        /// </summary>
        static public shaco.Base.IProfiler profiler
        {
            get { return GetOrSetDefaultInterface<shaco.Base.IProfiler, shaco.Base.Profiler>(); }
            set { BindInterface<shaco.Base.IProfiler>(value); }
        }

        /// <summary>
        /// 观察者模式管理类
        /// </summary>
        static public shaco.Base.IObserverManager observer
        {
            get { return GetOrSetDefaultInterface<shaco.Base.IObserverManager, shaco.Base.ObserverManager>(); }
            set { BindInterface<shaco.Base.IObserverManager>(value); }
        }

        /// <summary>
        /// 框架初始化方法
        /// <param name="persistentDataPath">当前平台可读可写路径</param>
        /// <return>初始化是否成功</return>
        /// </summary>
        static public bool InitFramework(string persistentDataPath)
        {
            if (string.IsNullOrEmpty(persistentDataPath))
            {
                throw new System.ArgumentException("GameHelper InitFramework exception: persistentDataPath is empty");
            }

            //配置本地可读写目录
            shaco.Base.FileDefine.persistentDataPath = persistentDataPath;
            return true;
        }

        /// <summary>
        /// 绑定接口实现类
        /// </summary>
        static protected void BindInterface<T>(object to)
        {
            if (null == to)
            {
                Log.Error("GameHelper BindInterface error: valid param, from=" + typeof(T) + " to=" + to);
                return;
            }
            shaco.Base.GameEntry.SetInstance(typeof(T), to.GetType());
        }

        /// <summary>
        /// 获取接口类
        /// <return>接口类</return>
        /// </summary>
        static protected T GetOrSetDefaultInterface<T, DEFAULT>() where DEFAULT : class, IGameInstance where T : IGameInstance
        {
            var retValue = default(T);
            retValue = shaco.Base.GameEntry.GetInstance<T>();
            if (null == retValue)
            {
                //从配置中读取自定义的接口类型
                if (null != gameConfig)
                {
                    var readTypeName = gameConfig.ReadString(typeof(T).ToTypeString());
                    if (!string.IsNullOrEmpty(readTypeName))
                    {
                        var instantiateTypeTmp = shaco.Base.Utility.Assembly.GetTypeWithinLoadedAssemblies(readTypeName);
                        if (null == instantiateTypeTmp)
                        {
                            // throw new System.ArgumentException("invalid type=" + readTypeName);
                            instantiateTypeTmp = typeof(DEFAULT);
                        }
                        shaco.Base.GameEntry.SetInstance(typeof(T), instantiateTypeTmp);
                        retValue = shaco.Base.GameEntry.GetInstance<T>();
                    }
                }

                //获取接口配置对应数据失败，使用默认接口类型
                if (null == retValue)
                {
                    shaco.Base.GameEntry.SetInstance<T, DEFAULT>();
                    retValue = shaco.Base.GameEntry.GetInstance<T>();
                }
            }

            return retValue;
        }

        /// <summary>
        /// 读取配置文件二进制数据
        /// <return>配置文件二进制数据</return>
        /// </summary>
        static private byte[] LoadConfigFromResources()
        {
            byte[] retValue = null;

#if !(UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_PS4 || UNITY_XBOXONE || UNITY_WSA || UNITY_WEBGL)
            //c#的加载配置文件需要在这里添加读取框架配置的代码
            Log.Exception("GameHelper LoadConfigFromResources exception: not support in current platfrom, please set custom load function");
            return null;
#else

            shaco.Base.GameHelper.InitFramework(UnityEngine.Application.persistentDataPath);
#if UNITY_EDITOR
            //编辑器直接读取本地文件可以防止AssetDatabase没有及时刷新导致配置读取失败的bug
            retValue = System.IO.File.ReadAllBytes(GameHelperConfigPath);
#else
            var assetLoaded = UnityEngine.Resources.Load<UnityEngine.TextAsset>(shaco.Base.FileHelper.RemoveAllExtentsion(GameHelperConfigFileName));
            retValue = null == assetLoaded ? null : assetLoaded.bytes;
#endif
            //禁止使用shaco框架中的内容进行加载，否则很容易导致死循环！！！
            // retValue = shaco.Base.FileHelper.ReadAllByteByUserPath(GameHelperConfigPath);
#endif

            if (null == retValue)
                throw new System.Exception("GameHelper LoadConfigFromResources error: path=" + GameHelperConfigFileName);
            return retValue;
        }
    }
}
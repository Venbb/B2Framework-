using System.Collections;
using System.Collections.Generic;

namespace shacoEditor
{
    /// <summary>
    /// 游戏类管理中心，负责c#使用类的管理
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public abstract class GameHelper : shaco.Base.GameHelper
    {
        /// <summary>
        /// 读取spine二进制文件接口
        /// </summary>
        static public shacoEditor.ISpineBinaryReader spineBinaryReader
        {
            get { return GetOrSetDefaultInterfaceEditor<shacoEditor.ISpineBinaryReader, shacoEditor.SpineBinaryReader>(); }
            set { BindInterface<shacoEditor.ISpineBinaryReader>(value); }
        }

        /// <summary>
        /// 获取接口类
        /// <return>接口类</return>
        /// </summary>
        static protected T GetOrSetDefaultInterfaceEditor<T, DEFAULT>() where DEFAULT : class, shaco.Base.IGameInstance where T : shaco.Base.IGameInstance
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
    }
}
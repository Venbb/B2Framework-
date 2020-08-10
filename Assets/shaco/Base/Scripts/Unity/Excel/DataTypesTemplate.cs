using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class DataTypesTemplate : ScriptableObject, shaco.Base.IDataTypesTemplate, shaco.Base.IGameInstanceCreator
    {
        /// <summary>
        /// 替换new
        /// </summary>
        static public object Create()
        {
            return ScriptableObject.CreateInstance<DataTypesTemplate>();
        }        

        /// <summary> 
        /// 字符串类型对应的c#类型表
        /// </summary>
        public List<shaco.Base.StringTypePair> stringToTypePairs
        {
            get
            {
                CheckInit();
                return _stringToTypePairs;
            }
        }
        [UnityEngine.SerializeField]
        private List<shaco.Base.StringTypePair> _stringToTypePairs = null;

        /// <summary>
        /// 获取unity默认的类型模板
        /// </summary>
        public shaco.Base.StringTypePair[] GetTypesTemplate()
        {
            CheckInit();
            return _stringToTypePairs.IsNullOrEmpty() ? null : _stringToTypePairs.ToArray();
        }

        /// <summary>
        /// 根据配置表将类型转换为字符串，如果转换失败则使用c#默认的类型标签
        /// <param name="type">数据类型</param>
        /// <return>类型字符串</return>
        /// </summary>
        public string TypeToCustomString(System.Type type)
        {
            return TypeToCustomString(type.FullName);
        }

        /// <summary>
        /// 根据配置表将类型转换为字符串，如果转换失败则使用c#默认的类型标签
        /// <param name="typeString">数据类型字符串</param>
        /// <return>类型字符串</return>
        /// </summary>
        public string TypeToCustomString(string typeString)
        {
            var pariTmp = GetTypesTemplate();
            shaco.Base.StringTypePair findValue = null;
            if (!pariTmp.IsNullOrEmpty())
            {
                for (int i = pariTmp.Length - 1; i >= 0; --i)
                {
                    if (typeString == pariTmp[i].fullTypeName)
                    {
                        findValue = pariTmp[i];
                        break;
                    }
                }
            }
            return null != findValue ? findValue.customTypeString : typeString;
        }

        /// <summary>
        /// 检查配置是否初始化
        /// </summary>
        private void CheckInit()
        {
            if (_stringToTypePairs.IsNullOrEmpty())
            {
                //读取动态配置
                var typeName = this.GetType().Name;
                if (shaco.GameHelper.res.ExistsResourcesOrLocal(typeName))
                {
                    var loadTemplateScript = shaco.GameHelper.res.LoadResourcesOrLocal<DataTypesTemplate>(typeName);
                    if (null != loadTemplateScript)
                    {
                        this._stringToTypePairs = loadTemplateScript._stringToTypePairs;
                        if (null != _stringToTypePairs)
                        {
                            foreach (var iter in this._stringToTypePairs)
                                iter.customTypeString = iter.customTypeString.ToLower();
                        }
                    }
                    else
                    {
                        Log.Error("DataTypesTemplate CheckInit erorr: can't load 'DataTypesTemplate.asset' ");
                    }
                }
                else
                {
                    Log.Error("DataTypesTemplate CheckInit erorr: not found 'DataTypesTemplate.asset' in Resources Folder");
                }

                //读取动态资源失败，使用默认配置
                if (this._stringToTypePairs.IsNullOrEmpty())
                {
                    this._stringToTypePairs = shaco.Base.StringTypePair.DEFAULT_TYPES_PAIRS.ToList();
                    foreach (var iter in this._stringToTypePairs)
                        iter.customTypeString = iter.customTypeString.ToLower();
                }
            }
        }
    }
}
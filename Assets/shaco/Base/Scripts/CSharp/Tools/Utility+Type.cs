using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace shaco.Base
{
    public static partial class Utility
    {
        /// <summary>
        /// 序列化对象信息组
        /// </summary>
        private struct SerializableInformationPair
        {
            public bool isProperty;
            public string name;
            public object value;
            public System.Type type;
        }

        static public T Instantiate<T>()
        {
            return (T)(typeof(T).Instantiate());
        }

        /// <summary>
        /// 实例化一个对象
        /// <param name="typeName">对象类型全称</param>
        /// <return>实例化后的对象</return>
        /// </summary>
        static public object Instantiate(string typeName)
        {
            object retValue = null;
            var assemblys = shaco.Base.Utility.Assembly.GetLoadedAssembly();

            for (int i = 0; i < assemblys.Length; ++i)
            {
                var assemblyTmp = assemblys[i];
                retValue = assemblyTmp.CreateInstance(typeName);
                if (null != retValue)
                {
                    break;
                }
            }

            if (null == retValue)
            {
                Log.Error("Utility+Type Instantiate error: can't instantiate from type name=" + typeName);
            }
            return retValue;
        }

        static public object InstantiateGeneric(System.Type baseType, params System.Type[] templateTypes)
        {
            if (null == baseType)
            {
                Log.Error("Utility+Type InstantiateGeneric error: invalide type");
                return null;
            }

            if (!baseType.IsGenericType)
            {
                Log.Error("Utility+Type InstantiateGeneric error: not a genric type=" + baseType.ToTypeString());
                return null;
            }
            return baseType.MakeGenericType(templateTypes).Instantiate();
        }


        static public string GetGenericBaseTypeString(System.Type genericType)
        {
            return (null == genericType || !genericType.IsGenericType) ? string.Empty : genericType.ToTypeString().RemoveBehind("`1[[") + "`1";
        }

        /// <summary>
        /// 获取类型全称
        /// <return>类型全称</return>
        /// </summary>
        static public string ToTypeString<T>()
        {
            return typeof(T).FullName;
        }

        /// <summary>
        /// 获取枚举所有类型
        /// </summary>
        /// <return>枚举所有类型</return>
        static public T[] ToEnums<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        /// <summary>
        /// 获取枚举
        /// <param name="value">枚举字符串类型全称</param>
        /// <return>枚举</return>
        /// </summary>
        static public T ToEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// 获取变量名字，在.Net6.0以上有nameof(T)可以作为替代
        /// <param name="expr">访问回调，例如获取class.A的变量名，则传入()=> this.A</param>
        /// <return>变量名字</return>
        /// </summary>
        static public string ToVariableName<T>(System.Linq.Expressions.Expression<Func<T>> expr)
        {
            return ((System.Linq.Expressions.MemberExpression)expr.Body).Member.Name;
        }

        /// <summary>
        /// 通过反射筛选具有指定属性的类型
        /// <param name="flags">属性类型标签</param>
        /// <param name="ignoreTypes">筛选过滤类型，不会被计算在返回值内</param>
        /// <return>使用了该属性的所有类型</return>
        /// </summary>
        static public System.Type[] GetAttributes<T>(System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, params System.Type[] ignoreTypes)
        {
            return GetAttributes(typeof(T), flags, ignoreTypes);
        }

        /// <summary>
        /// 通过反射筛选具有指定属性的类型
        /// <param name="type">查找类型</param>
        /// <param name="flags">属性类型标签</param>
        /// <param name="ignoreTypes">筛选过滤类型，不会被计算在返回值内</param>
        /// <return>使用了该属性的所有类型</return>
        /// </summary>
        static public System.Type[] GetAttributes(System.Type type, System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, params System.Type[] ignoreTypes)
        {
            //加载程序集信息
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            var types = asm.GetTypes();

            //验证指定自定义属性(使用的是4.0的新语法，匿名方法实现的)
            Func<System.Attribute[], bool> IsAttribute = o =>
             {
                 foreach (System.Attribute a in o)
                 {
                     if (a.GetType() == type)
                         return true;
                 }
                 return false;
             };

            var retValue = types.Where(o =>
            {
                return IsAttribute(System.Attribute.GetCustomAttributes(o, true));
            }).ToList();

            //过滤忽略的类型
            if (null != ignoreTypes && ignoreTypes.Length > 0)
            {
                for (int i = ignoreTypes.Length - 1; i >= 0; --i)
                {
                    var typeTmp = ignoreTypes[i];
                    if (retValue.Contains(typeTmp))
                    {
                        retValue.Remove(typeTmp);
                    }
                }
            }

            //过滤忽略的属性
            for (int i = retValue.Count - 1; i >= 0; --i)
            {
                var constructorsTmp = retValue[i].GetConstructors(flags);
                if (null == constructorsTmp || constructorsTmp.Length == 0)
                {
                    retValue.RemoveAt(i);
                }
            }
            return retValue.ToArray();
        }

        /// <summary>
        /// 通过反射筛选具有指定属性的类型
        /// <param name="ignoreTypes">筛选过滤类型，不会被计算在返回值内</param>
        /// <return>使用了该属性的所有类型</return>
        /// </summary>
        static public System.Type[] GetAttributes<T>(params System.Type[] ignoreTypes)
        {
            return GetAttributes<T>(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, ignoreTypes);
        }

        /// <summary>
        /// 通过反射筛选该类型及继承自该类型的所有类
        /// <param name="isAlsoGetInheritedType">是否同时获取继承自该类的类型</param>
        /// <param name="ignoreTypes">筛选过滤类型，不会被计算在返回值内</param>
        /// <return>该类型及继承自该类型的所有类</return>
        /// </summary>
        static public System.Type[] GetTypes<T>(bool isAlsoGetInheritedType, params System.Type[] ignoreTypes)
        {
            var retValue = new List<System.Type>();
            var findType = typeof(T);
            bool needCheckIgnoreType = !ignoreTypes.IsNullOrEmpty();
            var assemblys = shaco.Base.Utility.Assembly.GetLoadedAssembly();

            foreach (var assembly in assemblys)
            {
                var types = assembly.GetTypes();
                if (types.IsNullOrEmpty())
                {
                    Log.Warning("Utility+Type GetTypes error: can't load assembly name=" + assembly.GetName());
                    continue;
                }

                for (int j = types.Length - 1; j >= 0; --j)
                {
                    var typeTmp = types[j];
                    if (typeTmp == findType || (isAlsoGetInheritedType && typeTmp.IsInherited(findType)))
                    {
                        if (needCheckIgnoreType)
                        {
                            if (!ignoreTypes.Contains(typeTmp))
                                retValue.Add(typeTmp);
                        }
                        else
                            retValue.Add(typeTmp);
                    }
                }
            }

            return retValue.ToArray();
        }

        /// <summary>
        /// 获取程序集中指定的类型，包括继承子类
        /// <param name="T">基类类型</param>
        /// <param name="assemblyNames">程序集</param>
        /// <param name="ignoreTypes">忽略类型</param>
        /// <return>程序集中查找到的类型</return>
        /// </summary>
        static public System.Type[] GetClasses<T>(params System.Type[] ignoreTypes)
        {
            var typeBase = typeof(T);
            var retValue = new System.Collections.Generic.List<System.Type>();
            var ignoreTypesList = ignoreTypes.IsNullOrEmpty() ? new System.Type[0] : ignoreTypes;

            var assemblys = shaco.Base.Utility.Assembly.GetLoadedAssembly();
            foreach (var assembly in assemblys)
            {
                if (assembly == null)
                {
                    continue;
                }

                System.Type[] types = assembly.GetTypes();
                foreach (System.Type type in types)
                {
                    if (type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom(type))
                    {
                        if (!ignoreTypesList.Contains(type))
                            retValue.Add(type);
                    }
                }
            }
            return retValue.ToArray();
        }

        /// <summary>
        /// 获取程序集中指定的类型全称，包括继承子类
        /// <param name="T">基类类型</param>
        /// <return>程序集中查找到的类型全称</return>
        /// </summary>
        static public string[] GetClassNames<T>(params System.Type[] ignoreTypes)
        {
            var classesTmp = GetClasses<T>(ignoreTypes);
            var retValue = new string[classesTmp.Length];
            for (int i = classesTmp.Length - 1; i >= 0; --i)
            {
                retValue[i] = classesTmp[i].FullName;
            }
            return retValue;
        }

        /// <summary>
        /// 加强版搜集引用方法，会自动遍历所有public属性
        /// <param name="roots">搜索引用对象</param>
        /// <return>引用对象列表</return>
        /// </summary>
        static public T[] CollectDependenciesEx<T>(params T[] roots)
        {
            var retValue = new Dictionary<T, T>();
            var hasCheckedTargets = new Dictionary<T, T>();
            for (int i = 0; i < roots.Length; ++i)
            {
                var depencies = CollectDependenciesExBase(ref hasCheckedTargets, roots[i]);
                retValue.AddRange(depencies);
            }
            return retValue.Values.ToArray();
        }

        /// <summary>
        /// 复制一个对象所有参数
        /// <param name="copyFrom">拷贝源对象</param>
        /// <param name="copyTo">拷贝到对象</param>
        /// <param name="flags">查找对象的反射类型</param>
        /// </summary>
        static public void CopyPropertiesAndFields(object copyFrom, object copyTo, System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
        {
            if (null == copyFrom)
            {
                Log.Error("Utility+Type CopyPropertiesAndFields copyFrom is null");
                return;
            }

            if (null == copyTo)
            {
                Log.Error("Utility+Type CopyPropertiesAndFields copyTo is null");
                return;
            }

            var typeFrom = copyFrom.GetType();
            var typeTo = copyTo.GetType();
            var properties = typeFrom.GetProperties(flags);
            var fields = typeFrom.GetFields(flags);

            // if (typeFrom != typeTo)
            // {
            //     Log.Error("Utility+Type CopyPropertiesAndFields error: type not equal, from=" + typeFrom + " to=" + typeTo);
            //     return;
            // }

            //拷贝属性
            if (!properties.IsNullOrEmpty())
            {
                for (int i = 0; i < properties.Length; ++i)
                {
                    var propertyTmp = properties[i];

                    //只考虑可以读写的属性
                    if (null != propertyTmp && propertyTmp.CanWrite && propertyTmp.CanRead)
                    {
                        var valueFrom = propertyTmp.GetValue(copyFrom, null);
                        var findProperty = typeTo.GetProperty(propertyTmp.Name, flags);
                        if (null != findProperty && findProperty.PropertyType == propertyTmp.PropertyType)
                            findProperty.SetValue(copyTo, valueFrom, null);
                    }
                }
            }

            //拷贝参数
            if (!fields.IsNullOrEmpty())
            {
                for (int i = 0; i < fields.Length; ++i)
                {
                    var fieldTmp = fields[i];

                    //只考虑可以读写的属性
                    if (null != fieldTmp)
                    {
                        var valueFrom = fieldTmp.GetValue(copyFrom);
                        var findFiled = typeTo.GetField(fieldTmp.Name, flags);
                        if (null != findFiled && findFiled.FieldType == fieldTmp.FieldType)
                            findFiled.SetValue(copyTo, valueFrom);
                    }
                }
            }
        }

        /// <summary>
        /// 获取对象所有属性打印信息，struct的非public对象是无法获取信息的
        /// <param name="obj">任意c#对象</param>
        /// <param name="maxDeepCount">序列化递归最大深度，该值为了避免出现递归死循环</param>
        /// <param name="flags">查找对象的反射类型</param>
        /// <return>格式化后的打印信息</return>
        /// </summary>
        static public string GetSerializableInformation(object obj, int maxDeepCount = 4, System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
        {
            return GetSerializableInformation(string.Empty, obj, maxDeepCount, flags);
        }

        /// <summary>
        /// 获取对象所有属性打印信息，struct的非public对象是无法获取信息的
        /// <param name="title">序列化标题名字，为空的时候会默认使用对象类型作为title</param>
        /// <param name="obj">任意c#对象</param>
        /// <param name="maxDeepCount">序列化递归最大深度，该值为了避免出现递归死循环</param>
        /// <param name="flags">查找对象的反射类型</param>
        /// <return>格式化后的打印信息</return>
        /// </summary>
        static public string GetSerializableInformation(string title, object obj, int maxDeepCount = 4, System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
        {
            if (null == obj)
                return string.Empty;

            var retValue = new System.Text.StringBuilder();
            var titleCheck = string.IsNullOrEmpty(title) ? shaco.Base.GameHelper.dateTypesTemplate.TypeToCustomString(obj.GetType()) : title;
            retValue.Append(string.Format("\"[{0}]\":\n", titleCheck));
            retValue.Append("{\n");
            retValue.Append(GetSerializableInformation(obj, 1, maxDeepCount, flags));
            retValue.Append("}\n");

            return retValue.ToString();
        }

        /// <summary>
        /// 获取有效的类名，将不支持的符号自动替换为_
        /// <param name=""> </param>
        /// <return></return>
        /// </summary>
        static public string ConvertToValidClassName(string className)
        {
            if (!string.IsNullOrEmpty(className))
            {
                for (int i = 0; i < className.Length; ++i)
                {
                    var charTmp = className[i];

                    //判断是否为ASCII编码内
                    if (charTmp > 0 && charTmp < 127)
                    {
                        //如果不是数字和英文则统一替换为下划线
                        bool isNumber = charTmp >= '0' && charTmp <= '9';
                        bool isEnglishLower = charTmp >= 'a' && charTmp <= 'z';
                        bool isEnglishUpper = charTmp >= 'A' && charTmp <= 'Z';
                        if (!isNumber && !isEnglishLower && !isEnglishUpper)
                        {
                            className = className.Replace(charTmp, '_');
                        }
                    }
                }
            }
            return className;
        }

        /// <summary>
        /// 获取对象所有属性打印信息，struct的非public对象是无法获取信息的
        /// <param name="obj">任意c#对象</param>
        /// <param name="deepIndex">当前递归深度</param>
        /// <param name="maxDeepCount">序列化递归最大深度，该值为了避免出现递归死循环</param>
        /// <param name="flags">查找对象的反射类型</param>
        /// <return>格式化后的打印信息</return>
        /// </summary>
        static private string GetSerializableInformation(object obj, int deepIndex, int maxDeepCount, System.Reflection.BindingFlags flags)
        {
            if (null == obj || deepIndex > maxDeepCount)
                return string.Empty;

            var retValue = new System.Text.StringBuilder();
            var paramPairs = new List<SerializableInformationPair>();

            //如果是重写过ToString方法的则直接返回ToString()
            var method = obj.GetType().GetMethod("ToString", new Type[] { });
            if (null != method && method.DeclaringType == obj.GetType())
            {
                paramPairs.Add(new SerializableInformationPair() { isProperty = false, name = string.Empty, value = obj, type = obj.GetType() });
            }
            else
            {
                var properties = obj.GetType().GetProperties(flags);
                var fields = obj.GetType().GetFields(flags);

                if (!properties.IsNullOrEmpty())
                {
                    for (int i = 0; i < properties.Length; ++i)
                    {
                        var propertyTmp = properties[i];
                        object valueTmp = null;
                        try
                        {
                            valueTmp = propertyTmp.GetValue(obj, null);
                        }
                        catch { }


                        paramPairs.Add(new SerializableInformationPair() { isProperty = true, name = propertyTmp.Name, value = valueTmp, type = propertyTmp.PropertyType });
                    }
                }

                if (!fields.IsNullOrEmpty())
                {
                    for (int i = 0; i < fields.Length; ++i)
                    {
                        var fieldTmp = fields[i];
                        object valueTmp = null;

                        try
                        {
                            valueTmp = fieldTmp.GetValue(obj);
                        }
                        catch { }

                        paramPairs.Add(new SerializableInformationPair() { isProperty = false, name = fieldTmp.Name, value = valueTmp, type = fieldTmp.FieldType });
                    }
                }
            }

            if (!paramPairs.IsNullOrEmpty())
            {
                var spaceString = new System.Text.StringBuilder();
                var onceSpaceString = "   ";
                for (int j = 0; j < deepIndex; ++j)
                {
                    spaceString.Append(onceSpaceString);
                }

                for (int i = 0; i < paramPairs.Count; ++i)
                {
                    var nameTmp = paramPairs[i].name;
                    var valueTmp = paramPairs[i].value;
                    var typeTmp = paramPairs[i].type;

                    //如果是C#反射中的类型直接过滤掉
                    if (typeTmp.FullName.StartsWith("System.Reflection"))
                    {
                        continue;
                    }

                    retValue.Append(spaceString);

                    //如果是集合则需要便利他
                    if (typeTmp.IsInherited<System.Collections.ICollection>())
                    {
                        var collectionTmp = valueTmp as System.Collections.ICollection;
                        retValue.Append(string.Format("\"[{0}] Size\" : {1}", nameTmp, collectionTmp.Count));
                        retValue.Append("\n");

                        retValue.Append(spaceString);
                        retValue.Append("{\n");
                        foreach (var iter in collectionTmp)
                        {
                            var appendStringTmp = GetSerializableInformation(iter, deepIndex + 1, maxDeepCount, flags);
                            if (string.IsNullOrEmpty(appendStringTmp))
                            {
                                retValue.Append(spaceString + onceSpaceString);

                                var valueNameTmp = null == iter ? "null" : iter.ToString();
                                var nullTypeNameTmp = typeTmp.UnderlyingSystemType.ToString().Substring("[", "]");
                                var convertTypeName = null == iter ? shaco.Base.GameHelper.dateTypesTemplate.TypeToCustomString(nullTypeNameTmp) : shaco.Base.GameHelper.dateTypesTemplate.TypeToCustomString(iter.GetType());
                                if (typeTmp == typeof(string))
                                    retValue.Append(string.Format("\"[{0}]\" : \"{1}\"", convertTypeName, valueNameTmp));
                                else
                                    retValue.Append(string.Format("\"[{0}]\" : {1}", convertTypeName, valueNameTmp));
                                retValue.Append("\n");
                            }
                            else
                            {
                                retValue.Append(appendStringTmp);
                            }
                        }
                        retValue.Append(spaceString);
                        retValue.Append("}\n");
                    }
                    else
                    {
                        var convertTypeName = shaco.Base.GameHelper.dateTypesTemplate.TypeToCustomString(typeTmp);

                        var valueNameTmp = null == valueTmp ? "null" : valueTmp.ToString();
                        if (typeTmp == typeof(string))
                            retValue.Append(string.Format("\"[{0}] {1}\" : \"{2}\"", convertTypeName, nameTmp, valueNameTmp));
                        else
                            retValue.Append(string.Format("\"[{0}] {1}\" : {2}", convertTypeName, nameTmp, valueNameTmp));
                        retValue.Append("\n");

                        //case 1: 空引用不继续递归
                        //case 2: 不处理相同的数据，防止递归死循环
                        //case 3: 如果是已知自定义类型则不再继续递归了
                        var fullTypeName = typeTmp.ToTypeString();
                        bool isSameType = convertTypeName.Equals(fullTypeName);
                        if (null != valueTmp && !valueTmp.Equals(obj) && isSameType)
                        {
                            string appendStringTmp = string.Empty;

                            //case 1: 因为属性存在动态方法可能会有递归获取死循环的问题
                            //        所以当发现get中获取到的数据类型和自身一样，则不再向下递归了，而是直接以字符串格式化出数据显示
                            //case 2: 当类型自身已经重写过Tostring()方法的时候，也不再向下递归了
                            var methodDeep = typeTmp.GetMethod("ToString", new Type[] { });
                            if ((paramPairs[i].isProperty && valueTmp.GetType() == obj.GetType())
                                || (null != methodDeep && methodDeep.DeclaringType == typeTmp))
                            {
                                appendStringTmp = string.Empty;
                            }
                            else
                            {
                                appendStringTmp = GetSerializableInformation(valueTmp, deepIndex + 1, maxDeepCount, flags);
                            }
                            if (!string.IsNullOrEmpty(appendStringTmp))
                            {
                                retValue.Append(spaceString);
                                retValue.Append("{\n");
                                retValue.Append(appendStringTmp);
                                retValue.Append(spaceString);
                                retValue.Append("}\n");
                            }
                        }
                    }
                }
            }
            return retValue.ToString();
        }

        static public Dictionary<T, T> CollectDependenciesExBase<T>(ref Dictionary<T, T> hasCheckedTargets, T target)
        {
            Dictionary<T, T> depencies = new Dictionary<T, T>();

            //避免重复计算引用
            if (hasCheckedTargets.ContainsKey(target))
                return depencies;
            hasCheckedTargets.Add(target, target);

            //获取引用参数信息
            var fields = target.GetType().GetFields(System.Reflection.BindingFlags.Public
                                                    | System.Reflection.BindingFlags.Instance
                                                    | System.Reflection.BindingFlags.DeclaredOnly
                                                    | System.Reflection.BindingFlags.NonPublic
                                                    | System.Reflection.BindingFlags.Static);
            if (!fields.IsNullOrEmpty())
            {
                for (int j = 0; j < fields.Length; ++j)
                {
                    object getValueTmp = null;

                    //过滤已经过时或者无法获取的参数
                    try
                    {
                        //不计算含有弃用属性的对象
                        if (!fields[j].CustomAttributes.Any(v => v.AttributeType == typeof(System.ObsoleteAttribute)))
                            getValueTmp = fields[j].GetValue(target);
                    }
                    catch
                    {
                        //忽略异常
                    }

                    if (null != getValueTmp && getValueTmp.GetType().IsInherited<T>())
                    {
                        //去除重复引用
                        if (!depencies.ContainsKey((T)getValueTmp))
                        {
                            depencies.Add((T)getValueTmp, (T)getValueTmp);
                        }
                    }
                }
            }

            //获取引用get方法信息
            var properties = target.GetType().GetProperties(System.Reflection.BindingFlags.Public
                                                            | System.Reflection.BindingFlags.Instance
                                                            | System.Reflection.BindingFlags.GetProperty
                                                            | System.Reflection.BindingFlags.GetField);
            if (!properties.IsNullOrEmpty())
            {
                for (int j = 0; j < properties.Length; ++j)
                {
                    object getValueTmp = null;

                    //过滤已经过时或者无法获取的参数
                    try
                    {
                        //不计算含有弃用属性的对象
                        if (!properties[j].CustomAttributes.Any(v => v.AttributeType == typeof(System.ObsoleteAttribute)))
                            getValueTmp = properties[j].GetValue(target, null);
                    }
                    catch
                    {
                        //忽略异常
                    }

                    if (null != getValueTmp && getValueTmp.GetType().IsInherited<T>())
                    {
                        //去除重复引用
                        if (!depencies.ContainsKey((T)getValueTmp))
                        {
                            depencies.Add((T)getValueTmp, (T)getValueTmp);
                        }
                    }
                }
            }

            var depenciesTmp = depencies.Values.ToArray();
            foreach (var iter in depenciesTmp)
            {
                //不再重复计算自己引用
                if (target.Equals(iter))
                    continue;

                var subDepencies = CollectDependenciesExBase(ref hasCheckedTargets, iter);
                foreach (var iter2 in subDepencies)
                {
                    //去除重复引用
                    if (!depencies.ContainsKey(iter2.Key))
                    {
                        depencies.Add((T)iter2.Key, (T)iter2.Value);
                    }
                }
            }
            return depencies;
        }
    }
}
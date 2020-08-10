using System.Collections;
using System.Collections.Generic;

[System.Diagnostics.DebuggerStepThrough]
static public class shaco_ExtensionsUtility 
{
    //delete by shaco 2020/05/12
    //TODO: 通过ToString字符串判断一个对象是否为空存在GC问题，正式弃用该方法
    // /// <summary>
    // /// 判断对象是否为空，支持c#和unity空引用对象的判断
    // /// <param name="obj">对象</param>
    // /// <return>是否为空</return>
    // /// </summary>
	// static public bool IsNull(this object obj)
    // {
    //     if (null == obj)
    //         return true;
    //     else
    //     {
    //         string typeNameTmp = obj.ToString();
    //         return (typeNameTmp.Length == 4 && typeNameTmp == "null");
    //     }
    // }

    /// <summary>
    /// 获取对象类型全称
    /// <param name="obj">对象</param>
    /// <return>类型全称</return>
    /// </summary>
	static public string ToTypeString(this object obj)
    {
        return null == obj ? "null" : obj.GetType().FullName;
    }

    /// <summary>
    /// 获取类型全称
    /// <param name="type">类型</param>
    /// <return>类型全称</return>
    /// </summary>
    static public string ToTypeString(this System.Type type)
    {
        return null == type ? "null" : type.FullName;
    }
	
    /// <summary>
    /// 通过类型实例化一个对象
    /// <param name="type">类型</param>
    /// <return>实例化的对象，如果实例化失败则返回整型0</return>
    /// </summary>
    static public object Instantiate(this System.Type type)
    {
        if (type == typeof(short) 
            || type == typeof(int) 
            || type == typeof(long) 
            || type == typeof(ushort)
            || type == typeof(uint)
            || type == typeof(ulong))
        {
            return 0;
        }
        else if (type == typeof(float)) return 0.0f;
        else if (type == typeof(double)) return 0.0;
        else if (type == typeof(string)) return string.Empty;
        else if (type == typeof(bool)) return false;
        else if (type == typeof(char)) return '0';

        if (null == type || type.IsAbstract)
            return null;
        else
        {
            var bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
            var constructors = type.GetConstructors(bindingFlags);
            System.Reflection.ConstructorInfo defaultConstructorInfo = null;
            if (null != constructors)
            {
                for (int i = 0; i < constructors.Length; ++i)
                {
                    if (!constructors[i].IsAbstract && constructors[i].GetParameters().Length == 0)
                    {
                        defaultConstructorInfo = constructors[i];
                        break;
                    }
                }
            }

            if (null == defaultConstructorInfo)
                return null;

            if (type.IsArray)
            {
                var paramsType = shaco.Base.Utility.Assembly.GetTypeWithinLoadedAssemblies(type.FullName.RemoveBehind("[]"));
                var retValue = defaultConstructorInfo.Invoke(new object[] { paramsType.Instantiate() });
                return retValue;
            }
            else
            {
                var retValue = defaultConstructorInfo.Invoke(null);
                return retValue;
            }
        }
    }

    /// <summary>
    /// 通过类型实例化一个对象
    /// <param name="type">类型</param>
    /// <return>实例化的对象，如果实例化失败则返回整型0</return>
    /// </summary>
    static public T Instantiate<T>(this T target)
    {
        if (null == target)
            return default(T);

        return (T)target.GetType().Instantiate();
    }

    /// <summary>
    /// 判断类型继承关系
    /// <param name="type">type</param>
    /// <return>继承自PARENT_TYPE返回true，反之false</return>
    /// </summary>
    static public bool IsInherited<PARENT_TYPE>(this System.Type type)
	{
		return type.IsInherited(typeof(PARENT_TYPE));
	}

    /// <summary>
    /// 判断类型继承关系
    /// <param name="type">type</param>
    /// <param name="ParentType">父类型</param>
    /// <return>继承自IsInherited返回true，反之false</return>
    /// </summary>
    static public bool IsInherited(this System.Type type, System.Type ParentType)
    {
        bool retValue = false;

        //如果和自身类型一样，则立即返回
        if (type == ParentType)
        {
            return true;
        }

        if (null == type)
        {
            shaco.Base.Log.Error("ExtensionUtility IsInherited error: type is null");
            return retValue;
        }

        var parentType = ParentType;
        retValue = type.IsSubclassOf(parentType);

        if (!retValue)
        {
            var interfaces = type.GetInterfaces();
            for (int i = interfaces.Length - 1; i >= 0; --i)
            {
                if (interfaces[i] == parentType)
                {
                    retValue = true;
                    break;
                }
            }
        }
        return retValue;
    }

    /// <summary>
    /// 通过字符串调用对象方法 
    /// <param name="target">对象</param>
    /// <param name="method">方法名字</param>
    /// <param name="parameters">方法参数</param>
    /// <return>调用成功返回true, 反之false</return>
    /// </summary>
    static public object InvokeMethod(this object target, string method, params object[] parameters)
    {
		if (null == target)
		{
			shaco.Base.Log.Error("ExtensionUtility InvokeMethod error: target is null");
			return null;
		}

        var targetType = target is System.Type ? (System.Type)target : target.GetType();

		if (string.IsNullOrEmpty(method))
		{
			shaco.Base.Log.Error("ExtensionUtility InvokeMethod error: method is empty, target=" + target + " type=" + targetType);
            return null;
		}

        var methodTmp = targetType.GetMethod(method);
        if (null == methodTmp)
        {
            shaco.Base.Log.Error("ExtensionUtility InvokeMethod error: not found method by target=" + target + " method=" + method + " type=" + targetType);
            return null;
        }

        if (targetType.IsAbstract && targetType.IsSealed)
            return methodTmp.Invoke(null, parameters);
        else
            return methodTmp.Invoke(target, parameters);
    }

    /// <summary>
    /// 交换2个对象引用
    /// <param name="me">当前对象</param>
    /// <param name="other">需要被交换的对象的引用</param>
    /// <return>交换后的当前对象</return>
    /// </summary>
    static public T SwapValue<T>(this T me, ref T other)
    {
        T exchangeValue = me;
        me = other;
        other = exchangeValue;
        return me;
    }

    /// <summary>
    /// 判断字符串是否为数字
    /// <param name="str">字符串</param>
    /// <return>为数字返回true，反之false</return>
    /// </summary>
    static public bool IsNumber(this string str)
    {
        return shaco.Base.Utility.IsNumber(str);
    }

    /// <summary>
    /// 给字典添加一个集合
    /// <param name="map">当前字典</param>
    /// <param name="other">需要添加的字典数据</param>
    /// </summary>
    static public void AddRange<KEY, VALUE>(this System.Collections.Generic.Dictionary<KEY, VALUE> map, IEnumerable<KeyValuePair<KEY, VALUE>> other)
    {
        if (null != other)
        {
            foreach (var iter in other)
            {
                if (!map.ContainsKey(iter.Key))
                {
                    map.Add(iter.Key, iter.Value);
                }
                else
                {
                    shaco.Base.Log.Error("Dictionary AddRange error: duplicate key=" + iter.Key);
                }
            }
        }
        else
        {
            shaco.Base.Log.Error("Dictionary AddRange error: invalid param");
        }
    }

    /// <summary>
    /// 批量移除字典数据
    /// <param name="map">当前字典</param>
    /// <param name="keys">需要移除的字典键值</param>
    /// </summary>
    static public void RemoveRange<KEY, VALUE>(this System.Collections.Generic.Dictionary<KEY, VALUE> map, System.Collections.Generic.List<KEY> keys)
    {
        if (null != keys)
        {
            foreach (var iter in keys)
            {
                if (map.ContainsKey(iter))
                {
                    map.Remove(iter);
                }
                else
                {
                    shaco.Base.Log.Error("Dictionary RemoveRange[List] error: not found key=" + iter);
                }
            }
        }
        else
        {
            shaco.Base.Log.Error("Dictionary RemoveRange[List] error: invalid param");
        }
    }

    //// <summary>
    /// 获取类型中包含的属性
    /// <param name="type">类型</param>
    /// <return>如果包含该属性返回属性对象，否则返回null</return>
    /// </summary>
    static public T GetAttribute<T>(this System.Type type) where T : System.Attribute
    {
        return (T)GetAttribute(type, typeof(T));
    }
    static public System.Attribute GetAttribute(this System.Type type, System.Type attributeType)
    {
        System.Attribute retValue = null;
        if (type.IsDefined(attributeType, false))
        {
            retValue = (System.Attribute)type.GetCustomAttributes(attributeType, false)[0];
        }
        return retValue;
    }

    static object ToType(object obj, System.Type conversionType)
    {
        System.Convert.ChangeType(obj, conversionType);
        return null;
    }
}

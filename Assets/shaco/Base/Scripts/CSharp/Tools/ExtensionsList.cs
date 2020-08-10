using System.Collections;
using System.Collections.Generic;

static public class shaco_ExtensionsList
{
    static public List<T> ToList<T>(this IEnumerable<T> list)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList ToList erorr: list is null");
            return null;
        }

        if (list.GetType() == typeof(List<T>))
            return (List<T>)list;

        var ret = new List<T>(list);
        return ret;
    }

    static public T[] ToArray<T>(this ICollection<T> list)
    {
         if (null == list)
         {
            shaco.Base.Log.Error("ExtensionsList ToArray erorr: list is null");
            return null;
        }

        if (list.GetType() == typeof(T[]))
            return (T[])list;

        var ret = new T[list.Count];
        int index = 0;
        foreach (var iter in list)
        {
            ret[index++] = iter;
        }
        return ret;
    }

    static public T1[] ToKeyArray<T1, T2>(this IDictionary<T1, T2> dic)
    {
         if (null == dic)
         {
            shaco.Base.Log.Error("ExtensionsList ToKeyArray erorr: dic is null");
            return null;
         }

        T1[] ret = new T1[dic.Count];
        int index = 0;
        foreach (var iter in dic)
        {
           ret[index++] = iter.Key;
        }
        return ret;
    }

    static public T2[] ToValueArray<T1, T2>(this IDictionary<T1, T2> dic)
    {
        if (null == dic)
        {
            shaco.Base.Log.Error("ExtensionsList ToValueArray erorr: dic is null");
            return null;
        }

        T2[] ret = new T2[dic.Count];
        int index = 0;
        foreach (var iter in dic)
        {
            ret[index++] = iter.Value;
        }
        return ret;
    }

    static public List<T1> ToKeyList<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> dic)
    {
        if (null == dic)
        {
            shaco.Base.Log.Error("ExtensionsList ToKeyList erorr: dic is null");
            return null;
        }

        List<T1> ret = new List<T1>();
        foreach (var iter in dic)
        {
            ret.Add(iter.Key);
        }
        return ret;
    }

    static public List<T2> ToValueList<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> dic)
    {
        if (null == dic)
        {
            shaco.Base.Log.Error("ExtensionsList ToValueList erorr: dic is null");
            return null;
        }

        List<T2> ret = new List<T2>();
        foreach (var iter in dic)
        {
            ret.Add(iter.Value);
        }
        return ret;
    }

    static public T2[] Convert<T1, T2>(this ICollection<T1> list, System.Func<T1, T2> callbackConvert)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList Convert erorr: list is null");
            return null;
        }

        if (null == callbackConvert)
        {
            shaco.Base.Log.Error("ExtensionsList Convert error: missing param 'callbackConvert'");
            return null;
        }

        var ret = new T2[list.Count];
        int index = 0;
        foreach (var iter in list)
        {
            ret[index++] = callbackConvert(iter);
        }
        return ret;
    }

    static public IList<T2> ConvertList<T1, T2>(this ICollection<T1> list, System.Func<T1, T2> callbackConvert)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList ConvertList erorr: list is null");
            return null;
        }

        if (null == callbackConvert)
        {
            shaco.Base.Log.Error("ExtensionsList ConvertList error: missing param 'callbackConvert'");
            return null;
        }

        var ret = new List<T2>();
        foreach (var iter in list)
        {
            ret.Add(callbackConvert(iter));
        }
        return ret;
    }

    static public bool SwapValue<T>(this IList<T> list, int sourceIndex, int destinationIndex)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList SwapValue erorr: list is null");
            return false;
        }

        if (list.Count <= 1)
        {
            return false;
        }

        if (sourceIndex == destinationIndex)
            return false;

        if (sourceIndex < 0 || sourceIndex > list.Count - 1 || destinationIndex < 0 || destinationIndex > list.Count - 1)
        {
            shaco.Base.Log.Error("ExtensionsList SwapValue error: out of range sourceIndex=" + sourceIndex + " destinationIndex=" + destinationIndex + " Count=" + list.Count);
            return false;
        }

        var sourceItem = list[sourceIndex];
        var desItem = list[destinationIndex];
        list[sourceIndex] = desItem;
        list[destinationIndex] = sourceItem;
        return true;
    }

    //moveOffset: -1(move all values behind to front) 
    //             1(move all values front to behind)
    static public void MoveValues<T>(this IList<T> list, int moveOffset)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList MoveValues erorr: list is null");
            return;
        }

        if (list.Count <= 1)
            return;

        if (moveOffset > 0)
        {
            var firstData = list[0];
            for (int i = 0; i < list.Count - 1; ++i)
            {
                list[i] = list[i + 1];
            }
            list[list.Count - 1] = firstData;
        }
        else
        {
            var lastData = list[list.Count - 1];
            for (int i = list.Count - 1; i >= 1; --i)
            {
                list[i] = list[i - 1];
            }
            list[0] = lastData;
        }
    }

    static public bool IsNullOrEmpty<KEY, VALUE>(this IDictionary<KEY, VALUE> list)
    {
        return list == null || list.Count == 0;
    }

    static public bool IsNullOrEmpty<T>(this ICollection<T> list)
    {
        return list == null || list.Count == 0;
    }

    static public void CopyFrom<T>(this IList<T> list, List<T> other)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList CopyFrom 1 erorr: list is null");
            return;
        }

        if (null == other)
        {
            shaco.Base.Log.Error("ExtensionsList CopyFrom 1 erorr: other is null");
            return;
        }

        list.Clear();
        int countTmp = other.Count;
        for (int i = 0; i < countTmp; ++i)
        {
            list.Add(other[i]);
        }
    }

    static public void CopyFrom<T>(this IList<T> list, T[] other)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList CopyFrom 2 erorr: list is null");
            return;
        }

        if (null == other)
        {
            shaco.Base.Log.Error("ExtensionsList CopyFrom 2 erorr: other is null");
            return;
        }

        list.Clear();
        int countTmp = other.Length;
        for (int i = 0; i < countTmp; ++i)
        {
            list.Add(other[i]);
        }
    }

    static public void CopyFrom<T>(this T[] list, List<T> other)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList CopyFrom 3 erorr: list is null");
            return;
        }

        if (null == other)
        {
            shaco.Base.Log.Error("ExtensionsList CopyFrom 3 erorr: other is null");
            return;
        }

        int countTmp = other.Count;
        list = new T[countTmp];
        for (int i = 0; i < countTmp; ++i)
        {
            list[i] = other[i];
        }
    }

    static public void CopyFrom<T>(this T[] list, T[] other)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList CopyFrom 4 erorr: list is null");
            return;
        }

        if (null == other)
        {
            shaco.Base.Log.Error("ExtensionsList CopyFrom 4 erorr: other is null");
            return;
        }

        int countTmp = other.Length;
        list = new T[countTmp];
        for (int i = 0; i < countTmp; ++i)
        {
            list[i] = other[i];
        }
    }

    static public int IndexOf<T>(this T[] list, T find)
    {
        int retValue = -1;
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList IndexOf erorr: list is null");
            return retValue;
        }

        if (null == find)
        {
            shaco.Base.Log.Error("ExtensionsList IndexOf erorr: find is null");
            return retValue;
        }

        for (int i = 0; i < list.Length; ++i)
        {
            if (list[i].Equals(find))
            {
                retValue = i;
                break;
            }
        }
        return retValue;
    }

    /// <summary>
    /// 移除数组中标记的组建
    /// <param name="tag">要被移除的查找标记</param>
    /// </summary>
    static public void Trim<T>(this IList<T> list, T tag)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList Trim erorr: list is null");
            return;
        }

        if (null == tag)
        {
            shaco.Base.Log.Error("ExtensionsList Trim erorr: tag is null");
            return;
        }

        for (int i = list.Count - 1; i >= 0; --i)
        {
            if (list[i].Equals(tag))
            {
                list.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 获取数组成员连接的字符串
    /// <param name="list">数组</param>
    /// <param name="contactFlag">连接字符</param>
    /// <return>连接字符串</return>
    /// </summary>
    static public string ToContactString(this IEnumerable collection, string contactFlag)
    {
        if (null == collection)
        {
            shaco.Base.Log.Error("ExtensionsList ToContactString erorr: collection is null");
            return string.Empty;
        }

        if (string.IsNullOrEmpty(contactFlag))
        {
            shaco.Base.Log.Error("ExtensionsList ToContactString erorr: contactFlag is empty string");
            return string.Empty;
        }

        var retValue = new System.Text.StringBuilder();

        foreach (var iter in collection)
        {
            retValue.Append(iter.ToString());
            retValue.Append(contactFlag);
        }

        if (retValue.Length >= contactFlag.Length)
        {
            retValue.Remove(retValue.Length - contactFlag.Length, contactFlag.Length);
        }
        return retValue.ToString();
    }

    /// <summary>
    /// 获取数组序列化字符串
    /// <param name="list">数组</param>
    /// <param name="maxLogItemLength">最大日志数组组件长度</param>
    /// <return>序列化字符串</return>
    /// </summary>
    static public string ToSerializeString<T>(this ICollection<T> list, int maxLogItemLength = 50)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList ToSerializeString erorr: list is null");
            return string.Empty;
        }

        var retValue = new System.Text.StringBuilder();
        int loopCount = System.Math.Min(list.Count, maxLogItemLength);
        int index = 0;

        foreach (var iter in list)
        {
            retValue.Append("[");
            retValue.Append(index++);
            retValue.Append("]");
            retValue.Append(iter);
            retValue.Append((index % 4 == 0) ? "\n" : "\t");
        }
        return retValue.ToString();
    }

    /// <summary>
    /// 获取字典序列化字符串
    /// <param name="dic">字典</param>
    /// <param name="maxLogItemLength">最大日志数组组件长度</param>
    /// <return>序列化字符串</return>
    /// </summary>
    static public string ToSerializeString<KEY, VALUE>(this IDictionary<KEY, VALUE> dic, int maxLogItemLength = 50)
    {
        if (null == dic)
        {
            shaco.Base.Log.Error("ExtensionsList ToSerializeString erorr: dic is null");
            return string.Empty;
        }

        var retValue = new System.Text.StringBuilder();
        int index = 0;
        int loopCount = System.Math.Min(dic.Count, maxLogItemLength);
        foreach (var iter in dic)
        {
            if (index >= loopCount)
                break;

            retValue.Append("[");
            retValue.Append(iter.Key);
            retValue.Append("]");

            var typeValueTmp = iter.Value.GetType();

            if (typeValueTmp.IsClass || typeValueTmp.IsInherited<System.Collections.ICollection>())
            {
                retValue.Append(typeValueTmp.Name);
            }
            else
            {
                retValue.Append(iter.Value);
            }
            retValue.Append(((index++ + 1) % 3 == 0) ? "\n" : "\t");
        }
        return retValue.ToString();
    }

    /// <summary>
    /// 判断下标是否越界
    /// <param name="list">数组对象</param>
    /// <param name="index">当前下标</param>
    /// <return>true:越界 false:不越界</return>
    /// </summary>
    static public bool IsOutOfRange<T>(this IList<T> list, int index)
    {
        if ((null == list || 0 == list.Count))
        {
            // shaco.Base.Log.Error("list is empty, index=" + index);
            return true;
        }
        return index < 0 || index > list.Count - 1;
    }

    /// <summary>
    /// 打印集合越界日志
    /// <param name="list">数组对象</param>
    /// <param name="index">当前下标</param>
    /// <param name="erorrMessagePrefix">发生越界错误的提示文本前缀</param>
    /// <param name="maxLogItemLength">最大日志数组组件长度</param>
    /// </summary>
    [System.Diagnostics.Conditional("DEBUG_LOG")]
    static public void DebugLogOutOfRange<T>(this ICollection<T> list, int index, string erorrMessagePrefix, int maxLogItemLength = 50)
    {
        var errorString = new System.Text.StringBuilder(erorrMessagePrefix);
        errorString.Append(": out of range, index=" + index + " count=" + (null == list ? "null" : list.Count.ToString()));
        shaco.Base.Log.Error(errorString.ToString());
        errorString.Remove(0, errorString.Length);

        shaco.Base.Log.Error(ToSerializeString(list, maxLogItemLength));
    }

    /// <summary>
    /// 判断字典是否越界
    /// <param name="dic">字典对象</param>
    /// <param name="key">查找key</param>
    /// <return>true:越界 false:不越界</return>
    /// </summary>
    static public bool IsOutOfRange<KEY, VALUE>(this IDictionary<KEY, VALUE> dic, KEY key)
    {
        if ((null == dic || 0 == dic.Count))
        {
            // shaco.Base.Log.Error("dic is empty, key=" + key);
            return true;
        }
        return null == key || !dic.ContainsKey(key);
    }

    /// <summary>
    /// 打印集合越界日志
    /// <param name="dic">字典对象</param>
    /// <param name="key">查找key</param>
    /// <param name="erorrMessagePrefix">发生越界错误的提示文本前缀</param>
    /// <param name="maxLogItemLength">最大日志数组组件长度</param>
    /// </summary>
    [System.Diagnostics.Conditional("DEBUG_LOG")]
    static public void DebugLogOutOfRange<KEY, VALUE>(this IDictionary<KEY, VALUE> dic, KEY key, string erorrMessagePrefix, int maxLogItemLength = 50)
    {
        var errorString = new System.Text.StringBuilder(erorrMessagePrefix);
        errorString.Append(": not found in dictionary, key=" + key + " count=" + (null == dic ? "null" : dic.Count.ToString()));
        shaco.Base.Log.Error(errorString.ToString());
        errorString.Remove(0, errorString.Length);
        shaco.Base.Log.Error(ToSerializeString(dic, maxLogItemLength));
    }

    static public void RemoveOne<T>(this T[] array, System.Predicate<T> match)
    {
        if (null == array)
        {
            shaco.Base.Log.Error("ExtensionsList RemoveOne 1 erorr: array is null");
            return;
        }

        if (null == match)
        {
            shaco.Base.Log.Error("ExtensionsList RemoveOne 1 erorr: match is null");
            return;
        }

        for (int i = 0; i < array.Length; ++i)
        {
            if (match(array[i]))
            {
                array[i] = default(T);
                break;
            }
        }
    }

    static public bool RemoveOne<T>(this IList<T> list, System.Predicate<T> match)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList RemoveOne 2 erorr: list is null");
            return false;
        }

        if (null == match)
        {
            shaco.Base.Log.Error("ExtensionsList RemoveOne 2 erorr: match is null");
            return false;
        }

        bool retValue = false;
        for (int i = 0; i < list.Count; ++i)
        {
            if (match(list[i]))
            {
                list.RemoveAt(i);
                retValue = true;
                break;
            }
        }
        return retValue;
    }

    static public T Find<T>(this T[] array, System.Predicate<T> match)
    {
        var retValue = default(T);

        if (null == array)
        {
            shaco.Base.Log.Error("ExtensionsList Find erorr: array is null");
            return retValue;
        }

        if (null == match)
        {
            shaco.Base.Log.Error("ExtensionsList Find erorr: match is null");
            return retValue;
        }
            
        for (int i = 0; i < array.Length; ++i)
        {
            if (match(array[i]))
            {
                retValue = array[i];
                break;
            }
        }
        return retValue;
    }

    static public T[] FindAll<T>(this T[] array, System.Predicate<T> match)
    {
        var retValue = new List<T>();

        if (null == array)
        {
            shaco.Base.Log.Error("ExtensionsList FindAll erorr: array is null");
            return retValue.ToArray();
        }

        if (null == match)
        {
            shaco.Base.Log.Error("ExtensionsList FindAll erorr: match is null");
            return retValue.ToArray();
        }

        for (int i = 0; i < array.Length; ++i)
        {
            if (match(array[i]))
            {
                retValue.Add(array[i]);
            }
        }
        return retValue.ToArray();
    }

    /// <summary>
    /// 保留列表中一定数量内容，移除其他内容
    /// </summary>
    static public void Retain<T>(this System.Collections.Generic.List<T> list, int start, int end)
    {
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList Retain erorr: array is null");
            return;
        }

        if (start < 0)
            start = 0;
        else if (start > list.Count - 1)
            start = list.Count - 1;

        if (end < 0)
            end = 0;
        else if (end > list.Count - 1)
            end = list.Count - 1;

        if (end < start)
        {
            int temp = start;
            start = end;
            end = temp;
        }

        for (int i = list.Count - 1; i > end; --i)
        {
            list.RemoveAt(i);
        }

        for (int i = start - 1; i >= 0; --i)
        {
            list.RemoveAt(i);
        }
    }

    static public int IndexOf<T>(this IList<T> list, System.Predicate<T> match)
    {
        int retValue = -1;
        if (null == list)
        {
            shaco.Base.Log.Error("ExtensionsList IndexOf erorr: list is null");
            return retValue;
        }

        if (null == match)
        {
            shaco.Base.Log.Error("ExtensionsList IndexOf erorr: match is null");
            return retValue;
        }

        for (int i = 0; i < list.Count; ++i)
        {
            if (match(list[i]))
            {
                retValue = i;
                break;
            }
        }
        return retValue;
    }

    //修正数组大小，超出部分移除，不足部分填充默认参数
    static public T[] FixSize<T>(this T[] list, int size, T defaultValue)
    {
        T[] retValue = list;

        //超出首行数据时候默认丢弃
        if (list.Length > size)
        {
            var valuesListTmp = list.ToList();
            valuesListTmp.RemoveRange(list.Length - size + 1, list.Length - size);
            retValue = valuesListTmp.ToArray();
        }

        //行数据不足时候自动用空字符串补齐
        if (list.Length < size)
        {
            var valuesListTmp = list.ToList();
            for (int i = size - list.Length - 1; i >= 0; --i)
                valuesListTmp.Add(defaultValue);
            retValue = valuesListTmp.ToArray();
        }
        return retValue;
    }

    //修正数组大小，超出部分移除，不足部分填充默认参数
    static public void FixSize<T>(this IList<T> list, int size, T defaultValue)
    {
        //超出首行数据时候默认丢弃
        if (list.Count > size)
        {
            int startIndex = list.Count - size + 1;
            for (int i = list.Count - 1; i >= startIndex; --i)
                list.RemoveAt(i);
        }

        //行数据不足时候自动用空字符串补齐
        if (list.Count < size)
        {
            for (int i = size - list.Count - 1; i >= 0; --i)
                list.Add(defaultValue);
        }
    }
}
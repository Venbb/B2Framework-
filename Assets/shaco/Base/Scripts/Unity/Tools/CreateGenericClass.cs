using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace shaco
{
    public class CreateGenericClass
    {
        //c#常用泛型类
        static public List<object> List() { return new List<object>(); }
        static public List<object> List(IEnumerable<object> collection) { return new List<object>(collection); }
        static public Dictionary<object, object> Dictionary() { return new Dictionary<object, object>(); }
        static public Dictionary<object, object> Dictionary(IEqualityComparer<object> comparer) { return new Dictionary<object, object>(comparer); }
        static public Dictionary<object, object> Dictionary(IDictionary<object, object> dictionary) { return new Dictionary<object, object>(); }
        static public Dictionary<object, object> Dictionary(IDictionary<object, object> dictionary, IEqualityComparer<object> comparer) { return new Dictionary<object, object>(dictionary, comparer); }
        static public Queue<object> Queue() { return new Queue<object>(); }
        static public Stack<object> Stack() { return new Stack<object>(); }
        static public SortedDictionary<object, object> SortedDictionary() { return new SortedDictionary<object, object>(); }
        static public HashSet<object> HashSet() { return new HashSet<object>(); }
        static public KeyValuePair<object, object> KeyValuePair() { return new KeyValuePair<object, object>(); }

        //shaco框架常用泛型类
        static public shaco.Base.EventCallBack<object> EventCallBack() { return new shaco.Base.EventCallBack<object>(); }
        static public shaco.Base.ISubject<object> Subject() { return new shaco.Base.Subject<object>(); }

        // static public List<T> List<T>() { return new List<T>(); }
        // static public List<T> List<T>(IEnumerable<T> collection) { return new List<T>(collection); }
        // static public Dictionary<KEY, VALUE> Dictionary<KEY, VALUE>() { return new Dictionary<KEY, VALUE>(); }
        // static public Dictionary<KEY, VALUE> Dictionary<KEY, VALUE>(IEqualityComparer<KEY> comparer) { return new Dictionary<KEY, VALUE>(comparer); }
        // static public Dictionary<KEY, VALUE> Dictionary<KEY, VALUE>(IDictionary<KEY, VALUE> dictionary) { return new Dictionary<KEY, VALUE>(); }
        // static public Dictionary<KEY, VALUE> Dictionary<KEY, VALUE>(IDictionary<KEY, VALUE> dictionary, IEqualityComparer<KEY> comparer) { return new Dictionary<KEY, VALUE>(dictionary, comparer); }
        // static public Queue<T> Queue<T>() { return new Queue<T>(); }
        // static public Stack<T> Stack<T>() { return new Stack<T>(); }
        // static public SortedDictionary<KEY, VALUE> SortedDictionary<KEY, VALUE>() { return new SortedDictionary<KEY, VALUE>(); }
        // static public HashSet<T> HashSet<T>() { return new HashSet<T>(); }
        // static public KeyValuePair<KEY, VALUE> KeyValuePair<KEY, VALUE>() { return new KeyValuePair<KEY, VALUE>(); }

        // //shaco框架常用泛型类
        // static public shaco.Base.EventCallBack<T> EventCallBack<T>() { return new shaco.Base.EventCallBack<T>(); }
    }
}
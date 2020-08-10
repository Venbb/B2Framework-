using System.Collections.Generic;

namespace System.Linq
{
    public static class LinqExtension
    {
        //Tuple需要c#7以上才支持，为了兼容低版本逻辑自定义一个简单的元祖类型
        private class CustomTuple<T1, T2>
        {
            public T1 Item1;
            public T2 Item2;

            public CustomTuple() { }
            public CustomTuple(T1 t1, T2 t2) { Item1 = t1; Item2 = t2; }
        }

        public static T MaxBy<T, TR>(this IEnumerable<T> en, System.Func<T, TR> evaluate) where TR : System.IComparable<TR>
        {
            return en.Select(t => new CustomTuple<T, TR>(t, evaluate(t)))
                .Aggregate((max, next) => next.Item2.CompareTo(max.Item2) > 0 ? next : max).Item1;
        }

        public static T MinBy<T, TR>(this IEnumerable<T> en, System.Func<T, TR> evaluate) where TR : System.IComparable<TR>
        {
            return en.Select(t => new CustomTuple<T, TR>(t, evaluate(t)))
                .Aggregate((max, next) => next.Item2.CompareTo(max.Item2) < 0 ? next : max).Item1;
        }
    }
}
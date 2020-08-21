using System;

namespace B2Framework
{
    public static partial class Utility
    {
        public static partial class Time
        {
            /// <summary>
            /// 当前时区的当前时间
            /// 与DateTime.Now不同的是不受设备影响，是真实的当前时间
            /// </summary>
            /// <value></value>
            public static DateTime Now
            {
                // get { return TimeZone.CurrentTimeZone.ToLocalTime(DateTime.UtcNow); }
                get { return DateTime.UtcNow.ToLocalTime(); }
            }
            /// <summary>
            /// 当前时间戳
            /// </summary>
            /// <returns></returns>
            public static double GetTimestamp()
            {
                var origin = new DateTime(1970, 1, 1);
                return (DateTime.UtcNow - origin).TotalSeconds;
            }
            /// <summary>
            /// 格式化时间
            /// </summary>
            /// <param name="seconds"></param>
            /// <returns></returns>
            public static string ToString(int seconds)
            {
                var ts = TimeSpan.FromSeconds(seconds);
                var timeStr = string.Format("{0}{1}{2}{3}",
                    ts.Days == 0 ? "" : ts.Days + "天",
                    ts.Hours == 0 ? "" : ts.Hours + "小时",
                    ts.Minutes == 0 ? "" : ts.Minutes + "分钟",
                    ts.Seconds == 0 ? "" : ts.Seconds + "秒");

                return timeStr;
            }
        }
    }
}
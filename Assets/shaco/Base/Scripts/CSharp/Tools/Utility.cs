using System.Collections;
using System;
using System.Linq;

namespace shaco.Base
{
    public static partial class Utility
    {
        static private DateTime _prevDateTime = DateTime.Now;

        private class ExecuteMethodInfo
        {
            public System.Type targetType = null;
            public System.Reflection.MethodInfo method = null;
            public shaco.Base.OrderCallBackAttribute attribute = null;
        }
        
        static public TimeSpan GetEplaseTime()
        {
            var nowTimeTmp = DateTime.Now;
            var retValue = nowTimeTmp - _prevDateTime;
            _prevDateTime = nowTimeTmp;
            return retValue;
        }

        static public int Random()
        {
            return GetRandomSeed().Next();
        }

        /// <summary>
        /// include min, exclude max
        /// </summary>
        static public int Random(int min, int max)
        {
            return GetRandomSeed().Next(min, max);
        }

        /// <summary>
        /// include min, exclude max, and keep 8 decial places
        /// </summary>
        static public float Random(float min, float max)
        {
            int maxDigit = 100000000;
            int randNumber = GetRandomSeed().Next((int)(min * maxDigit), (int)(max * maxDigit));
            var retValue = (float)randNumber / (float)maxDigit;
            
            return retValue;
        }

        /// <summary>
        /// compare t1 and t2
        /// </summary>
        /// <returns></returns> 0: t1 == t2    -1: t1 < t2     1: t1 > t2
        public delegate int COMPARE_CALLFUNC(object t1, object t2);

        //todo: get current system time
        static public System.DateTime GetCurrentTime()
        {
            var ret = System.DateTime.Now;
            return ret;
        }

        //todo: get how many days this month 
        static public int GetDaysOfCurrentMonth()
        {
            var nowTime = GetCurrentTime();
            return System.DateTime.DaysInMonth(nowTime.Year, nowTime.Month);
        }

        /// <summary>
        /// convert week of enumeration to index 
        /// </summary>
        /// <param name="dayOfWeek"></param> 
        /// <returns></returns>
        static public int GetWeekOfIndex(System.DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case System.DayOfWeek.Monday: return 0;
                case System.DayOfWeek.Tuesday: return 1;
                case System.DayOfWeek.Wednesday: return 2;
                case System.DayOfWeek.Thursday: return 3;
                case System.DayOfWeek.Friday: return 4;
                case System.DayOfWeek.Saturday: return 5;
                case System.DayOfWeek.Sunday: return 6;
                default: Log.Error("getWeekOfIndex error: invalid nowTime.DayOfWeek=" + dayOfWeek); return 0;
            }
        }

        /// <summary>
        /// 判断字符串是否为数字
        /// </summary>
        static public bool IsNumber(string str)
        {
            bool ret = true;
            for (int i = 0; i < str.Length; ++i)
            {
                var c = str[i];
                if (c < '0' || c > '9')
                {
                    if (c == '.')
                    {
                        if (i == 0 || i == str.Length - 1)
                            ret = false;
                    }
                    else if (c == '-')
                    {
                        if (i != 0)
                            ret = false;
                    }
                    else
                    {
                        ret = false;
                    }
                }
                if (!ret)
                    break;
            }
            if (string.IsNullOrEmpty(str))
                ret = false;
            return ret;
        }

        /// <summary>
        /// 取范围值，在最小和最大之间
        /// <param name="value">当前值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <return>在最小和最大之间的值</return>
        /// </summary>
        static public float Clamp(float value, float min, float max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }

        /// <summary>
        /// 取范围值，在最小和最大之间
        /// <param name="value">当前值</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <return>在最小和最大之间的值</return>
        /// </summary>
        static public int Clamp(int value, int min, int max)
        {
            if (value < min) value = min;
            if (value > max) value = max;
            return value;
        }

        /// <summary>
        /// 遍历并执行默认程序集中所有带该属性的静态方法
        /// <param name="attribute">属性类型</param>
        /// <param name="methodName">方法名字</param>
        /// <param name="parameters">调用方法参数</param>
        /// </summary>
        static public void ExecuteAttributeStaticFunction<T>(params object[] parameters) where T : shaco.Base.OrderCallBackAttribute
        {
            ExecuteAttributeFunction<T>(null, parameters);
        }

        /// <summary>
        /// 遍历并执行默认程序集中所有带该属性的对象方法
        /// <param name="attribute">属性类型</param>
        /// <param name="methodName">方法名字</param>
        /// <param name="target">执行方法对象，如果为空，则表示调用静态方法</param>
        /// <param name="parameters">调用方法参数</param>
        /// </summary>
        static public void ExecuteAttributeInstanceFunction<T>(object target, params object[] parameters) where T : shaco.Base.OrderCallBackAttribute
        {
            ExecuteAttributeFunction<T>(target, parameters);
        }

        /// <summary>
        /// 遍历并执行程序集中所有带该属性的方法，方法名字为属性名字+CallBack
        /// <param name="assemblyNames">程序集</param>
        /// <param name="attribute">属性类型</param>
        /// <param name="target">执行方法对象，如果为空，则表示调用静态方法</param>
        /// <param name="parameters">调用方法参数</param>
        /// </summary>
        static public void ExecuteAttributeFunction(System.Type attributeType, object target, params object[] parameters)
        {
            int expectParamCount = parameters.IsNullOrEmpty() ? 0 : parameters.Length;
            var needExexcuteInfos = new System.Collections.Generic.List<ExecuteMethodInfo>();
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
                    //过滤接口和委托类
                    if (!type.IsClass || type.IsInterface)
                        continue;

                    //过滤不是对应方法名字的类
                    var bindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
                    bindingFlags |= null != target ? System.Reflection.BindingFlags.Instance : System.Reflection.BindingFlags.Static;

                    var allMethods = type.GetMethods(bindingFlags);

                    if (allMethods.IsNullOrEmpty())
                        continue;

                    for (int i = 0; i < allMethods.Length; ++i)
                    {
                        var selectMethodInfo = SelectMatchExecuteFunction(allMethods[i], type, attributeType, expectParamCount, target, parameters);
                        if (null != selectMethodInfo)
                        {
                            needExexcuteInfos.Add(selectMethodInfo);
                        }
                    }
                }
            }

            //对执行方法进行执行顺序排序
            needExexcuteInfos.Sort((value1, value2) =>
            {
                if (value1.attribute.callbackOrder < value2.attribute.callbackOrder) return -1;
                if (value1.attribute.callbackOrder > value2.attribute.callbackOrder) return 1;
                return 0;
            });

            for (int i = 0; i < needExexcuteInfos.Count; ++i)
            {
#if DEBUG_LOG
                var nowTimeTmp = System.DateTime.Now;
#endif
                var executeInfo = needExexcuteInfos[i];
                executeInfo.method.Invoke(target, parameters);
#if DEBUG_LOG
                var useTimeTmp = System.DateTime.Now - nowTimeTmp;
                if (null == target)
                    shaco.Base.Log.Info("ExecuteAttributeFunction static function: " + executeInfo.targetType.ToTypeString() + "." + executeInfo.method.Name + "\nuseTime=" + useTimeTmp.TotalMilliseconds);
                else
                    shaco.Base.Log.Info("ExecuteAttributeFunction instance function: " + target.ToTypeString() + "[" + executeInfo.targetType.ToTypeString() + "." + executeInfo.method.Name + "]\nuseTime=" + useTimeTmp.TotalMilliseconds);
#endif
            }
        }
        static public void ExecuteAttributeFunction<T>(object target, params object[] parameters)
        {
            ExecuteAttributeFunction(typeof(T), target, parameters);
        }

        /// <summary>
        /// 根据条件筛选出满足条件的执行函数
        /// <param name="methodInfo">函数数据</param>
        /// <param name="targetType">执行方法对象类型</param>
        /// <param name="attributeType">函数绑定的数量类型</param>
        /// <param name="expectParamCount">期望执行的函数参数数量</param>
        /// <param name="target">执行函数的对象，当是静态函数的时候该值为空</param>
        /// <param name="parameters">执行函数的参数，可以为空</param>
        /// <return>满足执行条件的函数</return>
        /// </summary>
        static private ExecuteMethodInfo SelectMatchExecuteFunction(System.Reflection.MethodInfo methodInfo, System.Type targetType, System.Type attributeType, int expectParamCount, object target, params object[] parameters)
        {
            ExecuteMethodInfo retValue = null;

            //过滤target为空，要求方法却不是static的类
            if (null == target && !methodInfo.IsStatic)
                return retValue;

            //过滤target类型与函数要求的target类型不一致的类
            if (null != target && targetType != target.GetType())
                return retValue;

            //只查找包含该属性的方法
            var attributeFind = methodInfo.GetCustomAttributes(attributeType, true);
            if (attributeFind.IsNullOrEmpty())
                return retValue;

            //过滤参数数量不一致的类
            var paramsFind = methodInfo.GetParameters();
            if (expectParamCount != paramsFind.Length)
            {
                Log.Error("Utility SelectMatchExecuteFunction erorr: has Inconsistent parameter count, attribute=" + attributeType+ " targetType=" + targetType
                          + "\nexpectParamCount=" + expectParamCount + " executeParaCount=" + paramsFind.Length);
                return retValue;
            }

            //二者都没有参数，直接匹配成功
            bool shouldExecute = false;
            if (0 == expectParamCount && paramsFind.IsNullOrEmpty())
                shouldExecute = true;
            else
            {
                //对比每个参数类型是否匹配
                shouldExecute = true;
                for (int i = 0; i < paramsFind.Length; ++i)
                {
                    if (!parameters[i].GetType().IsInherited(paramsFind[i].ParameterType))
                    {
                        shouldExecute = false;
                        Log.Error("Utility SelectMatchExecuteFunction erorr: has Inconsistent parameter type, attribute=" + attributeType + " targetType=" + targetType
                                  + "\nparamIndex=" + i + " expectType=" + parameters[i].GetType() + " executeType=" + paramsFind[i].ParameterType);
                        break;
                    }
                }
            }

            //检查完毕，以上条件均满足，则返回可执行方法
            if (shouldExecute)
            {
                retValue = new ExecuteMethodInfo()
                {
                    targetType = targetType,
                    method = methodInfo,
                    attribute = (shaco.Base.OrderCallBackAttribute)attributeFind[0]
                };
            }
            return retValue;
        }

        /// <summary>
        /// 获取随机数种子
        /// <return>随机数种子</return>
        /// </summary>
        static private Random GetRandomSeed()
        {
            long tick = DateTime.Now.Ticks;
            Random retValue = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
            return retValue;
        }

        /// <summary>
        /// 获取http请求拼接链接
        /// <param name="url">原地址</param>
        /// <param name="header">参数头</param>
        /// <return>拼接后的链接</return>
        /// </summary>
        static public string GetHttpRequestFullUrl(string url, params HttpComponent[] header)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            string ret = url;

            int indexFind = ret.LastIndexOf('?');
            if (indexFind < 0)
            {
                ret += '?';
                indexFind = ret.LastIndexOf('?') + 1;
            }
            else
            {
                indexFind = ret.Length;
            }

            int indexFindFlag = ret.LastIndexOf('?') + 1;
            for (int i = 0; i < header.Length; ++i)
            {
                int indexFindKey = ret.IndexOf(header[i].key, indexFindFlag);

                //delete old key
                if (indexFindKey > 0)
                {
                    int indexFindLast = ret.IndexOf("&", indexFindKey);
                    if (indexFindLast < 0)
                        indexFindLast = ret.Length;

                    int removeCount = indexFindLast - indexFindKey;
                    ret = ret.Remove(indexFindKey, removeCount);
                    indexFind = indexFindKey;
                }
                //insert new key
                else
                {

                    if (ret[ret.Length - 1] != '?')
                    {
                        ret += "&";
                        indexFind += 1;
                    }
                    indexFind = ret.Length;
                }

                var inserValue = header[i].key + "=" + header[i].value;
                ret = ret.Insert(indexFind, inserValue);

                indexFind += inserValue.Length;
            }

            return ret;
        }

        /// <summary>
        /// 获取下载速度格式化后的字符串
        /// <param name="speed">下载速度</param>
        /// <param name="decimals">保留小数点后几位</param>
        /// <return>格式化字符串</return>
        /// </summary>
        static public string GetSpeedFormatString(long speed, int decimals)
        {
            System.Text.StringBuilder ret = new System.Text.StringBuilder();
            double speedB = (double)speed;

            if (speedB > 1024.0)
            {
                if (speedB > 1024.0 * 1024.0)
                {
                    ret.Append((speedB / 1024.0 / 1024.0).Round(decimals) + "M/S");
                }
                else
                {
                    ret.Append((speedB / 1024.0).Round(0) + "KB/S");
                }
            }
            else
            {
                ret.Append(speedB.Round(0) + "B/S");
            }
            return ret.ToString();
        }

        /// <summary>
        /// 获取脚本命令变量
        /// <param name="key">命令key，要求格式如Key=Value</param>
        /// </summary>
        static public string GetEnviromentCommandValue(string key)
        {
            string retValue = null;
            var allArgs = System.Environment.GetCommandLineArgs();
            foreach (var value1 in allArgs)
            {
                if (value1.Contains(key))
                {
                    int indexFindValue = value1.IndexOf("=");
                    if (indexFindValue < 0)
                    {
                        Log.Error("Utility.GetEnviromentCommandValue error: not find '=' in command=" + value1);
                    }
                    else
                    {
                        //skip '=' flag
                        ++indexFindValue;
                        for (int i = indexFindValue; i < value1.Length; ++i)
                        {
                            char cValueTmp = value1[i];
                            if (cValueTmp != ' ')
                            {
                                indexFindValue = i;
                                break;
                            }
                        }
                        if (indexFindValue >= 0 && indexFindValue < value1.Length)
                        {
                            retValue = value1.Substring(indexFindValue, value1.Length - indexFindValue);
                        }
                    }
                    break;
                }
            }

            if (string.IsNullOrEmpty(retValue))
            {
                Log.Warning("Utility.GetEnviromentCommandValue warning: not find command value by key=" + key);
            }
            return retValue;
        }
    }
}
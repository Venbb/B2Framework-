using System.Collections;

namespace shaco.Base
{
    public class StackLocation
    {
#if DEBUG_LOG && UNITY_EDITOR
        private enum TimeCalculatePairCheckState
        {
            Start,
            End
        }

        private System.Diagnostics.StackTrace stackInformationTotal = null;
        private string stackInformationCurrent = string.Empty;
        private int stackLine = -1;
        private int numberOfCalls = 0;
        private double useMilliseconds = 0;
        private string currentTime = string.Empty;
        private System.DateTime _nowTime = System.DateTime.Now;
        private TimeCalculatePairCheckState _timeCalculatePairCheckState = TimeCalculatePairCheckState.End;
        private bool _isUseBeginSample = false;
#endif

        public string GetTotalStackInformation()
        {
#if DEBUG_LOG && UNITY_EDITOR
            return null != stackInformationTotal ? stackInformationTotal.GetFrames().ToContactString("\n") : "no stack trace";
#else
            return string.Empty;
#endif
        }

        public string GetStackInformation()
        {
#if DEBUG_LOG && UNITY_EDITOR
            return stackInformationCurrent;
#else
            return string.Empty;
#endif
        }

        public int GetStackLine()
        {
#if DEBUG_LOG && UNITY_EDITOR
            return stackLine;
#else
            return -1;
#endif
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        public void Reset()
        {
#if DEBUG_LOG && UNITY_EDITOR
            stackInformationCurrent = string.Empty;
            stackInformationTotal = null;
            stackLine = -1;
            numberOfCalls = 0;
            useMilliseconds = 0;
#endif
        }

        public bool HasStack()
        {
#if DEBUG_LOG && UNITY_EDITOR
            return !string.IsNullOrEmpty(stackInformationCurrent);
#else
            return false;
#endif
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        public void GetStack(params System.Type[] types)
        {
#if DEBUG_LOG && UNITY_EDITOR
            string[] classPaths = null;
            if (types.IsNullOrEmpty())
                classPaths = GlobalParams.DEFAULT_IGNORE_STACK_TAG;
            else 
            {
                classPaths = new string[types.Length];
                for (int i = 0; i < classPaths.Length; ++i)
                    classPaths[i] = types.GetType().FullName;
            }

            int indexstack = -1;
            for (int i = 0; i < classPaths.Length; ++i)
            {
                indexstack = FileHelper.FindLastStackLevelWhereCallLocated(classPaths[i]);
                if (indexstack >= 0)
                    break;
            }

            if (indexstack < 0)
            {
                indexstack = FileHelper.FindLastStackLevelWhereCallLocated(string.Empty);
            }

            ++numberOfCalls;

            var callAddEventstackTmp = FileHelper.GetPathWhereCallLocated(indexstack);
            stackInformationTotal = new System.Diagnostics.StackTrace(0, true);
            stackInformationCurrent = callAddEventstackTmp;
            stackLine = FileHelper.GetFileLineNumberWhereCallLocated(indexstack);
#endif
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        public void StartTimeSpanCalculate(string tag = shaco.Base.GlobalParams.EmptyString)
        {
#if DEBUG_LOG && UNITY_EDITOR
            if (_timeCalculatePairCheckState == TimeCalculatePairCheckState.Start)
            {
                Log.Error("StackLocation StartTimeSpanCalculate error: call 'StackLocation.StopTimeSpanCalculate' first");
                return;
            }
            _nowTime = System.DateTime.Now;
            _timeCalculatePairCheckState = TimeCalculatePairCheckState.Start;

            if (!string.IsNullOrEmpty(tag))
            {
                _isUseBeginSample = true;
                GameHelper.profiler.BeginSample(tag);
            }
#endif
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        public void StopTimeSpanCalculate()
        {
#if DEBUG_LOG && UNITY_EDITOR
            if (_timeCalculatePairCheckState == TimeCalculatePairCheckState.End)
            {
                Log.Error("StackLocation StopTimeSpanCalculate error: call 'StackLocation.StartTimeSpanCalculate' first");
                return;
            }

            var nowTime = System.DateTime.Now;
            var timeSpan = nowTime - _nowTime;
            useMilliseconds = timeSpan.TotalMilliseconds;
            currentTime = nowTime.ToString("HH:mm:ss");
            _timeCalculatePairCheckState = TimeCalculatePairCheckState.End;

            if (_isUseBeginSample)
            {
                _isUseBeginSample = false;
                GameHelper.profiler.EndSample();
            }
#endif
        }

        public double GetTimeSpan()
        {
#if DEBUG_LOG && UNITY_EDITOR
            return useMilliseconds;
#else
            return 0;
#endif
        }

        public string GetPerformanceDescription()
        {
#if DEBUG_LOG && UNITY_EDITOR
            var retValue = new System.Text.StringBuilder();
            if (numberOfCalls > 0)
            {
                retValue.Append(": ");
                retValue.Append(numberOfCalls);
                retValue.Append("(times)");
            }
            if (useMilliseconds > 0)
            {
                retValue.Append(useMilliseconds);
                retValue.Append("(ms)");
                retValue.Append(currentTime);
            }
            return retValue.ToString();
#else
            return string.Empty;
#endif
        }

        public StackLocation Clone()
        {
#if DEBUG_LOG && UNITY_EDITOR
            StackLocation retValue = new StackLocation();
            retValue.stackInformationCurrent = this.stackInformationCurrent;
            retValue.stackInformationTotal = this.stackInformationTotal;
            retValue.stackLine = this.stackLine;
            retValue.numberOfCalls = this.numberOfCalls;
            retValue.useMilliseconds = this.useMilliseconds;
            return retValue;
#else
            return new StackLocation();
#endif
        }
    }
}


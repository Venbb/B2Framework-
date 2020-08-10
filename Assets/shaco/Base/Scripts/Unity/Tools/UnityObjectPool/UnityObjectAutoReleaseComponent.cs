using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace shaco
{
    public class UnityObjectAutoReleaseComponent : MonoBehaviour
    {
        public class AutoReleaseInfo
        {
            public class CallbackInfo
            {
                public System.Action callback = null;
                public shaco.Base.StackLocation stackLocationBind = new shaco.Base.StackLocation();
            }
            public object target;
            public List<CallbackInfo> callbacks = new List<CallbackInfo>();
        }

        public ICollection<AutoReleaseInfo> willAutoReleaseInfos { get { return _onDestroyCallBack.Values; } }

        private Dictionary<object, AutoReleaseInfo> _onDestroyCallBack = new Dictionary<object, AutoReleaseInfo>();

        //绑定销毁事件
        public void AddOnDestroyCallBack(object target, System.Action callback)
        {
            if (null == target)
                return;

            AutoReleaseInfo findValue = null;
            if (!_onDestroyCallBack.TryGetValue(target, out findValue))
            {
                findValue = new AutoReleaseInfo();
                findValue.target = target;
                _onDestroyCallBack.Add(target, findValue);
            }

            var newCallBackInfo = new AutoReleaseInfo.CallbackInfo();
            newCallBackInfo.callback = callback;
            newCallBackInfo.stackLocationBind.GetStack();
            findValue.callbacks.Add(newCallBackInfo);
        }

        //移除销毁事件
        public void RemoveOnDestroyCallBacks(object target)
        {
            if (!_onDestroyCallBack.ContainsKey(target))
            {
                shaco.Log.Error("UnityObjectAutoReleaseComponent RemoveOnDestroyCallBacks error: not found target=" + target, this);
                return;
            }
            _onDestroyCallBack.Remove(target);
            CheckAutoDestroyMeWhenCallBackEmpty();
        }

        //移除销毁事件
        public void RemoveOnDestroyCallBack(object target, System.Action callback)
        {
            AutoReleaseInfo findValue = null;
            if (!_onDestroyCallBack.TryGetValue(target, out findValue))
            {
                shaco.Log.Error("UnityObjectAutoReleaseComponent RemoveOnDestroyCallBack error: not found 1 target=" + target + " callback=" + callback, this);
                return;
            }

            int countCheck = findValue.callbacks.Count;
            findValue.callbacks.RemoveOne(v => v.callback == callback);
            if (countCheck == findValue.callbacks.Count)
            {
                shaco.Log.Error("UnityObjectAutoReleaseComponent RemoveOnDestroyCallBack error: not found 2 target=" + target + " callback=" + callback, this);
                return;
            }

            if (findValue.callbacks.Count == 0)
            {
                _onDestroyCallBack.Remove(target);
                CheckAutoDestroyMeWhenCallBackEmpty();
            }
        }

        //清空销毁事件
        public void ClearOnDestroyCallBack()
        {
            _onDestroyCallBack.Clear();
        }

        void OnApplicationQuit()
        {
            _onDestroyCallBack.Clear();
        }

        void OnDestroy()
        {
            if (!_onDestroyCallBack.IsNullOrEmpty())
            {
                foreach (var iter in _onDestroyCallBack)
                {
                    for (int i = 0; i < iter.Value.callbacks.Count; ++i)
                    {
                        try
                        {
                            iter.Value.callbacks[i].callback();
                        }
                        catch (System.Exception e)
                        {
                            var callbackTmp = iter.Value.callbacks[i].callback;
                            Log.Error("UnityObjectAutoReleaseComponent OnDestroy error: Target=" + callbackTmp.Target + " Method=" + callbackTmp.Method + " e=" + e, this);
                        }
                    }
                }
                _onDestroyCallBack.Clear();
            }
        }

        private void CheckAutoDestroyMeWhenCallBackEmpty()
        {
            //如果当前组件没有需要绑定的对象则销毁自己
            if (_onDestroyCallBack.IsNullOrEmpty())
            {
                MonoBehaviour.Destroy(this);
            }
        }
    }
}
using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace shaco.Test
{
    public class TestEvent : MonoBehaviour
    {
        public GameObject eventTarget1;
        public int eventTarget2 = 100;

        private shaco.Base.EventCallBack _eventCallBack = new shaco.Base.EventCallBack();
        private shaco.Base.EventCallBack<int> _eventCallBackArg = new shaco.Base.EventCallBack<int>();

        void AddEventCallBack()
        {
            _eventCallBack.ClearCallBack();
            _eventCallBackArg.ClearCallBack();

            //强烈推荐使用该方式来添加事件
            //因为它可以用任意Unity实例化作为事件绑定对象，当该对象被销毁时候，它的关联事件也会自动销毁
            eventTarget1.AddAutoRealeaseEvent<TestArg2>(callfunc2_2);

            //如果事件绑定对象非Unity实例化对象，推荐使用该方法绑定事件
            this.AddEvent<TestArg2>(callfunc2);

            //其他事件绑定方法，仅作参考
            int tmpEventTarget = 2;
            shaco.GameHelper.Event.AddEvent<TestArg>(tmpEventTarget, callfunc1, true);
            _eventCallBack.AddCallBack(this, callfunc3);
            _eventCallBackArg.AddCallBack(this, callfunc4);
        }

        void OnGUI()
        {
            if (TestMainMenu.DrawButton("AddCallBack"))
            {
                AddEventCallBack();
            }

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("invoke 1"))
                {
                    var argTmp = new TestArg();
                    this.InvokeEvent(argTmp);
                }

                if (TestMainMenu.DrawButton("invoke 2"))
                {
                    shaco.GameHelper.Event.InvokeEvent(new TestArg2());
                }

                if (TestMainMenu.DrawButton("invoke sequeue event"))
                {
                    this.InvokeSequeueEvent(do1(), do2());
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("remove"))
                {
                    eventTarget1.RemoveEvent<TestArg2>();
                }

                if (TestMainMenu.DrawButton("remove with id"))
                {
                    shaco.GameHelper.Event.RemoveEvent<TestArg2>();
                }

                if (TestMainMenu.DrawButton("remove with sender"))
                {
                    eventTarget1.RemoveAllEvent();
                }

                if (TestMainMenu.DrawButton("remove callback1"))
                {
                    shaco.GameHelper.Event.RemoveEvent<TestArg>(callfunc1);
                }

                if (TestMainMenu.DrawButton("remove callback2"))
                {
                    shaco.GameHelper.Event.RemoveEvent<TestArg2>(callfunc2);
                }
            }
            GUILayout.EndHorizontal();

#if UNITY_EDITOR && DEBUG_LOG
            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("location1"))
                {
                    if (_eventCallBack.Count > 0)
                    {
                        var path = _eventCallBack[0].CallAddEventStack.GetStackInformation();
                        var line = _eventCallBack[0].CallAddEventStack.GetStackLine();
                        var indexTmp = path.IndexOf("Assets/");
                        if (indexTmp >= 0)
                            path = path.Substring(indexTmp, path.Length - indexTmp);
                        UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), line);

                    }
                }

                if (TestMainMenu.DrawButton("location2"))
                {
                    if (_eventCallBackArg.Count > 0)
                    {
                        var path = _eventCallBackArg[0].CallAddEventStack.GetStackInformation();
                        var line = _eventCallBackArg[0].CallAddEventStack.GetStackLine();
                        var indexTmp = path.IndexOf("Assets/");
                        if (indexTmp >= 0)
                            path = path.Substring(indexTmp, path.Length - indexTmp);
                        UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), line);
                    }
                }
            }
            GUILayout.EndHorizontal();
#endif
            if (TestMainMenu.DrawButton("destroy target"))
            {
                Destroy(eventTarget1.gameObject);
            }

            TestMainMenu.DrawBackToMainMenuButton();
        }

        void callfunc1(object sender, shaco.Base.BaseEventArg arg)
        {
            if (arg as TestArg != null)
            {
                shaco.Log.Info("1 event id=" + arg.eventID + " message=" + (arg as TestArg).message + " target=" + this);
            }
            else
            {
                shaco.Log.Info("1 event id=" + arg.eventID + " message=" + (arg as TestArg2).message + " target=" + this);
            }

            shaco.GameHelper.Event.RemoveEvent<TestArg>();

            shaco.GameHelper.Event.AddEvent<TestArg>(eventTarget2, callfunc1, false);
            shaco.GameHelper.Event.RemoveAllEvent(eventTarget2);

            float b = 3;
            shaco.GameHelper.Event.AddEvent<TestArg>(b, callfunc1, false);

            shaco.GameHelper.Event.AddEvent<TestArg>(this, callfunc1, false);
            this.RemoveAllEvent();
        }

        void callfunc2(object sender, shaco.Base.BaseEventArg arg)
        {
            shaco.Log.Info("2 event id=" + arg.eventID + " message=" + (arg as TestArg2).message + " target=" + this);
        }

        void callfunc2_2(object sender, shaco.Base.BaseEventArg arg)
        {
            shaco.Log.Info("2_2 event id=" + arg.eventID + " message=" + (arg as TestArg2).message + " target=" + this);
        }

        void callfunc3(object sender)
        {
            shaco.Log.Info("3 sender=" + sender);
        }

        void callfunc4(object sender, int arg)
        {
            shaco.Log.Info("4 sender=" + sender + " arg=" + arg);
        }

        IEnumerator do1()
        {
            yield return new WaitForSeconds(1.0f);
            Debug.Log("1111");
        }

        IEnumerator do2()
        {
            yield return new WaitForSeconds(3.0f);
            Debug.Log("2222");
        }
    }
}
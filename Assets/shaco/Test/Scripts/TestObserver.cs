using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Test
{
    public class TestObserver : MonoBehaviour
    {
        /// <summary>
        /// 自定义的数据主体
        /// </summary>
        public class CustomSubject<T> : shaco.Base.Subject<T>
        {
            public override void Notify(object oldValue, object newValue)
            {
                Debug.Log("CustomSubject Notify.....");
                base.Notify(oldValue, newValue);
            }
        }

        /// <summary>
        /// 自定义数据观察对象
        /// </summary>
        public class CustomObserver<T> : shaco.Base.Observer<T>
        {
            public override void OnUpdateCallBack()
            {
                base.OnUpdateCallBack();
                Debug.Log("CustomObserver OnUpdateCallBack.....");
            }
        }

        public class TestBindTarget
        {
            public shaco.Base.ISubject<float> data2 = new CustomSubject<float>();
        }

        public shaco.Base.ISubject<int> data1 = new shaco.Base.Subject<int>();
        public shaco.Base.ISubject<bool> data3 = new shaco.Base.Subject<bool>();
        public TestBindTarget testBindTarget = new TestBindTarget();
        public shaco.Base.Subject<TestBindTarget> subjectTest4 = new shaco.Base.Subject<TestBindTarget>();

        
        private shaco.Base.IObserver<int> _data1Observer = null;

        void Start()
        {
            //支持新旧数据观察
            _data1Observer = data1.OnValueUpdateFromTo((int oldValue, int value) =>
            {
                Debug.Log("update 1 data1=" + value + " oldValue=" + oldValue);
            }).OnValueInit((int value) =>
            {
                Debug.Log("init 1 data1=" + value);
            }).Start(this);

            //支持新数据对象观察 [大部分情况下该方法应该最常用]
            data1.OnValueUpdate(value =>
            {
                Debug.Log("update 2 data1=" + value);
            });

            //支持旧数据和新数据对象观察
            data1.OnSubjectValueUpdate((oldValue, subject) =>
            {
                Debug.Log("update 3 data1=" + subject.value + " oldValue=" + oldValue);
            }).Start(this);

            data3.OnSubjectValueUpdate(subject => Debug.Log("data3=" + subject.value)).Start(this);

            //复合嵌套的观察类型
            subjectTest4.OnValueUpdate(v => 
            {
                v.data2.OnValueUpdate(v2 => 
                {
                    Debug.Log("update date 4");
                }).Start(this);
            }).Start(this);
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("Change data 1"))
                {
                    data1.value = shaco.Base.Utility.Random();
                }

                if (TestMainMenu.DrawButton("Change data 2"))
                {
                    testBindTarget.data2.value = shaco.Base.Utility.Random();
                }

                if (TestMainMenu.DrawButton("Change data 3"))
                {
                    data3.value = shaco.Base.Utility.Random(0, 2) == 0 ? false : true;
                }

                if (TestMainMenu.DrawButton("Change data 4"))
                {
                    subjectTest4.value = new TestBindTarget();
                }
            }
            GUILayout.EndHorizontal();

            if (TestMainMenu.DrawButton("Subject end"))
            {
                Debug.Log("Subject end");
                testBindTarget.data2.End();
            }

            if (TestMainMenu.DrawButton("Observer end"))
            {
                Debug.Log("observer end result=" + _data1Observer.End());
            }

            if (TestMainMenu.DrawButton("Destroy and auto release"))
            {
                MonoBehaviour.Destroy(this.gameObject);
            }

            TestMainMenu.DrawBackToMainMenuButton();
        }
    }
}
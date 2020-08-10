using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using XLua;

namespace shaco.Test
{
    public class TestClass
    {
        public Text testValue;
        public int TestFunction(Vector3 value)
        {
            return 100;
        }
    }

    public class TestLuaHotFix : MonoBehaviour
    {
        public Text TextLog;
        public System.Func<string> getlogcallback = null;
        public System.Collections.Generic.List<int> ListTest;

        private LuaEnv luaenv = new LuaEnv();   

        void Start()
        {
            //默认实例化对象，防止计算效率时间第一次出现偏差
            shaco.Base.GameEntry.GetInstance<shaco.XLuaManager>();
        }

        void Update()
        {
            
        }

        int TestFunction(int a, int b)
        {
            return 100;
        }

        int TestFunction2(Vector3 value)
        {
            Debug.Log("call c# TestFunction2");
            return 100;
        }

        public string getLog()
        {
            return getlogcallback();
        }

        public IEnumerator LoopCallBack()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);
                Debug.Log("----------unity LoopCallBack---------");
            }
        }

        public struct Param1//结构体参数
        {
            public int x;
            public string y;
        }

        public double ComplexFunc(Param1 p1, ref int p2, out string p3, System.Action luafunc, out System.Action csfunc)
        {
            Debug.Log("P1 = {x=" + p1.x + ",y=" + p1.y + "},p2 = " + p2);
            luafunc();
            p2 = p2 * p1.x;
            p3 = "hello " + p1.y;
            csfunc = () =>
            {
                Debug.Log("csharp callback invoked!");
            };
            return 1.23;
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("RunWithFile"))
                {
                    shaco.XLuaManager.RunWithFile("lua_example/TestLuaHotFix" + shaco.Base.GlobalParams.LUA_FILE_EXTENSIONS);
                    getlogcallback += () =>
                    {
                        return "this a unity c# log";
                    };
                    TextLog.text = "log result =" + getLog();
                    Debug.Log(TextLog.text);
                    
                    //注意：方法需要放到HotFix后才执行，否则还是没Fix的方法
                    StopCoroutine(LoopCallBack());
                    StartCoroutine(LoopCallBack());
                }

                if (TestMainMenu.DrawButton("RunWithFolder"))
                {
                    shaco.XLuaManager.RunWithFolder("lua_example", ()=>
                    {
                        Debug.Log("read lua folder end......");
                    }, (float percent) =>
                    {
                        Debug.Log("load progress percent=" + percent);
                    });
                }
            }
            GUILayout.EndHorizontal();

            if (TestMainMenu.DrawButton("c# call lua function"))
            {
                string script = @"
                    function TestFunction()
                        return 100;
                    end
                    for i = 0, 10000 do
                        TestFunction()
                    end
                    ";
                var nowTime = System.DateTime.Now;
                luaenv.DoString(script);
                TextLog.text = "usetime=" + (System.DateTime.Now - nowTime).TotalMilliseconds;
            }

            if (TestMainMenu.DrawButton("lua call c# function"))
            {
                string script = @"
                    local testClass = CS.shaco.Test.TestClass()
                    for i = 0, 10000 do
                        testClass:TestFunction(CS.UnityEngine.Vector3.zero)
                    end
                    ";
                var nowTime = System.DateTime.Now;
                luaenv.DoString(script);
                TextLog.text = "usetime=" + (System.DateTime.Now - nowTime).TotalMilliseconds;
            }

            if (TestMainMenu.DrawButton("call c# function"))
            {
                var nowTime = System.DateTime.Now;
                var testClass = new TestClass();
                for (int i = 0; i < 10000; ++i)
                {
                    testClass.TestFunction(Vector3.zero);
                }
                TextLog.text = "usetime=" + (System.DateTime.Now - nowTime).TotalMilliseconds;
            }

            if (TestMainMenu.DrawButton("lua call c# parameter"))
            {
                string script = @"
                    local testClass = CS.shaco.Test.TestClass()
                    for i = 0, 10000 do
                        local v = testClass.testValue
                    end 
                    ";
                var nowTime = System.DateTime.Now;
                luaenv.DoString(script);
                TextLog.text = "usetime=" + (System.DateTime.Now - nowTime).TotalMilliseconds;
            }

            if (TestMainMenu.DrawButton("call c# parameter"))
            {
                var testClass = new TestClass();
                var nowTime = System.DateTime.Now;
                for (int i = 0; i < 10000; ++i)
                {
                    var v = testClass.testValue;
                    v = null;
                }
                TextLog.text = "usetime=" + (System.DateTime.Now - nowTime).TotalMilliseconds;
            }

            if (TestMainMenu.DrawButton("call lua function"))
            {
                string script = @"
                    function TestFunction() end
                    for i = 0, 10000 do
                        TestFunction()
                    end
                    ";
                var nowTime = System.DateTime.Now;
                luaenv.DoString(script);
                TextLog.text = "usetime=" + (System.DateTime.Now - nowTime).TotalMilliseconds;
            }

            if (TestMainMenu.DrawButton("call lua parameter"))
            {
                string script = @"
                    local testValue = {}
                    for i = 0, 10000 do
                        local v = testValue
                    end
                    ";
                var nowTime = System.DateTime.Now;
                luaenv.DoString(script);
                TextLog.text = "usetime=" + (System.DateTime.Now - nowTime).TotalMilliseconds;
            }

            TestMainMenu.DrawBackToMainMenuButton();
        }
    }
}
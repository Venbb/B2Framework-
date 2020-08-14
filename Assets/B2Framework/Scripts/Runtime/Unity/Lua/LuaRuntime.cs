using System;
using UnityEngine;
using XLua;

namespace B2Framework.Unity
{
    /// <summary>
    /// Lua全局的生命周期
    /// </summary>
    public class LuaRuntime : MonoBehaviour
    {
        protected LuaTable scriptEnv;
        protected LuaTable metatable;
        Action<LuaTable, float, float> onUpdate = null;
        Action<LuaTable> onLateUpdate = null;
        Action<LuaTable, float> onFixedUpdate = null;
        Action<LuaTable, string, string> onNotify = null;

        public void Run(string path)
        {
            if (scriptEnv != null) Dispose();

            var luaMgr = LuaManager.Instance;
            scriptEnv = luaMgr.CreateTable();

            // scriptEnv.Set("self", this);

            object[] result = luaMgr.LoadScript(path, path, scriptEnv);
            metatable = (LuaTable)result[0];
            // foreach (var itme in scriptEnv.GetKeys())//具体映射到的所有内容
            // {
            //     var all = scriptEnv.Get<object>(itme);
            //     Debug.Log(itme + " = " + all);
            // }
            metatable.Get("Update", out onUpdate);
            metatable.Get("LateUpdate", out onLateUpdate);
            metatable.Get("FixedUpdate", out onFixedUpdate);
            metatable.Get("Notify", out onNotify);
        }
        /// <summary>
        /// 向Lua同步发送数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Send(string key, string value)
        {
            if (onNotify != null)
            {
                try
                {
                    onNotify(metatable, key, value);
                }
                catch (Exception ex)
                {
                    Log.Error("onNotify err : " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        void Update()
        {
            if (onUpdate != null)
            {
                try
                {
                    onUpdate(metatable, Time.deltaTime, Time.unscaledDeltaTime);
                }
                catch (Exception ex)
                {
                    Log.Error("onUpdate err : " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }
        void LateUpdate()
        {
            if (onLateUpdate != null)
            {
                try
                {
                    onLateUpdate(metatable);
                }
                catch (Exception ex)
                {
                    Log.Error("onLateUpdate err : " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }
        void FixedUpdate()
        {
            if (onFixedUpdate != null)
            {
                try
                {
                    onFixedUpdate(metatable, Time.fixedDeltaTime);
                }
                catch (Exception ex)
                {
                    Log.Error("onFixedUpdate err : " + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        public void Dispose()
        {
            onUpdate = null;
            onLateUpdate = null;
            onFixedUpdate = null;
            onNotify = null;

            if (scriptEnv != null)
            {
                scriptEnv.Dispose();
                scriptEnv = null;
            }
        }
        void OnDestroy()
        {
            Dispose();
        }
    }
}
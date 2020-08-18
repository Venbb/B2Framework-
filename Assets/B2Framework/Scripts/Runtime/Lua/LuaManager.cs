using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace B2Framework
{
    public class LuaManager : MonoSingleton<LuaManager>, IDisposable
    {
        private List<LuaBehaviour> luaBehaviours = new List<LuaBehaviour>();
        public void Register(LuaBehaviour behaviour)
        {
            luaBehaviours.Add(behaviour);
        }
        public void Unregister(LuaBehaviour behaviour)
        {
            luaBehaviours.Remove(behaviour);
        }
        // Lua 环境，全局唯一
        public LuaEnv luaEnv { get; private set; }
        private float interval = 2;
        public float Interval
        {
            get { return interval; }
            set
            {
                if (interval <= 0) return;

                interval = value;
                wait = new WaitForSeconds(interval);
            }
        }
        private WaitForSeconds wait;
        public LuaRuntime luaRuntime { get; private set; }
        public LuaCoroutine luaCoroutine { get; private set; }
        protected override void Awake()
        {
            base.Awake();
            InitLuaEnv();
        }
        public LuaManager Initialize()
        {
            if (luaEnv != null)
            {
                LoadScript("Comm.Main");

                luaRuntime = gameObject.GetComponent<LuaRuntime>();
                if (luaRuntime == null)
                {
                    luaRuntime = gameObject.AddComponent<LuaRuntime>();
                }

                luaCoroutine = gameObject.GetComponent<LuaCoroutine>();
                if (luaCoroutine == null)
                {
                    luaCoroutine = gameObject.AddComponent<LuaCoroutine>();
                }
            }
            return this;
        }
        /// <summary>
        /// 初始化LuaEnv
        /// </summary>
        private void InitLuaEnv()
        {
            if (luaEnv == null)
            {
                luaEnv = new LuaEnv();
                luaEnv.AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);
                luaEnv.AddBuildin("lpeg", XLua.LuaDLL.Lua.LoadLpeg);
                luaEnv.AddBuildin("pb", XLua.LuaDLL.Lua.LoadLuaProfobuf);

                luaEnv.AddLoader(CustomLoader);

                wait = new WaitForSeconds(interval);
                StartCoroutine(DoTick());
            }
        }
        private static byte[] CustomLoader(ref string filepath)
        {
            string scriptPath = string.Empty;
            filepath = filepath.Replace(".", "/");

            if (!GameUtility.Assets.runtimeMode)
            {
                scriptPath = Utility.Path.Combine(GameConst.LUA_SCRIPTS_PATH, filepath) + ".lua"; ;

                return Utility.Files.ReadAllBytes(scriptPath);
            }

            scriptPath = GameUtility.Assets.GetAssetPath(AssetBundles.Lua, filepath) + GameConst.LUA_EXTENSION;
            var request = AssetsManger.LoadAsset(scriptPath, typeof(TextAsset));
            if (!string.IsNullOrEmpty(request.error))
            {
                Log.Error(request.error);
                return null;
            }
            request.Release();
            var text = (request.asset as TextAsset).text;
            return System.Text.Encoding.UTF8.GetBytes(text);
        }
        private IEnumerator DoTick()
        {
            while (true)
            {
                yield return wait;
                try
                {
                    luaEnv.Tick();
                    if (Time.frameCount % 100 == 0)
                    {
                        luaEnv.FullGc();
                    }
                }
                catch (Exception e)
                {
                    Log.Warning("LuaEnv.Tick Error:{0}", e.Message);
                }
            }
        }
        /// <summary>
        /// 创建一个LuaTable
        /// </summary>
        /// <returns></returns>
        public LuaTable CreateTable()
        {
            var table = luaEnv.NewTable();

            // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
            var meta = luaEnv.NewTable();
            meta.Set("__index", luaEnv.Global);
            table.SetMetaTable(meta);
            meta.Dispose();

            return table;
        }
        /// <summary>
        /// 加载并执行脚本
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object[] LoadScript(string name, string chunkName = "chunk", LuaTable env = null)
        {
            return DoString(Utility.Text.Format("return require('{0}')", name), chunkName, env);
        }
        /// <summary>
        /// 执行Lua 脚本
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="chunkName"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public object[] DoString(string chunk, string chunkName = "chunk", LuaTable env = null)
        {
            if (luaEnv != null)
            {
                try
                {
                    return luaEnv.DoString(chunk, chunkName, env);
                }
                catch (System.Exception ex)
                {
                    string msg = Utility.Text.Format("xLua exception : {0}\n {1}", ex.Message, ex.StackTrace);
                    Log.Error(msg);
                }
            }
            return null;
        }
        /// <summary>
        /// 重启Lua
        /// </summary>
        public void Restart()
        {
            Dispose();
            InitLuaEnv();
            Initialize();
        }
        /// <summary>
        /// 进入游戏，开启Lua的游戏逻辑
        /// </summary>
        public void StartGame()
        {
            if (luaEnv != null && luaRuntime != null)
            {
                luaRuntime.Run("Game.GameLogic");
            }
        }
        /// <summary>
        /// 给Lua发送数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="msg"></param>
        public void Send(string key, string msg)
        {
            if (luaEnv != null && luaRuntime != null)
            {
                luaRuntime.Send(key, msg);
            }
        }
        public void Dispose()
        {
            Log.Debug("LuaManager:Dispose");

            foreach (var behaviour in luaBehaviours) behaviour?.Dispose();

            luaRuntime?.Dispose();
            luaCoroutine?.Dispose();

            StopAllCoroutines();
            DoString("require 'xlua.util' .print_func_ref_by_csharp()");
            if (luaEnv != null)
            {
                try
                {
                    luaEnv.customLoaders.Clear();
                    luaEnv.Dispose();
                    luaEnv = null;
                }
                catch (System.Exception ex)
                {
                    string msg = Utility.Text.Format("xLua exception : {0}\n {1}", ex.Message, ex.StackTrace);
                    Log.Error(msg);
                }
            }
            wait = null;
        }
    }
}
using System;
using UnityEngine;
using XLua;

namespace B2Framework.Unity
{
    [RequireComponent(typeof(CanvasGroup))]
    public class LuaBehaviour : MonoBehaviour
    {
        public LuaScriptBinder luaScript;
        public VariableList variables;

        protected LuaTable scriptEnv;
        public LuaTable metatable;
        protected Action<LuaTable> onAwake;
        protected Action<LuaTable> onEnable;
        protected Action<LuaTable> onDisable;
        protected Action<LuaTable> onStart;
        protected Action<LuaTable> onUpdate;
        protected Action<LuaTable> onDispose;
        protected Action<LuaTable> onDestroy;

        LuaManager luaMgr;
        protected virtual void Initialize()
        {
            luaMgr = The.LuaMgr;
            // scriptEnv = luaMgr.CreateTable();

            // // scriptEnv.Set("self", this);

            // object[] result;
            // if (luaScript.Type != LuaScriptBindEnum.Filename)
            //    result = luaMgr.DoString(luaScript.Text.text, luaScript.Text.name, scriptEnv);
            // else
            //    result = luaMgr.LoadScript(luaScript.Filename, luaScript.Filename, scriptEnv);

            // if (result.Length != 1 || !(result[0] is LuaTable)) throw new Exception("");

            // metatable = (LuaTable)result[0];
            // metatable.Set("transform", transform);
            // metatable.Set("gameObject", gameObject);
            if (variables != null && variables.Variables != null)
            {
                foreach (var variable in variables.Variables)
                {
                    var name = variable.Name.Trim();
                    if (string.IsNullOrEmpty(name))
                        continue;

                    metatable.Set(name, variable.GetValue());
                }
            }
            // foreach (var itme in metatable.GetKeys())//具体映射到的所有内容
            // {
            //     var all = metatable.Get<object>(itme);
            //     Debug.Log(itme + " = " + all);
            // }
            metatable.Get("Awake", out onAwake);
            metatable.Get("OnEnable", out onEnable);
            metatable.Get("Start", out onStart);
            metatable.Get("Update", out onUpdate);
            metatable.Get("OnDisable", out onDisable);
            metatable.Get("OnDispose", out onDispose);
            metatable.Get("OnDestroy", out onDestroy);
        }
        protected virtual void Awake()
        {
            this.Initialize();
            if (onAwake != null) onAwake(metatable);
        }
        protected virtual void OnEnable()
        {
            if (onEnable != null) onEnable(metatable);
        }
        protected virtual void Start()
        {
            luaMgr?.Register(this);
            if (onStart != null) onStart(metatable);
        }
        protected virtual void Update()
        {
            if (onUpdate != null) onUpdate(metatable);
        }
        protected virtual void OnDisable()
        {
            if (onDisable != null) onDisable(metatable);
        }
        protected virtual void OnDestroy()
        {
            Log.Debug("LuaBehaviour:OnDestroy");
            // Dispose();
            if (onDestroy != null) onDestroy(metatable);
            onDestroy = null;
            onDispose = null;
            onDisable = null;
            onUpdate = null;
            onStart = null;
            onEnable = null;
            onAwake = null;

            if (metatable != null)
            {
                metatable.Dispose();
                metatable = null;
            }

            if (scriptEnv != null)
            {
                scriptEnv.Dispose();
                scriptEnv = null;
            }
            luaMgr?.Unregister(this);
            luaMgr = null;
        }
        public virtual void Dispose()
        {
            if (onDispose != null) onDispose(metatable);
        }
    }
}
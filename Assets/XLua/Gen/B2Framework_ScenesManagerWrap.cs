#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class B2FrameworkScenesManagerWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(B2Framework.ScenesManager);
			Utils.BeginObjectRegister(type, L, translator, 0, 3, 4, 2);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "LoadSceneAsync", _m_LoadSceneAsync);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "UnloadSceneAsync", _m_UnloadSceneAsync);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Dispose", _m_Dispose);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "progress", _g_get_progress);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "isDone", _g_get_isDone);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "OnProgress", _g_get_OnProgress);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "OnCompleted", _g_get_OnCompleted);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "OnProgress", _s_set_OnProgress);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "OnCompleted", _s_set_OnCompleted);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					var gen_ret = new B2Framework.ScenesManager();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to B2Framework.ScenesManager constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadSceneAsync(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.ScenesManager gen_to_be_invoked = (B2Framework.ScenesManager)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)) 
                {
                    string _sceneName = LuaAPI.lua_tostring(L, 2);
                    bool _additive = LuaAPI.lua_toboolean(L, 3);
                    
                        var gen_ret = gen_to_be_invoked.LoadSceneAsync( _sceneName, _additive );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)) 
                {
                    string _sceneName = LuaAPI.lua_tostring(L, 2);
                    
                        var gen_ret = gen_to_be_invoked.LoadSceneAsync( _sceneName );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to B2Framework.ScenesManager.LoadSceneAsync!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UnloadSceneAsync(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.ScenesManager gen_to_be_invoked = (B2Framework.ScenesManager)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _sceneName = LuaAPI.lua_tostring(L, 2);
                    
                    gen_to_be_invoked.UnloadSceneAsync( _sceneName );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Dispose(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.ScenesManager gen_to_be_invoked = (B2Framework.ScenesManager)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Dispose(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_progress(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.ScenesManager gen_to_be_invoked = (B2Framework.ScenesManager)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.progress);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_isDone(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.ScenesManager gen_to_be_invoked = (B2Framework.ScenesManager)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.isDone);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_OnProgress(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.ScenesManager gen_to_be_invoked = (B2Framework.ScenesManager)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.OnProgress);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_OnCompleted(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.ScenesManager gen_to_be_invoked = (B2Framework.ScenesManager)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.OnCompleted);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_OnProgress(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.ScenesManager gen_to_be_invoked = (B2Framework.ScenesManager)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.OnProgress = translator.GetDelegate<System.Action<float>>(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_OnCompleted(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.ScenesManager gen_to_be_invoked = (B2Framework.ScenesManager)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.OnCompleted = translator.GetDelegate<System.Action<string>>(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

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
    public class B2FrameworkUtilityConverterWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(B2Framework.Utility.Converter);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 0, 0);
			
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 11, 0, 0);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "GetString", _m_GetString_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ToBoolean", _m_ToBoolean_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ToSingle", _m_ToSingle_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ToInt32", _m_ToInt32_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ToString", _m_ToString_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ToColor", _m_ToColor_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ToVector2", _m_ToVector2_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ToVector3", _m_ToVector3_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ToVector4", _m_ToVector4_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ToRect", _m_ToRect_xlua_st_);
            
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            return LuaAPI.luaL_error(L, "B2Framework.Utility.Converter does not have a constructor!");
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetString_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 1)) 
                {
                    bool _value = LuaAPI.lua_toboolean(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.GetString( _value );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    float _value = (float)LuaAPI.lua_tonumber(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.GetString( _value );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    int _value = LuaAPI.xlua_tointeger(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.GetString( _value );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)) 
                {
                    string _value = LuaAPI.lua_tostring(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.GetString( _value );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& translator.Assignable<UnityEngine.Color>(L, 1)) 
                {
                    UnityEngine.Color _value;translator.Get(L, 1, out _value);
                    
                        var gen_ret = B2Framework.Utility.Converter.GetString( _value );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& translator.Assignable<UnityEngine.Vector2>(L, 1)) 
                {
                    UnityEngine.Vector2 _value;translator.Get(L, 1, out _value);
                    
                        var gen_ret = B2Framework.Utility.Converter.GetString( _value );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& translator.Assignable<UnityEngine.Vector3>(L, 1)) 
                {
                    UnityEngine.Vector3 _value;translator.Get(L, 1, out _value);
                    
                        var gen_ret = B2Framework.Utility.Converter.GetString( _value );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& translator.Assignable<UnityEngine.Vector4>(L, 1)) 
                {
                    UnityEngine.Vector4 _value;translator.Get(L, 1, out _value);
                    
                        var gen_ret = B2Framework.Utility.Converter.GetString( _value );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& translator.Assignable<UnityEngine.Rect>(L, 1)) 
                {
                    UnityEngine.Rect _value;translator.Get(L, 1, out _value);
                    
                        var gen_ret = B2Framework.Utility.Converter.GetString( _value );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to B2Framework.Utility.Converter.GetString!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToBoolean_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _value = LuaAPI.lua_tostring(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.ToBoolean( _value );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToSingle_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _value = LuaAPI.lua_tostring(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.ToSingle( _value );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToInt32_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _value = LuaAPI.lua_tostring(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.ToInt32( _value );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToString_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _value = LuaAPI.lua_tostring(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.ToString( _value );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToColor_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _value = LuaAPI.lua_tostring(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.ToColor( _value );
                        translator.PushUnityEngineColor(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToVector2_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _value = LuaAPI.lua_tostring(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.ToVector2( _value );
                        translator.PushUnityEngineVector2(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToVector3_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _value = LuaAPI.lua_tostring(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.ToVector3( _value );
                        translator.PushUnityEngineVector3(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToVector4_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _value = LuaAPI.lua_tostring(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.ToVector4( _value );
                        translator.PushUnityEngineVector4(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToRect_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _value = LuaAPI.lua_tostring(L, 1);
                    
                        var gen_ret = B2Framework.Utility.Converter.ToRect( _value );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        
        
		
		
		
		
    }
}

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
    public class B2FrameworkVersionWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(B2Framework.Version);
			Utils.BeginObjectRegister(type, L, translator, 0, 3, 3, 2);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetFile", _m_GetFile);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Serialize", _m_Serialize);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Deserialize", _m_Deserialize);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "version", _g_get_version);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ver", _g_get_ver);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "files", _g_get_files);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "ver", _s_set_ver);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "files", _s_set_files);
            
			
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
					
					var gen_ret = new B2Framework.Version();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to B2Framework.Version constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetFile(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.Version gen_to_be_invoked = (B2Framework.Version)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        var gen_ret = gen_to_be_invoked.GetFile( _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Serialize(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.Version gen_to_be_invoked = (B2Framework.Version)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.IO.BinaryWriter _writer = (System.IO.BinaryWriter)translator.GetObject(L, 2, typeof(System.IO.BinaryWriter));
                    
                    gen_to_be_invoked.Serialize( _writer );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Deserialize(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.Version gen_to_be_invoked = (B2Framework.Version)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.IO.BinaryReader _reader = (System.IO.BinaryReader)translator.GetObject(L, 2, typeof(System.IO.BinaryReader));
                    
                    gen_to_be_invoked.Deserialize( _reader );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_version(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.Version gen_to_be_invoked = (B2Framework.Version)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.version);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ver(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.Version gen_to_be_invoked = (B2Framework.Version)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.ver);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_files(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.Version gen_to_be_invoked = (B2Framework.Version)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.files);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ver(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.Version gen_to_be_invoked = (B2Framework.Version)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ver = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_files(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.Version gen_to_be_invoked = (B2Framework.Version)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.files = (System.Collections.Generic.List<B2Framework.VFile>)translator.GetObject(L, 2, typeof(System.Collections.Generic.List<B2Framework.VFile>));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

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
    public class B2FrameworkVFileWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(B2Framework.VFile);
			Utils.BeginObjectRegister(type, L, translator, 0, 2, 3, 3);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Serialize", _m_Serialize);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Deserialize", _m_Deserialize);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "hash", _g_get_hash);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "len", _g_get_len);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "name", _g_get_name);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "hash", _s_set_hash);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "len", _s_set_len);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "name", _s_set_name);
            
			
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
					
					var gen_ret = new B2Framework.VFile();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to B2Framework.VFile constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Serialize(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.VFile gen_to_be_invoked = (B2Framework.VFile)translator.FastGetCSObj(L, 1);
            
            
                
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
            
            
                B2Framework.VFile gen_to_be_invoked = (B2Framework.VFile)translator.FastGetCSObj(L, 1);
            
            
                
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
        static int _g_get_hash(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.VFile gen_to_be_invoked = (B2Framework.VFile)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.hash);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_len(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.VFile gen_to_be_invoked = (B2Framework.VFile)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushint64(L, gen_to_be_invoked.len);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_name(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.VFile gen_to_be_invoked = (B2Framework.VFile)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.name);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_hash(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.VFile gen_to_be_invoked = (B2Framework.VFile)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.hash = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_len(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.VFile gen_to_be_invoked = (B2Framework.VFile)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.len = LuaAPI.lua_toint64(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_name(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.VFile gen_to_be_invoked = (B2Framework.VFile)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.name = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

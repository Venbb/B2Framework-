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
    public class B2FrameworkBundleManifestWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(B2Framework.BundleManifest);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 4, 4);
			
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "activeVariants", _g_get_activeVariants);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "dirs", _g_get_dirs);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "bundles", _g_get_bundles);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "assets", _g_get_assets);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "activeVariants", _s_set_activeVariants);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "dirs", _s_set_dirs);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "bundles", _s_set_bundles);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "assets", _s_set_assets);
            
			
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
					
					B2Framework.BundleManifest gen_ret = new B2Framework.BundleManifest();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to B2Framework.BundleManifest constructor!");
            
        }
        
		
        
		
        
        
        
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_activeVariants(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.BundleManifest gen_to_be_invoked = (B2Framework.BundleManifest)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.activeVariants);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_dirs(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.BundleManifest gen_to_be_invoked = (B2Framework.BundleManifest)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.dirs);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_bundles(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.BundleManifest gen_to_be_invoked = (B2Framework.BundleManifest)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.bundles);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_assets(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.BundleManifest gen_to_be_invoked = (B2Framework.BundleManifest)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.assets);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_activeVariants(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.BundleManifest gen_to_be_invoked = (B2Framework.BundleManifest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.activeVariants = (string[])translator.GetObject(L, 2, typeof(string[]));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_dirs(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.BundleManifest gen_to_be_invoked = (B2Framework.BundleManifest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.dirs = (string[])translator.GetObject(L, 2, typeof(string[]));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_bundles(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.BundleManifest gen_to_be_invoked = (B2Framework.BundleManifest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.bundles = (B2Framework.BundleInfo[])translator.GetObject(L, 2, typeof(B2Framework.BundleInfo[]));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_assets(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.BundleManifest gen_to_be_invoked = (B2Framework.BundleManifest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.assets = (B2Framework.AssetInfo[])translator.GetObject(L, 2, typeof(B2Framework.AssetInfo[]));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

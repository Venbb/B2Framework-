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
    public class B2FrameworkManifestWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(B2Framework.Manifest);
			Utils.BeginObjectRegister(type, L, translator, 0, 6, 2, 0);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetAssetBundleHash", _m_GetAssetBundleHash);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetAllAssetBundles", _m_GetAllAssetBundles);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetAllAssetBundlesWithVariant", _m_GetAllAssetBundlesWithVariant);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetAllDependencies", _m_GetAllDependencies);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetDirectDependencies", _m_GetDirectDependencies);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CompareTo", _m_CompareTo);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "manifest", _g_get_manifest);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Length", _g_get_Length);
            
			
			
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
				if(LuaAPI.lua_gettop(L) == 2 && translator.Assignable<UnityEngine.AssetBundleManifest>(L, 2))
				{
					UnityEngine.AssetBundleManifest _manifest = (UnityEngine.AssetBundleManifest)translator.GetObject(L, 2, typeof(UnityEngine.AssetBundleManifest));
					
					B2Framework.Manifest gen_ret = new B2Framework.Manifest(_manifest);
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to B2Framework.Manifest constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetAssetBundleHash(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.Manifest gen_to_be_invoked = (B2Framework.Manifest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _assetbundleName = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.Hash128 gen_ret = gen_to_be_invoked.GetAssetBundleHash( _assetbundleName );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetAllAssetBundles(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.Manifest gen_to_be_invoked = (B2Framework.Manifest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        string[] gen_ret = gen_to_be_invoked.GetAllAssetBundles(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetAllAssetBundlesWithVariant(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.Manifest gen_to_be_invoked = (B2Framework.Manifest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        string[] gen_ret = gen_to_be_invoked.GetAllAssetBundlesWithVariant(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetAllDependencies(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.Manifest gen_to_be_invoked = (B2Framework.Manifest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _assetbundleName = LuaAPI.lua_tostring(L, 2);
                    
                        string[] gen_ret = gen_to_be_invoked.GetAllDependencies( _assetbundleName );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetDirectDependencies(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.Manifest gen_to_be_invoked = (B2Framework.Manifest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _assetbundleName = LuaAPI.lua_tostring(L, 2);
                    
                        string[] gen_ret = gen_to_be_invoked.GetDirectDependencies( _assetbundleName );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CompareTo(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                B2Framework.Manifest gen_to_be_invoked = (B2Framework.Manifest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    B2Framework.Manifest _other = (B2Framework.Manifest)translator.GetObject(L, 2, typeof(B2Framework.Manifest));
                    
                        System.Collections.Generic.List<string> gen_ret = gen_to_be_invoked.CompareTo( _other );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_manifest(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.Manifest gen_to_be_invoked = (B2Framework.Manifest)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.manifest);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Length(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                B2Framework.Manifest gen_to_be_invoked = (B2Framework.Manifest)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.Length);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
		
		
		
		
    }
}

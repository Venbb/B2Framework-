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
    public class B2FrameworkUtilityAssetsWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(B2Framework.Utility.Assets);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 0, 0);
			
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 11, 13, 6);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "CreatePersistentDataDir", _m_CreatePersistentDataDir_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetPersistentDataPath", _m_GetPersistentDataPath_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetStreamingAssetsDataPath", _m_GetStreamingAssetsDataPath_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetPersistentDataRemotePath", _m_GetPersistentDataRemotePath_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetStreamingAssetsRemotePath", _m_GetStreamingAssetsRemotePath_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetDownloadURL", _m_GetDownloadURL_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ClearPersistentData", _m_ClearPersistentData_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetDataPath", _m_GetDataPath_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetAssetPath", _m_GetAssetPath_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsRemote", _m_IsRemote_xlua_st_);
            
			
            
			Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "basePath", _g_get_basePath);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "dataPath", _g_get_dataPath);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "relativeDataPath", _g_get_relativeDataPath);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "persistentDataPath", _g_get_persistentDataPath);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "streamingAssetsPath", _g_get_streamingAssetsPath);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "downloadUrlPath", _g_get_downloadUrlPath);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "persistentDataRemotePath", _g_get_persistentDataRemotePath);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "streamingAssetsRemotePath", _g_get_streamingAssetsRemotePath);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "manifestFilePath", _g_get_manifestFilePath);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "loadHander", _g_get_loadHander);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "runtimeMode", _g_get_runtimeMode);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "platform", _g_get_platform);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "downloadURL", _g_get_downloadURL);
            
			Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "basePath", _s_set_basePath);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "dataPath", _s_set_dataPath);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "loadHander", _s_set_loadHander);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "runtimeMode", _s_set_runtimeMode);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "platform", _s_set_platform);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "downloadURL", _s_set_downloadURL);
            
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            return LuaAPI.luaL_error(L, "B2Framework.Utility.Assets does not have a constructor!");
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreatePersistentDataDir_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                        string gen_ret = B2Framework.Utility.Assets.CreatePersistentDataDir(  );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetPersistentDataPath_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    
                        string gen_ret = B2Framework.Utility.Assets.GetPersistentDataPath( _path );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetStreamingAssetsDataPath_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    
                        string gen_ret = B2Framework.Utility.Assets.GetStreamingAssetsDataPath( _path );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetPersistentDataRemotePath_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    
                        string gen_ret = B2Framework.Utility.Assets.GetPersistentDataRemotePath( _path );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetStreamingAssetsRemotePath_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    
                        string gen_ret = B2Framework.Utility.Assets.GetStreamingAssetsRemotePath( _path );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetDownloadURL_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    
                        string gen_ret = B2Framework.Utility.Assets.GetDownloadURL( _path );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ClearPersistentData_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    B2Framework.Utility.Assets.ClearPersistentData(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetDataPath_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    
                        string gen_ret = B2Framework.Utility.Assets.GetDataPath( _path );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetAssetPath_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)) 
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    
                        string gen_ret = B2Framework.Utility.Assets.GetAssetPath( _path );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& translator.Assignable<B2Framework.AssetBundles>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)) 
                {
                    B2Framework.AssetBundles _bundle;translator.Get(L, 1, out _bundle);
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        string gen_ret = B2Framework.Utility.Assets.GetAssetPath( _bundle, _path );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to B2Framework.Utility.Assets.GetAssetPath!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsRemote_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    
                        bool gen_ret = B2Framework.Utility.Assets.IsRemote( _path );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_basePath(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, B2Framework.Utility.Assets.basePath);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_dataPath(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, B2Framework.Utility.Assets.dataPath);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_relativeDataPath(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, B2Framework.Utility.Assets.relativeDataPath);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_persistentDataPath(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, B2Framework.Utility.Assets.persistentDataPath);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_streamingAssetsPath(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, B2Framework.Utility.Assets.streamingAssetsPath);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_downloadUrlPath(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, B2Framework.Utility.Assets.downloadUrlPath);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_persistentDataRemotePath(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, B2Framework.Utility.Assets.persistentDataRemotePath);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_streamingAssetsRemotePath(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, B2Framework.Utility.Assets.streamingAssetsRemotePath);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_manifestFilePath(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, B2Framework.Utility.Assets.manifestFilePath);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_loadHander(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, B2Framework.Utility.Assets.loadHander);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_runtimeMode(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushboolean(L, B2Framework.Utility.Assets.runtimeMode);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_platform(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, B2Framework.Utility.Assets.platform);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_downloadURL(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushstring(L, B2Framework.Utility.Assets.downloadURL);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_basePath(RealStatePtr L)
        {
		    try {
                
			    B2Framework.Utility.Assets.basePath = LuaAPI.lua_tostring(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_dataPath(RealStatePtr L)
        {
		    try {
                
			    B2Framework.Utility.Assets.dataPath = LuaAPI.lua_tostring(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_loadHander(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    B2Framework.Utility.Assets.loadHander = translator.GetDelegate<System.Func<string, System.Type, UnityEngine.Object>>(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_runtimeMode(RealStatePtr L)
        {
		    try {
                
			    B2Framework.Utility.Assets.runtimeMode = LuaAPI.lua_toboolean(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_platform(RealStatePtr L)
        {
		    try {
                
			    B2Framework.Utility.Assets.platform = LuaAPI.lua_tostring(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_downloadURL(RealStatePtr L)
        {
		    try {
                
			    B2Framework.Utility.Assets.downloadURL = LuaAPI.lua_tostring(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

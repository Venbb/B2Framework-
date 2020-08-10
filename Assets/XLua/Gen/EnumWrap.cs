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
    
    public class UnityEngineUICanvasScalerScaleModeWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(UnityEngine.UI.CanvasScaler.ScaleMode), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(UnityEngine.UI.CanvasScaler.ScaleMode), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(UnityEngine.UI.CanvasScaler.ScaleMode), L, null, 4, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "ConstantPixelSize", UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "ScaleWithScreenSize", UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "ConstantPhysicalSize", UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPhysicalSize);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(UnityEngine.UI.CanvasScaler.ScaleMode), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushUnityEngineUICanvasScalerScaleMode(L, (UnityEngine.UI.CanvasScaler.ScaleMode)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "ConstantPixelSize"))
                {
                    translator.PushUnityEngineUICanvasScalerScaleMode(L, UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "ScaleWithScreenSize"))
                {
                    translator.PushUnityEngineUICanvasScalerScaleMode(L, UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "ConstantPhysicalSize"))
                {
                    translator.PushUnityEngineUICanvasScalerScaleMode(L, UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPhysicalSize);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for UnityEngine.UI.CanvasScaler.ScaleMode!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for UnityEngine.UI.CanvasScaler.ScaleMode! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class UnityEngineUICanvasScalerScreenMatchModeWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode), L, null, 4, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "MatchWidthOrHeight", UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Expand", UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Shrink", UnityEngine.UI.CanvasScaler.ScreenMatchMode.Shrink);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushUnityEngineUICanvasScalerScreenMatchMode(L, (UnityEngine.UI.CanvasScaler.ScreenMatchMode)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "MatchWidthOrHeight"))
                {
                    translator.PushUnityEngineUICanvasScalerScreenMatchMode(L, UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Expand"))
                {
                    translator.PushUnityEngineUICanvasScalerScreenMatchMode(L, UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Shrink"))
                {
                    translator.PushUnityEngineUICanvasScalerScreenMatchMode(L, UnityEngine.UI.CanvasScaler.ScreenMatchMode.Shrink);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for UnityEngine.UI.CanvasScaler.ScreenMatchMode!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for UnityEngine.UI.CanvasScaler.ScreenMatchMode! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class B2FrameworkPlatformWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(B2Framework.Platform), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(B2Framework.Platform), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(B2Framework.Platform), L, null, 11, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Unknown", B2Framework.Platform.Unknown);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Editor", B2Framework.Platform.Editor);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Windows", B2Framework.Platform.Windows);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Windows64", B2Framework.Platform.Windows64);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "MacOSX", B2Framework.Platform.MacOSX);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Linux", B2Framework.Platform.Linux);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "iOS", B2Framework.Platform.iOS);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Android", B2Framework.Platform.Android);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "WindowsStore", B2Framework.Platform.WindowsStore);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "WebGL", B2Framework.Platform.WebGL);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(B2Framework.Platform), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushB2FrameworkPlatform(L, (B2Framework.Platform)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "Unknown"))
                {
                    translator.PushB2FrameworkPlatform(L, B2Framework.Platform.Unknown);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Editor"))
                {
                    translator.PushB2FrameworkPlatform(L, B2Framework.Platform.Editor);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Windows"))
                {
                    translator.PushB2FrameworkPlatform(L, B2Framework.Platform.Windows);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Windows64"))
                {
                    translator.PushB2FrameworkPlatform(L, B2Framework.Platform.Windows64);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "MacOSX"))
                {
                    translator.PushB2FrameworkPlatform(L, B2Framework.Platform.MacOSX);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Linux"))
                {
                    translator.PushB2FrameworkPlatform(L, B2Framework.Platform.Linux);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "iOS"))
                {
                    translator.PushB2FrameworkPlatform(L, B2Framework.Platform.iOS);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Android"))
                {
                    translator.PushB2FrameworkPlatform(L, B2Framework.Platform.Android);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "WindowsStore"))
                {
                    translator.PushB2FrameworkPlatform(L, B2Framework.Platform.WindowsStore);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "WebGL"))
                {
                    translator.PushB2FrameworkPlatform(L, B2Framework.Platform.WebGL);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for B2Framework.Platform!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for B2Framework.Platform! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class B2FrameworkScenesWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(B2Framework.Scenes), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(B2Framework.Scenes), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(B2Framework.Scenes), L, null, 7, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Start", B2Framework.Scenes.Start);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Updater", B2Framework.Scenes.Updater);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Login", B2Framework.Scenes.Login);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Main", B2Framework.Scenes.Main);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Loading", B2Framework.Scenes.Loading);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Battle", B2Framework.Scenes.Battle);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(B2Framework.Scenes), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushB2FrameworkScenes(L, (B2Framework.Scenes)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "Start"))
                {
                    translator.PushB2FrameworkScenes(L, B2Framework.Scenes.Start);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Updater"))
                {
                    translator.PushB2FrameworkScenes(L, B2Framework.Scenes.Updater);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Login"))
                {
                    translator.PushB2FrameworkScenes(L, B2Framework.Scenes.Login);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Main"))
                {
                    translator.PushB2FrameworkScenes(L, B2Framework.Scenes.Main);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Loading"))
                {
                    translator.PushB2FrameworkScenes(L, B2Framework.Scenes.Loading);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Battle"))
                {
                    translator.PushB2FrameworkScenes(L, B2Framework.Scenes.Battle);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for B2Framework.Scenes!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for B2Framework.Scenes! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class B2FrameworkAssetBundlesWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(B2Framework.AssetBundles), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(B2Framework.AssetBundles), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(B2Framework.AssetBundles), L, null, 12, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Animations", B2Framework.AssetBundles.Animations);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Assets", B2Framework.AssetBundles.Assets);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Audio", B2Framework.AssetBundles.Audio);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Fonts", B2Framework.AssetBundles.Fonts);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Localization", B2Framework.AssetBundles.Localization);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Lua", B2Framework.AssetBundles.Lua);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Materials", B2Framework.AssetBundles.Materials);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Prefabs", B2Framework.AssetBundles.Prefabs);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Protos", B2Framework.AssetBundles.Protos);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Shaders", B2Framework.AssetBundles.Shaders);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Textures", B2Framework.AssetBundles.Textures);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(B2Framework.AssetBundles), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushB2FrameworkAssetBundles(L, (B2Framework.AssetBundles)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "Animations"))
                {
                    translator.PushB2FrameworkAssetBundles(L, B2Framework.AssetBundles.Animations);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Assets"))
                {
                    translator.PushB2FrameworkAssetBundles(L, B2Framework.AssetBundles.Assets);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Audio"))
                {
                    translator.PushB2FrameworkAssetBundles(L, B2Framework.AssetBundles.Audio);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Fonts"))
                {
                    translator.PushB2FrameworkAssetBundles(L, B2Framework.AssetBundles.Fonts);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Localization"))
                {
                    translator.PushB2FrameworkAssetBundles(L, B2Framework.AssetBundles.Localization);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Lua"))
                {
                    translator.PushB2FrameworkAssetBundles(L, B2Framework.AssetBundles.Lua);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Materials"))
                {
                    translator.PushB2FrameworkAssetBundles(L, B2Framework.AssetBundles.Materials);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Prefabs"))
                {
                    translator.PushB2FrameworkAssetBundles(L, B2Framework.AssetBundles.Prefabs);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Protos"))
                {
                    translator.PushB2FrameworkAssetBundles(L, B2Framework.AssetBundles.Protos);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Shaders"))
                {
                    translator.PushB2FrameworkAssetBundles(L, B2Framework.AssetBundles.Shaders);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Textures"))
                {
                    translator.PushB2FrameworkAssetBundles(L, B2Framework.AssetBundles.Textures);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for B2Framework.AssetBundles!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for B2Framework.AssetBundles! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class B2FrameworkGameLanguageWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(B2Framework.GameLanguage), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(B2Framework.GameLanguage), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(B2Framework.GameLanguage), L, null, 7, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "None", B2Framework.GameLanguage.None);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "English", B2Framework.GameLanguage.English);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "ChineseSimplified", B2Framework.GameLanguage.ChineseSimplified);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "ChineseTraditional", B2Framework.GameLanguage.ChineseTraditional);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Japanese", B2Framework.GameLanguage.Japanese);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "All", B2Framework.GameLanguage.All);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(B2Framework.GameLanguage), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushB2FrameworkGameLanguage(L, (B2Framework.GameLanguage)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "None"))
                {
                    translator.PushB2FrameworkGameLanguage(L, B2Framework.GameLanguage.None);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "English"))
                {
                    translator.PushB2FrameworkGameLanguage(L, B2Framework.GameLanguage.English);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "ChineseSimplified"))
                {
                    translator.PushB2FrameworkGameLanguage(L, B2Framework.GameLanguage.ChineseSimplified);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "ChineseTraditional"))
                {
                    translator.PushB2FrameworkGameLanguage(L, B2Framework.GameLanguage.ChineseTraditional);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Japanese"))
                {
                    translator.PushB2FrameworkGameLanguage(L, B2Framework.GameLanguage.Japanese);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "All"))
                {
                    translator.PushB2FrameworkGameLanguage(L, B2Framework.GameLanguage.All);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for B2Framework.GameLanguage!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for B2Framework.GameLanguage! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class B2FrameworkLoadStateWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(B2Framework.LoadState), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(B2Framework.LoadState), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(B2Framework.LoadState), L, null, 6, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Init", B2Framework.LoadState.Init);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "LoadAssetBundle", B2Framework.LoadState.LoadAssetBundle);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "LoadAsset", B2Framework.LoadState.LoadAsset);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Loaded", B2Framework.LoadState.Loaded);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Unload", B2Framework.LoadState.Unload);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(B2Framework.LoadState), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushB2FrameworkLoadState(L, (B2Framework.LoadState)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "Init"))
                {
                    translator.PushB2FrameworkLoadState(L, B2Framework.LoadState.Init);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "LoadAssetBundle"))
                {
                    translator.PushB2FrameworkLoadState(L, B2Framework.LoadState.LoadAssetBundle);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "LoadAsset"))
                {
                    translator.PushB2FrameworkLoadState(L, B2Framework.LoadState.LoadAsset);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Loaded"))
                {
                    translator.PushB2FrameworkLoadState(L, B2Framework.LoadState.Loaded);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Unload"))
                {
                    translator.PushB2FrameworkLoadState(L, B2Framework.LoadState.Unload);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for B2Framework.LoadState!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for B2Framework.LoadState! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class B2FrameworkVerifyByWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(B2Framework.VerifyBy), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(B2Framework.VerifyBy), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(B2Framework.VerifyBy), L, null, 3, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Size", B2Framework.VerifyBy.Size);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Hash", B2Framework.VerifyBy.Hash);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(B2Framework.VerifyBy), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushB2FrameworkVerifyBy(L, (B2Framework.VerifyBy)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "Size"))
                {
                    translator.PushB2FrameworkVerifyBy(L, B2Framework.VerifyBy.Size);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Hash"))
                {
                    translator.PushB2FrameworkVerifyBy(L, B2Framework.VerifyBy.Hash);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for B2Framework.VerifyBy!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for B2Framework.VerifyBy! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class B2FrameworkUStatusWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(B2Framework.UStatus), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(B2Framework.UStatus), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(B2Framework.UStatus), L, null, 7, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Idle", B2Framework.UStatus.Idle);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Requesting", B2Framework.UStatus.Requesting);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Checking", B2Framework.UStatus.Checking);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "CheckOver", B2Framework.UStatus.CheckOver);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Downloading", B2Framework.UStatus.Downloading);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Completed", B2Framework.UStatus.Completed);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(B2Framework.UStatus), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushB2FrameworkUStatus(L, (B2Framework.UStatus)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "Idle"))
                {
                    translator.PushB2FrameworkUStatus(L, B2Framework.UStatus.Idle);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Requesting"))
                {
                    translator.PushB2FrameworkUStatus(L, B2Framework.UStatus.Requesting);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Checking"))
                {
                    translator.PushB2FrameworkUStatus(L, B2Framework.UStatus.Checking);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "CheckOver"))
                {
                    translator.PushB2FrameworkUStatus(L, B2Framework.UStatus.CheckOver);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Downloading"))
                {
                    translator.PushB2FrameworkUStatus(L, B2Framework.UStatus.Downloading);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Completed"))
                {
                    translator.PushB2FrameworkUStatus(L, B2Framework.UStatus.Completed);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for B2Framework.UStatus!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for B2Framework.UStatus! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class B2FrameworkUActWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(B2Framework.UAct), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(B2Framework.UAct), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(B2Framework.UAct), L, null, 4, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "No", B2Framework.UAct.No);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "App", B2Framework.UAct.App);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Res", B2Framework.UAct.Res);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(B2Framework.UAct), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushB2FrameworkUAct(L, (B2Framework.UAct)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "No"))
                {
                    translator.PushB2FrameworkUAct(L, B2Framework.UAct.No);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "App"))
                {
                    translator.PushB2FrameworkUAct(L, B2Framework.UAct.App);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Res"))
                {
                    translator.PushB2FrameworkUAct(L, B2Framework.UAct.Res);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for B2Framework.UAct!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for B2Framework.UAct! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class B2FrameworkVariableEnumWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(B2Framework.VariableEnum), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(B2Framework.VariableEnum), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(B2Framework.VariableEnum), L, null, 12, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Component", B2Framework.VariableEnum.Component);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "GameObject", B2Framework.VariableEnum.GameObject);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Object", B2Framework.VariableEnum.Object);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Boolean", B2Framework.VariableEnum.Boolean);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Integer", B2Framework.VariableEnum.Integer);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Float", B2Framework.VariableEnum.Float);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "String", B2Framework.VariableEnum.String);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Color", B2Framework.VariableEnum.Color);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Vector2", B2Framework.VariableEnum.Vector2);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Vector3", B2Framework.VariableEnum.Vector3);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Vector4", B2Framework.VariableEnum.Vector4);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(B2Framework.VariableEnum), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushB2FrameworkVariableEnum(L, (B2Framework.VariableEnum)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "Component"))
                {
                    translator.PushB2FrameworkVariableEnum(L, B2Framework.VariableEnum.Component);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "GameObject"))
                {
                    translator.PushB2FrameworkVariableEnum(L, B2Framework.VariableEnum.GameObject);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Object"))
                {
                    translator.PushB2FrameworkVariableEnum(L, B2Framework.VariableEnum.Object);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Boolean"))
                {
                    translator.PushB2FrameworkVariableEnum(L, B2Framework.VariableEnum.Boolean);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Integer"))
                {
                    translator.PushB2FrameworkVariableEnum(L, B2Framework.VariableEnum.Integer);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Float"))
                {
                    translator.PushB2FrameworkVariableEnum(L, B2Framework.VariableEnum.Float);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "String"))
                {
                    translator.PushB2FrameworkVariableEnum(L, B2Framework.VariableEnum.String);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Color"))
                {
                    translator.PushB2FrameworkVariableEnum(L, B2Framework.VariableEnum.Color);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Vector2"))
                {
                    translator.PushB2FrameworkVariableEnum(L, B2Framework.VariableEnum.Vector2);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Vector3"))
                {
                    translator.PushB2FrameworkVariableEnum(L, B2Framework.VariableEnum.Vector3);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Vector4"))
                {
                    translator.PushB2FrameworkVariableEnum(L, B2Framework.VariableEnum.Vector4);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for B2Framework.VariableEnum!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for B2Framework.VariableEnum! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
    public class B2FrameworkLuaScriptBindEnumWrap
    {
		public static void __Register(RealStatePtr L)
        {
		    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
		    Utils.BeginObjectRegister(typeof(B2Framework.LuaScriptBindEnum), L, translator, 0, 0, 0, 0);
			Utils.EndObjectRegister(typeof(B2Framework.LuaScriptBindEnum), L, translator, null, null, null, null, null);
			
			Utils.BeginClassRegister(typeof(B2Framework.LuaScriptBindEnum), L, null, 3, 0, 0);

            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "TextAsset", B2Framework.LuaScriptBindEnum.TextAsset);
            
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Filename", B2Framework.LuaScriptBindEnum.Filename);
            

			Utils.RegisterFunc(L, Utils.CLS_IDX, "__CastFrom", __CastFrom);
            
            Utils.EndClassRegister(typeof(B2Framework.LuaScriptBindEnum), L, translator);
        }
		
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CastFrom(RealStatePtr L)
		{
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			LuaTypes lua_type = LuaAPI.lua_type(L, 1);
            if (lua_type == LuaTypes.LUA_TNUMBER)
            {
                translator.PushB2FrameworkLuaScriptBindEnum(L, (B2Framework.LuaScriptBindEnum)LuaAPI.xlua_tointeger(L, 1));
            }
			
            else if(lua_type == LuaTypes.LUA_TSTRING)
            {

			    if (LuaAPI.xlua_is_eq_str(L, 1, "TextAsset"))
                {
                    translator.PushB2FrameworkLuaScriptBindEnum(L, B2Framework.LuaScriptBindEnum.TextAsset);
                }
				else if (LuaAPI.xlua_is_eq_str(L, 1, "Filename"))
                {
                    translator.PushB2FrameworkLuaScriptBindEnum(L, B2Framework.LuaScriptBindEnum.Filename);
                }
				else
                {
                    return LuaAPI.luaL_error(L, "invalid string for B2Framework.LuaScriptBindEnum!");
                }

            }
			
            else
            {
                return LuaAPI.luaL_error(L, "invalid lua type for B2Framework.LuaScriptBindEnum! Expect number or string, got + " + lua_type);
            }

            return 1;
		}
	}
    
}
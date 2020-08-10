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
    public class B2FrameworkAppConstWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(B2Framework.AppConst);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 0, 0);
			
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 15, 0, 0);
			
			
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "SIZE_KB", B2Framework.AppConst.SIZE_KB);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "SIZE_MB", B2Framework.AppConst.SIZE_MB);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "SIZE_GB", B2Framework.AppConst.SIZE_GB);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "RELEASE_DIR", B2Framework.AppConst.RELEASE_DIR);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "ASSETBUNDLES", B2Framework.AppConst.ASSETBUNDLES);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "ASSETBUNDLE_ASSETS_PATH", B2Framework.AppConst.ASSETBUNDLE_ASSETS_PATH);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "ASSETBUNDLE_VARIANT", B2Framework.AppConst.ASSETBUNDLE_VARIANT);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "BUNDLE_MANIFEST", B2Framework.AppConst.BUNDLE_MANIFEST);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "VERSION", B2Framework.AppConst.VERSION);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "RES_VER_FILE", B2Framework.AppConst.RES_VER_FILE);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "LUA_SCRIPTS_PATH", B2Framework.AppConst.LUA_SCRIPTS_PATH);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "LUA_EXTENSIONS", B2Framework.AppConst.LUA_EXTENSIONS);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "LUA_EXTENSION", B2Framework.AppConst.LUA_EXTENSION);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "SYMBOL_DEBUG", B2Framework.AppConst.SYMBOL_DEBUG);
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            return LuaAPI.luaL_error(L, "B2Framework.AppConst does not have a constructor!");
        }
        
		
        
		
        
        
        
        
        
        
        
        
        
		
		
		
		
    }
}

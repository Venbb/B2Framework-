#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;


namespace XLua.CSObjectWrap
{
    public class XLua_Gen_Initer_Register__
	{
        
        
        static void wrapInit0(LuaEnv luaenv, ObjectTranslator translator)
        {
        
            translator.DelayWrapLoader(typeof(object), SystemObjectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Object), UnityEngineObjectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Vector2), UnityEngineVector2Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Vector3), UnityEngineVector3Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Vector4), UnityEngineVector4Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Quaternion), UnityEngineQuaternionWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Color), UnityEngineColorWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Ray), UnityEngineRayWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Bounds), UnityEngineBoundsWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Ray2D), UnityEngineRay2DWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Time), UnityEngineTimeWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.GameObject), UnityEngineGameObjectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Component), UnityEngineComponentWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Behaviour), UnityEngineBehaviourWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Transform), UnityEngineTransformWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Resources), UnityEngineResourcesWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.TextAsset), UnityEngineTextAssetWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Keyframe), UnityEngineKeyframeWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.AnimationCurve), UnityEngineAnimationCurveWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.AnimationClip), UnityEngineAnimationClipWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.MonoBehaviour), UnityEngineMonoBehaviourWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.ParticleSystem), UnityEngineParticleSystemWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.SkinnedMeshRenderer), UnityEngineSkinnedMeshRendererWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Renderer), UnityEngineRendererWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Light), UnityEngineLightWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Mathf), UnityEngineMathfWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(System.Collections.Generic.List<int>), SystemCollectionsGenericList_1_SystemInt32_Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Debug), UnityEngineDebugWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Canvas), UnityEngineCanvasWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Rect), UnityEngineRectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.RectTransform), UnityEngineRectTransformWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.RectOffset), UnityEngineRectOffsetWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Sprite), UnityEngineSpriteWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.CanvasScaler), UnityEngineUICanvasScalerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.CanvasScaler.ScaleMode), UnityEngineUICanvasScalerScaleModeWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode), UnityEngineUICanvasScalerScreenMatchModeWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.GraphicRaycaster), UnityEngineUIGraphicRaycasterWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.Text), UnityEngineUITextWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.InputField), UnityEngineUIInputFieldWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.Button), UnityEngineUIButtonWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.Image), UnityEngineUIImageWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.ScrollRect), UnityEngineUIScrollRectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.Scrollbar), UnityEngineUIScrollbarWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.Toggle), UnityEngineUIToggleWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.ToggleGroup), UnityEngineUIToggleGroupWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.Button.ButtonClickedEvent), UnityEngineUIButtonButtonClickedEventWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.ScrollRect.ScrollRectEvent), UnityEngineUIScrollRectScrollRectEventWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.GridLayoutGroup), UnityEngineUIGridLayoutGroupWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.ContentSizeFitter), UnityEngineUIContentSizeFitterWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.UI.Slider), UnityEngineUISliderWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Assets), B2FrameworkAssetsWrap.__Register);
        
        }
        
        static void wrapInit1(LuaEnv luaenv, ObjectTranslator translator)
        {
        
            translator.DelayWrapLoader(typeof(B2Framework.AssetInfo), B2FrameworkAssetInfoWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.BundleInfo), B2FrameworkBundleInfoWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.BundleManifest), B2FrameworkBundleManifestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Download), B2FrameworkDownloadWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Downloader), B2FrameworkDownloaderWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Manifest), B2FrameworkManifestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.ILoader), B2FrameworkILoaderWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.AssetLoader), B2FrameworkAssetLoaderWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.SceneLoader), B2FrameworkSceneLoaderWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.GameObjectLoader), B2FrameworkGameObjectLoaderWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.PreloadManager), B2FrameworkPreloadManagerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.AssetRequest), B2FrameworkAssetRequestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.BundleAssetRequest), B2FrameworkBundleAssetRequestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.BundleAssetAsyncRequest), B2FrameworkBundleAssetAsyncRequestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.BundleRequest), B2FrameworkBundleRequestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.BundleAsyncRequest), B2FrameworkBundleAsyncRequestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.WebBundleRequest), B2FrameworkWebBundleRequestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.ManifestRequest), B2FrameworkManifestRequestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Reference), B2FrameworkReferenceWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.SceneAssetRequest), B2FrameworkSceneAssetRequestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.SceneAssetAsyncRequest), B2FrameworkSceneAssetAsyncRequestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.WebAssetRequest), B2FrameworkWebAssetRequestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.IUpdater), B2FrameworkIUpdaterWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Updater), B2FrameworkUpdaterWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Versions), B2FrameworkVersionsWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Version), B2FrameworkVersionWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.VFile), B2FrameworkVFileWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.AppConst), B2FrameworkAppConstWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.ReadOnlyAttribute), B2FrameworkReadOnlyAttributeWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Platform), B2FrameworkPlatformWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Scenes), B2FrameworkScenesWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.AssetBundles), B2FrameworkAssetBundlesWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.GameLanguage), B2FrameworkGameLanguageWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.LoadState), B2FrameworkLoadStateWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.VerifyBy), B2FrameworkVerifyByWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.UStatus), B2FrameworkUStatusWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.UAct), B2FrameworkUActWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.VariableEnum), B2FrameworkVariableEnumWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.LuaScriptBindEnum), B2FrameworkLuaScriptBindEnumWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.DontDestroyOnLoad), B2FrameworkDontDestroyOnLoadWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Game), B2FrameworkGameWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.GameStart), B2FrameworkGameStartWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Language), B2FrameworkLanguageWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Localization), B2FrameworkLocalizationWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.SensitiveWordsChecker), B2FrameworkSensitiveWordsCheckerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.SensitiveWordsFilter), B2FrameworkSensitiveWordsFilterWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Debug), B2FrameworkDebugWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Splash), B2FrameworkSplashWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.The), B2FrameworkTheWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Variable), B2FrameworkVariableWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.VariableList), B2FrameworkVariableListWrap.__Register);
        
        }
        
        static void wrapInit2(LuaEnv luaenv, ObjectTranslator translator)
        {
        
            translator.DelayWrapLoader(typeof(B2Framework.LuaBehaviour), B2FrameworkLuaBehaviourWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.LuaCoroutine), B2FrameworkLuaCoroutineWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.LuaManager), B2FrameworkLuaManagerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.LuaRuntime), B2FrameworkLuaRuntimeWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.LuaScriptBinder), B2FrameworkLuaScriptBinderWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.ScenesManager), B2FrameworkScenesManagerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Settings), B2FrameworkSettingsWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.StringEx), B2FrameworkStringExWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Utility), B2FrameworkUtilityWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.UI.UIText), B2FrameworkUIUITextWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.UI.MessageBox), B2FrameworkUIMessageBoxWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.UI.UpdateScreen), B2FrameworkUIUpdateScreenWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Utility.Assets), B2FrameworkUtilityAssetsWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Utility.Converter), B2FrameworkUtilityConverterWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Utility.Encryption), B2FrameworkUtilityEncryptionWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Utility.Files), B2FrameworkUtilityFilesWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Utility.Path), B2FrameworkUtilityPathWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Utility.Text), B2FrameworkUtilityTextWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Utility.Verifier), B2FrameworkUtilityVerifierWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(B2Framework.Utility.Zip), B2FrameworkUtilityZipWrap.__Register);
        
        
        
        }
        
        static void Init(LuaEnv luaenv, ObjectTranslator translator)
        {
            
            wrapInit0(luaenv, translator);
            
            wrapInit1(luaenv, translator);
            
            wrapInit2(luaenv, translator);
            
            
            translator.AddInterfaceBridgeCreator(typeof(System.Collections.IEnumerator), SystemCollectionsIEnumeratorBridge.__Create);
            
        }
        
	    static XLua_Gen_Initer_Register__()
        {
		    XLua.LuaEnv.AddIniter(Init);
		}
		
		
	}
	
}
namespace XLua
{
	public partial class ObjectTranslator
	{
		static XLua.CSObjectWrap.XLua_Gen_Initer_Register__ s_gen_reg_dumb_obj = new XLua.CSObjectWrap.XLua_Gen_Initer_Register__();
		static XLua.CSObjectWrap.XLua_Gen_Initer_Register__ gen_reg_dumb_obj {get{return s_gen_reg_dumb_obj;}}
	}
	
	internal partial class InternalGlobals
    {
	    
		delegate bool __GEN_DELEGATE0( string str);
		
		delegate int __GEN_DELEGATE1( string str,  int def);
		
		delegate uint __GEN_DELEGATE2( string str,  uint def);
		
		delegate long __GEN_DELEGATE3( string str);
		
		delegate string __GEN_DELEGATE4( string str,  bool forward);
		
		delegate bool __GEN_DELEGATE5( string str);
		
		delegate System.DateTime __GEN_DELEGATE6( string str);
		
		delegate string __GEN_DELEGATE7( string str,  string format);
		
	    static InternalGlobals()
		{
		    extensionMethodMap = new Dictionary<Type, IEnumerable<MethodInfo>>()
			{
			    
				{typeof(string), new List<MethodInfo>(){
				
				  new __GEN_DELEGATE0(B2Framework.StringEx.IsInt)
#if UNITY_WSA && !UNITY_EDITOR
                                      .GetMethodInfo(),
#else
                                      .Method,
#endif
				
				  new __GEN_DELEGATE1(B2Framework.StringEx.ToInt)
#if UNITY_WSA && !UNITY_EDITOR
                                      .GetMethodInfo(),
#else
                                      .Method,
#endif
				
				  new __GEN_DELEGATE2(B2Framework.StringEx.ToUInt)
#if UNITY_WSA && !UNITY_EDITOR
                                      .GetMethodInfo(),
#else
                                      .Method,
#endif
				
				  new __GEN_DELEGATE3(B2Framework.StringEx.ToInt64)
#if UNITY_WSA && !UNITY_EDITOR
                                      .GetMethodInfo(),
#else
                                      .Method,
#endif
				
				  new __GEN_DELEGATE4(B2Framework.StringEx.ToPath)
#if UNITY_WSA && !UNITY_EDITOR
                                      .GetMethodInfo(),
#else
                                      .Method,
#endif
				
				  new __GEN_DELEGATE5(B2Framework.StringEx.IsDateTime)
#if UNITY_WSA && !UNITY_EDITOR
                                      .GetMethodInfo(),
#else
                                      .Method,
#endif
				
				  new __GEN_DELEGATE6(B2Framework.StringEx.ToDateTime)
#if UNITY_WSA && !UNITY_EDITOR
                                      .GetMethodInfo(),
#else
                                      .Method,
#endif
				
				  new __GEN_DELEGATE7(B2Framework.StringEx.ToDateTime)
#if UNITY_WSA && !UNITY_EDITOR
                                      .GetMethodInfo(),
#else
                                      .Method,
#endif
				
				}},
				
			};
			
			genTryArrayGetPtr = StaticLuaCallbacks.__tryArrayGet;
            genTryArraySetPtr = StaticLuaCallbacks.__tryArraySet;
		}
	}
}

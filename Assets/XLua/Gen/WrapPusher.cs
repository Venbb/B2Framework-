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


namespace XLua
{
    public partial class ObjectTranslator
    {
        
        class IniterAdderUnityEngineVector2
        {
            static IniterAdderUnityEngineVector2()
            {
                LuaEnv.AddIniter(Init);
            }
			
			static void Init(LuaEnv luaenv, ObjectTranslator translator)
			{
			
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Vector2>(translator.PushUnityEngineVector2, translator.Get, translator.UpdateUnityEngineVector2);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Vector3>(translator.PushUnityEngineVector3, translator.Get, translator.UpdateUnityEngineVector3);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Vector4>(translator.PushUnityEngineVector4, translator.Get, translator.UpdateUnityEngineVector4);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Color>(translator.PushUnityEngineColor, translator.Get, translator.UpdateUnityEngineColor);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Quaternion>(translator.PushUnityEngineQuaternion, translator.Get, translator.UpdateUnityEngineQuaternion);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Ray>(translator.PushUnityEngineRay, translator.Get, translator.UpdateUnityEngineRay);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Bounds>(translator.PushUnityEngineBounds, translator.Get, translator.UpdateUnityEngineBounds);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Ray2D>(translator.PushUnityEngineRay2D, translator.Get, translator.UpdateUnityEngineRay2D);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.UI.CanvasScaler.ScaleMode>(translator.PushUnityEngineUICanvasScalerScaleMode, translator.Get, translator.UpdateUnityEngineUICanvasScalerScaleMode);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.UI.CanvasScaler.ScreenMatchMode>(translator.PushUnityEngineUICanvasScalerScreenMatchMode, translator.Get, translator.UpdateUnityEngineUICanvasScalerScreenMatchMode);
				translator.RegisterPushAndGetAndUpdate<B2Framework.Platform>(translator.PushB2FrameworkPlatform, translator.Get, translator.UpdateB2FrameworkPlatform);
				translator.RegisterPushAndGetAndUpdate<B2Framework.Scenes>(translator.PushB2FrameworkScenes, translator.Get, translator.UpdateB2FrameworkScenes);
				translator.RegisterPushAndGetAndUpdate<B2Framework.AssetBundles>(translator.PushB2FrameworkAssetBundles, translator.Get, translator.UpdateB2FrameworkAssetBundles);
				translator.RegisterPushAndGetAndUpdate<B2Framework.GameLanguage>(translator.PushB2FrameworkGameLanguage, translator.Get, translator.UpdateB2FrameworkGameLanguage);
				translator.RegisterPushAndGetAndUpdate<B2Framework.LoadState>(translator.PushB2FrameworkLoadState, translator.Get, translator.UpdateB2FrameworkLoadState);
				translator.RegisterPushAndGetAndUpdate<B2Framework.VerifyBy>(translator.PushB2FrameworkVerifyBy, translator.Get, translator.UpdateB2FrameworkVerifyBy);
				translator.RegisterPushAndGetAndUpdate<B2Framework.UStatus>(translator.PushB2FrameworkUStatus, translator.Get, translator.UpdateB2FrameworkUStatus);
				translator.RegisterPushAndGetAndUpdate<B2Framework.UAct>(translator.PushB2FrameworkUAct, translator.Get, translator.UpdateB2FrameworkUAct);
				translator.RegisterPushAndGetAndUpdate<B2Framework.VariableEnum>(translator.PushB2FrameworkVariableEnum, translator.Get, translator.UpdateB2FrameworkVariableEnum);
				translator.RegisterPushAndGetAndUpdate<B2Framework.LuaScriptBindEnum>(translator.PushB2FrameworkLuaScriptBindEnum, translator.Get, translator.UpdateB2FrameworkLuaScriptBindEnum);
			
			}
        }
        
        static IniterAdderUnityEngineVector2 s_IniterAdderUnityEngineVector2_dumb_obj = new IniterAdderUnityEngineVector2();
        static IniterAdderUnityEngineVector2 IniterAdderUnityEngineVector2_dumb_obj {get{return s_IniterAdderUnityEngineVector2_dumb_obj;}}
        
        
        int UnityEngineVector2_TypeID = -1;
        public void PushUnityEngineVector2(RealStatePtr L, UnityEngine.Vector2 val)
        {
            if (UnityEngineVector2_TypeID == -1)
            {
			    bool is_first;
                UnityEngineVector2_TypeID = getTypeId(L, typeof(UnityEngine.Vector2), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 8, UnityEngineVector2_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Vector2 ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Vector2 val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineVector2_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Vector2");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Vector2");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Vector2)objectCasters.GetCaster(typeof(UnityEngine.Vector2))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineVector2(RealStatePtr L, int index, UnityEngine.Vector2 val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineVector2_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Vector2");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Vector2 ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineVector3_TypeID = -1;
        public void PushUnityEngineVector3(RealStatePtr L, UnityEngine.Vector3 val)
        {
            if (UnityEngineVector3_TypeID == -1)
            {
			    bool is_first;
                UnityEngineVector3_TypeID = getTypeId(L, typeof(UnityEngine.Vector3), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 12, UnityEngineVector3_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Vector3 ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Vector3 val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineVector3_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Vector3");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Vector3");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Vector3)objectCasters.GetCaster(typeof(UnityEngine.Vector3))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineVector3(RealStatePtr L, int index, UnityEngine.Vector3 val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineVector3_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Vector3");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Vector3 ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineVector4_TypeID = -1;
        public void PushUnityEngineVector4(RealStatePtr L, UnityEngine.Vector4 val)
        {
            if (UnityEngineVector4_TypeID == -1)
            {
			    bool is_first;
                UnityEngineVector4_TypeID = getTypeId(L, typeof(UnityEngine.Vector4), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 16, UnityEngineVector4_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Vector4 ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Vector4 val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineVector4_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Vector4");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Vector4");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Vector4)objectCasters.GetCaster(typeof(UnityEngine.Vector4))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineVector4(RealStatePtr L, int index, UnityEngine.Vector4 val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineVector4_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Vector4");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Vector4 ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineColor_TypeID = -1;
        public void PushUnityEngineColor(RealStatePtr L, UnityEngine.Color val)
        {
            if (UnityEngineColor_TypeID == -1)
            {
			    bool is_first;
                UnityEngineColor_TypeID = getTypeId(L, typeof(UnityEngine.Color), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 16, UnityEngineColor_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Color ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Color val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineColor_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Color");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Color");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Color)objectCasters.GetCaster(typeof(UnityEngine.Color))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineColor(RealStatePtr L, int index, UnityEngine.Color val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineColor_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Color");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Color ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineQuaternion_TypeID = -1;
        public void PushUnityEngineQuaternion(RealStatePtr L, UnityEngine.Quaternion val)
        {
            if (UnityEngineQuaternion_TypeID == -1)
            {
			    bool is_first;
                UnityEngineQuaternion_TypeID = getTypeId(L, typeof(UnityEngine.Quaternion), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 16, UnityEngineQuaternion_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Quaternion ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Quaternion val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineQuaternion_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Quaternion");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Quaternion");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Quaternion)objectCasters.GetCaster(typeof(UnityEngine.Quaternion))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineQuaternion(RealStatePtr L, int index, UnityEngine.Quaternion val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineQuaternion_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Quaternion");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Quaternion ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineRay_TypeID = -1;
        public void PushUnityEngineRay(RealStatePtr L, UnityEngine.Ray val)
        {
            if (UnityEngineRay_TypeID == -1)
            {
			    bool is_first;
                UnityEngineRay_TypeID = getTypeId(L, typeof(UnityEngine.Ray), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 24, UnityEngineRay_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Ray ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Ray val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineRay_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Ray");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Ray");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Ray)objectCasters.GetCaster(typeof(UnityEngine.Ray))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineRay(RealStatePtr L, int index, UnityEngine.Ray val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineRay_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Ray");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Ray ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineBounds_TypeID = -1;
        public void PushUnityEngineBounds(RealStatePtr L, UnityEngine.Bounds val)
        {
            if (UnityEngineBounds_TypeID == -1)
            {
			    bool is_first;
                UnityEngineBounds_TypeID = getTypeId(L, typeof(UnityEngine.Bounds), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 24, UnityEngineBounds_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Bounds ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Bounds val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineBounds_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Bounds");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Bounds");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Bounds)objectCasters.GetCaster(typeof(UnityEngine.Bounds))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineBounds(RealStatePtr L, int index, UnityEngine.Bounds val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineBounds_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Bounds");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Bounds ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineRay2D_TypeID = -1;
        public void PushUnityEngineRay2D(RealStatePtr L, UnityEngine.Ray2D val)
        {
            if (UnityEngineRay2D_TypeID == -1)
            {
			    bool is_first;
                UnityEngineRay2D_TypeID = getTypeId(L, typeof(UnityEngine.Ray2D), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 16, UnityEngineRay2D_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Ray2D ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Ray2D val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineRay2D_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Ray2D");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Ray2D");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Ray2D)objectCasters.GetCaster(typeof(UnityEngine.Ray2D))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineRay2D(RealStatePtr L, int index, UnityEngine.Ray2D val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineRay2D_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Ray2D");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Ray2D ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineUICanvasScalerScaleMode_TypeID = -1;
		int UnityEngineUICanvasScalerScaleMode_EnumRef = -1;
        
        public void PushUnityEngineUICanvasScalerScaleMode(RealStatePtr L, UnityEngine.UI.CanvasScaler.ScaleMode val)
        {
            if (UnityEngineUICanvasScalerScaleMode_TypeID == -1)
            {
			    bool is_first;
                UnityEngineUICanvasScalerScaleMode_TypeID = getTypeId(L, typeof(UnityEngine.UI.CanvasScaler.ScaleMode), out is_first);
				
				if (UnityEngineUICanvasScalerScaleMode_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(UnityEngine.UI.CanvasScaler.ScaleMode));
				    UnityEngineUICanvasScalerScaleMode_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, UnityEngineUICanvasScalerScaleMode_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, UnityEngineUICanvasScalerScaleMode_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for UnityEngine.UI.CanvasScaler.ScaleMode ,value="+val);
            }
			
			LuaAPI.lua_getref(L, UnityEngineUICanvasScalerScaleMode_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.UI.CanvasScaler.ScaleMode val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineUICanvasScalerScaleMode_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.UI.CanvasScaler.ScaleMode");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for UnityEngine.UI.CanvasScaler.ScaleMode");
                }
				val = (UnityEngine.UI.CanvasScaler.ScaleMode)e;
                
            }
            else
            {
                val = (UnityEngine.UI.CanvasScaler.ScaleMode)objectCasters.GetCaster(typeof(UnityEngine.UI.CanvasScaler.ScaleMode))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineUICanvasScalerScaleMode(RealStatePtr L, int index, UnityEngine.UI.CanvasScaler.ScaleMode val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineUICanvasScalerScaleMode_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.UI.CanvasScaler.ScaleMode");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for UnityEngine.UI.CanvasScaler.ScaleMode ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineUICanvasScalerScreenMatchMode_TypeID = -1;
		int UnityEngineUICanvasScalerScreenMatchMode_EnumRef = -1;
        
        public void PushUnityEngineUICanvasScalerScreenMatchMode(RealStatePtr L, UnityEngine.UI.CanvasScaler.ScreenMatchMode val)
        {
            if (UnityEngineUICanvasScalerScreenMatchMode_TypeID == -1)
            {
			    bool is_first;
                UnityEngineUICanvasScalerScreenMatchMode_TypeID = getTypeId(L, typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode), out is_first);
				
				if (UnityEngineUICanvasScalerScreenMatchMode_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode));
				    UnityEngineUICanvasScalerScreenMatchMode_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, UnityEngineUICanvasScalerScreenMatchMode_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, UnityEngineUICanvasScalerScreenMatchMode_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for UnityEngine.UI.CanvasScaler.ScreenMatchMode ,value="+val);
            }
			
			LuaAPI.lua_getref(L, UnityEngineUICanvasScalerScreenMatchMode_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.UI.CanvasScaler.ScreenMatchMode val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineUICanvasScalerScreenMatchMode_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.UI.CanvasScaler.ScreenMatchMode");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for UnityEngine.UI.CanvasScaler.ScreenMatchMode");
                }
				val = (UnityEngine.UI.CanvasScaler.ScreenMatchMode)e;
                
            }
            else
            {
                val = (UnityEngine.UI.CanvasScaler.ScreenMatchMode)objectCasters.GetCaster(typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineUICanvasScalerScreenMatchMode(RealStatePtr L, int index, UnityEngine.UI.CanvasScaler.ScreenMatchMode val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineUICanvasScalerScreenMatchMode_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.UI.CanvasScaler.ScreenMatchMode");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for UnityEngine.UI.CanvasScaler.ScreenMatchMode ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int B2FrameworkPlatform_TypeID = -1;
		int B2FrameworkPlatform_EnumRef = -1;
        
        public void PushB2FrameworkPlatform(RealStatePtr L, B2Framework.Platform val)
        {
            if (B2FrameworkPlatform_TypeID == -1)
            {
			    bool is_first;
                B2FrameworkPlatform_TypeID = getTypeId(L, typeof(B2Framework.Platform), out is_first);
				
				if (B2FrameworkPlatform_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(B2Framework.Platform));
				    B2FrameworkPlatform_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, B2FrameworkPlatform_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, B2FrameworkPlatform_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for B2Framework.Platform ,value="+val);
            }
			
			LuaAPI.lua_getref(L, B2FrameworkPlatform_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out B2Framework.Platform val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkPlatform_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.Platform");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for B2Framework.Platform");
                }
				val = (B2Framework.Platform)e;
                
            }
            else
            {
                val = (B2Framework.Platform)objectCasters.GetCaster(typeof(B2Framework.Platform))(L, index, null);
            }
        }
		
        public void UpdateB2FrameworkPlatform(RealStatePtr L, int index, B2Framework.Platform val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkPlatform_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.Platform");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for B2Framework.Platform ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int B2FrameworkScenes_TypeID = -1;
		int B2FrameworkScenes_EnumRef = -1;
        
        public void PushB2FrameworkScenes(RealStatePtr L, B2Framework.Scenes val)
        {
            if (B2FrameworkScenes_TypeID == -1)
            {
			    bool is_first;
                B2FrameworkScenes_TypeID = getTypeId(L, typeof(B2Framework.Scenes), out is_first);
				
				if (B2FrameworkScenes_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(B2Framework.Scenes));
				    B2FrameworkScenes_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, B2FrameworkScenes_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, B2FrameworkScenes_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for B2Framework.Scenes ,value="+val);
            }
			
			LuaAPI.lua_getref(L, B2FrameworkScenes_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out B2Framework.Scenes val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkScenes_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.Scenes");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for B2Framework.Scenes");
                }
				val = (B2Framework.Scenes)e;
                
            }
            else
            {
                val = (B2Framework.Scenes)objectCasters.GetCaster(typeof(B2Framework.Scenes))(L, index, null);
            }
        }
		
        public void UpdateB2FrameworkScenes(RealStatePtr L, int index, B2Framework.Scenes val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkScenes_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.Scenes");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for B2Framework.Scenes ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int B2FrameworkAssetBundles_TypeID = -1;
		int B2FrameworkAssetBundles_EnumRef = -1;
        
        public void PushB2FrameworkAssetBundles(RealStatePtr L, B2Framework.AssetBundles val)
        {
            if (B2FrameworkAssetBundles_TypeID == -1)
            {
			    bool is_first;
                B2FrameworkAssetBundles_TypeID = getTypeId(L, typeof(B2Framework.AssetBundles), out is_first);
				
				if (B2FrameworkAssetBundles_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(B2Framework.AssetBundles));
				    B2FrameworkAssetBundles_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, B2FrameworkAssetBundles_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, B2FrameworkAssetBundles_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for B2Framework.AssetBundles ,value="+val);
            }
			
			LuaAPI.lua_getref(L, B2FrameworkAssetBundles_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out B2Framework.AssetBundles val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkAssetBundles_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.AssetBundles");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for B2Framework.AssetBundles");
                }
				val = (B2Framework.AssetBundles)e;
                
            }
            else
            {
                val = (B2Framework.AssetBundles)objectCasters.GetCaster(typeof(B2Framework.AssetBundles))(L, index, null);
            }
        }
		
        public void UpdateB2FrameworkAssetBundles(RealStatePtr L, int index, B2Framework.AssetBundles val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkAssetBundles_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.AssetBundles");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for B2Framework.AssetBundles ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int B2FrameworkGameLanguage_TypeID = -1;
		int B2FrameworkGameLanguage_EnumRef = -1;
        
        public void PushB2FrameworkGameLanguage(RealStatePtr L, B2Framework.GameLanguage val)
        {
            if (B2FrameworkGameLanguage_TypeID == -1)
            {
			    bool is_first;
                B2FrameworkGameLanguage_TypeID = getTypeId(L, typeof(B2Framework.GameLanguage), out is_first);
				
				if (B2FrameworkGameLanguage_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(B2Framework.GameLanguage));
				    B2FrameworkGameLanguage_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, B2FrameworkGameLanguage_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, B2FrameworkGameLanguage_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for B2Framework.GameLanguage ,value="+val);
            }
			
			LuaAPI.lua_getref(L, B2FrameworkGameLanguage_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out B2Framework.GameLanguage val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkGameLanguage_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.GameLanguage");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for B2Framework.GameLanguage");
                }
				val = (B2Framework.GameLanguage)e;
                
            }
            else
            {
                val = (B2Framework.GameLanguage)objectCasters.GetCaster(typeof(B2Framework.GameLanguage))(L, index, null);
            }
        }
		
        public void UpdateB2FrameworkGameLanguage(RealStatePtr L, int index, B2Framework.GameLanguage val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkGameLanguage_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.GameLanguage");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for B2Framework.GameLanguage ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int B2FrameworkLoadState_TypeID = -1;
		int B2FrameworkLoadState_EnumRef = -1;
        
        public void PushB2FrameworkLoadState(RealStatePtr L, B2Framework.LoadState val)
        {
            if (B2FrameworkLoadState_TypeID == -1)
            {
			    bool is_first;
                B2FrameworkLoadState_TypeID = getTypeId(L, typeof(B2Framework.LoadState), out is_first);
				
				if (B2FrameworkLoadState_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(B2Framework.LoadState));
				    B2FrameworkLoadState_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, B2FrameworkLoadState_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, B2FrameworkLoadState_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for B2Framework.LoadState ,value="+val);
            }
			
			LuaAPI.lua_getref(L, B2FrameworkLoadState_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out B2Framework.LoadState val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkLoadState_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.LoadState");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for B2Framework.LoadState");
                }
				val = (B2Framework.LoadState)e;
                
            }
            else
            {
                val = (B2Framework.LoadState)objectCasters.GetCaster(typeof(B2Framework.LoadState))(L, index, null);
            }
        }
		
        public void UpdateB2FrameworkLoadState(RealStatePtr L, int index, B2Framework.LoadState val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkLoadState_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.LoadState");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for B2Framework.LoadState ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int B2FrameworkVerifyBy_TypeID = -1;
		int B2FrameworkVerifyBy_EnumRef = -1;
        
        public void PushB2FrameworkVerifyBy(RealStatePtr L, B2Framework.VerifyBy val)
        {
            if (B2FrameworkVerifyBy_TypeID == -1)
            {
			    bool is_first;
                B2FrameworkVerifyBy_TypeID = getTypeId(L, typeof(B2Framework.VerifyBy), out is_first);
				
				if (B2FrameworkVerifyBy_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(B2Framework.VerifyBy));
				    B2FrameworkVerifyBy_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, B2FrameworkVerifyBy_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, B2FrameworkVerifyBy_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for B2Framework.VerifyBy ,value="+val);
            }
			
			LuaAPI.lua_getref(L, B2FrameworkVerifyBy_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out B2Framework.VerifyBy val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkVerifyBy_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.VerifyBy");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for B2Framework.VerifyBy");
                }
				val = (B2Framework.VerifyBy)e;
                
            }
            else
            {
                val = (B2Framework.VerifyBy)objectCasters.GetCaster(typeof(B2Framework.VerifyBy))(L, index, null);
            }
        }
		
        public void UpdateB2FrameworkVerifyBy(RealStatePtr L, int index, B2Framework.VerifyBy val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkVerifyBy_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.VerifyBy");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for B2Framework.VerifyBy ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int B2FrameworkUStatus_TypeID = -1;
		int B2FrameworkUStatus_EnumRef = -1;
        
        public void PushB2FrameworkUStatus(RealStatePtr L, B2Framework.UStatus val)
        {
            if (B2FrameworkUStatus_TypeID == -1)
            {
			    bool is_first;
                B2FrameworkUStatus_TypeID = getTypeId(L, typeof(B2Framework.UStatus), out is_first);
				
				if (B2FrameworkUStatus_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(B2Framework.UStatus));
				    B2FrameworkUStatus_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, B2FrameworkUStatus_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, B2FrameworkUStatus_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for B2Framework.UStatus ,value="+val);
            }
			
			LuaAPI.lua_getref(L, B2FrameworkUStatus_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out B2Framework.UStatus val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkUStatus_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.UStatus");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for B2Framework.UStatus");
                }
				val = (B2Framework.UStatus)e;
                
            }
            else
            {
                val = (B2Framework.UStatus)objectCasters.GetCaster(typeof(B2Framework.UStatus))(L, index, null);
            }
        }
		
        public void UpdateB2FrameworkUStatus(RealStatePtr L, int index, B2Framework.UStatus val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkUStatus_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.UStatus");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for B2Framework.UStatus ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int B2FrameworkUAct_TypeID = -1;
		int B2FrameworkUAct_EnumRef = -1;
        
        public void PushB2FrameworkUAct(RealStatePtr L, B2Framework.UAct val)
        {
            if (B2FrameworkUAct_TypeID == -1)
            {
			    bool is_first;
                B2FrameworkUAct_TypeID = getTypeId(L, typeof(B2Framework.UAct), out is_first);
				
				if (B2FrameworkUAct_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(B2Framework.UAct));
				    B2FrameworkUAct_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, B2FrameworkUAct_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, B2FrameworkUAct_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for B2Framework.UAct ,value="+val);
            }
			
			LuaAPI.lua_getref(L, B2FrameworkUAct_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out B2Framework.UAct val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkUAct_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.UAct");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for B2Framework.UAct");
                }
				val = (B2Framework.UAct)e;
                
            }
            else
            {
                val = (B2Framework.UAct)objectCasters.GetCaster(typeof(B2Framework.UAct))(L, index, null);
            }
        }
		
        public void UpdateB2FrameworkUAct(RealStatePtr L, int index, B2Framework.UAct val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkUAct_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.UAct");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for B2Framework.UAct ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int B2FrameworkVariableEnum_TypeID = -1;
		int B2FrameworkVariableEnum_EnumRef = -1;
        
        public void PushB2FrameworkVariableEnum(RealStatePtr L, B2Framework.VariableEnum val)
        {
            if (B2FrameworkVariableEnum_TypeID == -1)
            {
			    bool is_first;
                B2FrameworkVariableEnum_TypeID = getTypeId(L, typeof(B2Framework.VariableEnum), out is_first);
				
				if (B2FrameworkVariableEnum_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(B2Framework.VariableEnum));
				    B2FrameworkVariableEnum_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, B2FrameworkVariableEnum_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, B2FrameworkVariableEnum_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for B2Framework.VariableEnum ,value="+val);
            }
			
			LuaAPI.lua_getref(L, B2FrameworkVariableEnum_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out B2Framework.VariableEnum val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkVariableEnum_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.VariableEnum");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for B2Framework.VariableEnum");
                }
				val = (B2Framework.VariableEnum)e;
                
            }
            else
            {
                val = (B2Framework.VariableEnum)objectCasters.GetCaster(typeof(B2Framework.VariableEnum))(L, index, null);
            }
        }
		
        public void UpdateB2FrameworkVariableEnum(RealStatePtr L, int index, B2Framework.VariableEnum val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkVariableEnum_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.VariableEnum");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for B2Framework.VariableEnum ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int B2FrameworkLuaScriptBindEnum_TypeID = -1;
		int B2FrameworkLuaScriptBindEnum_EnumRef = -1;
        
        public void PushB2FrameworkLuaScriptBindEnum(RealStatePtr L, B2Framework.LuaScriptBindEnum val)
        {
            if (B2FrameworkLuaScriptBindEnum_TypeID == -1)
            {
			    bool is_first;
                B2FrameworkLuaScriptBindEnum_TypeID = getTypeId(L, typeof(B2Framework.LuaScriptBindEnum), out is_first);
				
				if (B2FrameworkLuaScriptBindEnum_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(B2Framework.LuaScriptBindEnum));
				    B2FrameworkLuaScriptBindEnum_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, B2FrameworkLuaScriptBindEnum_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, B2FrameworkLuaScriptBindEnum_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for B2Framework.LuaScriptBindEnum ,value="+val);
            }
			
			LuaAPI.lua_getref(L, B2FrameworkLuaScriptBindEnum_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out B2Framework.LuaScriptBindEnum val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkLuaScriptBindEnum_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.LuaScriptBindEnum");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for B2Framework.LuaScriptBindEnum");
                }
				val = (B2Framework.LuaScriptBindEnum)e;
                
            }
            else
            {
                val = (B2Framework.LuaScriptBindEnum)objectCasters.GetCaster(typeof(B2Framework.LuaScriptBindEnum))(L, index, null);
            }
        }
		
        public void UpdateB2FrameworkLuaScriptBindEnum(RealStatePtr L, int index, B2Framework.LuaScriptBindEnum val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != B2FrameworkLuaScriptBindEnum_TypeID)
				{
				    throw new Exception("invalid userdata for B2Framework.LuaScriptBindEnum");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for B2Framework.LuaScriptBindEnum ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        
		// table cast optimze
		
        
    }
	
	public partial class StaticLuaCallbacks
    {
	    internal static bool __tryArrayGet(Type type, RealStatePtr L, ObjectTranslator translator, object obj, int index)
		{
		
			if (type == typeof(UnityEngine.Vector2[]))
			{
			    UnityEngine.Vector2[] array = obj as UnityEngine.Vector2[];
				translator.PushUnityEngineVector2(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Vector3[]))
			{
			    UnityEngine.Vector3[] array = obj as UnityEngine.Vector3[];
				translator.PushUnityEngineVector3(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Vector4[]))
			{
			    UnityEngine.Vector4[] array = obj as UnityEngine.Vector4[];
				translator.PushUnityEngineVector4(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Color[]))
			{
			    UnityEngine.Color[] array = obj as UnityEngine.Color[];
				translator.PushUnityEngineColor(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Quaternion[]))
			{
			    UnityEngine.Quaternion[] array = obj as UnityEngine.Quaternion[];
				translator.PushUnityEngineQuaternion(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Ray[]))
			{
			    UnityEngine.Ray[] array = obj as UnityEngine.Ray[];
				translator.PushUnityEngineRay(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Bounds[]))
			{
			    UnityEngine.Bounds[] array = obj as UnityEngine.Bounds[];
				translator.PushUnityEngineBounds(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Ray2D[]))
			{
			    UnityEngine.Ray2D[] array = obj as UnityEngine.Ray2D[];
				translator.PushUnityEngineRay2D(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.UI.CanvasScaler.ScaleMode[]))
			{
			    UnityEngine.UI.CanvasScaler.ScaleMode[] array = obj as UnityEngine.UI.CanvasScaler.ScaleMode[];
				translator.PushUnityEngineUICanvasScalerScaleMode(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode[]))
			{
			    UnityEngine.UI.CanvasScaler.ScreenMatchMode[] array = obj as UnityEngine.UI.CanvasScaler.ScreenMatchMode[];
				translator.PushUnityEngineUICanvasScalerScreenMatchMode(L, array[index]);
				return true;
			}
			else if (type == typeof(B2Framework.Platform[]))
			{
			    B2Framework.Platform[] array = obj as B2Framework.Platform[];
				translator.PushB2FrameworkPlatform(L, array[index]);
				return true;
			}
			else if (type == typeof(B2Framework.Scenes[]))
			{
			    B2Framework.Scenes[] array = obj as B2Framework.Scenes[];
				translator.PushB2FrameworkScenes(L, array[index]);
				return true;
			}
			else if (type == typeof(B2Framework.AssetBundles[]))
			{
			    B2Framework.AssetBundles[] array = obj as B2Framework.AssetBundles[];
				translator.PushB2FrameworkAssetBundles(L, array[index]);
				return true;
			}
			else if (type == typeof(B2Framework.GameLanguage[]))
			{
			    B2Framework.GameLanguage[] array = obj as B2Framework.GameLanguage[];
				translator.PushB2FrameworkGameLanguage(L, array[index]);
				return true;
			}
			else if (type == typeof(B2Framework.LoadState[]))
			{
			    B2Framework.LoadState[] array = obj as B2Framework.LoadState[];
				translator.PushB2FrameworkLoadState(L, array[index]);
				return true;
			}
			else if (type == typeof(B2Framework.VerifyBy[]))
			{
			    B2Framework.VerifyBy[] array = obj as B2Framework.VerifyBy[];
				translator.PushB2FrameworkVerifyBy(L, array[index]);
				return true;
			}
			else if (type == typeof(B2Framework.UStatus[]))
			{
			    B2Framework.UStatus[] array = obj as B2Framework.UStatus[];
				translator.PushB2FrameworkUStatus(L, array[index]);
				return true;
			}
			else if (type == typeof(B2Framework.UAct[]))
			{
			    B2Framework.UAct[] array = obj as B2Framework.UAct[];
				translator.PushB2FrameworkUAct(L, array[index]);
				return true;
			}
			else if (type == typeof(B2Framework.VariableEnum[]))
			{
			    B2Framework.VariableEnum[] array = obj as B2Framework.VariableEnum[];
				translator.PushB2FrameworkVariableEnum(L, array[index]);
				return true;
			}
			else if (type == typeof(B2Framework.LuaScriptBindEnum[]))
			{
			    B2Framework.LuaScriptBindEnum[] array = obj as B2Framework.LuaScriptBindEnum[];
				translator.PushB2FrameworkLuaScriptBindEnum(L, array[index]);
				return true;
			}
            return false;
		}
		
		internal static bool __tryArraySet(Type type, RealStatePtr L, ObjectTranslator translator, object obj, int array_idx, int obj_idx)
		{
		
			if (type == typeof(UnityEngine.Vector2[]))
			{
			    UnityEngine.Vector2[] array = obj as UnityEngine.Vector2[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Vector3[]))
			{
			    UnityEngine.Vector3[] array = obj as UnityEngine.Vector3[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Vector4[]))
			{
			    UnityEngine.Vector4[] array = obj as UnityEngine.Vector4[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Color[]))
			{
			    UnityEngine.Color[] array = obj as UnityEngine.Color[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Quaternion[]))
			{
			    UnityEngine.Quaternion[] array = obj as UnityEngine.Quaternion[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Ray[]))
			{
			    UnityEngine.Ray[] array = obj as UnityEngine.Ray[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Bounds[]))
			{
			    UnityEngine.Bounds[] array = obj as UnityEngine.Bounds[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Ray2D[]))
			{
			    UnityEngine.Ray2D[] array = obj as UnityEngine.Ray2D[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.UI.CanvasScaler.ScaleMode[]))
			{
			    UnityEngine.UI.CanvasScaler.ScaleMode[] array = obj as UnityEngine.UI.CanvasScaler.ScaleMode[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode[]))
			{
			    UnityEngine.UI.CanvasScaler.ScreenMatchMode[] array = obj as UnityEngine.UI.CanvasScaler.ScreenMatchMode[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(B2Framework.Platform[]))
			{
			    B2Framework.Platform[] array = obj as B2Framework.Platform[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(B2Framework.Scenes[]))
			{
			    B2Framework.Scenes[] array = obj as B2Framework.Scenes[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(B2Framework.AssetBundles[]))
			{
			    B2Framework.AssetBundles[] array = obj as B2Framework.AssetBundles[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(B2Framework.GameLanguage[]))
			{
			    B2Framework.GameLanguage[] array = obj as B2Framework.GameLanguage[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(B2Framework.LoadState[]))
			{
			    B2Framework.LoadState[] array = obj as B2Framework.LoadState[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(B2Framework.VerifyBy[]))
			{
			    B2Framework.VerifyBy[] array = obj as B2Framework.VerifyBy[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(B2Framework.UStatus[]))
			{
			    B2Framework.UStatus[] array = obj as B2Framework.UStatus[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(B2Framework.UAct[]))
			{
			    B2Framework.UAct[] array = obj as B2Framework.UAct[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(B2Framework.VariableEnum[]))
			{
			    B2Framework.VariableEnum[] array = obj as B2Framework.VariableEnum[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(B2Framework.LuaScriptBindEnum[]))
			{
			    B2Framework.LuaScriptBindEnum[] array = obj as B2Framework.LuaScriptBindEnum[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
            return false;
		}
	}
}
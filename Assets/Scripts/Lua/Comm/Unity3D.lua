-----------------------------------------------------------
-- FileName:    Unity3D.lua
-- Author:      Administrator
-- date:        2020-05-21 10:57:56
--
-- 1、这里定义C#的访问接口
-- 2、所有这些接口必须在Unity侧打上[LuaCallCSharp]标签或者配置在XLuaGenConfig的LuaCallCSharp列表，否则XLua将会用反射的形式访问
-- 3、尽量将经常访问的接口直接定义出来，避免多重访问
-----------------------------------------------------------

-- Unity API
UnityEngine         = CS.UnityEngine
GameObject          = UnityEngine.GameObject
Transform           = UnityEngine.Transform
Object              = UnityEngine.Object
AudioSource         = UnityEngine.AudioSource
AudioClip           = UnityEngine.AudioClip

-- B2Framework
UnityB2Framework    = CS.B2Framework.Unity              -- CS端框架命名空间
Game                = UnityB2Framework.Game             -- CS的全局访问接口
Loc                 = UnityB2Framework.Localization     -- 多语言接口:Loc.Get(key)
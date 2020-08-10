using System.Collections.Generic;
using System;
using UnityEngine;
using XLua;
using System.Reflection;
using System.Linq;

namespace B2Framework.Editor
{
    public static class LuaGenConfig
    {
        //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
        [LuaCallCSharp]
        public static List<Type> LuaCallCSharp = new List<Type>()
        {
            typeof(System.Object),
            typeof(UnityEngine.Object),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Quaternion),
            typeof(Color),
            typeof(Ray),
            typeof(Bounds),
            typeof(Ray2D),
            typeof(Time),
            typeof(GameObject),
            typeof(Component),
            typeof(Behaviour),
            typeof(Transform),
            typeof(Resources),
            typeof(TextAsset),
            typeof(Keyframe),
            typeof(AnimationCurve),
            typeof(AnimationClip),
            typeof(MonoBehaviour),
            typeof(ParticleSystem),
            typeof(SkinnedMeshRenderer),
            typeof(Renderer),
            typeof(Light),
            typeof(Mathf),
            typeof(System.Collections.Generic.List<int>),
            typeof(Action<string>),
            typeof(UnityEngine.Debug),

            // UGUI  
            typeof(UnityEngine.Canvas),
            typeof(UnityEngine.Rect),
            typeof(UnityEngine.RectTransform),
            typeof(UnityEngine.RectOffset),
            typeof(UnityEngine.Sprite),
            typeof(UnityEngine.UI.CanvasScaler),
            typeof(UnityEngine.UI.CanvasScaler.ScaleMode),
            typeof(UnityEngine.UI.CanvasScaler.ScreenMatchMode),
            typeof(UnityEngine.UI.GraphicRaycaster),
            typeof(UnityEngine.UI.Text),
            typeof(UnityEngine.UI.InputField),
            typeof(UnityEngine.UI.Button),
            typeof(UnityEngine.UI.Image),
            typeof(UnityEngine.UI.ScrollRect),
            typeof(UnityEngine.UI.Scrollbar),
            typeof(UnityEngine.UI.Toggle),
            typeof(UnityEngine.UI.ToggleGroup),
            typeof(UnityEngine.UI.Button.ButtonClickedEvent),
            typeof(UnityEngine.UI.ScrollRect.ScrollRectEvent),
            typeof(UnityEngine.UI.GridLayoutGroup),
            typeof(UnityEngine.UI.ContentSizeFitter),
            typeof(UnityEngine.UI.Slider),
        };
        // 动态配置
        [LuaCallCSharp]
        public static List<Type> LuaCallCSharp_Dynamic
        {
            get
            {
                List<string> namespaces = new List<string>() // 在这里添加名字空间
                {
                    "B2Framework",
                    "B2Framework.UI"
                };
                return (from type in Assembly.Load("Assembly-CSharp").GetTypes()
                        where type.Namespace != null && namespaces.Contains(type.Namespace)
                        select type).ToList();
            }
        }
        //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
        [CSharpCallLua]
        public static List<Type> CSharpCallLua = new List<Type>()
        {
            typeof(Action),
            typeof(Func<double, double, double>),
            typeof(Action<string>),
            typeof(Action<double>),
            typeof(Action<float>),
            typeof(Action<float, float>),
            typeof(Action<LuaTable>),
            typeof(Action<LuaTable, float, float>),
            typeof(Action<LuaTable, float>),
            typeof(Action<LuaTable, string, string>),
            typeof(UnityEngine.Events.UnityAction),
            typeof(System.Collections.IEnumerator),
            typeof(System.Action<UnityEngine.GameObject, int>)
        };
        //黑名单
        [BlackList]
        public static List<List<string>> BlackList = new List<List<string>>()
        {
                    new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
                    new List<string>(){"UnityEngine.WWW", "movie"},
        #if UNITY_WEBGL
                    new List<string>(){"UnityEngine.WWW", "threadPriority"},
        #endif
                    new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                    new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                    new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                    new List<string>(){"UnityEngine.Light", "areaSize"},
                    new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
                    new List<string>(){"UnityEngine.WWW", "MovieTexture"},
                    new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
                    new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
        #if !UNITY_WEBPLAYER
                    new List<string>(){"UnityEngine.Application", "ExternalEval"},
        #endif
                    new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                    new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                    new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                    new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                    new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                    new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                    new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                    new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                    new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
        };
        //黑名单
        [BlackList]
        public static List<List<string>> BlackList_Ex = new List<List<string>>()
        {
            new List<string>(){"UnityEngine.Light", "SetLightDirty"},
            new List<string>(){"UnityEngine.Light", "shadowRadius"},
            new List<string>(){"UnityEngine.Light", "shadowAngle"},
            new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},
        };
    }
}
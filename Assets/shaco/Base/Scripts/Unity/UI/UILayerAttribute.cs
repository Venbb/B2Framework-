using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    /// <summary>
    /// 允许打开多个重复的ui
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class UILayerAllowDuplicateAttribute : System.Attribute
    {

    }

    /// <summary>
    /// 自定义prefab加载路径，如果没有该属性则根据ui类型自动识别路径
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class UILayerCustomPrefabPathAttribute : System.Attribute
    {
        public string customPrefabPath { get { return _customPrefabPath; } }
        private string _customPrefabPath = string.Empty;

        public UILayerCustomPrefabPathAttribute(string customPrefabPath)
        {
            this._customPrefabPath = customPrefabPath;
        }
    }

    /// <summary>
    /// 资源版本相对路径，用于多资源版本管理，如果填空或者不设定该属性，则默认为全局资源
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class UILayerMultiVersionControlRelativePathAttribute : System.Attribute
    {
        public string multiVersionControlRelativePath { get { return _multiVersionControlRelativePath; } }
        private string _multiVersionControlRelativePath = string.Empty;

        public UILayerMultiVersionControlRelativePathAttribute(string multiVersionControlRelativePath)
        {
            this._multiVersionControlRelativePath = multiVersionControlRelativePath;
        }
    }

    /// <summary>
    /// UI所在根节点下标属性
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class UILayerIndexAttribute : System.Attribute
    {
        //ui所在UIRoot的下标
        public int layerIndex { get { return _layerIndex; } }
        private int _layerIndex = 0;

        public UILayerIndexAttribute(int layerIndex)
        {
            this._layerIndex = layerIndex;
        }
    }

    /// <summary>
    /// 修改UI加载规则为异步加载
    /// ps: 这会导致OpenUI返回的对象多半是null，因为ui不可能在同一桢中加载出来
    ///     如需获取ui对象，建议监听shaco.UIStateChangedEvents.OnUIOpenEndEvent事件
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class UILayerOpenAsyncAttribute : System.Attribute
    {

    }

    /// <summary>
    /// 自定义的界面打开超时时长
    /// 当没有设置该属性的时候默认使用UIRootComponent.DEFAULT_OPEN_UI_TIMEOUT_SECONDS
    /// 当发生UI打开超时时候，会通知shaco.UIStateChangedEvents.OpenUITimeoutStartEvent(超时开始，UI还在加载中)
    ///                       和shaco.UIStateChangedEvents.OpenUITimeoutEndEvent(超时结束，UI加载完毕)事件
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class UILayerCustomOpenTimeoutSecondsAttribute : System.Attribute
    {
        public float seconds { get { return _seconds; } }
        private float _seconds = 1.0f;

        public UILayerCustomOpenTimeoutSecondsAttribute(float seconds)
        {
            _seconds = seconds;
        }
    }

    /// <summary>
    /// 标记为全屏UI
    /// case1: 当该UI打开的时候会自动隐藏上一层的全屏UI
    /// case2: 当上层全屏UI销毁或者隐藏时候，自动恢复该UI显示
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class UILayerFullScreenAttribute : System.Attribute
    {

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public interface IUIManager : shaco.Base.IGameInstance
    {
        /// <summary>
        /// 打开一个该类型UI，自动显示到最上层
        /// <param name="arg">参数</param>
        /// <return>UI对象</return>
        /// </summary>
        T OpenUI<T>(shaco.Base.BaseEventArg arg = null) where T : UnityEngine.Component;

        /// <summary>
        /// 隐藏一组类型UI
        /// </summary>
        void HideUI<T>() where T : UnityEngine.Component;

        /// <summary>
        /// 关闭一组类型UI，自动显示到最上层
        /// </summary>
        void CloseUI<T>() where T : UnityEngine.Component;

        /// <summary>
        /// 刷新一组类型UI，不会影响显示层级，仅仅刷新界面数据
        /// <param name="arg">参数</param>
        /// </summary>
        void RefreshUI<T>(shaco.Base.BaseEventArg arg = null) where T : UnityEngine.Component;

        /// <summary>
        /// 将UI显示到最上层
        /// </summary>
        void BringToFront<T>() where T : UnityEngine.Component;

        /// <summary>
        /// 隐藏指定的一个ui对象
        /// <param name="target">ui对象</param>
        /// </summary>
        void HideUITarget(UnityEngine.Component target);

        /// <summary>
        /// 关闭指定的一个ui对象
        /// <param name="target">ui对象</param>
        /// </summary>
        void CloseUITarget(UnityEngine.Component target);

        /// <summary>
        /// 将UI显示到最上层
        /// </summary>
        void BringToFrontTarget(UnityEngine.Component target);

        /// <summary>
        /// 预加载一组类型UI
        /// <param name="preloadCount">预加载该类型ui数量</param>
        /// </summary>
        void PreLoadUI<T>(int preloadCount = 1) where T : UnityEngine.Component;

        /// <summary>
        /// 预加载一个该类型UI
        /// </summary>
        void PreLoadUIOnlyOne<T>() where T : UnityEngine.Component;

		/// <summary>
		/// 获取该类型UI预加载数量
		/// <return>UI预加载数量</return>
		/// </summary>
        int GetPreLoadUICount<T>() where T : UnityEngine.Component;

        /// <summary>
        /// 弹出并隐藏显示在最上层的UI
        /// <param name="layerIndex">UI根节点下标</param>
        /// <return></return>
        /// </summary>
        shaco.IUIState PopupUIAndHide(int layerIndex = 0, params System.Type[] igoreUIs);

        /// <summary>
        /// 弹出并关闭显示在最上层的UI
        /// <param name="layerIndex">UI根节点下标</param>
        /// <return></return>
        /// </summary>
        shaco.IUIState PopupUIAndClose(bool onlyActivedUI, int layerIndex = 0, params System.Type[] igoreUIs);

        /// <summary>
        /// 恢复ui根节点下所有ui显示
        /// <param name="layerIndex">UI根节点下标</param>
        /// </summary>
        void ResumeAllUI(int layerIndex);

        /// <summary>
        /// 隐藏ui根节点下所有ui
        /// <param name="layerIndex">UI根节点下标</param>
        /// </summary>
        void HideAllUI(int layerIndex);

        /// <summary>
        /// 关闭ui根节点下所有ui
        /// <param name="layerIndex">UI根节点下标</param>
        /// </summary>
        void CloseAllUI(int layerIndex);

        /// <summary>
        /// 添加一个UI根节点
        /// <param name="uiRoot">UI根节点对象</param>
        /// </summary>
        void AddUIRootComponent(IUIRootComponent uiRoot);

        /// <summary>
        /// 移除一个UI根节点，自动关闭它下面所有UI
        /// <param name="uiRoot">UI根节点对象</param>
        /// </summary>
        void RemoveUIRootComponent(IUIRootComponent uiRoot);

        /// <summary>
        /// 获取显示在最上层的UI
        /// <param name="onlyActivedUI">true获取只显示的ui对象</param>
        /// <param name="layerIndex">UI根节点下标</param>
        /// <return>UI对象数据</return>
        /// </summary>
        GameObject GetTopUI(bool onlyActivedUI = true, int layerIndex = 0);

        /// <summary>
        /// 判断是为最上层UI
        /// <param name="uiName">ui名字</param>
        /// <param name="onlyActivedUI">true获取只显示的ui对象</param>
        /// </summary>
        bool IsTopUI(string uiName, bool onlyActivedUI = true);

        /// <summary>
        /// 根据UI类型，获取数组数据最靠前的一个UI对象
        /// <param name="onlyActivedUI">true获取只显示的ui对象</param>
        /// <return>UI对象</return>
        /// </summary>
        T GetUIComponent<T>(bool onlyActivedUI = true) where T : UnityEngine.Component;

        /// <summary>
        /// 根据UI类型，获取所有该类型的UI对象
        /// <param name="onlyActivedUI">true获取只显示的ui对象</param>
        /// <return>该类型所有UI对象</return>
        /// </summary>
        List<T> GetUIComponents<T>(bool onlyActivedUI = true) where T : UnityEngine.Component;

        /// <summary>
        /// 根据UI名字，获取数组数据最靠前的一个UI对象
        /// <param name="uiType">ui类型</param>
        /// <param name="onlyActivedUI">true获取只显示的ui对象</param>
        /// </summary>
        UnityEngine.Component GetUIComponent(System.Type uiType, bool onlyActivedUI = true);

        /// <summary>
        /// 根据UI名字，获取所有该类型的UI对象
        /// <param name="uiType">ui类型</param>
        /// <param name="onlyActivedUI">true获取只显示的ui对象</param>
        /// </summary>
        List<UnityEngine.Component> GetUIComponents(System.Type uiType, bool onlyActivedUI = true);

        /// <summary>
        /// 根据Unity标记的tag获取一个UI对象
        /// <param name="tag">UnityEngine.GameObject.tag</param>
        /// <return>UI对象</return>
        /// </summary>
        GameObject GetUIGameObjectWithTag(string tag);

        /// <summary>
        /// 根据Unity标记的tag获取所有该tag的UI对象
        /// <param name="tag">UnityEngine.GameObject.tag</param>
        /// <return>UI对象</return>
        /// </summary>
        List<GameObject> GetUIGameObjectsWithTag(string tag);

        /// <summary>
        /// 根据Unity标记的layer获取一个UI对象
        /// <param name="layer">UnityEngine.GameObject.layer</param>
        /// <return>UI对象</return>
        /// </summary>
        GameObject GetUIGameObjectWithLayer(int layer);

        /// <summary>
        /// 根据Unity标记的layer获取该layer的UI对象
        /// <param name="layer">UnityEngine.GameObject.layer</param>
        /// <return>UI对象</return>
        /// </summary>
        List<GameObject> GetUIGameObjectsWithLayer(int layer);

        /// <summary>
        /// 遍历所有ui根节点
        /// <param name="callback">遍历结果回调</param>
        /// </summary>
        void ForeachActiveUIRoot(System.Action<IUIRootComponent> callback);

        /// <summary>
        /// 获取ui根节点数量
        /// <return>ui根节点数量</return>
        /// </summary>
        int GetUIRootCount();

        /// <summary>
        /// 获取ui根节点
        /// <param name="layerIndex">UI根节点下标</param>
        /// <return>ui根节点</return>
        /// </summary>
        IUIRootComponent GetUIRootComponent(int layerIndex = 0);

		/// <summary>
		/// 清空关闭所有ui
		/// </summary>
        void ClearUI();

        /// <summary>
        /// 是否还有UI在加载中
        /// </summary>
        bool IsUILoading();

        /// <summary>
        /// 设置界面属性开关
        /// <param name="attributeType">属性类型</param>
        /// <param name="isOn"是否开启</param>
        /// </summary>
        void SetLayerAttributeOn<T>(System.Type attributeType, bool isOn) where T : UnityEngine.Component;

        /// <summary>
        /// 设置所有界面属性开关
        /// <param name="attributeType">属性类型</param>
        /// <param name="isOn"是否开启</param>
        /// </summary>
        void SetAllLayerAttributeOn(System.Type attributeType, bool isOn);
    }
}
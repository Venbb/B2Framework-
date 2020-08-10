using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public interface IUIRootComponent
    {
        /// <summary>
        /// ui根节点下标
        /// </summary>
        int layerIndex { get; }

        /// <summary>
        /// ui数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 正在加载中的ui数量
        /// </summary>
        int loadingUICount { get; }

        /// <summary>
        /// ui名字
        /// </summary>
        string uiName { get; set; }

        /// <summary>
        /// ui是否为显示状态
        /// </summary>
        bool isActived { get; }

        /// <summary>
        /// 打开一个该类型UI，自动显示到最上层
        /// <param name="arg">参数</param>
        /// <return>UI对象</return>
        /// </summary>
        T OpenUI<T>(string multiVersionControlRelativePath, shaco.Base.BaseEventArg arg = null) where T : UnityEngine.Component;

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
        void PreLoadUI<T>(string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, int preloadCount = 1) where T : UnityEngine.Component;

        /// <summary>
        /// 获取该类型UI预加载数量
        /// <return>UI预加载数量</return>
        /// </summary>
        int GetPreLoadUICount<T>() where T : UnityEngine.Component;

        IUIState PopupUIAndHide(params System.Type[] igoreUIs);

        IUIState PopupUIAndClose(bool onlyActivedUI, params System.Type[] igoreUIs);

        /// <summary>
        /// 恢复ui根节点下所有ui显示
        /// <param name="layerIndex">UI根节点下标</param>
        /// </summary>
        void ResumeAllUI();

        /// <summary>
        /// 隐藏ui根节点下所有ui
        /// <param name="layerIndex">UI根节点下标</param>
        /// </summary>
        void HideAllUI();

        /// <summary>
        /// 关闭ui根节点下所有ui
        /// <param name="layerIndex">UI根节点下标</param>
        /// </summary>
        void CloseAllUI();

        UnityEngine.GameObject GetTopUI(bool onlyActivedUI);

        IUIState GetUIState(System.Type uiType);

        IUIState[] GetAllUIState();

        void Foreach(System.Func<IUIState, bool> callback);

        void ClearUI();

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
        /// 将指定的一个ui对象显示在最上层
        /// <param name="target">ui对象</param>
        /// </summary>
        void BringToFrontTarget(UnityEngine.Component target);

        /// <summary>
        /// 设置界面属性开关
        /// <param name="attributeType">属性类型</param>
        /// <param name="isOn"是否开启</param>
        /// </summary>
        void SetLayerAttributeOn(System.Type attributeType, bool isOn);
    }
}
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    /// <summary>
    /// 默认的UI显示深度变化方法，以UGUI为准
    /// </summary>
    public class DefaultUIDepthChange : IUIDepthChange
    {
        private class DepthData
        {
            public UnityEngine.UI.GraphicRaycaster graphicRaycaster = null;
            public UnityEngine.Canvas canvas = null; 
        }

        /// <summary>
        /// <param name="uiRoot">ui根结点</param>
        /// <param name="state">ui状态信息</param>
        /// </summary>
        public void ChangeDepthAsTopDisplay(IUIRootComponent uiRoot, IUIState state)
        {
            var currentParentNode = state.parent.transform;

            //设置渲染到最高层
            currentParentNode.SetAsLastSibling();

            //刷新同一父节点下的所有UI对象的cavans层级
            var uiRootCompnent = uiRoot as Component;
            if (null != uiRootCompnent && uiRootCompnent.transform.childCount > 0)
            {
                for (int i = uiRootCompnent.transform.childCount - 1; i >= 0; --i)
                {
                    var otherParentNode = uiRootCompnent.transform.GetChild(i).gameObject;
                    var canvasFind = otherParentNode.GetComponent<UnityEngine.Canvas>();
                    if (null == canvasFind)
                    {
                        otherParentNode.GetOrAddComponent<UnityEngine.UI.GraphicRaycaster>();
                        canvasFind = otherParentNode.GetOrAddComponent<UnityEngine.Canvas>();
                        canvasFind.overrideSorting = true;
                        canvasFind.sortingLayerName = LayerMask.LayerToName(uiRootCompnent.gameObject.layer);
                    }
                    canvasFind.sortingOrder = (uiRoot.layerIndex * 100) + i;
                }
            }
        }
    }
}
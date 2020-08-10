using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    /// <summary>
    /// UI的显示深度变化
    /// </summary>
    public interface IUIDepthChange : shaco.Base.IGameInstance
    {
        /// <summary>
        /// <param name="uiRoot">ui根结点</param>
        /// <param name="state">ui状态信息</param>
        /// </summary>
        void ChangeDepthAsTopDisplay(IUIRootComponent uiRoot, IUIState state);
    }
}
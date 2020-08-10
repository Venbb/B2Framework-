using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 模拟真实平台操作事件类
    /// </summary>
    public interface ISimulatePlatformEvent : IGameInstance
    {
        /// <summary>
        /// 点击对象
        /// <param name="name">对象名字</param>
        /// <return>是否点击中对象</return>
        /// </summary>
        bool PressGameObject(string name);

        // /// <summary>
        // /// 触摸滚动屏幕
        // /// <param name="name">对象名字</param>
        // /// <param name="startX">触摸开始x坐标</param>
        // /// <param name="startY">触摸开始y坐标</param>
        // /// <param name="endX">触摸结束x坐标</param>
        // /// <param name="endY">触摸结束y坐标</param>
        // /// </summary>
        // bool DragPosition(string name, float startX, float startY, float endX, float endY);
    }
}
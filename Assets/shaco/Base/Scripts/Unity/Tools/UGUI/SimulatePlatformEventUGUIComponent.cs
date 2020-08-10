using UnityEngine;
using UnityEngine.EventSystems;

namespace shaco
{
    public class SimulatePlatformEventUGUIComponent : shaco.Base.ISimulatePlatformEvent
    {
        public UnityEngine.UI.GraphicRaycaster raycaster;

        /// <summary>
        /// 点击对象
        /// <param name="name">对象名字</param>
        /// <return>是否点击中对象</return>
        /// </summary>
        public bool PressGameObject(string name)
        {
            var gameObjectTarget = GameObject.Find(name);
            if (null != gameObjectTarget)
            {
                gameObjectTarget = ExecuteEvents.ExecuteHierarchy(gameObjectTarget, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            }
            return null != gameObjectTarget;
        }

        // /// <summary> 目前暂时无该功能需求，暂时不做了
        // /// 触摸滚动屏幕
        // /// <param name="name">对象名字</param>
        // /// <param name="startX">触摸开始x坐标</param>
        // /// <param name="startY">触摸开始y坐标</param>
        // /// <param name="endX">触摸结束x坐标</param>
        // /// <param name="endY">触摸结束y坐标</param>
        // /// </summary>
        // public bool DragPosition(string name, float startX, float startY, float endX, float endY)
        // {
        //     var gameObjectTarget = GameObject.Find(name);
        //     if (null != gameObjectTarget)
        //     {
        //         var startPosition = new Vector2(startX, startY);
        //         var endPosition = new Vector2(endX, endY);
        //         gameObjectTarget = ExecuteEvents.ExecuteHierarchy(gameObjectTarget, new PointerEventData(EventSystem.current)
        //         {
        //             position = startPosition
        //         }, ExecuteEvents.beginDragHandler);
        //         gameObjectTarget = ExecuteEvents.ExecuteHierarchy(gameObjectTarget, new PointerEventData(EventSystem.current)
        //         {
        //             position = endPosition
        //         }, ExecuteEvents.dragHandler);
        //         gameObjectTarget = ExecuteEvents.ExecuteHierarchy(gameObjectTarget, new PointerEventData(EventSystem.current)
        //         {
        //             position = endPosition
        //         }, ExecuteEvents.endDragHandler);
        //     }
        //     return null != gameObjectTarget;
        // }
    }	
}
using UnityEngine;
using UnityEngine.EventSystems;

namespace shaco
{
    public class SimulatePlatformEventDefault : shaco.Base.ISimulatePlatformEvent
    {
        /// <summary>
        /// 点击对象
        /// <param name="name">对象名字</param>
        /// <return>是否点击中对象</return>
        /// </summary>
        public bool PressGameObject(string name)
        {
            throw new System.NotImplementedException("SimulatePlatformEventDefault.PressGameObject");
        }
    }	
}
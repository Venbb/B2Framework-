using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace shaco
{
    public class MethodInfoEx
    {
        public object target = null;
        public MethodInfo method = null;
    }

    [System.Serializable]
    public class UIPrefab
    {
        public bool isInited = false;
        public GameObject prefab = null;
        public Component mainComponent = null;
        public Component[] componets = null;
        public List<MethodInfoEx> methodOnPreLoad = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnInit = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnOpen = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnHide = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnResume = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnClose = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnRefresh = new List<MethodInfoEx>(0);
        public List<MethodInfoEx> methodOnBringToFront = new List<MethodInfoEx>(0);
        public bool isAnimation = false;

        public void ClearAllMethod()
        {
            methodOnPreLoad.Clear();
            methodOnInit.Clear();
            methodOnOpen.Clear();
            methodOnHide.Clear();
            methodOnResume.Clear();
            methodOnClose.Clear();
            methodOnRefresh.Clear();
            methodOnBringToFront.Clear();
        }

        public void SetComponents(params Component[] components)
        {
            if (null == components)
                return;

            this.componets = components;

            if (null != components)
            {
                if (1 == components.Length)
                    mainComponent = components[0];
                else
                {
                    //当出现多个组件的时候，找到其中一个名字相同的组件
                    for (int i = 0; i < componets.Length; ++i)
                    {
                        if (components[i].gameObject.name.StartsWith(componets[i].GetType().Name))
                        {
                            mainComponent = components[i];
                            break;
                        }
                    }

                    //当没有匹配名字的组件时候使用第一个组件
                    if (null == mainComponent)
                        mainComponent = components[0];
                }
            }
            this.prefab = componets.Length > 0 ? componets[0].gameObject : null;
        }
    }
}


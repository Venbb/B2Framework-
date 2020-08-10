//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2017 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

#if DEBUG_WINDOW
using GameFramework.Debugger;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private abstract class ScrollableDebuggerWindowBase : IDebuggerWindow
        {
            //因为TouchScrollView不支持嵌套，所以子窗口如果再使用了该组件，应该继承并设置该属性为false
            virtual public bool useTouchScrollView { get { return true; } }

            private Vector2 m_ScrollPosition = Vector2.zero;
            //add by shaco 2020/2/18
            private shaco.TouchScroll.TouchScrollView _touchScrollView = new shaco.TouchScroll.TouchScrollView();
            //add end

            public virtual void Initialize(params object[] args)
            {

            }

            public virtual void Shutdown()
            {

            }

            public virtual void OnEnter()
            {

            }

            public virtual void OnLeave()
            {

            }

            public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {

            }

            public void OnDraw()
            {
                var lastRect = GUILayoutUtility.GetLastRect();

                if (useTouchScrollView)
                {
                    _touchScrollView.BeginScrollView();
                }

                OnDrawScrollableWindow(lastRect);

                if (useTouchScrollView)
                    _touchScrollView.EndScrollView();
            }

            protected abstract void OnDrawScrollableWindow(Rect lastGUIRect);

            protected void DrawItem(string title, string content)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(title, GUILayout.Width(DebuggerComponent.DefaultWindowRect.width * 0.25f));
                    GUILayout.Label(content);
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}
#endif
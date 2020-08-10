//------------------------------------------------------------
// Shaco Framework v1.7.1
// Copyright © 2017-2020 shaco. All rights reserved.
// Feedback: mailto:449612236@qq.com
//------------------------------------------------------------

#if DEBUG_WINDOW
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private sealed class ObjectPoolWindow : ScrollableDebuggerWindowBase
        {
            override public bool useTouchScrollView { get { return false; } }

            private string _searcName = string.Empty;
            private string _searcNameLower = string.Empty;
            private int _maxShowCount = 100;
            private string _maxShowCountStr = "100";
            private Rect _lastRect;
            private shaco.TouchScroll.TouchScrollView _touchScrollViewLeft = new shaco.TouchScroll.TouchScrollView();
            private shaco.TouchScroll.TouchScrollView _touchScrollViewRight = new shaco.TouchScroll.TouchScrollView();

            public override void OnLeave()
            {
                _searcName = string.Empty;
                _searcNameLower = string.Empty;
            }

            protected override void OnDrawScrollableWindow(Rect lastGUIRect)
            {
                float widthGUITmp = DebuggerComponent.DefaultWindowRect.width / 16;

                GUILayout.BeginHorizontal("box");
                {
                    GUILayout.FlexibleSpace();

                    GUILayout.Label("Search Name: ");
                    GUI.changed = false;
                    _searcName = GUILayout.TextField(_searcName, GUILayout.Width(widthGUITmp * 2));
                    if (GUI.changed)
                    {
                        _searcNameLower = _searcName.ToLower();
                    }

                    GUILayout.Label("Max Show Count: ");
                    GUI.changed = false;
                    _maxShowCountStr = GUILayout.TextField(_maxShowCountStr, GUILayout.Width(widthGUITmp));
                    if (GUI.changed)
                    {
                        int tmpCount = 0;
                        if (int.TryParse(_maxShowCountStr, out tmpCount))
                            _maxShowCount = tmpCount;
                        else
                            _maxShowCountStr = _maxShowCount.ToString();
                    }
                }
                GUILayout.EndHorizontal();

                if (null != Event.current && Event.current.type == EventType.Repaint)
                {
                    _lastRect = GUILayoutUtility.GetLastRect();
                }

                var rectLeft = new Rect(0, _lastRect.yMax, DebuggerComponent.DefaultWindowRect.width / 2, DebuggerComponent.DefaultWindowRect.height - _lastRect.yMax - lastGUIRect.yMax);
                var rectRight = new Rect(rectLeft.width, _lastRect.yMax, rectLeft.width, rectLeft.height);

                //绘制边框
                GUI.Box(rectLeft, string.Empty);
                GUI.Box(rectRight, string.Empty);

                //绘制左边窗口
                GUILayout.BeginArea(rectLeft);
                {
                    GUILayout.Label("Instantiated Count:" + shaco.GameHelper.objectpool.instantiatedCount);
                    _touchScrollViewLeft.BeginScrollView();
                    {
                        DrawObjectPoolItems(shaco.GameHelper.objectpool.ForeachInstantiatePool, rectLeft.width);
                    }
                    _touchScrollViewLeft.EndScrollView();
                }
                GUILayout.EndArea();

                //绘制右边窗口
                GUILayout.BeginArea(rectRight);
                {
                    GUILayout.Label("Unused Count:" + shaco.GameHelper.objectpool.unsuedCount);
                    _touchScrollViewRight.BeginScrollView();
                    {
                        DrawObjectPoolItems(shaco.GameHelper.objectpool.ForeacUnusedPool, rectRight.width);
                    }
                    _touchScrollViewRight.EndScrollView();
                }
                GUILayout.EndArea();
            }

            private void DrawObjectPoolItems(System.Action<System.Func<string, System.Collections.Generic.List<shaco.Base.PoolDataInfo>, bool>> foreachFunction, float width)
            {
                int showCountTmp = 0;
                foreachFunction((name, poolDatas) =>
                {
                    if (!string.IsNullOrEmpty(_searcNameLower))
                    {
                        if (!name.ToLower().Contains(_searcNameLower))
                            return true;
                    }

                    if (++showCountTmp > _maxShowCount)
                        return false;

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(name);
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(poolDatas.Count.ToString());
                    }
                    GUILayout.EndHorizontal();
                    return true;
                });
            }
        }
    }
}
#endif
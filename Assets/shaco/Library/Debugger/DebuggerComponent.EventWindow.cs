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
        private sealed class EventWindow : ScrollableDebuggerWindowBase
        {
            override public bool useTouchScrollView { get { return false; } }

            private string _searcName = string.Empty;
            private string _searcNameLower = string.Empty;
            private int _maxShowCount = 100;
            private string _maxShowCountStr = "100";
            private Rect _lastRect;
            private shaco.TouchScroll.TouchScrollView _touchScrollViewLeft = new shaco.TouchScroll.TouchScrollView();
            private shaco.TouchScroll.TouchScrollView _touchScrollViewRight = new shaco.TouchScroll.TouchScrollView();
            private string _currentSelectEventID = string.Empty;
            private shaco.Base.EventCallBackInfo _currentSelectEventInfo = null;

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
                        _currentSelectEventID = string.Empty;
                        _currentSelectEventInfo = null;
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

                var rectLeft = new Rect(0, _lastRect.yMax, DebuggerComponent.DefaultWindowRect.width / 4, DebuggerComponent.DefaultWindowRect.height - _lastRect.yMax - lastGUIRect.yMax);
                var rectRight = new Rect(rectLeft.width, _lastRect.yMax, DebuggerComponent.DefaultWindowRect.width - rectLeft.width, rectLeft.height);

                //绘制边框
                GUI.Box(rectLeft, string.Empty);
                GUI.Box(rectRight, string.Empty);

                //绘制左边窗口
                GUILayout.BeginArea(rectLeft);
                {
                    GUILayout.Label("Event Count:" + shaco.GameHelper.Event.Count);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Event ID");
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Count");
                    }
                    GUILayout.EndHorizontal();

                    _touchScrollViewLeft.BeginScrollView();
                    {
                        int showCountTmp = 0;
                        shaco.GameHelper.Event.Foreach((eventID, callbackInfo) =>
                        {
                            if (!string.IsNullOrEmpty(_searcNameLower))
                            {
                                if (!eventID.ToLower().Contains(_searcNameLower))
                                    return true;
                            }

                            if (++showCountTmp > _maxShowCount)
                                return false;

                            GUILayout.BeginHorizontal();
                            {
                                if (GUILayout.Toggle(_currentSelectEventID == eventID, eventID ))
                                {
                                    _currentSelectEventID = eventID;
                                    _currentSelectEventInfo = callbackInfo;
                                }

                                GUILayout.FlexibleSpace();
                                GUILayout.Label(callbackInfo.CallBack.Count.ToString());
                            }
                            GUILayout.EndHorizontal();
                            return true;
                        });
                    }
                    _touchScrollViewLeft.EndScrollView();
                }
                GUILayout.EndArea();

                //绘制右边窗口
                GUILayout.BeginArea(rectRight);
                {
                    var guilayoutPerWidth = GUILayout.Width(rectRight.width / 3);
                    GUILayout.Label("Listener Count:" + (null != _currentSelectEventInfo ? _currentSelectEventInfo.CallBack.Count : 0));
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Default Sender", guilayoutPerWidth);
                        GUILayout.Label("Target", guilayoutPerWidth);
                        GUILayout.Label("Method", guilayoutPerWidth);
                    }
                    GUILayout.EndHorizontal();

                    _touchScrollViewRight.BeginScrollView();
                    {
                        if (null != _currentSelectEventInfo)
                        {
                            var callbacksInfoTmp = _currentSelectEventInfo.CallBack;
                            var loopCount = System.Math.Min(_maxShowCount, callbacksInfoTmp.Count);
                            for (int i = 0; i < loopCount; ++i)
                            {
                                var infoTmp = callbacksInfoTmp[i];
                                GUILayout.BeginHorizontal("box");
                                {
                                    GUILayout.Label(infoTmp.DefaultSender.ToString(), guilayoutPerWidth);
                                    GUILayout.Label(infoTmp.CallFunc.Target.ToString(), guilayoutPerWidth);
                                    GUILayout.Label(infoTmp.CallFunc.Method.ToString(), guilayoutPerWidth);
                                }
                                GUILayout.EndHorizontal();

                                if (i != callbacksInfoTmp.Count - 1)
                                {
                                    DrawSeparator();
                                }
                            }
                        }
                    }
                    _touchScrollViewRight.EndScrollView();
                }
                GUILayout.EndArea();
            }

            static private void DrawSeparator()
            {
                GUILayout.Space(5);
                GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(3));
                GUILayout.Space(5);
            }
        }
    }
}
#endif
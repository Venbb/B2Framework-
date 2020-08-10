//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2017 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

#if DEBUG_WINDOW
using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private sealed class GeneralSettingsWindow : ScrollableDebuggerWindowBase
        {
            private DebuggerComponent m_DebuggerComponent = null;
            private BaseComponent m_BaseComponent = null;
            // private float m_LastIconX = 0f;
            // private float m_LastIconY = 0f;
            // private float m_LastWindowX = 0f;
            // private float m_LastWindowY = 0f;
            // private float m_LastWindowWidth = 0f;
            // private float m_LastWindowHeight = 0f;

            public override void Initialize(params object[] args)
            {
                m_DebuggerComponent = shaco.GameEntry.GetComponentInstance<DebuggerComponent>();
                if (m_DebuggerComponent == null)
                {
                    shaco.Log.Exception("Debugger component is invalid.");
                    return;
                }

                m_BaseComponent = shaco.GameEntry.GetComponentInstance<BaseComponent>();
                if (m_BaseComponent == null)
                {
                    shaco.Log.Exception("Base component is invalid.");
                    return;
                }
                // m_LastIconX = m_SettingComponent.GetFloat("Debugger.Icon.X", DefaultIconRect.x);
                // m_LastIconY = m_SettingComponent.GetFloat("Debugger.Icon.Y", DefaultIconRect.y);
                // m_LastWindowX = m_SettingComponent.GetFloat("Debugger.Window.X", DefaultWindowRect.x);
                // m_LastWindowY = m_SettingComponent.GetFloat("Debugger.Window.Y", DefaultWindowRect.y);
                // m_LastWindowWidth = m_SettingComponent.GetFloat("Debugger.Window.Width", DefaultWindowRect.width);
                // m_LastWindowHeight = m_SettingComponent.GetFloat("Debugger.Window.Height", DefaultWindowRect.height);
                // m_DebuggerComponent.WindowScale = m_LastWindowScale = shaco.GameHelper.datasave.ReadFloat("Debugger.Window.Scale", DefaultWindowScale);
                // m_DebuggerComponent.IconRect = new Rect(m_LastIconX, m_LastIconY, DefaultIconRect.width, DefaultIconRect.height);
                // m_DebuggerComponent.WindowRect = new Rect(m_LastWindowX, m_LastWindowY, m_LastWindowWidth, m_LastWindowHeight);
            }

            public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                // if (m_LastIconX != m_DebuggerComponent.IconRect.x)
                // {
                //     m_LastIconX = m_DebuggerComponent.IconRect.x;
                //     m_SettingComponent.SetFloat("Debugger.Icon.X", m_DebuggerComponent.IconRect.x);
                // }

                // if (m_LastIconY != m_DebuggerComponent.IconRect.y)
                // {
                //     m_LastIconY = m_DebuggerComponent.IconRect.y;
                //     m_SettingComponent.SetFloat("Debugger.Icon.Y", m_DebuggerComponent.IconRect.y);
                // }

                // if (m_LastWindowX != m_DebuggerComponent.WindowRect.x)
                // {
                //     m_LastWindowX = m_DebuggerComponent.WindowRect.x;
                //     m_SettingComponent.SetFloat("Debugger.Window.X", m_DebuggerComponent.WindowRect.x);
                // }

                // if (m_LastWindowY != m_DebuggerComponent.WindowRect.y)
                // {
                //     m_LastWindowY = m_DebuggerComponent.WindowRect.y;
                //     m_SettingComponent.SetFloat("Debugger.Window.Y", m_DebuggerComponent.WindowRect.y);
                // }

                // if (m_LastWindowWidth != m_DebuggerComponent.WindowRect.width)
                // {
                //     m_LastWindowWidth = m_DebuggerComponent.WindowRect.width;
                //     m_SettingComponent.SetFloat("Debugger.Window.Width", m_DebuggerComponent.WindowRect.width);
                // }

                // if (m_LastWindowHeight != m_DebuggerComponent.WindowRect.height)
                // {
                //     m_LastWindowHeight = m_DebuggerComponent.WindowRect.height;
                //     m_SettingComponent.SetFloat("Debugger.Window.Height", m_DebuggerComponent.WindowRect.height);
                // }

                // if (m_LastWindowScale != m_DebuggerComponent.WindowScale)
                // {
                //     m_LastWindowScale = m_DebuggerComponent.WindowScale;
                //     shaco.GameHelper.datasave.Write("Debugger.Window.Scale", m_DebuggerComponent.WindowScale);
                // }
            }

            protected override void OnDrawScrollableWindow(Rect lastGUIRect)
            {
                GUILayout.Label("Window Settings");
                GUILayout.BeginVertical("box");
                {
                    // m_DebuggerComponent.WindowScale = DrawSlider("WindowScale", m_DebuggerComponent.WindowScale, 0.5f, 5.0f);
                    m_BaseComponent.RunInBackground = GUILayout.Toggle(m_BaseComponent.RunInBackground, "RunInBackground");
                    m_BaseComponent.GameSpeed = DrawSlider("TimeScale", m_BaseComponent.GameSpeed, 0.1f, 10.0f);
                    m_BaseComponent.FrameRate = (int)DrawSlider("FrameRate", (float)m_BaseComponent.FrameRate, 10.0f, 120.0f);

                    GUILayout.BeginHorizontal();
                    {
                        m_BaseComponent.SleepTimeout = (int)DrawSlider("SleepTimeout", (float)m_BaseComponent.SleepTimeout, SleepTimeout.SystemSetting, 120);
                        if (m_BaseComponent.SleepTimeout == SleepTimeout.NeverSleep)
                            GUILayout.Label("Nerver Sleep", GUILayout.ExpandWidth(false));
                        else if (m_BaseComponent.SleepTimeout == SleepTimeout.SystemSetting)
                            GUILayout.Label("System Setting", GUILayout.ExpandWidth(false));
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }

            static private float DrawSlider(string prefix, float inputValue, float minValue, float maxValue)
            {
                var retValue = inputValue;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(prefix, GUILayout.Width(Screen.width * 0.1f));

                    GUI.changed = false;
                    var outputString = GUILayout.TextField(inputValue.ToString(), GUILayout.Width(Screen.width * 0.05f));
                    if (GUI.changed)
                    {
                        float parseValue = 0;
                        if (float.TryParse(outputString, out parseValue))
                        {
                            retValue = parseValue;
                        }
                    }

                    retValue = GUILayout.HorizontalSlider(retValue, minValue, maxValue, GUILayout.ExpandWidth(true));
                    retValue = Mathf.Clamp(retValue, minValue, maxValue);
                }
                GUILayout.EndHorizontal();
                return retValue;
            }

            static private int DrawSlider(string prefix, int inputValue, int minValue, int maxValue)
            {
                var retValue = inputValue;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(prefix, GUILayout.Width(Screen.width * 0.1f));

                    GUI.changed = false;
                    var outputString = GUILayout.TextField(inputValue.ToString(), GUILayout.Width(Screen.width * 0.05f));
                    if (GUI.changed)
                    {
                        int parseValue = 0;
                        if (int.TryParse(outputString, out parseValue))
                        {
                            retValue = parseValue;
                        }
                    }

                    retValue = (int)GUILayout.HorizontalSlider(retValue, (int)minValue, (int)maxValue, GUILayout.ExpandWidth(true));
                    retValue = Mathf.Clamp(retValue, minValue, maxValue);
                }
                GUILayout.EndHorizontal();
                return retValue;
            }
        }
    }
}
#endif
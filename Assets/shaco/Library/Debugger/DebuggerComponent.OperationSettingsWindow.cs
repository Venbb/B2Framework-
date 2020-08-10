//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2017 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

#if DEBUG_WINDOW
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private sealed class OperationSettingsWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow(Rect lastGUIRect)
            {
                float heightGUITmp = DefaultWindowRect.height / 18;

                GUILayout.Label("Operation Settings");
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Pause"))
                        {
                            var debuggerComponent = shaco.GameEntry.GetComponentInstance<BaseComponent>();
                            debuggerComponent.PauseGame();
                        }

                        if (GUILayout.Button("Resume"))
                        {
                            var debuggerComponent = shaco.GameEntry.GetComponentInstance<BaseComponent>();
                            debuggerComponent.ResumeGame();
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Unload All Assets", GUILayout.Height(heightGUITmp)))
                    {
                        shaco.GameHelper.resCache.UnloadUnusedDatas(true);
                    }

                    if (GUILayout.Button("DeleteAllSettings", GUILayout.Height(heightGUITmp)))
                    {
                        shaco.GameHelper.datasave.Clear();
                        PlayerPrefs.DeleteAll();
                    }

                    if (GUILayout.Button("Reset Window Position", GUILayout.Height(heightGUITmp)))
                    {
                        if (shaco.GameHelper.datasave.ContainsKey(WINDOW_SAVED_POSITION_KEY))
                            shaco.GameHelper.datasave.Remove(WINDOW_SAVED_POSITION_KEY);

                        var debuggerComponent = shaco.GameEntry.GetComponentInstance<DebuggerComponent>();
                        if (debuggerComponent == null)
                        {
                            shaco.Log.Exception("Debugger component is invalid.");
                            return;
                        }
                        debuggerComponent.ResetDebuggerWindowRect();
                        debuggerComponent.SetShowFullWindow(false);
                    }

                    if (GUILayout.Button("Close Debug Window", GUILayout.Height(heightGUITmp)))
                    {
                        shaco.GameEntry.RemoveIntance<DebuggerComponent>();
                    }

                    // if (GUILayout.Button("Shutdown Game Framework (None)", GUILayout.Height(heightGUITmp)))
                    // {
                    //     GameEntry.Shutdown(ShutdownType.None);
                    // }
                    // if (GUILayout.Button("Shutdown Game Framework (Restart)", GUILayout.Height(heightGUITmp)))
                    // {
                    //     GameEntry.Shutdown(ShutdownType.Restart);
                    // }
                    // if (GUILayout.Button("Shutdown Game Framework (Quit)", GUILayout.Height(heightGUITmp)))
                    // {
                    //     GameEntry.Shutdown(ShutdownType.Quit);
                    // }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
#endif
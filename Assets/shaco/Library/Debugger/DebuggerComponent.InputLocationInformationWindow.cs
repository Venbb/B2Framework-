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
        private sealed class InputLocationInformationWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow(Rect lastGUIRect)
            {
                GUILayout.Label("Input Location Information");
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Enable", GUILayout.Height(DefaultWindowRect.height / 18)))
                        {
                            Input.location.Start();
                        }
                        if (GUILayout.Button("Disable", GUILayout.Height(DefaultWindowRect.height / 18)))
                        {
                            Input.location.Stop();
                        }
                    }
                    GUILayout.EndHorizontal();

                    DrawItem("Is Enabled By User:", Input.location.isEnabledByUser.ToString());
                    DrawItem("Status:", Input.location.status.ToString());
                    DrawItem("Horizontal Accuracy:", Input.location.status == LocationServiceStatus.Running ? Input.location.lastData.horizontalAccuracy.ToString() : "not running");
                    DrawItem("Vertical Accuracy:", Input.location.status == LocationServiceStatus.Running ? Input.location.lastData.verticalAccuracy.ToString() : "not running");
                    DrawItem("Longitude:", Input.location.status == LocationServiceStatus.Running ? Input.location.lastData.longitude.ToString() : "not running");
                    DrawItem("Latitude:", Input.location.status == LocationServiceStatus.Running ? Input.location.lastData.latitude.ToString() : "not running");
                    DrawItem("Altitude:", Input.location.status == LocationServiceStatus.Running ? Input.location.lastData.altitude.ToString() : "not running");
                    DrawItem("Timestamp:", Input.location.status == LocationServiceStatus.Running ? Input.location.lastData.timestamp.ToString() : "not running");
                }
                GUILayout.EndVertical();
            }
        }
    }
}
#endif
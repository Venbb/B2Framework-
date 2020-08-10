//------------------------------------------------------------
// Shaco Framework v1.4.0
// Copyright © 2017-2020 shaco. All rights reserved.
// Feedback: mailto:449612236@qq.com
//------------------------------------------------------------

#if DEBUG_WINDOW
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public partial class DebuggerComponent
    {
        private sealed class AssetBundleWindow : ScrollableDebuggerWindowBase
        {
            override public bool useTouchScrollView { get { return false; } }

            private string _searcName = string.Empty;
            private string _searcNameLower = string.Empty;
            private int _maxShowCount = 100;
            private string _maxShowCountStr = "100";
            private shaco.TouchScroll.TouchScrollView _touchScrollView = new shaco.TouchScroll.TouchScrollView();

            public override void OnLeave()
            {
                _searcName = string.Empty;
                _searcNameLower = string.Empty;
            }

            protected override void OnDrawScrollableWindow(Rect lastGUIRect)
            {
                var allCachedAssetBundles = shaco.GameHelper.resCache.GetAllCachedAssetbundle();
                float widthGUITmp = DebuggerComponent.DefaultWindowRect.width / 16;

                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Load Mode: ");
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(shaco.GameHelper.res.resourcesLoadMode.ToString());
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Load Order: ");
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(shaco.GameHelper.res.resourcesLoadOrder.ToString());
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Excel Load Type: ");
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(shaco.GameHelper.excelSetting.runtimeLoadFileType.ToString());
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                GUILayout.BeginHorizontal("box");
                {
                    GUILayout.Label("Count: " + allCachedAssetBundles.Count);

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

                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("AssetBundle Name");
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Reference Count");
                    }
                    GUILayout.EndHorizontal();

                    _touchScrollView.BeginScrollView();
                    {
                        int showCountTmp = 0;
                        foreach (var iter in allCachedAssetBundles)
                        {
                            if (!string.IsNullOrEmpty(_searcNameLower))
                            {
                                if (!iter.Key.ToLower().Contains(_searcNameLower))
                                    continue;
                            }

                            if (++showCountTmp > _maxShowCount)
                                break;

                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(iter.Key);
                                GUILayout.FlexibleSpace();
                                GUILayout.Label(iter.Value.referenceCount.ToString());
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    _touchScrollView.EndScrollView();
                }
                GUILayout.EndVertical();
            }
        }
    }
}
#endif

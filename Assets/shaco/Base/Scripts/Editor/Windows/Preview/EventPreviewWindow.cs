using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace shacoEditor
{
    public class EventPreviewWindow : EditorWindow
    {
        private enum PanelType
        {
            API,
            Location
        }

        private readonly int MAX_SHOW_EVENT_COUNT = 50;

        private Vector2 _vec2ScrollPosition = Vector2.zero;
        private string _searchName = string.Empty;
        private bool _isSearchEventID = true;
        private object _currentSelectSender = null;
        private bool _isCurrentSelectSenderChanged = false;
        private Dictionary<string, List<shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CallBackInfo>> _searchCallBackInfos = new Dictionary<string, List<shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CallBackInfo>>();
        private bool _isTestBusy = false;
        private PanelType _panelType = PanelType.Location;

        private EventPreviewWindow _currentWindow = null;


        [MenuItem("shaco/Viewer/EventPreview " + ToolsGlobalDefine.MenuPriority.ViewerShortcutKeys.EVENT_MANAGER, false, (int)ToolsGlobalDefine.MenuPriority.Viewer.EVENT_MANAGER)]
        static void Open()
        {
            EditorHelper.GetWindow<EventPreviewWindow>(null, true, "EventPreview");
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow(this, true, "EventPreview");
        }

        void OnGUI()
        {
            if (_currentWindow == null)
            {
                return;
            }
            this.Repaint();

            _vec2ScrollPosition = GUILayout.BeginScrollView(_vec2ScrollPosition);
            DrawSearchEvent();
            DrawAllEvent(shaco.GameHelper.Event);
            shaco.GameHelper.Event.UseCurrentEventManagerEnd();

            GUILayout.EndScrollView();
        }

        private void DrawSearchEvent()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(_currentWindow.position.width / 3);
                GUILayout.Label("Search Mode: ", GUILayout.Width(_currentWindow.position.width / 8));
                if (GUILayout.Button(_isSearchEventID ? "Event ID" : "Sender"))
                {
                    _isSearchEventID = !_isSearchEventID;
                }

                if (_isSearchEventID)
                {
                    _searchName = GUILayoutHelper.SearchField(_searchName);
                }
                else
                {
                    var selectsTmp = Selection.GetFiltered(typeof(object), SelectionMode.TopLevel);
                    if (selectsTmp == null || selectsTmp.Length != 1)
                    {
                        GUILayout.Label("Please select a target in unity editor");
                    }
                    else
                    {
                        if (_currentSelectSender != (object)selectsTmp[0])
                        {
                            _searchCallBackInfos.Clear();
                            _isCurrentSelectSenderChanged = true;
                        }
                        _currentSelectSender = selectsTmp[0];
                        EditorGUILayout.ObjectField(selectsTmp[0], typeof(object), true);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawAllEvent(shaco.Base.IEventManager eventManager)
        {
            if (null == eventManager)
            {
                return;
            }

            int drawCount = 0;

            GUILayout.BeginHorizontal();
            {
                eventManager.Enabled = EditorGUILayout.Toggle("Enabled", eventManager.Enabled);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Count: " + eventManager.Count);
            }
            GUILayout.EndHorizontal();

            if (!_isSearchEventID)
            {
                if (_isCurrentSelectSenderChanged && _currentSelectSender != null)
                {
                    _searchCallBackInfos = shaco.GameHelper.Event.GetEvents(_currentSelectSender);
                    _isCurrentSelectSenderChanged = false;
                }

                foreach (var key in _searchCallBackInfos.Keys)
                {
                    GUILayout.BeginVertical("box");
                    {
                        var value = _searchCallBackInfos[key];
                        if (DrawEventHeader(key, value.Count, false))
                        {
                            for (int i = 0; i < value.Count; ++i)
                            {
                                if (!DrawEvent(key, value[i]))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    GUILayout.EndVertical();
                    if (++drawCount >= MAX_SHOW_EVENT_COUNT) break;
                }
            }
            else
            {
                eventManager.Foreach((string eventID, shaco.Base.EventCallBackInfo callbackInfo) =>
                {
                    if (!string.IsNullOrEmpty(_searchName) && !eventID.ToLower().Contains(_searchName.ToLower()))
                    {
                        return true;
                    }

                    GUILayout.BeginVertical("box");
                    {
                        DrawEvents(eventID, callbackInfo);
                    }
                    GUILayout.EndVertical();

                    if (_isTestBusy)
                    {
                        _isTestBusy = false;
                        return false;
                    }
                    return ++drawCount < MAX_SHOW_EVENT_COUNT;
                });
            }
        }

        private bool DrawEventHeader(string eventID, int eventCount, bool drawInvoke)
        {
            bool isOpen = true;
            GUILayout.BeginHorizontal();
            {
                isOpen = GUILayoutHelper.DrawHeader("Event ID: " + eventID + "(Count: " + eventCount + ")", eventID, null, GUILayout.Width(_currentWindow.position.width / 3 * 2));

                if (drawInvoke)
                {
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(0);
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("Invoke", GUILayout.Width(50)))
                            {
                                _isTestBusy = true;
                                var defaultEventArg = shaco.Base.Utility.Instantiate(eventID) as shaco.Base.BaseEventArg;
                                shaco.GameHelper.Event.InvokeEvent(defaultEventArg);
                            }
                            if (GUILayout.Button("Delete", GUILayout.Width(50)))
                            {
                                _isTestBusy = true;
                                var defaultEventArg = shaco.Base.Utility.Instantiate(eventID) as shaco.Base.BaseEventArg;
                                shaco.GameHelper.Event.RemoveEvent(defaultEventArg.GetType());
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndVertical();
                }
            }

            GUILayout.EndHorizontal();

            return isOpen;
        }

        private void DrawEvents(string eventID, shaco.Base.EventCallBackInfo callbackInfo)
        {
            if (DrawEventHeader(eventID, callbackInfo.CallBack.Count, true))
            {
                int countTmp = callbackInfo.CallBack.Count;
                for (int i = 0; i < countTmp; ++i)
                {
                    if (!DrawEvent(eventID, callbackInfo.CallBack[i]))
                    {
                        break;
                    }
                }
            }
        }

        private bool DrawEvent(string eventID, shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CallBackInfo callBackInfo)
        {
            GUILayout.BeginHorizontal();
            {
                var maxWidthTmp = GUILayout.MaxWidth(_currentWindow.position.width / 3 * 2 - 283);

                //draw sender
                GUILayout.Label("Default Sender:", GUILayout.Width(85));

                if (callBackInfo.DefaultSender as Object != null)
                {
                    EditorGUILayout.ObjectField(((Object)callBackInfo.DefaultSender), typeof(Object), true, GUILayout.Width(140));
                }
                else
                {
                    GUILayout.TextArea(callBackInfo.DefaultSender.ToTypeString(), GUILayout.Width(140));
                }

                //draw callback
                GUILayout.Label("Function:", GUILayout.Width(53));
                GUILayout.TextArea(callBackInfo.CallFunc.Method.Name, maxWidthTmp);

                //draw invoke type
                GUILayout.Label(callBackInfo.InvokeOnce ? "Once" : "Loop", GUILayout.Width(32));

                _panelType = (PanelType)EditorGUILayout.EnumPopup(_panelType, GUILayout.Width(60));
                switch (_panelType)
                {
                    case PanelType.API:
                        {
                            DrawHeaderAPI(eventID, callBackInfo);
                            break;
                        }
                    case PanelType.Location:
                        {
                            DrawHeaderLocation(eventID, callBackInfo);
                            break;
                        }
                    default: shaco.Log.Info("unsupport panel type=" + _panelType); break;
                }
            }
            GUILayout.EndHorizontal();

            return !_isTestBusy;
        }

        private void DrawHeaderLocation(string eventID, shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CallBackInfo callBackInfo)
        {
            //draw location 'AddCallBack' button
            if (callBackInfo.CallAddEventStack.HasStack() && GUILayout.Button("Add" + callBackInfo.CallAddEventStack.GetPerformanceDescription()))
            {
                shaco.Log.Info(callBackInfo.CallAddEventStack.GetTotalStackInformation());
                EditorHelper.OpenAsset(callBackInfo.CallAddEventStack.GetStackInformation(), callBackInfo.CallAddEventStack.GetStackLine());
            }

            //draw location 'InvokeCallBack' button
            if (callBackInfo.CallInvokeEventStack.HasStack() && GUILayout.Button("Invoke" + callBackInfo.CallInvokeEventStack.GetPerformanceDescription()))
            {
                shaco.Log.Info(callBackInfo.CallInvokeEventStack.GetTotalStackInformation());
                EditorHelper.OpenAsset(callBackInfo.CallInvokeEventStack.GetStackInformation(), callBackInfo.CallInvokeEventStack.GetStackLine());
            }
        }

        private void DrawHeaderAPI(string eventID, shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CallBackInfo callBackInfo)
        {
            //draw delete button
            if (GUILayout.Button("Delete", GUILayout.Width(55)))
            {
                _isTestBusy = true;
                var defaultEventArg = (shaco.Base.BaseEventArg)typeof(shaco.Base.BaseEventArg).Assembly.CreateInstance(eventID);
                shaco.GameHelper.Event.RemoveEvent(defaultEventArg.GetType(), callBackInfo.DefaultSender, callBackInfo.CallFunc);
            }
        }
    }
}


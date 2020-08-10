using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class ParamsInputView : EditorWindow
    {
        public class InputInfo
        {
            public string paramName;
            public object paramValue;
            public System.Type paramType;

            public InputInfo(string paramName, object paramValue, System.Type paramType)
            {
                this.paramName = paramName;
                this.paramValue = paramValue;
                this.paramType = paramType;
            }
        }

        private enum SelectMode
        {
            One,
            Muilti
        }

        private class ParamInfo
        {
            public bool isSelected = false;
            public object value;
            public System.Type type;
        }

        private System.Action<ParamsInputView, string, object> _callbackOutputParam = null;
        private System.Action<ParamsInputView, Dictionary<string, object>> _callbackOutputParams = null;
        private System.Action<string, object> _callbackSelectParam = null;
        private Dictionary<string, ParamInfo> _parameters = new Dictionary<string, ParamInfo>();
        private SelectMode _selectMode = SelectMode.One;
        private ParamInfo _currentSelect = null;
        private bool _isSelectAll = false;
        private EditorWindow _targetWindow = null;
        private List<ParamInfo> _selectParamsIndex = new List<ParamInfo>();

        static public ParamsInputView OpenOneMode(EditorWindow targetWindow, string title, System.Action<ParamsInputView, string, object> callbackOutputParam, params InputInfo[] parameters)
        {
            return OpenOneMode(targetWindow, title, callbackOutputParam, null, parameters);
        }

        static public ParamsInputView OpenOneMode(EditorWindow targetWindow, string title, System.Action<ParamsInputView, string, object> callbackOutputParam, System.Action<string, object> callbackSelectParam, params InputInfo[] parameters)
        {
            if (null == callbackOutputParam || parameters.IsNullOrEmpty())
                return null;

            var retValue = shacoEditor.EditorHelper.GetWindow<ParamsInputView>(null, true, title);
            retValue.Init(targetWindow, SelectMode.One, callbackOutputParam, null, callbackSelectParam, parameters);
            return retValue;
        }

        static public ParamsInputView OpenMuiltiMode(EditorWindow targetWindow, string title, System.Action<ParamsInputView, Dictionary<string, object>> callbackOutputParams, params InputInfo[] parameters)
        {
            return OpenMuiltiMode(targetWindow, title, callbackOutputParams, null, parameters);
        }

        static public ParamsInputView OpenMuiltiMode(EditorWindow targetWindow, string title, System.Action<ParamsInputView, Dictionary<string, object>> callbackOutputParams, System.Action<string, object> callbackSelectParam, params InputInfo[] parameters)
        {
            if (null == callbackOutputParams || parameters.IsNullOrEmpty())
                return null;

            var retValue = shacoEditor.EditorHelper.GetWindow<ParamsInputView>(null, true, title);
            retValue.Init(targetWindow, SelectMode.Muilti, null, callbackOutputParams, callbackSelectParam, parameters);
            return retValue;
        }

        private void Init(EditorWindow targetWindow, SelectMode selectMode,
                            System.Action<ParamsInputView, string, object> callbackOutputParam,
                            System.Action<ParamsInputView, Dictionary<string, object>> callbackOutputParams,
                            System.Action<string, object> callbackSelectParam,
                            params InputInfo[] parameters)
        {
            this._callbackOutputParam = callbackOutputParam;
            this._callbackOutputParams = callbackOutputParams;
            this._selectMode = selectMode;
            this._targetWindow = targetWindow;
            this._callbackSelectParam = callbackSelectParam;
            this._parameters.Clear();
            for (int i = 0; i < parameters.Length; ++i)
            {
                var paramTmp = parameters[i];
                if (this._parameters.ContainsKey(paramTmp.paramName))
                {
                    Debug.LogError("ParamsInputView Init error: has duplicate param name=" + paramTmp.paramName);
                }
                else
                {
                    this._parameters.Add(paramTmp.paramName, new ParamInfo()
                    {
                        isSelected = false,
                        value = paramTmp.paramValue,
                        type = paramTmp.paramType
                    });
                }
            }

            //停靠界面位置
            if (null != targetWindow)
                this.position = new Rect(targetWindow.position.x, targetWindow.position.y + targetWindow.position.height, targetWindow.position.width, this.position.height);
        }

        private void Update()
        {
            if (null == _callbackOutputParams && null == _callbackOutputParam)
            {
                this.Close();
                return;
            }
        }

        private void OnGUI()
        {
            if (null == _callbackOutputParams && null == _callbackOutputParam)
            {
                return;
            }

            UpdateEvent();
            DrawTopButtons();

            GUILayout.BeginVertical("box");
            {
                DrawParameters();
            }
            GUILayout.EndVertical();

        }

        private void DrawTopButtons()
        {
            GUILayout.BeginHorizontal();
            {
                if (_selectMode == SelectMode.Muilti)
                {
                    GUI.changed = false;
                    GUILayout.BeginHorizontal();
                    {
                        _isSelectAll = EditorGUILayout.Toggle(_isSelectAll, GUILayout.Width(12));
                        EditorGUILayout.LabelField("All");
                    }
                    GUILayout.EndHorizontal();
                    if (GUI.changed)
                    {
                        foreach (var iter in _parameters)
                        {
                            iter.Value.isSelected = _isSelectAll;
                        }
                        _currentSelect = null;
						GUI.FocusControl(string.Empty);
                    }
                }

                EditorGUI.BeginDisabledGroup(this._parameters.Count == 0 || (null == _currentSelect && !_isSelectAll));
                {
                    if (GUILayout.Button("OK(Enter)"))
                    {
                        ClickOkButton();
                    }
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Cancel(Esc)"))
                {
                    this.Close();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawParameters()
        {
            foreach (var iter in _parameters)
            {
                var paramInfo = _parameters[iter.Key];
                GUILayout.BeginHorizontal();
                {
                    if (_selectMode == SelectMode.One)
                    {

                    }
                    else if (_selectMode == SelectMode.Muilti)
                    {

                    }
                    else
                        Debug.LogError("ParamsInputView OnGUI erorr: unsupport select mode=" + _selectMode);

                    EditorGUI.BeginChangeCheck();
                    {
                        paramInfo.isSelected = EditorGUILayout.Toggle(paramInfo.isSelected, GUILayout.Width(12));
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        switch (_selectMode)
                        {
                            //当使用单选模式下，反选上一个
                            case SelectMode.One:
                                {
                                    if (null != _currentSelect && paramInfo != _currentSelect)
                                    {
                                        _currentSelect.isSelected = !_currentSelect.isSelected;
                                        CheckSortIndex(_currentSelect);
                                    }
                                    break;
                                }
                            //当多选模式下，检查All标记是否还需要存在
                            case SelectMode.Muilti:
                                {
                                    bool isAllSelected = true;
                                    foreach (var iter2 in _parameters)
                                    {
                                        if (!iter2.Value.isSelected)
                                        {
                                            isAllSelected = false;
                                            break;
                                        }
                                    }
                                    _isSelectAll = isAllSelected;
                                    break;
                                }
                        }

                        CheckSortIndex(paramInfo);

                        //设置当前选择的对象
                        _currentSelect = paramInfo.isSelected ? paramInfo : null;
                        if (null != _callbackSelectParam)
                        {
                            if (paramInfo.isSelected)
                                _callbackSelectParam(iter.Key, iter.Value);
                            else
                                _callbackSelectParam(iter.Key, null);
                            if (null != _targetWindow)
                                _targetWindow.Repaint();
                        }

                        GUI.FocusControl(string.Empty);
                    }

                    DrawSelectIndex(paramInfo);

                    //组件对象仅在选中情况下可以编辑
                    EditorGUI.BeginDisabledGroup(!paramInfo.isSelected);
                    {
                        object changedValue = iter.Value;
                        GUI.changed = false;
                        if (null == iter.Value.value)
                            EditorGUILayout.LabelField(iter.Key);
                        else
                            changedValue = GUILayoutHelper.DrawValue(iter.Key, iter.Value.value, null == iter.Value.type ? iter.Value.value.GetType() : iter.Value.type);
                        if (GUI.changed)
                        {
                            paramInfo.value = changedValue;
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }
                GUILayout.EndHorizontal();
            }
        }

        private void OnDestroy()
        {
            if (null != _callbackSelectParam)
            {
                _callbackSelectParam(string.Empty, null);
                if (null != _targetWindow)
                    _targetWindow.Repaint();
            }
        }

		private void ClickOkButton()
		{
            if (_selectMode == SelectMode.One)
            {
				string keyTmp = string.Empty;
				ParamInfo paramTmp = null;
				bool hasSelected = false;
				foreach (var iter in this._parameters)
				{
					if (iter.Value.isSelected)
					{
                        keyTmp = iter.Key;
                        paramTmp = iter.Value;
                        hasSelected = true;
						break;
					}
				}

				if (hasSelected)
                    _callbackOutputParam(this, keyTmp, paramTmp);
				else
					Debug.LogError("ParamsInputView ClickOkButton error: nothing is selected");
            }
            else
            {
                var retValue = new Dictionary<string, object>();
                foreach (var iter in _parameters)
                {
                    if (iter.Value.isSelected)
                        retValue.Add(iter.Key, iter.Value.value);
                }
                _callbackOutputParams(this, retValue);
            }

            if (null != this._targetWindow)
                this._targetWindow.Repaint();
		}

        private void CheckSortIndex(ParamInfo paramInfo)
        {
            if (null == paramInfo)
                return;

            if (!paramInfo.isSelected)
            {
                for (int i = 0; i < _selectParamsIndex.Count; ++i)
                {
                    if (paramInfo == _selectParamsIndex[i])
                    {
                        _selectParamsIndex.RemoveAt(i);
                        break;
                    }
                }
            }
            else if (_selectParamsIndex.IndexOf(paramInfo) < 0)
            {
                _selectParamsIndex.Add(paramInfo);
            }
        }

        private void DrawSelectIndex(ParamInfo paramInfo)
        {
            var findIndex = _selectParamsIndex.IndexOf(paramInfo);
            if (findIndex >= 0)
            {
                GUILayout.Label(findIndex.ToString(), GUILayout.Width(30));
            }
        }

        private void UpdateEvent()
        {
            var curEvent = Event.current;
            if (curEvent == null)
                return;

            switch (curEvent.type)
            {
                case EventType.KeyUp:
                    {
                        switch (curEvent.keyCode)
                        {
                            case KeyCode.Escape:
                                {
                                    this.Close();
                                    break;
                                }
                            case KeyCode.KeypadEnter:
							case KeyCode.Return:
                                {
                                    ClickOkButton();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace shacoEditor
{
    /// <summary>
    /// 新手引导编辑窗口
    /// tips: 如果引导配置资源引用因为手动修改过路径导致读取失败
    ///       别担心，重新打开一次该编辑窗口一切就会正常了，因为资源在编辑器环境下是基于gui读取的并非路径
    /// </summary>
    public class GuideManagerWindow : EditorWindow
    {
        public class GuideStepExpandInfo
        {
            public Object bindObject = null;
        }

        private readonly GuideStepExpandInfo INVALID_STEP_EXPAND_INFO = new GuideStepExpandInfo();

        [SerializeField]
        private string _loadedConfigPath = string.Empty;
        private Dictionary<string, GuideStepExpandInfo> _guideStepToObject = new Dictionary<string, GuideStepExpandInfo>();
        private shaco.Base.IGuideStep _placeholderStep = new shaco.Base.GuideStepPlaceholder();
        private shaco.Base.IGuideStep _currentFrameDrawStep = null;
        [SerializeField]
        private string _searchName = string.Empty;
        [SerializeField]
        private string _searchNameLower = string.Empty;

        //当前发生了改变的步骤id
        //主要用于在步骤id发生改变后，保持原来界面打开的状态
        private string _currentChangedFromID = null;
        private string _currentChangedToID = null;
        private bool _isFirstStepIDChangeDirty = false;
        [SerializeField]
        private string _inputNewStepGuideID = string.Empty;
        private bool _isValidNewStepGuideID = false;
        private ParamsInputView _currentParamsInputView = null;
        private Vector2 _scrollViewPosition = Vector2.zero;
        private int _maxShowStepsCount = 10;
        private Color _colorRed = Color.red;
        private string _ignoreSettingValue = string.Empty;
        private string _ignoreIsOpenValue = string.Empty;
        private string _ignoreIsEndValue = string.Empty;
        [SerializeField]
        private string _inputSerializeAttributeTypeFullName = string.Empty;
        [SerializeField]
        private System.Type _inputSerializeAttributeType = null;
        [SerializeField]
        private bool _isAutoOverrwiteSave = true;

        [MenuItem("shaco/Viewer/GuideManager " + ToolsGlobalDefine.MenuPriority.ViewerShortcutKeys.GUIDE_MANAGER, false, (int)ToolsGlobalDefine.MenuPriority.Viewer.GUIDE_MANAGER)]
        static void OpenGuideManagerWindow()
        {
            shacoEditor.EditorHelper.GetWindow<GuideManagerWindow>(null, true, "GuideManager");
        }

        private void OnEnable()
        {
            Init();
        }

        private void Update()
        {
            if (shaco.GameHelper.newguide.isReSaveFileDirty)
            {
                shaco.GameHelper.newguide.isReSaveFileDirty = false;
                SaveToFile(_loadedConfigPath);
            }
        }

        private void OnGUI()
        {
            EditorHelper.RecordObjectWindow(this);

            UpdateEvent();

            DrawLoadedConfigPath();
            DrawSearchFiled();
            DrawToolsButton();

            int drawIndex = 0;
            _scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition);
            {
                shaco.GameHelper.newguide.ForeachSteps((IList<shaco.Base.IGuideStep> steps) =>
                {
                    if (drawIndex++ >= _maxShowStepsCount)
                        return false;

                    DrawStepCheck(steps);
                    return true;
                });
            }
            GUILayout.EndScrollView();

            CheckDelayRemoveStepID();
        }

        private void UpdateEvent()
        {
            if (string.IsNullOrEmpty(_loadedConfigPath))
                return;

            var currentEvent = Event.current;
            if (null == currentEvent)
                return;

            //保存配置
            if ((currentEvent.command || currentEvent.control) && currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.S)
            {
                ExecuteSave();
            }
        }

        private void OnDestroy()
        {
            if (shaco.GameHelper.newguide.callbackBeforeOnceStepStart.HasCallBack(OnGuideStepChangedCallBack))
                shaco.GameHelper.newguide.callbackBeforeOnceStepStart.RemoveCallBack(OnGuideStepChangedCallBack);
            SaveSettings();
            CheckCloseParamsInputView();
        }

        private void Init()
        {
            LoadSettings();

            if (shaco.GameHelper.newguide.callbackBeforeOnceStepStart.HasCallBack(OnGuideStepChangedCallBack))
                shaco.GameHelper.newguide.callbackBeforeOnceStepStart.RemoveCallBack(OnGuideStepChangedCallBack);
            shaco.GameHelper.newguide.callbackBeforeOnceStepStart.AddCallBack(this, OnGuideStepChangedCallBack);
            shaco.GameHelper.newguide.callbackAfterOnceStepEnd.AddCallBack(this, OnGuideStepChangedCallBack);

            _ignoreSettingValue = shaco.Base.Utility.ToVariableName(() => _placeholderStep.settingValue);
            _ignoreIsOpenValue = shaco.Base.Utility.ToVariableName(() => _placeholderStep.isOpen);
            _ignoreIsEndValue = shaco.Base.Utility.ToVariableName(() => _placeholderStep.isEnd);

            //设定所有引导步骤header为收起状态
            var allFirstSteps = shaco.GameHelper.newguide.GetAllFirstStep();
            if (null != allFirstSteps)
            {
                for (int i = 0; i < allFirstSteps.Length; ++i)
                {
                    var firstStep = allFirstSteps[i];
                    var keyTmp = "[ID: " + firstStep.guideStepID + "]";
                    if (!shaco.GameHelper.datasave.ContainsKey(keyTmp))
                        shaco.GameHelper.datasave.WriteBool(keyTmp, false);
                }
            }
        }

        private void OnGuideStepChangedCallBack(object sender, shaco.Base.IGuideStep step)
        {
            this.Repaint();
        }

        private void CheckCloseParamsInputView()
        {
            if (null != _currentParamsInputView)
            {
                _currentParamsInputView.Close();
                _currentParamsInputView = null;
            }
        }

        private void SaveSettings()
        {
            shaco.GameHelper.gameConfig.WriteString("GuideManagerWindow._loadedConfigPath", _loadedConfigPath);
            shaco.GameHelper.gameConfig.WriteBool("GuideManagerWindow._isAutoOverrwiteSave", _isAutoOverrwiteSave);

            if (shaco.GameHelper.guideSettingHelper.serializeAttributeType.FullName != _inputSerializeAttributeTypeFullName)
            {
                bool changeAttributeTypeSuccess = false;
                var attributeType = shaco.Base.Utility.Assembly.GetTypeWithinLoadedAssemblies(_inputSerializeAttributeTypeFullName);
                if (null != attributeType)
                {
                    if (!attributeType.IsInherited(typeof(System.Attribute)))
                        Debug.LogError("GuideManagerWindow SaveSettings error: not inherited from attribute type=" + attributeType);
                    else
                    {
                        //如果使用了AttributeUsageAttribute属性，则要求它自少支持成员变量开启开启
                        var customAttributeFlag = attributeType.GetAttribute(typeof(System.AttributeUsageAttribute)) as System.AttributeUsageAttribute;
                        if (null != customAttributeFlag && ((customAttributeFlag.ValidOn & System.AttributeTargets.Field) == 0 && (customAttributeFlag.ValidOn & System.AttributeTargets.Property) == 0))
                            Debug.LogError("GuideManagerWindow SaveSettings error: can't use attribute on property or field, type=" + attributeType);
                        else
                        {
                            shaco.GameHelper.guideSettingHelper.serializeAttributeType = attributeType;
                            changeAttributeTypeSuccess = true;
                        }
                    }
                }
                if (!changeAttributeTypeSuccess)
                {
                    _inputSerializeAttributeTypeFullName = _inputSerializeAttributeType.FullName;
                }
            }
            shaco.GameHelper.gameConfig.WriteString("GuideSettingHelper.serializeAttributeType", shaco.GameHelper.guideSettingHelper.serializeAttributeType.FullName);
        }

        private void LoadSettings()
        {
            _loadedConfigPath = shaco.GameHelper.gameConfig.ReadString("GuideManagerWindow._loadedConfigPath");
            _isAutoOverrwiteSave = shaco.GameHelper.gameConfig.ReadBool("GuideManagerWindow._isAutoOverrwiteSave", _isAutoOverrwiteSave);
            var attributeTypeName = shaco.GameHelper.gameConfig.ReadString("GuideSettingHelper.serializeAttributeType", typeof(UnityEngine.SerializeField).FullName);
            shaco.GameHelper.guideSettingHelper.serializeAttributeType = shaco.Base.Utility.Assembly.GetTypeWithinLoadedAssemblies(attributeTypeName);
            _inputSerializeAttributeType = shaco.GameHelper.guideSettingHelper.serializeAttributeType;
            _inputSerializeAttributeTypeFullName = shaco.GameHelper.guideSettingHelper.serializeAttributeType.FullName;
            if (!Application.isPlaying)
            {
                ReloadGuideStepsFromFile();
            }
        }

        private void ReloadGuideStepsFromFile()
        {
            if (string.IsNullOrEmpty(_loadedConfigPath))
                return;

            if (shaco.Base.FileHelper.ExistsFile(_loadedConfigPath))
            {
                shaco.GameHelper.newguide.LoadFromString(shaco.Base.FileHelper.ReadAllByUserPath(_loadedConfigPath), !Application.isPlaying);
            }
            else
            {
                Debug.LogError("GuideManagerWindow ReloadGuideStepsFromFile erorr: not found path=" + _loadedConfigPath);
                _loadedConfigPath = string.Empty;
                shaco.GameHelper.datasave.WriteString("GuideManagerWindow._loadedConfigPath", _loadedConfigPath);
            }

            CheckCloseParamsInputView();
        }

        /// <summary>
        /// 刷新绑定脚本路径和它所在unity对象的关系
        /// </summary>
        private GuideStepExpandInfo RefreshGuideStepToUnityObjectReference(shaco.Base.ISetting setting)
        {
            GuideStepExpandInfo retValue = null;

            //根据脚本名字搜索文件并取得对应的类
            var guideStepTypeFullName = setting.GetType().FullName;

            //过滤重复类型
            if (_guideStepToObject.TryGetValue(guideStepTypeFullName, out retValue))
                return retValue;
            else
                retValue = INVALID_STEP_EXPAND_INFO;

            var findResult = AssetDatabase.FindAssets(string.Format("{0} t:script", setting.GetType().Name));
            if (null == findResult || findResult.Length == 0)
            {
                Debug.LogError("GuideManagerWindow Refresh error: not found script by type=" + guideStepTypeFullName + "\nPlease make sure the file name and type name are the same");
                _guideStepToObject.Add(guideStepTypeFullName, INVALID_STEP_EXPAND_INFO);
                return retValue;
            }

            //从找到的脚本中找出含类型名字的
            var findAssetPath = string.Empty;
            foreach (var iter in findResult)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(iter);
                var readScriptText = shaco.Base.FileHelper.ReadAllByUserPath(EditorHelper.GetFullPath(assetPath));
                var findFullName = shaco.Base.Utility.GetFullClassNames(readScriptText);
                if (null != findFullName && findFullName.Length > 0 && null != findFullName.Find(v => v == guideStepTypeFullName))
                {
                    findAssetPath = assetPath;
                    break;
                }
            }

            //没有从查找到的脚本找到对应类的类型
            if (string.IsNullOrEmpty(findAssetPath))
            {
                Debug.LogError("GuideManagerWindow Refresh error: not found script in files=" + findResult.Convert(v => AssetDatabase.GUIDToAssetPath(v)).ToSerializeString() + "\nPlease make sure the file name and type name are the same");
                _guideStepToObject.Add(guideStepTypeFullName, INVALID_STEP_EXPAND_INFO);
                return retValue;
            }
            else
            {
                _guideStepToObject.Add(guideStepTypeFullName, new GuideStepExpandInfo() { bindObject = AssetDatabase.LoadAssetAtPath<TextAsset>(findAssetPath) });
                return retValue;
            }
        }

        private void DrawLoadedConfigPath()
        {
            _inputSerializeAttributeTypeFullName = EditorGUILayout.TextField("Serialize Attribute Type", _inputSerializeAttributeTypeFullName);

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                _isAutoOverrwiteSave = EditorGUILayout.Toggle(new GUIContent("Auto Save With Overwrite", "是否自动保存并覆盖文件"), _isAutoOverrwiteSave);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUI.changed = false;
                int guideCount = shaco.GameHelper.newguide.Count;

                _loadedConfigPath = GUILayoutHelper.PathField("Config Path", _loadedConfigPath, "bytes");
                EditorGUI.BeginDisabledGroup(shaco.GameHelper.newguide.isStarting);
                {
                    if (GUI.changed && shaco.Base.FileHelper.ExistsFile(_loadedConfigPath))
                    {
                        SaveSettings();
                        ReloadGuideStepsFromFile();
                        return;
                    }

                    if (guideCount > 0)
                    {
                        if (GUILayout.Button(_isAutoOverrwiteSave ? "Overwrite" : "Save", GUILayout.ExpandWidth(false)))
                        {
                            ExecuteSave();
                        }

                        if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                        {
                            shaco.GameHelper.newguide.ClearStep();
                            CheckCloseParamsInputView();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("New", GUILayout.ExpandWidth(false)))
                        {
                            shaco.GameHelper.newguide.isOpen = true;
                            shaco.GameHelper.newguide.AddFirstStep(new shaco.Base.GuideStepPlaceholder() { guideStepID = "default", firstStepID = "default" });

                            var savePathTmp = EditorUtility.SaveFilePanel("Select save path", Application.dataPath, "GuideSetting", "bytes");
                            if (!string.IsNullOrEmpty(savePathTmp))
                            {
                                SaveToFile(savePathTmp);
                            }
                            else
                            {
                                shaco.GameHelper.newguide.ClearStep();
                                CheckCloseParamsInputView();
                            }
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();

                if (!string.IsNullOrEmpty(_loadedConfigPath) && GUILayout.Button(new GUIContent(Application.isPlaying ? "Restart" : "Reset", "重置新手引导"), GUILayout.ExpandWidth(false)))
                {
                    shaco.GameHelper.newguide.ReloadFromString(shaco.Base.FileHelper.ReadAllByUserPath(_loadedConfigPath));

                    if (Application.isPlaying)
                        shaco.GameHelper.newguide.Start();
                    Debug.Log("GuideManagerWindow: restart guide success");
                }

                if (!string.IsNullOrEmpty(_loadedConfigPath) && GUILayout.Button(new GUIContent("Refresh", "重新加载引导并刷新界面"), GUILayout.ExpandWidth(false)))
                {
                    shaco.GameHelper.newguide.LoadFromString(shaco.Base.FileHelper.ReadAllByUserPath(_loadedConfigPath), true);
                    _guideStepToObject.Clear();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSearchFiled()
        {
            int stepCount = shaco.GameHelper.newguide.Count;
            if (stepCount == 0)
                return;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Count: " + stepCount);
                GUILayout.Space(Screen.width / 3);

                GUI.changed = false;
                _searchName = GUILayoutHelper.SearchField(_searchName);
                if (GUI.changed)
                    _searchNameLower = _searchName.ToLower();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawToolsButton()
        {
            var newguideInstance = shaco.GameHelper.newguide;
            if (newguideInstance.Count == 0)
                return;

            GUILayout.BeginHorizontal();
            {
                GUI.changed = false;
                var changedOpened = EditorGUILayout.Toggle(new GUIContent("Open", "新手引导总开关"), newguideInstance.isOpen);
                if (GUI.changed)
                {
                    newguideInstance.isOpen = changedOpened;
                }

                if (newguideInstance.isOpen && GUILayout.Button("Open All", GUILayout.ExpandWidth(false)))
                {
                    newguideInstance.SetAllStepOpen(true);
                }

                if (newguideInstance.isOpen && GUILayout.Button("Close All", GUILayout.ExpandWidth(false)))
                {
                    newguideInstance.SetAllStepOpen(false);
                }

                //靠右对齐
                GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();
                {
                    GUILayout.Label("New ID");
                    _inputNewStepGuideID = EditorGUILayout.TextField(_inputNewStepGuideID, GUILayout.Width(210));
                }
                if (EditorGUI.EndChangeCheck())
                {
                    _isValidNewStepGuideID = !newguideInstance.HasGuide(_inputNewStepGuideID);
                }

                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_inputNewStepGuideID) || !_isValidNewStepGuideID);
                {
                    if (GUILayout.Button("NewStep"))
                    {
                        newguideInstance.AddFirstStep(new shaco.Base.GuideStepPlaceholder()
                        {
                            guideStepID = _inputNewStepGuideID,
                            firstStepID = _inputNewStepGuideID
                        });
                        _isValidNewStepGuideID = false;
                        _inputNewStepGuideID = string.Empty;
                        GUI.FocusControl(string.Empty);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawStepCheck(IList<shaco.Base.IGuideStep> steps)
        {
            if (steps.IsNullOrEmpty() || IsChangeStepID())
                return;

            if (null == steps[0])
                return;

            var newguideInstance = shaco.GameHelper.newguide;

            var firstStep = newguideInstance.GetFirstStep(steps[0].firstStepID);
            DrawStep(firstStep, steps);
        }

        private void DrawStep(shaco.Base.IGuideStep firstStep, IList<shaco.Base.IGuideStep> steps)
        {
            var executingStep = shaco.GameHelper.newguide.GetExecutingStep(firstStep.firstStepID);
            bool isOpendedStep = shaco.GameHelper.newguide.IsStepOpened(firstStep.firstStepID);
            var titleTmp = "[ID: " + firstStep.guideStepID + "]";

            System.Func<shaco.Base.IGuideStep, shaco.Base.IGuideStep, bool> onCheckValueChangeCallBack = (oldValue, newValue) =>
            {
                // //清空数据
                // if (null == oldValue)
                // {
                //     SetDelayRemoveStepID(firstStep.guideStepID, string.Empty);
                //     return true;
                // }
                //添加数据
                if (null == oldValue && null != newValue)
                {
                    //如果已有相同first id则禁止添加
                    if (shaco.GameHelper.newguide.HasGuide(newValue.guideStepID))
                    {
                        Debug.LogError("GuideManagerWindow add step error: has duplicate first step id=" + newValue.guideStepID);
                        return false;
                    }
                    else
                        return true;
                }
                //删除数据
                if (null == newValue)
                {
                    if (shaco.GameHelper.newguide.IsFirstStep(oldValue))
                    {
                        shaco.Base.IGuideStep stepNext = null;
                        for (int i = 1; i < steps.Count; ++i)
                        {
                            var nextStepTmp = steps[i];
                            if (null != nextStepTmp && !shaco.GameHelper.newguide.IsFirstStep(nextStepTmp))
                            {
                                //处理下个步骤id可能为空的问题
                                if (string.IsNullOrEmpty(nextStepTmp.guideStepID))
                                    nextStepTmp.guideStepID = oldValue.guideStepID;
                                stepNext = nextStepTmp;
                                break;
                            }
                        }

                        //下一个步骤自动作为第一个步骤
                        SetDelayRemoveStepID(oldValue.guideStepID, null != stepNext ? stepNext.guideStepID : string.Empty);
                    }
                    return true;
                }
                else
                    return true;
            };

            System.Action<System.Action<shaco.Base.IGuideStep>> onCreateCallBack = (callback) =>
            {
                shaco.Base.GuideStepPlaceholder retValue = new shaco.Base.GuideStepPlaceholder();
                if (steps.Count > 0)
                {
                    retValue.firstStepID = steps[0].firstStepID;
                }

                steps.Add(retValue);
                // callback(retValue);
            };

            System.Func<bool> onBeforeDrawHeaderCallBack = () =>
            {
                EditorGUI.BeginDisabledGroup(!shaco.GameHelper.newguide.isOpen);
                {
                    GUI.changed = false;
                    var changedOpened = EditorGUILayout.Toggle(isOpendedStep, GUILayout.Width(15));
                    if (GUI.changed)
                    {
                        shaco.GameHelper.newguide.SetStepOpen(firstStep.firstStepID, changedOpened);
                        // shaco.GameHelper.datasave.Write(titleTmp, changedOpened); 不再open开关的时候控制header收开

                        if (Application.isPlaying)
                        {
                            if (changedOpened && !shaco.GameHelper.newguide.IsGuiding(firstStep.firstStepID))
                            {
                                shaco.GameHelper.newguide.Start(firstStep.firstStepID);
                            }
                            else if (!changedOpened && shaco.GameHelper.newguide.IsGuiding(firstStep.firstStepID))
                            {
                                shaco.GameHelper.newguide.Stop(firstStep.firstStepID);
                            }
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
                return true;
            };

            System.Action onAfterDrawHeaderCallBack = () =>
            {
                OpenCopyInputWindow(steps);
                GUILayout.Label("Count: " + steps.Count, GUILayout.MinWidth(65), GUILayout.ExpandWidth(false));
            };

            System.Func<int, int, bool> onWillSwapValueCallBack = (fromIndex, toIndex) =>
            {
                var stepFrom = steps[fromIndex];
                var stepTo = steps[toIndex];

                if (shaco.GameHelper.newguide.IsFirstStep(stepFrom))
                {
                    SetDelayRemoveStepID(stepFrom.guideStepID, stepTo.guideStepID);
                    if (string.IsNullOrEmpty(stepTo.guideStepID))
                    {
                        Debug.LogError("GuideManagerWindow swap error: changed to step id can't be empty");
                        _currentChangedFromID = null;
                        _currentChangedToID = null;
                        return false;
                    }
                }
                if (shaco.GameHelper.newguide.IsFirstStep(stepTo))
                {
                    SetDelayRemoveStepID(stepTo.guideStepID, stepFrom.guideStepID);
                    if (string.IsNullOrEmpty(stepFrom.guideStepID))
                    {
                        Debug.LogError("GuideManagerWindow swap error: changed to step id can't be empty");
                        _currentChangedFromID = null;
                        _currentChangedToID = null;
                        return false;
                    }
                }
                return true;
            };

            System.Func<int, shaco.Base.IGuideStep, System.Action<shaco.Base.IGuideStep>, bool> onDrawValueCallBack = (index, step, callback) =>
            {
                bool retValue = true;
                var stepTmp = steps[index];

                //有正在进行的引导步骤
                var oldColor = GUI.color;
                if (executingStep == step)
                    GUI.color = Color.green;

                DrawFollowStep(stepTmp, (changedValue) =>
                {
                    callback(changedValue);
                });

                if (executingStep == step)
                    GUI.color = oldColor;

                return retValue;
            };

            GUILayoutHelper.DrawListBase(steps, titleTmp, onCheckValueChangeCallBack, onCreateCallBack, onBeforeDrawHeaderCallBack, onAfterDrawHeaderCallBack, onDrawValueCallBack, onWillSwapValueCallBack);
        }

        private void DrawFollowStep(shaco.Base.IGuideStep step, System.Action<shaco.Base.IGuideStep> onValueChangedCallBack)
        {
            if (!string.IsNullOrEmpty(_searchNameLower))
            {
                if (!step.guideStepID.ToLower().Contains(_searchNameLower) && !step.ToTypeString().ToLower().Contains(_searchNameLower))
                    return;
            }

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(GetDrawStepHeaderString(step));
                    DrawScriptPathUnityObject(string.Empty, step, step.GetType(), (changedValue) =>
                    {
                        onValueChangedCallBack((shaco.Base.IGuideStep)changedValue);
                    });
                }
                GUILayout.EndHorizontal();
                DrawProperty(step);
            }
            GUILayout.EndVertical();
        }

        public void DrawProperty(object target)
        {
            if (null == target)
            {
                return;
            }

            var stepCheck = target as shaco.Base.IGuideStep;
            if (stepCheck != null)
                _currentFrameDrawStep = stepCheck;

            var oldColor = GUI.color;

            GUILayout.BeginVertical();
            {
                var properties = target.GetType().GetProperties(shaco.Base.GuideManagerDefine.BINDING_FLAGS_GET_PROPERTY);
                for (int i = 0; i < properties.Length; ++i)
                {
                    var propertyTmp = properties[i];
                    if (!propertyTmp.CanRead)
                        continue;

                    if (((int)propertyTmp.ReflectedType.Attributes & (int)System.Reflection.TypeAttributes.NotPublic) == 1)
                    {
                        if (!propertyTmp.IsDefined(_inputSerializeAttributeType, false))
                            continue;
                    }

                    var name = propertyTmp.Name;
                    var value = propertyTmp.GetValue(target, null);

                    //引导id不允许为空，红色高亮提示
                    bool isGuideStepIDInValid = (object)_currentFrameDrawStep.guideStepID == (object)value && string.IsNullOrEmpty(_currentFrameDrawStep.guideStepID);
                    if (isGuideStepIDInValid)
                        GUI.color = _colorRed;
                    DrawParameters(name, value, propertyTmp.GetCustomAttributes(false), propertyTmp.PropertyType, propertyTmp.CanWrite, changedValue => propertyTmp.SetValue(target, changedValue, null));
                    if (isGuideStepIDInValid)
                        GUI.color = oldColor;
                }

                var fileds = target.GetType().GetFields(shaco.Base.GuideManagerDefine.BINDING_FLAGS_GET_FILED);
                for (int i = 0; i < fileds.Length; ++i)
                {
                    var filedTmp = fileds[i];
                    string name = null;
                    if (!filedTmp.IsPublic)
                    {
                        if (!filedTmp.IsDefined(_inputSerializeAttributeType, false))
                            continue;
                    }

                    name = filedTmp.Name;
                    var value = filedTmp.GetValue(target);
                    DrawParameters(name, value, filedTmp.GetCustomAttributes(false), filedTmp.FieldType, true, changedValue => filedTmp.SetValue(target, changedValue));
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawParameters(string name, object value, object[] attributes, System.Type type, bool canWrite, System.Action<object> callBackSet)
        {
            //不再需要绘制first step id了，因为它可以根据guide step id自动生成
            if (IsChangeStepID())
                return;

            //过滤保存字段
            if (name == _ignoreSettingValue || name == _ignoreIsOpenValue || name == _ignoreIsEndValue)
            {
                return;
            }

            if (null != _currentFrameDrawStep && name == shaco.Base.Utility.ToVariableName(() => _currentFrameDrawStep.firstStepID))
            {
                return;
            }

            if (!canWrite)
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            // System.Func<object> callbackDraw = () =>
            // {
            //     return value;
            // };

            GUI.changed = false;
            object changedValue = null;

            var findAttrybutes = attributes.Where(v => v.GetType() == typeof(HeaderAttribute));
            if (null != findAttrybutes && findAttrybutes.Count() > 0)
                name = ((HeaderAttribute)findAttrybutes.First()).header;
            changedValue = GUILayoutHelper.DrawValue(name, value, type);

            if (!canWrite)
            {
                EditorGUI.EndDisabledGroup();
            }

            if (GUI.changed)
            {
                bool isChangedFirstStepID = false;
                bool isChangedGuideStepID = name == shaco.Base.Utility.ToVariableName(() => _placeholderStep.guideStepID);
                isChangedFirstStepID |= isChangedGuideStepID && shaco.GameHelper.newguide.IsFirstStep(_currentFrameDrawStep);
                isChangedFirstStepID |= name == shaco.Base.Utility.ToVariableName(() => _placeholderStep.firstStepID);

                //检查是否有重复的id
                var changedFirstStepID = changedValue.ToString();
                if (shaco.GameHelper.newguide.HasGuide(changedFirstStepID))
                {
                    Debug.LogError("GuideManagerWindow DrawProperty erorr: has duplicate first guide id=" + changedFirstStepID);
                    GUI.FocusControl(string.Empty);
                    return;
                }

                if (isChangedFirstStepID)
                {
                    //修改了初始id，需要对所有节点重新设置初始id
                    var guideStepFrom = shaco.GameHelper.newguide.GetFirstStep(value.ToString());
                    if (null == guideStepFrom)
                        Debug.LogError("GuideManagerWindow DrawProperty error: not found step id=" + value);
                    else
                    {
                        // //修改的步骤值不能是已经存在的(自己不算在内)
                        // var guideStepTo = shaco.GameHelper.newguide.GetFirstStep(changedValue.ToString());
                        // if (null != guideStepTo && guideStepFrom != guideStepTo)
                        // {
                        //     Debug.LogError("GuideManagerWindow DrawProperty error: duplicate change step id=" + guideStepTo.guideStepID);
                        //     GUI.FocusControl(string.Empty);
                        //     return;
                        // }

                        //修改了初始id，需要对所有节点重新设置初始id
                        if (isChangedFirstStepID || shaco.GameHelper.newguide.IsFirstStep(guideStepFrom))
                        {
                            SetDelayRemoveStepID(value.ToString(), changedFirstStepID);
                        }
                    }
                }
                callBackSet(changedValue);
            }
        }

        private void DrawScriptPathUnityObject(string prefixName, shaco.Base.ISetting setting, System.Type type, System.Action<shaco.Base.ISetting> callbackChanged)
        {
            GuideStepExpandInfo stepExpandInfo = null;
            UnityEngine.Object changedScriptObject = null;
            shaco.Base.ISetting retValue = setting;

            GUI.changed = false;
            if (null == setting)
            {
                var oldColor = GUI.color;
                GUI.color = _colorRed;
                changedScriptObject = (TextAsset)EditorGUILayout.ObjectField(prefixName, null, typeof(TextAsset), true);
                GUI.color = oldColor;
            }
            else
            {
                var fullTypeName = setting.ToTypeString();
                stepExpandInfo = RefreshGuideStepToUnityObjectReference(setting);

                if (null != stepExpandInfo)
                {
                    GUILayout.BeginHorizontal();
                    {
                        changedScriptObject = (TextAsset)EditorGUILayout.ObjectField(prefixName, stepExpandInfo.bindObject, typeof(TextAsset), true);
                        EditorGUI.BeginDisabledGroup(true);
                        {
                            EditorGUILayout.LabelField(setting.GetType().Name);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    var oldColor = GUI.color;
                    GUI.color = _colorRed;
                    GUILayout.Label("not found script: ", GUILayout.ExpandWidth(false));
                    EditorGUILayout.TextField(fullTypeName);
                    GUI.color = oldColor;
                }
            }
            if (GUI.changed)
            {
                if (null == changedScriptObject)
                {
                    //不允许删除脚本，所以这里不重新覆盖任何数据
                }
                else
                {
                    System.Type[] parentTypes = null;
                    if (type.IsInherited<shaco.Base.IGuideStep>())
                        parentTypes = new System.Type[] { typeof(shaco.Base.IGuideStep), typeof(shaco.Base.GuideStepDefault) };

                    var classes = shaco.Base.Utility.GetFullClassNames(changedScriptObject.ToString(), parentTypes);
                    if (classes.IsNullOrEmpty())
                    {
                        Debug.LogError(string.Format("GuideManagerWindow change value error: select script class must inherit from '{0}'", parentTypes.ToContactString(",")));
                    }
                    else if (classes.Length == 1)
                    {
                        retValue = LoadNewISetting(classes[0], setting, changedScriptObject);

                        shaco.Delay.Run(() =>
                        {
                            callbackChanged(retValue);
                        }, 0.01f);
                    }
                    else
                    {
                        GenericMenu menuTmp = new GenericMenu();
                        for (int i = 0; i < classes.Length; ++i)
                        {
                            var classNameTmp = classes[i];
                            menuTmp.AddItem(new GUIContent(classNameTmp), false, (object context) =>
                            {
                                retValue = LoadNewISetting(classNameTmp, setting, changedScriptObject);
                                callbackChanged(retValue);
                            }, null);
                        }
                        menuTmp.ShowAsContext();
                    }
                }
            }
        }

        private void ExecuteSave()
        {
            if (_isAutoOverrwiteSave && !string.IsNullOrEmpty(_loadedConfigPath))
            {
                var fullPathTmp = EditorHelper.GetFullPath(_loadedConfigPath);
                SaveToFile(fullPathTmp);
            }
            else
            {
                var savePathTmp = EditorUtility.SaveFilePanel("Select save path", _loadedConfigPath, shaco.Base.FileHelper.GetLastFileName(_loadedConfigPath), "bytes");
                if (!string.IsNullOrEmpty(savePathTmp))
                {
                    SaveToFile(savePathTmp);
                }
            }

            SaveSettings();

            //取消输入焦点
            GUI.FocusControl(string.Empty);
        }

        private void OpenCopyInputWindow(IList<shaco.Base.IGuideStep> steps)
        {
            if (GUILayout.Button("Copy", GUILayout.ExpandWidth(false)))
            {
                var inputParams = new List<ParamsInputView.InputInfo>();
                for (int i = 0; i < steps.Count; ++i)
                {
                    inputParams.Add(new ParamsInputView.InputInfo(steps[i].guideStepID, steps[i], typeof(shaco.Base.IGuideStep)));
                }
                _currentParamsInputView = ParamsInputView.OpenMuiltiMode(this, "Select Copy From Steps", (windowTarget, outputParams) =>
                {
                    CheckCloseParamsInputView();
                    if (!outputParams.IsNullOrEmpty())
                    {
                        var selectedGuidSteps = new List<shaco.Base.IGuideStep>();
                        foreach (var iter in outputParams)
                        {
                            selectedGuidSteps.Add(iter.Value as shaco.Base.IGuideStep);
                        }
                        OpenPasteInputWindow(selectedGuidSteps);
                    }
                }, inputParams.ToArray());
            }
        }

        private void OpenPasteInputWindow(List<shaco.Base.IGuideStep> copySteps)
        {
            var inputParams = new List<ParamsInputView.InputInfo>();
            var firstSteps = shaco.GameHelper.newguide.GetAllFirstStep();
            for (int i = 0; i < firstSteps.Length; ++i)
            {
                inputParams.Add(new ParamsInputView.InputInfo(firstSteps[i].guideStepID, firstSteps[i], typeof(shaco.Base.IGuideStep)));
            }
            _currentParamsInputView = ParamsInputView.OpenOneMode(this, "Select Copy To Step", (windowTarget, outputKey, outputValue) =>
            {
                if (!string.IsNullOrEmpty(outputKey))
                {
                    for (int i = 0; i < copySteps.Count; ++i)
                    {
                        if (null != copySteps[i])
                            shaco.GameHelper.newguide.AddFollowupStep(outputKey, copySteps[i]);
                    }
                }
                CheckCloseParamsInputView();
            }, inputParams.ToArray());
        }

        private shaco.Base.ISetting LoadNewISetting(string fullClassName, shaco.Base.ISetting oldSetting, UnityEngine.Object changedScriptObject)
        {
            shaco.Base.ISetting retValue = null;

            object instatiateValue = null;
            try
            {
                instatiateValue = shaco.Base.Utility.Instantiate(fullClassName);
            }
            catch
            {
                instatiateValue = null;
            }
            if (null == instatiateValue)
            {
                retValue = oldSetting;
                Debug.LogError("GuideManagerWindow DrawScriptPathUnityObject error: can't instantiate from type=" + fullClassName
                                + "\n1、type must be a 'class'"
                                + "\n2、type must have default and public construction");
            }
            else if (instatiateValue.GetType().IsInherited<UnityEngine.Object>())
            {
                retValue = oldSetting;
                Debug.LogError("GuideManagerWindow DrawScriptPathUnityObject error: can't instantiate from type=" + fullClassName
                                + "\n can't inherit from 'UnityEngine.Object'");
            }
            else
            {
                //确认引导步骤参数转移
                if (instatiateValue is shaco.Base.IGuideStep && oldSetting is shaco.Base.IGuideStep)
                {
                    var stepNew = instatiateValue as shaco.Base.IGuideStep;
                    var stepOld = oldSetting as shaco.Base.IGuideStep;
                    stepNew.firstStepID = stepOld.firstStepID;
                    stepNew.guideStepID = stepOld.guideStepID;
                }
                retValue = (shaco.Base.ISetting)instatiateValue;
            }

            if (!_guideStepToObject.ContainsKey(retValue.ToTypeString()))
                _guideStepToObject.Add(retValue.ToTypeString(), new GuideStepExpandInfo() { bindObject = changedScriptObject });
            return retValue;
        }

        private void SaveToFile(string savePath)
        {
            shaco.GameHelper.newguide.SaveAsFile(savePath);
            _loadedConfigPath = savePath;

            _loadedConfigPath = EditorHelper.FullPathToUnityAssetPath(_loadedConfigPath);
            AssetDatabase.ImportAsset(_loadedConfigPath);
        }

        private string GetDrawStepHeaderString(shaco.Base.IGuideStep guideStep)
        {
            if (null == guideStep)
                return "null";
            else
                return guideStep.guideStepID + "(" + guideStep.ToTypeString() + ")";
        }

        //设置需要延时删除的步骤id
        private void SetDelayRemoveStepID(string stepIDFrom, string stepIDTo)
        {
            CheckDelayRemoveStepID();

            _currentChangedFromID = stepIDFrom;
            _currentChangedToID = stepIDTo;
            _isFirstStepIDChangeDirty = true;
        }

        //检查并删除需要延时删除的步骤
        private void CheckDelayRemoveStepID()
        {
            if (IsChangeStepID())
            {
                //默认继承为打开状态
                shaco.GameHelper.datasave.WriteBool("GuideStepHeader" + _currentChangedFromID, true);

                //如果原id为第一个引导步骤，则同时交换他们的condition
                shaco.GameHelper.newguide.ChangeStepFirstID(_currentChangedFromID, _currentChangedToID);

                _currentChangedFromID = null;
                _currentChangedToID = null;
                _isFirstStepIDChangeDirty = false;
            }
        }

        /// <summary>
        /// 修改步骤id中
        /// </summary>
        private bool IsChangeStepID()
        {
            //允许first id为空了
            return _isFirstStepIDChangeDirty;
            // return !string.IsNullOrEmpty(_currentChangedFromID);
        }
    }
}
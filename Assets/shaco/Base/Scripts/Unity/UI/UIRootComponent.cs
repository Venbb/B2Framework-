using UnityEngine;
using System.Collections.Generic;

//---------------------------------------------------------------------------------------------------
//UI LifeCircle: OnUIInit[arg] -> OnUIOpen[arg] -> [RunOpenAnimation] -> OnUIBringToFront -> [Loop] -> [RunCloseAnimation] -> OnUIClose
//                                                                                    |
//                                                                  OnUIResume        ->        OnUIHide
//                                                                       |                         |
//                                                                       |            <-           |
//                                                                       |     OnUIBringToFront    |
//
//特殊UI事件说明
//OnPreLoad:         ui预加载接口，当调用PreLoadUI时候触发
//OnUIRefresh[arg]:    ui刷新接口，仅仅刷新ui数据
//OnUIBringToFront:  ui被置顶接口，当它上层ui隐藏或者关闭时候调用
//OnCustom:          ui自定义接口，调用当前显示在最上层的ui
//
//RunOpenAnimation & RunCloseAnimation: UI动画需要实现shaco.IUIAnimation接口，否则没有UI动画流程
//---------------------------------------------------------------------------------------------------

namespace shaco
{
    [DisallowMultipleComponent]
    public partial class UIRootComponent : MonoBehaviour, IUIRootComponent
    {
        [SerializeField]
        private partial class UIState : MonoBehaviour, IUIState
        {
            public bool isLoading { get { return _isLoading; } }
            public string key { get { return _key; } }
            public System.Type uiType { get { return _uiType; } }
            public GameObject parent { get { return _parent; } }
            public System.Collections.ObjectModel.ReadOnlyCollection<UIPrefab> uiPrefabs { get { return _uiPrefabs.AsReadOnly(); } }
            public UIRootComponent uiRoot { get { return _uiRoot; } }
            public UIEvent uiEvent { get { return _uiEvent; } }
            public UIPrefab firstUIPrefab
            {
                get
                {
                    if (!IsValid() || null == _uiPrefabs || 0 == _uiPrefabs.Count)
                    {
                        return new UIPrefab();
                    }
                    else
                        return _uiPrefabs[0];
                }
            }

            public string _key = null;
            public System.Type _uiType;
            public GameObject _parent = null;
            [SerializeField]
            public List<UIPrefab> _uiPrefabs = new List<UIPrefab>();
            public UIRootComponent _uiRoot = null;
            public UIEvent _uiEvent = new UIEvent();
            public bool _isDestroyed = false;
            public bool _isDestroying = false;
            public int _hidingUICount = 0;

            private bool _isLoading = false;

            public void SetLoading(bool value)
            {
                this._isLoading = value;
            }

            public bool IsValid(bool isPrintError = true)
            {
                //正在销毁中，可能是在播放关闭动画
                if (_isDestroying || 0 < _hidingUICount)
                    return false;

                if (_isDestroyed)
                {
                    if (isPrintError)
                        shaco.Log.Error("UIRootComponent IsValid error: is destroyed, key=" + _key, _parent);
                    return false;
                }

                if (_isLoading)
                {
                    if (isPrintError)
                        shaco.Log.Error("UIRootComponent IsValid error: is loading, key=" + _key, _parent);
                    return false;
                }
                return true;
            }

            public void RemovePrefabAtIndex(int index)
            {
                if (index < 0 || index > _uiPrefabs.Count - 1)
                {
                    Log.Error("UIRootComponent RemovePrefabAtIndex error: out of range, index=" + index + " count=" + _uiPrefabs.Count);
                    return;
                }
                _uiPrefabs.RemoveAt(index);
            }
        }

        private class UIPreLoadInfo
        {
            public UIState uiState = null;
            public List<UIPrefab> uiPrefabs = new List<UIPrefab>();
        }
        public int layerIndex
        {
            get { return _layerIndex; }
        }

        public int Count
        {
            get { return _uiDictionary.Count; }
        }

        public int loadingUICount { get { return _loadingUICount; } }

        public string uiName
        {
            get { return this.gameObject.name; }
            set { this.gameObject.name = value; }
        }

        public bool isActived
        {
            get { return this.gameObject.activeInHierarchy; }
        }

        [SerializeField]
        private int _layerIndex = 0;

        private Dictionary<string, UIState> _uiDictionary = new Dictionary<string, UIState>();
        private Dictionary<string, UIPreLoadInfo> _uiPreLoadCache = new Dictionary<string, UIPreLoadInfo>();
        private Dictionary<System.Type, bool> _uiAttributesOn = new Dictionary<System.Type, bool>();
        private GameObject _uiPreLoadCacheParent = null;
        private int _loadingUICount = 0;

        //默认ui打开超时时长，当打开ui超过这个时间后会发放超时事件
        private const float DEFAULT_OPEN_UI_TIMEOUT_SECONDS = 1.0f;

        //ui动画委托名字
        static private readonly string INTERFACE_NAME_UI_ANIMATION = typeof(IUIAnimation).FullName;

        private bool _isDestroyed = false;

        void Awake()
        {
            GameHelper.ui.AddUIRootComponent(this);
        }

        void OnDestroy()
        {
            _isDestroyed = true;
            GameHelper.ui.RemoveUIRootComponent(this);
        }

        public T OpenUI<T>(string multiVersionControlRelativePath, shaco.Base.BaseEventArg arg = null) where T : UnityEngine.Component
        {
            GameObject retValue = null;
            var key = shaco.Base.Utility.ToTypeString<T>();

            UIState uiState = null;

            //if has been added, only set activie
            if (_uiDictionary.ContainsKey(key))
            {
                uiState = (UIState)_uiDictionary[key];
                retValue = OnOpenUIResume<T>(uiState, key, arg, multiVersionControlRelativePath);
            }
            else
            {
                uiState = CreateUIStateFromLoadOrCahce(key, typeof(T));
                var topUI = GetFullScreenUI(uiState);
                AddUIState(uiState);

                uiState.uiEvent.GetOnUIInitStatck();
                BindUIStatePrefab<T>(uiState, multiVersionControlRelativePath, (loadPrefab) =>
                {
                    if (ChangeUIToScene(uiState, arg))
                    {
                        retValue = loadPrefab.prefab;
                        uiState.uiEvent.DispatchEvent(uiState.key, loadPrefab, arg, UIEvent.EventType.OnOpen);
                        CheckUIOpenAnimation(uiState, loadPrefab, () =>
                        {
                            uiState.uiEvent.DispatchEvent(uiState.key, loadPrefab, null, UIEvent.EventType.OnBringToFront);
                            CheckFullScreenUIAutoHide(uiState, topUI);
                        });
                    }
                    else
                    {
                        Log.Error("UIRootCompnent OpenUI error: can't change ui to scene, key=" + key, this);
                    }
                });
            }

            uiState.uiEvent.GetOnUIOpenStatck();
            shaco.GameHelper.uiDepth.ChangeDepthAsTopDisplay(this, uiState);
            return retValue == null ? null : retValue.GetComponent<T>();
        }

        public void HideUI<T>() where T : UnityEngine.Component
        {
            HideUI(typeof(T));
        }
        private void HideUI(System.Type uiType)
        {
            var uiState = GetUIState(uiType);
            if (uiState == null)
            {
                Log.Error("UIRootComponent HideUI error: ui is missing, key=" + uiType.ToTypeString(), this);
                return;
            }
            HideUIBase((UIState)uiState, true);
        }

        public void CloseUI<T>() where T : UnityEngine.Component
        {
            CloseUI(typeof(T));
        }
        private void CloseUI(System.Type uiType)
        {
            var uiState = GetUIState(uiType);
            if (uiState == null)
            {
                Log.Error("UIRootComponent CloseUI error: ui is missing, key=" + uiType.ToTypeString(), this);
                return;
            }
            CloseUIBase((UIState)uiState);
        }

        public void RefreshUI<T>(shaco.Base.BaseEventArg arg = null) where T : UnityEngine.Component
        {
            var uiState = GetUIState(typeof(T));
            if (uiState == null)
            {
                Log.Error("UIRootComponent RefreshUI error: ui is missing, key=" + shaco.Base.Utility.ToTypeString<T>(), this);
                return;
            }

            for (int i = 0; i < uiState.uiPrefabs.Count; ++i)
            {
                RefreshTargetUIBase(uiState, uiState.uiPrefabs[i], arg);
            }
        }

        /// <summary>
        /// 将UI显示到最上层
        /// </summary>
        public void BringToFront<T>() where T : UnityEngine.Component
        {
            BringToFront(typeof(T));
        }
        private void BringToFront(System.Type uiType)
        {
            var key = uiType.ToTypeString();

            UIState uiState = null;

            if (_uiDictionary.ContainsKey(key))
            {
                uiState = (UIState)_uiDictionary[key];
                shaco.GameHelper.uiDepth.ChangeDepthAsTopDisplay(this, uiState);
                NotifyBringToFrontUI();
            }
            else
            {
                shaco.Log.Error("UIRootComponent BringToFront error: not found ui=" + key, this);
            }
        }

        public void PreLoadUI<T>(string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, int preloadCount = 1) where T : UnityEngine.Component
        {
            var key = shaco.Base.Utility.ToTypeString<T>();
            UIPreLoadInfo uiPreLoadFind = null;

            //check parent
            if (null == _uiPreLoadCacheParent)
            {
                _uiPreLoadCacheParent = new GameObject("UIPrefabPreLoadCache");
                UnityHelper.ChangeParentLocalPosition(_uiPreLoadCacheParent, this.gameObject);
                _uiPreLoadCacheParent.transform.SetAsFirstSibling();
            }

            //check ui prefab
            for (int i = 0; i < preloadCount; ++i)
            {
                CreateUIPrefab<T>(key, multiVersionControlRelativePath, (loadPrefab) =>
                {
                    //check ui state
                    if (!_uiPreLoadCache.TryGetValue(key, out uiPreLoadFind))
                    {
                        uiPreLoadFind = new UIPreLoadInfo();
                        uiPreLoadFind.uiState = CreateUIState(key, typeof(T));
                        uiPreLoadFind.uiState.gameObject.SetActive(false);
                        UnityHelper.ChangeParentLocalPosition(uiPreLoadFind.uiState.parent, _uiPreLoadCacheParent);
                        _uiPreLoadCache.Add(key, uiPreLoadFind);
                    }

                    if (null != loadPrefab && null != loadPrefab.prefab)
                    {
                        uiPreLoadFind.uiPrefabs.Add(loadPrefab);

                        loadPrefab.prefab.SetActive(false);
                        MonoBehaviour.DontDestroyOnLoad(loadPrefab.prefab);
                        shaco.UnityHelper.ChangeParentLocalPosition(loadPrefab.prefab, _uiPreLoadCacheParent);
                        uiPreLoadFind.uiState.uiEvent.DispatchEvent(key, loadPrefab, null, UIEvent.EventType.OnPreLoad);
                    }
                    else
                    {
                        _uiPreLoadCache.Remove(key);
                    }
                });
            }
        }

        public int GetPreLoadUICount<T>() where T : UnityEngine.Component
        {
            var key = shaco.Base.Utility.ToTypeString<T>();
            UIPreLoadInfo uiPreLoadFind = null;
            _uiPreLoadCache.TryGetValue(key, out uiPreLoadFind);
            return uiPreLoadFind == null ? 0 : uiPreLoadFind.uiPrefabs.Count;
        }

        public IUIState PopupUIAndHide(params System.Type[] igoreUIs)
        {
            UIState retValue = PopupUI(true, igoreUIs);
            if (null == retValue)
            {
                shaco.Log.Error("UIRootComponent PopupUIAndHide erorr: not found active ui", this);
                return retValue;
            }

            HideUIBase(retValue, true);
            return retValue;
        }

        public IUIState PopupUIAndClose(bool onlyActivedUI, params System.Type[] igoreUIs)
        {
            UIState retValue = PopupUI(onlyActivedUI, igoreUIs);
            if (null == retValue)
            {
                shaco.Log.Error("UIRootComponent PopupUIAndClose erorr: not found ui, onlyActivedUI=" + onlyActivedUI, this);
                return retValue;
            }

            CloseUIBase(retValue);
            return retValue;
        }

        /// <summary>
        /// 恢复ui根节点下所有ui显示
        /// <param name="layerIndex">UI根节点下标</param>
        /// </summary>
        public void ResumeAllUI()
        {
            foreach (var iter in _uiDictionary)
            {
                var uiState = iter.Value;
                bool isHided = !uiState.parent.activeInHierarchy;
                uiState.parent.SetActive(true);
                if (isHided && uiState.IsValid())
                {
                    for (int i = 0; i < uiState.uiPrefabs.Count; ++i)
                    {
                        var prefabTmp = uiState.uiPrefabs[i].prefab;
                        var uiPrefabTmp = uiState.uiPrefabs[i];
                        uiState.uiEvent.DispatchEvent(uiState, null, UIEvent.EventType.OnResume);
                        CheckUIOpenAnimation(uiState, uiPrefabTmp, () =>
                        {
                            uiState.uiEvent.DispatchEvent(uiState, null, UIEvent.EventType.OnBringToFront);
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 隐藏ui根节点下所有ui
        /// <param name="layerIndex">UI根节点下标</param>
        /// </summary>
        public void HideAllUI()
        {
            foreach (var iter in _uiDictionary)
            {
                HideUIBase(iter.Value, true);
            }
        }

        /// <summary>
        /// 关闭ui根节点下所有ui
        /// <param name="layerIndex">UI根节点下标</param>
        /// </summary>
        public void CloseAllUI()
        {
            foreach (var iter in _uiDictionary)
            {
                CloseUIBase(iter.Value);
            }
        }

        public UnityEngine.GameObject GetTopUI(bool onlyActivedUI)
        {
            UnityEngine.GameObject retValue = null;
            var findUI = GetTopUIBase(onlyActivedUI);
            if (null == findUI || findUI.uiPrefabs.IsNullOrEmpty())
            {
                return null;
            }

            if (onlyActivedUI)
            {
                for (int i = findUI.uiPrefabs.Count - 1; i >= 0; --i)
                {
                    var topUIPrefab = findUI.uiPrefabs[i];
                    if (topUIPrefab.prefab.activeInHierarchy)
                    {
                        retValue = topUIPrefab.prefab;
                        break;
                    }
                }
            }
            else if (!findUI.uiPrefabs.IsNullOrEmpty())
            {
                retValue = findUI.uiPrefabs[findUI.uiPrefabs.Count - 1].prefab;
            }

            if (null == retValue)
            {
                Log.Error("UIRootComponent GetTopUI error: not found top ui, onlyActivedUI=" + onlyActivedUI);
            }
            return retValue;
        }

        public IUIState GetUIState(System.Type uiType)
        {
            var key = uiType.FullName;
            return GetUIState(key);
        }

        public IUIState[] GetAllUIState()
        {
            return _uiDictionary.Values.ToArray();
        }

        public void Foreach(System.Func<IUIState, bool> callback)
        {
            foreach (var iter in _uiDictionary)
            {
                var result = true;
                try
                {
                    result = callback(iter.Value);
                }
                catch (System.Exception e)
                {
                    shaco.Log.Error("UIRootComponent Foreach exception: key=" + iter.Value.key + " e=" + e, this);
                }

                if (!result)
                {
                    break;
                }
            }
        }

        public void ClearUI()
        {
            foreach (var iter in _uiDictionary)
            {
                Destroy(iter.Value.parent.gameObject);
            }
            _uiDictionary.Clear();
        }

        public void SetLayerAttributeOn(System.Type attributeType, bool isOn)
        {
            if (!isOn)
            {
                if (!_uiAttributesOn.ContainsKey(attributeType))
                    _uiAttributesOn.Add(attributeType, false);
            }
            else
            {
                if (_uiAttributesOn.ContainsKey(attributeType))
                    _uiAttributesOn.Remove(attributeType);
            }
        }

        private UIState GetUIState(string key)
        {
            return _uiDictionary.ContainsKey(key) ? _uiDictionary[key] : null;
        }

        private UIState PopupUI(bool onlyActivedUI, params System.Type[] igoreUI)
        {
            UIState retValue = null;
            var childount = this.transform.childCount;
            for (int i = childount - 1; i >= 0; --i)
            {
                var uiStateTarget = this.transform.GetChild(i);
                if (!onlyActivedUI || (onlyActivedUI && uiStateTarget.gameObject.activeInHierarchy))
                {
                    bool isIgnoreUI = false;
                    var uiStateTmp = uiStateTarget.GetComponent<UIState>();

                    if (null != uiStateTmp)
                    {
                        if (!uiStateTmp.IsValid(false))
                        {
                            isIgnoreUI = true;
                        }
                        else
                        {
                            for (int j = igoreUI.Length - 1; j >= 0; --j)
                            {
                                if (uiStateTmp.key == igoreUI[j].ToTypeString())
                                {
                                    isIgnoreUI = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!isIgnoreUI)
                    {
                        retValue = uiStateTmp;
                        break;
                    }
                }
            }
            return retValue;
        }

        private GameObject ResumeUIBaseWithFullScreen(UIState uiState)
        {
            var topUI = GetFullScreenUI(uiState);

            //当UILayerAllowDuplicate和UILayerFullScreen同时使用的情况下
            //顶层UI还在动画中的时候禁止自动化
            //否则会出现顶层UI和次顶层UI同时被关闭的情况
            if (null != topUI && null != topUI._uiPrefabs.Find(v => v.isAnimation))
            {
                topUI = null;
            }

            return ResumeUIBase((UIState)uiState, () =>
            {
                CheckFullScreenUIAutoHide(uiState, topUI);
            });
        }

        private GameObject ResumeUIBase(UIState uiState, System.Action callbackEnd)
        {
            GameObject retValue = null;
            bool isHided = !uiState.parent.activeInHierarchy;
            uiState.parent.SetActive(true);

            if (isHided && uiState.IsValid())
            {
                retValue = uiState.firstUIPrefab.prefab;
                uiState.uiEvent.DispatchEvent(uiState, null, UIEvent.EventType.OnResume);
                CheckUIOpenAnimation(uiState, uiState.firstUIPrefab, () =>
                {
                    uiState.uiEvent.DispatchEvent(uiState, null, UIEvent.EventType.OnBringToFront);

                    if (null != callbackEnd)
                        callbackEnd();
                });
            }
            return retValue;
        }

        private void HideUIBase(UIState uiState, bool shouldCheckAutoResume)
        {
            if (null == uiState) return;

            if (uiState.parent.activeInHierarchy && uiState.IsValid())
            {
                for (int i = 0; i < uiState.uiPrefabs.Count; ++i)
                {
                    var prefabTmp = uiState.uiPrefabs[i].prefab;
                    var uiPrefabTmp = uiState.uiPrefabs[i];

                    ++uiState._hidingUICount;
                    CheckUICloseAnimation(uiState, uiPrefabTmp, () =>
                    {
                        --uiState._hidingUICount;
                    });
                }

                bool isTopUI = GetTopUIBase(true) == uiState;
                shaco.Base.WaitFor.Run(() =>
                {
                    return uiState._hidingUICount <= 0;
                }, () =>
                {
                    uiState.uiEvent.DispatchEvent(uiState, null, UIEvent.EventType.OnHide);
                    uiState.parent.SetActive(false);

                    //通知被显示到最上层的ui界面
                    if (isTopUI)
                        NotifyBringToFrontUI();

                    if (shouldCheckAutoResume)
                        CheckFullScreenUIAutoResume(uiState);
                });
            }
        }

        private GameObject OnOpenUIResume<T>(UIState uiState, string key, shaco.Base.BaseEventArg arg, string multiVersionControlRelativePath) where T : UnityEngine.Component
        {
            GameObject retValue = null;

            //允许重复打开的UI
            if (IsDefinedAttribute(typeof(T), typeof(UILayerAllowDuplicateAttribute)))
            {
                uiState.parent.SetActive(true);
                OpenDuplicateUI<T>(uiState, key, multiVersionControlRelativePath, arg, (loadPrefab) =>
                {
                    if (null != loadPrefab)
                    {
                        retValue = loadPrefab.prefab;
                    }
                });
            }
            //如果资源还在加载，则等待加载完毕
            else if (uiState.IsValid())
            {
                ResumeUIBaseWithFullScreen(uiState);
            }
            return retValue;
        }

        private UIState GetFullScreenUI(IUIState uiState)
        {
            if (null != uiState && !IsDefinedAttribute(uiState, typeof(UILayerFullScreenAttribute)))
                return null;

            var topUI = GetTopUIBase((v) =>
            {
                if (uiState == v)
                    return false;

                if (IsDefinedAttribute(v, typeof(UILayerFullScreenAttribute)))
                    return true;
                else
                    return false;
            });
            return topUI;
        }

        private void CheckFullScreenUIAutoHide(IUIState uiState, IUIState topUI)
        {
            if (null == topUI)
                return;

            //如果该界面已经被关闭了，则不再执行自动隐藏
            if (!_uiDictionary.ContainsKey(topUI.key))
            {
                return;
            }

            HideUIBase((UIState)topUI, false);
        }

        private void CheckFullScreenUIAutoResume(UIState uiState)
        {
            if (null == uiState || !IsDefinedAttribute(uiState, typeof(UILayerFullScreenAttribute)))
                return;

            var topUI = GetFullScreenUI(uiState);
            if (null != topUI && topUI.IsValid())
            {
                ResumeUIBase(topUI, null);
            }
        }

        private void CloseUIBase(UIState uiState)
        {
            if (null == uiState) return;

            int closedUICount = 0;
            for (int i = 0; i < uiState.uiPrefabs.Count; ++i)
            {
                var prefabTmp = uiState.uiPrefabs[i].prefab;
                var uiPrefabTmp = uiState.uiPrefabs[i];

                ++closedUICount;
                CheckUICloseAnimation(uiState, uiPrefabTmp, () =>
                {
                    --closedUICount;
                });
            }

            uiState._isDestroying = true;

            //等待所有ui动画执行完毕再统一移除它们
            bool isTopUI = GetTopUIBase(true) == uiState;
            shaco.Base.WaitFor.Run(() =>
            {
                return closedUICount <= 0;
            }, () =>
            {
                uiState._isDestroying = false;

                uiState.uiEvent.DispatchEvent(uiState, null, UIEvent.EventType.OnClose);
                RemoveUI(uiState);

                //通知被显示到最上层的ui界面
                if (isTopUI)
                    NotifyBringToFrontUI();

                CheckFullScreenUIAutoResume(uiState);
                CheckAutoUnloadAsset(uiState);
            });
        }

        private void CheckAutoUnloadAsset(IUIState uiState)
        {
            var uiPrefabPath = GetUIPrefabPath(uiState.key, uiState.uiType);
            if (!string.IsNullOrEmpty(uiPrefabPath))
            {
                shaco.GameHelper.resCache.UnloadAssetBundle(uiPrefabPath, true);
            }
        }

        private void NotifyBringToFrontUI()
        {
            var topUI = GetTopUIBase(true);
            if (null != topUI)
            {
                topUI.uiEvent.DispatchEvent(topUI, null, UIEvent.EventType.OnBringToFront);
                var uiType = shaco.Base.Utility.Assembly.GetTypeWithinLoadedAssemblies(topUI.key);
            }
        }

        private string GetUIPrefabPath(string key, System.Type type)
        {
            string retValue = null;
            var attributeTmp = type.GetAttribute<UILayerCustomPrefabPathAttribute>();
            if (null == attributeTmp)
            {
                var prefabPath = UIManagerConfig.GetFullPrefabPath(key);
                var resourcesPathTmp = "Resources/";

                retValue = prefabPath;
                int indexFind = retValue.IndexOf(resourcesPathTmp);
                if (indexFind >= 0)
                {
                    retValue = retValue.Remove(0, indexFind + resourcesPathTmp.Length);
                }
            }
            else
            {
                retValue = attributeTmp.customPrefabPath;
            }
            return retValue;
        }

        private UIState CreateUIStateFromLoadOrCahce(string key, System.Type type)
        {
            UIState retValue = (UIState)GetUIState(key);

            if (null == retValue)
            {
                UIPreLoadInfo uiPreLoadFind = null;
                if (!_uiPreLoadCache.TryGetValue(key, out uiPreLoadFind))
                {
                    retValue = CreateUIState(key, type);
                }
                else
                {
                    retValue = uiPreLoadFind.uiState;
                }
                retValue.parent.SetActive(true);
            }
            else
            {
                shaco.Log.Info("UIRootComponent CreateUIStateFromLoadOrCahce error: has created ui, key=" + key, retValue.parent);
            }

            retValue._isDestroyed = false;
            return retValue;
        }

        private UIState CreateUIState(string key, System.Type uiType)
        {
            UIState retValue = null;
            var parentNew = new GameObject();
            retValue = parentNew.AddComponent<UIState>();

            parentNew.name = key;
            retValue._key = key;
            retValue._uiType = uiType;
            retValue._parent = parentNew;
            retValue._uiRoot = this;
            shaco.UnityHelper.ChangeParentLocalPosition(retValue.parent, this.gameObject);

            //for UGUI
            //如果父节点设置了RectTransform则和父节点的RectTrasnform保持一致
            var rootRectTransform = this.GetComponent<RectTransform>();
            if (null != rootRectTransform)
            {
                var parentRectTransform = retValue._parent.AddComponent<RectTransform>();
                parentRectTransform.sizeDelta = rootRectTransform.sizeDelta;
                parentRectTransform.anchoredPosition = rootRectTransform.anchoredPosition;
                parentRectTransform.anchorMin = rootRectTransform.anchorMin;
                parentRectTransform.anchorMax = rootRectTransform.anchorMax;
                parentRectTransform.pivot = rootRectTransform.pivot;
                parentRectTransform.localPosition = Vector3.zero;
            }

            return retValue;
        }

        private void BindUIStatePrefab<T>(UIState uiState, string multiVersionControlRelativePath, System.Action<UIPrefab> callbackLoadEnd) where T : UnityEngine.Component
        {
            UIPreLoadInfo removedInfo = null;
            CreateUIPrefabFromLoadOrCache<T>(uiState, multiVersionControlRelativePath, out removedInfo, (loadPrefab) =>
            {
                var retValue = loadPrefab;
                if (null == retValue)
                    Log.Error("UIRootComponent BindUIStatePrefab error: can't creat ui, key=" + uiState.key, this);
                else
                    uiState._uiPrefabs.Add(retValue);
                callbackLoadEnd(retValue);
            });
        }

        private bool ChangeUIToScene(UIState uiState, shaco.Base.BaseEventArg arg)
        {
            if (!uiState.uiEvent.DispatchEvent(uiState, arg, UIEvent.EventType.OnInit))
                return false;

            shaco.UnityHelper.ChangeParentLocalPosition(uiState.parent, this.gameObject);
            bool hadValidUI = false;

            for (int i = uiState.uiPrefabs.Count - 1; i >= 0; --i)
            {
                if (null != uiState.uiPrefabs[i].prefab)
                {
                    shaco.UnityHelper.ChangeParentLocalPosition(uiState.uiPrefabs[i].prefab.gameObject, uiState.parent);
                    hadValidUI = true;
                }
            }
            uiState.parent.layer = this.gameObject.layer;
            return hadValidUI;
        }

        private void CreateUIPrefabFromLoadOrCache<T>(IUIState uiState, string multiVersionControlRelativePath, out UIPreLoadInfo removedInfo, System.Action<UIPrefab> callbackLoadEnd) where T : UnityEngine.Component
        {
            removedInfo = null;
            if (_uiPreLoadCache.ContainsKey(uiState.key))
            {
                var uiCacheTmp = _uiPreLoadCache[uiState.key];

                if (uiCacheTmp.uiPrefabs.IsNullOrEmpty())
                {
                    shaco.Log.Error("UIRootCompnent CreateUIPrefabFromLoadOrCache error: not found prefab in cache, key=" + uiState.key, uiState.parent);
                    return;
                }

                var retValue = uiCacheTmp.uiPrefabs[uiCacheTmp.uiPrefabs.Count - 1];
                uiCacheTmp.uiPrefabs.RemoveAt(uiCacheTmp.uiPrefabs.Count - 1);

                retValue.prefab.SetActive(true);

                if (uiCacheTmp.uiPrefabs.Count == 0)
                {
                    removedInfo = uiCacheTmp;
                    _uiPreLoadCache.Remove(uiState.key);
                }
                callbackLoadEnd(retValue);

                var requestLoadAsync = IsDefinedAttribute(typeof(T), typeof(shaco.UILayerOpenAsyncAttribute));
                if (requestLoadAsync && shaco.GameHelper.Event.HasEventID<shaco.UIStateChangedEvents.OnUIOpenEndEvent<T>>())
                {
                    this.InvokeEvent(new shaco.UIStateChangedEvents.OnUIOpenEndEvent<T>()
                    {
                        uiTarget = null == retValue || null == retValue.prefab ? null : retValue.prefab.GetComponent<T>()
                    });
                }
            }
            else
            {
                uiState.SetLoading(true);
                CreateUIPrefab<T>(uiState.key, multiVersionControlRelativePath, (loadPrefab) =>
                {
                    if (null != loadPrefab && null != loadPrefab.prefab)
                    {
                        uiState.SetLoading(false);
                        callbackLoadEnd(loadPrefab);
                    }
                });
            }
        }

        private void CreateUIPrefab<T>(string key, string multiVersionControlRelativePath, System.Action<UIPrefab> callbackLoadEnd) where T : UnityEngine.Component
        {
            var retValue = new UIPrefab();
            var pathTmp = GetUIPrefabPath(key, typeof(T));

            var requestLoadAsync = IsDefinedAttribute(typeof(T), typeof(shaco.UILayerOpenAsyncAttribute));
            ActionBase timeoutCheckAction = null;
            if (requestLoadAsync)
            {
                var haveInvokedTimeoutEvent = false;
                if (shaco.GameHelper.Event.HasEventID<shaco.UIStateChangedEvents.OpenUITimeoutStartEvent>())
                {
                    var customTimeoutSecondsAttribute = typeof(T).GetAttribute(typeof(shaco.UILayerCustomOpenTimeoutSecondsAttribute)) as shaco.UILayerCustomOpenTimeoutSecondsAttribute;
                    var timeoutSeconds = null == customTimeoutSecondsAttribute ? DEFAULT_OPEN_UI_TIMEOUT_SECONDS : customTimeoutSecondsAttribute.seconds;
                    timeoutCheckAction = shaco.Delay.Run(() =>
                    {
                        haveInvokedTimeoutEvent = true;
                        timeoutCheckAction = null;
                        this.InvokeEvent(new shaco.UIStateChangedEvents.OpenUITimeoutStartEvent() { uiKey = key });
                    }, timeoutSeconds);
                }

                ++_loadingUICount;
                shaco.GameHelper.res.LoadResourcesOrLocalAsync<GameObject>(pathTmp, (loadObj) =>
                {
                    --_loadingUICount;

                    //关闭可能还在运行的超时检查
                    if (null != timeoutCheckAction)
                    {
                        timeoutCheckAction.StopMe();
                        timeoutCheckAction = null;
                    }

                    //资源加载完毕，表示超时检查也结束了
                    if (haveInvokedTimeoutEvent && shaco.GameHelper.Event.HasEventID<shaco.UIStateChangedEvents.OpenUITimeoutEndEvent>())
                    {
                        this.InvokeEvent(new shaco.UIStateChangedEvents.OpenUITimeoutEndEvent() { uiKey = key });
                    }

                    //如果资源异步加载完毕后，但是UI根结点已经被销毁的情况下，不执行后续逻辑
                    if (_isDestroyed)
                    {
                        shaco.Log.Error("UIRootCompnent CreateUIPrefab aysnc load ui error: UIRootComponent is already destroyed", this);
                        return;
                    }

                    if (null != loadObj)
                    {
                        var componets = ((GameObject)MonoBehaviour.Instantiate(loadObj)).GetComponents<UnityEngine.Component>();
                        retValue.SetComponents(componets);
                    }

                    bool canRealoadUI = false;
                    shaco.UIStateChangedEvents.OpenAysncUIErrorEvent openAysncUIErrorEvent = null;
                    if (null == loadObj)
                    {
                        if (shaco.GameHelper.Event.HasEventID<shaco.UIStateChangedEvents.OpenAysncUIErrorEvent>())
                        {
                            openAysncUIErrorEvent = new shaco.UIStateChangedEvents.OpenAysncUIErrorEvent();
                            openAysncUIErrorEvent.uiKey = key;
                            this.InvokeEvent(openAysncUIErrorEvent);
                            canRealoadUI = true;
                        }
                    }

                    //只有界面加载失败和外部要求重新加载情况下才重新加载
                    if (canRealoadUI && null != openAysncUIErrorEvent)
                    {
                        openAysncUIErrorEvent.requestReloadUI = () =>
                        {
                            CreateUIPrefab<T>(key, multiVersionControlRelativePath, callbackLoadEnd);
                        };
                    }
                    //加载成功了则直接回调
                    else
                    {
                        callbackLoadEnd(retValue);
                    }

                    if (shaco.GameHelper.Event.HasEventID<shaco.UIStateChangedEvents.OnUIOpenEndEvent<T>>())
                    {
                        this.InvokeEvent(new shaco.UIStateChangedEvents.OnUIOpenEndEvent<T>()
                        {
                            uiTarget = null == loadObj ? null : loadObj.GetComponent<T>()
                        });
                    }
                }, null, multiVersionControlRelativePath);
            }
            else
            {
                var newPrefab = shaco.GameHelper.res.LoadResourcesOrLocal<GameObject>(pathTmp, multiVersionControlRelativePath);
                if (null != newPrefab)
                {
                    var componets = ((GameObject)MonoBehaviour.Instantiate(newPrefab)).GetComponents<UnityEngine.Component>();
                    retValue.SetComponents(componets);
                }

                callbackLoadEnd(retValue);

                if (shaco.GameHelper.Event.HasEventID<shaco.UIStateChangedEvents.OnUIOpenEndEvent<T>>())
                {
                    this.InvokeEvent(new shaco.UIStateChangedEvents.OnUIOpenEndEvent<T>()
                    {
                        uiTarget = null == newPrefab ? null : newPrefab.GetComponent<T>()
                    });
                }
            }
        }

        private bool AddUIState(UIState uiState)
        {
            if (_uiDictionary.ContainsKey(uiState.key))
            {
                Log.Error("UIRootComponent AddUI error: the ui has been added, key=" + uiState.key, uiState.parent);
                return false;
            }
            _uiDictionary.Add(uiState.key, uiState);
            return true;
        }

        private void RemoveUI(IUIState uiState)
        {
            uiState.uiEvent.RestStackLocations();

            _uiDictionary.Remove(uiState.key);
            uiState.parent.SetActive(false);

            if (!uiState.IsValid())
                return;

            if (uiState is UIState)
                ((UIState)uiState)._isDestroyed = true;
            UnityHelper.SafeDestroy(uiState.parent.gameObject);

            UIPreLoadInfo uiPreLoadFind = null;
            if (_uiPreLoadCache.TryGetValue(uiState.key, out uiPreLoadFind))
            {
                for (int i = uiPreLoadFind.uiPrefabs.Count - 1; i >= 0; --i)
                {
                    UnityHelper.SafeDestroy(uiPreLoadFind.uiPrefabs[i].prefab);
                }
                uiPreLoadFind.uiPrefabs.Clear();
                _uiPreLoadCache.Remove(uiState.key);
            }
        }

        private bool IsDefinedAttribute(IUIState uiState, System.Type attributeType)
        {
            var retValue = (null == uiState || null == uiState.uiType) ? false : uiState.uiType.IsDefined(attributeType, false);
            if (retValue && _uiAttributesOn.ContainsKey(attributeType))
            {
                retValue = _uiAttributesOn[attributeType];
            }
            return retValue;
        }

        private bool IsDefinedAttribute(System.Type uiType, System.Type attributeType)
        {
            var retValue = uiType.IsDefined(attributeType, false);
            if (retValue && _uiAttributesOn.ContainsKey(attributeType))
            {
                retValue = _uiAttributesOn[attributeType];
            }
            return retValue;
        }

        private UIState GetTopUIBase(bool onlyActivedUI)
        {
            if (onlyActivedUI)
                return GetTopUIBase(v => v.IsValid(false) && HaveAcitveUIInGroup(v));
            else
                return GetTopUIBase(v => v.IsValid(false));
        }

        private bool HaveAcitveUIInGroup(IUIState uiState)
        {
            for (int i = uiState.uiPrefabs.Count - 1; i >= 0; --i)
            {
                if (uiState.uiPrefabs[i].prefab.activeInHierarchy)
                    return true;
            }
            return false;
        }

        private UIState GetTopUIBase(System.Func<IUIState, bool> conditionCallBack = null)
        {
            UIState retValue = null;
            for (int i = this.transform.childCount - 1; i >= 0; --i)
            {
                var uiStateTarget = this.transform.GetChild(i);
                var uiState = uiStateTarget.GetComponent<UIState>();
                if (null != uiState && (null == conditionCallBack || conditionCallBack(uiState)))
                {
                    retValue = uiState;
                    break;
                }
            }
            return retValue;
        }

        private void CheckUIOpenAnimation(UIState uiState, UIPrefab targetPrefab, System.Action callbackEnd)
        {
            var findInterface = uiState.uiType.GetInterface(INTERFACE_NAME_UI_ANIMATION);
            if (null == findInterface)
            {
                callbackEnd();
                return;
            }

            var uiAnimation = targetPrefab.prefab.GetComponent(typeof(IUIAnimation)) as IUIAnimation;
            if (null == uiAnimation)
            {
                Log.Error("UIRootCompnent CheckUIOpenAnimation erorr: not found componnet 'IUIAnimation' in target=" + targetPrefab + " ui type=" + uiState.uiType.ToTypeString(), uiState.parent);
                return;
            }

            //如果UI正在播放动画，则等待它播放完毕为止
            if (targetPrefab.isAnimation)
            {
                shaco.Base.WaitFor.Run(() =>
                {
                    return !targetPrefab.isAnimation;
                }, () =>
                {
                    if (!uiState._isDestroyed)
                    {
                        targetPrefab.isAnimation = true;
                        uiAnimation.RunOpenAnimation(() =>
                        {
                            //可能在播放打开动画中又调用了关闭方法，所以在打开动画完毕后需要再次SetActive(true)
                            uiState.parent.SetActive(true);
                            
                            callbackEnd();
                            targetPrefab.isAnimation = false;
                        });
                    }
                });
            }
            else
            {
                targetPrefab.isAnimation = true;
                uiAnimation.RunOpenAnimation(() =>
                {
                    //可能在播放打开动画中又调用了关闭方法，所以在打开动画完毕后需要再次SetActive(true)
                    uiState.parent.SetActive(true);

                    callbackEnd();
                    targetPrefab.isAnimation = false;
                });
            }
        }

        private void CheckUICloseAnimation(UIState uiState, UIPrefab targetPrefab, System.Action callbackEnd)
        {
            var findInterface = uiState.uiType.GetInterface(INTERFACE_NAME_UI_ANIMATION);
            if (null == findInterface)
            {
                callbackEnd();
                return;
            }

            var uiAnimation = targetPrefab.prefab.GetComponent(typeof(IUIAnimation)) as IUIAnimation;
            if (null == uiAnimation)
            {
                Log.Error("UIRootCompnent CheckUICloseAnimation erorr: not found componnet 'IUIAnimation' in target=" + targetPrefab + " ui type=" + uiState.uiType.ToTypeString(), uiState.parent);
                return;
            }

            System.Action callbackRunCloseAnimation = () =>
            {
                if (!uiState._isDestroyed)
                {
                    if (targetPrefab.prefab.activeInHierarchy)
                    {
                        targetPrefab.isAnimation = true;
                        uiAnimation.RunCloseAnimation(() =>
                        {
                            callbackEnd();
                            targetPrefab.isAnimation = false;
                        });
                    }
                    //如果对象是已经隐藏状态了，则看不到任何动画，可以不播放动画并立即回调
                    else
                    {
                        callbackEnd();
                    }
                }
            };

            //如果UI正在播放动画，则等待它播放完毕为止
            if (targetPrefab.isAnimation)
            {
                shaco.Base.WaitFor.Run(() =>
                {
                    return !targetPrefab.isAnimation;
                }, () =>
                {
                    callbackRunCloseAnimation();
                });
            }
            else
            {
                callbackRunCloseAnimation();
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class UIManager : shaco.IUIManager
    {
        private Dictionary<int, IUIRootComponent> _roots = new Dictionary<int, IUIRootComponent>();

        public T OpenUI<T>(shaco.Base.BaseEventArg arg = null) where T : UnityEngine.Component
        {
            var typeTmp = typeof(T);
            var root = GetUIRootComponent(SafeGetLayerIndexWithAttribute(typeTmp));
            return null == root ? null : root.OpenUI<T>(SafeGetMultiVersionControlRelativePathWithAttribute(typeTmp), arg);
        }

        public void HideUI<T>() where T : UnityEngine.Component
        {
            var root = GetUIRootComponent(SafeGetLayerIndexWithAttribute(typeof(T)));
            if (null != root)
                root.HideUI<T>();
        }

        public void CloseUI<T>() where T : UnityEngine.Component
        {
            var root = GetUIRootComponent(SafeGetLayerIndexWithAttribute(typeof(T)));
            if (null != root)
                root.CloseUI<T>();
        }

        public void RefreshUI<T>(shaco.Base.BaseEventArg arg = null) where T : UnityEngine.Component
        {
            var root = GetUIRootComponent(SafeGetLayerIndexWithAttribute(typeof(T)));
            if (null != root)
                root.RefreshUI<T>(arg);
        }

        public void BringToFront<T>() where T : UnityEngine.Component
        {
            var root = GetUIRootComponent(SafeGetLayerIndexWithAttribute(typeof(T)));
            if (null != root)
                root.BringToFront<T>();
        }

        public void HideUITarget(UnityEngine.Component target)
        {
            var root = GetUIRootComponent(SafeGetLayerIndexWithAttribute(target.GetType()));
            if (null != root)
                root.HideUITarget(target);
        }

        public void CloseUITarget(UnityEngine.Component target)
        {
            var root = GetUIRootComponent(SafeGetLayerIndexWithAttribute(target.GetType()));
            if (null != root)
                root.CloseUITarget(target);
        }

        public void BringToFrontTarget(UnityEngine.Component target)
        {
            var root = GetUIRootComponent(SafeGetLayerIndexWithAttribute(target.GetType()));
            if (null != root)
                root.BringToFrontTarget(target);
        }

        public void PreLoadUI<T>(int preloadCount = 1) where T : UnityEngine.Component
        {
            var typeTmp = typeof(T);
            var root = GetUIRootComponent(SafeGetLayerIndexWithAttribute(typeTmp));
            if (null != root)
                root.PreLoadUI<T>(SafeGetMultiVersionControlRelativePathWithAttribute(typeTmp), preloadCount);
        }

        public void PreLoadUIOnlyOne<T>() where T : UnityEngine.Component
        {
            //如果之前没有预加载过该ui，则预加载一次
            if (GetPreLoadUICount<T>() == 0)
            {
                PreLoadUI<T>(1);
            }
        }

        public int GetPreLoadUICount<T>() where T : UnityEngine.Component
        {
            var root = GetUIRootComponent(SafeGetLayerIndexWithAttribute(typeof(T)));
            return null == root ? 0 : root.GetPreLoadUICount<T>();
        }

        public shaco.IUIState PopupUIAndHide(int layerIndex = 0, params System.Type[] igoreUIs)
        {
            var root = GetUIRootComponent(layerIndex);
            if (null == root)
                return null;
            else
            {
                var retValue = root.PopupUIAndHide(igoreUIs);
                if (null == retValue)
                    shaco.Log.Error("UIManager PopupUIAndHide erorr: not found active ui, layerIndex=" + layerIndex);
                return retValue;
            }
        }

        public shaco.IUIState PopupUIAndClose(bool onlyActivedUI, int layerIndex = 0, params System.Type[] igoreUIs)
        {
            var root = GetUIRootComponent(layerIndex);
            if (null == root)
                return null;
            else
            {
                var retValue = root.PopupUIAndClose(onlyActivedUI, igoreUIs);
                if (null == retValue)
                    shaco.Log.Error("UIManager PopupUIAndClose erorr: not found active ui, layerIndex=" + layerIndex);
                return retValue;
            }
        }

        /// <summary>
        /// 恢复ui根节点下所有ui显示
        /// <param name="layerIndex">UI根节点下标</param>
        /// </summary>
        public void ResumeAllUI(int layerIndex)
        {
            var root = GetUIRootComponent(layerIndex);
            if (null != root)
            {
                root.ResumeAllUI();
            }
        }

        /// <summary>
        /// 隐藏ui根节点下所有ui
        /// <param name="layerIndex">UI根节点下标</param>
        /// <param name="arg">事件参数</param>
        /// </summary>
        public void HideAllUI(int layerIndex)
        {
            var root = GetUIRootComponent(layerIndex);
            if (null != root)
            {
                root.HideAllUI();
            }
        }

        /// <summary>
        /// 关闭ui根节点下所有ui
        /// <param name="layerIndex">UI根节点下标</param>
        /// <param name="arg">事件参数</param>
        /// </summary>
        public void CloseAllUI(int layerIndex)
        {
            var root = GetUIRootComponent(layerIndex);
            if (null != root)
            {
                root.CloseAllUI();
            }
        }

        public void AddUIRootComponent(IUIRootComponent uiRoot)
        {
            if (!this._roots.ContainsKey(uiRoot.layerIndex))
            {
                uiRoot.uiName = uiRoot.uiName + "[layer:" + uiRoot.layerIndex + "]";
                this._roots.Add(uiRoot.layerIndex, uiRoot);
            }
        }

        public void RemoveUIRootComponent(IUIRootComponent uiRoot)
        {
            if (this._roots.Count == 0)
                return;
                
            if (!this._roots.ContainsKey(uiRoot.layerIndex))
            {
                Log.Error("UIManager RemoveUIRootComponent error: not find uiRoot=" + uiRoot + " layerIndex=" + uiRoot.layerIndex);
            }
            else
            {
                this._roots.Remove(uiRoot.layerIndex);
            }
        }

        public UnityEngine.GameObject GetTopUI(bool onlyActivedUI, int layerIndex = 0)
        {
            var root = GetUIRootComponent(layerIndex);
            return null == root ? null : root.GetTopUI(onlyActivedUI);
        }

        public bool IsTopUI(string uiName, bool onlyActivedUI = true)
        {
            var typeTmp = shaco.Base.Utility.Assembly.GetTypeWithinLoadedAssemblies(uiName);
            if (null == typeTmp)
            {
                shaco.Log.Error("UIManager IsTopUI error: not found ui script name=" + uiName);
                return false;
            }

            var layerIndex = SafeGetLayerIndexWithAttribute(typeTmp);
            if (_roots.IsOutOfRange(layerIndex))
                return false;
            
            var topUI = GetTopUI(onlyActivedUI, layerIndex);
            if (null == topUI)
                return false;
                
            return topUI.GetComponent(typeTmp) != null;
        }

        public T GetUIComponent<T>(bool onlyActivedUI = false) where T : UnityEngine.Component
        {
            return (T)GetUIComponent(typeof(T), onlyActivedUI);
        }

        public List<T> GetUIComponents<T>(bool onlyActivedUI = false) where T : UnityEngine.Component
        {
            return (List<T>)GetUIComponents(typeof(T), onlyActivedUI).ConvertList(v => (T)v);
        }

        public UnityEngine.Component GetUIComponent(System.Type uiType, bool onlyActivedUI = true)
        {
            UnityEngine.Component retValue = null;
            var layerIndex = SafeGetLayerIndexWithAttribute(uiType);
            if (_roots.IsOutOfRange(layerIndex))
                return null;

            var root = GetUIRootComponent(layerIndex);
            if (null != root)
            {
                var uiStateTmp = root.GetUIState(uiType);
                if (null != uiStateTmp && uiStateTmp.uiPrefabs.Count > 0)
                {
                    if (uiStateTmp.IsValid())
                    {
                        var findUIComponent = uiStateTmp.firstUIPrefab.prefab.GetComponent(uiType);
                        if (!onlyActivedUI || (onlyActivedUI && findUIComponent.gameObject.activeInHierarchy))
                        {
                            if (uiStateTmp.IsValid())
                                retValue = findUIComponent;
                        }
                    }
                }
            }
            return retValue;
        }

        public List<UnityEngine.Component> GetUIComponents(System.Type uiType, bool onlyActivedUI = true)
        {
            var retValue = new List<UnityEngine.Component>();
            var root = GetUIRootComponent(SafeGetLayerIndexWithAttribute(uiType));
            if (null != root)
            {
                var uiStateTmp = root.GetUIState(uiType);
                if (null != uiStateTmp)
                {
                    for (int i = 0; i < uiStateTmp.uiPrefabs.Count; ++i)
                    {
                        var componentTmp = uiStateTmp.uiPrefabs[i].prefab.GetComponent(uiType);
                        if (null != componentTmp)
                        {
                            if (!onlyActivedUI || (onlyActivedUI && componentTmp.gameObject.activeInHierarchy))
                            {
                                if (uiStateTmp.IsValid())
                                    retValue.Add(componentTmp);
                            }
                        }
                    }
                }
            }
            return retValue;
        }

        public GameObject GetUIGameObjectWithTag(string tag)
        {
            GameObject retValue = null;

            ForeachAllUIGameObject((GameObject obj) =>
            {
                if (obj.tag == tag)
                {
                    retValue = obj;
                    return false;
                }
                else
                    return true;
            });
            return retValue;
        }

        public List<GameObject> GetUIGameObjectsWithTag(string tag)
        {
            var retValue = new List<GameObject>();

            ForeachAllUIGameObject((GameObject obj) =>
            {
                if (obj.tag == tag)
                {
                    retValue.Add(obj);
                }
                return true;
            });
            return retValue;
        }

        public GameObject GetUIGameObjectWithLayer(int layer)
        {
            GameObject retValue = null;

            ForeachAllUIGameObject((GameObject obj) =>
            {
                if (obj.layer == layer)
                {
                    retValue = obj;
                    return false;
                }
                else
                    return true;
            });
            return retValue;
        }

        public List<GameObject> GetUIGameObjectsWithLayer(int layer)
        {
            var retValue = new List<GameObject>();

            ForeachAllUIGameObject((GameObject obj) =>
            {
                if (obj.layer == layer)
                {
                    retValue.Add(obj);
                }
                return true;
            });
            return retValue;
        }

        public void ForeachActiveUIRoot(System.Action<IUIRootComponent> callback)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Log.Warning("UIManager warning: 'ForeachActiveUIRoot' cannot be used when the editor is not running");
                return;
            }
#endif
            if (this._roots.Count == 0)
            {
                // Log.Error("UIManaer error: not found 'UIRootComponent', please check 'UIManager.AddUIRootComponent'");
                return;
            }

            var beginIter = this._roots.GetEnumerator();
            while (beginIter.MoveNext())
            {
                var uiRootTmp = beginIter.Current.Value;
                if (null != uiRootTmp && uiRootTmp.isActived)
                {
                    try
                    {
                        callback(uiRootTmp);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("UIManager ForeachActiveUIRoot error: callback=" + callback + " e=" + e);
                    }
                }
            }
        }

        public int GetUIRootCount()
        {
            return this._roots.Count;
        }

        public IUIRootComponent GetUIRootComponent(int layerIndex = 0)
        {
            IUIRootComponent retValue = null;

            if (this._roots.IsOutOfRange(layerIndex))
                Log.Error("UIManager GetUIRootComponent error: out of range index=" + layerIndex + " count=" + this._roots.Count);
            else
                retValue = this._roots[layerIndex];

            if (null == retValue)
                Log.Error("UIManager GetUIRootComponent error: There is no UIRoot at the current layer index=" + layerIndex);
            return retValue;
        }

        public void ClearUI()
        {
            ForeachActiveUIRoot((IUIRootComponent uiRoot) =>
            {
                uiRoot.ClearUI();
            });
        }

        public bool IsUILoading()
        {
            foreach (var iter in _roots)
            {
                if (iter.Value.loadingUICount > 0)
                    return true;
            }
            return false;
        }

        public void SetLayerAttributeOn<T>(System.Type attributeType, bool isOn) where T : UnityEngine.Component
        {
            var root = GetUIRootComponent(SafeGetLayerIndexWithAttribute(typeof(T)));
            if (null != root)
            {
                root.SetLayerAttributeOn(attributeType, isOn);
            }
        }

        public void SetAllLayerAttributeOn(System.Type attributeType, bool isOn)
        {
            foreach (var iter in this._roots)
            {
                iter.Value.SetLayerAttributeOn(attributeType, isOn);
            }
        }

        private void ForeachAllUIGameObject(System.Func<GameObject, bool> callback)
        {
            var beginIter = this._roots.GetEnumerator();
            while (beginIter.MoveNext())
            {
                var rootTmp = beginIter.Current.Value;
                var allUIStateTmp = rootTmp.GetAllUIState();
                if (allUIStateTmp.IsNullOrEmpty())
                    continue;
                    
                for (int j = allUIStateTmp.Length - 1; j >= 0; --j)
                {
                    var uiStateTmp = allUIStateTmp[j];
                    bool needBreak = false;
                    
                    for (int k = uiStateTmp.uiPrefabs.Count - 1; k >= 0; --k)
                    {
                        var gameObjectTmp = uiStateTmp.uiPrefabs[k].prefab;

                        if (!callback(gameObjectTmp))
                        {
                            needBreak = true;
                        }

                        UnityHelper.ForeachChildren(gameObjectTmp, (int index, GameObject child) =>
                        {
                            if (!callback(child))
                            {
                                needBreak = true;
                                return false;
                            }
                            else
                                return true;
                        });

                        if (needBreak)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private int SafeGetLayerIndexWithAttribute(System.Type type)
        {
            var attribute = type.GetAttribute<shaco.UILayerIndexAttribute>();
            return null == attribute ? 0 : attribute.layerIndex;
        }

        private string SafeGetMultiVersionControlRelativePathWithAttribute(System.Type type)
        {
            var attribute = type.GetAttribute<shaco.UILayerMultiVersionControlRelativePathAttribute>();
            return null == attribute ? string.Empty : attribute.multiVersionControlRelativePath;
        }

        private string SafeGetCustomPrefabPathPathWithAttribute(System.Type type)
        {
            var attribute = type.GetAttribute<shaco.UILayerCustomPrefabPathAttribute>();
            return null == attribute ? string.Empty : attribute.customPrefabPath;
        }
    }
}
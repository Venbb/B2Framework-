using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public partial class UIRootComponent : MonoBehaviour, IUIRootComponent
    {
        public void HideUITarget(UnityEngine.Component target)
        {
            var allowedDuplicate = IsDefinedAttribute(target.GetType(), typeof(UILayerAllowDuplicateAttribute));
            if (!allowedDuplicate)
            {
                HideUI(target.GetType());
            }
            else
            {
                var key = target.ToTypeString();
                var uiState = GetUIState(key);
                if (uiState == null)
                {
                    Log.Error("UIRootComponent HideUITarget error: ui is missing, key=" + key, target);
                    return;
                }

                HideUITargetBase(uiState, target);
            }
        }

        public void CloseUITarget(UnityEngine.Component target)
        {
            var allowedDuplicate = IsDefinedAttribute(target.GetType(), typeof(UILayerAllowDuplicateAttribute));
            if (!allowedDuplicate)
            {
                CloseUI(target.GetType());
            }
            else
            {
                var key = target.ToTypeString();
                var uiState = GetUIState(key);
                if (uiState == null)
                {
                    Log.Error("UIRootComponent CloseUITarget error: ui is missing, key=" + key, target);
                    return;
                }
                CloseUITargetBase((UIState)uiState, target);
            }
        }

        public void BringToFrontTarget(UnityEngine.Component target)
        {
            BringToFront(target.GetType());
        }

        private void OpenDuplicateUI<T>(UIState uiState, string key, string multiVersionControlRelativePath, shaco.Base.BaseEventArg arg, System.Action<UIPrefab> callbackLoadEnd) where T : UnityEngine.Component
        {
            if (null == uiState) return;

            UIPreLoadInfo removedInfo = null;

            uiState.uiEvent.GetOnUIInitStatck();
            var topUI = GetFullScreenUI(uiState);

            //当UILayerAllowDuplicate和UILayerFullScreen同时使用的情况下
            //顶层UI还在动画中的时候禁止自动化
            //否则会出现顶层UI和次顶层UI同时被关闭的情况
            if (null != topUI && null != topUI._uiPrefabs.Find(v => v.isAnimation))
            {
                topUI = null;
            }
            
            CreateUIPrefabFromLoadOrCache<T>(uiState, multiVersionControlRelativePath, out removedInfo, (loadPrefab) =>
            {
                UIPrefab retValue = loadPrefab;
                if (null == retValue)
                {
                    Log.Error("UIRootComponent OpenDuplicateUI error: can't creat ui, key=" + key, this);
                    callbackLoadEnd(retValue);
                    return;
                }

                uiState._uiPrefabs.Add(retValue);

                ChangeUITargetToScene(uiState, retValue, arg, uiState.parent);

                retValue.prefab.SetActive(true);
                callbackLoadEnd(retValue);

                uiState.uiEvent.DispatchEvent(uiState.key, retValue, arg, UIEvent.EventType.OnOpen);
                CheckUIOpenAnimation(uiState, retValue, () =>
                {
                    CheckFullScreenUIAutoHide(uiState, topUI);
                    uiState.uiEvent.DispatchEvent(uiState.key, retValue, null, UIEvent.EventType.OnBringToFront);
                });
            });
            uiState.uiEvent.GetOnUIOpenStatck();
        }

        private void HideUITargetBase(UIState uiState, UnityEngine.Component target)
        {
            if (null == uiState || !uiState.IsValid()) return;

            var findUITarget = GetUITargetInUIState(uiState, target);
            if (null != findUITarget && findUITarget.prefab.activeInHierarchy)
            {
                ++uiState._hidingUICount;
                CheckUICloseAnimation(uiState, findUITarget, () =>
                {
                    var key = target.ToTypeString();
                    uiState.uiEvent.DispatchEvent(key, findUITarget, null, UIEvent.EventType.OnHide);
                    findUITarget.prefab.SetActive(false);
                    --uiState._hidingUICount;
                });
            }
        }

        private void CloseUITargetBase(UIState uiState, UnityEngine.Component target)
        {
            if (null == uiState || !uiState.IsValid()) return;

            var findUITarget = GetUITargetInUIState(uiState, target);
            if (null != findUITarget)
            {
                uiState._isDestroying = true;
                CheckUICloseAnimation(uiState, findUITarget, () =>
                {
                    uiState._isDestroying = false;

                    var key = target.ToTypeString();
                    uiState.uiEvent.DispatchEvent(key, findUITarget, null, UIEvent.EventType.OnClose);
                    RemoveUITarget(uiState, findUITarget);
                });
            }
            CheckAutoUnloadAsset(uiState);
        }

        private UIPrefab GetUITargetInUIState(IUIState uiState, UnityEngine.Component target)
        {
            UIPrefab retValue = null;
            if (null == uiState || !uiState.IsValid())
            {
                return retValue;
            }

            for (int i = uiState.uiPrefabs.Count - 1; i >= 0; --i)
            {
                var prefabTmp = uiState.uiPrefabs[i].prefab;
                if (prefabTmp == target.gameObject)
                {
                    retValue = uiState.uiPrefabs[i];
                    break;
                }
            }
            return retValue;
        }

        private void RemoveUITarget(IUIState uiState, UIPrefab uiPrefab)
        {
            for (int i = uiState.uiPrefabs.Count - 1; i >= 0; --i)
            {
                var prefabTmp = uiState.uiPrefabs[i];
                if (prefabTmp == uiPrefab)
                {
                    prefabTmp.prefab.gameObject.SetActive(false);
                    MonoBehaviour.Destroy(prefabTmp.prefab.gameObject);
                    uiState.RemovePrefabAtIndex(i);
                    break;
                }
            }

            if (uiState.uiPrefabs.Count == 0)
            {
                RemoveUI(uiState);
            }
        }

        private void ChangeUITargetToScene(UIState uiState, UIPrefab newPrefab, shaco.Base.BaseEventArg arg, GameObject parent)
        {
            uiState.uiEvent.DispatchEvent(uiState.key, newPrefab, arg, UIEvent.EventType.OnInit);
            shaco.UnityHelper.ChangeParentLocalPosition(newPrefab.prefab, parent);
        }

        private void RefreshTargetUIBase(IUIState uiState, UIPrefab prefab, shaco.Base.BaseEventArg arg)
        {
            if (null == uiState || !uiState.IsValid()) return;
            uiState.uiEvent.DispatchEvent(uiState.key, prefab, arg, UIEvent.EventType.OnRefresh);
        }
    }
}
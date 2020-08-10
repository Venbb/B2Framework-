using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class Action : IAction
    {
        private class RemoveData
        {
            public GameObject key;
            public ActionBase value;
        }

        public float currentDeltaTime { get { return _fCurrentTime; } }

        private ActionDelegate _actionDelegate = null;

        private Dictionary<GameObject, List<ActionBase>> _mapActions = new Dictionary<GameObject, List<ActionBase>>();

        private List<RemoveData> _listAdd = new List<RemoveData>();
        private List<RemoveData> _listRemove = new List<RemoveData>();

        private bool _isMainUpdating = false;

        private float _fCurrentTime = 0;

        public bool CheckInit()
        {
            //Editor非运行模式下已经在EditorMonoBehaviour.Update中执行过了Action.instance.MainUpdate
            //所以这里不用再依赖创建一个ActionDelegate
            if (Application.isPlaying)
            {
                GetGlobalInvokeTarget();
            }
            return true;
        }

        public void MainUpdate(float delayTime)
        {
            _isMainUpdating = true;
            this._fCurrentTime = delayTime;

            foreach (var listActions in this._mapActions)
            {
                foreach (var valueAction in listActions.Value)
                {
                    if (!valueAction.isPaused && !valueAction.isRemoved)
                    {
                        if (valueAction == null || valueAction.target == null)
                        {
                            Log.Warning("Target is null, update action error! ActionName=" + valueAction.actionName);
                            AddRemove(listActions.Key, valueAction);
                            continue;
                        }

                        if (!valueAction.IsActionAlive())
                        {
                            AddRemove(listActions.Key, valueAction);

                            //如果动画本身duration就是0那么在这里需要补充一次回调
                            if (valueAction.duration <= 0)
                                valueAction.NotifyActionCompleted();
                        }
                        else
                        {
                            float prePercent = valueAction.GetCurrentPercent();
                            valueAction.UpdateAction(prePercent);
                        }
                    }
                }
            }

            _isMainUpdating = false;

            CheckRemove();
            CheckAdd();
        }

        public bool HasAction(GameObject target, ActionBase findAction)
        {
            bool ret = false;
            this.ForeachActions(target, (ActionBase action) =>
                                   {
                                       if (target == action.target && action == findAction)
                                       {
                                           ret = true;
                                           return false;
                                       }
                                       else
                                           return true;
                                   });

            return ret;
        }

        public ActionBase FindAction(GameObject target, string name)
        {
            ActionBase ret = null;
            this.ForeachActions(target, (ActionBase action) =>
                                   {
                                       if (action.actionName == name)
                                       {
                                           ret = action;
                                           return false;
                                       }
                                       else
                                           return true;
                                   });

            return ret;
        }

        public bool HasAction(GameObject target)
        {
            bool ret = false;
            this.ForeachActions(target, (ActionBase action) =>
                                   {
                                       ret = true;
                                       return false;
                                   });

            return ret;
        }

        public bool HasActionWithTag(GameObject target, int tag)
        {
            bool ret = false;
            this.ForeachActions(target, (ActionBase action) =>
                                   {
                                       if (action.tag == tag)
                                       {
                                           ret = true;
                                           return false;
                                       }
                                       else
                                           return true;
                                   });

            return ret;
        }

        public void StopAllAction(bool isPlayEndWithDirectly = false)
        {
            _isMainUpdating = true;
            foreach (var listAction in this._mapActions)
            {
                foreach (var action in listAction.Value)
                {
                    if (isPlayEndWithDirectly)
                        action.PlayEndDirectly();
                    AddRemove(action.target, action);
                }
            }
            _isMainUpdating = false;
        }

        public void StopAllAction(GameObject target, bool isPlayEndWithDirectly = false)
        {
            this.ForeachActions(target, (ActionBase action) =>
                                   {
                                       if (isPlayEndWithDirectly)
                                           action.PlayEndDirectly();

                                       AddRemove(target, action);
                                       return true;
                                   });
        }

        public void StopAction(GameObject target, ActionBase action, bool isPlayEndWithDirectly = false)
        {
            this.ForeachActions(target, (ActionBase actionTmp) =>
                                   {
                                       if (actionTmp == action)
                                       {
                                           if (isPlayEndWithDirectly)
                                               actionTmp.PlayEndDirectly();
                                           AddRemove(target, actionTmp);
                                           return false;
                                       }
                                       else
                                       {
                                           return true;
                                       }
                                   });
        }

        public void StopActionByType<T>(GameObject target, bool isPlayEndWithDirectly = false) where T : ActionBase
        {
            var fullTypeName = shaco.Base.Utility.ToTypeString<T>();
            this.ForeachActions(target, (ActionBase action) =>
                                   {
                                       if (action.actionName == fullTypeName)
                                       {
                                           if (isPlayEndWithDirectly)
                                               action.PlayEndDirectly();
                                           AddRemove(target, action);
                                       }
                                       return true;
                                   });
        }

        public void StopActionByTag(GameObject target, int actionTag, bool isPlayEndWithDirectly = false)
        {
            this.ForeachActions(target, (ActionBase action) =>
                                   {
                                       if (action.tag == actionTag)
                                       {
                                           if (isPlayEndWithDirectly)
                                               action.PlayEndDirectly();
                                           AddRemove(target, action);
                                       }
                                       return true;
                                   });
        }

        public void PauseAllAction(GameObject target)
        {
            this.ForeachActions(target, (ActionBase action) =>
                                   {
                                       action.Pause();
                                       return true;
                                   });
        }

        public void PauseAllActionByTag(GameObject target, int actionTag)
        {
            this.ForeachActions(target, (ActionBase action) =>
                                   {
                                       if (action.tag == actionTag)
                                       {
                                           action.Pause();
                                       }
                                       return true;
                                   });
        }

        public void ResumeAllAction(GameObject target)
        {
            this.ForeachActions(target, (ActionBase action) =>
                                   {
                                       action.Resume();
                                       return true;
                                   });
        }

        public void ResumeAllActionByTag(GameObject target, int actionTag)
        {
            this.ForeachActions(target, (ActionBase action) =>
                                   {
                                       if (action.tag == actionTag)
                                       {
                                           action.Resume();
                                       }
                                       return true;
                                   });
        }

        public Dictionary<GameObject, List<ActionBase>> GetAllActions()
        {
            return this._mapActions;
        }

        public MonoBehaviour GetGlobalInvokeTarget()
        {
            if (!Application.isPlaying)
            {
                var retValue = GameObject.FindObjectOfType<MonoBehaviour>();

                if (null == retValue)
                {
                    Debug.LogWarning("Action GetGlobalInvokeTarget warning: no behaviour target in current scene");
                    retValue = new GameObject().AddComponent<ActionDelegate>();
                }
                return retValue;
            }
            else
            {
                if (null == _actionDelegate)
                {
                    var findObject = GameObject.FindObjectOfType<ActionDelegate>();

                    if (findObject == null)
                    {
                        GameObject objTmp = new GameObject();
                        findObject = objTmp.AddComponent<ActionDelegate>();
                    }

                    this._actionDelegate = findObject;
                    findObject.transform.name = "Action_Delegate";
                    UnityHelper.SafeDontDestroyOnLoad(findObject.gameObject);
                }
                return _actionDelegate;
            }
        }

        public void AddRemove(GameObject key, ActionBase value)
        {
            if (value.isRemoved || !value.isAdded)
                return;

            if (!this._mapActions.ContainsKey(key))
            {
                // Log.Error("Action AddRemove error: not found key=" + key + " action=" + value);
                value.MarkAsRemoved();
                return;
            }

            value.MarkAsRemoved();

            //如果没有在使用字典则直接移除数据
            if (!_isMainUpdating)
            {
                this._mapActions.Remove(key);
                return;
            }

            RemoveData rData = new RemoveData();
            rData.key = key;
            rData.value = value;

            this._listRemove.Add(rData);
        }

        public void AddAction(GameObject key, ActionBase value)
        {
            value.MarkAsAdded();
            if (!this._isMainUpdating)
                _AddAction(key, value);
            else
            {
                RemoveData rData = new RemoveData();
                rData.key = key;
                rData.value = value;
                this._listAdd.Add(rData);
            }
        }

        private void CheckRemove()
        {
            if (this._listRemove.Count == 0)
                return;

            foreach (var data in this._listRemove)
            {
                __CheckRemove(data.key, data.value);
            }
            this._listRemove.Clear();
        }

        private void __CheckRemove(GameObject key, ActionBase value)
        {
            bool isFind = false;
            List<ActionBase> listAction;

            if (this._mapActions.TryGetValue(key, out listAction))
            {
                value.Pause();

                int prevListCount = listAction.Count;
                listAction.Remove(value);
                if (prevListCount == listAction.Count)
                {
                    Log.Error("Action __CheckRemove error: not find remove data by key=" + key + " value=" + value);
                }

                if (listAction.Count == 0)
                {
                    this._mapActions.Remove(key);
                }
                isFind = true;
            }
            else
            {
                for (int i = this._listAdd.Count - 1; i >= 0; --i)
                {
                    if (this._listAdd[i].key == key && this._listAdd[i].value == value)
                    {
                        isFind = true;
                        this._listAdd.RemoveAt(i);
                        break;
                    }
                }
            }

            if (!isFind)
                Log.Error("Action __CheckRemove erorr: not find remove data by key=" + key + " value=" + value, key);
        }

        private void CheckAdd()
        {
            foreach (var data in this._listAdd)
            {
                _AddAction(data.key, data.value);
            }
            this._listAdd.Clear();
        }

        private void _AddAction(GameObject key, ActionBase value)
        {
            List<ActionBase> listActions;
            if (!this._mapActions.TryGetValue(key, out listActions))
            {
                listActions = new List<ActionBase>();
                listActions.Add(value);
                this._mapActions.Add(key, listActions);
            }
            else
            {
                listActions.Add(value);
            }
        }

        private void ForeachActions(GameObject target, System.Func<ActionBase, bool> doFunc)
        {
            List<ActionBase> listAction;
            if (this._mapActions.TryGetValue(target, out listAction))
            {
                for (int i = 0; i < listAction.Count; ++i)
                {
                    if (!doFunc(listAction[i]))
                        break;
                }
            }
            else
            {
                for (int i = 0; i < _listAdd.Count; ++i)
                {
                    if (_listAdd[i].key == target)
                    {
                        if (!doFunc(_listAdd[i].value))
                            break;
                    }
                }
            }
        }
    }
}
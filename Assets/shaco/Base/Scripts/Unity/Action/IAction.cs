using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public interface IAction : shaco.Base.IGameInstance
    {
        float currentDeltaTime { get; }

        bool CheckInit();

        void MainUpdate(float delayTime);

        bool HasAction(GameObject target, ActionBase findAction);

        ActionBase FindAction(GameObject target, string name);

        bool HasAction(GameObject target);

        bool HasActionWithTag(GameObject target, int tag);

        void StopAllAction(bool isPlayEndWithDirectly = false);

        void StopAllAction(GameObject target, bool isPlayEndWithDirectly = false);

        void StopAction(GameObject target, ActionBase action, bool isPlayEndWithDirectly = false);

        void StopActionByType<T>(GameObject target, bool isPlayEndWithDirectly = false) where T : ActionBase;

        void StopActionByTag(GameObject target, int actionTag, bool isPlayEndWithDirectly = false);

        void PauseAllAction(GameObject target);

        void PauseAllActionByTag(GameObject target, int actionTag);

        void ResumeAllAction(GameObject target);

        void ResumeAllActionByTag(GameObject target, int actionTag);

        Dictionary<GameObject, List<ActionBase>> GetAllActions();

        MonoBehaviour GetGlobalInvokeTarget();

        void AddRemove(GameObject key, ActionBase value);

        void AddAction(GameObject key, ActionBase value);
    }
}
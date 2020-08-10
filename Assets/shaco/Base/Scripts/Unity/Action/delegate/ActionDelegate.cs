using UnityEngine;
using System.Collections;

namespace shaco
{
    [DisallowMultipleComponent]
    public class ActionDelegate : MonoBehaviour
    {
        // [SerializeField]
        // private bool _isDrawDebug = false;
        private shaco.IAction _actionInstance = null;

        void Start()
        {
            _actionInstance = shaco.GameHelper.action;
        }

        void OnDestroy()
        {
            _actionInstance = null;
        }

        void Update()
        {
            if (null != _actionInstance)
                _actionInstance.MainUpdate(Time.deltaTime);
        }

        // void OnGUI()
        // {
        //     if (!_isDrawDebug)
        //         return;

        //     Accelerate.TestDrawAccelrateGUI();
        // }
    }
}

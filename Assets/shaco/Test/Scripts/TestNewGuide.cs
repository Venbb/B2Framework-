using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace shaco.Test
{
    public class TestNewGuide : MonoBehaviour
    {
        [SerializeField]
        private TextAsset _newGuideConfig = null;

        [SerializeField]
        private Image _imageGuideMask = null;

#if UNITY_EDITOR
        private GUILayoutOption[] _onGUIOptions = new GUILayoutOption[] { GUILayout.Width(160), GUILayout.Height(30) };
#else
        private GUILayoutOption[] _onGUIOptions = new GUILayoutOption[] { GUILayout.Width(160 * 4), GUILayout.Height(30 * 4) };
#endif

        private void Start()
        {
            AddGuideListener();
        }

        private void OnGUI()
        {
            if (TestMainMenu.DrawButton("Start"))
            {
                shaco.GameHelper.newguide.LoadFromString(_newGuideConfig.ToString());
                shaco.GameHelper.newguide.Start();
                Debug.Log("click load configuration");
            }

            if (TestMainMenu.DrawButton("Restart"))
            {
                shaco.GameHelper.newguide.ReloadFromString(_newGuideConfig.ToString());
                shaco.GameHelper.newguide.Start();
                Debug.Log("click restart");
            }

            if (TestMainMenu.DrawButton("Reset"))
            {
                shaco.GameHelper.newguide.ReloadFromString(_newGuideConfig.ToString());
                Debug.Log("click reset");
            }

            TestMainMenu.DrawBackToMainMenuButton();
        }

        private void AddGuideListener()
        {
            shaco.GameHelper.newguide.callbackBeforeShowGuide.AddCallBack(this, (object sender, shaco.Base.IGuideStep step) =>
            {
                _imageGuideMask.gameObject.SetActive(true);
                Debug.Log("begin step=" + step.guideStepID);
            });

            shaco.GameHelper.newguide.callbackAfterOnceStepEnd.AddCallBack(this, (object sender, shaco.Base.IGuideStep step) =>
            {
                Debug.Log("once step over=" + step.guideStepID);
            });

            shaco.GameHelper.newguide.callbackAfterCloseGuide.AddCallBack(this, (object sender, shaco.Base.IGuideStep step) =>
            {
                Debug.Log("end step=" + step.guideStepID);
            });

            shaco.GameHelper.newguide.callbackAllStepStop.AddCallBack(this, (object sender) =>
            {
                _imageGuideMask.gameObject.SetActive(false);
                Debug.Log("all step stop=");
            });
        }
    }
}
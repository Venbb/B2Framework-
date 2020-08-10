using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Test
{
    public class TestMainMenu : MonoBehaviour
    {
        static public bool DrawButton(string title)
        {
            return GUILayout.Button(title, GUILayout.MaxHeight(Screen.height));
        }

        static public void DrawBackToMainMenuButton()
        {
            if (DrawButton("BackToMainMenu"))
            {
                shaco.SceneManager.LoadScene("TestMainMenu");
            }
        }

        private void OnGUI()
        {
            if (DrawButton("Action"))
            {
                shaco.SceneManager.LoadScene("TestAction");
            }

            if (DrawButton("BehaviourTree"))
            {
                shaco.SceneManager.LoadScene("TestBehaviourTree");
            }

            if (DrawButton("Event"))
            {
                shaco.SceneManager.LoadScene("TestEvent");
            }

            if (DrawButton("LuaHotFix"))
            {
                shaco.SceneManager.LoadScene("TestLuaHotFix");
            }

            if (DrawButton("Math"))
            {
                shaco.SceneManager.LoadScene("TestMath");
            }

            if (DrawButton("NewGuide"))
            {
                shaco.SceneManager.LoadScene("TestNewGuide");
            }

            if (DrawButton("ObjectPool"))
            {
                shaco.SceneManager.LoadScene("TestObjectPool");
            }

            if (DrawButton("Observer"))
            {
                shaco.SceneManager.LoadScene("TestObserver");
            }

            if (DrawButton("ResourceUpdate"))
            {
                shaco.SceneManager.LoadScene("TestResourceUpdate");
            }

            if (DrawButton("Tools"))
            {
                shaco.SceneManager.LoadScene("TestTools");
            }

            if (DrawButton("UI"))
            {
                shaco.SceneManager.LoadScene("TestUI");
            }
        }
    }
}
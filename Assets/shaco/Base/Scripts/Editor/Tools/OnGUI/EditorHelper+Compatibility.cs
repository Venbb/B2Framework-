using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public partial class EditorHelper
    {
        static public void SetDirty(Object target)
        {
            if (target != null)
                EditorUtility.SetDirty(target);

#if UNITY_2018_1_OR_NEWER
            GameObject gameObjectTarget = null;
            if (target is Component)
                gameObjectTarget = ((Component)target).gameObject;
            else if (target is GameObject)
                gameObjectTarget = target as GameObject;
            if (null != gameObjectTarget)
            {
                var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObjectTarget);
                if (null != prefabStage)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                }
                else
                {
                    if (!Application.isPlaying)
                        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                }
            }
#elif UNITY_5_3_OR_NEWER
            if (!Application.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#else
            Debug.LogError("EditorSceneManager+Compatibility SetDirty error: no support method");
#endif
        }

        static public string GetEditorPrefsKey(string key)
        {
#if UNITY_5_3_OR_NEWER
            return Application.companyName + "_" + Application.productName + "_" + Application.unityVersion + "_shaco_" + key;
#else
            return PlayerSettings.companyName + "_" + PlayerSettings.productName + "_" + Application.unityVersion + "_shaco_" + key;
#endif
        }

        static public long GetRuntimeMemorySizeLong(Object o)
        {
#if UNITY_5_6_OR_NEWER
            var sampleSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(o);
#else
            var sampleSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(o);
#endif
            return (long)sampleSize;
        }

        static public T FindWindow<T>(T window = null) where T : UnityEditor.EditorWindow
        {
            T retValue = window;
            if (null == window)
            {
                EditorWindow.FocusWindowIfItsOpen(typeof(T));
                var findWindow = EditorWindow.focusedWindow as T;
                if (null != findWindow)
                {
                    retValue = findWindow;
                }
            }
            return retValue;
        }

        static public T GetWindow<T>(T window, bool utility, string title = shaco.Base.GlobalParams.EmptyString, params System.Type[] desiredDockNextTo) where T : UnityEditor.EditorWindow
        {
            T ret = window;

            //当title为空使用类型名字作为默认title
            if (string.IsNullOrEmpty(title))
            {
                title = typeof(T).Name;
            }

#if UNITY_5_3_OR_NEWER
            if (null != window && window.titleContent.text == typeof(T).FullName)
#else
            if (null != window && window.title == typeof(T).FullName)
#endif
            {
                return ret;
            }

            if (ret == null)
            {
                if (!desiredDockNextTo.IsNullOrEmpty() && !utility)
                    ret = EditorWindow.GetWindow<T>(title, true, desiredDockNextTo) as T;
                else
                    ret = EditorWindow.GetWindow(typeof(T), utility, title) as T;

                ret.Show();
            }
            EditorWindow.FocusWindowIfItsOpen(typeof(T));
            return ret;
        }

        static public bool IsPressedControlButton()
        {
            if (null == Event.current) return false;
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            return Event.current.command;
#else
            return Event.current.control;
#endif
        }

        static public bool Foldout(bool foldout, string content)
        {
#if UNITY_5_3_OR_NEWER
            return EditorGUILayout.Foldout(foldout, content, true);
#else
            return EditorGUILayout.Foldout(foldout, content);
#endif
        }

        static public bool Foldout(bool foldout, GUIContent content)
        {
#if UNITY_5_3_OR_NEWER
            return EditorGUILayout.Foldout(foldout, content, true);
#else
            return EditorGUILayout.Foldout(foldout, content);
#endif
        }

        static public void SaveCurrentScene()
        {
            if (string.IsNullOrEmpty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name))
                return;
#if UNITY_5_3_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
#else
            EditorApplication.SaveScene();
#endif
        }

        static public void OpenScene(string sceneName)
        {
#if UNITY_5_3_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(sceneName);
#else
            EditorApplication.OpenScene(sceneName);
#endif
        }

        static public Texture2D GetPrefabIcon()
        {
#if UNITY_2018_1_OR_NEWER
            return EditorGUIUtility.FindTexture("Prefab Icon");
#else
            return EditorGUIUtility.FindTexture("PrefabNormal Icon");
#endif
        }
    }
}
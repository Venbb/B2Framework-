#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace shacoEditor
{
    [InitializeOnLoad]
    public class EditorMonoBehaviour
    {
        public enum PlayModeState
        {
            Playing,
            Paused,
            Stop,
            PlayingOrWillChangePlaymode
        }

        private System.DateTime _nowTime = System.DateTime.Now;
        private shaco.IAction _actionInstance = null;

        static EditorMonoBehaviour()
        {
            new EditorMonoBehaviour().OnEditorMonoBehaviour();
        }

        private void OnEditorMonoBehaviour()
        {
            EditorApplication.update += Update;
            //EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
            //EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
            //EditorApplication.projectWindowChanged += OnProjectWindowChanged;
            //EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
            //EditorApplication.modifierKeysChanged += OnModifierKeysChanged;

            // globalEventHandler
            //EditorApplication.CallbackFunction function = () => OnGlobalEventHandler(Event.current);
            //FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            //EditorApplication.CallbackFunction functions = (EditorApplication.CallbackFunction)info.GetValue(null);
            //functions += function;
            //info.SetValue(null, (object)functions);

            //EditorApplication.searchChanged += OnSearchChanged;

            EditorPlayModeListener.playModeChanged += (oldState, newState) => 
            {
                if (oldState == newState)
                    return;

                if (newState == EditorPlayModeListener.PlayModeState.Playing)
                {
                    OnEnteredPlayMode();
                }
                else if (newState == EditorPlayModeListener.PlayModeState.Stopped)
                {
                    OnEnteredEditMode();
                }
            };
        }

        public virtual void Update()
        {
            if (!Application.isPlaying)
            {
                // shaco.Log.Info("每一帧回调一次");
                // shaco.GameInitComponent.CheckConstantsData();

                var curTotalSeconds = (float)(DateTime.Now - _nowTime).TotalSeconds;
                _nowTime = DateTime.Now;

                if (_actionInstance == null)
                    _actionInstance = shaco.GameHelper.action;
                _actionInstance.MainUpdate(curTotalSeconds);
                shaco.GameInitComponent.MainUpdate(curTotalSeconds);
            }
        }

        public virtual void OnHierarchyWindowChanged()
        {
            //Log.Info("层次视图发生变化");
        }

        public virtual void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            //	SceneManager.Log (string.Format ("{0} : {1} - {2}", EditorUtility.InstanceIDToObject (instanceID), instanceID, selectionRect));
        }

        public virtual void OnProjectWindowChanged()
        {
            //	SceneManager.Log ("当资源视图发生变化");

        }

        public virtual void ProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            //根据GUID得到资源的准确路径
            //SceneManager.Log (string.Format ("{0} : {1} - {2}", AssetDatabase.GUIDToAssetPath (guid), guid, selectionRect));
        }

        public virtual void OnModifierKeysChanged()
        {
            //	SceneManager.Log ("当触发键盘事件");
        }

        public virtual void OnGlobalEventHandler(Event e)
        {
            //SceneManager.Log ("全局事件回调: " + e);
        }

        public virtual void OnSearchChanged()
        {
        }

        public virtual void OnEnteredPlayMode()
        {
            //强制解锁一次，避免死锁
            EditorApplication.UnlockReloadAssemblies();

            //如果正在编译时候运行了unity，则不再锁定编译，因为unity会在编译完毕后才真的开始运行
            if (EditorApplication.isCompiling)
            {
                return;
            }

            //禁止在运行时刻编译，可能导致各种未知bug和报错
            //实际上在运行时刻编译也没啥意义
            EditorApplication.LockReloadAssemblies();
        }

        public virtual void OnEnteredEditMode()
        {
            //恢复编译
            EditorApplication.UnlockReloadAssemblies();

            shaco.GameHelper.resCache.UnloadUnusedDatas(true);
            shaco.GameHelper.action.StopAllAction();
            shaco.Base.BehaviourRootTree.StopAll();
            shaco.GameHelper.observer.Clear();
            UnityEditor.EditorUtility.ClearProgressBar();
            shaco.GameEntry.ClearIntances();
        }

        [UnityEditor.Callbacks.OnOpenAssetAttribute()]
        private static bool OnOpenAsset(int instanceID, int line)
        {
#if UNITY_5_3_OR_NEWER
            UnityEngine.Object selected = EditorUtility.InstanceIDToObject(instanceID);
            if (null != selected && null != selected as SceneAsset)
#else
            var pathTmp = AssetDatabase.GetAssetPath(instanceID);
            if (shaco.Base.FileHelper.GetFilNameExtension(pathTmp) == "unity")
#endif
            {
                shaco.GameHelper.action.StopAllAction();
                shaco.Base.Utility.ExecuteAttributeStaticFunction<shacoEditor.OpenSceneAssetAttribute>(selected);
            }
            return false;
        }
    }
}
#endif
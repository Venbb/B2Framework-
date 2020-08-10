using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{

    [CustomEditor(typeof(shaco.UnityObjectAutoReleaseComponent))]
    public class UnityObjectAutoReleaseInspector : Editor
    {
        private shaco.UnityObjectAutoReleaseComponent _target = null;
        private string _searchName = string.Empty;
        private string _searchNameLower = string.Empty;
        private Dictionary<string, Object> _assetbundlePathToAsset = new Dictionary<string, Object>();
        private System.Text.StringBuilder _formatBuilder = new System.Text.StringBuilder(); 

        private readonly int MAX_SHOW_CALLBACKS_COUNT = 10;

        private void OnEnable()
        {
            _target = target as shaco.UnityObjectAutoReleaseComponent;
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, target.GetType().FullName);
            base.OnInspectorGUI();

            DrawSearchFiled();
            foreach (var iter in _target.willAutoReleaseInfos)
            {
                if (IsIgnoreSearch(iter))
                {
                    continue;
                }
                GUILayout.BeginVertical("box");
                {
                    DrawTask(iter);
                }
                GUILayout.EndVertical();
            }
        }

        private void DrawSearchFiled()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(Screen.width / 2 * 1);
                GUI.changed = false;
                _searchName = GUILayoutHelper.SearchField(_searchName);
                if (GUI.changed)
                    _searchNameLower = _searchName.ToLower();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawTask(shaco.UnityObjectAutoReleaseComponent.AutoReleaseInfo releaseInfo)
        {
            if (releaseInfo.target is UnityEngine.Object)
                EditorGUILayout.ObjectField("Key", (Object)releaseInfo.target, releaseInfo.target.GetType(), true);
            else
                EditorGUILayout.TextField("Key", releaseInfo.target.ToString());

            GUILayout.Label("CallBack Count: " + releaseInfo.callbacks.Count);
            var maxLoopCount = System.Math.Min(MAX_SHOW_CALLBACKS_COUNT, releaseInfo.callbacks.Count);
            for (int i = 0; i < maxLoopCount; ++i)
            {
                var callbackInfo = releaseInfo.callbacks[i];

                GUILayout.BeginHorizontal();
                {
                    EditorGUI.BeginDisabledGroup(!callbackInfo.stackLocationBind.HasStack());
                    {
                        _formatBuilder.Length = 0;
                        if (GUILayout.Button(_formatBuilder.AppendFormat("Location:{0}", i).ToString(), GUILayout.Width(120)))
                        {
                            shaco.Log.Info(callbackInfo.stackLocationBind.GetTotalStackInformation());
                            EditorHelper.OpenAsset(callbackInfo.stackLocationBind.GetStackInformation(), callbackInfo.stackLocationBind.GetStackLine());
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                    GUILayout.Label(callbackInfo.callback.Target.ToTypeString());
                    GUILayout.Label(callbackInfo.callback.Method.Name);
                }
                GUILayout.EndHorizontal();
            }
            
            if (maxLoopCount == MAX_SHOW_CALLBACKS_COUNT)
            {
                GUILayout.Label("...");
            }
        }

        private bool IsIgnoreSearch(shaco.UnityObjectAutoReleaseComponent.AutoReleaseInfo releaseInfo)
        {
            if (!string.IsNullOrEmpty(_searchNameLower))
            {
                if (!releaseInfo.target.ToString().ToLower().Contains(_searchNameLower))
                    return true;
            }
            return false;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.AudioInfoComponent))]
    public class AudioInfoComponentInspector : Editor
    {
        public override void OnInspectorGUI()
		{
            Undo.RecordObject(target, target.GetType().FullName);
            
            var targetTmp = target as shaco.AudioInfoComponent;
			base.OnInspectorGUI();

            if (null == targetTmp.audioTarget)
                return;

            EditorGUILayout.TextField("File Name", targetTmp.audioTarget.fileName);
            EditorGUI.BeginDisabledGroup(true);
            {
                EditorGUILayout.Toggle("IsPlaying", targetTmp.isPlaying);
                EditorGUILayout.Toggle("IsClipPlaying", targetTmp.audioSource.isPlaying);
                EditorGUILayout.Slider("Volume", targetTmp.audioSource.volume, 0, 1.0f);
            }
            EditorGUI.EndDisabledGroup();

            bool hasLocationStackPlay = targetTmp.stackLocationPlay.HasStack();
            bool hasLocationStackPause = targetTmp.stackLocationPause.HasStack();
            bool hasLocationStackResume = targetTmp.stackLocationResume.HasStack();

            if (!hasLocationStackPlay && !hasLocationStackPause && !hasLocationStackResume)
                return;

            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.PrefixLabel("Location");

                if (hasLocationStackPlay && GUILayout.Button("Play"))
                {
                    shaco.Log.Info(targetTmp.stackLocationPlay.GetTotalStackInformation());
                    EditorHelper.OpenAsset(targetTmp.stackLocationPlay.GetStackInformation(), targetTmp.stackLocationPlay.GetStackLine());
                }

                if (hasLocationStackPause && GUILayout.Button("Pause"))
                {
                    shaco.Log.Info(targetTmp.stackLocationPause.GetTotalStackInformation());
                    EditorHelper.OpenAsset(targetTmp.stackLocationPause.GetStackInformation(), targetTmp.stackLocationPause.GetStackLine());
                }

                if (hasLocationStackResume && GUILayout.Button("Resume"))
                {
                    shaco.Log.Info(targetTmp.stackLocationResume.GetTotalStackInformation());
                    EditorHelper.OpenAsset(targetTmp.stackLocationResume.GetStackInformation(), targetTmp.stackLocationResume.GetStackLine());
                }
            }
            GUILayout.EndVertical();
		}
    }
}
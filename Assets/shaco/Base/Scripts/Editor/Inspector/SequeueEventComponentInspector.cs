using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.SequeueEventComponent))]
    public class SequeueEventComponentInspector : Editor
    {
        public override void OnInspectorGUI()
		{
            Undo.RecordObject(target, target.GetType().FullName);
			base.OnInspectorGUI();

			var targetTmp = target as shaco.SequeueEventComponent;

			if (GUILayout.Button("Location"))
			{
                shaco.Log.Info(targetTmp.stackLocation.GetTotalStackInformation());
                EditorHelper.OpenAsset(targetTmp.stackLocation.GetStackInformation(), targetTmp.stackLocation.GetStackLine());
            }
		}
    }
}
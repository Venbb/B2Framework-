using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using B2Framework.UI;

[CustomEditor(typeof(UIText))]
public class UITextEditor : UnityEditor.UI.TextEditor
{
    public override void OnInspectorGUI()
    {
        UIText component = (UIText)target;
        base.OnInspectorGUI();
        component.lc_key = EditorGUILayout.TextField("Loc Key", component.lc_key);   
    }
}

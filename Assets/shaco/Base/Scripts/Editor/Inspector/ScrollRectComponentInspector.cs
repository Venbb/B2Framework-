using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.ScrollRectComponent))]
    public class ScrollRectComponentInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, target.GetType().FullName);
            base.OnInspectorGUI();
        }
    }
}
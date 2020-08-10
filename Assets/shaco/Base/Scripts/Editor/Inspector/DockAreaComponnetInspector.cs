using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.DockAreaComponent))]
    public class DockAreaComponnetInspector : Editor
    {
        private shaco.DockAreaComponent _target = null;

        void OnEnable()
        {
            _target = target as shaco.DockAreaComponent;
        }

        override public void OnInspectorGUI()
        {
            Undo.RecordObject(target, target.GetType().FullName);
            base.OnInspectorGUI();

            bool isAnchorChanged = false;

            //margin
            _target.margin = EditorGUILayout.Vector3Field("Margin", _target.margin);
            isAnchorChanged |= GUI.changed;

            //dock prev target, to do set target position
            GUILayout.BeginVertical("box");
            {
                _target.dockTarget = (RectTransform)EditorGUILayout.ObjectField("Target", _target.dockTarget, typeof(RectTransform), true);

                if (null != _target.dockTarget)
                {
                    _target.dockAnchor = GUILayoutHelper.DrawAnchor("Anchor", _target.dockAnchor);
                    isAnchorChanged |= GUI.changed;
                }
            }
            GUILayout.EndVertical();

            if (isAnchorChanged)
            {
                Undo.RegisterCompleteObjectUndo(target, target.name);
                EditorHelper.SetDirty(target);
                _target.SetUpdateLayoutDirty();
                GUI.changed = false;
            }
        }
    }
}


using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.ActionDelegate))]
    public class ActionDelegateInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, target.GetType().FullName);
			
            this.Repaint();
            base.OnInspectorGUI();

			var allActions = shaco.GameHelper.action.GetAllActions();
			foreach (var iter in allActions)
			{
				GUILayout.BeginVertical("box");
				{
					if (DrawHeader(iter.Key))
                    {
						for (int i = 0; i < iter.Value.Count; ++i)
						{
							var action = iter.Value[i];

							if (!action.isRemoved)
							{
								GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(24);
                                    GUILayout.Label(action.actionName);
                                }
                                GUILayout.EndHorizontal();
							}
                        }
                    }
				}
				GUILayout.EndVertical();
			}
        }
		
		private bool DrawHeader(GameObject target)
		{
			bool isOpened = shaco.GameHelper.datasave.ReadBool(target.name, true);

			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button(isOpened ? "-" : "+", GUILayout.Width(20)))
				{
					isOpened = !isOpened;
					shaco.GameHelper.datasave.WriteBool(target.name, isOpened);
				}
				
				EditorGUI.BeginDisabledGroup(true);
				{
                    EditorGUILayout.ObjectField(target, typeof(GameObject), true);
                }
				EditorGUI.EndDisabledGroup();
			}
			GUILayout.EndHorizontal();

			return isOpened;
		}
    }
}
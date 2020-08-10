using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.LocalizationComponent))]
    public class LocalizationComponentInspector : Editor
    {
        private UnityEditorInternal.ReorderableList _formatParamsList = null;

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, target.GetType().FullName);
            base.OnInspectorGUI();
            shaco.LocalizationComponent targetTmp = (shaco.LocalizationComponent)target;

            //是否异步加载资源
            if (targetTmp.type == shaco.LocalizationComponent.TargetType.Image || targetTmp.type == shaco.LocalizationComponent.TargetType.Prefab)
            {
                targetTmp.isAsyncLoad = EditorGUILayout.Toggle("AnsyLoad", targetTmp.isAsyncLoad);
            }

            //本地化类型
            var oldColor = GUI.color;
            if (targetTmp.type == shaco.LocalizationComponent.TargetType.None)
                GUI.color = Color.red;

            GUI.changed = false;
            var newType = (shaco.LocalizationComponent.TargetType)EditorGUILayout.EnumPopup("Type", targetTmp.type);
            if (GUI.changed)
            {
                targetTmp.ChangeTargetType(newType);
            }

            if (targetTmp.type == shaco.LocalizationComponent.TargetType.None)
                GUI.color = oldColor;

            //本地化key
            if (targetTmp.type != shaco.LocalizationComponent.TargetType.None)
            {
                targetTmp.languageKey = EditorGUILayout.TextField("LanguageKey", targetTmp.languageKey);

                //自动格式化参数列表
                _formatParamsList = GUILayoutHelper.DrawReorderableList(_formatParamsList, serializedObject, shaco.Base.Utility.ToVariableName(() => targetTmp.formatParams));
            }

            if (GUI.changed)
            {
                EditorHelper.SetDirty(targetTmp);
            }
        }
    }
}
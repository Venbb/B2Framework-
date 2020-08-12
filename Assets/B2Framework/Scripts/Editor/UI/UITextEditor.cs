using UnityEditor;
using B2Framework.Unity;

namespace B2Framework.Editor
{
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
}
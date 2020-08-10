using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using UnityEditorInternal;
using System;

namespace shacoEditor
{
    [CustomPropertyDrawer(typeof(shaco.SortingLayerAttribute))]
    public class SortingLayerAttributeDrawer : PropertyDrawer
    {
        private struct DrawerValuePair
        {
            public string str;
            public SerializedProperty property;

            public DrawerValuePair(string val, SerializedProperty property)
            {
                this.str = val;
                this.property = property;
            }
        }

        private const string NONE = "<None>";
        private const float ITEM_HEIGHT = 16;

        private string[] _layersName = null;
        private bool _isCheckedLayerNameValid = false;
        private bool _isLayerNameValid = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, "ERROR:", "May only apply to type string");
                return;
            }

            string value = property.stringValue;
            CheckLayerNameValid(value);

            if (!_isLayerNameValid)
            {
                EditorGUI.HelpBox(new Rect(position.x, position.y, position.width, ITEM_HEIGHT * 2), string.Format("Not found sorting layer name: {0}", value), MessageType.Error);
                position.y += ITEM_HEIGHT * 2;
            }

            var oldColor = GUI.color;
            if (!_isLayerNameValid)
                GUI.color = Color.red;
            position = EditorGUI.PrefixLabel(new Rect(position.x, position.y, position.width, ITEM_HEIGHT), label);
            if (string.IsNullOrEmpty(value))
                value = NONE;
            if (GUI.Button(new Rect(position.x, position.y, position.width, ITEM_HEIGHT), value, EditorStyles.popup))
            {
                Selector(property);
            }
            GUI.color = oldColor;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            return _isLayerNameValid ? ITEM_HEIGHT * 1 : ITEM_HEIGHT * 3;
        }

        void Selector(SerializedProperty property)
        {
            _layersName = GetSortingLayerNames();

            GenericMenu menu = new GenericMenu();

            bool isNone = string.IsNullOrEmpty(property.stringValue);
            menu.AddItem(new GUIContent(NONE), isNone, HandleSelect, new DrawerValuePair(NONE, property));

            for (int i = 0; i < _layersName.Length; ++i)
            {
                string name = _layersName[i];
                menu.AddItem(new GUIContent(name), name == property.stringValue, HandleSelect, new DrawerValuePair(name, property));
            }

            menu.AddItem(new GUIContent(string.Empty), false, null);
            menu.AddItem(new GUIContent("Editor Layers"), false, () =>
            {
                var bindFlags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
                var findProperty = typeof(EditorApplication).GetProperty("tagManager", bindFlags);
                var tagManager = findProperty.GetValue(new EditorApplication());
                Selection.activeObject = (UnityEngine.Object)tagManager;
            });
            menu.ShowAsContext();
        }

        void HandleSelect(object val)
        {
            var pair = (DrawerValuePair)val;
            if (pair.str.Equals(NONE))
            {
                pair.property.stringValue = string.Empty;
            }
            else
            {
                pair.property.stringValue = pair.str;
                _isCheckedLayerNameValid = false;
                CheckLayerNameValid(pair.property.stringValue);
            }
            pair.property.serializedObject.ApplyModifiedProperties();
        }

        // Get the sorting layer names
        public string[] GetSortingLayerNames()
        {
            Type internalEditorUtilityType = typeof(InternalEditorUtility);
            PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            return (string[])sortingLayersProperty.GetValue(null, new object[0]);
        }

        private void CheckLayerNameValid(string layerName)
        {
            if (_isCheckedLayerNameValid)
                return;

            if (null == _layersName || 0 == _layersName.Length)
                _layersName = GetSortingLayerNames();

            if (layerName == NONE)
            {
                _isLayerNameValid = true;   
            }
            else 
            {
                _isLayerNameValid = !string.IsNullOrEmpty(_layersName.Find(v => v == layerName));
            }
            _isCheckedLayerNameValid = true;
        }
    }
}
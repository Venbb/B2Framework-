using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(LayoutElementWithMaxValues), true)]
[CanEditMultipleObjects]
public class LayoutElementWithMaxValuesInspector : LayoutElementEditor
{
    LayoutElementWithMaxValues layoutMax;

    SerializedProperty maxHeightProperty;
    SerializedProperty maxWidthProperty;

    SerializedProperty useMaxHeightProperty;
    SerializedProperty useMaxWidthProperty;

    RectTransform myRectTransform;

    protected override void OnEnable()
    {
        base.OnEnable();

        layoutMax = target as LayoutElementWithMaxValues;
        myRectTransform = layoutMax.transform as RectTransform;

        maxHeightProperty = serializedObject.FindProperty(nameof(layoutMax.maxHeight));
        maxWidthProperty = serializedObject.FindProperty(nameof(layoutMax.maxWidth));

        useMaxHeightProperty = serializedObject.FindProperty(nameof(layoutMax.useMaxHeight));
        useMaxWidthProperty = serializedObject.FindProperty(nameof(layoutMax.useMaxWidth));
    }

    public override void OnInspectorGUI()
    {
        Draw(maxWidthProperty, useMaxWidthProperty);
        Draw(maxHeightProperty, useMaxHeightProperty);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        base.OnInspectorGUI();
    }

    void Draw(SerializedProperty property, SerializedProperty useProperty)
    {
        Rect position = EditorGUILayout.GetControlRect();

        GUIContent label = EditorGUI.BeginProperty(position, null, property);

        Rect fieldPosition = EditorGUI.PrefixLabel(position, label);

        Rect toggleRect = fieldPosition;
        toggleRect.width = 16;

        Rect floatFieldRect = fieldPosition;
        floatFieldRect.xMin += 16;


        var use = EditorGUI.Toggle(toggleRect, useProperty.boolValue);
        useProperty.boolValue = use;

        if (use)
        {
            EditorGUIUtility.labelWidth = 4;
            property.floatValue = EditorGUI.FloatField(floatFieldRect, new GUIContent(" "), property.floatValue);
            EditorGUIUtility.labelWidth = 0;
        }

        EditorGUI.EndProperty();
    }
}
using UnityEngine;
using UnityEditor;
using B2Framework.Unity;

namespace B2Framework.Editor
{
    /// <summary>
    /// Lua 脚本绑定
    /// https://blog.csdn.net/l100142548/article/details/105857923/
    /// </summary>
    [CustomPropertyDrawer(typeof(LuaScriptBinder))]
    public class LuaScriptBinderDrawer : PropertyDrawer
    {
        private const float HORIZONTAL_GAP = 5;
        const string luaRootPath1 = "";
        const string luaRootPath2 = "";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var textProperty = property.FindPropertyRelative("text");
            var filenameProperty = property.FindPropertyRelative("filename");
            var typeProperty = property.FindPropertyRelative("type");

            float y = position.y;
            float x = position.x;
            float height = GetPropertyHeight(property, label);
            float width = position.width - HORIZONTAL_GAP * 2;

            Rect nameRect = new Rect(x, y, Mathf.Min(80, width * 0.3f), height);
            Rect typeRect = new Rect(nameRect.xMax + HORIZONTAL_GAP, y, Mathf.Min(100, width * 0.2f), height);
            Rect valueRect = new Rect(typeRect.xMax + HORIZONTAL_GAP, y, 0, height);

            EditorGUI.LabelField(nameRect, property.displayName);

            LuaScriptBindEnum typeValue = (LuaScriptBindEnum)typeProperty.enumValueIndex;
            EditorGUI.BeginChangeCheck();
            typeValue = (LuaScriptBindEnum)EditorGUI.EnumPopup(typeRect, typeValue);
            if (EditorGUI.EndChangeCheck())
            {
                typeProperty.enumValueIndex = (int)typeValue;
            }

            switch (typeValue)
            {
                case LuaScriptBindEnum.TextAsset:
                    valueRect.width = position.xMax - typeRect.xMax - HORIZONTAL_GAP;
                    textProperty.objectReferenceValue = EditorGUI.ObjectField(valueRect, GUIContent.none, textProperty.objectReferenceValue, typeof(UnityEngine.TextAsset), false);
                    break;
                case LuaScriptBindEnum.Filename:
                    valueRect.width = position.xMax - typeRect.xMax - HORIZONTAL_GAP - height;
                    if (EditorUtility.IsFieldClick(valueRect, Event.current))
                    {
                        var path = string.IsNullOrEmpty(filenameProperty.stringValue) ? "" : FindFilePath(filenameProperty.stringValue.Replace('.', '/'));
                        if (!string.IsNullOrEmpty(path))
                        {
                            Log.Debug(path);
                            EditorUtility.PingObject(path);
                        }
                    }
                    // var obj = GetDragObject<Object>(valueRect, Event.current);
                    if (GUI.Button(new Rect(valueRect.xMax, y, height, height), GUIContent.none, "IN ObjectField"))
                    {
                        EditorGUIUtility.ShowObjectPicker<Object>(null, false, property.serializedObject.targetObject.name, 1001);
                    }
                    if (EditorGUIUtility.GetObjectPickerControlID() == 1001)
                    {
                        var obj = EditorGUIUtility.GetObjectPickerObject();
                        if (obj != null)
                        {
                            var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                            if (Utility.Path.IsExtension(path, AppConst.LUA_EXTENSIONS.Split(',')))
                            {
                                var start = AppConst.LUA_SCRIPTS_PATH;
                                int idx = path.IndexOf(start);
                                if (idx == -1)
                                {
                                    start = "Resources/";
                                    idx = path.IndexOf(start);
                                }
                                if (idx != -1)
                                {
                                    idx += start.Length;
                                    path = path.Substring(idx, path.IndexOf(".lua") - idx).Replace('/', '.');
                                    // Debug.Log(path);
                                    filenameProperty.stringValue = path;
                                }
                            }
                        }
                    }
                    filenameProperty.stringValue = EditorGUI.TextField(valueRect, GUIContent.none, filenameProperty.stringValue);
                    break;
            }
            EditorGUI.EndProperty();
        }
        string FindFilePath(string filter)
        {
            var files = Utility.Files.GetFiles(Application.dataPath, AppConst.LUA_SCRIPTS_PATH);
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Contains(filter)) return files[i];
            }
            return null;
        }
        /// <summary>
        /// 拖拽功能
        /// </summary>
        private T GetDragObject<T>(Rect rect, Event @event) where T : UnityEngine.Object
        {
            T @object = default;
            if (rect.Contains(@event.mousePosition))
            {
                if (DragAndDrop.objectReferences.Length > 0)
                {
                    if (DragAndDrop.objectReferences[0].GetType() == typeof(T))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        if (@event.type == EventType.DragExited)
                        {
                            DragAndDrop.AcceptDrag();
                            // GUI.changed = true;
                            @object = (T)DragAndDrop.objectReferences[0];
                            Event.current.Use();
                        }
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.None;
                    }
                }
            }
            return @object;
        }
    }
}
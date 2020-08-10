using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.ResourcePackSetting))]
    public class ResourcePackSettingInspector : Editor
    {
        private bool _shoulSaveAsset = false;
        private bool _isValueChanged = false;
        private shaco.ResourcePackSetting _target = null;

        [SerializeField]
        private List<Object> _objectsInPack = new List<Object>();

        void OnEnable()
        {
            _target = target as shaco.ResourcePackSetting;
            RefreshObjectsInPack();
        }

        void RefreshObjectsInPack()
        {
            _objectsInPack.Clear();
            foreach (var iter in _target.assetsGUID)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(iter);
                _objectsInPack.Add(AssetDatabase.LoadAssetAtPath<Object>(assetPath));
            }
        }

        private void OnDestroy()
        {
            if (_shoulSaveAsset)
            {
                //数据发生修改后，在关闭该窗口时应该主动保存一次资源
                //否则unity本身不会保存文件，修改的asset数据还在内存中
                AssetDatabase.SaveAssets();
            }
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(this, this.GetType().FullName);
            Undo.RecordObject(_target, _target.GetType().FullName);

            var newExportFormat = (shaco.HotUpdateDefine.ExportFileFormat)EditorGUILayout.EnumPopup("Export Foramt", _target.exportFormat);
            if (newExportFormat != _target.exportFormat)
            {
                _target.exportFormat = newExportFormat;
                _isValueChanged = true;
            }

            GUILayoutHelper.DrawListBase(_objectsInPack, "Objects for Packing", (oldValue, newValue) =>
            {
                //添加
                if (null == oldValue && null != newValue)
                {
                    //过滤自己
                    if (_target.GetType() == newValue.GetType())
                        return false;

                    var assetPath = AssetDatabase.GetAssetPath(newValue);
                    var guid = AssetDatabase.AssetPathToGUID(assetPath);

                    //检查是否重复
                    if (null != _target.assetsGUID.Find(v => v == guid))
                    {
                        Debug.LogError("ResourcePackSettingInspector add item error: duplicate path=" + assetPath + "\nguid=" + guid + "\nobj=" + newValue);
                        return false;
                    }
                    _target.assetsGUID.Add(guid);
                    _isValueChanged = true;
                    return true;
                }
                //删除
                else if (null != oldValue && null == newValue)
                {
                    var assetPath = AssetDatabase.GetAssetPath(oldValue);
                    var guid = AssetDatabase.AssetPathToGUID(assetPath);

                    if (!_target.assetsGUID.RemoveOne(v => v == guid))
                    {
                        Debug.LogError("ResourcePackSettingInspector remove item error: not found path=" + assetPath + "\nguid=" + guid + "\nobj=" + oldValue);
                        return false;
                    }
                    _isValueChanged = true;
                    return true;
                }
                //清空
                else if (null == oldValue && null == newValue)
                {
                    _target.assetsGUID.Clear();
                    _isValueChanged = true;
                    return true;
                }
                //修改
                else if (null != oldValue && null != newValue)
                {
                    var assetPath = AssetDatabase.GetAssetPath(oldValue);
                    var guid = AssetDatabase.AssetPathToGUID(assetPath);

                    var findIndex = _target.assetsGUID.IndexOf(guid);
                    if (findIndex < 0)
                    {
                        Debug.LogError("ResourcePackSettingInspector chnage item error: not found path=" + assetPath + "\nguid=" + guid + "\nobj=" + oldValue);
                        return false;
                    }

                    var newAssetPath = AssetDatabase.GetAssetPath(newValue);
                    var newGUID = AssetDatabase.AssetPathToGUID(newAssetPath);
                    if (null != _target.assetsGUID.Find(v => v == newGUID))
                    {
                        Debug.LogError("ResourcePackSettingInspector chnage item error: duplicate path=" + newAssetPath + "\nguid=" + newGUID + "\nobj=" + newValue);
                        return false;
                    }

                    _target.assetsGUID[findIndex] = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newValue));
                    _isValueChanged = true;
                    return true;
                }

                Debug.LogError("ResourcePackSettingInspector remove item error: invalid operation, old=" + oldValue + " new=" + newValue);
                return false;
            }, (callback) =>
            {
                callback(AssetDatabase.LoadAssetAtPath<Object>("Assets"));
            }, null, null, null, (oldIndex, newIndex) =>
            {
                //交换
                _target.assetsGUID.SwapValue(oldIndex, newIndex);
                _isValueChanged = true;
                return true;
            });


            if (_isValueChanged)
            {
                _isValueChanged = false;
                _shoulSaveAsset = true;

                EditorHelper.SetDirty(_target);
            }
        }
    }
}
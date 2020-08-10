using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.NumberLoopScrollActionComponent))]
    public class NumberLoopScrollActionComponentInspector : Editor
    {
        private shaco.NumberLoopScrollActionComponent _target = null;
        private GridLayoutGroup _gridLayout = null;
        private RectTransform _rectTransform = null;
        private Vector2 _prevCellSize;
        private Vector2 _prevSpacing;
        private float _prevGridCellSizeY = 0;
        private float _prevRectTransformSizeY = 0;

        void OnEnable()
        {
            _target = target as shaco.NumberLoopScrollActionComponent;
            _gridLayout = _target.gameObject.GetComponent<GridLayoutGroup>();
            _rectTransform = _target.gameObject.GetComponent<RectTransform>();

            _gridLayout.cellSize = new Vector2(_gridLayout.cellSize.x, _rectTransform.sizeDelta.y);

            _prevCellSize = _gridLayout.cellSize;
            _prevSpacing = _gridLayout.spacing;
            _prevGridCellSizeY = _gridLayout.cellSize.y;
            _prevRectTransformSizeY = _rectTransform.sizeDelta.y;
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, target.GetType().FullName);
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Text");
                _target.text = EditorGUILayout.TextArea(_target.text, GUILayout.MinHeight(48));
            }
            GUILayout.EndHorizontal();

            if (CheckGridLayoutUpdate())
            {
                _target.UpdateTextLayout();
            }
        }

        private bool CheckGridLayoutUpdate()
        {
            if (null == _gridLayout)
                return false;

            if (_prevCellSize != _gridLayout.cellSize)
            {
                _prevCellSize = _gridLayout.cellSize;
                return true;
            }
            else if (_prevSpacing != _gridLayout.spacing)
            {
                _prevSpacing = _gridLayout.spacing;
                return true;
            }
            else if (_prevGridCellSizeY != _gridLayout.cellSize.y)
            {
                _prevGridCellSizeY = _gridLayout.cellSize.y;
                _prevRectTransformSizeY = _gridLayout.cellSize.y;

                _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, _prevGridCellSizeY);
                return true;
            }
            else if (_prevRectTransformSizeY != _rectTransform.sizeDelta.y)
            {
                _prevGridCellSizeY = _rectTransform.sizeDelta.y;
                _prevRectTransformSizeY = _rectTransform.sizeDelta.y;

                _gridLayout.cellSize = new Vector2(_gridLayout.cellSize.x, _prevGridCellSizeY);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

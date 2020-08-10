using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.ListViewComponent))]
    public class ListViewComponentInspector : Editor
    {
        private readonly GUIContent[] HorizontalDirections = new GUIContent[] { new GUIContent("Automatic"), new GUIContent("Left->Right"), new GUIContent("Right->Left") };
        private readonly GUIContent[] VerticalDirections = new GUIContent[] { new GUIContent("Automatic"), new GUIContent("Top->Bottom"), new GUIContent("Bottom->Top") };
        private readonly GUIContent[] AllDirections = new GUIContent[] { new GUIContent("Left->Right"), new GUIContent("Right->Left"), new GUIContent("Top->Bottom"), new GUIContent("Bottom->Top") };

        private shaco.ListViewComponent _target = null;

        [SerializeField]
        private AnimBool _isShowAdvancedSetting = new AnimBool(true);

        void OnEnable()
        {
            _target = target as shaco.ListViewComponent;
            _target.CheckCompoment();
            _isShowAdvancedSetting.value = shaco.GameHelper.datasave.ReadBool("ListViewInspector.ShowAdvancedSetting", false);
        }

        void OnDisable()
        {
            shaco.GameHelper.datasave.WriteBool("ListViewInspector.ShowAdvancedSetting", _isShowAdvancedSetting.value);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Undo.RecordObject(target, target.GetType().FullName);
            DrawListViewInsepectorGUI();
        }

        private void DrawListViewInsepectorGUI()
        {
            GUIContent[] validGroupDirection = null;
            int indexOffset = 0;
            int directionCountPre = 2;

            switch (_target.GetScrollDirection())
            {
                case shaco.Direction.Right:
                case shaco.Direction.Left: validGroupDirection = VerticalDirections; indexOffset = directionCountPre; break;
                case shaco.Direction.Down:
                case shaco.Direction.Up: validGroupDirection = HorizontalDirections; indexOffset = 0; break;
                default: shaco.Log.Error("unsupport direction=" + _target.GetScrollDirection()); break;
            }

            //Main Scroll Direction
            int scrollDirection = (int)_target.GetScrollDirection();
            var newScrollDirection = EditorGUILayout.Popup(new GUIContent("ScrollDirection", "主滚动方向"), scrollDirection, AllDirections);
            if (newScrollDirection != scrollDirection)
            {
                _target.SetGroupDirection(shaco.Direction.Automatic);
                _target.ChangeDirection((shaco.Direction)newScrollDirection);
            }         

            //Group Item Scroll Direction
            if (_target.eachOfGroup > 1)
            {
                int groupDirection = (int)_target.GetGroupDirection() % directionCountPre + 1;
                var newGroupDirection = EditorGUILayout.Popup(new GUIContent("GroupDirection", "次要滚动方向"), groupDirection, validGroupDirection);
                if (newGroupDirection != groupDirection)
                    _target.ChangeGroupItemDirection((shaco.Direction)(newGroupDirection + indexOffset - 1));
            }

            //center layout flag
            var centerLayoutTypeTmp = _target.GetCenterLayoutType();
            if (centerLayoutTypeTmp != shaco.ListViewComponent.CenterLayoutType.NoCenter)
            {
                var newIsCenterLayout = EditorGUILayout.Toggle(new GUIContent(centerLayoutTypeTmp.ToString(), "中心布局"), _target.isCenterLayout);
                if (newIsCenterLayout != _target.isCenterLayout)
                {
                    _target.isCenterLayout = newIsCenterLayout;
                    _target.UpdateCenterLayout();
                }
            }
            
            //first item offset
            _target.firstItemOffset = EditorGUILayout.Vector3Field(new GUIContent("First Item Offset", "第一个组建偏移量，会带动其他组建整体偏移"), _target.firstItemOffset);

            //item margin
            _target.itemMargin = EditorGUILayout.Vector3Field(new GUIContent("Item Margin", "主组建之间间隔大小"), _target.itemMargin);

            //group item margin
            if (_target.eachOfGroup > 1)
            {
                _target.groupItemMargin = EditorGUILayout.Vector3Field(new GUIContent("Group Item Margin", "次要组建之间间隔大小"), _target.groupItemMargin);
            }

            //each of group item count
            _target.eachOfGroup = EditorGUILayout.IntSlider(new GUIContent("Each Of Group", "每组组建数量"), _target.eachOfGroup, 1, 1000);

            DrawDebugTest();
            DrawAdvancedSetting();

            //check valud changed
            if (GUI.changed)
            {
                shacoEditor.EditorHelper.SetDirty(_target);
                _target.OnPublicValueChangedCheck();
            }
        }

        private void DrawAdvancedSetting()
        {
            Undo.RecordObject(this, this.GetType().FullName);
            _isShowAdvancedSetting.target = EditorHelper.Foldout(_isShowAdvancedSetting.target, new GUIContent("Advanced Settings", "高级设置"));

            //在过渡动画时候需要Repaint否则动画会卡顿
            if (_isShowAdvancedSetting.faded != 0 && _isShowAdvancedSetting.faded != 1)
                this.Repaint();

            if (EditorGUILayout.BeginFadeGroup(_isShowAdvancedSetting.faded))
            {
                GUILayout.BeginHorizontal();
                {
                    //front arrow
                    GUILayout.Label(new GUIContent("Front Arrow", "前置箭头，会在滚动到达最前端自动隐藏"), GUILayout.Width(80));
                    _target.frontArrow = (GameObject)EditorGUILayout.ObjectField(_target.frontArrow, typeof(GameObject), true);

                    //behind arrow
                    GUILayout.Label(new GUIContent("Behind Arrow", "后置箭头，会在滚动到达最后端自动隐藏"), GUILayout.Width(80));
                    _target.behindArrow = (GameObject)EditorGUILayout.ObjectField(_target.behindArrow, typeof(GameObject), true);
                }
                GUILayout.EndHorizontal();

                //auto update params
                GUILayout.BeginVertical("box");
                {
                    _target.autoUpdateItemCountWhenTiming = EditorGUILayout.IntSlider(new GUIContent("Auto Update Item Count", "当合适时机自动刷新的组建数量"), _target.autoUpdateItemCountWhenTiming, 0, 1000);
                    if (_target.autoUpdateItemCountWhenTiming > 0)
                    {
                        _target.autoUpdateItemTiming = (shaco.ListViewComponent.AutoUpdateItemTiming)EditorGUILayout.EnumPopup(new GUIContent("Update Timing", "自动更新组件的时机"), _target.autoUpdateItemTiming);
                        GUILayout.BeginHorizontal();
                        {
                            _target.autoUpdateItemMinIndex = EditorGUILayout.IntField(new GUIContent("MinIndex", "自动刷新组建的最小下标"), _target.autoUpdateItemMinIndex);
                            _target.autoUpdateItemMaxIndex = EditorGUILayout.IntField(new GUIContent("MaxIndex", "自动刷新组建的最大下标"), _target.autoUpdateItemMaxIndex);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();

                //max drag out of bounds ratio
                // if (_target.autoUpdateItemTiming == shaco.ListView.AutoUpdateItemTiming.WillDragOutOfBounds)
                {
                    GUILayout.BeginVertical("box");
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Max Drag Out of Bounds");
                            _target.isMultipleDragOutOfBoundsSet = EditorGUILayout.Toggle(new GUIContent("Allowed Multiple", "允许同时前后拖拽比率"), _target.isMultipleDragOutOfBoundsSet, GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndHorizontal();

                        if (_target.isMultipleDragOutOfBoundsSet)
                        {
                            _target.maxDragOutOfFrontBoundsRatio = EditorGUILayout.Slider(new GUIContent("Front Ratio", "允许拖拽超出前置组建的比率"), _target.maxDragOutOfFrontBoundsRatio, 0, 1.0f);
                            _target.maxDragOutOfBehindBoundsRatio = EditorGUILayout.Slider(new GUIContent("Behind Ratio", "允许拖拽超出后置组建的比率"), _target.maxDragOutOfBehindBoundsRatio, 0, 1.0f);
                        }
                        else
                        {
                            _target.maxDragOutOfFrontBoundsRatio = EditorGUILayout.Slider(new GUIContent("Ratio", "允许拖拽超出前后组建的比率"), _target.maxDragOutOfFrontBoundsRatio, 0, 1.0f);
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawDebugTest()
        {
            //open debug mode
            if (_target.openDebugMode)
            {
                GUILayout.BeginVertical("box");
                {
                    _target.openDebugMode = EditorGUILayout.Toggle(new GUIContent("Debug Mode", "调试模式"), _target.openDebugMode);

                    if (_target.openDebugMode)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("Add"))
                            {
                                var model = _target.AddItemByModel();
                                if (null != model)
                                {
                                    Undo.RegisterCreatedObjectUndo(model, model.name);
                                }
                            }

                            if (GUILayout.Button("Remove"))
                            {
                                _target.RemoveItem(_target.Count - 1);
                            }

                            if (GUILayout.Button("Clear"))
                            {
                                _target.ClearItem();
                            }

                            if (GUILayout.Button("Location"))
                            {
                                // var item = _target.GetItem(_target.Count - 1);
                                // _target.LocationActionByGameObject(item, 1.0f);
                                _target.LocationActionByItemIndex(0, 1.0f);
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
            }
            else 
            {
                _target.openDebugMode = EditorGUILayout.Toggle(new GUIContent("Debug Mode", "调试模式"), _target.openDebugMode);
            }
        }
    }
}
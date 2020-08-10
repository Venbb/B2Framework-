using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.TouchScroll
{
    /// <summary>
    /// 基于GUILayout的ScrollView，提供拖拽滚动的方法
    /// </summary>
    public class TouchScrollView
    {
        public Vector2 scrollPosition = Vector2.zero;

        private bool _isDraging = false;
        private Rect _viewRect;
        private Rect _clipRect;
        private System.Type _guiScrollGroupType = null;
        private System.Reflection.MethodInfo _guiScrollGroupMethod_BeginLayoutGroup = null;
        private System.Reflection.MethodInfo _guiScrollGroupMethod_EndLayoutGroup = null;
        private System.Reflection.MethodInfo _guiScrollGroupMethod_ApplyOptions = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_rect = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_clientWidth = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_clientHeight = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_calcMaxWidth = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_calcMaxHeight = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_resetCoords = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_isVertical = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_stretchWidth = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_stretchHeight = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_verticalScrollbar = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_horizontalScrollbar = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_needsVerticalScrollbar = null;
        private System.Reflection.FieldInfo _guiScrollGroupField_needsHorizontalScrollbar = null;

        //鼠标焦点处理，需要在拖拽期间保持鼠标焦点，否则拖拽到屏幕外后会丢失响应
        private int _grabMouseControlID = -1;
        private bool _isGrabedMouse = false;
        private System.Reflection.MethodInfo _guiMethod_GrabMouseControl = null;
        private System.Reflection.MethodInfo _guiMethod_ReleaseMouseControl = null;

        /// <summary>
        /// 是否正在拖拽滚动窗口
        /// </summary>
        public bool IsDraging()
        {
            return _isDraging;
        }

        /// <summary>
        /// 开启滚动窗口布局，不支持嵌套使用
        /// </summary>
        public void BeginScrollView(params GUILayoutOption[] options)
        {
            if (null == _guiScrollGroupMethod_BeginLayoutGroup)
            {
                var bindingFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                _guiScrollGroupType = System.Type.GetType("UnityEngine.GUIScrollGroup, UnityEngine");
                _guiScrollGroupMethod_BeginLayoutGroup = typeof(GUILayoutUtility).GetMethod("BeginLayoutGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                _guiScrollGroupMethod_EndLayoutGroup = typeof(GUILayoutUtility).GetMethod("EndLayoutGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                _guiScrollGroupMethod_ApplyOptions = _guiScrollGroupType.GetMethod("ApplyOptions", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                _guiScrollGroupField_rect = _guiScrollGroupType.GetField("rect", bindingFlags);
                _guiScrollGroupField_clientWidth = _guiScrollGroupType.GetField("clientWidth", bindingFlags);
                _guiScrollGroupField_clientHeight = _guiScrollGroupType.GetField("clientHeight", bindingFlags);
                _guiScrollGroupField_calcMaxWidth = _guiScrollGroupType.GetField("calcMaxWidth", bindingFlags);
                _guiScrollGroupField_calcMaxHeight = _guiScrollGroupType.GetField("calcMaxHeight", bindingFlags);
                _guiScrollGroupField_resetCoords = _guiScrollGroupType.GetField("resetCoords", bindingFlags);
                _guiScrollGroupField_isVertical = _guiScrollGroupType.GetField("isVertical", bindingFlags);
                _guiScrollGroupField_stretchWidth = _guiScrollGroupType.GetField("stretchWidth", bindingFlags);
                _guiScrollGroupField_stretchHeight = _guiScrollGroupType.GetField("stretchHeight", bindingFlags);
                _guiScrollGroupField_verticalScrollbar = _guiScrollGroupType.GetField("verticalScrollbar", bindingFlags);
                _guiScrollGroupField_horizontalScrollbar = _guiScrollGroupType.GetField("horizontalScrollbar", bindingFlags);
                _guiScrollGroupField_needsVerticalScrollbar = _guiScrollGroupType.GetField("needsVerticalScrollbar", bindingFlags);
                _guiScrollGroupField_needsHorizontalScrollbar = _guiScrollGroupType.GetField("needsHorizontalScrollbar", bindingFlags);

                _guiMethod_GrabMouseControl = typeof(GUI).GetMethod("GrabMouseControl", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly);
                _guiMethod_ReleaseMouseControl = typeof(GUI).GetMethod("ReleaseMouseControl", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly);
            }

            //开始布局
            var guiScrollGroup = _guiScrollGroupMethod_BeginLayoutGroup.Invoke(null, new object[] { GUI.skin.scrollView, null, _guiScrollGroupType });

            //获取视角矩形范围
            var layoutRect = _guiScrollGroupField_rect.GetValue(guiScrollGroup);
            _clipRect = (Rect)layoutRect;

            switch (Event.current.type)
            {
                case EventType.Layout:
                    {
                        //设置滚动参数
                        _guiScrollGroupField_resetCoords.SetValue(guiScrollGroup, true);
                        _guiScrollGroupField_isVertical.SetValue(guiScrollGroup, true);
                        _guiScrollGroupField_stretchWidth.SetValue(guiScrollGroup, 1);
                        _guiScrollGroupField_stretchHeight.SetValue(guiScrollGroup, 1);
                        _guiScrollGroupField_verticalScrollbar.SetValue(guiScrollGroup, GUI.skin.verticalScrollbar);
                        _guiScrollGroupField_horizontalScrollbar.SetValue(guiScrollGroup, GUI.skin.horizontalScrollbar);
                        _guiScrollGroupField_needsVerticalScrollbar.SetValue(guiScrollGroup, false);
                        _guiScrollGroupField_needsHorizontalScrollbar.SetValue(guiScrollGroup, false);
                        _guiScrollGroupMethod_ApplyOptions.Invoke(guiScrollGroup, new object[] { options });
                        break;
                    }
                default: break;
            }

            //获取滚动裁剪矩形范围
            var clientWidth = (float)_guiScrollGroupField_clientWidth.GetValue(guiScrollGroup);
            var clientHeight = (float)_guiScrollGroupField_clientHeight.GetValue(guiScrollGroup);
            var calcMaxWidth = (float)_guiScrollGroupField_calcMaxWidth.GetValue(guiScrollGroup);
            var calcMaxHeight = (float)_guiScrollGroupField_calcMaxHeight.GetValue(guiScrollGroup);
            _viewRect = new Rect(0, 0, calcMaxWidth, calcMaxHeight);

            //刷新拖拽事件
            UpdateGragEvent();

            scrollPosition = GUI.BeginScrollView(_clipRect, scrollPosition, new Rect(0, 0, clientWidth, clientHeight));
        }

        /// <summary>
        /// 停止滚动窗口布局，不支持嵌套使用
        /// </summary>
        public void EndScrollView()
        {
            GUI.EndScrollView();

            //停止布局
            _guiScrollGroupMethod_EndLayoutGroup.Invoke(null, null);
        }

        /// <summary>
        /// 事件刷新，拖拽手势处理
        /// </summary>
        private void UpdateGragEvent()
        {
            var currentEvent = Event.current;
            if (null == currentEvent || currentEvent.type == EventType.Layout)
                return;

            switch (currentEvent.type)
            {
                case EventType.MouseDown:
                    {
                        var mousePosition = currentEvent.mousePosition;
                        _isDraging = _clipRect.Contains(mousePosition);

                        //如果界面滚动区域不足滚动时候则不用滚动了
                        if (_isDraging)
                        {
                            if (_clipRect.width >= _viewRect.width && _clipRect.height >= _viewRect.height)
                                _isDraging = false;
                        }

                        if (_isDraging)
                        {
                            //判断点击区域是否在滚动条范围内，防止和滚动条拖动事件冲突
                            var horizontalScrollbar = GUI.skin.horizontalScrollbar;
                            var verticalScrollbar = GUI.skin.verticalScrollbar;
                            if (mousePosition.x > _clipRect.width + _clipRect.x - verticalScrollbar.fixedWidth)
                                _isDraging = false;
                            else if (mousePosition.y > _clipRect.height + _clipRect.y - horizontalScrollbar.fixedHeight)
                                _isDraging = false;
                        }
                        break;
                    }
                case EventType.Ignore:
                case EventType.MouseUp:
                    {
                        if (_isGrabedMouse)
                        {
                            _grabMouseControlID = -1;
                            _isGrabedMouse = false;

                            if (null != _guiMethod_ReleaseMouseControl)
                            {
                                _guiMethod_ReleaseMouseControl.Invoke(null, null);
                            }
                        }
                        _isDraging = false;
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        if (_isDraging)
                        {
                            //真机和PC的滑动方向是相反的
#if UNITY_EDITOR
                            scrollPosition -= new Vector2(currentEvent.delta.x, currentEvent.delta.y);
#else
                            scrollPosition -= new Vector2(currentEvent.delta.x, -currentEvent.delta.y);
#endif

                            //拖拽开始时候保留鼠标控制
                            if (!_isGrabedMouse && null != _guiMethod_GrabMouseControl)
                            {
                                _isGrabedMouse = true;
                                _grabMouseControlID = GUIUtility.GetControlID(FocusType.Keyboard);
                                _guiMethod_GrabMouseControl.Invoke(null, new object[] { _grabMouseControlID });
                            }
                        }
                        break;
                    }
            }
        }
    }
}
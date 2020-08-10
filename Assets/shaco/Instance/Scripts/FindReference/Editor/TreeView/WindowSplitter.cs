using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shaco.Instance.Editor.TreeView
{
    /// <summary>
    /// 编辑器窗口分割线
    /// </summary>
    public class WindowSplitter
    {
        public enum Direction
        {
            Horizontol = 0,
            Vertical
        }

        public bool isDragSplitter = false;

        private readonly float MIN_SPLIT_WINDOW_SIZE = 50;

        private int _indexDragSplitWindow = -1;
        private List<Rect> _rectWindows = new List<Rect>();
        private Direction _direction = Direction.Horizontol;
        private float _splitterLineWidth = 1;
        private EditorWindow _windowTarget = null;
        private Rect _prevFullWindowRect = new Rect();
        private int _lastBeginAreaWindowIndex = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WindowSplitter(Direction direction = Direction.Horizontol, float splitterLineWidth = 1)
        {
            this._direction = direction;
            this._splitterLineWidth = splitterLineWidth;
        }

        /// <summary>
        /// 设置初始的分屏窗口大小
        /// <param name="target">窗口对象</param>
        /// <param name="defaultSplitPercents">默认分割比率，允许分割多个窗口</param>
        /// </summary>
        public void SetSplitWindow(EditorWindow target, params float[] defaultSplitPercents)
        {
            if (null == target)
            {
                Debug.LogError("WindowSplitter SetWindow error: invalid parameters");
                return;
            }

            var rectTmp = target.position;
            rectTmp.x = rectTmp.y = 0;
            SetSplitWindow(rectTmp, defaultSplitPercents);
            this._windowTarget = target;
        }

        /// <summary>
        /// 设置初始的分屏窗口大小
        /// <param name="windowRect">窗口区域大小</param>
        /// <param name="defaultSplitPercents">默认分割比率，允许分割多个窗口</param>
        /// </summary>
        public void SetSplitWindow(Rect windowRect, params float[] defaultSplitPercents)
        {
            if (windowRect.width == 0 || windowRect.height == 0)
            {
                Debug.LogError("WindowSplitter SetWindow error: invalid parameters");
                return;
            }

            this._windowTarget = null;
            if (null != defaultSplitPercents && defaultSplitPercents.Length > 0)
            {
                //获取总分割比率，计算每个窗口自身比率
                float allPercent = 0;
                for (int i = 0; i < defaultSplitPercents.Length; ++i)
                {
                    allPercent += defaultSplitPercents[i];
                }
                for (int i = 0; i < defaultSplitPercents.Length; ++i)
                {
                    defaultSplitPercents[i] = defaultSplitPercents[i] / allPercent;
                }
            }
            else
            {
                defaultSplitPercents = GetSplitWindowsPercent();
            }

            //刷新拆分区域大小
            UpdateWindowSize(windowRect, defaultSplitPercents);
        }

        /// <summary>
        /// 开始绘制分割窗口
        /// <param name="firstLayout">是否为第一个分割窗口</param>
        /// <param name="parentSplitter">父分割线，当多重WindowSplitter包含使用的时候使用</param>
        /// </summary>
        public void BeginLayout(bool firstLayout)
        {
            if (firstLayout)
            {
                _lastBeginAreaWindowIndex = 0;
            }
            BeginLayout();
        }

        public void BeginLayout()
        {
            var rectTmp = GetSplitWindowRect(_lastBeginAreaWindowIndex, false);
            // rectTmp = GetReadRectWithParentSplitter(rectTmp);
            GUILayout.BeginArea(rectTmp);
        }

        /// <summary>
        /// 结束绘制分割窗口
        /// </summary>
        public void EndLayout()
        {
            GUILayout.EndArea();
            if (_lastBeginAreaWindowIndex++ == _rectWindows.Count - 1)
            {
                this.Draw();
                _lastBeginAreaWindowIndex = 0;
            }
        }

        /// <summary>
        /// 获取拆分窗口矩形大小
        /// <param name="index">窗口下标，从左往右，从上往下</param>
        /// <param name="isAutoLayout">是否使用自动布局，如果为true则返回的x和y为0</param>
        /// <return>矩形大小</return>
        /// </summary>
        public Rect GetSplitWindowRect(int index, bool isAutoLayout = true)
        {
            if (index < 0 || index > _rectWindows.Count - 1)
            {
                Debug.LogError("WindowSplitter GetSplitWindowRect error: out of range, index=" + index + " count=" + _rectWindows.Count);
                return new Rect();
            }

            var retValue = _rectWindows[index];
            if (isAutoLayout)
            {
                retValue.x = retValue.y = 0;
                // if (null != this._windowTarget || 0 != index)
                // else 
                // {
                //     retValue.x = _prevFullWindowRect.x;
                //     retValue.y = _prevFullWindowRect.y;
                // }
            }
            return retValue;
        }

        /// <summary>
        /// 获取拆分窗口所占总窗口大小的百分比(0 ~ 1)
        /// <param name="index">窗口下标，从左往右，从上往下</param>
        /// <return>所占百分比</return>
        /// </summary>
        public float GetSplitWindowPercent(int index)
        {
            if (index < 0 || index > _rectWindows.Count - 1)
            {
                Debug.LogError("WindowSplitter GetSplitWindowPercent error: out of range, index=" + index + " count=" + _rectWindows.Count);
                return 0;
            }
            return GetSplitWindowsPercent()[index];
        }

        /// <summary>
        /// 刷新整个窗口大小，动态调整分屏窗口绘制
        /// <param name="fullWindowSize">2个分屏窗口中整个窗口的大小</param>
        /// <param name="splitPercents">拆分比率</param>
        /// </summary>
        public void UpdateWindowSize(Rect windowRect)
        {
            UpdateWindowSize(windowRect, GetSplitWindowsPercent());
        }

        private void UpdateWindowSize(Rect windowRect, float[] splitPercents)
        {
            if (_prevFullWindowRect.width != windowRect.width && _prevFullWindowRect.height != windowRect.height)
            {
                //如果设置了窗口目标。则默认为自动匹配整个窗口大小
                if (null != _windowTarget)
                {
                    windowRect.x = windowRect.y = 0;
                }

                //设置拆分区域大小
                _rectWindows.Clear();
                float offsetSize = 0;
                switch (_direction)
                {
                    case Direction.Horizontol:
                        {
                            offsetSize = windowRect.x;
                            for (int i = 0; i < splitPercents.Length; ++i)
                            {
                                float currentSplitPercent = splitPercents[i];
                                Rect rectTmp = new Rect(offsetSize, windowRect.y, windowRect.size.x * currentSplitPercent, windowRect.size.y);
                                _rectWindows.Add(rectTmp);
                                offsetSize = rectTmp.xMax;
                            }
                            break;
                        }
                    case Direction.Vertical:
                        {
                            offsetSize = windowRect.y;
                            for (int i = 0; i < splitPercents.Length; ++i)
                            {
                                float currentSplitPercent = splitPercents[i];
                                Rect rectTmp = new Rect(windowRect.x, offsetSize, windowRect.size.x, windowRect.size.y * currentSplitPercent);
                                _rectWindows.Add(rectTmp);
                                offsetSize = rectTmp.yMax;
                            }
                            break;
                        }
                    default:
                        {
                            Debug.LogError("WindpwSplitter UpdateWindowSize error: unsupport direction type=" + _direction);
                            break;
                        }
                }
                _prevFullWindowRect = windowRect;
            }
        }

        /// <summary>
        /// 绘制分屏窗口
        /// <return>如果有拖拽窗口返回true，反之false</return>
        /// </summary>
        private bool Draw()
        {
            bool retValue = false;

            if (null != _windowTarget && _prevFullWindowRect != _windowTarget.position)
            {
                UpdateWindowSize(_windowTarget.position);
            }

            retValue = HandleWindowResize();
            DrawSplitLine();
            return retValue;
        }

        // /// <summary>
        // /// 根据父分割线获取偏移位置
        // /// <param name="rect">原始矩形区域</param>
        // /// <param name="parentSplitter">父分割线，当多重WindowSplitter包含使用的时候使用</param>
        // /// <return>计算过偏移量的矩形区域</return>
        // /// </summary>
        // private Rect GetReadRectWithParentSplitter(Rect rect, WindowSplitter parentSplitter)
        // {
        //     var retValue = rect;
        //     if (null != parentSplitter)
        //     {
        //         var otherRect = parentSplitter.GetSplitWindowRect(parentSplitter._lastBeginAreaWindowIndex);
        //         switch (parentSplitter._direction)
        //         {
        //             case Direction.Horizontol:
        //                 {
        //                     retValue.width = otherRect.width;
        //                     break;
        //                 }
        //             case Direction.Vertical:
        //                 {
        //                     retValue.height = otherRect.height;
        //                     break;
        //                 }
        //             default:
        //                 {
        //                     Debug.LogError("WindowSplitter GetReadRectWithParentSplitter error: unsupport direction type=" + parentSplitter._direction);
        //                     break;
        //                 }
        //         }
        //     }
        //     return retValue;
        // }

        /// <summary>
        /// 绘制分割线
        /// </summary>
        private void DrawSplitLine()
        {
            if (null == Event.current || Event.current.type != EventType.Repaint)
                return;

            //绘制带颜色的分割线
            Color colorTmp = new Color();
            if (EditorGUIUtility.isProSkin)
            {
                colorTmp.r = 0.12f;
                colorTmp.g = 0.12f;
                colorTmp.b = 0.12f;
                colorTmp.a = 1.0f;
            }
            else
            {
                colorTmp.r = 0.6f;
                colorTmp.g = 0.6f;
                colorTmp.b = 0.6f;
                colorTmp.a = 1.333f;
            }

            Color orgColor = GUI.color;
            GUI.color = GUI.color * colorTmp;

            for (int i = 0; i < _rectWindows.Count; ++i)
            {
                var startPoint = Vector2.zero;
                var endPoint = Vector2.zero;
                var rectTmp = _rectWindows[i];

                switch (_direction)
                {
                    case Direction.Horizontol:
                        {
                            startPoint = new Vector2(rectTmp.xMax, rectTmp.y);
                            endPoint = new Vector2(rectTmp.xMax, rectTmp.yMax);
                            break;
                        }
                    case Direction.Vertical:
                        {
                            startPoint = new Vector2(rectTmp.x, rectTmp.yMax);
                            endPoint = new Vector2(rectTmp.xMax, rectTmp.yMax);
                            break;
                        }
                    default:
                        {
                            Debug.LogError("WindowSplitter DrawSplitLine error: unsupport direction type=" + _direction);
                            break;
                        }
                }

                DrawLine(startPoint, endPoint, _splitterLineWidth);
            }

            GUI.color = orgColor;
        }

        static private float AngleFixed(Vector3 src, Vector3 des)
        {
            Vector3 dir = des - src;
            float angle = Vector3.Angle(dir, Vector3.up);
            if (src.x < des.x)
                angle = -angle;

            return angle;
        }

        static private void DrawLine(Vector3 start, Vector3 end, float lineWidth = 1)
        {
            Vector3 dir = end - start;
            float angle = AngleFixed(start, end);
            float height = dir.magnitude;

            //draw line
            Matrix4x4 matrixOldTmp = GUI.matrix;
            var pivotPoint = new Vector2(start.x + lineWidth / 2, start.y);
            GUIUtility.RotateAroundPivot(angle, pivotPoint);
            GUI.DrawTexture(new Rect(start.x, start.y, lineWidth, height), Texture2D.whiteTexture);
            GUI.matrix = matrixOldTmp;
        }

        /// <summary>
        /// 获取所有分割窗口所占百分比
        /// </summary>
        private float[] GetSplitWindowsPercent()
        {
            var retValue = new float[_rectWindows.Count];
            for (int i = 0; i < _rectWindows.Count; ++i)
            {
                switch (_direction)
                {
                    case Direction.Horizontol:
                        {
                            retValue[i] = _rectWindows[i].size.x / _prevFullWindowRect.size.x;
                            break;
                        }
                    case Direction.Vertical:
                        {
                            retValue[i] = _rectWindows[i].size.y / _prevFullWindowRect.size.y;
                            break;
                        }
                    default:
                        {
                            Debug.LogError("WindowSplitter GetSplitWindowsPercent error: unsupport direction type=" + _direction);
                            break;
                        }
                }
            }
            return retValue;
        }

        /// <summary>
        /// 刷新分屏窗口大小
        /// <param name="index">下标</param>
        /// <param name="windowSize">新的窗口大小</param>
        /// </summary>
        private void UpdateSplitWindowsSize(int index, Vector2 windowSize)
        {
            if (index < 0 || index > _rectWindows.Count - 2)
            {
                Debug.LogError("WindowSplitter UpdateSplitWindowSize error: out of range, index=" + index + " count=" + _rectWindows.Count);
                return;
            }

            var sizeOffset = windowSize - _rectWindows[index].size;

            switch (_direction)
            {
                case Direction.Horizontol:
                    {
                        var currentWindow = _rectWindows[index];
                        var otherWindow = _rectWindows[index + 1];
                        _rectWindows[index] = new Rect(currentWindow.x, currentWindow.y, currentWindow.size.x + sizeOffset.x, currentWindow.size.y);
                        _rectWindows[index + 1] = new Rect(otherWindow.x + sizeOffset.x, otherWindow.y, otherWindow.size.x - sizeOffset.x, otherWindow.size.y);
                        break;
                    }
                case Direction.Vertical:
                    {
                        var currentWindow = _rectWindows[index];
                        var otherWindow = _rectWindows[index + 1];
                        _rectWindows[index] = new Rect(currentWindow.x, currentWindow.y, currentWindow.size.x, currentWindow.size.y + sizeOffset.y);
                        _rectWindows[index + 1] = new Rect(otherWindow.x, otherWindow.y + sizeOffset.y, otherWindow.size.x, otherWindow.size.y - sizeOffset.y);
                        break;
                    }
                default:
                    {
                        Debug.LogError("WindowSplitter UpdateSplitWindowsSize error: unsupport direction type=" + _direction);
                        break;
                    }
            }
        }

        /// <summary>
        /// 控制左右窗口大小
        /// <return>如果窗口有大小变化返回true，反之false </return>
        /// </summary>
        private bool HandleWindowResize()
        {
            bool retValue = false;
            if (_indexDragSplitWindow >= 0)
            {
                if (_windowTarget != null)
                    _windowTarget.Repaint();
                else if (null != EditorWindow.focusedWindow)
                    EditorWindow.focusedWindow.Repaint();
            }

            if (null == Event.current)
                return retValue;

            float expandSplitterLineWidthTmp = _splitterLineWidth * 6;

            //当鼠标放置在分割线上，绘制拖动图标
            //最后一个窗口右边的分割线忽略
            if (!isDragSplitter)
            {
                _indexDragSplitWindow = -1;
                for (int i = 0; i < _rectWindows.Count - 1; ++i)
                {
                    var rectTmp = _rectWindows[i];
                    switch (_direction)
                    {
                        case Direction.Horizontol:
                            {
                                rectTmp = new Rect(rectTmp.xMax - expandSplitterLineWidthTmp, rectTmp.y, expandSplitterLineWidthTmp * 2, rectTmp.height);
                                break;
                            }
                        case Direction.Vertical:
                            {
                                rectTmp = new Rect(rectTmp.x, rectTmp.yMax - expandSplitterLineWidthTmp, rectTmp.width, expandSplitterLineWidthTmp * 2);
                                break;
                            }
                        default:
                            {
                                Debug.LogError("WindowSplitter HandleWindowResize error: unsupport direction type=" + _direction);
                                break;
                            }
                    }
                    if (rectTmp.Contains(Event.current.mousePosition))
                    {
                        _indexDragSplitWindow = i;
                        break;
                    }
                }
            }

            //按下鼠标准备拖拽分割线
            if (_indexDragSplitWindow >= 0 && Event.current.type == EventType.MouseDown)
            {
                isDragSplitter = true;
            }

            if (isDragSplitter)
            {
                //拽动分割线，重新计算窗口大小
                var currentRect = _rectWindows[_indexDragSplitWindow];
                var nextRect = _rectWindows[_indexDragSplitWindow + 1];

                Vector2 modifyWindowSize = Vector2.zero;
                var clampMousePosition = Event.current.mousePosition;

                switch (_direction)
                {
                    case Direction.Horizontol:
                        {
                            //控制分割窗口最大大小
                            if (clampMousePosition.x > nextRect.xMax - MIN_SPLIT_WINDOW_SIZE)
                                clampMousePosition.x = nextRect.xMax - MIN_SPLIT_WINDOW_SIZE;

                            modifyWindowSize = new Vector2(clampMousePosition.x - currentRect.position.x, currentRect.y);

                            //控制分割窗口最小大小
                            if (modifyWindowSize.x < MIN_SPLIT_WINDOW_SIZE)
                                modifyWindowSize.x = MIN_SPLIT_WINDOW_SIZE;
                            break;
                        }
                    case Direction.Vertical:
                        {
                            //控制分割窗口最大大小
                            if (clampMousePosition.y > nextRect.yMax - MIN_SPLIT_WINDOW_SIZE)
                                clampMousePosition.y = nextRect.yMax - MIN_SPLIT_WINDOW_SIZE;

                            modifyWindowSize = new Vector2(currentRect.y, clampMousePosition.y - currentRect.position.y);

                            //控制分割窗口最小大小
                            if (modifyWindowSize.y < MIN_SPLIT_WINDOW_SIZE)
                                modifyWindowSize.y = MIN_SPLIT_WINDOW_SIZE;
                            break;
                        }
                    default:
                        {
                            Debug.LogError("WindowSplitter HandleWindowResize error: unsupport direction type=" + _direction);
                            break;
                        }
                }

                UpdateSplitWindowsSize(_indexDragSplitWindow, modifyWindowSize);
                retValue = true;
            }

            //当鼠标在分割线上的时候，才绘制图标
            if (_indexDragSplitWindow >= 0)
            {
                var rectTmp = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 60, 60);
                rectTmp.x -= 30;
                rectTmp.y -= 30;
                EditorGUIUtility.AddCursorRect(rectTmp, _direction == Direction.Horizontol ? MouseCursor.SplitResizeLeftRight : MouseCursor.SplitResizeUpDown);
            }

            //停止拖拽分割线
            if (Event.current.type == EventType.MouseUp)
            {
                isDragSplitter = false;
                _indexDragSplitWindow = -1;
            }
            return retValue;
        }
    }
}
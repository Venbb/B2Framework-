using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace shaco
{
    /// <summary>
    /// 实现镂空效果的Mask组件
    /// </summary>
    public class HollowOutMaskComponent : MaskableGraphic, ICanvasRaycastFilter
    {
        public bool isOnlyTouchEndEvent = true;
       
        [SerializeField]
        [shaco.CantBeNull]
        private RectTransform _targetRect = null;

        [SerializeField]
        private RectTransform _eventRect = null;

        //拖拽差值，大于该值表示在拖动中
        [SerializeField]
        private float _dragTolerance = 0.01f;

        private Vector3 _targetMin = Vector3.zero;
        private Vector3 _targetMax = Vector3.zero;

        private bool _canRefresh = true;
        private RectTransform _cacheTrans = null;
        private System.Action<Vector2> _onClickCallBack = null;
        private Vector3 _mouseTouchBeganPos;
        private TouchPhase _currentTouchPhase = TouchPhase.Stationary;
        private bool _isWillExecuteCallBack = false;
        private Vector2 _touchScreenPos;
        private bool _hasTouchMoved = false;

        public Vector3 GetTargetLocalPosition()
        {
            return _targetRect.localPosition;
        }

        public void SetTarget(Vector3 worldPos, Vector3 hollowLocalOffset, Vector2 hollowSize, Vector2 eventSize)
        {
            _canRefresh = true;
            _targetRect.position = worldPos;
            _currentTouchPhase = TouchPhase.Stationary;
            _targetRect.localPosition += hollowLocalOffset;
            _targetRect.sizeDelta = hollowSize;

            if (null != _eventRect)
            {
                _eventRect.position = worldPos;
                _eventRect.sizeDelta = eventSize;
            }
            _RefreshView();
        }

        public void RemoveClickCalllBack(System.Action<Vector2> callback)
        {
            _onClickCallBack -= callback;
        }

        public void AddClickCalllBack(System.Action<Vector2> callback)
        {
            _onClickCallBack += callback;
        }

        public void ClearClickCallBack()
        {
            _onClickCallBack = null;
        }

        private void _SetTarget(Vector3 tarMin, Vector3 tarMax)
        {
            if (tarMin == _targetMin && tarMax == _targetMax)
                return;
            _targetMin = tarMin;
            _targetMax = tarMax;
            SetAllDirty();
        }

        private void _RefreshView()
        {
            if (!_canRefresh) return;
            _canRefresh = false;

            if (null == _targetRect)
            {
                _SetTarget(Vector3.zero, Vector3.zero);
                SetAllDirty();
            }
            else
            {
                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(_cacheTrans, _targetRect);
                _SetTarget(bounds.min, bounds.max);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (_targetMin == Vector3.zero && _targetMax == Vector3.zero)
            {
                base.OnPopulateMesh(vh);
                return;
            }
            vh.Clear();

            // 填充顶点
            UIVertex vert = UIVertex.simpleVert;
            vert.color = color;

            Vector2 selfPiovt = rectTransform.pivot;
            Rect selfRect = rectTransform.rect;
            float outerLx = -selfPiovt.x * selfRect.width;
            float outerBy = -selfPiovt.y * selfRect.height;
            float outerRx = (1 - selfPiovt.x) * selfRect.width;
            float outerTy = (1 - selfPiovt.y) * selfRect.height;
            // 0 - Outer:LT
            vert.position = new Vector3(outerLx, outerTy);
            vh.AddVert(vert);
            // 1 - Outer:RT
            vert.position = new Vector3(outerRx, outerTy);
            vh.AddVert(vert);
            // 2 - Outer:RB
            vert.position = new Vector3(outerRx, outerBy);
            vh.AddVert(vert);
            // 3 - Outer:LB
            vert.position = new Vector3(outerLx, outerBy);
            vh.AddVert(vert);

            // 4 - Inner:LT
            vert.position = new Vector3(_targetMin.x, _targetMax.y);
            vh.AddVert(vert);
            // 5 - Inner:RT
            vert.position = new Vector3(_targetMax.x, _targetMax.y);
            vh.AddVert(vert);
            // 6 - Inner:RB
            vert.position = new Vector3(_targetMax.x, _targetMin.y);
            vh.AddVert(vert);
            // 7 - Inner:LB
            vert.position = new Vector3(_targetMin.x, _targetMin.y);
            vh.AddVert(vert);

            // 设定三角形
            vh.AddTriangle(4, 0, 1);
            vh.AddTriangle(4, 1, 5);
            vh.AddTriangle(5, 1, 2);
            vh.AddTriangle(5, 2, 6);
            vh.AddTriangle(6, 2, 3);
            vh.AddTriangle(6, 3, 7);
            vh.AddTriangle(7, 3, 0);
            vh.AddTriangle(7, 0, 4);
        }

        bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 screenPos, Camera eventCamera)
        {
            if (null == _eventRect) 
                return true;

            if (IsTouchBegan())
            {
                _currentTouchPhase = TouchPhase.Began;
                _hasTouchMoved = false;
            }
            else if (IsTouchEnded())
                _currentTouchPhase = TouchPhase.Ended;
            else if (IsTouchMoved())
            {
                _currentTouchPhase = TouchPhase.Moved;
                _hasTouchMoved = true;
            }
            else
                _currentTouchPhase = TouchPhase.Stationary;

            if (isOnlyTouchEndEvent && _hasTouchMoved)
                return true;
            
            // 将目标对象范围内的事件镂空（使其穿过）
            var retValue = RectTransformUtility.RectangleContainsScreenPoint(_eventRect, screenPos, eventCamera);

            if (null != _onClickCallBack && retValue)
            {
                if (isOnlyTouchEndEvent && _currentTouchPhase == TouchPhase.Ended)
                {
                    _touchScreenPos = screenPos;
                    _isWillExecuteCallBack = true;
                }
            }

            if (_currentTouchPhase == TouchPhase.Ended)
                _currentTouchPhase = TouchPhase.Stationary;

            return !retValue;
        }

        protected override void Awake()
        {
            base.Awake();
            _cacheTrans = GetComponent<RectTransform>();
        }

        private bool IsMouseMoved()
        {
            if (!Input.mousePresent)
                return false;

            if (Input.GetMouseButtonUp(0))
            {
                return false;
            }

            if (Input.GetMouseButton(0))
            {
                var distance = Vector3.Distance(_mouseTouchBeganPos, Input.mousePosition);
                return distance > _dragTolerance;
            }
            else
                return false;
        }

        private bool IsTouchBegan()
        {
            if (Input.mousePresent && Input.GetMouseButtonDown(0))
            {
                _mouseTouchBeganPos = Input.mousePosition;
                return true;
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                return true;

            return false;
        }

        private bool IsTouchEnded()
        {
            if (Input.mousePresent && Input.GetMouseButtonUp(0))
                return true;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
                return true;

            return false;
        }

        private bool IsTouchMoved()
        {
            bool isMouseTouchMoved = IsMouseMoved();
            bool isTouchMoved = false;

            if (Input.touchCount > 0)
            {
                var touchesTmp = Input.touches;
                for (int i = touchesTmp.Length - 1; i >= 0; --i)
                {
                    var touchTmp = touchesTmp[i];
                    if (touchTmp.phase == TouchPhase.Moved && (touchTmp.deltaPosition.magnitude > _dragTolerance))
                    {
                        isTouchMoved = true;
                        break;
                    }
                }
            }
            return isMouseTouchMoved || isTouchMoved;
        }

        void Update()
        {
#if UNITY_EDITOR
            _canRefresh = true;
            _RefreshView();
#endif

            if (_isWillExecuteCallBack && null != _onClickCallBack)
            {
                _isWillExecuteCallBack = false;

                _onClickCallBack(_touchScreenPos);
                _onClickCallBack = null;
            }
        }
    }
}
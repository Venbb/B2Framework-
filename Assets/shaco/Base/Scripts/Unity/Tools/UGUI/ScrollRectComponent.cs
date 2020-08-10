using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace shaco
{
    public class ScrollRectComponent : ScrollRect
    {
        public System.Action<PointerEventData> onScrollingCallBack = null;
        public System.Action<PointerEventData> onBeginDragCallBack = null;
        public System.Action<PointerEventData> onEndDragCallBack = null;
        public System.Action onVeryFastScrollingEndCallBack = null;
        public PointerEventData lastDragData = null;

        //滚动速度过快阀值
        [SerializeField]
        [Range(0.01f, 0.5f)]
        [Header("快速滚动阀值")]
        private float _isVeryFastThreshold = 0.1f;
        private Vector2 _lastNormalizedPosition = Vector2.zero;
        private bool _isInitedLastNormalizedPos = false;
        private RectTransform _rectTransform = null;
        private Vector2 _offsetNormalizedPosition = Vector2.zero;
        private shaco.Direction _isVeryFastScrollDirection = shaco.Direction.None;

        private float _isVeryFastScrollEndDelayTime = 0;

        /// <summary>
        /// 判断是否快速滚动中
        /// </summary>
        public bool IsVeryFastScrolling(shaco.Direction direction)
        {
            bool retValue = false;
            switch (direction)
            {
                case shaco.Direction.Left:
                case shaco.Direction.Right:
                    {
                        retValue = System.Math.Abs(this._offsetNormalizedPosition.x) > _isVeryFastThreshold;
                        if (retValue)
                            _isVeryFastScrollDirection = direction;
                        break;
                    }
                case shaco.Direction.Up:
                case shaco.Direction.Down:
                    {
                        retValue = System.Math.Abs(this._offsetNormalizedPosition.y) > _isVeryFastThreshold;
                        if (retValue)
                            _isVeryFastScrollDirection = direction;
                        break;
                    }
                // default: shaco.Log.Error("ScrollRectEx IsVeryFastScrolling erorr: unsupport direction=" + direction); break;
            }
            return retValue;
        }

        protected override void Start()
        {
            base.Start();

            onValueChanged.RemoveListener(OnValueChangedCallBack);
            onValueChanged.AddListener(OnValueChangedCallBack);
            _rectTransform = this.GetComponent<RectTransform>();
        }

        public bool CanScroll()
        {
            if (null == _rectTransform)
                _rectTransform = this.GetComponent<RectTransform>();
            if (null == _rectTransform)
            {
                Log.ErrorFormat("ScrollBarEx CanScroll error: missing componnt '{0}'", typeof(RectTransform).FullName);
                return false;
            }

            if (horizontal && !vertical)
                return this.content.sizeDelta.x > _rectTransform.rect.size.x;
            else if (!horizontal && vertical)
                return this.content.sizeDelta.y > _rectTransform.rect.size.y;
            else
                return this.content.sizeDelta.x > _rectTransform.rect.size.x || this.content.sizeDelta.y > _rectTransform.rect.size.y;
        }

        private void OnValueChangedCallBack(Vector2 value)
        {
            if (!_isInitedLastNormalizedPos)
            {
                _isInitedLastNormalizedPos = true;
                _lastNormalizedPosition = this.normalizedPosition;
            }

            bool canScroll = CanScroll();
            if (canScroll)
            {
                _offsetNormalizedPosition = value - _lastNormalizedPosition;
                _lastNormalizedPosition = value;
                var contentSize = this.content.rect.size;

                if (null == this.lastDragData)
                    this.lastDragData = new PointerEventData(EventSystem.current);
                this.lastDragData.delta = -new Vector2(contentSize.x * _offsetNormalizedPosition.x, contentSize.y * _offsetNormalizedPosition.y);
            }

            if (null != onScrollingCallBack && null != this.lastDragData)
            {
                onScrollingCallBack(this.lastDragData);
            }
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (null != onVeryFastScrollingEndCallBack && _isVeryFastScrollDirection != shaco.Direction.None)
            {
                var isVeryFastScrolling = IsVeryFastScrolling(_isVeryFastScrollDirection);
                if (!isVeryFastScrolling)
                {
                    //固定等待0.1秒后如果没有快速拖拽的行为才表示快速拖拽结束
                    _isVeryFastScrollEndDelayTime += Time.deltaTime;
                    if (_isVeryFastScrollEndDelayTime >= 0.2f)
                    {
                        if (null != onVeryFastScrollingEndCallBack)
                            onVeryFastScrollingEndCallBack();

                        onVeryFastScrollingEndCallBack = null;
                        _isVeryFastScrollEndDelayTime = 0;
                        _isVeryFastScrollDirection = shaco.Direction.None;
                    }
                }
                else
                {
                    _isVeryFastScrollEndDelayTime = 0;
                }
                _offsetNormalizedPosition = Vector2.zero;
            }
        }

        // public override void OnDrag(PointerEventData eventData)
        // {
        //     base.OnDrag(eventData);
        //     lastDragData = eventData;

        //     if (null != onScrollingCallBack)
        //     {
        //         onScrollingCallBack(eventData);
        //     }
        // }

        public override void OnScroll(PointerEventData data)
        {
            base.OnScroll(data);
            lastDragData = data;        

            if (null != onScrollingCallBack)
            {
                onScrollingCallBack(data);
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            lastDragData = eventData;
            if (null != onBeginDragCallBack)
            {
                onBeginDragCallBack(eventData);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            lastDragData = eventData;
            if (null != onEndDragCallBack)
            {
                onEndDragCallBack(eventData);
// #if UNITY_5_3_OR_NEWER
//                 SetDirtyCaching();
// #endif
            }
        }

        public void StopDrag()
        {
            if (null != lastDragData)
            {
                base.OnBeginDrag(lastDragData);
                base.OnEndDrag(lastDragData);
                lastDragData = null;
            }
        }

        public void UpdateDragEvent(PointerEventData eventData)
        {
            lastDragData = eventData;
            if (null != onBeginDragCallBack)
            {
                eventData.delta = -eventData.delta;
                eventData.scrollDelta = -eventData.scrollDelta;
                onScrollingCallBack(eventData);
            }
        }
    }
}
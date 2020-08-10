using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//----------------------
//this scprit only can use on UGUI Unity4.6+
//----------------------
namespace shaco
{
    [RequireComponent(typeof(shaco.ScrollRectComponent))]
    public partial class ListViewComponent : MonoBehaviour
    {
        public int Count
        {
            get { return _listItems.Count; }
        }

        //主滚动方向
        [HideInInspector]
        [SerializeField]
        private shaco.Direction scrollDirection = shaco.Direction.Right;
        //每小组组件滚动方向
        [HideInInspector]
        [SerializeField]
        private shaco.Direction groupItemDirection = shaco.Direction.Automatic;

        //是否居中所有组建布局 
        [HideInInspector]
        public bool isCenterLayout = false;
        //是否开启Debug调试模式
        [HideInInspector]
        public bool openDebugMode = false;
        //第一个组件的位置
        [HideInInspector]
        public Vector3 firstItemOffset = new Vector3(0, 0, 0);
        //组件间距
        [HideInInspector]
        public Vector3 itemMargin = new Vector3(0, 0, 0);
        //每小组组件间距
        [HideInInspector]
        public Vector3 groupItemMargin = new Vector3(0, 0, 0);
        //每小组组件数量
        [HideInInspector]
        [Range(1, 1000)]
        public int eachOfGroup = 1;
        //自动刷新组件时机
        [HideInInspector]
        public AutoUpdateItemTiming autoUpdateItemTiming = AutoUpdateItemTiming.WhenScrolling;
        //当合适时机自动刷新的组件数量，如果<=0则不自动刷新
        //该参数建议不要超过当前ListView已有组件数量，否则会自动使用ListView当前组件数量替代该参数
        [HideInInspector]
        [Range(1, 1000)]
        public int autoUpdateItemCountWhenTiming = 0;
        //当滚动回弹的时候自动刷新组建最小下标
        [HideInInspector]
        public int autoUpdateItemMinIndex = 0;
        //当滚动回弹的时候自动刷新组建最大下标
        [HideInInspector]
        public int autoUpdateItemMaxIndex = 9;
        //是否同时设置多个拖拽最大边界
        [HideInInspector]
        public bool isMultipleDragOutOfBoundsSet = false;
        //拖拽超过边界最大比率(0~1.0)，当超过一定距离(距离=组建宽或高*拖拽比率)后停止拖拽
        //当该值为1的时候不检查拖拽范围，默认使用ListView自带的回弹
        [HideInInspector]
        [Range(0, 1)]
        public float maxDragOutOfFrontBoundsRatio = 1;
        [HideInInspector]
        [Range(0, 1)]
        public float maxDragOutOfBehindBoundsRatio = 1;
        //前置箭头，当滚动达到最前端的时候自动会隐藏
        [HideInInspector]
        public GameObject frontArrow;
        //后置箭头，当滚动达到最后端的时候自动会隐藏
        [HideInInspector]
        public GameObject behindArrow;

        //当所有组建准备自动刷新前的时候回调
        //[参数: 本次准备刷新的组建起始下标, 本次准备刷新的组建终止下标]
        //返回值:
        //true: 会自动开始刷新组建
        //false: 停止自动刷新所有组建
        public System.Func<int, int, bool> onItemsCanAutoUpdateCallBack = null;
        //当所有单个准备自动刷新前的时候回调
        //[参数: 准备添加的新组建下标]
        //返回值:
        //true: 会自动添加并刷新该组建
        //false: 停止自动刷新该组建和它之后的所有组建
        public System.Func<int, bool> onItemCanAutoUpdateCallBack = null;
        //当组件自动刷新的时候回调
        //[参数: 新的组建下标, 新的组建对象]
        public System.Action<int, GameObject> onItemAutoUpdateCallBack = null;
        //当所有组建自动刷新完毕的时候回调
        //[参数: 本次刷新成功的组建起始下标, 本次刷新成功的组建终止下标]
        public System.Action<int, int> onItemsDidAutoUpdateCallBack = null;
        //当组建拖拽超出边界需要回弹的时候回调
        //[参数：接近最大回弹临界值的百分比(范围0～1，当为1的时候则表示要回弹或者自动刷新数据)，滚动方向]
        public System.Action<float, shaco.Direction> onItemsDragOutOfBoundsCallBack = null;
        //当拖拽超出边界松开按钮时候回调
        //[参数：接近最大回弹临界值的百分比(范围0～1，当为1的时候则表示要回弹或者自动刷新数据)，滚动方向]
        public System.Action<float, shaco.Direction> onItemsDragOutOfBoundsEndCallBack = null;

        //组件模板
        [SerializeField]
        private GameObject _itemModel;

        [SerializeField]
        [HideInInspector]
        private List<Item> _listItems = new List<Item>();
        private List<GameObject> _listItemCache = new List<GameObject>();
        private bool _isUpdateListviewDirty = true;
        private bool _willResumeHorizontalScroll = false;
        private bool _willResumeVerticalScroll = false;
        private bool _isCustomPauseScrolling = false;
        private bool _isInited = false;
        private bool _isApplicationQuit = false;

        void Start()
        {
            CheckInit();
        }

        void Reset()
        {
            CheckCompoment();
        }

        void OnApplicationQuit()
        {
            _isApplicationQuit = true;
        }

        void OnDestroy()
        {
            if (_isApplicationQuit)
                return;
                
            //因为并没有对模板对象重新实例化赋值的
            //所以在这里没有必要控制销毁逻辑
            // if (_itemModel != null)
            // {
            //     UnityHelper.SafeDestroy(_itemModel);
            //     _itemModel = null;
            // }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!Application.isPlaying)
                OnPublicValueChangedCheck();
        }
#endif

        public void PauseScrolling()
        {
            PauseScrollingBase();
            _isCustomPauseScrolling = true;
        }

        public void ResumeScrolling()
        {
            _isCustomPauseScrolling = false;
            ResumeScrollingBase();
        }

        public void OnPublicValueChangedCheck()
        {
            if (_prevDirection != scrollDirection)
            {
                // groupItemDirection = shaco.Direction.Automatic;
                ChangeDirection(scrollDirection);
            }

            if (_prevfirstItemOffset != firstItemOffset)
            {
                SetUpdateListViewDirty();
                _prevfirstItemOffset = firstItemOffset;
            }

            if (_previtemMargin != itemMargin)
            {
                SetUpdateListViewDirty();
                _previtemMargin = itemMargin;
                _prevgroupItemMargin = groupItemMargin;
            }

            if (_prevopenDebugMode != openDebugMode)
            {
                CheckCompoment();
                _prevopenDebugMode = openDebugMode;
            }

            if (_prevgroupItemMargin != groupItemMargin)
            {
                if (eachOfGroup <= 1)
                {
                    _prevgroupItemMargin = Vector3.zero;
                    groupItemMargin = Vector3.zero;
                    Log.Error("set groupItemMargin error, eachOfGroup must > 1", this);
                }
                else
                {
                    SetUpdateListViewDirty();
                    _prevgroupItemMargin = groupItemMargin;
                }
            }

            if (_preveachOfGroup != eachOfGroup)
            {
                if (eachOfGroup < 1)
                {
                    Log.Error("set eachOfGroup error: value must > 0", this);
                    _preveachOfGroup = 1;
                    eachOfGroup = 1;
                }
                else
                {
                    SetUpdateListViewDirty();
                    _preveachOfGroup = eachOfGroup;
                }
            }

            if (_prevgroupItemDirection != groupItemDirection)
            {
                SetUpdateListViewDirty();
                _prevgroupItemDirection = groupItemDirection;
            }

            if (autoUpdateItemMinIndex != _prevAutoUpdateItemMinIndex || autoUpdateItemMaxIndex != _prevAutoUpdateItemMaxIndex)
            {
                CheckAutoUpdateItemWhenRangeChanged(_prevAutoUpdateItemMinIndex != autoUpdateItemMinIndex || autoUpdateItemMaxIndex > _prevAutoUpdateItemMaxIndex);
                _prevAutoUpdateItemMinIndex = autoUpdateItemMinIndex;
                _prevAutoUpdateItemMaxIndex = autoUpdateItemMaxIndex;
            }
        }

        private void PauseScrollingBase()
        {
            if (_isCustomPauseScrolling)
                return;

            _willResumeHorizontalScroll |= _scrollRectContent.horizontal;
            _willResumeVerticalScroll |= _scrollRectContent.vertical;

            if (_willResumeHorizontalScroll)
                _scrollRectContent.horizontal = false;
            if (_willResumeVerticalScroll)
                _scrollRectContent.vertical = false;
            _scrollRectContent.StopMovement();
        }

        private void ResumeScrollingBase()
        {
            if (_isCustomPauseScrolling)
                return;

            if (_willResumeHorizontalScroll)
            {
                _scrollRectContent.horizontal = true;
                _willResumeHorizontalScroll = false;
            }
            if (_willResumeVerticalScroll)
            {
                _scrollRectContent.vertical = true;
                _willResumeVerticalScroll = false;
            }
        }

        public void CheckCompoment()
        {
            if (null == _scrollRectContent)
            {
                var oldRectContnet = this.gameObject.GetComponent<ScrollRect>();
                if (null != oldRectContnet && oldRectContnet.GetType() != typeof(ScrollRectComponent))
                    Destroy(oldRectContnet);

                _scrollRectContent = this.gameObject.GetOrAddComponent<ScrollRectComponent>();
            }
            else
                _scrollRectContent = this.GetComponent<ScrollRectComponent>();

            if (null == _scrollRectContent.content)
            {
                //create scroll content automatic
                var contentNew = new GameObject();
                contentNew.name = "content";
                ListViewComponent.ChangeParentLocal(contentNew, this.gameObject);
                var transNew = contentNew.AddComponent<RectTransform>();
                transNew.sizeDelta = GetComponent<RectTransform>().sizeDelta;
                _scrollRectContent.content = transNew;
            }
            else
            {
                _scrollRectContent.onScrollingCallBack = OnScrollingCallBack;
                _scrollRectContent.onBeginDragCallBack = OnBeginDragCallBack;
                _scrollRectContent.onEndDragCallBack = OnEndDragCallBack;
            }

// #if UNITY_5_3_OR_NEWER
//             if (null == GetComponent<Mask>())
//             {
//                 gameObject.AddComponent<Mask>();
//             }
// #endif

            if (null == _rectTransform)
            {
                _rectTransform = this.GetComponent<RectTransform>();
            }

            if (openDebugMode)
            {
                if (_imageDebugContentDraw == null)
                {
                    var content = _scrollRectContent.content;
                    _imageDebugContentDraw = content.GetComponent<Image>();
                    if (_imageDebugContentDraw == null)
                    {
                        _imageDebugContentDraw = content.gameObject.AddComponent<Image>();
                        _imageDebugContentDraw.sprite = null;
                        _imageDebugContentDraw.color = new Color(1, 1, 1, 0.5f);
                    }
                    var rectTrans = _imageDebugContentDraw.GetComponent<RectTransform>();
                    rectTrans.sizeDelta = content.sizeDelta;
                }
                _imageDebugContentDraw.enabled = true;
            }
            else
            {
                if (_imageDebugContentDraw)
                {
                    _imageDebugContentDraw.color = new Color(1, 1, 1, 0);
                }
            }
        }

        public shaco.Direction GetScrollDirection()
        {
            return scrollDirection;
        }

        public shaco.Direction GetGroupDirection()
        {
            return groupItemDirection;
        }

        public void SetGroupDirection(shaco.Direction direction)
        {
            groupItemDirection = direction;

            if (direction == shaco.Direction.Automatic)
                UpdateGroupItemDirection();
        }

        public void RefreshAutoUpdateItems()
        {
            if (onItemAutoUpdateCallBack != null)
            {
                int startIndex = GetItemStartDisplayIndex();
                int count = _listItems.Count;

                for (int i = 0; i < count; ++i)
                    onItemAutoUpdateCallBack(i + startIndex, _listItems[i].current);
            }
        }

        private void Update()
        {
            OnPublicValueChangedCheck();
        }

        private void LateUpdate()
        {
            UpdateListView();
        }

        private void UpdateListView()
        {            
            if (_isUpdateListviewDirty)
            {
                CheckCompoment();
                if (!_listItems.IsNullOrEmpty())
                {
                    _isUpdateListviewDirty = false;

                    UpdateGroupItemDirection();
                    UpdateContentLayout();

                    for (int i = 0; i < _listItems.Count; ++i)
                    {
                        UpdateItem(i);
                    }

                    FixScrollContentPositionWhenEndDrag();

                    if (_isRevertOldPosDirty)
                    {
                        _isRevertOldPosDirty = false;
                        RevertToOldPos();
                    }
                }
                else
                {
                    //当没有数据时候仅仅刷新滚动区域大小
                    UpdateContentLayout();
                }
            }
        }
        
        private void CheckInit()
        {
            if (!_isInited)
            {
                CheckCompoment();
                if (_itemModel != null)
                {
                    SetItemModel(_itemModel);
                }

                _prevDirection = scrollDirection;
                _prevfirstItemOffset = firstItemOffset;
                _previtemMargin = itemMargin;
                _prevgroupItemMargin = groupItemMargin;
                _prevopenDebugMode = openDebugMode;
                _preveachOfGroup = eachOfGroup;
                _prevgroupItemDirection = groupItemDirection;
                _prevAutoUpdateItemMinIndex = autoUpdateItemMinIndex;
                _prevAutoUpdateItemMaxIndex = autoUpdateItemMaxIndex;
                _oldContentSize = new Vector3(_scrollRectContent.content.rect.width, _scrollRectContent.content.rect.height);

                CheckOutOfBoundsChanged();
                _isInited = true;
            }
            UpdateListView();
        }

        private void SetUpdateListViewDirty()
        {
#if UNITY_EDITOR
            _isUpdateListviewDirty = true;
            if (!Application.isPlaying)
                LateUpdate();
#else
            _isUpdateListviewDirty = true;
#endif
        }

        //static public void SortQuickly(List<int> listValue, int begin, int end)
        //{
        //    if (begin >= end)
        //        return;

        //    int left = begin;
        //    int right = end;
        //    int key = listValue[begin];

        //    while (left < right)
        //    {
        //        while (left < right && key <= listValue[right]) --right;

        //        listValue[left] = listValue[right];

        //        while (left < right && key >= listValue[left]) ++left;

        //        listValue[right] = listValue[left];
        //    }

        //    listValue[left] = key;
        //    SortQuickly(listValue, begin, left - 1);
        //    SortQuickly(listValue, left + 1, end);
        //}
    }
}

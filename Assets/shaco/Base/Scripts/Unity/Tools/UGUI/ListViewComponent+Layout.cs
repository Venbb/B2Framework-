using UnityEngine;
using System.Collections;

namespace shaco
{
    public partial class ListViewComponent
    {
        private bool _isOutOfFrontBounds = false;
        private bool _isOutOfBehindBounds = false;
        private shaco.Direction _prevDirection = shaco.Direction.Right;
        private Vector3 _prevfirstItemOffset = Vector3.zero;
        private Vector3 _previtemMargin = Vector3.zero;
        private Vector3 _prevgroupItemMargin = Vector3.zero;
        private int _prevAutoUpdateItemMinIndex = -1;
        private int _prevAutoUpdateItemMaxIndex = -1;
        private bool _prevopenDebugMode = false;
        private int _preveachOfGroup = 0;
        private shaco.Direction _prevgroupItemDirection = shaco.Direction.Automatic;
        private Vector3 _oldContentSize = Vector3.zero;
        private UnityEngine.UI.Image _imageDebugContentDraw;
        private Vector3 _currentGroupItemPivotPrev = Vector3.zero;
        private Vector3 _currentGroupItemPivotNext = Vector3.zero;
        private Vector3 _fixContentOffsetWhenEndGrag = Vector3.zero;
        private Item _lastItemWhenEndGrag = null;
        private Item _firstItemWhenEndGrag = null;
        private int _itemIndexOffsetUseInSpringbackCallBack = 0;
        private ScrollRectComponent _scrollRectContent = null;
        private float _currentMaxDragOutOfFrontBoundsRatio = 0;
        private float _currentMaxDragOutOfBehindBoundsRatio = 0;
        private RectTransform _rectTransform = null;
        private Vector2 _recordOldPos;
        private bool _isRevertOldPosDirty = false;

        #region 检查超出边界参数
        //当前是否在即将到达边缘区域内，当离开此区域后才能再次调用自动刷新组件方法
        private bool _isOutOfBounds = false;
        private shaco.Direction _springBackDirection = shaco.Direction.Automatic;
        private shaco.Direction _currentScrollDirection = shaco.Direction.Automatic;
        private float _outOfBoundsRate = 0;
        private Vector3 _newContentWorldPosition = Vector3.zero;
        private Vector3 _newContentPivot = Vector3.zero;
        private Vector3 _prevTouchDelta = Vector3.zero;
        private Vector3 _negativeCheckTouchDelta = Vector3.zero;
        private Vector3 _contentOffsetPosition = Vector3.zero;
        #endregion

        public void ChangeDirection(shaco.Direction dir)
        {
            if (dir == shaco.Direction.Automatic)
            {
                scrollDirection = _prevDirection;
                Log.Error("ListView ChangeDirection error: unsupport direction=" + dir);
                return;
            }
            scrollDirection = dir;
            _prevDirection = dir;
            SetUpdateListViewDirty();
        }

        public void ChangeGroupItemDirection(shaco.Direction dir)
        {
            groupItemDirection = dir;
            SetUpdateListViewDirty();
        }

        public void UpdateCenterLayout()
        {
            SetUpdateListViewDirty();
        }

        public CenterLayoutType GetCenterLayoutType()
        {
            CenterLayoutType retValue = CenterLayoutType.NoCenter;

            if (!_scrollRectContent.horizontal && _scrollRectContent.vertical)
            {
                retValue = CenterLayoutType.CenterHorizontalOnly;
            }
            else if (_scrollRectContent.horizontal && !_scrollRectContent.vertical)
            {
                retValue = CenterLayoutType.CenterVerticalOnly;
            }
            else if (!_scrollRectContent.horizontal && !_scrollRectContent.vertical)
            {
                retValue = CenterLayoutType.Center;
            }

            return retValue;
        }

        /// <summary>
        /// 记录当前滚动位置，以便在刷新布局完成后保持当前滚动位置
        /// </summary>
        public void RecordOldScrollPosition()
        {
            if (_scrollRectContent != null)
            {
                _recordOldPos = _scrollRectContent.normalizedPosition;
            }
            else
            {
                _recordOldPos = Vector2.one;
            }
            _isRevertOldPosDirty = true;
        }

        /// <summary>
        /// 检查当自动刷新组件范围变化时候，并自动重新设置组件位置
        /// </summary>
        private void CheckAutoUpdateItemWhenRangeChanged(bool isNegative)
        {
            // if (_listItems.IsNullOrEmpty())
            //     return;

            // var oldPositions = new System.Collections.Generic.List<Vector3>();
            // for (int i = 0; i < _listItems.Count; ++i)
            // {
            //     oldPositions.Add(_listItems[i].current.transform.position);
            // }

            // var oldSize = _scrollRectContent.content.sizeDelta;
            // _scrollRectContent.content.sizeDelta = GetAllSizeItem();
            // var newSize = _scrollRectContent.content.sizeDelta;
            // if (!isNegative)
            // {
            //     _scrollRectContent.content.localPosition += new Vector3(newSize.x - oldSize.x, newSize.y - oldSize.y);
            // }

            // for (int i = 0; i < _listItems.Count; ++i)
            // {
            //     _listItems[i].current.transform.position = oldPositions[i];
            // }

            //范围刷新后还是重新刷新一次list表更加安全，否则会有更多问题需要处理
            _isUpdateListviewDirty = true;
        }

        /// <summary>
        /// 获取组件在显示一组数组中的下标
        /// </summary>
        private int GetItemDisplayIndexInGroup(Item target)
        {
            int retValue = -1;
            if (_listItems.IsNullOrEmpty() && _itemModel == null)
                return retValue;

            var itemModelTmp = null == _itemModel ? _listItems[0].current : _itemModel;
            float floatCount = 0;
            if (null != itemModelTmp)
            {
                var modelSizeTmp = itemModelTmp.GetComponent<RectTransform>().sizeDelta;
                switch (groupItemDirection)
                {
                    case shaco.Direction.Right:
                        {
                            floatCount = ((UnityHelper.GetLocalPositionByPivot(target.current, shaco.Pivot.LowerLeft).x - firstItemOffset.x) / (modelSizeTmp.x + groupItemMargin.x));
                            break;
                        }
                    case shaco.Direction.Left:
                        {
                            floatCount = ((UnityHelper.GetLocalPositionByPivot(target.current, shaco.Pivot.LowerRight).x - firstItemOffset.x) / (modelSizeTmp.x - groupItemMargin.x));
                            break;
                        }
                    case shaco.Direction.Up:
                        {
                            floatCount = ((UnityHelper.GetLocalPositionByPivot(target.current, shaco.Pivot.LowerLeft).y - firstItemOffset.y) / (modelSizeTmp.y + groupItemMargin.y));
                            break;
                        }
                    case shaco.Direction.Down:
                        {
                            floatCount = ((UnityHelper.GetLocalPositionByPivot(target.current, shaco.Pivot.UpperLeft).y + firstItemOffset.y) / (modelSizeTmp.y - groupItemMargin.y));
                            break;
                        }
                    default: Log.Error("ListView+Layout GetItemIndexInGroup error: unsupport direction"); break;
                }
                retValue = Mathf.FloorToInt(floatCount + 0.5f);
            }

            if (retValue < 0)
                retValue = -retValue;

            return retValue;
        }

        private void MoveItemToTargetSide(int moveItemIndex, int targetItemIndex, bool isNegative)
        {
            if (!_isInited && Application.isPlaying)
                return;

            if (_listItems.IsOutOfRange(moveItemIndex))
            {
                _listItems.DebugLogOutOfRange(moveItemIndex, "ListView+Layout MoveItemToTargetSide error:");
                return;
            }

            if (groupItemDirection == Direction.Automatic)
                return;

            //处理多行多列数据换行问题
            bool shouldReturn = false;
            var targetItem = _listItems.IsOutOfRange(targetItemIndex) ? null : _listItems[targetItemIndex];

            if (this.eachOfGroup > 1 && null != targetItem)
            {
                int rowIndexWithSize = GetItemDisplayIndexInGroup(targetItem);
                if (rowIndexWithSize != -1)
                {
                    if ((!isNegative && rowIndexWithSize == this.eachOfGroup - 1))
                    {
                        shouldReturn = true;
                        for (int i = this.eachOfGroup - 2; i >= 0; --i)
                        {
                            targetItem = targetItem.prev;
                        }
                    }
                    else if ((isNegative && rowIndexWithSize == 0))
                    {
                        shouldReturn = true;
                        for (int i = this.eachOfGroup - 2; i >= 0; --i)
                        {
                            targetItem = targetItem.next;
                        }
                    }
                }
            }

            var moveItem = _listItems[moveItemIndex];

            //calculate position
            Vector3 pivotDecide = Vector3.zero;
            Vector3 pivotAdd = Vector3.zero;

            switch (scrollDirection)
            {
                case shaco.Direction.Right:
                    {
                        if (isNegative)
                        {
                            pivotDecide = shaco.Pivot.LowerRight;
                            pivotAdd = shaco.Pivot.LowerLeft;
                        }
                        else
                        {
                            pivotDecide = shaco.Pivot.LowerLeft;
                            pivotAdd = shaco.Pivot.LowerRight;
                        }
                        break;
                    }
                case shaco.Direction.Left:
                    {
                        if (isNegative)
                        {
                            pivotDecide = shaco.Pivot.LowerLeft;
                            pivotAdd = shaco.Pivot.LowerRight;
                        }
                        else
                        {
                            pivotDecide = shaco.Pivot.LowerRight;
                            pivotAdd = shaco.Pivot.LowerLeft;
                        }
                        break;
                    }
                case shaco.Direction.Up:
                    {
                        if (isNegative)
                        {
                            pivotDecide = shaco.Pivot.UpperLeft;
                            pivotAdd = shaco.Pivot.LowerLeft;
                        }
                        else
                        {
                            pivotDecide = shaco.Pivot.LowerLeft;
                            pivotAdd = shaco.Pivot.UpperLeft;
                        }
                        break;
                    }
                case shaco.Direction.Down:
                    {
                        if (isNegative)
                        {
                            pivotDecide = shaco.Pivot.LowerLeft;
                            pivotAdd = shaco.Pivot.UpperLeft;
                        }
                        else
                        {
                            pivotDecide = shaco.Pivot.UpperLeft;
                            pivotAdd = shaco.Pivot.LowerLeft;
                        }
                        break;
                    }
                default: Log.Error("ListView+Layout MoveItemToTargetSide erorr: unsupport direction" + scrollDirection); break;
            }

            if (null == targetItem)
            {
                RectTransform content = _scrollRectContent.content;

                if (scrollDirection == shaco.Direction.Left)
                    UnityHelper.SetLocalPositionByPivot(moveItem.current, new Vector3(content.rect.width - _contentOffsetPosition.x, 0), pivotDecide);
                else if (scrollDirection == shaco.Direction.Down)
                    UnityHelper.SetLocalPositionByPivot(moveItem.current, new Vector3(0, content.rect.height - _contentOffsetPosition.y), pivotDecide);
                else if (scrollDirection == shaco.Direction.Right)
                    UnityHelper.SetLocalPositionByPivot(moveItem.current, new Vector3(-_contentOffsetPosition.x, 0), pivotDecide);
                else if (scrollDirection == shaco.Direction.Up)
                    UnityHelper.SetLocalPositionByPivot(moveItem.current, new Vector3(0, -_contentOffsetPosition.y), pivotDecide);

                FixedLocalPositionByContent(moveItem.current);
            }
            else
            {
                if (shouldReturn || this.eachOfGroup <= 1)
                {
                    Vector3 prevPosition = UnityHelper.GetLocalPositionByPivot(targetItem.current, pivotAdd);
                    UnityHelper.SetLocalPositionByPivot(moveItem.current, prevPosition, pivotDecide);
                    FixeditemMargin(moveItem.current, isNegative ? -itemMargin : itemMargin);
                }
                else
                {
                    //next item of group
                    GameObject behindItem = null;
                    var pivotAddTmp = _currentGroupItemPivotPrev;
                    var pivotDecideTmp = _currentGroupItemPivotNext;
                    if (isNegative)
                    {
                        behindItem = moveItem.next.current;
                        switch (groupItemDirection)
                        {
                            case shaco.Direction.Right:
                            case shaco.Direction.Left:
                                {
                                    pivotAddTmp.x = pivotAddTmp.x == 0 ? 1 : 0;
                                    pivotDecideTmp.x = pivotDecideTmp.x == 0 ? 1 : 0;
                                    break;
                                }
                            case shaco.Direction.Up:
                            case shaco.Direction.Down:
                                {
                                    pivotAddTmp.y = pivotAddTmp.y == 0 ? 1 : 0;
                                    pivotDecideTmp.y = pivotDecideTmp.y == 0 ? 1 : 0;
                                    break;
                                }
                            default: Log.Error("unsupport direction"); break;
                        }
                    }
                    else
                    {
                        behindItem = moveItem.prev.current;
                    }
                    Vector3 prevPosition = UnityHelper.GetLocalPositionByPivot(behindItem, pivotAddTmp);
                    UnityHelper.SetLocalPositionByPivot(moveItem.current, prevPosition, pivotDecideTmp);
                    FixeditemMargin(moveItem.current, isNegative ? -groupItemMargin : groupItemMargin);
                }
            }
        }

        /// <summary>
        /// 获取组件下标在满的一行数据内的，因为下一行数据可能不足一行了
        /// 返回-1表示目标行数据是满的
        /// </summary>
        private int GetItemIndexInFullRow(bool isNegative)
        {
            int retValue = -1;
            if (autoUpdateItemCountWhenTiming <= 0 || _listItems.Count == 0 || this.eachOfGroup <= 1 || !_isInited)
                return retValue;

            var itemModelTmp = null == _itemModel ? _listItems[0].current : _itemModel;

            if (null == itemModelTmp)
                return retValue;

            var modelSizeTmp = itemModelTmp.GetComponent<RectTransform>().sizeDelta;
            Item targetItem = _listItems[isNegative ? _listItems.Count - 1 : 0];
            int rowIndexWithSize = GetItemDisplayIndexInGroup(targetItem);
            if (isNegative)
            {
                if (rowIndexWithSize != this.eachOfGroup - 1)
                {
                    retValue = _listItems.Count - this.eachOfGroup - 1;
                }
            }
            else
            {
                if (rowIndexWithSize != 0)
                {
                    retValue = this.eachOfGroup;
                }
            }
            return retValue;
        }

        private void UpdateItem(int index)
        {
            // if (!_listItems.IsOutOfRange(index))
            // {
            //     var item = _listItems[index];
            //     item.current.SetActive(true);
            // }
            MoveItemToTargetSide(index, index - 1, false);
            // if (index < 0 || index > _listItems.Count - 1)
            // {
            //     Log.Error("ListView UpdateItem error: out of range");
            //     return;
            // }
            // var item = _listItems[index];
            // RectTransform content = _scrollRectContent.content;

            // item.current.SetActive(true);

            // //calculate position
            // Vector3 pivotDecide = Vector3.zero;
            // Vector3 pivotAdd = Vector3.zero;

            // switch (scrollDirection)
            // {
            //     case shaco.Direction.Right: pivotDecide = shaco.Pivot.LeftBottom; pivotAdd = shaco.Pivot.RightBottom; break;
            //     case shaco.Direction.Left: pivotDecide = shaco.Pivot.RightBottom; pivotAdd = shaco.Pivot.LeftBottom; break;
            //     case shaco.Direction.Up: pivotDecide = shaco.Pivot.LeftBottom; pivotAdd = shaco.Pivot.LeftTop; break;
            //     case shaco.Direction.Down: pivotDecide = shaco.Pivot.LeftTop; pivotAdd = shaco.Pivot.LeftBottom; break;
            //     default: Log.Error("unsupport direction"); break;
            // }

            // if (index == 0)
            // {
            //     if (scrollDirection == shaco.Direction.Left)
            //         UnityHelper.SetLocalPositionByPivot(item.current, new Vector3(content.rect.width, 0), pivotDecide);
            //     else if (scrollDirection == shaco.Direction.Down)
            //         UnityHelper.SetLocalPositionByPivot(item.current, new Vector3(0, content.rect.height), pivotDecide);
            //     else
            //     {
            //         UnityHelper.SetLocalPositionByPivot(item.current, Vector3.zero, pivotDecide);
            //     }
            //     FixedLocalPositionByContent(item.current);
            // }
            // else
            // {
            //     //first item of group
            //     if (index % eachOfGroup == 0)
            //     {
            //         var prevItem = GetItem((index / eachOfGroup - 1) * eachOfGroup);
            //         Vector3 prevPosition = UnityHelper.GetLocalPositionByPivot(prevItem, pivotAdd);
            //         UnityHelper.SetLocalPositionByPivot(item.current, prevPosition, pivotDecide);

            //         FixeditemMargin(item.current, itemMargin);
            //     }
            //     else
            //     {
            //         //next item of group
            //         Vector3 prevPosition = UnityHelper.GetLocalPositionByPivot(item.prev.current, _currentGroupItemPivotPrev);
            //         UnityHelper.SetLocalPositionByPivot(item.current, prevPosition, _currentGroupItemPivotNext);

            //         FixeditemMargin(item.current, groupItemMargin);
            //     }
            // }
        }

        private void UpdateGroupItemDirection()
        {
            if (groupItemDirection == shaco.Direction.Automatic)
            {
                switch (scrollDirection)
                {
                    case shaco.Direction.Right: groupItemDirection = shaco.Direction.Down; break;
                    case shaco.Direction.Left: groupItemDirection = shaco.Direction.Down; break;
                    case shaco.Direction.Up: groupItemDirection = shaco.Direction.Right; break;
                    case shaco.Direction.Down: groupItemDirection = shaco.Direction.Right; break;
                    default: groupItemDirection = shaco.Direction.Down; Log.Error("unsupport direction"); break;
                }
                UpdateGroupItemDirection();
            }
            else
            {
                switch (groupItemDirection)
                {
                    case shaco.Direction.Right: _currentGroupItemPivotPrev = shaco.Pivot.LowerRight; _currentGroupItemPivotNext = shaco.Pivot.LowerLeft; break;
                    case shaco.Direction.Left: _currentGroupItemPivotPrev = shaco.Pivot.LowerLeft; _currentGroupItemPivotNext = shaco.Pivot.LowerRight; break;
                    case shaco.Direction.Up: _currentGroupItemPivotPrev = shaco.Pivot.UpperLeft; _currentGroupItemPivotNext = shaco.Pivot.LowerLeft; break;
                    case shaco.Direction.Down: _currentGroupItemPivotPrev = shaco.Pivot.LowerLeft; _currentGroupItemPivotNext = shaco.Pivot.UpperLeft; break;
                    default: Log.Error("unsupport direction"); break;
                }
            }
        }

        private void UpdateContentLayout()
        {
            if (_listItems.IsNullOrEmpty() && autoUpdateItemTiming != AutoUpdateItemTiming.WhenScrolling)
                return;

            var content = _scrollRectContent.content;
            var allItemSize = GetAllSizeItem();

            float widthNew = content.rect.width;
            float heightNew = content.rect.height;
            Vector3 pivotContent = GetContentPivot();
            var itemModelTmp = null == _itemModel && _listItems.Count > 0 ? _listItems[0].current : _itemModel;
            if (null == itemModelTmp)
                return;

            var rectSizeTmp = itemModelTmp.GetComponent<RectTransform>().rect.size;

            _contentOffsetPosition = Vector2.zero;

            //当滚动刷新模式时候，content会流出很大空白以计算scrollbar，所以当有组件数量不足刚好铺满的时候，要适当偏移下位置
            if (AutoUpdateItemTiming.WhenScrolling == autoUpdateItemTiming && autoUpdateItemCountWhenTiming > 0)
            {
                int remainFrontStartItemCount, remainFrontEndItemCount, remainBehindStartItemCount, remainBehindEndItemCount;
                GetStartAndEndRemainItemCount(out remainFrontStartItemCount, out remainFrontEndItemCount, out remainBehindStartItemCount, out remainBehindEndItemCount);

                bool fixedFront = remainFrontStartItemCount != this.eachOfGroup;
                int fixedItemCount = System.Math.Abs(autoUpdateItemMinIndex) / this.eachOfGroup;
                if (fixedFront)
                    ++fixedItemCount;
                _contentOffsetPosition -= new Vector3(rectSizeTmp.x, rectSizeTmp.y) * fixedItemCount;
                var startOffset = _itemIndexOffsetUseInSpringbackCallBack / this.eachOfGroup * rectSizeTmp;
                _contentOffsetPosition -= new Vector3(startOffset.x, startOffset.y);
            }

            switch (scrollDirection)
            {
                case shaco.Direction.Right:
                    {
                        widthNew = allItemSize.x + firstItemOffset.x;
                        heightNew = GetMaxHeightItem();
                        _contentOffsetPosition.y = 0;
                        break;
                    }
                case shaco.Direction.Left:
                    {
                        widthNew = allItemSize.x + firstItemOffset.x;
                        heightNew = GetMaxHeightItem();
                        _contentOffsetPosition = -_contentOffsetPosition;
                        _contentOffsetPosition.y = 0;
                        break;
                    }
                case shaco.Direction.Up:
                    {
                        widthNew = GetMaxWidthItem();
                        heightNew = allItemSize.y + firstItemOffset.y;
                        _contentOffsetPosition.x = 0;
                        break;
                    }
                case shaco.Direction.Down:
                    {
                        widthNew = GetMaxWidthItem();
                        heightNew = allItemSize.y + firstItemOffset.y;
                        _contentOffsetPosition = -_contentOffsetPosition;
                        _contentOffsetPosition.x = 0;
                        break;
                    }
                default: Log.Error("updateContentSize error: unsupport direction !"); break;
            }

            content.pivot = pivotContent;
            content.sizeDelta = new Vector3(widthNew, heightNew);

            if (isCenterLayout)
            {
                pivotContent = CheckContentPivotWhenCenterLayout(pivotContent);
            }

            var pos1 = UnityHelper.GetLocalPositionByPivot(this.gameObject, pivotContent);
            UnityHelper.SetLocalPositionByPivot(content.gameObject, pos1, pivotContent);
            content.transform.localPosition -= this.transform.localPosition;

            //当自动刷新下标为负数的时候需要移动content位置
            content.transform.localPosition += _contentOffsetPosition;

            CheckCompoment();

            //刷新界面布局后禁止一次自动滚动
            _scrollRectContent.StopMovement();
        }

        private Vector3 GetContentPivot()
        {
            Vector3 retValue = shaco.Pivot.UpperLeft;
            if (groupItemDirection == shaco.Direction.Automatic)
                UpdateGroupItemDirection();

            switch (groupItemDirection)
            {
                case shaco.Direction.Right:
                    {
                        retValue = scrollDirection == shaco.Direction.Up ? shaco.Pivot.LowerLeft : shaco.Pivot.UpperLeft;
                        break;
                    }
                case shaco.Direction.Left:
                    {
                        retValue = scrollDirection == shaco.Direction.Down ? shaco.Pivot.UpperRight : shaco.Pivot.LowerRight;
                        break;
                    }
                case shaco.Direction.Up:
                    {
                        retValue = scrollDirection == shaco.Direction.Left ? shaco.Pivot.LowerRight : shaco.Pivot.LowerLeft;
                        break;
                    }
                case shaco.Direction.Down:
                    {
                        retValue = scrollDirection == shaco.Direction.Left ? shaco.Pivot.UpperRight : shaco.Pivot.UpperLeft;
                        break;
                    }
                default: Log.Error("ListView GetContentPivot error: unsupport direction=" + groupItemDirection); break;
            }
            return retValue;
        }

        private float GetMaxWidthItem()
        {
            float ret = 0;
            for (int i = 0; i < _listItems.Count; ++i)
            {
                var rectTrans = _listItems[i].current.transform.GetComponent<RectTransform>();
                if (ret < rectTrans.rect.width)
                    ret = rectTrans.rect.width;
            }
            return ret;
        }

        private float GetMaxHeightItem()
        {
            float ret = 0;
            for (int i = 0; i < _listItems.Count; ++i)
            {
                var rectTrans = _listItems[i].current.transform.GetComponent<RectTransform>();
                if (ret < rectTrans.rect.height)
                    ret = rectTrans.rect.height;
            }
            return ret;
        }

        /// <summary>
        /// 获取列表滚动到最前方和最后方时候多出的组件数量
        /// 例如滚动到最前方的时候是这个样子：
        /// 口口
        /// 口口口
        /// 口
        /// 那么头剩余2个，尾剩余1个
        /// </summary>
        private void GetStartAndEndRemainItemCount(out int remainFrontStartItemCount, out int remainFrontEndItemCount, out int remainBehindStartItemCount, out int remainBehindEndItemCount)
        {
            int autoMinIndexTmp = 0 + System.Math.Abs(autoUpdateItemMinIndex);
            remainFrontStartItemCount = autoMinIndexTmp;
            remainFrontStartItemCount = remainFrontStartItemCount % this.eachOfGroup;
            remainFrontEndItemCount = (_listItems.Count - remainFrontStartItemCount) % this.eachOfGroup;

            if (remainFrontStartItemCount == 0) remainFrontStartItemCount = this.eachOfGroup;
            if (remainFrontEndItemCount == 0) remainFrontEndItemCount = this.eachOfGroup;

            // Debug.Log("front start=" + remainFrontStartItemCount + " end=" + remainFrontEndItemCount);

            remainBehindEndItemCount = System.Math.Abs(autoUpdateItemMaxIndex) - 0 + 1;
            remainBehindEndItemCount = remainBehindEndItemCount % this.eachOfGroup;
            remainBehindStartItemCount = (_listItems.Count - remainBehindEndItemCount) % this.eachOfGroup;

            if (remainBehindStartItemCount == 0) remainBehindStartItemCount = this.eachOfGroup;
            if (remainBehindEndItemCount == 0) remainBehindEndItemCount = this.eachOfGroup;

            // Debug.Log("behind start=" + remainBehindStartItemCount + " end=" + remainBehindEndItemCount);
        }

        private Vector3 GetAllSizeItem()
        {
            var retValue = Vector3.zero;

            if (_listItems.IsNullOrEmpty())
                return retValue;

            if (autoUpdateItemCountWhenTiming > 0 && autoUpdateItemTiming == AutoUpdateItemTiming.WhenScrolling)
            {
                var itemSizeTmp = 0;
                if (autoUpdateItemCountWhenTiming > 0)
                {
                    int remainFrontStartItemCount, remainFrontEndItemCount, remainBehindStartItemCount, remainBehindEndItemCount;
                    GetStartAndEndRemainItemCount(out remainFrontStartItemCount, out remainFrontEndItemCount, out remainBehindStartItemCount, out remainBehindEndItemCount);

                    bool fixedFront = remainFrontStartItemCount != this.eachOfGroup && remainBehindStartItemCount != this.eachOfGroup;
                    bool fixedBehind = remainFrontEndItemCount != this.eachOfGroup && remainBehindEndItemCount != this.eachOfGroup;

                    //当有组件数量不足刚好铺满的时候，要适当偏移下位置
                    int addCount = 0;
                    if (fixedFront) ++addCount;
                    if (fixedBehind) ++addCount;

                    if (0 == addCount && (remainFrontStartItemCount != this.eachOfGroup || remainBehindEndItemCount != this.eachOfGroup))
                        ++addCount;

                    itemSizeTmp = autoUpdateItemMaxIndex - autoUpdateItemMinIndex + 1;
                    itemSizeTmp /= this.eachOfGroup;
                    itemSizeTmp += addCount;
                }
                else
                    itemSizeTmp = _listItems.Count;

                if (itemSizeTmp == 0)
                    return retValue;

                var itemModelTmp = null == _itemModel ? _listItems[0].current : _itemModel;
                var rectSizeTmp = itemModelTmp.GetComponent<RectTransform>().rect.size;
                var itemModelSizeTmp = new Vector3(rectSizeTmp.x, rectSizeTmp.y, 0);
                var totalContentSize = itemSizeTmp * itemModelSizeTmp - (itemSizeTmp - 1) * itemMargin;

                switch (scrollDirection)
                {
                    case shaco.Direction.Right:
                    case shaco.Direction.Left: retValue = new Vector2(totalContentSize.x, rectSizeTmp.y); break;
                    case shaco.Direction.Up:
                    case shaco.Direction.Down: retValue = new Vector2(rectSizeTmp.x, totalContentSize.y); break;
                    default: Log.Error("ListView+Layout GetContentSize error: unsupport direction" + scrollDirection); break;
                }
            }
            else
            {
                for (int i = 0; i < _listItems.Count; i += eachOfGroup)
                {
                    var rectTrans = _listItems[i].current.transform.GetComponent<RectTransform>();
                    retValue += new Vector3(rectTrans.rect.width, rectTrans.rect.height);
                }

                var fixeditemMargin = new Vector3(itemMargin.x, itemMargin.y, itemMargin.z);
                if (!isCenterLayout)
                {
                    bool isNegative = scrollDirection == shaco.Direction.Right || scrollDirection == shaco.Direction.Up;
                    if (isNegative)
                    {
                        if (_scrollRectContent.horizontal)
                        {
                            fixeditemMargin.x = -fixeditemMargin.x;
                        }
                        if (_scrollRectContent.vertical)
                        {
                            fixeditemMargin.y = -fixeditemMargin.y;
                        }
                    }
                }
                retValue -= ((_listItems.Count - 1) / eachOfGroup) * fixeditemMargin;
            }
            return retValue;
        }

        private void FixedLocalPositionByContent(GameObject target)
        {
            var contentTmp = _scrollRectContent.content;

            var rectTrans = _scrollRectContent.content.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Log.Error("FixedLocalPositionByContent error: dose not contain RectTransform !");
                return;
            }

            var rectTransTarget = target.GetComponent<RectTransform>();
            if (rectTransTarget == null)
            {
                Log.Error("setLocalPositionByArchor error: target dose not contain RectTransform !");
                return;
            }

            target.transform.localPosition -= new Vector3(contentTmp.pivot.x * contentTmp.rect.width, contentTmp.pivot.y * contentTmp.rect.height);

            if (firstItemOffset.x != 0 || firstItemOffset.y != 0)
            {
                target.transform.localPosition += new Vector3(firstItemOffset.x, firstItemOffset.y);
            }
        }

        private void FixeditemMargin(GameObject target, Vector3 margin)
        {
            target.transform.localPosition += margin;
        }

        private void OnScrollingCallBack(UnityEngine.EventSystems.PointerEventData data)
        {
            CheckDragOutOfBounds();
            CheckOutOfBoundsChanged();
        }

        private void OnBeginDragCallBack(UnityEngine.EventSystems.PointerEventData eventData)
        {

        }

        private void OnEndDragCallBack(UnityEngine.EventSystems.PointerEventData eventData)
        {
            //如果当前滚动速度超过了可视区域大小，则看作是快速滚动并非拖拽结束
            bool isVeryFastScrolling = this._scrollRectContent.IsVeryFastScrolling(this._currentScrollDirection);
            if (isVeryFastScrolling)
                return;

            if (null != onItemsDragOutOfBoundsEndCallBack)
                onItemsDragOutOfBoundsEndCallBack(_outOfBoundsRate, _currentScrollDirection);

            if (null == onItemAutoUpdateCallBack || autoUpdateItemCountWhenTiming <= 0 || _listItems.Count == 0)
                return;

            if (autoUpdateItemTiming == AutoUpdateItemTiming.WillDragOutOfBounds)
            {
                if (_isOutOfBounds)
                {
                    //如果有组件自动更新应该马上刷新界面布局，否则会导致滚动窗口因为ContentSize大小突然变大而回弹一下
                    //Unity5.6.5p3没有这个问题，但是在Unity5.6.4p4有问题，为了统一处理则刷新一次布局
                    if (CheckAutoUpdateItems(_springBackDirection) > 0)
                    {
                        UpdateListView();
                    }
                }
            }
        }

        private int CheckAutoUpdateItems(shaco.Direction springBackDirection)
        {
            if (null == onItemAutoUpdateCallBack || autoUpdateItemCountWhenTiming <= 0 || _listItems.Count == 0)
                return 0;

            if (springBackDirection == shaco.Direction.Up || springBackDirection == shaco.Direction.Left)
            {
                int startIndex = _itemIndexOffsetUseInSpringbackCallBack + _listItems.Count;
                int endIndex = startIndex + autoUpdateItemCountWhenTiming - 1;
                return CheckAutoItemSize(endIndex);
            }
            else if (springBackDirection == shaco.Direction.Down || springBackDirection == shaco.Direction.Right)
            {
                int startIndex = _itemIndexOffsetUseInSpringbackCallBack - 1;
                int endIndex = startIndex - autoUpdateItemCountWhenTiming + 1;
                return CheckAutoItemSize(endIndex);
            }
            else
                return 0;
        }

        private GameObject GetFrontItem()
        {
            GameObject retValue = null;
            switch (scrollDirection)
            {
                case shaco.Direction.Right:
                case shaco.Direction.Up: retValue = GetFirstItem(); break;
                case shaco.Direction.Left:
                case shaco.Direction.Down: retValue = GetLastItem(); break;
                default: Log.Error("unsupport direction"); break;
            }
            return retValue;
        }

        private GameObject GetBehindItem()
        {
            GameObject retValue = null;
            switch (scrollDirection)
            {
                case shaco.Direction.Right:
                case shaco.Direction.Up: retValue = GetLastItem(); break;
                case shaco.Direction.Left:
                case shaco.Direction.Down: retValue = GetFirstItem(); break;
                default: Log.Error("unsupport direction"); break;
            }
            return retValue;
        }

        private Vector3 GetTouchDelta()
        {
            if (null == _scrollRectContent.lastDragData)
                return Vector3.zero;
            else if (_scrollRectContent.lastDragData.delta.x != 0 || _scrollRectContent.lastDragData.delta.y != 0)
            {
                return _scrollRectContent.lastDragData.delta;
            }
            else
            {
                return new Vector3(_scrollRectContent.lastDragData.scrollDelta.x, -_scrollRectContent.lastDragData.scrollDelta.y);
            }
            // #if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
            //             //拖拽屏幕
            //             return Input.touchCount > 0 ? Input.GetTouch(0).deltaPosition : Vector2.zero;
            // #else
            //             var scrollDelta = Input.mouseScrollDelta;
            //             var currentMousePosition = Input.mousePosition;

            //             //滚轮滚动
            //             if (scrollDelta.x != 0 && scrollDelta.y != 0)
            //             {
            //                 return -scrollDelta;
            //             }
            //             //拖拽鼠标
            //             else
            //             {
            //                 if (Input.GetKey(KeyCode.Mouse0))
            //                 {
            //                     var touchDelta = currentMousePosition - _prevMousePosition;
            //                     _prevMousePosition = currentMousePosition;
            //                     return touchDelta;
            //                 }
            //                 else
            //                     return Vector3.zero;
            //             }
            // #endif
        }

        private void CheckDragOutOfBounds()
        {
            if (_listItems.Count == 0)
                return;

            var currentTouchDelta = GetTouchDelta();
            var rectTrans = _scrollRectContent.content;
            float dragOffset = 0;
            float maxBoundsOffset = 0;
            bool isNegative = scrollDirection == shaco.Direction.Left || scrollDirection == shaco.Direction.Down;
            bool isHorizontol = scrollDirection == shaco.Direction.Left || scrollDirection == shaco.Direction.Right;
            bool isVertical = scrollDirection == shaco.Direction.Down || scrollDirection == shaco.Direction.Up;
            Vector3 pivotTmp = Vector3.zero;

            //没有发生拖拽事件
            if ((isHorizontol && currentTouchDelta.x == 0) || (isVertical && currentTouchDelta.y == 0))
                return;

            //当发生反向拖拽的时候，必须反向拖拽距离超过一个组件大小才会生效
            if (isHorizontol)
            {
                if ((_prevTouchDelta.x > 0 && currentTouchDelta.x < 0) || (_prevTouchDelta.x < 0 && currentTouchDelta.x > 0))
                {
                    _negativeCheckTouchDelta = currentTouchDelta * 2;
                }
            }
            else if (isVertical)
            {
                if ((_prevTouchDelta.y > 0 && currentTouchDelta.y < 0) || (_prevTouchDelta.y < 0 && currentTouchDelta.y > 0))
                {
                    _negativeCheckTouchDelta = currentTouchDelta * 2;
                }
            }

            //记录上次拖拽距离
            _prevTouchDelta = currentTouchDelta;

            //计算方向拖拽距离
            if (_negativeCheckTouchDelta != Vector3.zero)
            {
                _negativeCheckTouchDelta -= currentTouchDelta;
                if ((isHorizontol && Mathf.Abs(_negativeCheckTouchDelta.x) < 2.0f)
                    || (isVertical && Mathf.Abs(_negativeCheckTouchDelta.y) < 2.0f))
                    return;
                else
                {
                    _negativeCheckTouchDelta = Vector3.zero;
                }
            }

            if (isMultipleDragOutOfBoundsSet)
            {
                _currentMaxDragOutOfFrontBoundsRatio = isNegative ? maxDragOutOfBehindBoundsRatio : maxDragOutOfFrontBoundsRatio;
                _currentMaxDragOutOfBehindBoundsRatio = isNegative ? maxDragOutOfFrontBoundsRatio : maxDragOutOfBehindBoundsRatio;
            }
            else
            {
                _currentMaxDragOutOfFrontBoundsRatio = maxDragOutOfFrontBoundsRatio;
                _currentMaxDragOutOfBehindBoundsRatio = maxDragOutOfFrontBoundsRatio;
            }

            //初始化参数
            _isOutOfBounds = false;
            _springBackDirection = shaco.Direction.Automatic;
            _outOfBoundsRate = 0;
            _newContentWorldPosition = Vector3.zero;
            _newContentPivot = Vector3.zero;

            int frontIndex = isNegative ? 0 : _listItems.Count - 1;
            int behindIndex = isNegative ? _listItems.Count - 1 : 0;

            if (isHorizontol)
            {
                if (currentTouchDelta.x < 0)
                {
                    pivotTmp = scrollDirection == shaco.Direction.Right && rectTrans.sizeDelta.x < this.GetComponent<RectTransform>().sizeDelta.x ? shaco.Pivot.UpperLeft : shaco.Pivot.UpperRight;
                    pivotTmp = CheckContentPivotWhenCenterLayout(pivotTmp);
                    var contentPositionTmp = UnityHelper.GetWorldPositionByPivot(_listItems[frontIndex].current, pivotTmp);
                    var listviewPositionTmp = UnityHelper.GetWorldPositionByPivot(this.gameObject, pivotTmp);
                    dragOffset = listviewPositionTmp.x - contentPositionTmp.x;
                    maxBoundsOffset = GetItemSizeByWorldPosition(GetBehindItem()).x * _currentMaxDragOutOfBehindBoundsRatio;

                    if (_currentMaxDragOutOfBehindBoundsRatio > 0 && dragOffset >= maxBoundsOffset * CHECK_OUT_OF_BOUNDS_FIXED_RATE)
                    {
                        _isOutOfBounds = true;
                    }
                    _springBackDirection = isNegative ? shaco.Direction.Right : shaco.Direction.Left;
                    if (dragOffset >= maxBoundsOffset)
                    {
                        _newContentWorldPosition = listviewPositionTmp - new Vector3(maxBoundsOffset, 0);
                        _newContentPivot = pivotTmp;
                    }

                    _currentScrollDirection = shaco.Direction.Left;
                }
                else
                {
                    pivotTmp = scrollDirection == shaco.Direction.Left && rectTrans.sizeDelta.x < this.GetComponent<RectTransform>().sizeDelta.x ? shaco.Pivot.UpperRight : shaco.Pivot.UpperLeft;
                    pivotTmp = CheckContentPivotWhenCenterLayout(pivotTmp);
                    var contentPositionTmp = UnityHelper.GetWorldPositionByPivot(_listItems[behindIndex].current, pivotTmp);
                    var listviewPositionTmp = UnityHelper.GetWorldPositionByPivot(this.gameObject, pivotTmp);
                    dragOffset = contentPositionTmp.x - listviewPositionTmp.x;
                    maxBoundsOffset = GetItemSizeByWorldPosition(GetFrontItem()).x * _currentMaxDragOutOfFrontBoundsRatio;

                    if (_currentMaxDragOutOfFrontBoundsRatio > 0 && dragOffset >= maxBoundsOffset * CHECK_OUT_OF_BOUNDS_FIXED_RATE)
                    {
                        _isOutOfBounds = true;
                    }
                    _springBackDirection = isNegative ? shaco.Direction.Left : shaco.Direction.Right;
                    if (dragOffset >= maxBoundsOffset)
                    {
                        _newContentWorldPosition = listviewPositionTmp + new Vector3(maxBoundsOffset, 0);
                        _newContentPivot = pivotTmp;
                    }

                    _currentScrollDirection = shaco.Direction.Right;
                }
            }
            else if (isVertical)
            {
                if (currentTouchDelta.y < 0)
                {
                    pivotTmp = scrollDirection == shaco.Direction.Up && rectTrans.sizeDelta.y < this.GetComponent<RectTransform>().sizeDelta.y ? shaco.Pivot.LowerLeft : shaco.Pivot.UpperLeft;
                    pivotTmp = CheckContentPivotWhenCenterLayout(pivotTmp);
                    var contentPositionTmp = UnityHelper.GetWorldPositionByPivot(_listItems[frontIndex].current, pivotTmp);
                    var listviewPositionTmp = UnityHelper.GetWorldPositionByPivot(this.gameObject, pivotTmp);

                    dragOffset = listviewPositionTmp.y - contentPositionTmp.y;
                    maxBoundsOffset = GetItemSizeByWorldPosition(GetFrontItem()).y * _currentMaxDragOutOfBehindBoundsRatio;

                    if (_currentMaxDragOutOfBehindBoundsRatio > 0 && dragOffset >= maxBoundsOffset * CHECK_OUT_OF_BOUNDS_FIXED_RATE)
                    {
                        _isOutOfBounds = true;
                    }
                    _springBackDirection = isNegative ? shaco.Direction.Down : shaco.Direction.Up;
                    if (dragOffset >= maxBoundsOffset)
                    {
                        _newContentWorldPosition = listviewPositionTmp - new Vector3(0, maxBoundsOffset);
                        _newContentPivot = pivotTmp;
                    }

                    _currentScrollDirection = shaco.Direction.Down;
                }
                else
                {
                    pivotTmp = scrollDirection == shaco.Direction.Down && rectTrans.sizeDelta.y < this.GetComponent<RectTransform>().sizeDelta.y ? shaco.Pivot.UpperLeft : shaco.Pivot.LowerLeft;
                    pivotTmp = CheckContentPivotWhenCenterLayout(pivotTmp);
                    var contentPositionTmp = UnityHelper.GetWorldPositionByPivot(_listItems[behindIndex].current, pivotTmp);
                    var listviewPositionTmp = UnityHelper.GetWorldPositionByPivot(this.gameObject, pivotTmp);

                    dragOffset = contentPositionTmp.y - listviewPositionTmp.y;
                    maxBoundsOffset = GetItemSizeByWorldPosition(GetBehindItem()).y * _currentMaxDragOutOfFrontBoundsRatio;

                    if (_currentMaxDragOutOfFrontBoundsRatio > 0 && dragOffset >= maxBoundsOffset * CHECK_OUT_OF_BOUNDS_FIXED_RATE)
                    {
                        _isOutOfBounds = true;
                    }
                    _springBackDirection = isNegative ? shaco.Direction.Up : shaco.Direction.Down;
                    if (dragOffset >= maxBoundsOffset)
                    {
                        _newContentWorldPosition = listviewPositionTmp + new Vector3(0, maxBoundsOffset);
                        _newContentPivot = pivotTmp;
                    }

                    _currentScrollDirection = shaco.Direction.Up;
                }
            }

            if (maxBoundsOffset > 0)
            {
                if (maxBoundsOffset != 0)
                    _outOfBoundsRate = dragOffset / maxBoundsOffset;
            }
            else if (dragOffset > 0)
            {
                _outOfBoundsRate = 1;
                _scrollRectContent.StopMovement();
            }
            else if (maxBoundsOffset == 0 && dragOffset == 0)
            {
                _scrollRectContent.StopMovement();
            }
        }

        private Vector3 CheckContentPivotWhenCenterLayout(Vector3 pivot)
        {
            if (isCenterLayout)
            {
                if (!_scrollRectContent.horizontal)
                {
                    pivot.x = shaco.Pivot.MiddleCenter.x;
                }
                if (!_scrollRectContent.vertical)
                {
                    pivot.y = shaco.Pivot.MiddleCenter.y;
                }
            }
            return pivot;
        }

        private Vector2 GetItemSizeByWorldPosition(GameObject target)
        {
            var posLeftBottom = UnityHelper.GetWorldPositionByPivot(target, shaco.Pivot.LowerLeft);
            var posRightTop = UnityHelper.GetWorldPositionByPivot(target, shaco.Pivot.UpperRight);
            return new Vector2(posRightTop.x - posLeftBottom.x, posRightTop.y - posLeftBottom.y);
        }

        private void RetainContentPositionByFirstItem()
        {
            if (_listItems.Count <= 0)
                return;

            _firstItemWhenEndGrag = _listItems[0];
            var rectTrans = _scrollRectContent.content;
            var contentRect = new Rect(rectTrans.localPosition.x, rectTrans.localPosition.y, rectTrans.rect.width, rectTrans.rect.height);
            var firstItemTrans = _firstItemWhenEndGrag.currentRectTransform;
            var oldPosition = firstItemTrans.localPosition;

            _fixContentOffsetWhenEndGrag = contentRect.position + new Vector2(oldPosition.x, oldPosition.y);
        }

        private void RetainContentPositionByLastItem()
        {
            if (_listItems.Count <= 0)
                return;

            _lastItemWhenEndGrag = _listItems[_listItems.Count - 1];
            var rectTrans = _scrollRectContent.content;
            var contentRect = new Rect(rectTrans.localPosition.x, rectTrans.localPosition.y, rectTrans.rect.width, rectTrans.rect.height);
            var lastItemTrans = _lastItemWhenEndGrag.currentRectTransform;
            var oldPosition = lastItemTrans.localPosition;

            _fixContentOffsetWhenEndGrag = contentRect.position + new Vector2(oldPosition.x, oldPosition.y);
        }

        private void FixScrollContentPositionWhenEndDrag()
        {
            if (autoUpdateItemTiming != AutoUpdateItemTiming.WillDragOutOfBounds)
                return;

            if (_fixContentOffsetWhenEndGrag != Vector3.zero)
            {
                var scrollContentTmp = _scrollRectContent.content;

                var oldContentLocalPos = scrollContentTmp.localPosition;

                //fix last item
                if (null != _lastItemWhenEndGrag)
                {
                    var posTmp = _lastItemWhenEndGrag.current.transform.localPosition;
                    var newScrollContentPos = new Vector3(0, 0, scrollContentTmp.localPosition.z);
                    scrollContentTmp.localPosition = newScrollContentPos - posTmp + _fixContentOffsetWhenEndGrag;
                    _lastItemWhenEndGrag = null;
                }
                //fix first item
                if (null != _firstItemWhenEndGrag)
                {
                    var posTmp = _firstItemWhenEndGrag.current.transform.localPosition;
                    var newScrollContentPos = new Vector3(0, 0, scrollContentTmp.localPosition.z);
                    scrollContentTmp.localPosition = newScrollContentPos - posTmp + _fixContentOffsetWhenEndGrag;
                    _firstItemWhenEndGrag = null;
                }

                switch (scrollDirection)
                {
                    case shaco.Direction.Right:
                    case shaco.Direction.Left: scrollContentTmp.localPosition = new Vector3(scrollContentTmp.localPosition.x, oldContentLocalPos.y, scrollContentTmp.localPosition.z); break;
                    case shaco.Direction.Down:
                    case shaco.Direction.Up: scrollContentTmp.localPosition = new Vector3(oldContentLocalPos.x, scrollContentTmp.localPosition.y, scrollContentTmp.localPosition.z); break;
                    default: Log.Error("ListView+Layout FixScrollContentPositionWhenEndDrag error: unsupport type=" + scrollDirection); break;
                }
                _fixContentOffsetWhenEndGrag = Vector3.zero;
            }

            //当使用拖拽滚动即将到达边界自动更新组件模式的时候
            //因为需要在更新的时候暂停滚动，否则会因为拖拽信息还在影响界面布局
            //所以需要在布局更新完毕后，恢复滚动和拖拽信息
            _scrollRectContent.StopDrag();
        }

        // private bool IsItemInVisibleArea(Item item)
        // {
        //     var rectItem = item.currentRectTransform.rect;
        //     return _rectTransform.rect.Contains(rectItem);
        // }

        private void CheckOutOfBoundsChanged()
        {
            if (_listItems.Count == 0) return;

            CheckRemainCountPromptArrow();

            //弃用方法，显示拖拽到边界的范围，实际上并不很友好，动画效果容易出问题
            // CheckOutOfBoundsRatioLimit();

            if (autoUpdateItemTiming == AutoUpdateItemTiming.WhenScrolling)
            {
                CheckDragRechBoundsAutoUpdateItem();
            }

            //check out of bounds callback 
            if (null != onItemsDragOutOfBoundsCallBack)
            {
                if (_outOfBoundsRate > 0.1f)
                {
                    if (_outOfBoundsRate > 1)
                        _outOfBoundsRate = 1;
                    onItemsDragOutOfBoundsCallBack(_outOfBoundsRate, _currentScrollDirection);
                }
            }
        }

        /// <summary>
        /// 获取组件大小
        /// </summary>
        private Vector2 GetItemSize(GameObject target)
        {
            return target.GetComponent<RectTransform>().sizeDelta;
        }

        /// <summary>
        /// 检查即将到达组件边缘需要自动刷新组件的功能
        /// </summary>
        private bool CheckDragRechBoundsAutoUpdateItem()
        {
            if (_listItems.Count == 0)
                return false;
                
            if (autoUpdateItemTiming == AutoUpdateItemTiming.WhenScrolling && _springBackDirection != shaco.Direction.Automatic)
            {
                //计算显示区域和滚动区域的差值，判断是否要自动刷新组件了
                var rechBoundsRate = 1.0f;
                float maxOutOBoundsRatio = 0;
                bool isNegative = scrollDirection == shaco.Direction.Left || scrollDirection == shaco.Direction.Down;

                int frontIndex = isNegative ? 0 : _listItems.Count - 1;
                int behindIndex = isNegative ? _listItems.Count - 1 : 0;

                bool isNegativeScrollDirection = (groupItemDirection == Direction.Right && _springBackDirection == shaco.Direction.Up)
                                                || (groupItemDirection == Direction.Left && _springBackDirection == shaco.Direction.Up)
                                                || (groupItemDirection == Direction.Up && _springBackDirection == shaco.Direction.Left)
                                                || (groupItemDirection == Direction.Down && _springBackDirection == shaco.Direction.Left);

                var itemIndexInfFullRow = GetItemIndexInFullRow(isNegativeScrollDirection);
                if (!_listItems.IsOutOfRange(itemIndexInfFullRow))
                {
                    frontIndex = behindIndex = itemIndexInfFullRow;
                    // Debug.Log("item=" + _listItems[itemIndexInfFullRow].current.name + " dir=" + _springBackDirection);
                }

                System.Action actionComputerUp = () =>
                {
                    var firstItem = _listItems[frontIndex];
                    var itemWorldRect = UnityHelper.GetWorldRect(firstItem.currentRectTransform);
                    var viewPos = shaco.UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.UpperCenter);
                    var scrollPos = shaco.UnityHelper.GetWorldPositionByPivot(firstItem.current.gameObject, shaco.Pivot.UpperCenter);
                    var viewToScrollOffsetPosition = scrollPos - viewPos;
                    maxOutOBoundsRatio = _currentMaxDragOutOfFrontBoundsRatio;
                    rechBoundsRate = viewToScrollOffsetPosition.y / (itemWorldRect.height);
                };

                System.Action actionComputerDown = () =>
                {
                    var lastItem = _listItems[behindIndex];
                    var itemWorldRect = UnityHelper.GetWorldRect(lastItem.currentRectTransform);
                    var viewPos = shaco.UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.LowerCenter);
                    var scrollPos = shaco.UnityHelper.GetWorldPositionByPivot(lastItem.current.gameObject, shaco.Pivot.LowerCenter);
                    var viewToScrollOffsetPosition = scrollPos - viewPos;
                    maxOutOBoundsRatio = _currentMaxDragOutOfBehindBoundsRatio;
                    rechBoundsRate = -viewToScrollOffsetPosition.y / (itemWorldRect.height);
                };

                System.Action actionComputerLeft = () =>
                {
                    var firstItem = _listItems[frontIndex];
                    var itemWorldRect = UnityHelper.GetWorldRect(firstItem.currentRectTransform);
                    var viewPos = shaco.UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.MiddleRight);
                    var scrollPos = shaco.UnityHelper.GetWorldPositionByPivot(firstItem.current.gameObject, shaco.Pivot.MiddleRight);
                    var viewToScrollOffsetPosition = scrollPos - viewPos;
                    maxOutOBoundsRatio = _currentMaxDragOutOfBehindBoundsRatio;
                    rechBoundsRate = viewToScrollOffsetPosition.x / (itemWorldRect.width);
                };

                System.Action actionComputerRight = () =>
                {
                    var lastItem = _listItems[behindIndex];
                    var itemWorldRect = UnityHelper.GetWorldRect(lastItem.currentRectTransform);
                    var viewPos = shaco.UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.MiddleLeft);
                    var scrollPos = shaco.UnityHelper.GetWorldPositionByPivot(lastItem.current.gameObject, shaco.Pivot.MiddleLeft);
                    var viewToScrollOffsetPosition = scrollPos - viewPos;
                    maxOutOBoundsRatio = _currentMaxDragOutOfFrontBoundsRatio;
                    rechBoundsRate = -viewToScrollOffsetPosition.x / (itemWorldRect.width);
                };

                switch (_springBackDirection)
                {
                    case shaco.Direction.Up:
                        {
                            if (!isNegative)
                                actionComputerUp();
                            else
                                actionComputerDown();
                            break;
                        }
                    case shaco.Direction.Down:
                        {
                            if (isNegative)
                                actionComputerUp();
                            else
                                actionComputerDown();
                            break;
                        }
                    case shaco.Direction.Left:
                        {
                            if (!isNegative)
                                actionComputerLeft();
                            else
                                actionComputerRight();
                            break;
                        }
                    case shaco.Direction.Right:
                        {
                            if (isNegative)
                                actionComputerLeft();
                            else
                                actionComputerRight();
                            break;
                        }
                    default: shaco.Log.Error("ListView+Layout CheckDragRechBoundsAutoUpdateItem erorr: unsupport type=" + _springBackDirection); break;
                }

                //设置为1是无意义的，因为刚好会完整过滤一行数据，所以重制为0
                if (1 == maxOutOBoundsRatio)
                    maxOutOBoundsRatio = 0;

                if (rechBoundsRate <= maxOutOBoundsRatio)
                {
                    int updateItemCount = CheckAutoUpdateItems(_springBackDirection);
                    if (updateItemCount > 0)
                    {
                        //强制不使用自动布局，因为只需要移动部分组件就可以完成了
                        //如果全部自动布局效率会比较低
                        _isUpdateListviewDirty = false;
                        CheckDragRechBoundsAutoUpdateItem();
                        return true;
                    }
                }
            }

            return false;
        }

        private void CheckRemainCountPromptArrow()
        {
            if (frontArrow == null && behindArrow == null) return;

            bool shouldShowFrontArrow = false;
            bool shouldHideFrontArrow = false;
            bool shouldShowBehindArrow = false;
            bool shouldHideBehindArrow = false;
            int startIndex = _itemIndexOffsetUseInSpringbackCallBack;
            int endIndex = _itemIndexOffsetUseInSpringbackCallBack + _listItems.Count;
            bool haveMoreFrontItem = startIndex > autoUpdateItemMinIndex;
            bool haveMoreBehindItem = endIndex < autoUpdateItemMaxIndex;

            switch (scrollDirection)
            {
                case shaco.Direction.Left:
                case shaco.Direction.Right:
                    {
                        var posLeftContent = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.LowerLeft);
                        var posRightContent = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.LowerRight);
                        var posLeftView = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.LowerLeft);
                        var posRightView = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.LowerRight);
                        if (posLeftContent.x + ARROW_SHOW_OFFSET_RATE >= posLeftView.x)
                            shouldHideFrontArrow = true;
                        else
                            shouldShowFrontArrow = true;
                        if (posRightContent.x - ARROW_SHOW_OFFSET_RATE <= posRightView.x)
                            shouldHideBehindArrow = true;
                        else
                            shouldShowBehindArrow = true;

                        if (scrollDirection == shaco.Direction.Left)
                        {
                            haveMoreFrontItem = haveMoreFrontItem.SwapValue(ref haveMoreBehindItem);
                        }
                        break;
                    }
                case shaco.Direction.Up:
                case shaco.Direction.Down:
                    {
                        var posUpContent = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.UpperLeft);
                        var posDownContent = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.LowerLeft);
                        var posUpView = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.UpperLeft);
                        var posDownView = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.LowerLeft);

                        if (posUpContent.y <= posUpView.y + ARROW_SHOW_OFFSET_RATE)
                            shouldHideFrontArrow = true;
                        else
                            shouldShowFrontArrow = true;
                        if (posDownContent.y + ARROW_SHOW_OFFSET_RATE >= posDownView.y)
                            shouldHideBehindArrow = true;
                        else
                            shouldShowBehindArrow = true;

                        if (scrollDirection == shaco.Direction.Up)
                        {
                            haveMoreFrontItem = haveMoreFrontItem.SwapValue(ref haveMoreBehindItem);
                        }
                        break;
                    }
                default: Log.Error("unsupport direction=" + scrollDirection); break;
            }

            //check arrow active
            if (shouldShowFrontArrow && _isOutOfFrontBounds)
            {
                if (null != frontArrow)
                    frontArrow.SetActive(true);
                _isOutOfFrontBounds = false;
            }
            else if (shouldHideFrontArrow && !_isOutOfFrontBounds && !haveMoreFrontItem)
            {
                if (null != frontArrow)
                    frontArrow.SetActive(false);
                _isOutOfFrontBounds = true;
            }

            if (shouldShowBehindArrow && _isOutOfBehindBounds)
            {
                if (null != behindArrow)
                    behindArrow.SetActive(true);
                _isOutOfBehindBounds = false;
            }
            else if (shouldHideBehindArrow && !_isOutOfBehindBounds && !haveMoreBehindItem)
            {
                if (null != behindArrow) behindArrow.SetActive(false);
                _isOutOfBehindBounds = true;
            }
        }

        private void CheckOutOfBoundsRatioLimit()
        {
            if (_listItems.Count <= 0 || maxDragOutOfFrontBoundsRatio == 1.0f && maxDragOutOfBehindBoundsRatio == 1.0f)
                return;

            if (_outOfBoundsRate > 1.0f)
            {
                shaco.UnityHelper.SetWorldPositionByPivot(_scrollRectContent.content.gameObject, _newContentWorldPosition, _newContentPivot);
            }
        }

        private void RevertToOldPos()
        {
            _scrollRectContent.normalizedPosition = _recordOldPos;
        }

        static public void ChangeParentLocal(GameObject target, GameObject parent)
        {
            var oldPos = target.transform.localPosition;
            var oldScale = target.transform.localScale;

            target.transform.SetParent(parent.transform);

            target.transform.localPosition = oldPos;
            target.transform.localScale = oldScale;
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public partial class ListViewComponent
    {
        private bool _isAutoUpdating = false;
        private bool _isLocationPositioning = false;

        public IEnumerator<GameObject> GetEnumerator()
        {
            var count = _listItems.Count;
            for (int i = 0; i < count; ++i)
            {
                yield return _listItems[i].current;
            }
        }

        public void AddItem(GameObject newItem)
        {
            InsertItem(newItem, _listItems.Count);
        }

        public void InitItemWithAutoUpdate(int startIndex, int endIndex, bool isDestroy = false)
        {
            if (onItemAutoUpdateCallBack == null)
            {
                shaco.Log.Error("ListView+ItemOperator error: if you wan't call 'InitItemWithAutoUpdate', please set ListView.onItemAutoUpdateCallBack before");
                return;
            }

            if (startIndex > endIndex)
            {
                shaco.Log.Warning("ListView+ItemOperator warning: startIndex(" + startIndex + ") > " + " endIndex(" + endIndex + ")");
                startIndex = startIndex.SwapValue(ref endIndex);
            }

            // if (showIndex != int.MinValue)
            // {
            //     if (showIndex < startIndex)
            //     {
            //         shaco.Log.Warning("ListView+ItemOperator warning: showdIndex(" + showIndex + ") < " + " startIndex(" + startIndex + ")");
            //         showIndex = startIndex;
            //     }
            //     if (showIndex > endIndex)
            //     {
            //         shaco.Log.Warning("ListView+ItemOperator warning: showdIndex(" + showIndex + ") > " + " endIndex(" + endIndex + ")");
            //         showIndex = endIndex;
            //     }
            // }

            //当重复使用同一个ListView的时候
            //如果还在滚动中需要强制停止滚动
            //否则会因为CheckDragRechBoundsAutoUpdateItem方法优化将_isUpdateListviewDirty设置为false，导致Update不会刷新滚动区域大小
            if (null != _scrollRectContent)
                _scrollRectContent.StopMovement();

            ClearItem(isDestroy);

            _itemIndexOffsetUseInSpringbackCallBack = startIndex;
            CheckAutoItemSize(endIndex, false);
            _itemIndexOffsetUseInSpringbackCallBack = startIndex;

            // int locationIndex = showIndex == int.MinValue ? startIndex : showIndex;
            // LocationByItemIndex(locationIndex);
        }

        public void RefreshAllItemsWithAutoUpdate()
        {
            if (onItemAutoUpdateCallBack == null)
            {
                shaco.Log.Error("ListView+ItemOperator error: if you wan't call 'RefreshAllItemsWithAutoUpdate', please set ListView.onItemAutoUpdateCallBack before");
                return;
            }

            int startIndex = GetItemStartDisplayIndex();
            int endIndex = GetItemEndDisplayIndex();
            for (int i = startIndex; i <= endIndex; ++i)
            {
                onItemAutoUpdateCallBack(i, GetItem(i - startIndex));
            }
        }

        public void InsertItem(GameObject newItem, int index)
        {
            CheckCompoment();

            if (index < 0)
            {
                Log.Warning("InsertItem warning: out of range, but auto fixed index=" + index);
                index = 0;
            }
            else if (index > _listItems.Count)
            {
                Log.Warning("InsertItem warning: out of range, but auto fixed index=" + index);
                index = _listItems.Count;
            }

            Item itemTmp = new Item();
            _listItems.Insert(index, itemTmp);
            int prevIndex = index - 1;
            int nextIndex = index + 1;
            itemTmp.current = newItem;
            itemTmp.currentRectTransform = newItem.GetComponent<RectTransform>();

            if (prevIndex < 0)
            {
                prevIndex = _listItems.Count - 1;
            }
            itemTmp.prev = _listItems[prevIndex];
            _listItems[prevIndex].next = itemTmp;

            if (nextIndex > _listItems.Count - 1)
            {
                nextIndex = 0;
            }
            itemTmp.next = _listItems[nextIndex];
            _listItems[nextIndex].prev = itemTmp;

            newItem.name = "Item" + index;
            ListViewComponent.ChangeParentLocal(newItem, _scrollRectContent.content.gameObject);
            newItem.transform.SetSiblingIndex(_listItems.Count - index - 1);

            newItem.SetActive(true);
            SetUpdateListViewDirty();
        }

        public GameObject AddItemByModel()
        {
            GameObject retValue = null;
            if (_itemModel != null)
            {
                retValue = GetItemModel();
                AddItem(retValue);
            }
            return retValue;
        }

        public void InsertItemByModel(int index)
        {
            if (_itemModel != null)
                InsertItem(GetItemModel(), index);
        }

        public void RemoveItem(int index, bool isDestroy = true)
        {
            if (index < 0 || index > _listItems.Count - 1)
            {
                Log.Error("ListView RemoveItem error: out of range");
                return;
            }

            var item = _listItems[index];

            //cut connect
            item.prev.next = item.next;
            item.next.prev = item.prev;
            item.next = null;
            item.prev = null;

            //destroy object
            if (_listItems[index].current)
            {
                if (isDestroy)
                    UnityHelper.SafeDestroy(_listItems[index].current);
                else
                {
                    _listItems[index].current.gameObject.SetActive(false);
                    _listItemCache.Add(_listItems[index].current.gameObject);
                }
            }

            var itemTmp = _listItems[index];
            _listItems.RemoveAt(index);

            if (_listItems.Count > 0)
            {
                if (_firstItemWhenEndGrag == itemTmp) _firstItemWhenEndGrag = _listItems[_listItems.Count - 1];
                if (_lastItemWhenEndGrag == itemTmp) _lastItemWhenEndGrag = null;
            }
            SetUpdateListViewDirty();
        }

        public void RemoveItem(GameObject item, bool isDestroy = true)
        {
            int index = GetItemIndex(item);
            if (index >= 0 && index < _listItems.Count)
            {
                RemoveItem(index, isDestroy);
            }
            else
            {
                Log.Error("RemoveItem error: not find item =" + item);
            }
        }

        public void RemoveItemRange(int startIndex, int count, bool isDestroy = true)
        {
            if (startIndex < 0 || startIndex > _listItems.Count - 1 || count > _listItems.Count)
            {
                Log.Error("ListView RemoveItemRange erorr: out of range! startIndex=" + startIndex + " count=" + count + " item count=" + _listItems.Count);
                return;
            }

            for (int i = startIndex + count - 1; i >= startIndex; --i)
            {
                RemoveItem(i, isDestroy);
            }
        }

        public void SwapItem(int sourceIndex, int destinationIndex)
        {
            if (sourceIndex < 0 || sourceIndex > _listItems.Count - 1)
            {
                Log.Error("ListView SwapItem error: out of range: sourceIndex=" + sourceIndex + " count=" + _listItems.Count);
                return;
            }
            if (destinationIndex < 0 || destinationIndex > _listItems.Count - 1)
            {
                Log.Error("ListView SwapItem error: out of range: destinationIndex=" + destinationIndex + " count=" + _listItems.Count);
                return;
            }
            _listItems.SwapValue(sourceIndex, destinationIndex);
            SetUpdateListViewDirty();
        }

        public void ClearItem(bool isDestroy = true)
        {
            CheckCompoment();

            if (isDestroy)
            {
                for (int i = 0; i < _listItems.Count; ++i)
                {
                    UnityHelper.SafeDestroy(_listItems[i].current);
                }
            }
            else
            {
                //这里需要反向遍历以保证_listItemCache的弹出顺序和原来一致
                for (int i = _listItems.Count - 1; i >= 0; --i)
                {
                    _listItems[i].current.SetActive(false);
                    _listItemCache.Add(_listItems[i].current);
                }
            }
            _listItems.Clear();

            if (_oldContentSize.x > 0 && _oldContentSize.y > 0)
            {
                _scrollRectContent.content.sizeDelta = new Vector2(_oldContentSize.x, _oldContentSize.y);
                _scrollRectContent.content.transform.localPosition = Vector3.zero;
            }
            _itemIndexOffsetUseInSpringbackCallBack = 0;
            _firstItemWhenEndGrag = null;
            _lastItemWhenEndGrag = null;
            SetUpdateListViewDirty();
        }

        public void SetItemModel(GameObject item)
        {
            if (item == null)
            {
                Log.Error("SetItemModel error: item is null");
                return;
            }

            if (item.GetComponent<RectTransform>() == null)
            {
                Log.Error("SetItemModel error: item must contain RectTransform Compoment");
                return;
            }

            _itemModel = item;
            _itemModel.SetActive(false);
        }

        public GameObject GetItemModel()
        {
            if (_itemModel == null)
            {
                Log.Error("GetItemModel error: you don't set model, item model is null");
                return null;
            }
            else
            {
                var ret = Instantiate(_itemModel) as GameObject;
                ret.SetActive(true);
                return ret;
            }
        }

        public GameObject GetFirstItem()
        {
            GameObject retValue = null;
            if (_listItems.Count == 0)
            {
                Log.Error("ListView GetFirstItem error: listview is empty !");
                return retValue;
            }
            retValue = _listItems[0].current;
            return retValue;
        }

        public GameObject GetLastItem()
        {
            GameObject retValue = null;
            if (_listItems.Count == 0)
            {
                Log.Error("ListView GetFirstItem error: listview is empty !");
                return retValue;
            }
            retValue = _listItems[_listItems.Count - 1].current;
            return retValue;
        }

        public int GetItemStartDisplayIndex()
        {
            return _itemIndexOffsetUseInSpringbackCallBack;
        }

        public int GetItemEndDisplayIndex()
        {
            return _itemIndexOffsetUseInSpringbackCallBack + _listItems.Count - 1;
        }

        public int GetItemIndexByDispalyIndex(int displayIndex)
        {
            return displayIndex - _itemIndexOffsetUseInSpringbackCallBack;
        }

        public void Sort(SortCompareFunc compareFunc)
        {
            if (_listItems.Count == 0)
                return;

            List<GameObject> listSwap = new List<GameObject>();
            _listItems.Sort(new _SortCompareFunc(compareFunc));

            while (_listItems.Count > 0)
            {
                listSwap.Add(_listItems[0].current);
                RemoveItem(0, false);
            }

            for (int i = 0; i < listSwap.Count; ++i)
            {
                AddItem(listSwap[i]);
                listSwap[i].gameObject.SetActive(true);
            }
            SetUpdateListViewDirty();
        }

        public GameObject GetItem(int index)
        {
            GameObject ret = null;
            if (index < 0 || index > _listItems.Count - 1)
            {
                return ret;
            }
            else
            {
                ret = _listItems[index].current;
            }
            return ret;
        }

        public GameObject PopItemFromCacheOrCreateFromModel()
        {
            GameObject ret = null;
            if (_listItemCache.Count > 0)
            {
                ret = _listItemCache[_listItemCache.Count - 1];
                _listItemCache.RemoveAt(_listItemCache.Count - 1);
            }
            else
            {
                ret = GetItemModel();
                if (null == ret)
                {
                    shaco.Log.Error("ListView PopItemFromCacheOrCreateFromModel error: model is null, please call 'SetItemModel' at frist");
                }
            }
            ret.SetActive(true);
            return ret;
        }

        public int GetItemIndex(GameObject item)
        {
            int ret = -1;
            for (int i = 0; i < _listItems.Count; ++i)
            {
                if (_listItems[i].current == item)
                {
                    ret = i;
                    break;
                }
            }

            if (ret == -1)
            {
                Log.Error("GetItemIndex error: not find item =" + item);
                return ret;
            }
            else
                return ret;
        }

        public void LocationByItemIndex(int displayIndex, shaco.Anchor anchor = shaco.Anchor.MiddleCenter)
        {
            if (!CanLocation())
                return;

            int addDisplayIndex = displayIndex;
            if (addDisplayIndex <= _itemIndexOffsetUseInSpringbackCallBack + this.eachOfGroup)
                addDisplayIndex -= this.eachOfGroup;
            else if (addDisplayIndex > _listItems.Count - this.eachOfGroup)
                addDisplayIndex += this.eachOfGroup;

            CheckAutoItemSize(addDisplayIndex);
            UpdateListView();

            var itemIndex = displayIndex - GetItemStartDisplayIndex();
            itemIndex = Mathf.Clamp(itemIndex, 0, _listItems.Count - 1);
            var item = GetItem(itemIndex);
            var newWorldPosition = GetLocationItemWorldPositionWithConvert(item, anchor);

            PauseScrollingBase();
            _scrollRectContent.content.transform.position = newWorldPosition;
            CheckRemainCountPromptArrow();
            ResumeScrollingBase();
        }

        /// <summary>
        /// 带动画的定位组建位置
        /// </summary>
        /// <param name="displayIndex">显示下标</param>
        /// <param name="moveDurationSeconds">移动动画执行时间</param>
        public void LocationActionByItemIndex(int displayIndex, float moveDurationSeconds, shaco.Anchor anchor = shaco.Anchor.MiddleCenter)
        {
            if (!CanLocation())
                return;

            if (autoUpdateItemTiming != AutoUpdateItemTiming.WhenScrolling)
            {
                shaco.Log.Error("ListView+ItemOperator LocationActionByItemIndex error: only support when scrolling auto update mode");
                return;
            }

            if (_listItems.IsNullOrEmpty() || null == _itemModel)
                return;

            if (_isLocationPositioning)
            {
                shaco.Log.Warning("ListView+ItemOperator LocationActionByItemIndex warning: is location positioning... please wait");
                return;
            }

            OnPublicValueChangedCheck();

            //计算当前组件到目标组件的距离
            int offsetItemCount = displayIndex;
            offsetItemCount = offsetItemCount - autoUpdateItemMinIndex;
            offsetItemCount = System.Math.Abs(offsetItemCount);

            var itemModelTmp = null == _itemModel ? _listItems[0].current : _itemModel;
            var modelSizeTmp = itemModelTmp.GetComponent<RectTransform>().sizeDelta;

            int remainFrontStartItemCount, remainFrontEndItemCount, remainBehindStartItemCount, remainBehindEndItemCount;
            GetStartAndEndRemainItemCount(out remainFrontStartItemCount, out remainFrontEndItemCount, out remainBehindStartItemCount, out remainBehindEndItemCount);

            bool isFisrtOrLastGroup = false;
            if (System.Math.Abs(displayIndex - autoUpdateItemMinIndex) <= remainFrontStartItemCount - 1)
                isFisrtOrLastGroup = true;
            else if (System.Math.Abs(autoUpdateItemMaxIndex - displayIndex) <= remainBehindEndItemCount - 1)
                isFisrtOrLastGroup = true;

            //补充差值
            int offsetItemMainCount = offsetItemCount / this.eachOfGroup;
            if (!isFisrtOrLastGroup && offsetItemCount % this.eachOfGroup != 0)
                ++offsetItemMainCount;

            var offsetItemLocalPos = offsetItemMainCount * new Vector3(itemMargin.x + modelSizeTmp.y, itemMargin.y + modelSizeTmp.y);
            Vector3 newWorldPosition = Vector3.zero;

            //获取偏移锚点
            var itemIndex = displayIndex - GetItemStartDisplayIndex();
            itemIndex = Mathf.Clamp(itemIndex, 0, _listItems.Count - 1);
            var item = GetItem(itemIndex);
            var offsetPivot = shaco.Anchor.MiddleCenter.ToPivot() - anchor.ToNegativePivot();
            var sizeOffsetWithAnchor = item.GetComponent<RectTransform>().sizeDelta * offsetPivot;

            switch (scrollDirection)
            {
                case shaco.Direction.Down:
                    {
                        if (!isFisrtOrLastGroup)
                            offsetItemLocalPos -= new Vector3(0, modelSizeTmp.y / 2);
                        newWorldPosition = new Vector3(_scrollRectContent.content.localPosition.x, offsetItemLocalPos.y + sizeOffsetWithAnchor.y);
                        break;
                    }
                case shaco.Direction.Up:
                    {
                        if (!isFisrtOrLastGroup)
                            offsetItemLocalPos += new Vector3(0, modelSizeTmp.y / 2 + modelSizeTmp.y);
                        newWorldPosition = new Vector3(_scrollRectContent.content.localPosition.x, -offsetItemLocalPos.y + sizeOffsetWithAnchor.y);
                        break;
                    }
                case shaco.Direction.Right:
                    {
                        if (!isFisrtOrLastGroup)
                            offsetItemLocalPos += new Vector3(modelSizeTmp.x / 2 + modelSizeTmp.x, 0);
                        newWorldPosition = new Vector3(-offsetItemLocalPos.x + sizeOffsetWithAnchor.x, _scrollRectContent.content.localPosition.y);
                        break;
                    }
                case shaco.Direction.Left:
                    {
                        if (!isFisrtOrLastGroup)
                            offsetItemLocalPos -= new Vector3(modelSizeTmp.x / 2, 0);
                        newWorldPosition = new Vector3(offsetItemLocalPos.x + sizeOffsetWithAnchor.x, _scrollRectContent.content.localPosition.y);
                        break;
                    }
                default: shaco.Log.Error("ListView+ItemOperator LocationActionByItemIndex error: unsupport type=" + scrollDirection); break;
            }

            //如果就在当前位置，都不再执行滚动方法
            if (IsSamePlaceWithLocationMove(newWorldPosition, false))
                return;

            _isLocationPositioning = true;

            //pause scrolling
            PauseScrollingBase();

            //run move action
            var moveAction = RunLocatinAction(newWorldPosition, moveDurationSeconds, false);
            moveAction.onCompleteFunc += (shaco.ActionBase ac) =>
            {
                ResumeScrollingBase();
                _isLocationPositioning = false;
            };
        }

        public void LocationActionByGameObject(GameObject target, float moveDurationSeconds, shaco.Anchor anchor = shaco.Anchor.MiddleCenter)
        {
            LocationActionByWorldPosition(UnityHelper.GetWorldPositionByPivot(target, anchor.ToNegativePivot()), moveDurationSeconds);
        }

        public void LocationActionByWorldPosition(Vector3 worldPosition, float moveDurationSeconds)
        {
            if (!CanLocation())
                return;

            if (_isLocationPositioning)
            {
                shaco.Log.Warning("ListView+ItemOperator LocationActionByWorldPosition warning: is location positioning... please wait");
                return;
            }
            _isLocationPositioning = true;

            var newWorldPosition = GetLocationItemWorldPositionWithConvert(worldPosition);
            if (IsSamePlaceWithLocationMove(newWorldPosition, true))
            {
                _isLocationPositioning = false;
                return;
            }

            PauseScrollingBase();
            var moveAction = RunLocatinAction(newWorldPosition, moveDurationSeconds, true);
            moveAction.onCompleteFunc += (shaco.ActionBase action) =>
            {
                _isLocationPositioning = false;
            };
            CheckRemainCountPromptArrow();
            ResumeScrollingBase();
        }

        public void LocationByGameObject(GameObject target, shaco.Anchor anchor = shaco.Anchor.MiddleCenter)
        {
            LocationByWorldPosition(UnityHelper.GetWorldPositionByPivot(target, anchor.ToNegativePivot()));
        }

        public void LocationByWorldPosition(Vector3 worldPosition)
        {
            if (!CanLocation())
                return;

            var newWorldPosition = GetLocationItemWorldPositionWithConvert(worldPosition);

            PauseScrollingBase();
            _scrollRectContent.content.transform.position = newWorldPosition;
            CheckRemainCountPromptArrow();
            ResumeScrollingBase();
        }

        private bool CanLocation()
        {
            CheckInit();
            return _scrollRectContent.CanScroll();
        }

        private bool IsSamePlaceWithLocationMove(Vector3 newWorldPosition, bool isWorldPosition)
        {
            if (isWorldPosition)
            {
                if (newWorldPosition.x.Round(1) == _scrollRectContent.content.transform.position.x.Round(1)
                && newWorldPosition.y.Round(1) == _scrollRectContent.content.transform.position.y.Round(1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (newWorldPosition.x.Round(1) == _scrollRectContent.content.transform.localPosition.x.Round(1)
                && newWorldPosition.y.Round(1) == _scrollRectContent.content.transform.localPosition.y.Round(1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private Vector3 GetLocationItemWorldPositionWithConvert(GameObject target, shaco.Anchor anchor)
        {
            return GetLocationItemWorldPositionWithConvert(UnityHelper.GetWorldPositionByPivot(target, anchor.ToNegativePivot()));
        }

        private Vector3 GetLocationItemWorldPositionWithConvert(Vector3 worldPosition)
        {
            var retValue = Vector3.zero;
            var newWorldPosition = Vector3.zero;
            var newContentPivot = shaco.Pivot.MiddleCenter;

            switch (scrollDirection)
            {
                case shaco.Direction.Down:
                case shaco.Direction.Up:
                    {
                        // newContentPivot = shaco.Pivot.LeftMiddle;
                        var offsetY = UnityHelper.GetWorldPositionByPivot(this.gameObject, newContentPivot).y - worldPosition.y;

                        //fix in listview content
                        var contentTopY = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.UpperLeft).y;
                        var listviewTopY = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.UpperLeft).y;

                        var contentBottomY = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.LowerLeft).y;
                        var listviewBottomY = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.LowerLeft).y;

                        if (listviewTopY - contentTopY > offsetY)
                            offsetY = listviewTopY - contentTopY;

                        if (listviewBottomY - contentBottomY < offsetY)
                            offsetY = listviewBottomY - contentBottomY;

                        newWorldPosition = new Vector3(_scrollRectContent.content.transform.position.x, _scrollRectContent.content.transform.position.y + offsetY);
                        break;
                    }
                case shaco.Direction.Right:
                case shaco.Direction.Left:
                    {
                        // newContentPivot = shaco.Pivot.MiddleTop;
                        var offsetX = UnityHelper.GetWorldPositionByPivot(this.gameObject, newContentPivot).x - worldPosition.x;

                        //fix in listview content
                        var contentLeftX = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.UpperLeft).x;
                        var listviewLeftX = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.UpperLeft).x;

                        var contentRightX = UnityHelper.GetWorldPositionByPivot(_scrollRectContent.content.gameObject, shaco.Pivot.LowerRight).x;
                        var listviewRightX = UnityHelper.GetWorldPositionByPivot(this.gameObject, shaco.Pivot.LowerRight).x;

                        if (listviewRightX - contentRightX > offsetX)
                            offsetX = listviewRightX - contentRightX;

                        if (listviewLeftX - contentLeftX < offsetX)
                            offsetX = listviewLeftX - contentLeftX;

                        newWorldPosition = new Vector3(_scrollRectContent.content.transform.position.x + offsetX, _scrollRectContent.content.transform.position.y);
                        break;
                    }
            }

            retValue = newWorldPosition;
            return retValue;
        }

        private ActionBase RunLocatinAction(Vector3 newPosition, float moveDurationSeconds, bool isWorldPosition)
        {
            //run move action
            var moveToAction = shaco.MoveTo.Create(newPosition, moveDurationSeconds, isWorldPosition);
            var accelerateAction = shaco.Accelerate.Create(moveToAction,
                new shaco.Accelerate.ControlPoint(0, 3.0f),
                new shaco.Accelerate.ControlPoint(0.5f, 2.0f),
                new shaco.Accelerate.ControlPoint(1, 0.2f));
            accelerateAction.RunAction(_scrollRectContent.content.gameObject);

            return accelerateAction;
        }

        private int CheckAutoItemSize(int endIndex, bool autoRemove = true)
        {
            int realAddCount = 0;
            if (_isAutoUpdating)
            {
                if (endIndex < GetItemStartDisplayIndex() || endIndex > GetItemEndDisplayIndex())
                {
                    shaco.Log.Warning("ListView+ItemOperator CheckAutoItemSize warning: updating now... endIndex is out of range, index=" + endIndex + " start=" + GetItemStartDisplayIndex() + " end=" + GetItemEndDisplayIndex());
                }
                return realAddCount;
            }

            _isAutoUpdating = true;

            if (null == onItemAutoUpdateCallBack)
            {
                if (endIndex < 0 || endIndex > _listItems.Count - 1)
                {
                    if (autoUpdateItemCountWhenTiming > 0)
                        shaco.Log.Error("ListView CheckAutoItemSize error: can't auto update item size automatic when no 'onItemAutoUpdateCallBack' delegate ");
                }
            }
            else
            {
                if (endIndex >= _itemIndexOffsetUseInSpringbackCallBack)
                {
                    endIndex = endIndex < autoUpdateItemMaxIndex ? endIndex : autoUpdateItemMaxIndex;

                    int startIndex = _itemIndexOffsetUseInSpringbackCallBack + _listItems.Count;
                    var addCount = endIndex - startIndex + 1;
                    var maxDisplayItemCount = System.Math.Max(_listItems.Count, _itemIndexOffsetUseInSpringbackCallBack);
                    if (autoRemove && addCount > maxDisplayItemCount)
                    {
                        startIndex = endIndex - maxDisplayItemCount + 1;
                        int newAddCount = endIndex - startIndex + 1;
                        _itemIndexOffsetUseInSpringbackCallBack += addCount - newAddCount;
                        addCount = newAddCount;
                    }
                    var removeCount = System.Math.Min(maxDisplayItemCount, addCount);

                    if (addCount > 0)
                    {
                        RetainContentPositionByLastItem();
                    }

                    if (null != onItemsCanAutoUpdateCallBack && addCount > 0)
                    {
                        if (!onItemsCanAutoUpdateCallBack(startIndex, endIndex))
                        {
                            _isAutoUpdating = false;
                            return realAddCount;
                        }
                    }

                    for (int i = startIndex; i <= endIndex; ++i)
                    {
                        if (AutoUpdateItem(i, _listItems.Count, 0, removeCount-- > 0 ? autoRemove : false))
                        {
                            ++_itemIndexOffsetUseInSpringbackCallBack;
                            ++realAddCount;
                        }
                    }

                    if (null != onItemsDidAutoUpdateCallBack && realAddCount > 0)
                    {
                        onItemsDidAutoUpdateCallBack(startIndex, endIndex - (addCount - realAddCount));
                    }
                    if (realAddCount <= 0)
                    {
                        _lastItemWhenEndGrag = null;
                        _fixContentOffsetWhenEndGrag = Vector3.zero;
                    }
                }
                else
                {
                    endIndex = endIndex > autoUpdateItemMinIndex ? endIndex : autoUpdateItemMinIndex;

                    int startIndex = _itemIndexOffsetUseInSpringbackCallBack - 1;
                    var addCount = startIndex - endIndex + 1;
                    var maxDisplayItemCount = System.Math.Max(_listItems.Count, _itemIndexOffsetUseInSpringbackCallBack);
                    if (autoRemove && addCount > maxDisplayItemCount)
                    {
                        startIndex = endIndex + maxDisplayItemCount - 1;
                        int newAddCount = startIndex - endIndex + 1;
                        _itemIndexOffsetUseInSpringbackCallBack -= addCount - newAddCount;
                        addCount = newAddCount;
                    }
                    var removeCount = System.Math.Min(maxDisplayItemCount, addCount);
                    if (addCount > 0)
                    {
                        RetainContentPositionByFirstItem();
                    }

                    if (null != onItemsCanAutoUpdateCallBack && addCount > 0)
                    {
                        if (!onItemsCanAutoUpdateCallBack(startIndex, endIndex))
                        {
                            _isAutoUpdating = false;
                            return realAddCount;
                        }
                    }

                    for (int i = startIndex; i >= endIndex; --i)
                    {
                        if (AutoUpdateItem(i, 0, _listItems.Count - 1, removeCount-- > 0 ? autoRemove : false))
                        {
                            --_itemIndexOffsetUseInSpringbackCallBack;
                            ++realAddCount;
                        }
                    }

                    if (null != onItemsDidAutoUpdateCallBack && realAddCount > 0)
                    {
                        onItemsDidAutoUpdateCallBack(startIndex, endIndex + (addCount - realAddCount));
                    }
                    if (realAddCount == 0)
                    {
                        _firstItemWhenEndGrag = null;
                        _fixContentOffsetWhenEndGrag = Vector3.zero;
                    }
                }
            }

            _isAutoUpdating = false;
            return realAddCount;
        }

        private bool AutoUpdateItem(int itemIndex, int insertIndex, int removeIndex, bool autoRemove)
        {
            if (null != onItemCanAutoUpdateCallBack)
            {
                if (!onItemCanAutoUpdateCallBack(itemIndex))
                {
                    return false;
                }
            }

            if (autoRemove)
            {
                this.RemoveItem(removeIndex, false);
            }

            if (insertIndex < 0) insertIndex = 0;
            if (insertIndex > _listItems.Count) insertIndex = _listItems.Count;

            var newItem = PopItemFromCacheOrCreateFromModel();
            this.InsertItem(newItem, insertIndex);

            bool isVeryFastScrolling = this._scrollRectContent.IsVeryFastScrolling(this._currentScrollDirection);

            //当快速滚动时候不要去刷新组件，否则非常卡
            if (null == _scrollRectContent.onVeryFastScrollingEndCallBack && !isVeryFastScrolling)
                onItemAutoUpdateCallBack(itemIndex, newItem);
            else
            {
                //等待快速滚动结束
                if (null == _scrollRectContent.onVeryFastScrollingEndCallBack)
                {
                    _scrollRectContent.onVeryFastScrollingEndCallBack += () =>
                    {
                        //重新回调并初始化所有组件
                        var startIndex = GetItemStartDisplayIndex();
                        var endIndex = GetItemEndDisplayIndex();
                        int index = 0;

                        for (int i = startIndex; i <= endIndex; ++i)
                        {
                            var itemTmp = _listItems[index++];
                            onItemAutoUpdateCallBack(i, itemTmp.current);
                        }
                    };
                }
            }

            newItem.gameObject.SetActive(true);
            bool isNegative = insertIndex < removeIndex;
            if (!isNegative)
            {
                this.MoveItemToTargetSide(insertIndex, insertIndex - 1, false);
            }
            else
            {
                this.MoveItemToTargetSide(insertIndex, insertIndex + 1, true);
            }
            return true;
        }
    }
}
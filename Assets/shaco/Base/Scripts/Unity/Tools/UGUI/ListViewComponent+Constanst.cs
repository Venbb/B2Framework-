using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public partial class ListViewComponent
    {
        public enum CenterLayoutType
        {
            NoCenter,
            CenterHorizontalOnly,
            CenterVerticalOnly,
            Center
        }

        /// <summary>
        /// 自动刷新组件的时机
        /// </summary>
        public enum AutoUpdateItemTiming
        {
            //滚动同时刷新组件
            WhenScrolling,
            //拖拽滚动即将超出边界
            WillDragOutOfBounds,
        }

        [System.Serializable]
        public class Item
        {
            public GameObject current;
            public RectTransform currentRectTransform;
            public Item prev;
            public Item next;
        }

        public delegate int SortCompareFunc(Item left, Item right);
        class _SortCompareFunc : IComparer<Item>
        {
            private SortCompareFunc compareFunc = null;
            public _SortCompareFunc(SortCompareFunc comp)
            {
                compareFunc = comp;
            }
            public int Compare(Item left, Item right)
            {
                return compareFunc == null ? 0 : compareFunc(left, right);
            }
        }

        //设定超出边界拖拽回来所占比率，0.5f刚好，一般不用修改了
        private readonly float CHECK_OUT_OF_BOUNDS_FIXED_RATE = 0.5f;

        //滚动中刷新超出边界范围修正，值越接近0越平滑，但是性能可能有所损耗
        // private readonly float MAX_RECH_BOUNDS_FIXED_RATE = 0.0f;

        //箭头显示用，当可视区域与滚动区域间隔小于该值时候，会显示或者隐藏箭头
        private readonly float ARROW_SHOW_OFFSET_RATE = 0.01f;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabScrollRect : ScrollRect
{
    enum DragDir
    {
        None,
        Horizontal,
        Vertical
    }
    //一个切页到下一个或者多个切页的滑动时间(小于一个按照半分比计算）
    private float oneTabSwitchTime = 0.2f;
    //当前切页
    private int currentPageIndex = 0;
    //拖动的百分比（计算从这个百分比到目标切页的滑动用）
    private float dragPersent = 0;
    //是否正在被拖拽滑动
    private bool isDragging = false;
    //是否滑动完成
    private bool tabReached = true;
    //记录时间用
    private float time = 0;
    //开始拖拽时候的坐标（计算横纵拖动用,从而计算是拖动切页还是拖动切页里面的ScrollView）
    private Vector2 beginDragPosition;
    //拖拽的方向
    private DragDir dragDir = DragDir.None;
    //拖拽方向识别的误差灵敏度（越小越灵敏）
    private float dragDirSensitive = 10;
    //当前显示的2个拖拽的切页
    private int leftShowIndex = 0;
    private int rightShowIndex = 0;

    // Start is called before the first frame update
    protected override void Awake()
    {
        for(int i = 0; i < content.childCount; ++i)
        {
            LoopScrollRect lcr = content.GetChild(i).GetComponent<LoopScrollRect>();
            if (lcr != null)
            {
                lcr.del_td += OnDrag_CB;
                lcr.del_tdbegin += OnBeginDrag_CB;
                lcr.del_tdend += OnEndDrag_CB;
            }
            InTabNormalScrollRect nsr = content.GetChild(i).GetComponent<InTabNormalScrollRect>();
            if (nsr != null)
            {
                nsr.del_td += OnDrag_CB;
                nsr.del_tdbegin += OnBeginDrag_CB;
                nsr.del_tdend += OnEndDrag_CB;
            }
        }
        base.Awake();
    }

    protected override void Start()
    {
        UpdateChildPageVisableState();
        base.Start();
    }

    void Update()
    {
        if (Application.isPlaying && !isDragging && !tabReached)
        {
            time = time + Time.deltaTime;
            float singlePersent = content.childCount == 1 ? 0 : 1f / (content.childCount - 1);
            float targetPersent = singlePersent * currentPageIndex;
            float totalSwitchtime = oneTabSwitchTime;
            if (Mathf.Abs(targetPersent - dragPersent) < singlePersent)
            {
                totalSwitchtime = Mathf.Abs(targetPersent - dragPersent) / singlePersent * oneTabSwitchTime;
            }

            normalizedPosition = Vector2.Lerp(new Vector2(dragPersent, 0f), new Vector2(targetPersent, 0f), time / totalSwitchtime);
            CaculateChildPageVisableState();

            if (time >= totalSwitchtime)
            {
                time = 0;
                tabReached = true;
                leftShowIndex = currentPageIndex;
                rightShowIndex = currentPageIndex;
                UpdateChildPageVisableState();
            }
        }
    }

    public void turnToPage(int pageIdx)
    {
        if (pageIdx < 0)
            pageIdx = 0;
        if (pageIdx > content.childCount - 1)
            pageIdx = content.childCount - 1;
        if(pageIdx == currentPageIndex)
        {
            return;
        }

        dragPersent = normalizedPosition.x;
        currentPageIndex = pageIdx;
        tabReached = false;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        tabReached = false;
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        CaculateChildPageVisableState();
        //int countPageIndex = (int)(GetNowPageNum() - 0.5f);
        //if (rightShowIndex > currentPageIndex)
        //{
        //    countPageIndex = (int)(GetNowPageNum() - 0.5f);
        //}
        //else if (leftShowIndex < currentPageIndex)
        //{
        //    countPageIndex = (int)Mathf.Ceil(GetNowPageNum() - 0.5f);
        //}
        //if (countPageIndex != currentPageIndex)
        //{
        //    return;
        //}
        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        dragPersent = normalizedPosition.x;
        currentPageIndex = (int)Mathf.Floor(GetNowPageNum());
        isDragging = false;
        base.OnEndDrag(eventData);
    }

    public bool OnBeginDrag_CB(PointerEventData eventData)
    {
        beginDragPosition = eventData.pointerCurrentRaycast.screenPosition;
        if(Mathf.Abs(eventData.delta.x) > dragDirSensitive)
        {
            dragDir = DragDir.Horizontal;
        }
        else if(Mathf.Abs(eventData.delta.y) > dragDirSensitive)
        {
            dragDir = DragDir.Vertical;
        }
        if (dragDir != DragDir.Vertical)
            this.OnBeginDrag(eventData);

        return dragDir == DragDir.Horizontal;
    }

    public bool OnDrag_CB(PointerEventData eventData)
    {
        if(dragDir == DragDir.None)
        {
            Vector2 dragPosition = eventData.pointerCurrentRaycast.screenPosition;
            if(Mathf.Abs(dragPosition.x - beginDragPosition.x) > dragDirSensitive)
            {
                dragDir = DragDir.Horizontal;
            }
            else if(Mathf.Abs(dragPosition.y - beginDragPosition.y) > dragDirSensitive)
            {
                dragDir = DragDir.Vertical;
            }
        }

        if (dragDir != DragDir.Vertical)
            this.OnDrag(eventData);

        return dragDir == DragDir.Horizontal; ;
    }

    public bool OnEndDrag_CB(PointerEventData eventData)
    {
        dragDir = DragDir.None;
        this.OnEndDrag(eventData);
        return false;
    }

    private float GetNowPageNum()
    {
        float pageNum = normalizedPosition.x * (content.childCount - 1) + 0.5f;
        if (pageNum < 0)
            pageNum = 0;
        if (pageNum > content.childCount - 0.5f)
            pageNum = content.childCount - 0.5f;

        return pageNum;
    }

    private void CaculateChildPageVisableState()
    {
        float nowLeftPageNum = GetNowPageNum() - 0.5f;

        if (leftShowIndex != Mathf.Floor(nowLeftPageNum) || rightShowIndex != Mathf.Ceil(nowLeftPageNum))
        {
            leftShowIndex = (int)Mathf.Floor(nowLeftPageNum);
            rightShowIndex = (int)Mathf.Ceil(nowLeftPageNum);
            UpdateChildPageVisableState();
        }
    }
    private void UpdateChildPageVisableState()
    {
        for(int i = 0; i < content.childCount; ++i)
        {
            //正在滑动时候，边界的左边和右边2页都显示、当前页面也显示（因为当前页需要接收滑动事件)
            if(i == leftShowIndex || i == rightShowIndex || i == currentPageIndex)
            {
                content.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                content.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InTabNormalScrollRect : ScrollRect
{
    public delegate bool tabDrag(PointerEventData eventData);
    public tabDrag del_td;
    public tabDrag del_tdbegin;
    public tabDrag del_tdend;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (del_tdbegin != null)
        {
            if (del_tdbegin(eventData))
            {
                return;
            }
        }
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (del_td != null)
        {
            if (del_td(eventData))
            {
                return;
            }
        }
        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        del_tdend?.Invoke(eventData);
        base.OnEndDrag(eventData);
    }
}

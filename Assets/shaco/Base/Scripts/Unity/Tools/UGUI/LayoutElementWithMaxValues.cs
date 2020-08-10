using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[System.Serializable]
public class LayoutElementWithMaxValues : LayoutElement
{
    public float maxHeight;
    public float maxWidth;

    public bool useMaxWidth;
    public bool useMaxHeight;

    bool ignoreOnGettingPreferedSize;

    public override int layoutPriority
    {
        get { return ignoreOnGettingPreferedSize ? -1 : base.layoutPriority; }
        set { base.layoutPriority = value; }
    }

    public override float preferredHeight
    {
        get
        {
            if (useMaxHeight)
            {
                var defaultIgnoreValue = ignoreOnGettingPreferedSize;
                ignoreOnGettingPreferedSize = true;

                var baseValue = LayoutUtility.GetPreferredHeight(transform as RectTransform);

                ignoreOnGettingPreferedSize = defaultIgnoreValue;

                return baseValue > maxHeight ? maxHeight : baseValue;
            }
            else
                return base.preferredHeight;
        }
        set { base.preferredHeight = value; }
    }

    public override float preferredWidth
    {
        get
        {
            if (useMaxWidth)
            {
                var defaultIgnoreValue = ignoreOnGettingPreferedSize;
                ignoreOnGettingPreferedSize = true;

                var baseValue = LayoutUtility.GetPreferredWidth(transform as RectTransform);

                ignoreOnGettingPreferedSize = defaultIgnoreValue;

                return baseValue > maxWidth ? maxWidth : baseValue;
            }
            else
                return base.preferredWidth;
        }
        set { base.preferredWidth = value; }
    }
}
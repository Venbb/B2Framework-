using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace shaco.Instance.RichText
{
    //仅使用在Untiy4.6+版本
    [RequireComponent(typeof(RectTransform))]
    public class RichTextContentComponent : MonoBehaviour
    {
        public RichTextComponent.TextType type = RichTextComponent.TextType.Text;
        public string value = string.Empty;
        public GameObject item = null;
        public bool isReturn = false;
    }
}
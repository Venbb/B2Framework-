using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace shaco.Instance.RichText
{
    //仅使用在Untiy4.6+版本
    [RequireComponent(typeof(RectTransform))]
    public partial class RichTextComponent : MonoBehaviour
    {
        public class Pivot
        {
            static public Vector3 UpperLeft = new Vector3(0, 1);
            static public Vector3 UpperCenter = new Vector3(0.5f, 1);
            static public Vector3 UpperRight = new Vector3(1, 1);
            static public Vector3 MiddleLeft = new Vector3(0, 0.5f);
            static public Vector3 MiddleCenter = new Vector3(0.5f, 0.5f);
            static public Vector3 MiddleRight = new Vector3(1, 0.5f);
            static public Vector3 LowerLeft = new Vector3(0, 0);
            static public Vector3 LowerCenter = new Vector3(0.5f, 0);
            static public Vector3 LowerRight = new Vector3(1, 0);
        }

        static public void SetPivotByLocalPosition(GameObject target, Vector2 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Debug.LogError("SetPivotByLocalPosition error: dose not contain RectTransform !");
                return;
            }

            var pivotOffset = pivot - rectTrans.pivot;
            var sizeTmp = GetRealSize(rectTrans);
            rectTrans.pivot = pivot;
            target.transform.localPosition = new Vector3(
                target.transform.localPosition.x + sizeTmp.x * pivotOffset.x,
                target.transform.localPosition.y + sizeTmp.y * pivotOffset.y,
                target.transform.localPosition.z);
        }

        static public Vector3 ToPivot(TextAnchor anchor)
        {
            var retValue = RichTextComponent.Pivot.UpperLeft;
            switch (anchor)
            {
                case TextAnchor.UpperLeft: retValue = RichTextComponent.Pivot.UpperLeft; break;
                case TextAnchor.UpperCenter: retValue = RichTextComponent.Pivot.UpperCenter; break;
                case TextAnchor.UpperRight: retValue = RichTextComponent.Pivot.UpperRight; break;
                case TextAnchor.MiddleLeft: retValue = RichTextComponent.Pivot.MiddleLeft; break;
                case TextAnchor.MiddleCenter: retValue = RichTextComponent.Pivot.MiddleCenter; break;
                case TextAnchor.MiddleRight: retValue = RichTextComponent.Pivot.MiddleRight; break;
                case TextAnchor.LowerLeft: retValue = RichTextComponent.Pivot.LowerLeft; break;
                case TextAnchor.LowerCenter: retValue = RichTextComponent.Pivot.LowerCenter; break;
                case TextAnchor.LowerRight: retValue = RichTextComponent.Pivot.LowerRight; break;
                default: Debug.LogError("RichTextComponent ToPivot error: unsupport anchor=" + anchor); break;
            }
            return retValue;
        }

        static private void SetLocalPositionByPivot(GameObject target, Vector3 newPosition, Vector3 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Debug.LogError("RichTextComponent SetLocalPositionByPivot error: target dose not contain RectTransform !");
                return;
            }
            var newPivot = new Vector3(pivot.x - rectTrans.pivot.x, pivot.y - rectTrans.pivot.y, pivot.z);
            var sizeTmp = GetRealSize(rectTrans);

            target.transform.localPosition = new Vector3(
                newPosition.x - sizeTmp.x * newPivot.x,
                newPosition.y - sizeTmp.y * newPivot.y,
                newPosition.z);
        }

        static private Vector3 GetLocalPositionByPivot(GameObject target, Vector3 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Debug.LogError("RichTextComponent GetLocalPositionByPivot error: dose not contain RectTransform !");
                return Vector3.zero;
            }
            var newPivot = new Vector3(pivot.x - rectTrans.pivot.x, pivot.y - rectTrans.pivot.y, pivot.z);
            var sizeTmp = GetRealSize(rectTrans);

            return new Vector3(
                rectTrans.localPosition.x + sizeTmp.x * newPivot.x,
                rectTrans.localPosition.y + sizeTmp.y * newPivot.y,
                rectTrans.localPosition.z);
        }

        static private Vector2 GetRealSize(RectTransform transform)
        {
            var retValue = Vector2.zero;
            var textTmp = transform.GetComponent<UnityEngine.UI.Text>();
            if (null == textTmp)
            {
                retValue = transform.sizeDelta;
            }
            else
            {
                retValue = GetTextRealSize(textTmp);
            }
            return retValue;
        }

        static private Vector2 GetTextRealSize(UnityEngine.UI.Text textTarget)
        {
            var retValue = Vector2.zero;
            // if (textTarget.resizeTextForBestFit && textTarget.horizontalOverflow != HorizontalWrapMode.Overflow)
            // {
            //     TextGenerationSettings generationSettings = textTarget.GetGenerationSettings(Vector2.zero);
            //     generationSettings.fontSize = textTarget.cachedTextGeneratorForLayout.fontSizeUsedForBestFit;

            //     retValue.x = textTarget.cachedTextGeneratorForLayout.GetPreferredWidth(textTarget.text, generationSettings) / textTarget.pixelsPerUnit;
            //     retValue.y = textTarget.preferredHeight;
            // }
            // else
            // {
            retValue = new Vector2(textTarget.preferredWidth, textTarget.preferredHeight);
            // }

            if (textTarget.horizontalOverflow == HorizontalWrapMode.Wrap)
            {
                retValue.x = Mathf.Min(retValue.x, textTarget.rectTransform.sizeDelta.x);
            }
            if (textTarget.verticalOverflow == VerticalWrapMode.Truncate)
            {
                retValue.y = Mathf.Min(retValue.y, textTarget.rectTransform.sizeDelta.y);
            }

            return retValue;
        }


        static private string RemoveSubstring(string str, string start, string end, int startIndex = 0)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if ((startIndex < 0 || startIndex > str.Length - 1))
                return str;

            var indexFindStart = string.IsNullOrEmpty(start) ? 0 : str.IndexOf(start, startIndex);
            var indexFindEnd = string.IsNullOrEmpty(end) ? str.Length - 1 : str.IndexOf(end, indexFindStart + (string.IsNullOrEmpty(start) ? 0 : start.Length));
            if (indexFindStart < 0 || indexFindEnd < 0 || indexFindEnd < indexFindStart || indexFindStart == indexFindEnd)
            {
                return str;
            }

            return str.Remove(indexFindStart, indexFindEnd - indexFindStart + end.Length);
        }

        static private string Substring(string str, string start, string end, int offsetIndex = 0)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var indexFindStart = string.IsNullOrEmpty(start) ? 0 : str.IndexOf(start, offsetIndex);
            if (indexFindStart < 0)
            {
                return string.Empty;
            }

            var indexFindEnd = string.IsNullOrEmpty(end) ? str.Length - 1 : str.IndexOf(end, indexFindStart + start.Length);
            if (indexFindEnd < 0)
            {
                return string.Empty;
            }

            //查找下标超出范围
            if (indexFindStart > indexFindEnd)
                return string.Empty;

            int subLength = indexFindEnd - indexFindStart - start.Length + (string.IsNullOrEmpty(end) ? 1 : 0);
            if (subLength < 0)
            {
                throw new System.Exception("ExtensionsString Substring error: str=" + str + " startIndex=" + indexFindStart + " endIndex=" + indexFindEnd + " start=" + start + " end=" + end);
            }
            else if (subLength == 0)
            {
                return string.Empty;
            }
            else
            {
                return str.Substring(indexFindStart + start.Length, subLength);
            }
        }
    }
}
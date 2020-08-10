using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace shaco
{
    [RequireComponent(typeof(RectTransform))]
    public class DockAreaComponent : MonoBehaviour
    {
        private class DockTargetTransfrom
        {
            public Vector3 oldPosition;
            public Vector3 oldScale;
            public Vector2 oldPivot;
            public Vector2 oldSize;
        }

        //每一帧都会检测停靠位置，当前置或者后置节点对象transfrom变化的时候，自身的transfrom都会自动一起变化
        public bool updateInPerFrame = true;

        [HideInInspector]
        public Vector3 margin = Vector3.zero;
        [HideInInspector]
        public TextAnchor dockAnchor;
        [HideInInspector]
        public RectTransform dockTarget;

        private bool _isUpdateLayoutDirty = true;
        private DockTargetTransfrom _prevTargetTransform = new DockTargetTransfrom();
        private Text _textTarget;
        private Text _textMe;
        private RectTransform _rectTransformComponent;

        public void SetUpdateLayoutDirty()
        {
            if (null == dockTarget)
                return;

#if UNITY_EDITOR
            _isUpdateLayoutDirty = true;
            if (!Application.isPlaying)
                UpdateLayout();
#else
            _isUpdateLayoutDirty = true;
#endif
        }

        void Start()
        {
            UpdateLayout();
        }

        public void Update()
        {
            CheckTargetsTransformChanged();
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (!_isUpdateLayoutDirty)
                return;
            _isUpdateLayoutDirty = false;

            _textTarget = dockTarget.GetComponent<Text>();
            _textMe = this.GetComponent<Text>();
            _rectTransformComponent = GetComponent<RectTransform>();

            SetLayoutByTarget(dockTarget, dockAnchor);
            this.transform.localPosition += margin;
        }

        private void SetLayoutByTarget(RectTransform targetDock, TextAnchor anchor)
        {
            var middlePivot = new Vector2(shaco.Pivot.MiddleCenter.x, shaco.Pivot.MiddleCenter.y);
            if (_textMe != null && _rectTransformComponent.pivot != middlePivot)
            {
                Debug.LogWarning("DockAreaWarning SetLayout warning: pivot is not support when target is text, please use alignment instead");
                SetPivotSmart(_rectTransformComponent, middlePivot, true);
            }
            if (_textTarget != null && targetDock.pivot != middlePivot)
            {
                Debug.LogWarning("DockAreaWarning SetLayout warning: pivot is not support when dock target is text, please use alignment instead");
                SetPivotSmart(targetDock, middlePivot, true);
            }

            //因为已经是要移动的对象，所以它的文字锚点就没有意义了，这样也不用计算它的文字真实偏移量
            if (null != _textMe)
            {
                _textMe.alignment = TextAnchor.MiddleCenter;
            }

            var pivotSet = anchor.ToPivot();
            var pivotGet = anchor.ToNegativePivot();
            var fixedPosTmp = UnityHelper.GetWorldPositionByPivot(targetDock.gameObject, pivotGet);
            var fixedOffsetTarget = GetFixedOffsetWhenText(targetDock, _textTarget);

            UnityHelper.SetWorldPositionByPivot(_rectTransformComponent.gameObject, fixedPosTmp, pivotSet);
            _rectTransformComponent.localPosition += fixedOffsetTarget;
        }

        /// <summary>
        /// 更加智能的方式设定锚点
        /// <param name="rect">矩形对象</param>
        /// <param name="pivot">锚点</param>
        /// <param name="smart">是否智能保留原始坐标，如果为false则修改锚点同时会修改坐标</param>
        /// </summary>
        public static void SetPivotSmart(RectTransform rect, Vector2 pivot, bool smart)
        {
            if (rect.pivot == pivot)
                return;
                
            Vector3 cornerBefore = GetRectReferenceCorner(rect);
            rect.pivot = pivot;

            if (smart)
            {
                Vector3 cornerAfter = GetRectReferenceCorner(rect);
                Vector3 cornerOffset = cornerAfter - cornerBefore;
                rect.anchoredPosition -= (Vector2)cornerOffset;

                Vector3 pos = rect.transform.position;
                pos.z -= cornerOffset.z;
                rect.transform.position = pos;
            }
        }

        static Vector3 GetRectReferenceCorner(RectTransform gui)
        {
            return (Vector3)gui.rect.min + gui.transform.localPosition;
        }

        static private Vector3 GetFixedOffsetWhenText(RectTransform targetDock, Text textTarget)
        {
            var retValue = Vector3.zero;
            if (null == textTarget)
                return retValue;

            //horizontal
            switch (textTarget.alignment)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.MiddleLeft:
                case TextAnchor.LowerLeft:
                    {
                        retValue.x = textTarget.preferredWidth * targetDock.pivot.x - targetDock.sizeDelta.x * targetDock.pivot.x;
                        break;
                    }
                case TextAnchor.UpperRight:
                case TextAnchor.MiddleRight:
                case TextAnchor.LowerRight:
                    {
                        retValue.x = targetDock.sizeDelta.x * targetDock.pivot.x - textTarget.preferredWidth * targetDock.pivot.x;
                        break;
                    }
                default: break;
            }

            //vertical
            switch (textTarget.alignment)
            {
                case TextAnchor.LowerLeft:
                case TextAnchor.LowerCenter:
                case TextAnchor.LowerRight:
                    {
                        retValue.y = textTarget.preferredHeight * targetDock.pivot.y - targetDock.sizeDelta.y * targetDock.pivot.y;
                        break;
                    }
                case TextAnchor.UpperLeft:
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperRight:
                    {
                        retValue.y = targetDock.sizeDelta.y * targetDock.pivot.y - textTarget.preferredHeight * targetDock.pivot.y;
                        break;
                    }
                default: break;
            }

            if (targetDock.pivot.x < 0)
            {
                retValue.x = -retValue.x;
            }
            if (targetDock.pivot.y < 0)
            {
                retValue.y = -retValue.y;
            }
            return retValue;
        }

        private void CheckTargetsTransformChanged()
        {
            if (updateInPerFrame)
            {
                bool isTransformChanged = false;
                isTransformChanged |= CheckTargetTransformChanged(_prevTargetTransform);

                if (isTransformChanged)
                {
                    SetUpdateLayoutDirty();
                }
            }
        }

        private bool CheckTargetTransformChanged(DockTargetTransfrom oldTransform)
        {
            bool retValue = false;
            if (null == dockTarget || _rectTransformComponent == null)
                return retValue;

            if (oldTransform.oldPosition != dockTarget.transform.position)
            {
                oldTransform.oldPosition = dockTarget.transform.position;
                retValue = true;
            }

            if (oldTransform.oldScale != dockTarget.transform.localScale)
            {
                oldTransform.oldScale = dockTarget.transform.localScale;
                retValue = true;
            }

            if (oldTransform.oldPivot != dockTarget.pivot)
            {
                oldTransform.oldPivot = dockTarget.pivot;
                retValue = true;
            }

            if (null != _textTarget)
            {
                var size = UnityHelper.GetTextRealSize(_textTarget);
                if (oldTransform.oldSize != size)
                {
                    oldTransform.oldSize = size;
                    retValue = true;
                }
            }

            if (oldTransform.oldSize != dockTarget.sizeDelta)
            {
                oldTransform.oldSize = dockTarget.sizeDelta;
                retValue = true;
            }
            return retValue;
        }
    }
}
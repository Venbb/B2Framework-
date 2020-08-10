using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class TransparentBy : ActionBase
    {
        protected enum DelegateType
        {
            None,
            Image,
            CanvasGroup
        }

        protected float _alpha;
        protected float _alphaEnd;
        protected DelegateType _delegateType = DelegateType.None;

        //alpha range is -1 ~ 1
        static public TransparentBy Create(float alpha, float duration)
        {
            TransparentBy ret = new TransparentBy();
	
			alpha = Mathf.Clamp(alpha, -1, 1);

		    ret._alpha = alpha;
            ret.duration = duration;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
			CheckAlphaDelegate(target);
            float currentAlpha = GetCurrentAlpha(target);

            _alphaEnd = currentAlpha + this._alpha;

            this.onCompleteFunc += (shaco.ActionBase action) =>
            {
                if (_alphaEnd != GetCurrentAlpha(target))
                {
					SetCurrentAlpha(target, _alphaEnd);
                }
            };
            base.RunAction(target);
        }

		public override float UpdateAction(float prePercent)
        {
			var currentAlpha = GetCurrentAlpha(this.target);
            SetCurrentAlpha(this.target, currentAlpha + _alpha * prePercent);

            return base.UpdateAction(prePercent);
        }

        public override ActionBase Clone()
        {
            return TransparentBy.Create(_alpha, duration);
        }

        public override ActionBase Reverse(GameObject target)
        {
            return TransparentBy.Create(-_alpha, duration);
        }

        protected float GetCurrentAlpha(GameObject target)
        {
			var retValue = 0.0f;
			switch (_delegateType)
            {
                case DelegateType.Image:
                    {
                        retValue = target.GetComponent<UnityEngine.UI.Image>().color.a;
                        break;
                    }
                case DelegateType.CanvasGroup:
                    {
                        retValue = target.GetComponent<UnityEngine.CanvasGroup>().alpha;
                        break;
                    }
                default:
                    {
                        shaco.Log.Error("TransparentBy GetCurrentAlpha error: unsupport type", target);
                        break;
                    }
            }
			return retValue;
        }

        protected void SetCurrentAlpha(GameObject target, float alpha)
        {
            switch (_delegateType)
            {
                case DelegateType.Image:
                    {
                        var imageTmp = target.GetComponent<UnityEngine.UI.Image>();
                        imageTmp.color = new Color(imageTmp.color.r, imageTmp.color.g, imageTmp.color.b, alpha);
                        break;
                    }
                case DelegateType.CanvasGroup:
                    {
                        target.GetComponent<UnityEngine.CanvasGroup>().alpha = alpha;
                        break;
                    }
                default:
                    {
						shaco.Log.Error("TransparentBy SetCurrentAlpha error: unsupport type", target);
                        break;
                    }
            }
        }

        protected TransparentBy() { }

        private void CheckAlphaDelegate(GameObject target)
        {
            if (target.GetComponent<UnityEngine.UI.Image>() != null)
            {
                _delegateType = DelegateType.Image;
            }
            else if (target.GetComponent<UnityEngine.CanvasGroup>() != null)
            {
                _delegateType = DelegateType.CanvasGroup;
            }
            else
            {
                shaco.Log.Error("TransparentBy CheckAlphaDelegate error: unsupport type", target);
            }
        }
    }
}
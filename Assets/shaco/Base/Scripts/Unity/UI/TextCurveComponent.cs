using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace shaco
{
    /// <summary>
    /// 通过修改文本定点使其发生扭曲
    /// </summary>
    [AddComponentMenu("UI/Effects/Extensions/Text Curve")]
    public class TextCurveComponent : BaseMeshEffect
    {
        //是否保持文字原样，不发生扭曲
        [SerializeField]
        private bool _keepTextShape = true;
        //曲线类型
        [SerializeField]
        private AnimationCurve _curveForText = AnimationCurve.Linear(0, 0, 1, 10);
        //曲线程度
        [SerializeField]
        private float _curveMultiplier = 1;

        private RectTransform _rectTrans;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (_curveForText[0].time != 0)
            {
                var tmpRect = _curveForText[0];
                tmpRect.time = 0;
                _curveForText.MoveKey(0, tmpRect);
            }
            if (_rectTrans == null)
                _rectTrans = GetComponent<RectTransform>();
            if (_curveForText[_curveForText.length - 1].time != _rectTrans.rect.width)
                OnRectTransformDimensionsChange();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            _rectTrans = GetComponent<RectTransform>();
            OnRectTransformDimensionsChange();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _rectTrans = GetComponent<RectTransform>();
            OnRectTransformDimensionsChange();
        }

        /// <summary>
        /// Modifies the mesh. 最重要的重载函数
        /// </summary>
        /// <param name="mesh">Mesh.</param>
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            // 从mesh 得到 顶点集
            List<UIVertex> verts = new List<UIVertex>();
            vh.GetUIVertexStream(verts);

            // 顶点的 y值按曲线变换
            if (_keepTextShape)
            {
                var startTime = _curveForText[0].time;
                var endTime = _curveForText[_curveForText.keys.Length - 1].time;
                var characterCount = verts.Count <= 6 ? 1 : verts.Count / 6 - 1;
                var perTimeInVerts = (endTime - startTime) / characterCount;

                for (int index = 0; index < verts.Count; ++index)
                {
                    var uiVertex = verts[index];
                    var timeTmp = (index / 6) * perTimeInVerts;
                    var curveValue = _curveForText.Evaluate(timeTmp);
                    uiVertex.position.y += curveValue * _curveMultiplier;
                    verts[index] = uiVertex;
                }
            }
            else
            {
                for (int index = 0; index < verts.Count; ++index)
                {
                    var uiVertex = verts[index];
                    uiVertex.position.y += _curveForText.Evaluate(_rectTrans.rect.width * _rectTrans.pivot.x + uiVertex.position.x) * _curveMultiplier;
                    verts[index] = uiVertex;
                }
            }

            // 在合成mesh
            vh.AddUIVertexTriangleStream(verts);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            var tmpRect = _curveForText[_curveForText.length - 1];
            if (_rectTrans != null)
            {
                tmpRect.time = _rectTrans.rect.width;
                _curveForText.MoveKey(_curveForText.length - 1, tmpRect);
            }
        }
    }
}
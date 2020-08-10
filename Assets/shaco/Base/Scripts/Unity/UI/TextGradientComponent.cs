using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

/// <summary>
/// 渐变字体
/// 已知bug，无法和UGUI的OutLine描边脚本同时使用，后期再考虑优化
/// </summary>
[AddComponentMenu("UI/Effects/Extensions/Text Gradient")]
public class TextGradientComponent : BaseMeshEffect
{
    [SerializeField]
    private Color32 _topColor = Color.white;
    [SerializeField]
    private Color32 _bottomColor = Color.black;
    
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }

        var count = vh.currentVertCount;
        if (count == 0)
            return;

        var vertexs = new List<UIVertex>();
        for (var i = 0; i < count; i++)
        {
            var vertex = new UIVertex();
            vh.PopulateUIVertex(ref vertex, i);
            vertexs.Add(vertex);
        }

        var topY = vertexs[0].position.y;
        var bottomY = vertexs[0].position.y;


        for (var i = 1; i < count; i++)
        {
            var y = vertexs[i].position.y;
            if (y > topY)
            {
                topY = y;
            }
            else if (y < bottomY)
            {
                bottomY = y;
            }

        }

        var height = topY - bottomY;
        for (var i = 0; i < count; i++)
        {
            var vertex = vertexs[i];

            var color = Color32.Lerp(_bottomColor, _topColor, (vertex.position.y - bottomY) / height);

            vertex.color = color;

            vh.SetUIVertex(vertex, i);
        }
    }
}
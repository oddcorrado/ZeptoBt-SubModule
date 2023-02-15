using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZeptoLineUi : Graphic
{
    public List<Vector2> Points { get; set; } = new List<Vector2>();
    public float Width { get; set; } = 1;
    public Color Color { get; set; } = Color.grey;

   protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (Points.Count < 1) return;

        for(int i = 0; i < Points.Count - 1; i++)
        {
            Vector2 norm = Points[i + 1] - Points[i];
            norm = norm.normalized;
            norm = new Vector2(-norm.y, norm.x);

            UIVertex a = UIVertex.simpleVert;
            UIVertex b = UIVertex.simpleVert;
            UIVertex c = UIVertex.simpleVert;
            UIVertex d = UIVertex.simpleVert;
            a.position = Points[i] + Width * norm;
            b.position = Points[i] - Width * norm;
            c.position = Points[i + 1] + Width * norm;
            d.position = Points[i + 1] - Width * norm;
            a.color = b.color = c.color = d.color = Color;
            vh.AddVert(a);
            vh.AddVert(b);
            vh.AddVert(c);
            vh.AddVert(d);
            vh.AddTriangle(0 + i * 4, 1 + i * 4, 2 + i * 4);
            vh.AddTriangle(2 + i * 4, 3 + i * 4, 1 + i * 4);
        } 
    }
}

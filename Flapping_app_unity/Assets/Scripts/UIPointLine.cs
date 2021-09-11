using UnityEngine;
using UnityEngine.UI;
public class UIPointLine : Graphic
{
	public Vector2 position1;
	public Vector2 position2;
    public float weight;
    int div = 180;
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var pos1_to_2 = position2 - position1;
        var verticalVector = CalcurateVerticalVector(pos1_to_2);
        var pos1Top = position1 + verticalVector * - weight / 2;
        var pos1Bottom = position1 + verticalVector * weight / 2;
        var pos2Top = position2 + verticalVector * - weight / 2;
        var pos2Bottom = position2 + verticalVector * weight / 2;
        AddVert(vh, pos1Top);
        AddVert(vh, pos1Bottom);
        AddVert(vh, pos2Top);
        AddVert(vh, pos2Bottom);
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(1, 2, 3);
        DrawCircle(vh, vh.currentVertCount, vh.currentVertCount + 1, position1, pos1Top, Mathf.PI, div);
		DrawCircle(vh, vh.currentVertCount, vh.currentVertCount + 1, position2, pos2Bottom, Mathf.PI, div);
	}
    private void AddVert(VertexHelper vh, Vector2 pos)
    {
        var vert = UIVertex.simpleVert;
        vert.position = pos;
        vert.color = color;
        vh.AddVert(vert);
    }

    private Vector2 CalcurateVerticalVector(Vector2 vec)
    {
        var verticalVector = new Vector2(vec.y, -vec.x);
        return verticalVector.normalized;
    }

	private void Update()
	{
        SetVerticesDirty();
    }

    private int DrawCircle(
            VertexHelper vh,
            int centerVert, int startVert,
            Vector3 centerPos, Vector3 startPos,
            float rad, int div
            )
    {
        AddVert(vh, centerPos);
        AddVert(vh, startPos);
        float divRad = rad / (div + 1);
        Vector3 startVector = startPos - centerPos;
        for (int i = startVert; i < startVert + 1 + div; i++)
        {
            AddVert(vh, new Vector3(
                startVector.x * Mathf.Cos(divRad * (i - startVert + 1)) - startVector.y * Mathf.Sin(divRad * (i - startVert + 1)) + centerPos.x,
                startVector.x * Mathf.Sin(divRad * (i - startVert + 1)) + startVector.y * Mathf.Cos(divRad * (i - startVert + 1)) + centerPos.y
            ));
        }
        for (int i = startVert; i < startVert + 1 + div; i++)
        {
            vh.AddTriangle(centerVert, i + 1, i);
        }
        return startVert + 1 + div;
    }
}
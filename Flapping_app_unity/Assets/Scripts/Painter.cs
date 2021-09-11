using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

public enum DrawingTools
{
    Pen,
    Line,
    Stamp,
    Bucket,
    Eraser
}

public class Painter : MonoBehaviour
{
    [SerializeField] RawImage RightImage = null;
    [SerializeField] RawImage LeftImage = null;
    [SerializeField] Image RightImage0 = null;
    [SerializeField] Image LeftImage0 = null;

    public int Weight;

    [SerializeField]
    RectTransform canvasRect;

    [SerializeField]
    ColorPalette palette;

    [SerializeField]
    ToggleGroup tools;

    public Color baseColor;

    [SerializeField]
    GameObject lineCircle;

    [SerializeField]
    private Canvas canvas;

    public Texture2D selectedStamp;

    private Vector2Int prevMousePos;
    private Texture2D texture = null;
    private Texture2D texture_image = null;
    private Texture2D texture_base = null;

    private bool isDrawing = false;
    private RectTransform rectTransform;
    private Rect rect;
    private Color[] beforePixels;
    private Vector2Int LinePos1, LinePos2;

    public DrawingTools tool;

    public static List<Color[]> textureList;

    bool isInDrawTouch = false;
    public void OnChangeTool(bool c)
	{
		if (c) Enum.TryParse(tools.ActiveToggles().First().gameObject.name, out tool);
	}

    private void Start()
    {
        texture_base = Settings.DrawingFlap ?? Settings.BaseShape;
        texture_image = Settings.BaseShape;
        baseColor = Settings.BaseColor;
        rectTransform = RightImage.gameObject.GetComponent<RectTransform>();
        rect = rectTransform.rect;
        texture = new Texture2D(texture_image.width, texture_image.height, TextureFormat.RGBA32, false);
        texture.SetPixels(texture_base.GetPixels());
        RightImage.texture = texture;
        LeftImage.texture = texture;
		if (Settings.BaseColor.a == 0)
        {
            var sp0 = (Sprite)Resources.Load(Settings.ShapeName + "0", typeof(Sprite));
            RightImage0.sprite = sp0;
            LeftImage0.sprite = sp0;
        }
        else
		{
            RightImage0.gameObject.SetActive(false);
            LeftImage0.gameObject.SetActive(false);
        }
        OnChangeTool(true);
		if (Settings.DrawingFlap == null)
		{
			for (int y = 0; y < texture_image.height; y++)
			{
				for (int x = 0; x < texture_image.width; x++)
				{
					if (texture_image.GetPixel(x, y).a != 0)
					{
						texture.SetPixel(x, y, baseColor);
					}
				}
			}
		}
		texture.Apply();
		if (textureList.Count == 0)
        {
            textureList.Add(texture.GetPixels());
        }
    }

    private void OnDestroy()
    {
        Settings.DrawingFlap = texture;
        //if (texture != null)
        //{
        //    Destroy(texture);
        //    texture = null;
        //}
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {

            var mousePos = Input.mousePosition;
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = mousePos;
            List<RaycastResult> result = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, result);
            Vector3 screenToWorldPointPosition;
            mousePos.z = 0f;
            screenToWorldPointPosition = Camera.main.ScreenToViewportPoint(mousePos);
            mousePos = new Vector2(
                screenToWorldPointPosition.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f
                    + rect.width / 2 - rectTransform.anchoredPosition.x,
                screenToWorldPointPosition.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f
                    + rect.height / 2 - rectTransform.anchoredPosition.y
            );
            var v = InTexForm(new Vector2Int((int)mousePos.x, (int)mousePos.y));
            if ((!isInDrawTouch || result.Count == 0 || result[0].gameObject != gameObject)
                && !isDrawing)
            {
                isInDrawTouch = false;
                return;
            }
            //Debug.Log(v);
            switch (tool)
            {
                case DrawingTools.Pen:
                    DrawPen(v, palette.selectedColor, true);
                    break;
                case DrawingTools.Line:
                    Vector2 mPos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,
                            Input.mousePosition, canvas.worldCamera, out mPos);
                    var lRect = lineCircle.GetComponent<RectTransform>();
                    var visualLine = lineCircle.GetComponentInChildren<UIPointLine>();
                    if (!isDrawing)
                    {
						lineCircle.SetActive(true);
						lRect.anchoredPosition = mPos;
                        visualLine.color = new Color(palette.selectedColor.r, palette.selectedColor.g,
                            palette.selectedColor.b, palette.Alpha);
                        visualLine.weight = Weight * (int)rect.width / texture_image.width;
                        visualLine.position1 = Vector2.zero;
                        LinePos1.x = v.x;
                        LinePos1.y = v.y;
                    }
                    visualLine.position2 = mPos - lRect.anchoredPosition;
                    LinePos2.x = v.x;
                    LinePos2.y = v.y;
                    break;
                case DrawingTools.Stamp:
                    if (!isDrawing)
                    {
                        var sw = 120;
                        var sh = 120;
						var rt = RenderTexture.GetTemporary(sw, sh);
						Graphics.Blit(selectedStamp, rt);
						var preRT = RenderTexture.active;
						RenderTexture.active = rt;
						var stamp = new Texture2D(sw, sh);
						stamp.ReadPixels(new Rect(0, 0, sw, sh), 0, 0);
						stamp.Apply();
						RenderTexture.active = preRT;
						RenderTexture.ReleaseTemporary(rt);
						var stampData = stamp.GetPixels();
                        for (int h = 0; h < sh; h++)
                        {
                            var y = v.y + h - sh / 2;
                            if (y <= 0 || y >= texture_image.height)
                            {
                                continue;
                            }
                            for (int w = 0; w < sw; w++)
                            {
                                var x = v.x + w - sw / 2;
                                if (x > 0 && x < texture_image.width && texture_image.GetPixel(x, y).a != 0 && stampData[h * sw + w].a >= 0.5)
                                {
                                    texture.SetPixel(x, y, palette.AColor(texture.GetPixel(x, y)));
                                }
                            }
                        }
                    }
                    break;
                case DrawingTools.Bucket:
                    beforePixels = texture_image.GetPixels();
                    if (!isDrawing)
                    {
                        paint(v.x, v.y, texture.GetPixel(v.x, v.y), palette.selectedColor);
                    }
                    //var fill_list = new Dictionary<Vector2Int, int>();
                    //var already_list = new List<int>();
                    //for (var i = 0; i < texture_image.height; i++)
                    //{
                    //    for (var j = 0; j < texture_image.width; j++)
                    //    {
                    //        fill_list.Add(new Vector2Int(j + 1, i + 1), 0);
                    //        already_list.Add(0);
                    //    }
                    //}
                    //getFillList(v.x, v.y, texture.GetPixel(v.x, v.y), fill_list, already_list);
                    //fill_list[new Vector2Int(v.x, v.y)] = 1;
                    //var color_fill = Color.red;
                    //foreach(var fill in fill_list)
                    //{
                    //    if (fill.Value == 1)
                    //    {
                    //        texture.SetPixel(fill.Key.x, fill.Key.y, color_fill);
                    //    }
                    //}
                    break;
                case DrawingTools.Eraser:
                    DrawPen(v, baseColor, false);
                    break;
			}
            texture.Apply();
            prevMousePos = v;
            if (v.x > 0 && v.x < texture_image.width &&
                v.y > 0 && v.y < texture_image.height) isDrawing = true;
        }
        else
        {
			if (isDrawing)
            {
				if (lineCircle.activeSelf)
                {
                    DrawPen(LinePos1, palette.selectedColor, true);
                    DrawPen(LinePos2, palette.selectedColor, true);
                    lineCircle.SetActive(false);
                }
                texture.Apply();
				if (!textureList[textureList.Count - 1].SequenceEqual(texture.GetPixels()))
                {
                    Debug.Log("save");
                    textureList.Add(texture.GetPixels());
                }
                isDrawing = false;
            }
            if (!isInDrawTouch)
            {
                isInDrawTouch = true;
            }
        }
    }
    private Vector2 CalcurateVerticalVector(Vector2 vec)
    {
        var verticalVector = new Vector2(vec.y, -vec.x);
        return verticalVector.normalized;
    }

    void DrawPen(Vector2Int v, Color c, bool enableOpacity)
	{
        int weight = Weight;

        var dir = isDrawing ? prevMousePos - v : Vector2.right;
        if (!isDrawing) prevMousePos = v;
        var dist = (int)dir.magnitude;
        dir = dir.normalized;
        var pixs = texture.GetPixels();
        var circlePoints = GetCirclePoint(weight);

		foreach (var cpoint in circlePoints)
		{
			var y = v.y + cpoint.y;
			if (y < 0 || y >= texture_image.height) continue;
			var x = v.x + cpoint.x;
			if (x < 0 || x >= texture_image.width) continue;
			var col = textureList.Last()[y * texture_image.width + x];
            if (col != c && texture_image.GetPixel(x, y).a != 0 && col == pixs[y * texture_image.width + x])
                pixs[y * texture_image.width + x] = enableOpacity ? palette.AColor(col) : c;
        }
        var verticalVector = CalcurateVerticalVector(dir);
		for (float d = 0; d < dist; d += 1 / (Mathf.Abs(dir.x) + Mathf.Abs(dir.y)))
        {
			for (float w = -weight / 2; w < weight / 2; w += 1 / (Mathf.Abs(dir.x) + Mathf.Abs(dir.y)))
			{
				var pos = v + dir * d + verticalVector * w;
				var posint = new Vector2Int((int)pos.x, (int)pos.y);
				var posindex = posint.y * texture_image.width + posint.x;
				if (posint.y < 0 || posint.y >= texture_image.height) continue;
				if (posint.x < 0 || posint.x >= texture_image.width) continue;
				var col = textureList.Last()[posindex];
                if (col != c && texture_image.GetPixel(posint.x, posint.y).a != 0 && col == pixs[posint.y * texture_image.width + posint.x])
                    pixs[posint.y * texture_image.width + posint.x] = enableOpacity ? palette.AColor(col) : c;
            }
			//var pos = v + dir * d;
			//foreach (var cpoint in circlePoints)
			//{
			//	var y = (int)pos.y + cpoint.y;
			//	if (y <= 0 || y >= texture_image.height) continue;
			//	var x = (int)pos.x + cpoint.x;
			//	if (x <= 0 || x >= texture_image.width) continue;
			//	var col = pixs[y * texture_image.width + x];
			//	if (col != c && col.a != 0) pixs[y * texture_image.width + x] = c;
			//}
			//for (int h = -weight / 2; h < weight / 2; h++)
			//{
			//    var y = (int)pos.y + h;
			//    if (y <= 0 || y >= texture_image.height)
			//    {
			//        continue;
			//    }
			//    for (int w = -weight / 2; w < weight / 2; w++)
			//    {
			//        var x = (int)pos.x + w;
			//        if ((new Vector2Int(x, y) - pos).magnitude > weight / 2) continue;
			//        if (x > 0 && x < texture_image.width && pixs[y * texture_image.width + x].a != 0)
			//        {
			//            pixs[y * texture_image.width + x] = c;
			//        }
			//    }
			//}
		}

        texture.SetPixels(pixs);
    }

    List<Vector2Int> GetCirclePoint(int weight)
	{
        var circle = new List<Vector2Int>();
        for (int h = -weight / 2; h < weight / 2; h++)
        {
            for (int w = -weight / 2; w < weight / 2; w++)
            {
                var point = new Vector2Int(w, h);
                if (point.magnitude < weight / 2) circle.Add(point);
            }
        }
        return circle;
    }

    public void OnBack()
	{
		if (textureList.Count > 1)
        {
            Debug.Log(textureList.Last());
            texture.SetPixels(textureList[textureList.Count - 2]);
            textureList.RemoveAt(textureList.Count - 1);
            texture.Apply();
        }
	}

    private Vector2Int InTexForm(Vector2Int v) => new Vector2Int(
        v.x * texture_image.width / (int)rect.width,
        v.y * texture_image.height / (int)rect.height
    );

    private List<Vector2Int> getFillList(int x, int y, Color targetcolor, Dictionary<Vector2Int, int> fill_list, List<int> already_list)
    {
        var newlist = new List<Vector2Int>();

        void AddNewList(int ax, int ay)
        {
            if (fill_list[new Vector2Int(ax, ay)] != 1)
            {
                fill_list[new Vector2Int(ax, ay)] = 1;
                newlist.Add(new Vector2Int(ax, ay));
            }
        }

        for (var i = x; i >= 1; i--) {
            if (i == x) continue;
            if (already_list[(i - 1) + ((y - 1) * texture_image.width)] == 1) break;
            already_list[(i - 1) + ((y - 1) * texture_image.width)] = 1;
            if (!(texture.GetPixel(x, y) == targetcolor)) break;
            AddNewList(i, y);
        }

        for (var i = x; i <= texture_image.width; i++) {
            if (i == x) continue;
            if (already_list[(i - 1) + ((y - 1) * texture_image.width)] == 1) break;
            already_list[(i - 1) + ((y - 1) * texture_image.width)] = 1;
            if (!(texture.GetPixel(x, y) == targetcolor)) break;
            AddNewList(i, y);
        }

        for (var i = y; i >= 1; i--) {
            if (i == y) continue;
            if (already_list[(x - 1) + ((i - 1) * texture_image.width)] == 1) break;
            already_list[(x - 1) + ((i - 1) * texture_image.width)] = 1;
            if (!(texture.GetPixel(x, y) == targetcolor)) break;
            AddNewList(x, i);
        }

        for (var i = y; i <= texture_image.height; i++) {
            if (i == y) continue;
            if (already_list[(x - 1) + ((i - 1) * texture_image.width)] == 1) break;
            already_list[(x - 1) + ((i - 1) * texture_image.width)] = 1;
            if (!(texture.GetPixel(x, y) == targetcolor)) break;
            AddNewList(x, i);
        }

        for (var i = 0; i < newlist.Count; i++) {
            //newlist.AddRange(getFillList(newlist[i].x, newlist[i].y, targetcolor, fill_list, already_list));
        }

        return newlist;
    }

    void paint(int x, int y, Color before, Color after)
    {
        Color b = beforePixels[x * texture.height + y];
        if (b.r != before.r || b.g != before.g || b.b != before.b)
        {
            return;
        }
        texture.SetPixel(x, y, after);
        paint(x + 1, y, before, after);
        paint(x - 1, y, before, after);
        paint(x, y + 1, before, after);
        paint(x, y - 1, before, after);
    }

    private void Onestroke(int x, int y, Color before, Color after)
    {
        for (int i = 0; i < 1000000; i++)
        {
            texture.SetPixel(x, y, after);
            if (texture.GetPixel(x, y - 1) == before)
                y = y - 1;
            else if (texture.GetPixel(x, y + 1) == before)
                y = y + 1;
            else if (texture.GetPixel(x - 1, y) == before)
                x = x - 1;
            else if (texture.GetPixel(x + 1, y) == before)
                x = x + 1;
            else
            {
                if (texture.GetPixel(x - 1, y - 1) == before && (texture.GetPixel(x - 1, y) == after || texture.GetPixel(x, y - 1) == after))
                {
                    x = x - 1;
                    y = y - 1;
                }
                else if (texture.GetPixel(x - 1, y + 1) == before && (texture.GetPixel(x - 1, y) == after || texture.GetPixel(x, y + 1) == after))
                {
                    x = x - 1;
                    y = y + 1;
                }
                else if (texture.GetPixel(x + 1, y - 1) == before && (texture.GetPixel(x + 1, y) == after || texture.GetPixel(x, y - 1) == after))
                {
                    x = x + 1;
                    y = y - 1;
                }
                else if (texture.GetPixel(x + 1, y + 1) == before && (texture.GetPixel(x + 1, y) == after || texture.GetPixel(x, y + 1) == after))
                {
                    x = x + 1;
                    y = y + 1;
                }
                else return;
            }
        }
    }
}
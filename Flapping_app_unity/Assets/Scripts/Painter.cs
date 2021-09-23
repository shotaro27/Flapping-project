using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// 描画ツールの種類
/// </summary>
public enum DrawingTools
{
    Pen,
    Line,
    Stamp,
    Bucket,
    Eraser
}

/// <summary>
/// 描画ツール
/// </summary>
public class Painter : MonoBehaviour
{
    /// <summary>
    /// 右側描画領域
    /// </summary>
    [SerializeField] RawImage RightImage = null;

    /// <summary>
    /// 左側描画領域
    /// </summary>
    [SerializeField] RawImage LeftImage = null;

    /// <summary>
    /// 右側枠
    /// </summary>
    [SerializeField] Image RightImageFrame = null;

    /// <summary>
    /// 左側枠
    /// </summary>
    [SerializeField] Image LeftImageFrame = null;

    /// <summary>
    /// 太さ
    /// </summary>
    public int Weight;

    /// <summary>
    /// キャンバスのRectTransform
    /// </summary>
    [SerializeField]
    RectTransform canvasRect;

    /// <summary>
    /// パレット
    /// </summary>
    [SerializeField]
    ColorPalette palette;

    /// <summary>
    /// 描画ツール選択
    /// </summary>
    [SerializeField]
    ToggleGroup tools;

    /// <summary>
    /// ベース色（素材）
    /// </summary>
    public Color baseColor;

    /// <summary>
    /// 直線描画円
    /// </summary>
    [SerializeField]
    GameObject lineCircle;

    [SerializeField]
    private Canvas canvas;

    /// <summary>
    /// 選択中のスタンプ
    /// </summary>
    public Texture2D selectedStamp;

    /// <summary>
    /// 前のマウス位置
    /// </summary>
    private Vector2Int prevMousePos;

    /// <summary>
    /// 編集するテクスチャ
    /// </summary>
    private Texture2D texture = null;

    /// <summary>
    /// ベースにするテクスチャ
    /// </summary>
    private Texture2D textureBase = null;

    /// <summary>
    /// 保存されているテクスチャ
    /// </summary>
    private Texture2D savedTexture = null;

    /// <summary>
    /// 描画中か
    /// </summary>
    private bool isDrawing = false;

    /// <summary>
    /// 右画像のRectTransform
    /// </summary>
    private RectTransform rectTransform;

    /// <summary>
    /// 右画像のRect
    /// </summary>
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
        savedTexture = Settings.DrawingFlap ?? Settings.BaseShape;
        textureBase = Settings.BaseShape;
        baseColor = Settings.BaseColor;
        rectTransform = RightImage.gameObject.GetComponent<RectTransform>();
        rect = rectTransform.rect;
        texture = new Texture2D(textureBase.width, textureBase.height, TextureFormat.RGBA32, false);
        texture.SetPixels(savedTexture.GetPixels());
        RightImage.texture = texture;
        LeftImage.texture = texture;
		if (Settings.BaseColor.a == 0)
        {
            var sp0 = (Sprite)Resources.Load(Settings.ShapeName + "0", typeof(Sprite));
            RightImageFrame.sprite = sp0;
            LeftImageFrame.sprite = sp0;
        }
        else
		{
            RightImageFrame.gameObject.SetActive(false);
            LeftImageFrame.gameObject.SetActive(false);
        }
        OnChangeTool(true);
		if (Settings.DrawingFlap == null)
		{
			for (int y = 0; y < textureBase.height; y++)
			{
				for (int x = 0; x < textureBase.width; x++)
				{
					if (textureBase.GetPixel(x, y).a != 0)
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
                        visualLine.weight = Weight * (int)rect.width / textureBase.width;
                        visualLine.position1 = Vector2.zero;
                        LinePos1 = v;
                    }
                    visualLine.position2 = mPos - lRect.anchoredPosition;
                    LinePos2 = v;
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
                            if (y <= 0 || y >= textureBase.height)
                            {
                                continue;
                            }
                            for (int w = 0; w < sw; w++)
                            {
                                var x = v.x + w - sw / 2;
                                if (x > 0 && x < textureBase.width && textureBase.GetPixel(x, y).a != 0 && stampData[h * sw + w].a >= 0.5)
                                {
                                    texture.SetPixel(x, y, palette.AColor(texture.GetPixel(x, y)));
                                }
                            }
                        }
                    }
                    break;
                case DrawingTools.Bucket:
                    beforePixels = textureBase.GetPixels();
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
            if (v.x > 0 && v.x < textureBase.width &&
                v.y > 0 && v.y < textureBase.height) isDrawing = true;
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

    /// <summary>
    /// 垂直なベクトルを計算する
    /// </summary>
    /// <param name="vec">基準のベクトル</param>
    /// <returns></returns>
    private Vector2 CalcurateVerticalVector(Vector2 vec)
    {
        var verticalVector = new Vector2(vec.y, -vec.x);
        return verticalVector.normalized;
    }

    /// <summary>
    /// テクスチャ上で有効な位置であるか
    /// </summary>
    /// <param name="pos">指定した位置</param>
    /// <returns></returns>
    bool IsValidPosition(Vector2Int pos)
        => (pos.y < 0 || pos.y >= textureBase.height)
        && (pos.x < 0 || pos.x >= textureBase.width);

    /// <summary>
    /// テクスチャに色をセットする
    /// </summary>
    /// <param name="texturePixels">テクスチャの色データ</param>
    /// <param name="pos">セットする位置</param>
    /// <param name="setColor">セットする色</param>
    /// <param name="isEnabledOpacity">透過するか</param>
    /// <returns>セットされたテクスチャの色データ</returns>
    Color[] SetColorWithTexture(Color[] texturePixels, Vector2Int pos, Color setColor, bool isEnabledOpacity)
	{
        var pixels = texturePixels;
        var posIndex = pos.y * textureBase.width + pos.x;
        if (IsValidPosition(pos)) return null;
        var col = textureList.Last()[posIndex];
        if (col != setColor && textureBase.GetPixel(pos.x, pos.y).a != 0
            && col == pixels[posIndex])
            pixels[posIndex] = isEnabledOpacity ? palette.AColor(col) : setColor;
        return pixels;
    }

    /// <summary>
    /// ペン描画
    /// </summary>
    /// <param name="pointPos">点座標</param>
    /// <param name="penColor">ペンの色</param>
    /// <param name="isEnabledOpacity">透過するか</param>
    void DrawPen(Vector2Int pointPos, Color penColor, bool isEnabledOpacity)
	{
        int weight = Weight;

        var dir = isDrawing ? prevMousePos - pointPos : Vector2.right;
        if (!isDrawing) prevMousePos = pointPos;
        var dist = (int)dir.magnitude;
        dir = dir.normalized;
        var pixs = texture.GetPixels();
        var circlePoints = GetCirclePoint(weight);
        foreach (var cpoint in circlePoints)
		{
            var circlePos = pointPos + cpoint;
            SetColorWithTexture(pixs, circlePos, penColor, isEnabledOpacity);
        }

        var verticalVector = CalcurateVerticalVector(dir);
		for (float d = 0; d < dist; d += 1 / (Mathf.Abs(dir.x) + Mathf.Abs(dir.y)))
        {
			for (float w = -weight / 2; w < weight / 2; w += 1 / (Mathf.Abs(dir.x) + Mathf.Abs(dir.y)))
			{
				var pos = Vector2Int.RoundToInt(pointPos + dir * d + verticalVector * w);
                SetColorWithTexture(pixs, pos, penColor, isEnabledOpacity);
            }
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
        v.x * textureBase.width / (int)rect.width,
        v.y * textureBase.height / (int)rect.height
    );

    //private List<Vector2Int> getFillList(int x, int y, Color targetcolor, Dictionary<Vector2Int, int> fill_list, List<int> already_list)
    //{
    //    var newlist = new List<Vector2Int>();

    //    void AddNewList(int ax, int ay)
    //    {
    //        if (fill_list[new Vector2Int(ax, ay)] != 1)
    //        {
    //            fill_list[new Vector2Int(ax, ay)] = 1;
    //            newlist.Add(new Vector2Int(ax, ay));
    //        }
    //    }

    //    for (var i = x; i >= 1; i--) {
    //        if (i == x) continue;
    //        if (already_list[(i - 1) + ((y - 1) * textureBase.width)] == 1) break;
    //        already_list[(i - 1) + ((y - 1) * textureBase.width)] = 1;
    //        if (!(texture.GetPixel(x, y) == targetcolor)) break;
    //        AddNewList(i, y);
    //    }

    //    for (var i = x; i <= textureBase.width; i++) {
    //        if (i == x) continue;
    //        if (already_list[(i - 1) + ((y - 1) * textureBase.width)] == 1) break;
    //        already_list[(i - 1) + ((y - 1) * textureBase.width)] = 1;
    //        if (!(texture.GetPixel(x, y) == targetcolor)) break;
    //        AddNewList(i, y);
    //    }

    //    for (var i = y; i >= 1; i--) {
    //        if (i == y) continue;
    //        if (already_list[(x - 1) + ((i - 1) * textureBase.width)] == 1) break;
    //        already_list[(x - 1) + ((i - 1) * textureBase.width)] = 1;
    //        if (!(texture.GetPixel(x, y) == targetcolor)) break;
    //        AddNewList(x, i);
    //    }

    //    for (var i = y; i <= textureBase.height; i++) {
    //        if (i == y) continue;
    //        if (already_list[(x - 1) + ((i - 1) * textureBase.width)] == 1) break;
    //        already_list[(x - 1) + ((i - 1) * textureBase.width)] = 1;
    //        if (!(texture.GetPixel(x, y) == targetcolor)) break;
    //        AddNewList(x, i);
    //    }

    //    for (var i = 0; i < newlist.Count; i++) {
    //        //newlist.AddRange(getFillList(newlist[i].x, newlist[i].y, targetcolor, fill_list, already_list));
    //    }

    //    return newlist;
    //}

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
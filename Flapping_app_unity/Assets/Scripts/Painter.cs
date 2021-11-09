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
/// 描画システム
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

    /// <summary>
    /// 線の位置
    /// </summary>
    private Vector2Int LinePos1, LinePos2;

    /// <summary>
    /// 描画ツール
    /// </summary>
    public DrawingTools tool;

    /// <summary>
    /// 履歴
    /// </summary>
    public static List<Color[]> textureHistory;

    /// <summary>
    /// 描画領域にあるか
    /// </summary>
    bool isInDrawTouch = false;

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
            var frameSprite = (Sprite)Resources.Load(Settings.ShapeName + "0", typeof(Sprite));
            RightImageFrame.sprite = frameSprite;
            LeftImageFrame.sprite = frameSprite;
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
		if (textureHistory.Count == 0)
        {
            textureHistory.Add(texture.GetPixels());
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
            var texturePos = ToTexturePos(mousePos);
            if ((!isInDrawTouch || result.Count == 0 || result[0].gameObject != gameObject)
                && !isDrawing)
            {
                isInDrawTouch = false;
                return;
            }
            switch (tool)
            {
                case DrawingTools.Pen:
                    DrawPen(texturePos, palette.selectedColor, true);
                    break;
                case DrawingTools.Line:
                    Vector2 posOnCanvas;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,
                            Input.mousePosition, canvas.worldCamera, out posOnCanvas);
                    var lineCircleRect = lineCircle.GetComponent<RectTransform>();
                    var temporaryLine = lineCircle.GetComponentInChildren<UIPointLine>();
                    if (!isDrawing)
                    {
						lineCircle.SetActive(true);
						lineCircleRect.anchoredPosition = posOnCanvas;
                        temporaryLine.color = new Color(palette.selectedColor.r, palette.selectedColor.g,
                            palette.selectedColor.b, palette.Alpha);
                        temporaryLine.weight = Weight * (int)rect.width / textureBase.width;
                        temporaryLine.position1 = Vector2.zero;
                        LinePos1 = texturePos;
                    }
                    temporaryLine.position2 = posOnCanvas - lineCircleRect.anchoredPosition;
                    LinePos2 = texturePos;
                    break;
                case DrawingTools.Stamp:
                    if (!isDrawing)
                    {
                        var size = new Vector2Int(120, 120);
						var rt = RenderTexture.GetTemporary(size.x, size.y);
						Graphics.Blit(selectedStamp, rt);
						var preRT = RenderTexture.active;
						RenderTexture.active = rt;
						var stamp = new Texture2D(size.x, size.y);
						stamp.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
						stamp.Apply();
						RenderTexture.active = preRT;
						RenderTexture.ReleaseTemporary(rt);
						var stampData = stamp.GetPixels();
                        var texturePixels = texture.GetPixels();
                        for (int h = 0; h < size.y; h++)
                        {
                            for (int w = 0; w < size.x; w++)
                            {
                                if (stampData[h * size.x + w].a < 0.5) continue;
                                var pos = new Vector2Int(texturePos.x + w - size.x / 2, texturePos.y + h - size.y / 2);
                                SetColorWithTexture(texturePixels, pos, palette.selectedColor, true);
                            }
                        }
                        texture.SetPixels(texturePixels);
                    }
                    break;
                case DrawingTools.Bucket:
                    //beforePixels = textureBase.GetPixels();
                    //if (!isDrawing)
                    //{
                    //    paint(v.x, v.y, texture.GetPixel(v.x, v.y), palette.selectedColor);
                    //}
                    break;
                case DrawingTools.Eraser:
                    DrawPen(texturePos, baseColor, false);
                    break;
			}
            texture.Apply();
            prevMousePos = texturePos;
            if (texturePos.x > 0 && texturePos.x < textureBase.width &&
                texturePos.y > 0 && texturePos.y < textureBase.height) isDrawing = true;
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
				if (!textureHistory[textureHistory.Count - 1].SequenceEqual(texture.GetPixels()))
                {
                    Debug.Log("save");
                    if (textureHistory.Count > 9)
                    {
                        textureHistory.RemoveAt(0);
                    }
                    textureHistory.Add(texture.GetPixels());
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
        => (pos.y >= 0 && pos.y < textureBase.height)
        && (pos.x >= 0 && pos.x < textureBase.width);

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
        if (!IsValidPosition(pos)) return null;
        var col = textureHistory.Last()[posIndex];
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

    /// <summary>
    /// 円を描く座標を取得する
    /// </summary>
    /// <param name="diameter">直径</param>
    /// <returns>円を描く座標</returns>
    List<Vector2Int> GetCirclePoint(int diameter)
	{
        var circle = new List<Vector2Int>();
        for (int h = -diameter / 2; h < diameter / 2; h++)
        {
            for (int w = -diameter / 2; w < diameter / 2; w++)
            {
                var point = new Vector2Int(w, h);
                if (point.magnitude < diameter / 2) circle.Add(point);
            }
        }
        return circle;
    }

    /// <summary>
    /// 前のテクスチャに戻る
    /// </summary>
    public void OnBack()
	{
		if (textureHistory.Count > 1)
        {
            Debug.Log(textureHistory.Last());
            texture.SetPixels(textureHistory[textureHistory.Count - 2]);
            textureHistory.RemoveAt(textureHistory.Count - 1);
            texture.Apply();
        }
    }

    /// <summary>
    /// ツールを変える
    /// </summary>
    /// <param name="c">トグル状態</param>
    public void OnChangeTool(bool c)
    {
        if (c) Enum.TryParse(tools.ActiveToggles().First().gameObject.name, out tool);
    }

    /// <summary>
    /// テクスチャ上の座標に変換する
    /// </summary>
    /// <param name="v">RectTransform上の座標</param>
    /// <returns></returns>
    private Vector2Int ToTexturePos(Vector2 v) => new Vector2Int(
        (int)v.x * textureBase.width / (int)rect.width,
        (int)v.y * textureBase.height / (int)rect.height
    );

    /// <summary>
    /// RectTransform上の座標に変換する
    /// </summary>
    /// <param name="v">テクスチャ上の座標</param>
    /// <returns></returns>
    private Vector2Int ToRectPos(Vector2 v) => new Vector2Int(
        (int)v.x * (int)rect.width / textureBase.width,
        (int)v.y * (int)rect.height / textureBase.height
    );

    /// <summary>
    /// Flapの名前設定
    /// </summary>
    /// <param name="s">Flap名前</param>
    public void SetName(string s) => Settings.FlapName = s;
}
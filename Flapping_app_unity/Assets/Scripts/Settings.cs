using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 設定
/// </summary>
public static class Settings
{
    /// <summary>
    /// ベース形の名前
    /// </summary>
    public static string ShapeName;

    /// <summary>
    /// ベース形のテクスチャ
    /// </summary>
    public static Texture2D BaseShape;

    /// <summary>
    /// 描いたFlap
    /// </summary>
    public static Texture2D DrawingFlap;

    /// <summary>
    /// ベース色（素材）
    /// </summary>
    public static Color BaseColor = Color.white;

    /// <summary>
    /// 保存場所
    /// </summary>
    public static int SaveSlot;

    /// <summary>
    /// ID
    /// </summary>
    public static int Id;

    /// <summary>
    /// Flap名前
    /// </summary>
    public static string FlapName;

    /// <summary>
    /// ベース形の選択した番号
    /// </summary>
    public static int shapeToggle = 0;

    /// <summary>
    /// ベース素材の選択した番号
    /// </summary>
    public static int materialToggle = 0;

    /// <summary>
    /// 描画モード
    /// </summary>
    public static DrawMode mode;

    /// <summary>
    /// 形を設定する
    /// </summary>
    /// <param name="s">形名前</param>
    public static void SetShape(string s)
    {
        ShapeName = s;
        BaseShape = (Texture2D)Resources.Load(s, typeof(Texture2D));
    }

    public static List<int> MyFlaps;
}

/// <summary>
/// 描画モード（新規か既存か）
/// </summary>
public enum DrawMode
{
    New,
    Open
}
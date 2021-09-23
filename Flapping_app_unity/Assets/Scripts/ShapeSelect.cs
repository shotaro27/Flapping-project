using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class ShapeSelect : MonoBehaviour
{
    Toggle shapeToggle;
    Toggle materialToggle;
    Toggle[] shapeToggles;
    Toggle[] materialToggles;
    [SerializeField] GameObject shapes;
    [SerializeField] GameObject materials;
    [SerializeField] SceneManagement manager;
    [SerializeField] GameObject alert;
    [SerializeField] Button applyButton;
    void Start()
    {
        shapeToggles = shapes.GetComponentsInChildren<Toggle>();
        shapeToggle = shapeToggles[Settings.shapeToggle];
        foreach (var s in shapeToggles)
		{
            s.onValueChanged.AddListener(ChangeShape);
            s.gameObject.GetComponentsInChildren<Image>()[2].sprite = (Sprite)Resources.Load(s.gameObject.name, typeof(Sprite));
        }
        shapeToggle.isOn = true;
        shapeToggle.gameObject.GetComponentsInChildren<Image>()[1].color = Color.cyan;
        materialToggles = materials.GetComponentsInChildren<Toggle>();
        materialToggle = materialToggles[Settings.materialToggle];
        foreach (var m in materialToggles)
        {
            m.onValueChanged.AddListener(ChangeMaterial);
        }
        materialToggle.isOn = true;
        materialToggle.gameObject.GetComponentsInChildren<Image>()[1].color = Color.cyan;
        ChangeShape(true);
        ChangeMaterial(true);
        Painter.textureList = new List<Color[]>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ChangeShape(bool c)
	{
		if (c)
        {
            shapeToggle = shapes.GetComponent<ToggleGroup>().ActiveToggles().First();
            Settings.SetShape(shapeToggle.gameObject.name);
		}
    }
    void ChangeMaterial(bool c)
    {
        if (c)
        {
            materialToggle = materials.GetComponent<ToggleGroup>().ActiveToggles().First();
            Settings.BaseColor.a = float.Parse(materialToggle.gameObject.name);
        }
    }

    public void SaveChange()
	{
        manager.PutScene("DrawScene");
        alert.SetActive(true);
        applyButton.onClick.AddListener(Save);
	}

    void Save()
    {
        Settings.shapeToggle = Array.IndexOf(shapeToggles, shapeToggle);
        Settings.materialToggle = Array.IndexOf(materialToggles, materialToggle);
        Settings.DrawingFlap = null;
    }
}
public enum DrawMode
{
    New,
    Open
}

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
}
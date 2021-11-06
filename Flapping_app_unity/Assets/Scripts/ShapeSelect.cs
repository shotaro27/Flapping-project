using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public abstract class SettingsToggle
{
    /// <summary>
    /// 親オブジェクト
    /// </summary>
    public GameObject ParentObject;

    /// <summary>
    /// 現在のトグル
    /// </summary>
    protected Toggle currentToggle;

    /// <summary>
    /// トグルの集合
    /// </summary>
    Toggle[] Toggles;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="currentToggleIndex">選択されていたトグル</param>
    public void Init(int currentToggleIndex)
	{
        Toggles = ParentObject.GetComponentsInChildren<Toggle>();
        currentToggle = Toggles[currentToggleIndex];
        foreach (var s in Toggles)
        {
            s.onValueChanged.AddListener(ChangeToggle);
        }
        currentToggle.isOn = true;
        currentToggle.gameObject.GetComponentsInChildren<Image>()[1].color = Color.cyan;
        ChangeToggle(true);
    }

    /// <summary>
    /// トグルを切り変える
    /// </summary>
    /// <param name="c">トグル状態</param>
    void ChangeToggle(bool c)
    {
        if (c)
        {
            currentToggle = ParentObject.GetComponent<ToggleGroup>().ActiveToggles().First();
            SetString(currentToggle.gameObject.name);
        }
    }

    /// <summary>
    /// 選択されたトグルの名前文字列に合わせて処理を行う
    /// </summary>
    /// <param name="currentString">トグルの名前文字列</param>
    abstract protected void SetString(string currentString);
}

public class ShapeToggleController : SettingsToggle
{
    override protected void SetString (string currentString)
    {
        Settings.SetShape(currentString);
    }
}

/// <summary>
/// 形と素材を選択する
/// </summary>
public class ShapeSelect : MonoBehaviour
{
    /// <summary>
    /// 選択されている形のトグル
    /// </summary>
    Toggle shapeToggle;

    /// <summary>
    /// 選択されている素材のトグル
    /// </summary>
    Toggle materialToggle;

    /// <summary>
    /// 形を選択するトグル
    /// </summary>
    Toggle[] shapeToggles;

    /// <summary>
    /// 素材を選択するトグル
    /// </summary>
    Toggle[] materialToggles;

    /// <summary>
    /// 形選択トグルの親要素
    /// </summary>
    [SerializeField] GameObject shapes;

    /// <summary>
    /// 素材選択トグルの親要素
    /// </summary>
    [SerializeField] GameObject materials;

    /// <summary>
    /// シーン制御
    /// </summary>
    [SerializeField] SceneManagement manager;

    /// <summary>
    /// 注意
    /// </summary>
    [SerializeField] GameObject alert;

    /// <summary>
    /// 適用ボタン
    /// </summary>
    [SerializeField] Button applyButton;

    void Start()
    {
        shapeToggles = shapes.GetComponentsInChildren<Toggle>();
        shapeToggle = shapeToggles[Settings.shapeToggle];
        foreach (var s in shapeToggles)
		{
            s.onValueChanged.AddListener(ChangeShape);
        }
        shapeToggle.isOn = true;
        shapeToggle.gameObject.GetComponentsInChildren<Image>()[1].color = Color.cyan;
        ChangeShape(true);

        foreach (var s in shapeToggles)
        {
            s.gameObject.GetComponentsInChildren<Image>()[2].sprite = (Sprite)Resources.Load(s.gameObject.name, typeof(Sprite));
        }

        materialToggles = materials.GetComponentsInChildren<Toggle>();
        materialToggle = materialToggles[Settings.materialToggle];
        foreach (var m in materialToggles)
        {
            m.onValueChanged.AddListener(ChangeMaterial);
        }
        materialToggle.isOn = true;
        materialToggle.gameObject.GetComponentsInChildren<Image>()[1].color = Color.cyan;
        ChangeMaterial(true);

        Painter.textureHistory = new List<Color[]>();
    }

    /// <summary>
    /// 形を変える
    /// </summary>
    /// <param name="c">トグル状態</param>
    void ChangeShape(bool c)
	{
		if (c)
        {
            shapeToggle = shapes.GetComponent<ToggleGroup>().ActiveToggles().First();
            Settings.SetShape(shapeToggle.gameObject.name);
		}
    }

    /// <summary>
    /// 素材を変える
    /// </summary>
    /// <param name="c">トグル状態</param>
    void ChangeMaterial(bool c)
    {
        if (c)
        {
            materialToggle = materials.GetComponent<ToggleGroup>().ActiveToggles().First();
            Settings.BaseColor.a = float.Parse(materialToggle.gameObject.name);
        }
    }

    /// <summary>
    /// 変更を保存する
    /// </summary>
    public void SaveChange()
	{
        manager.PutScene("DrawScene");
        alert.SetActive(true);
        applyButton.onClick.AddListener(Save);
	}

    /// <summary>
    /// 保存
    /// </summary>
    void Save()
    {
        Settings.shapeToggle = Array.IndexOf(shapeToggles, shapeToggle);
        Settings.materialToggle = Array.IndexOf(materialToggles, materialToggle);
        Settings.DrawingFlap = null;
    }
}

/// <summary>
/// 描画モード（新規か既存か）
/// </summary>
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
}
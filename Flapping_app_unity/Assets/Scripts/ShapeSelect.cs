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
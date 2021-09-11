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

public static class Settings
{
    public static string ShapeName;
    public static Texture2D BaseShape;
    public static Texture2D DrawingFlap;
    public static Color BaseColor = Color.white;
    public static int SaveSlot;
    public static string FlapName;
    public static int shapeToggle = 0;
    public static int materialToggle = 0;
    public static DrawMode mode;
    public static void SetShape(string s)
	{
        ShapeName = s;
        BaseShape = (Texture2D)Resources.Load(s, typeof(Texture2D));
    }
}
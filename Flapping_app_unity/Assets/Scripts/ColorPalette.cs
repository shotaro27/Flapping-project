using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ColorPalette : MonoBehaviour
{
    Animator Anim;
    [SerializeField]
    ToggleGroup toggleGroup;
    [SerializeField]
    Image ColorDisplayer;
    [SerializeField]
    Image ColorSlider;
    [SerializeField]
    GameObject ColorMark;
    [SerializeField]
    Painter painter;
    public Color selectedColor;
    public float Alpha;
    void Start()
    {
        Anim = gameObject.GetComponent<Animator>();
        SetColor(true);
        SetOpacity(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowHide(bool t)
    {
        ColorMark.SetActive(!t);
        Anim.SetBool("ColorPalette", t);
    }

    public void SetColor(bool c)
	{
		if (c)
		{
            selectedColor = toggleGroup.ActiveToggles().First().gameObject.GetComponent<Image>().color;
            ColorDisplayer.color = AColor(painter.baseColor);
            ColorSlider.color = selectedColor;
        }
	}

    public void SetOpacity(float a)
	{
        Alpha = a;
        ColorDisplayer.color = AColor(painter.baseColor);
    }

    public Color AColor(Color n) => selectedColor * Alpha + n * (1 - Alpha);
}

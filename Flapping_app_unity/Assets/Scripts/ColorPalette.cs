using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// カラーパレットを制御する
/// </summary>
public class ColorPalette : MonoBehaviour
{
    /// <summary>
    /// パレットのアニメーター
    /// </summary>
    Animator Anim;

    /// <summary>
    /// 色を選択するトグルグループ
    /// </summary>
    [SerializeField] private ToggleGroup toggleGroup;

    /// <summary>
    /// 選択中のカラーの表示
    /// </summary>
    [SerializeField] private Image ColorDisplayer;

    /// <summary>
    /// スライダー
    /// </summary>
    [SerializeField] private Image ColorSlider;

    /// <summary>
    /// Color文字のGameObject
    /// </summary>
    [SerializeField] private GameObject ColorMark;

    /// <summary>
    /// 描画プログラム
    /// </summary>
    [SerializeField] private Painter painter;
    
    /// <summary>
    /// 選択中の色
    /// </summary>
    public Color selectedColor;
    
    /// <summary>
    /// 選択中の透明度
    /// </summary>
    public float Alpha;

    void Start()
    {
        Anim = gameObject.GetComponent<Animator>();
        SetColor(true);
        SetOpacity(1);
    }

    /// <summary>
    /// パレットの表示/非表示
    /// </summary>
    /// <param name="t">トグル状態</param>
    public void ShowHide(bool t)
    {
        ColorMark.SetActive(!t);
        Anim.SetBool("ColorPalette", t);
    }

    /// <summary>
    /// カラー選択
    /// </summary>
    /// <param name="c">トグル状態</param>
    public void SetColor(bool c)
	{
		if (c)
		{
            selectedColor = toggleGroup.ActiveToggles().First().gameObject.GetComponent<Image>().color;
            ColorDisplayer.color = AColor(painter.baseColor);
            ColorSlider.color = selectedColor;
        }
	}

    /// <summary>
    /// 透明度の設定
    /// </summary>
    /// <param name="a">透明度</param>
    public void SetOpacity(float a)
	{
        Alpha = a;
        ColorDisplayer.color = AColor(painter.baseColor);
    }

    /// <summary>
    /// 透明なカラーを不透明なカラーに合成する
    /// </summary>
    /// <param name="n">合成するカラー</param>
    /// <returns>不透明なカラー</returns>
    public Color AColor(Color n) => selectedColor * Alpha + n * (1 - Alpha);
}

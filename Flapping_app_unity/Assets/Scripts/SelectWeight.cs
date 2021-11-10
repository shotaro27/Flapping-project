using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SelectWeight : MonoBehaviour
{
    [SerializeField]
    ToggleGroup selects;
    [SerializeField]
    Image toolImage;
    [SerializeField]
    bool enableImageChanging;
    [SerializeField]
    Sprite[] Images;
    [SerializeField]
    GameObject Bar;
    [SerializeField]
    Painter painter;
    void Start()
    {
        OnChangeSelect(true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnChangeSelect(bool c)
    {
        if (c)
        {
            var selectS = int.Parse(selects.ActiveToggles().First().gameObject.name);
            painter.Weight = selectS;
            if (enableImageChanging) toolImage.sprite = Images[selects.ActiveToggles().First().gameObject.transform.GetSiblingIndex()];
            OnSelected(false);
        }
    }
    public void OnSelected(bool c)
    {
        Bar.GetComponent<CanvasGroup>().alpha = c ? 1 : 0;
        Bar.GetComponent<GraphicRaycaster>().enabled = c;
    }
}

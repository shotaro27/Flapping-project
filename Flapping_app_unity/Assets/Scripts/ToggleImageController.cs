using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleImageController : MonoBehaviour
{
    [SerializeField] Sprite onSprite;
    [SerializeField] Sprite offSprite;
    Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnValueChanged);
        ((Image)toggle.targetGraphic).sprite = toggle.isOn ? onSprite : offSprite;
    }
    void OnValueChanged(bool value) => ((Image)toggle.targetGraphic).sprite = value ? onSprite : offSprite;
}

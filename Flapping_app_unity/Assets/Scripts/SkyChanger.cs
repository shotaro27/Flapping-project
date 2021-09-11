using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SkyChanger : MonoBehaviour
{
    [SerializeField] Material onMaterial;
    [SerializeField] Material offMaterial;
    Toggle toggle;
    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnValueChanged);
        RenderSettings.skybox = toggle.isOn ? onMaterial : offMaterial;
    }
    void OnValueChanged(bool value) => RenderSettings.skybox = value ? onMaterial : offMaterial;
}

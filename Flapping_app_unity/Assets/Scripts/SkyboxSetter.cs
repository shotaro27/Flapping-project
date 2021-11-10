using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SkyboxSetter : MonoBehaviour
{
    [Serializable]
    class SkyboxImageSet
    {
        public Material SkyboxMaterial;
        public Sprite SkyboxImage;
    }
    [SerializeField] List<SkyboxImageSet> Skyboxes;
    [SerializeField] bool isBlack;
    [SerializeField] GameObject black;
    public int DefaultSkyboxState;
    internal int skyboxState;
    Button setter;

    private void Start()
    {
        setter = GetComponent<Button>();
        skyboxState = DefaultSkyboxState;
        ((Image)setter.targetGraphic).sprite = Skyboxes[DefaultSkyboxState].SkyboxImage;
        RenderSettings.skybox = Skyboxes[DefaultSkyboxState].SkyboxMaterial;
        setter.onClick.AddListener(OnChange);
    }

	public void OnChange()
	{
        skyboxState = (skyboxState + 1) % Skyboxes.Count;
        ((Image)setter.targetGraphic).sprite = Skyboxes[skyboxState].SkyboxImage;
        RenderSettings.skybox = Skyboxes[skyboxState].SkyboxMaterial;
		if (isBlack)
		{
            if (skyboxState == 2)
            {
                black.SetActive(true);
            }
            else
			{
                black.SetActive(false);
			}
        }
    }
}
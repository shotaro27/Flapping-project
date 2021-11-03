using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButterflyTrack : MonoBehaviour
{
    public GameObject TargetButterfly;
    [SerializeField] Camera cam;
    [SerializeField] private RectTransform canvasRect;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (TargetButterfly)
		{
            var tarPos = TargetButterfly.transform.position;
            tarPos.y = TargetButterfly.GetComponent<FlapWing>().y;
            var d = Vector3.Distance(cam.transform.position, tarPos);
            Vector2 ViewportPosition = cam.WorldToViewportPoint(tarPos);
            Vector2 WorldObject_ScreenPosition = new Vector2(
            (ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
            (ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));
            GetComponent<RectTransform>().anchoredPosition = WorldObject_ScreenPosition + new Vector2(500 / d, 500 / d);
            transform.localScale = new Vector3(4 / d, 4 / d, 1);
        }
    }

    public void Go()
	{
		if (TargetButterfly)
        {
            FlapController.id = TargetButterfly.GetComponent<FlapWing>().id;
            SceneManager.LoadScene("PictureDetailScene");
        }
	}
}

using UnityEngine;

/// <summary>
/// カメラ表示領域を調整する
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraViewController : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private RectTransform canvasRect;
    public bool isFullScreen { get; set; }
    void Start()
    {
        cam = GetComponent<Camera>();
        isFullScreen = false;
    }

    void Update()
    {
		if (isFullScreen)
		{
            FullScreen();
        }
		else
		{
            SetView();
		}
    }

    void FullScreen()
    {
        var w = canvasRect.rect.width;
        var h = canvasRect.rect.height;
        cam.rect = new Rect(0, 0, w, h);
        isFullScreen = true;
    }

    void SetView()
    {
        var w = canvasRect.rect.width;
        var h = canvasRect.rect.height;
        cam.rect = new Rect(0.5f - 800 / w + 1600 / w * 0.04375f, 0.5f - 450 / h + 900 / h * 0.055f,
            1600 / w * 0.75f, 900 / h * 0.722f);
        isFullScreen = false;
    }
}

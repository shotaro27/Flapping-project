using UnityEngine;

/// <summary>
/// カメラ表示領域を調整する
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraViewController : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private RectTransform canvasRect;
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        var w = canvasRect.rect.width;
        var h = canvasRect.rect.height;
        cam.rect = new Rect(0.5f - 800 / w + 1600 / w * 0.04375f, 0.5f - 450 / h + 900 / h * 0.055f,
            1600 / w * 0.75f, 900 / h * 0.722f);
    }
}

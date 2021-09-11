using UnityEngine;

public class SetCamera : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    Camera cam;
    float baseFOV = 60;
    float baseAspect;
    void Start()
    {
        baseAspect = CalcAspect(1600, 900);
        cam = GetComponent<Camera>();
    }
	private void Update()
    {
        var baseHorizontalFOV = CalcHorizontalFOV(baseFOV, baseAspect);
        var currentAspect = CalcAspect(Screen.width, Screen.height);
        cam.fieldOfView = currentAspect < baseAspect ? CalcVerticalFOV(baseHorizontalFOV, currentAspect) : baseFOV;
    }
	private float CalcAspect(float width, float height)
    {
        return width / height;
    }
    private float CalcHorizontalFOV(float verticalFOV, float aspect)
    {
        return Mathf.Atan(Mathf.Tan(verticalFOV / 2f * Mathf.Deg2Rad) * aspect) * 2f * Mathf.Rad2Deg;
    }
    private float CalcVerticalFOV(float horizontalFOV, float aspect)
    {
        return Mathf.Atan(Mathf.Tan(horizontalFOV / 2f * Mathf.Deg2Rad) / aspect) * 2f * Mathf.Rad2Deg;
    }
}
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraMove : MonoBehaviour
{
    [SerializeField] float speed = 3;
    [SerializeField] float rotateSpeed = 10;
    public bool[] ds = { false, false, false, false, false, false };
    [SerializeField] Text nameText;
    [SerializeField] RectTransform canvasRect;
    //Camera cam;
	private void Start()
    {
        //cam = GetComponent<Camera>();
    }
	void Update()
    {
        if (ds[0]) transform.rotation *= Quaternion.Euler(0, -rotateSpeed * Time.deltaTime, 0);
        if (ds[1]) transform.rotation *= Quaternion.Euler(0, rotateSpeed * Time.deltaTime, 0);
        if (ds[2]) transform.position += transform.forward * speed * Time.deltaTime;
        if (ds[3]) transform.position -= transform.forward * speed * Time.deltaTime;
        if (ds[4]) transform.position += transform.up * speed * Time.deltaTime;
        if (ds[5]) transform.position -= transform.up * speed * Time.deltaTime;
        //Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		//if (Physics.Raycast(ray, out var hit, 10.0f) && hit.collider.gameObject.TryGetComponent(out FlapWing fl))
		//{
  //          nameText.gameObject.SetActive(true);
  //          Vector2 ViewportPosition = cam.WorldToViewportPoint(fl.transform.position);
  //          Vector2 WorldObject_ScreenPosition = new Vector2(
  //              (ViewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
  //              (ViewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f));
  //          nameText.gameObject.GetComponent<RectTransform>().anchoredPosition = WorldObject_ScreenPosition;
  //          nameText.text = fl.flapName;
		//}
		//else
  //      {
  //          nameText.gameObject.SetActive(false);
  //      }
  //名前表示
	}
    public void OnDs(int d) 
    {
        ds[d] = true;
    }
    public void OffDs(int d)
    {
        if (ds[d])
        {
            ds[d] = false;
        }
    }
}

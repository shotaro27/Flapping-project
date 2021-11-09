using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// カメラ移動
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraMove : MonoBehaviour
{
    /// <summary>
    /// 移動速度
    /// </summary>
    [SerializeField] private float speed = 3;

    /// <summary>
    /// 回転速度
    /// </summary>
    [SerializeField] private float rotateSpeed = 10;

    /// <summary>
    /// 入力状態
    /// 左回転、右回転、前、後ろ、上、下
    /// </summary>
    public bool[] ds = { false, false, false, false, false, false };

    [SerializeField] private GameObject goPictureBook;
    [SerializeField] private List<GameObject> inObjs;
    [SerializeField] private List<GameObject> outObjs;
    [SerializeField] private Text nameText;
    bool isMouseDown;
	Camera cam;
	private void Start()
    {
        isMouseDown = false;
        cam = GetComponent<Camera>();
	}
	void Update()
    {
        if (ds[0]) transform.rotation *= Quaternion.Euler(0, -rotateSpeed * Time.deltaTime, 0);
        if (ds[1]) transform.rotation *= Quaternion.Euler(0, rotateSpeed * Time.deltaTime, 0);
        if (ds[2]) transform.position += transform.forward * speed * Time.deltaTime;
        if (ds[3]) transform.position -= transform.forward * speed * Time.deltaTime;
        if (ds[4]) transform.position += transform.up * speed * Time.deltaTime;
        if (ds[5]) transform.position -= transform.up * speed * Time.deltaTime;

		if (Input.GetMouseButton(0))
		{
			if (!isMouseDown)
			{
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 10.0f) && hit.collider.gameObject.TryGetComponent(out FlapWing fl))
                {
                    goPictureBook.SetActive(true);
                    goPictureBook.GetComponent<ButterflyTrack>().TargetButterfly = fl.gameObject;
                    inObjs.ForEach(o => o.SetActive(true));
                    outObjs.ForEach(o => o.SetActive(false));
                    nameText.text = fl.flapName;
                }
                isMouseDown = true;
            }
        }
        else
		{
			if (isMouseDown)
            {
                isMouseDown = false;
            }
		}
	}

    /// <summary>
    /// 選択した方向のフラグを立てる
    /// </summary>
    /// <param name="d">方向</param>
    public void OnDs(int d) 
    {
        ds[d] = true;
    }

    /// <summary>
    /// 選択した方向のフラグを下ろす
    /// </summary>
    /// <param name="d">方向</param>
    public void OffDs(int d)
    {
        if (ds[d])
        {
            ds[d] = false;
        }
    }

    public void BackCamera()
    {
        goPictureBook.SetActive(false);
        outObjs.ForEach(o => o.SetActive(true));
        inObjs.ForEach(o => o.SetActive(false));
    }
}

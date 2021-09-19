using UnityEngine;

public class FlapWing : MonoBehaviour, IIDCountable
{
    public Vector3 diff;
    Vector3 r;
    [SerializeField] float speed = 2f;
    [SerializeField] Material flapMat;
    public Texture2D flapTexture;
    /// <summary>
    /// FlapのIDを取得または設定します。
    /// </summary>
    public int id { get; set; }
    float t = 0;
    public float y;
    public string flapName;
    void Start()
    {
        t = Random.Range(-3f, 3f);
        y = transform.position.y;
        foreach (var mesh in gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            mesh.material = new Material(flapMat);
            mesh.material.mainTexture = flapTexture;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += diff * Time.deltaTime * speed;
        t += Time.deltaTime;
        transform.position = new Vector3(transform.position.x, y - Mathf.Sin(t * 180 * Mathf.Deg2Rad), transform.position.z);
        var tpos = transform.position;
        if (Mathf.Abs(tpos.x) > 10 && tpos.x * diff.x > 0)
        {
            transform.position = new Vector3(Mathf.Sign(tpos.x) * 20 - tpos.x, tpos.y, tpos.z);
            diff = new Vector3(-diff.x, 0, diff.z);
        }
        else if (Mathf.Abs(tpos.z) > 10 && tpos.z * diff.z > 0)
        {
            transform.position = new Vector3(tpos.x, tpos.y, Mathf.Sign(tpos.z) * 20 - tpos.z);
            diff = new Vector3(diff.x, 0, -diff.z);
        }
        r = diff;
		if (diff.magnitude == 0) r = Vector3.back * Mathf.Cos(40 * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(40 * Mathf.Deg2Rad);
        r.y = 0;
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(r), 0.01f);
    }
}

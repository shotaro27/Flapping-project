using UnityEngine;

public class FlapWing : MonoBehaviour, IIDCountable
{
    /// <summary>
    /// 方向（座標差分）
    /// </summary>
    public Vector3 diff;

    /// <summary>
    /// 方向（Flapの向き）
    /// </summary>
    Vector3 r;

    /// <summary>
    /// Flapの速さ
    /// </summary>
    [SerializeField] float speed = 2f;
    
    /// <summary>
    /// Flapのベースマテリアル
    /// </summary>
    [SerializeField] Material flapMat;
    
    /// <summary>
    /// Flapのテクスチャデータ
    /// </summary>
    public Texture2D flapTexture;

    public int id { get; set; }
    
    /// <summary>
    /// Flapが上下するタイミング
    /// </summary>
    float t = 0;

    /// <summary>
    /// Flapの高度
    /// </summary>
    public float y;

    /// <summary>
    /// Flapの名前
    /// </summary>
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
        //位置移動
        transform.position += diff * Time.deltaTime * speed;
        t += Time.deltaTime;
        transform.position = new Vector3(transform.position.x, y - Mathf.Sin(t * 180 * Mathf.Deg2Rad), transform.position.z);

        //折返し処理
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

        //進行方向に向く
        r = diff;
		if (diff.magnitude == 0) r = Vector3.back * Mathf.Cos(40 * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(40 * Mathf.Deg2Rad);
        r.y = 0;
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(r), 0.01f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlapWing : MonoBehaviour
{
    public Vector3 diff;
    Vector3 r;
    [SerializeField] float speed = 2f;
    [SerializeField] Material flapMat;
    public Texture2D flapTexture;
    public int id;
    float t = 0;
    public float y;
    void Start()
    {
        t = Random.Range(-3f, 3f);
        foreach (var mesh in gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            mesh.material = new Material(flapMat);
            mesh.material.mainTexture = flapTexture;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //diff = (diff - transform.position * 0.1f).normalized;
        transform.position += diff * Time.deltaTime * speed;
        t += Time.deltaTime;
        transform.position = new Vector3(transform.position.x, y - Mathf.Sin(t * 180 * Mathf.Deg2Rad), transform.position.z);
        var v = transform.position;
        if (Mathf.Abs(v.x) > 10 && v.x * diff.x > 0)
        {
            transform.position = new Vector3(Mathf.Sign(v.x) * 20 - v.x, v.y, v.z);
            diff = new Vector3(-diff.x, 0, diff.z);
        }
        else if (Mathf.Abs(v.z) > 10 && v.z * diff.z > 0)
        {
            transform.position = new Vector3(v.x, v.y, Mathf.Sign(v.z) * 20 - v.z);
            diff = new Vector3(diff.x, 0, -diff.z);
        }
        r = diff;
		if (diff.magnitude == 0) r = Vector3.back * Mathf.Cos(40 * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(40 * Mathf.Deg2Rad);
        r.y = 0;
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(r), 0.01f);
    }
}

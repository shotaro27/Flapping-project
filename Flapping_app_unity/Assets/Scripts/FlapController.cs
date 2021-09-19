using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class FlapController : MonoBehaviour
{
    public static int id;
    public static bool isNight;
    DataNameIDSet d;
    [SerializeField] GameObject flap;
    Animator anim;
    [SerializeField] Text idText;
    [SerializeField] Text nameText;
    [SerializeField] Material mat;
    [SerializeField] GameObject next;
    [SerializeField] GameObject prev;
    [SerializeField] Toggle sky;
    void Start()
    {
        sky.isOn = isNight;
        d = PictureProvider.dataNameIDSets.First(f => f.id == id);
        idText.text = d.id.ToString();
        nameText.text = d.name;
        var textureData = d.data;
        var flyMaterial = new Material(mat);
        byte[] byte_After = Convert.FromBase64String(textureData);
        Texture2D texture_After = new Texture2D(flyMaterial.mainTexture.width, flyMaterial.mainTexture.height,
                                        TextureFormat.RGBA32, false);
        texture_After.LoadImage(byte_After);
        flyMaterial.mainTexture = texture_After;
        flap.GetComponentsInChildren<MeshRenderer>().ToList().ForEach(f => f.material = flyMaterial);
        anim = flap.GetComponent<Animator>();
        var info = anim.GetCurrentAnimatorStateInfo(0);
        anim.Play(info.shortNameHash, -1, 0.25f);
		if (d.id == 0)
		{
            prev.SetActive(false);
        }
        if (d.id == PictureProvider.dataNameIDSets.Count - 1)
        {
            next.SetActive(false);
        }
    }
    void Update()
    {

    }

    public void FlapToggle(bool t) => anim.SetFloat("FlapSpeed", t ? 1 : 0);
    public void PrevDetail() => ShowDetail(id - 1);
    public void NextDetail() => ShowDetail(id + 1);
    void ShowDetail(int id)
    {
        FlapController.id = id;
        isNight = sky.isOn;
        SceneManager.LoadScene("PictureDetailScene");
    }
}

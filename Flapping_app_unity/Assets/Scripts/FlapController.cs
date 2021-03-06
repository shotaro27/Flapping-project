using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Flapの詳細を表示する
/// </summary>
public class FlapController : MonoBehaviour
{
    /// <summary>
    /// FlapのID
    /// </summary>
    public static int id;

    /// <summary>
    /// 昼夜
    /// </summary>
    public static int SkyboxState;

    /// <summary>
    /// Flapデータ
    /// </summary>
    private DataNameSet d;

    /// <summary>
    /// Flapのゲームオブジェクト
    /// </summary>
    [SerializeField] private GameObject flap;

    /// <summary>
    /// Flapのアニメーション
    /// </summary>
    private Animator anim;

    /// <summary>
    /// ID表示テキスト
    /// </summary>
    [SerializeField] private Text idText;

    /// <summary>
    /// 名前表示テキスト
    /// </summary>
    [SerializeField] private Text nameText;

    /// <summary>
    /// Flapマテリアル
    /// </summary>
    [SerializeField] private Material mat;

    /// <summary>
    /// 次へボタン
    /// </summary>
    [SerializeField] private GameObject next;

    /// <summary>
    /// 前へボタン
    /// </summary>
    [SerializeField] private GameObject prev;

    /// <summary>
    /// 昼夜切り替え
    /// </summary>
    [SerializeField] private SkyboxSetter sky;

    /// <summary>
    /// Flyボタン
    /// </summary>
    [SerializeField] private GameObject fly;

    /// <summary>
    /// ガーデンからの詳細表示か
    /// </summary>
    internal static bool isGardenDetail = false;

    Texture2D flapTexture;
    void Start()
    {
        sky.DefaultSkyboxState = SkyboxState; //昼夜設定
        d = PictureProvider.dataNameIDSets[id]; //Flapデータ取得
        idText.text = id.ToString(); //ID表示
        nameText.text = d.name; //名前表示

        //画像設定
        var textureData = d.data; //画像データ
        var flyMaterial = new Material(mat);
        byte[] byte_After = Convert.FromBase64String(textureData);
        flapTexture = new Texture2D(flyMaterial.mainTexture.width, flyMaterial.mainTexture.height,
                                        TextureFormat.RGBA32, false);
        flapTexture.LoadImage(byte_After);
        flyMaterial.mainTexture = flapTexture;
        flap.GetComponentsInChildren<MeshRenderer>().ToList().ForEach(f => f.material = flyMaterial);

        //アニメーション
        anim = flap.GetComponent<Animator>();
        var info = anim.GetCurrentAnimatorStateInfo(0);
        anim.Play(info.shortNameHash, -1, 0.25f);

        //前へ/次へボタンの表示切り替え
		if ((PictureProvider.isMyFlap && id == Settings.MyFlaps[0])
            || (!PictureProvider.isMyFlap && id == PictureProvider.idOffset))
		{
            prev.SetActive(false);
        }
        if ((PictureProvider.isMyFlap && id == Settings.MyFlaps[Settings.MyFlaps.Count - 1])
            || (!PictureProvider.isMyFlap && id == PictureProvider.Count - 1))
        {
            next.SetActive(false);
        }
		if (ButterflyController.flapIDs.Contains(id))
		{
            fly.SetActive(false);
        }
    }

    /// <summary>
    /// フラップの動きを制御
    /// </summary>
    /// <param name="t">トグル状態</param>
    public void FlapToggle(bool t) => anim.SetFloat("FlapSpeed", t ? 1 : 0);

    /// <summary>
    /// 前の詳細に戻る
    /// </summary>
    public void PrevDetail() => ShowDetail(PictureProvider.isMyFlap ? Settings.MyFlaps[Settings.MyFlaps.IndexOf(id) - 1] : id - 1);

    /// <summary>
    /// 次の詳細に進む
    /// </summary>
    public void NextDetail() => ShowDetail(PictureProvider.isMyFlap ? Settings.MyFlaps[Settings.MyFlaps.IndexOf(id) + 1] : id + 1);

    /// <summary>
    /// 指定されたIDのFlap詳細を表示
    /// </summary>
    /// <param name="id">FlapID</param>
    void ShowDetail(int id)
    {
        FlapController.id = id;
        SkyboxState = sky.skyboxState;
        SceneManager.LoadScene("PictureDetailScene");
    }

    public void Fly()
	{
        Settings.Id = id;
        Settings.DrawingFlap = flapTexture;
        Settings.mode = DrawMode.Open;
	}

    public void BackBook()
	{
		if (isGardenDetail)
        {
            isGardenDetail = false;
            SceneManager.LoadScene("FlyingScene");
            return;
        }
        if (PictureProvider.isMyFlap)
        {
            SceneManager.LoadScene("MyFlapScene");
        }
		else
		{
            SceneManager.LoadScene("DictionaryScene");
		}
	}
}

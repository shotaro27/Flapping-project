using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnitySocketIO;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

[Serializable]
public class Ids
{
    public int id;
}

/// <summary>
/// 図鑑表示
/// </summary>
public class PictureProvider : MonoBehaviour
{
    /// <summary>
    /// Flap表示領域
    /// </summary>
    [SerializeField] GameObject[] flaps;

    /// <summary>
    /// Socket.ioコントローラー
    /// </summary>
    [SerializeField] SocketIOController io;

    /// <summary>
    /// ページネーション
    /// </summary>
    [SerializeField] Pagenation pagenation;

    /// <summary>
    /// テクスチャの大きさ基準
    /// </summary>
    [SerializeField] Texture2D textureReference;

    /// <summary>
    /// 図鑑UI
    /// </summary>
    [SerializeField] GameObject prev, next, load;

    /// <summary>
    /// サーバーから読み込んだFlapデータのリスト
    /// </summary>
    public static Dictionary<int, DataNameSet> dataNameIDSets = new Dictionary<int, DataNameSet>();
    Dictionary<int, DataNameSet> flapDatas;

    [SerializeField] GameObject myFlap;

    /// <summary>
    /// 現在のページ
    /// </summary>
    public static int page = 1;

    /// <summary>
    /// 最後のページ
    /// </summary>
    int lastPage = 1;

    /// <summary>
    /// myflapで開いているか
    /// </summary>
    public static bool isMyFlap = false;

    public static int idOffset = 0;

    public static int Count;

    List<int> myFlaps;

    [SerializeField]
    int idRange = 90;

    void Start()
    {
        io.On("connect", e =>
        {
            Debug.Log("SocketIO connected");
            var dcount = dataNameIDSets.Count == 0 ? idOffset : dataNameIDSets.OrderByDescending(d => d.Key).First().Key + 1;
            io.Emit("getFlapData", JsonUtility.ToJson(new Ids { id = dcount }));
        });
        io.Connect();
        io.On("flapCount", e =>
        {
            Count = int.Parse(e.data);
            idOffset = Math.Max(Count - idRange, 0);
            var dcount = dataNameIDSets.Count == 0 ? idOffset : dataNameIDSets.OrderByDescending(d => d.Key).First().Key + 1;
            if (FlapController.isGardenDetail && Count <= dcount)
            {
                SceneManager.LoadScene("PictureDetailScene");
            }
            dataNameIDSets = dataNameIDSets.Where(d => d.Key >= idOffset).ToDictionary(d => d.Key, d => d.Value);
        });
        io.On("flapData", e =>
        {
            var d = e.EscapeAndFromJson<DataNameIDSet>();
            if (d.id < idOffset) return;
            lastPage = (int)Mathf.Ceil(d.id / 6f);
            var dat = d.data;
            dataNameIDSets[d.id] = new DataNameSet(){data = dat, name = d.name};
            if (d.id - idOffset >= page * 6 - 1 && load.activeSelf)
            {
                SetPage(page);
            }
			if (FlapController.isGardenDetail && d.id == Count - 1)
			{
                SceneManager.LoadScene("PictureDetailScene");
            }
            pagenation.SetPagenation(page, lastPage, prev, next);
        });
        io.On("sendFlapData", e =>
        {
            SetPage(page);
        });
		if (!isMyFlap && !Settings.MyFlaps.Any())
		{
            myFlap.SetActive(false);
		}
    }

    /// <summary>
    /// Flapデータを表示する
    /// </summary>
    /// <param name="page">ページ</param>
    void SetFlapData(int page)
    {
        myFlaps = Settings.MyFlaps.Where(f => f >= idOffset).ToList();
        flapDatas = isMyFlap ? dataNameIDSets.Where(d => myFlaps.Contains(d.Key)).ToDictionary(d => d.Key, d => d.Value)
            : dataNameIDSets;
        var flag = false;
        Debug.Log(flapDatas.Count);
        for (int i = page * 6 - 6; i < page * 6; i++)
        {
			if ((isMyFlap && myFlaps.Count <= i) ||
                (!isMyFlap && !flapDatas.Select(f => f.Key).Contains(i + idOffset)) || flag)
            {
                flaps[i % 6].SetActive(false);
                flag = true;
                continue;
            }
            var id = isMyFlap ? myFlaps[i] : i + idOffset;
            Debug.Log(flapDatas[id].name);
            var d = flapDatas[id];
            var dat = d.data;
            var bytes = Convert.FromBase64String(dat);
            var t = new Texture2D(textureReference.width, textureReference.height, TextureFormat.RGBA32, false);
            t.LoadImage(bytes);
            var sp = Sprite.Create(t, new Rect(0, 0, t.width, t.height), Vector2.zero);
            flaps[i % 6].SetActive(true);
            flaps[i % 6].transform.GetChild(1).GetComponent<Image>().sprite = sp;
            flaps[i % 6].transform.GetChild(2).GetComponent<Image>().sprite = sp;
        }
        pagenation.SetPagenation(page, (int)Mathf.Ceil(flapDatas.Count / 6f), prev, next);
        load.SetActive(false);
    }

	private void OnDestroy()
	{
        io.Close();
	}

    /// <summary>
    /// ページを設定する
    /// </summary>
    /// <param name="page">ページ</param>
	internal void SetPage(int page)
    {
		if (!FlapController.isGardenDetail)
        {
            PictureProvider.page = page;
            SetFlapData(page);
        }
    }

    /// <summary>
    /// 前のページへ
    /// </summary>
    public void Prev() => SetPage(page - 1);

    /// <summary>
    /// 次のページへ
    /// </summary>
    public void Next() => SetPage(page + 1);

    /// <summary>
    /// 最初のページへ
    /// </summary>
    public void First() => SetPage(1);

    /// <summary>
    /// 最後のページへ
    /// </summary>
    public void Last() => SetPage(lastPage);

    /// <summary>
    /// 詳細を表示する
    /// </summary>
    /// <param name="id">FlapのID</param>
    public void ShowDetail(int id)
	{
        FlapController.id = isMyFlap ? myFlaps[page * 6 - 6 + id] : page * 6 - 6 + id + idOffset;
        SceneManager.LoadScene("PictureDetailScene");
	}

    public void GoMyflap()
	{
        isMyFlap = true;
        page = 1;
        SceneManager.LoadScene("MyFlapScene");
    }

    public void BackMyflap()
    {
        isMyFlap = false;
        page = 1;
        SceneManager.LoadScene("DictionaryScene");
    }
}

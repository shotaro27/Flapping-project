﻿using System.Collections.Generic;
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
    public static List<DataNameIDSet> dataNameIDSets = new List<DataNameIDSet>();
    
    /// <summary>
    /// 現在のページ
    /// </summary>
    public static int page = 1;

    /// <summary>
    /// 最後のページ
    /// </summary>
    int lastPage = 1;

    void Start()
    {
        lastPage = (int)Mathf.Ceil((dataNameIDSets.Count - 1) / 6f);
        io.On("connect", e =>
        {
            Debug.Log("SocketIO connected");
            io.Emit("getFlapData", JsonUtility.ToJson(new Ids { id = dataNameIDSets.Count }));
        });
        io.Connect();
        io.On("flapData", e =>
        {
            var d = e.EscapeAndFromJson<DataNameIDSet>();
            var dat = d.data;
            if (!dataNameIDSets.Exists(f => f.id == d.id))
            {
                dataNameIDSets.Add(d);
            }
            lastPage = (int)Mathf.Ceil(d.id / 6f);
            if (d.id >= page * 6 - 1 && load.activeSelf)
            {
                SetPage(page);
            }
            pagenation.SetPagenation(page, lastPage, prev, next);
        });
        io.On("sendFlapData", e =>
        {
            SetPage(page);
        });
    }

    /// <summary>
    /// Flapデータを表示する
    /// </summary>
    /// <param name="page">ページ</param>
    void SetFlapData(int page)
    {
        var flag = false;
        for (int i = page * 6 - 6; i < page * 6; i++)
        {
            var e = dataNameIDSets.Exists(f => f.id == i);
            if (!e || flag)
            {
                flaps[i % 6].SetActive(false);
                flag = true;
                continue;
            }
            var d = dataNameIDSets.First(f => f.id == i);
            var dat = d.data;
            var bytes = Convert.FromBase64String(dat);
            var t = new Texture2D(textureReference.width, textureReference.height, TextureFormat.RGBA32, false);
            t.LoadImage(bytes);
            var sp = Sprite.Create(t, new Rect(0, 0, t.width, t.height), Vector2.zero);
            flaps[d.id % 6].SetActive(true);
            flaps[d.id % 6].transform.GetChild(1).GetComponent<Image>().sprite = sp;
            flaps[d.id % 6].transform.GetChild(2).GetComponent<Image>().sprite = sp;
        }
        pagenation.SetPagenation(page, lastPage, prev, next);
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
        PictureProvider.page = page;
        SetFlapData(page);
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
        FlapController.id = page * 6 - 6 + id;
        SceneManager.LoadScene("PictureDetailScene");
	}
}

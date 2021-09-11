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

public class PictureProvider : MonoBehaviour
{
    [SerializeField] GameObject[] flaps;
    [SerializeField] SocketIOController io;
    [SerializeField] Texture2D textureReference;
    [SerializeField] GameObject prev, next, load;
    public static List<DataNameIDSet> dataNameIDSets = new List<DataNameIDSet>();
    public static int page = 1;
    void Start()
    {
        io.On("connect", e =>
        {
            Debug.Log("SocketIO connected");
            io.Emit("getFlapData", JsonUtility.ToJson(new Ids { id = dataNameIDSets.Count }));
            //GetFlapData(dataNameIDSets.Count);
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
            if (d.id >= page * 6 - 1 && load.activeSelf)
            {
                SetPage(page);
            }
    //        if (string.IsNullOrEmpty(dat))
    //        {
				//if (load.activeSelf)
    //            {
    //                SetPage(page);
    //            }
    //        }
    //        else
    //        {
    //            GetFlapData(d.id + 1);
    //            if (!dataNameIDSets.Exists(f => f.id == d.id))
    //            {
    //                dataNameIDSets.Add(d);
    //            }
				//if (d.id == page * 6 - 1)
    //            {
    //                SetPage(page);
    //            }
    //        }
        });
        io.On("sendFlapData", e =>
        {
            SetPage(page);
        });
    }
    //void GetFlapData(int id)
    //{
    //    io.Emit("getFlapData", JsonUtility.ToJson(new Ids { id = id }));
    //}

    void SetFlapData(int page)
    {
        var flag = false;
        prev.SetActive(true);
        next.SetActive(true);
        if (page == 1) prev.SetActive(false);
        for (int i = page * 6 - 6; i < page * 6; i++)
        {
            var e = dataNameIDSets.Exists(f => f.id == i);
            if (!e || flag)
            {
                flaps[i % 6].SetActive(false);
                next.SetActive(false);
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
        load.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {

    }
	private void OnDestroy()
	{
        io.Close();
	}
	void SetPage(int page)
    {
        PictureProvider.page = page;
        SetFlapData(page);
    }
    public void Prev() => SetPage(page - 1);
    public void Next() => SetPage(page + 1);
    public void ShowDetail(int id)
	{
        FlapController.id = page * 6 - 6 + id;
        SceneManager.LoadScene("PictureDetailScene");
	}
}

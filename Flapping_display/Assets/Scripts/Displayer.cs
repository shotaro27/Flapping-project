using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnitySocketIO;
using UnitySocketIO.Events;
using SocketIOJsonObjects;
using Newtonsoft.Json;
using System.Linq;

/// <summary>
/// Flapを管理する
/// </summary>
[Serializable]
public class Flap
{
    public int id;
    public GameObject obj;
}

/// <summary>
/// Flapを表示する
/// </summary>
public class Displayer : MonoBehaviour
{
    /// <summary>
    /// Flapのデフォルトテクスチャ
    /// </summary>
    [SerializeField]
    private Texture2D normalTexture;

    /// <summary>
    /// Flapのマテリアル
    /// </summary>
    [SerializeField]
    private Material flyMaterial;

    /// <summary>
    /// 名前を表示するテキスト(確認用)
    /// </summary>
    [SerializeField]
    private Text nameText;

    /// <summary>
    /// Socket.ioコントローラー
    /// </summary>
    [SerializeField] SocketIOController io;

    /// <summary>
    /// Flapのプレハブ
    /// </summary>
    [SerializeField]
    private GameObject FlapPrefab;
	
    /// <summary>
    /// Flapのリスト
    /// </summary>
    readonly List<Flap> flaps = new List<Flap>();
	
    /// <summary>
    /// 追加するFlapデータのリスト
    /// </summary>
    readonly List<FlapDataSet> addingFlapDatas = new List<FlapDataSet>();

    [SerializeField]
    int idRange = 90;

    private void Start()
    {
        io.On("connect", (SocketIOEvent e) =>
        {
            Debug.Log("SocketIO connected");
        });
        io.Connect();
        io.On("emit_to_garden", (SocketIOEvent e) =>
        {
			var f = e.EscapeAndFromJson<FlapDataSet>();
            addingFlapDatas.Add(f);
        });
        io.On("add_to_garden", (SocketIOEvent e) =>
        {
            var f = e.EscapeAndFromJson<FlapDataSet>();
            addingFlapDatas.Add(f);
            var oldFlap = flaps.FirstOrDefault(fl => fl.id == f.id - idRange) ?? flaps[0];
            flaps.Remove(oldFlap);
			var i = new FlapId { id = oldFlap.id };
			io.Emit("removeFlap", JsonConvert.SerializeObject(i));
            Destroy(oldFlap.obj);
        });
        io.On("emit_from_server", (SocketIOEvent e) =>
        {
            string msgstring = e.data;
            Debug.Log("WebSocket received message: " + msgstring);
        });
        io.On("accessFromViewer", (SocketIOEvent e) =>
        {
            var id = EscapeTrim(e);
            Debug.Log("access");
            flaps.ForEach(EmitPosDiff);
			var i = new SocketId { id = id };
			io.Emit("sendFlapDatas", JsonConvert.SerializeObject(i));
        });
    }
    string EscapeTrim(SocketIOEvent e)
    {
        var ed = Regex.Unescape(e.data);
        ed = ed.Substring(1, ed.Length - 2);
        return ed;
    }

    /// <summary>
    /// 位置方向を送信する
    /// </summary>
    /// <param name="f">Flapのgameobject</param>
    void EmitPosDiff(Flap f)
	{
        var wing = f.obj.GetComponent<FlapWing>();
        var p = new FlapPositionSet
        {
            id = f.id,
            pos = f.obj.transform.position
        };
        p.pos.y = wing.altitude;
        io.Emit("setPos", JsonUtility.ToJson(p));
        var d = new FlapPositionSet
        {
            id = f.id,
            pos = wing.diff
        };
        io.Emit("setDiff", JsonUtility.ToJson(d));
    }

    private void OnDestroy()
    {
        flyMaterial.mainTexture = normalTexture;
        io.Close();
    }

    private void Update()
    {
		if (addingFlapDatas.Count > 0)
        {
            var dat = addingFlapDatas[0];
            var textureData = dat.data;
            var name = dat.name;
            byte[] byte_After = Convert.FromBase64String(textureData);
            Texture2D texture_After = new Texture2D(flyMaterial.mainTexture.width, flyMaterial.mainTexture.height,
                                            TextureFormat.RGBA32, false);
            texture_After.LoadImage(byte_After);
            CreateFlap(texture_After, dat.id);
            nameText.text = name;
            addingFlapDatas.RemoveAt(0);
        }
    }

    /// <summary>
    /// フラップ生成
    /// </summary>
    /// <param name="texture">フラップ画像データ</param>
    /// <param name="id">フラップのID</param>
    void CreateFlap(Texture2D texture, int id)
	{
        var flapObj = Instantiate(FlapPrefab, Vector3.zero, Quaternion.identity);
        var f = flapObj.GetComponent<FlapWing>();
        f.flapTexture = texture;
        var diff = new Vector3(UnityEngine.Random.Range(-3f, 3f), 0, UnityEngine.Random.Range(-3f, 3f)).normalized;
        flapObj.transform.position += Vector3.forward * UnityEngine.Random.Range(-1f, 9f);
        flapObj.transform.position += Vector3.right * UnityEngine.Random.Range(-8f, 8f);
        f.altitude = UnityEngine.Random.Range(-3f, 3f);
        f.diff = diff;
        var flap = new Flap()
        {
            id = id,
            obj = flapObj
        };
        flaps.Add(flap);
        EmitPosDiff(flap);
        io.Emit("addFlap", JsonConvert.SerializeObject(new FlapId { id = id }));
    }

    public void Send()
    {
		foreach (var f in flaps)
		{
            var set = new FlapPositionSet();
            set.id = f.id;
            set.pos = f.obj.transform.position;
            var s = JsonConvert.SerializeObject(set);
            Debug.Log(s);
			io.Emit("emit_from_client", s);
		}
	}
}
public static class SocketIOEventEx
{
    /// <summary>
    /// エスケープを外し両端を切る(javascriptで送信するときに両側につくダブルクォーテーションを取る)
    /// </summary>
    /// <param name="e">Socket.ioイベント</param>
    /// <returns>整理された文字列</returns>
    public static string EscapeTrim(this SocketIOEvent e)
    {
        var ed = Regex.Unescape(e.data);
        ed = ed.Substring(1, ed.Length - 2);
        return ed;
    }

    /// <summary>
    /// エスケープしてJSONをObjectに変換する
    /// </summary>
    /// <typeparam name="T">変換したいObjectタイプ</typeparam>
    /// <param name="e">Socket.ioイベント</param>
    /// <returns>変換されたObject</returns>
    public static T EscapeAndFromJson<T>(this SocketIOEvent e) => JsonConvert.DeserializeObject<T>(EscapeTrim(e));
}
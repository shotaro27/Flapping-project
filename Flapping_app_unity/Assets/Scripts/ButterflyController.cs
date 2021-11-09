using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using UnitySocketIO;
using UnitySocketIO.Events;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.InteropServices;

/// <summary>
/// IDを含む
/// </summary>
interface IIDCountable
{
    /// <summary>
    /// FlapのIDを取得または設定します。
    /// </summary>
	int id { get; set; }
}

/// <summary>
/// IDとFlap画像データ、名前のセット
/// </summary>
public struct DataNameIDSet : IIDCountable
{
    public int id { get; set; }
    public string data;
    public string name;
}

/// <summary>
/// IDと位置及び方向のセット
/// </summary>
public struct IDPositionSet : IIDCountable
{
    public int id { get; set; }
    public Vector3 pos;
}

/// <summary>
/// サーバーから取得したFlapを羽ばたかせる
/// </summary>
public class ButterflyController : MonoBehaviour
{
    /// <summary>
    /// FlapのベースMaterial
    /// </summary>
    [SerializeField] private Material flyMaterial;

    /// <summary>
    /// flyMaterialに標準で適用されるTexture2D
    /// </summary>
    [SerializeField] private Texture2D normalTexture;

    /// <summary>
    /// Socket.ioコントローラー
    /// </summary>
    [SerializeField] private SocketIOController io;

    /// <summary>
    /// FlapのベースGameObject
    /// </summary>
    [SerializeField] private GameObject Flap;

    /// <summary>
    /// ローディングのGameObject
    /// </summary>
    [SerializeField] private GameObject load;

    /// <summary>
    /// myFlapをセットするか
    /// </summary>
    [SerializeField] private bool forceSetMyFlap;

    [SerializeField] private List<int> myFlapList;

    /// <summary>
    /// サーバーから受信したFlapデータのリスト
    /// </summary>
    private List<DataNameIDSet> flaps = new List<DataNameIDSet>();

    /// <summary>
    /// サーバーから受信した位置データのリスト
    /// </summary>
    private List<IDPositionSet> positions = new List<IDPositionSet>();

    /// <summary>
    /// サーバーから受信した方向データのリスト
    /// </summary>
    public List<IDPositionSet> diffs = new List<IDPositionSet>();

    /// <summary>
    /// Flap個々のGameObjectのリスト
    /// </summary>
    private List<GameObject> flapobjs = new List<GameObject>();

    /// <summary>
    /// Socket.ioで受信したデータを、既にIDが存在していない場合のみリストに追加する
    /// </summary>
    /// <param name="e">Socket.ioの受信イベント</param>
    /// <param name="findingList">IDを探す配列</param>
    /// <param name="addingList">データを追加する配列</param>
    private void AddID<T, S>(SocketIOEvent e, List<T> findingList, List<S> addingList) where T: IIDCountable where S: IIDCountable
    {
        var f = e.EscapeAndFromJson<S>();
        Debug.Log(e.data);
        if (findingList.Where(fl => fl.id == f.id).Count() == 0)
        {
            addingList.Add(f);
        }
    }

    public void RemoveAll()
    {
        var ob = flapobjs[0];
        flapobjs.RemoveAt(0);
        Destroy(ob);
        positions.RemoveAt(0);
        diffs.RemoveAt(0);
    }
    private void Start()
    {
		if (forceSetMyFlap) Storage.SetLocalStorage("MyFlap", JsonConvert.SerializeObject(myFlapList));
        var stString = Storage.GetLocalStorage("MyFlap");
        var myFlaps = JsonConvert.DeserializeObject<List<int>>(string.IsNullOrEmpty(stString) ? "[]" : stString);
        Settings.MyFlaps = myFlaps;
        Debug.Log(stString);
        io.On("connect", e =>
        {
            Debug.Log("SocketIO connected");
            io.Emit("emit_from_viewer"); //Flapビューエリアへのアクセスを報告→データ受信を待つ
        });

        io.Connect();

        //Flapデータ(画像及び名前、位置、方向)の受信
        io.On("data_set", e => AddID(e, flapobjs.Select(fl => fl.GetComponent<FlapWing>()).ToList(), flaps));
        io.On("pos_set", e => AddID(e, positions, positions));
        io.On("diff_set", e => AddID(e, diffs, diffs));

        //io.On("emit_from_server", e => {
        //    string msgstring = e.data;
        //    Debug.Log("WebSocket received message: " + msgstring);
        //});

        //Flap削除
        io.On("remove_flap", e => {
            var id = int.Parse(e.data);
            var oldfl = flapobjs.FirstOrDefault(fl => fl.GetComponent<FlapWing>().id == id); //削除対象の古いFlap
            flapobjs.Remove(oldfl);
            Destroy(oldfl);
            positions.RemoveAll(p => p.id == id);
            diffs.RemoveAll(d => d.id == id);
        });
    }

    //flyMaterialを初期化してSocket.ioを閉じる。閉じないとサーバーに負担がかかり続ける
    private void OnDestroy()
    {
        flyMaterial.mainTexture = normalTexture;
        io.Close();
    }

    private void Update()
    {
        while (flaps.Count > 0)
        {
            Debug.Log("create");
            DataNameIDSet dat = flaps[0]; //新規Flapデータ
            var textureData = dat.data; //画像データ
            byte[] byte_After = Convert.FromBase64String(textureData);
            Texture2D texture_After = new Texture2D(flyMaterial.mainTexture.width, flyMaterial.mainTexture.height,
                                            TextureFormat.RGBA32, false);
            texture_After.LoadImage(byte_After); //画像データをTexture2dに格納する
            CreateFlap(texture_After, dat.id, dat.name);
            flaps.RemoveAt(0);
            load.SetActive(false);
        }
    }

    /// <summary>
    /// Flapを作成する
    /// </summary>
    /// <param name="texture">Flap画像データ</param>
    /// <param name="id">FlapのID</param>
    /// <param name="name">Flapの名前</param>
    private void CreateFlap(Texture2D texture, int id, string name)
    {
        var flap = Instantiate(Flap); //GameObject作成
        var fl = flap.GetComponent<FlapWing>(); //Flap羽コントローラー
        fl.flapTexture = texture;
        fl.id = id;
        fl.flapName = name;
        var flpos = positions.Where(f => f.id == id).Select(f => f.pos).FirstOrDefault(); //位置
        flap.transform.position = flpos;
        fl.y = flpos.y;
        fl.diff = diffs.Where(f => f.id == id).Select(f => f.pos).FirstOrDefault();
        flapobjs.Add(flap);
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
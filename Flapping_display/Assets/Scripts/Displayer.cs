using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnitySocketIO;
using UnitySocketIO.Events;
using System.Linq;

[Serializable]
public struct DataNameIDSet
{
    public string data;
    public string name;
    public int id;
}

[Serializable]
public struct IDPositionSet
{
    public int id;
    public Vector3 pos;
}
[Serializable] public struct Ids { public string id; }
[Serializable] public struct Ints { public int id; }

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string dummy_json = $"{{\"{DummyNode<T>.ROOT_NAME}\": {json}}}";
        var obj = JsonUtility.FromJson<DummyNode<T>>(dummy_json);
        return obj.array;
    }
    public static string ToJson<T>(IEnumerable<T> collection)
    {
        string json = JsonUtility.ToJson(new DummyNode<T>(collection));
        int start = DummyNode<T>.ROOT_NAME.Length + 4;
        int len = json.Length - start - 1;
        return json.Substring(start, len);
    }
    [Serializable]
    private struct DummyNode<T>
    {
        public const string ROOT_NAME = nameof(array);
        public T[] array;
        public DummyNode(IEnumerable<T> collection) => array = collection.ToArray();
    }
}

[Serializable]
public class Flap
{
    public int id;
    public GameObject obj;
}

public class Displayer : MonoBehaviour
{
    [SerializeField]
    private Texture2D normalTexture;
    [SerializeField]
    private Material flyMaterial;
    [SerializeField]
    private Text nameText;
    [SerializeField] SocketIOController io;
    [SerializeField]
    private GameObject Flap;
	List<Flap> flaps = new List<Flap>();
    List<DataNameIDSet> addingFlapDatas = new List<DataNameIDSet>();
    private void Start()
    {
        io.On("connect", (SocketIOEvent e) =>
        {
            Debug.Log("SocketIO connected");
        });
        io.Connect();
        io.On("emit_to_garden", (SocketIOEvent e) =>
        {
			var f = EscapeAndFromJson<DataNameIDSet>(e);
            addingFlapDatas.Add(f);
        });
        io.On("add_to_garden", (SocketIOEvent e) =>
        {
            var f = EscapeAndFromJson<DataNameIDSet>(e);
            addingFlapDatas.Add(f);
            var oldfl = flaps[0];
            flaps.Remove(oldfl);
			var i = new Ints { id = oldfl.id };
			io.Emit("removeFlap", JsonUtility.ToJson(i));
            Destroy(oldfl.obj);
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
			var i = new Ids { id = id };
			io.Emit("sendFlapDatas", JsonUtility.ToJson(i));
        });
    }
    string EscapeTrim(SocketIOEvent e)
    {
        var ed = Regex.Unescape(e.data);
        ed = ed.Substring(1, ed.Length - 2);
        return ed;
    }

    T EscapeAndFromJson<T>(SocketIOEvent e) => JsonUtility.FromJson<T>(EscapeTrim(e));

    /// <summary>
    /// 位置方向を送信する
    /// </summary>
    /// <param name="f">Flapのgameobject</param>
    void EmitPosDiff(Flap f)
	{
        var wing = f.obj.GetComponent<FlapWing>();
        var p = new IDPositionSet
        {
            id = f.id,
            pos = f.obj.transform.position
        };
        p.pos.y = wing.y;
        io.Emit("setPos", JsonUtility.ToJson(p));
        var d = new IDPositionSet
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
    /// <param name="addFlag">新規フラップの数</param>
    void CreateFlap(Texture2D texture, int id)
	{
        var flapObj = Instantiate(Flap);
        var f = flapObj.GetComponent<FlapWing>();
        f.flapTexture = texture;
        var diff = new Vector3(UnityEngine.Random.Range(-3f, 3f), 0, UnityEngine.Random.Range(-3f, 3f)).normalized;
        transform.position += Vector3.forward * UnityEngine.Random.Range(-3f, 3f);
        transform.position += Vector3.right * UnityEngine.Random.Range(-3f, 3f);
        f.y = UnityEngine.Random.Range(-3f, 3f);
        f.diff = diff;
        var flap = new Flap()
        {
            id = id,
            obj = flapObj
        };
        flaps.Add(flap);
        EmitPosDiff(flap);
        io.Emit("addFlap", JsonUtility.ToJson(new Ints { id = id }));
    }

    public void Send()
    {
		foreach (var f in flaps)
		{
            var set = new IDPositionSet();
            set.id = f.id;
            set.pos = f.obj.transform.position;
            var s = JsonUtility.ToJson(set);
            Debug.Log(s);
			io.Emit("emit_from_client", s);
		}
	}
}
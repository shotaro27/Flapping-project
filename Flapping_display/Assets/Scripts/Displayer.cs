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
    List<DataNameIDSet> flaps = new List<DataNameIDSet>();
    List<GameObject> flapobjs = new List<GameObject>();
    int addFlag = 0;
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
            flaps.Add(f);
        });
        io.On("add_to_garden", (SocketIOEvent e) =>
        {
            var f = EscapeAndFromJson<DataNameIDSet>(e);
            flaps.Add(f);
            var oldfl = flapobjs.OrderBy(fl => fl.GetComponent<FlapWing>().id).FirstOrDefault();
            flapobjs.Remove(oldfl);
			var i = new Ints { id = oldfl.GetComponent<FlapWing>().id };
			io.Emit("removeFlap", JsonUtility.ToJson(i));
            Destroy(oldfl);
            addFlag++;
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
            flapobjs.ForEach(EmitPosDiff);
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

    void EmitPosDiff(GameObject f)
	{
        var wing = f.GetComponent<FlapWing>();
        var p = new IDPositionSet
        {
            id = wing.id,
            pos = f.transform.position
        };
        p.pos.y = wing.y;
        io.Emit("setPos", JsonUtility.ToJson(p));
        var d = new IDPositionSet
        {
            id = wing.id,
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
		if (flaps.Count > 0)
        {
            DataNameIDSet dat = flaps[0];
            var textureData = dat.data;
            var name = dat.name;
            byte[] byte_After = Convert.FromBase64String(textureData);
            Texture2D texture_After = new Texture2D(flyMaterial.mainTexture.width, flyMaterial.mainTexture.height,
                                            TextureFormat.RGBA32, false);
            texture_After.LoadImage(byte_After);
            CreateFlap(texture_After, dat.id, addFlag);
            nameText.text = name;
            flaps.RemoveAt(0);
            addFlag = addFlag == 0 ? addFlag : addFlag - 1;
        }
    }

    void CreateFlap(Texture2D texture, int id, int addFlag)
	{
        var flap = Instantiate(Flap);
        var f = flap.GetComponent<FlapWing>();
        f.flapTexture = texture;
        f.id = id;
        var diff = new Vector3(UnityEngine.Random.Range(-3f, 3f), 0, UnityEngine.Random.Range(-3f, 3f)).normalized;
        transform.position += Vector3.forward * UnityEngine.Random.Range(-3f, 3f);
        transform.position += Vector3.right * UnityEngine.Random.Range(-3f, 3f);
        f.y = UnityEngine.Random.Range(-3f, 3f);
        f.diff = diff;
        flapobjs.Add(flap);
        if (addFlag > 0)
        {
            EmitPosDiff(flap);
            io.Emit("addFlap", JsonUtility.ToJson(new Ints { id = id }));
        }
    }

    public void Send()
    {
		foreach (var f in flapobjs)
		{
            var set = new IDPositionSet();
            set.id = f.GetComponent<FlapWing>().id;
            set.pos = f.transform.position;
            var s = JsonUtility.ToJson(set);
            Debug.Log(s);
			io.Emit("emit_from_client", s);
		}
	}
}
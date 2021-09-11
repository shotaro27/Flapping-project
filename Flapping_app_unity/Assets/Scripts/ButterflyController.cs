using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using UnitySocketIO;
using UnitySocketIO.Events;
using System.Linq;

interface IIDCountable
{
	int id { get; set; }
}

[Serializable]
public struct DataNameIDSet : IIDCountable
{
    public string data;
    public string name;
    public int id { get; set; }
}

[Serializable]
public struct IDPositionSet : IIDCountable
{
    public int id { get; set; }
    public Vector3 pos;
}

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

public class ButterflyController : MonoBehaviour
{
    [SerializeField]
    private Texture2D normalTexture;
    [SerializeField]
    private Material flyMaterial;
    [SerializeField] SocketIOController io;
    [SerializeField]
    private GameObject Flap;
    [SerializeField]
    private GameObject load;
    List<DataNameIDSet> flaps = new List<DataNameIDSet>();
    List<GameObject> flapobjs = new List<GameObject>();
    List<IDPositionSet> positions = new List<IDPositionSet>();
    public List<IDPositionSet> diffs = new List<IDPositionSet>();
    void AddID<T, S>(SocketIOEvent e, List<T> l, List<S> addingList) where T: IIDCountable where S: IIDCountable
    {
        var f = e.EscapeAndFromJson<S>();
        if (l.Where(fl => fl.id == f.id).Count() == 0) addingList.Add(f);
    }
    private void Start()
    {
        io.On("connect", e =>
        {
            Debug.Log("SocketIO connected");
            io.Emit("emit_from_viewer");
        });
        io.Connect();
        io.On("data_set", e => AddID(e, flapobjs.Select(fl => fl.GetComponent<FlapWing>()).ToList(), flaps));
        io.On("pos_set", e => AddID(e, positions, positions));
        io.On("diff_set", e => AddID(e, diffs, diffs));
        io.On("emit_from_server", e => {
            string msgstring = e.data;
            Debug.Log("WebSocket received message: " + msgstring);
        });
        io.On("remove_flap", e => {
            var id = int.Parse(e.data);
            var oldfl = flapobjs.OrderBy(fl => fl.GetComponent<FlapWing>().id).FirstOrDefault();
            flapobjs.Remove(oldfl);
            Destroy(oldfl);
            positions.RemoveAll(p => p.id == id);
            diffs.RemoveAll(d => d.id == id);
        });
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
            byte[] byte_After = Convert.FromBase64String(textureData);
            Texture2D texture_After = new Texture2D(flyMaterial.mainTexture.width, flyMaterial.mainTexture.height,
                                            TextureFormat.RGBA32, false);
            texture_After.LoadImage(byte_After);
            CreateFlap(texture_After, dat.id, dat.name);
            flaps.RemoveAt(0);
            load.SetActive(false);
        }
    }

    void CreateFlap(Texture2D texture, int id, string name)
    {
        var flap = Instantiate(Flap);
        var fl = flap.GetComponent<FlapWing>();
        fl.flapTexture = texture;
        fl.id = id;
        fl.flapName = name;
        fl.diff = diffs.Where(f => f.id == id).Select(f => f.pos).FirstOrDefault();
        var flpos = positions.Where(f => f.id == id).Select(f => f.pos).FirstOrDefault();
        flap.transform.position = flpos;
        fl.y = flpos.y;
        flapobjs.Add(flap);
    }
}

public static class SocketIOEventEx
{
    public static string EscapeTrim(this SocketIOEvent e)
    {
        var ed = Regex.Unescape(e.data);
        ed = ed.Substring(1, ed.Length - 2);
        return ed;
    }
    public static T EscapeAndFromJson<T>(this SocketIOEvent e) => JsonUtility.FromJson<T>(EscapeTrim(e));
}
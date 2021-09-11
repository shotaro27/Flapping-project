using UnityEngine.UI;
using UnityEngine;
using UnitySocketIO;
using UnitySocketIO.Events;
using System.Runtime.InteropServices;
using System;

[Serializable]
public class DataNameSet
{
    public string data;
    public string name;
}

public class FlyButton : MonoBehaviour
{
    [SerializeField] Material flies;
    [SerializeField] Texture2D defaultTexture;
    [SerializeField] SocketIOController io;
    [SerializeField] WebGLNativeInputField field;
    [SerializeField] Button back;

    [DllImport("__Internal")]
    private static extern void SetLocalStorage(string key, string value);

    void Start()
    {
        //Debug.Log("{\"data\": \"2\", \"name\": \"asdfg\"}");
        io.On("connect", (SocketIOEvent e) => {
            Debug.Log("SocketIO connected");
        });

        io.Connect();

        io.On("emit_from_server", (SocketIOEvent e) => {
            Debug.Log("WebSocket received message: " + e.data);
        });
		switch (Settings.mode)
		{
			case DrawMode.New:
                back.onClick.AddListener(() => GetComponent<SceneManagement>().GoScene("DrawScene"));
				break;
			case DrawMode.Open:
                field.text = Settings.FlapName;
                back.onClick.AddListener(() => GetComponent<SceneManagement>().GoScene("FlyingScene"));
                break;
		}
		flies.mainTexture = Settings.DrawingFlap;
    }

    private void OnDestroy()
    {
        flies.mainTexture = defaultTexture;
        io.Close();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SendData()
    {
        byte[] PNGData = Settings.DrawingFlap.EncodeToPNG();
        string encodedText = Convert.ToBase64String(PNGData);
        DataNameSet set = new DataNameSet();
        set.data = encodedText;
        set.name = Settings.FlapName;
        string dataStr = JsonUtility.ToJson(set);
        Debug.Log(dataStr);
		io.Emit("emit_from_client", dataStr);
        SetLocalStorage(Settings.SaveSlot.ToString(), dataStr);
    }
    public void SetName(string s) => Settings.FlapName = s;
}

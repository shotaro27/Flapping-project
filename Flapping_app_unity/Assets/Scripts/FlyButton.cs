using UnityEngine.UI;
using UnityEngine;
using UnitySocketIO;
using UnitySocketIO.Events;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

[Serializable]
public class DataNameSet
{
    public string data;
    public string name;
}

public class FlyButton : MonoBehaviour
{
    /// <summary>
    /// Flapのマテリアル
    /// </summary>
    [SerializeField] Material flies;

    /// <summary>
    /// デフォルトのテクスチャ
    /// </summary>
    [SerializeField] Texture2D defaultTexture;

    /// <summary>
    /// Socket.IOコントローラー
    /// </summary>
    [SerializeField] SocketIOController io;

    /// <summary>
    /// 名前入力フィールド
    /// </summary>
    [SerializeField] WebGLNativeInputField field;

    /// <summary>
    /// 戻るボタン
    /// </summary>
    [SerializeField] Button back;

    /// <summary>
    /// シーンマネージャ
    /// </summary>
    [SerializeField] SceneManagement sceneManagement;

    int lastFlapID;

    void Start()
    {
        // Socket.IO接続
        io.On("connect", (SocketIOEvent e) => {
            Debug.Log("SocketIO connected");
            io.Emit("getLastFlapID");
        });

        io.Connect();

        io.On("emit_from_server", (SocketIOEvent e) => {
            Debug.Log("WebSocket received message: " + e.data);
        });

        io.On("lastFlapID", (SocketIOEvent e) => {
            lastFlapID = int.Parse(e.data);
        });

        io.On("data_set", e =>
        {
            sceneManagement.GoScene("FlyingScene");
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

    /// <summary>
    /// サーバーにデータを送る
    /// </summary>
    public void SendData()
    {
		if (Settings.mode == DrawMode.New)
        {
            byte[] PNGData = Settings.DrawingFlap.EncodeToPNG();
            string encodedText = Convert.ToBase64String(PNGData);
            DataNameSet set = new DataNameSet();
            set.data = encodedText;
            set.name = Settings.FlapName;
            string dataStr = JsonUtility.ToJson(set);
            Debug.Log(dataStr);
            io.Emit("emit_from_client", dataStr);
            var stString = Storage.GetLocalStorage("MyFlap");
            var myFlaps = JsonConvert.DeserializeObject<List<int>>(string.IsNullOrEmpty(stString) ? "[]" : stString);
            myFlaps.Add(lastFlapID);
            Storage.SetLocalStorage("MyFlap", JsonConvert.SerializeObject(myFlaps));
        }
		else
        {
            io.Emit("addFlapID", JsonConvert.SerializeObject(new Ids() {id = Settings.Id}));
        }
    }

    /// <summary>
    /// Flapの名前設定
    /// </summary>
    /// <param name="s">Flap名前</param>
    public void SetName(string s) => Settings.FlapName = s;
}

﻿using UnityEngine.UI;
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
    /// ローカルストレージに値を設定する
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="value">値</param>
    [DllImport("__Internal")] private static extern void SetLocalStorage(string key, string value);

    void Start()
    {
        // Socket.IO接続
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

    /// <summary>
    /// サーバーにデータを送る
    /// </summary>
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

    /// <summary>
    /// Flapの名前設定
    /// </summary>
    /// <param name="s">Flap名前</param>
    public void SetName(string s) => Settings.FlapName = s;
}

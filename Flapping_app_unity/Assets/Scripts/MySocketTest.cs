using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySocketIO;
using UnitySocketIO.Events;
using System.Text;
using System.Runtime.InteropServices;
using System;

public class MySocketTest : MonoBehaviour
{
    [SerializeField] SocketIOController io;

    void Start()
    {
        io.On("connect", (SocketIOEvent e) => {
            Debug.Log("SocketIO connected");
        });

        io.Connect();

        io.On("emit_from_server", (SocketIOEvent e) => {
            Debug.Log("WebSocket received message: " + e.data);
        });
    }

    private void OnDestroy()
    {
        io.Close();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SendData()
    {
        DataNameSet set = new DataNameSet();
        set.data = "うんこ";
        set.name = "おっぱい";
        string dataStr = JsonUtility.ToJson(set);
        Debug.Log(dataStr);
        io.Emit("emit_from_client", dataStr);
    }
}

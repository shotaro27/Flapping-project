using System;
using UnityEngine;

namespace SocketIOJsonObjects
{
    /// <summary>
    /// Flapの画像データ、名前、IDを管理する
    /// </summary>
    public struct FlapDataSet
    {
        public string data;
        public string name;
        public int id;
    }

    /// <summary>
    /// FlapのID、位置（または位置の差分）を管理する
    /// </summary>
    [Serializable]
    public struct FlapPositionSet
    {
        public int id;
        public Vector3 pos;
    }

    /// <summary>
    /// Socket.ioのID(アクセスしたブラウザのID)を管理する
    /// </summary>
    public struct SocketId { public string id; }

    /// <summary>
    /// FlapのIDを管理する
    /// </summary>
    public struct FlapId { public int id; }
}
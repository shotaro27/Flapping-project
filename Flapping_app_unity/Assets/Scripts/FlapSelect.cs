using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

/// <summary>
/// Flap保存場所選択
/// </summary>
public class FlapSelect : MonoBehaviour
{
    /// <summary>
    /// ローカルストレージから値を取得する
    /// </summary>
    /// <param name="key">値のキー</param>
    /// <returns>取得した値</returns>
	[DllImport("__Internal")] private static extern string GetLocalStorage(string key);

    /// <summary>
    /// テクスチャのサイズイメージ
    /// </summary>
	[SerializeField] Texture2D textureReference;

    /// <summary>
    /// 保存されたFlapを表示するTransform
    /// </summary>
    [SerializeField] List<Transform> flaps;

    /// <summary>
    /// シーン操作
    /// </summary>
    [SerializeField] SceneManagement sm;

    void Start()
    {
		for (int slot = 0; slot < flaps.Count; slot++)
		{
			string s = GetLocalStorage(slot.ToString());
            if (string.IsNullOrEmpty(s))
            {
                flaps[slot].GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                var dat = JsonUtility.FromJson<DataNameSet>(s).data;
                var bytes = Convert.FromBase64String(dat);
                var t = new Texture2D(textureReference.width, textureReference.height, TextureFormat.RGBA32, false);
                t.LoadImage(bytes);
                var sp = Sprite.Create(t, new Rect(0, 0, t.width, t.height), Vector2.zero);
                flaps[slot].GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite = sp;
                flaps[slot].GetChild(0).GetChild(1).gameObject.GetComponent<Image>().sprite = sp;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 新しいFlapを作る
    /// </summary>
    /// <param name="slot">保存場所</param>
    public void SetNew(int slot)
    {
        Settings.SaveSlot = slot;
        Settings.DrawingFlap = null;
        Settings.FlapName = null;
        Settings.mode = DrawMode.New;
        sm.GoScene("DrawSettingScene");
    }

    /// <summary>
    /// Flapを開く
    /// </summary>
    /// <param name="slot">保存場所</param>
    public void SetOpen(int slot)
    {
        Settings.SaveSlot = slot;
		string s = GetLocalStorage(slot.ToString());
        var f = JsonUtility.FromJson<DataNameSet>(s);
        var dat = f.data;
        Settings.FlapName = f.name;
        var bytes = Convert.FromBase64String(dat);
        Settings.DrawingFlap = new Texture2D(textureReference.width, textureReference.height, TextureFormat.RGBA32, false);
        Settings.DrawingFlap.LoadImage(bytes);
        Settings.mode = DrawMode.Open;
        sm.GoScene("PreviewScene");
    }
}

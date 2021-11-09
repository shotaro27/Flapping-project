using System.Runtime.InteropServices;
using UnityEngine;

public static class Storage
{
    /// <summary>
    /// ローカルストレージから値を取得する
    /// </summary>
    /// <param name="key">値のキー</param>
    /// <returns>取得した値</returns>
#if UNITY_WEBGL && !UNITY_EDITOR
	[DllImport("__Internal")] public static extern string GetLocalStorage(string key);
#else
    public static string GetLocalStorage(string key) => PlayerPrefs.GetString(key, "");
#endif

    /// <summary>
    /// ローカルストレージに値を設定する
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="value">値</param>
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] public static extern void SetLocalStorage(string key, string value);
#else
    public static void SetLocalStorage(string key, string value) => PlayerPrefs.SetString(key, value);
#endif
}

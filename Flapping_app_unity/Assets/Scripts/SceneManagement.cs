using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    string taskScene;

    public void GoScene(string sceneName) => SceneManager.LoadScene(sceneName);
    public void AddScene(string sceneName) => SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    public void RemoveScene(string sceneName) => SceneManager.UnloadSceneAsync(sceneName);

    public void PutScene(string sceneName) => taskScene = sceneName;

    public void ApplyScene() => GoScene(taskScene);
}

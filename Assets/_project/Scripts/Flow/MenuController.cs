using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string _gameSceneName = "GamePlay";
    [SerializeField] private string _mainMenuSceneName = "MainMenu";
    public void LoadGame()
    {
        SceneManager.LoadScene(_gameSceneName);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(_mainMenuSceneName);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
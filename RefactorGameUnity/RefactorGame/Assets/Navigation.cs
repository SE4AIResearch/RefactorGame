using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Navigation : MonoBehaviour
{
    public void PlayButtonSound()
    {
        GetComponent<AudioSource>().Play();
    }

    static public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    static public void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }

    static public void LoadLevelSelect()
    {
        LoadScene("LevelSelect");
    }

    static public void QuitGame()
    {
#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#else   
        Application.Quit();
#endif
    }
}

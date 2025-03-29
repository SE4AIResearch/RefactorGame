using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToLevelSelect : MonoBehaviour
{
    public void OnClick() 
    {
        SceneManager.LoadScene("LevelSelect");
    }
}

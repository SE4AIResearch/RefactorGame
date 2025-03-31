using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToLevelSelect : MonoBehaviour
{
    public CurrentKitchenState Kitchen;

    public void OnClick() 
    {
        SaveAndQuit();
    }

    private void SaveAndQuit()
    {
        if (!SaveData.LoadedGame.Solutions.ContainsKey(Kitchen.LoadedPuzzle.Name))
            SaveData.LoadedGame.Solutions.Add(Kitchen.LoadedPuzzle.Name, Kitchen.LastSolution);
        else
            SaveData.LoadedGame.Solutions[Kitchen.LoadedPuzzle.Name] = Kitchen.LastSolution;

        SaveData.SaveGame();

        SceneManager.LoadScene("LevelSelect");
    }
}

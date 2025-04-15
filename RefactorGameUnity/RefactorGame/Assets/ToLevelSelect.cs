using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RefactorLang;
using System.Linq;

public class ToLevelSelect : MonoBehaviour
{
    public CurrentKitchenState Kitchen;

    public void OnClick() 
    {
        SaveAndQuit();
    }

    private void SaveAndQuit()
    {
        try
        {
            if (!SaveData.LoadedGame.Solutions.ContainsKey(Kitchen.LoadedPuzzle.Name))
                SaveData.LoadedGame.Solutions.Add(Kitchen.LoadedPuzzle.Name, Kitchen.LastSolution);
            else
                SaveData.LoadedGame.Solutions[Kitchen.LoadedPuzzle.Name] = Kitchen.LastSolution;

            SaveData.LoadedGame.DefinitionsUnlocked = DictionaryState.Current.UnlockedDefinitions;

            SaveData.SaveGame();
        }
        catch
        {
            Debug.Log("something in the saving process went wrong");
        }

        SceneManager.LoadScene("LevelSelect");
    }
}

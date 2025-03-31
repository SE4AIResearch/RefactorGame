using RefactorLang;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelLoader
{
    private static string _puzzleName;

    public static void LoadPuzzleScene(string puzzleFilename)
    {
        _puzzleName = puzzleFilename;
        SceneManager.LoadScene("Level");
    }

    public static string GetPuzzleName() { return _puzzleName; }
}

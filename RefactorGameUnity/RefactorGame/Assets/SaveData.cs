using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class GameData
{
    public int NumLevelsUnlocked = 1;
    
    public Dictionary<string, string> Solutions = new ();
} 

public static class SaveData
{
    private static readonly string saveFilePath = Application.persistentDataPath + "/save.json";
    public static GameData LoadedGame { get; private set; }

    private readonly static bool DISABLE = false;

    public static void SaveGame()
    {
        if (DISABLE) return;

        File.WriteAllText(saveFilePath, JsonConvert.SerializeObject(LoadedGame));
    }

    public static void LoadGame()
    {
        if (DISABLE) { LoadedGame = new GameData(); return; }

        if (!File.Exists(saveFilePath))
        {
            LoadedGame = new GameData();
            return;
        }

        LoadedGame = JsonConvert.DeserializeObject<GameData>(File.ReadAllText(saveFilePath));
    }
}

using System.Collections;
using System.Collections.Generic;
using RefactorLang;
using UnityEngine;

public class DictionaryState : MonoBehaviour
{
    public static DictionaryState Current;

    public List<string> UnlockedDefinitions = new List<string>();

    private void Awake()
    {
        SaveData.LoadGame();
        Current = this;
        Current.UnlockedDefinitions = SaveData.LoadedGame.DefinitionsUnlocked;
        DontDestroyOnLoad(this.gameObject);
    }

    public void UnlockDefinitions(List<string> definitions)
    {
        foreach (string definition in definitions)
        {
            if (!Current.UnlockedDefinitions.Contains(definition))
            {
                Current.UnlockedDefinitions.Add(definition);
            }
        }
    }

    public void LockDefinitions(List<string> definitions)
    {
        foreach (string definition in definitions)
        {
            if (Current.UnlockedDefinitions.Contains(definition))
            {
                Current.UnlockedDefinitions.Remove(definition);
            }
        }
    }

    public bool IsLocked(string definition)
    {
        return !Current.UnlockedDefinitions.Contains(definition);
    }
}

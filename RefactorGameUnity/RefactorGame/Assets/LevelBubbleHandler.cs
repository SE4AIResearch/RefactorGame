using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBubbleHandler : MonoBehaviour
{
    public LevelSelectHandler LevelSelectHandler;
    public int LevelIndex;
    public bool Active;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnLevelWasLoaded()
    {
        SaveData.LoadGame();
        if (SaveData.LoadedGame.LevelsBeaten.Count + 1 < this.LevelIndex)
        {
            this.Active = false;
            this.GetComponent<SpriteRenderer>().color = Color.gray;
        }
        else
        {
            this.Active = true;
        }
    }

    private void OnMouseDown()
    {
        if (!Active) return;

        LevelSelectHandler.SelectedLevel = this.LevelIndex;
    }
}

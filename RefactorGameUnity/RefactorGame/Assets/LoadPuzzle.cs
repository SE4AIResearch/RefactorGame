using RefactorLang;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadPuzzle : MonoBehaviour
{
    public CurrentKitchenState kitchenState;
    public InGameTextEditor.TextEditor editor;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadSample()
    {
        Load(Puzzle.Deserialize(@"./Assets/Resources/Puzzles/OneSoupTwoSoup.json"));
    }

    void Load(Puzzle puzzle)
    {
        kitchenState.KitchenState = new KitchenState(puzzle);

        editor.Text = puzzle.StarterCode;

        kitchenState.LoadedPuzzle = puzzle;
    }
}

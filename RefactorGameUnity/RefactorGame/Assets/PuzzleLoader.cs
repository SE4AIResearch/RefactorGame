using RefactorLang;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PuzzleLoader : MonoBehaviour
{
    public CurrentKitchenState kitchenState;
    public InGameTextEditor.TextEditor editor;
    public GameObject StoryPopUp;

    // Start is called before the first frame update
    void Start()
    {
        LoadPuzzleFromName(LevelLoader.GetPuzzleName() ?? "OneSoupTwoSoup");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadPuzzleFromName(string puzzleName)
    {
        Load(Puzzle.Deserialize(@$"./Assets/Resources/Puzzles/{puzzleName}.json"));
    }

    void Load(Puzzle puzzle)
    {
        kitchenState.KitchenState = new KitchenState(puzzle);

        editor.Text = puzzle.StarterCode;

        kitchenState.LoadedPuzzle = puzzle;

        StoryPopUp.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = puzzle.Name;
        StoryPopUp.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = puzzle.StoryPrompt;

    }
}

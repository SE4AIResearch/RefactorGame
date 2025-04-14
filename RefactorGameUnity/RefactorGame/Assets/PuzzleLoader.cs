using RefactorLang;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PuzzleLoader : MonoBehaviour
{
    public CurrentKitchenState kitchenState;
    public InGameTextEditor.TextEditor editor;
    public GameObject StoryPopUp;
    public LineCounter Constraints;

    // Start is called before the first frame update
    void Start()
    {
        SaveData.LoadGame();
        LoadPuzzleFromName(LevelLoader.GetPuzzleName() ?? "OneSoupTwoSoup");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetLevel()
    {
        Load(kitchenState.LoadedPuzzle, original:true);
    }

    public void LoadPuzzleFromName(string puzzleName)
    {
        TextAsset json = Resources.Load<TextAsset>(@$"Puzzles/Json/{puzzleName}");
        Puzzle puzzle = JsonSerializer.Deserialize<Puzzle>(json.text);
        Load(puzzle);
    }

    void Load(Puzzle puzzle, bool original = false)
    {
        string fileName = puzzle.StarterCode.Remove(puzzle.StarterCode.IndexOf(".txt"));
        string starterCode = Resources.Load<TextAsset>(@$"Puzzles/Src/{fileName}").text;

        kitchenState.KitchenState = new KitchenState(puzzle);

        if (SaveData.LoadedGame.Solutions.TryGetValue(puzzle.Name, out string solution) && !original)
        {
            editor.Text = solution;
            kitchenState.LastSolution = solution;
        }
        else if (puzzle.Name == "R&D: All Together Now")
        {
            string sol = SaveData.LoadedGame.Solutions["R&D: Soup on Soup"];
            editor.Text = sol;
            kitchenState.LastSolution = sol;
        }
        else if (puzzle.Name == "R&D: One More Soup")
        {
            string sol = "func take_order(order) {\r\n\tif (order == \"Tomato Soup\") {\r\n\t\tmake_soup(\"Tomato\")\r\n\t}\r\n\telse {\r\n\t\tmake_soup(\"Squash\")\r\n\t}\r\n}\r\n\r\n"
                + SaveData.LoadedGame.Solutions["R&D: All Together Now"];

            editor.Text = sol;
            kitchenState.LastSolution = sol;
        }
        else
        {
            editor.Text = starterCode;
            kitchenState.LastSolution = starterCode;
        }
            

        kitchenState.LoadedPuzzle = puzzle;

        kitchenState.Definitions = puzzle.DictionaryItems;

        Constraints.CheckLines(starterCode);

        StoryPopUp.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = puzzle.Name;
        StoryPopUp.transform.Find("Description").Find("Text").GetComponent<TextMeshProUGUI>().text = puzzle.StoryPrompt;
    }
}

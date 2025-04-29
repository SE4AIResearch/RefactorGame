using RefactorLang;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public AdditionalConstraintCounter AdditionalConstraints;
    public ScriptCompiler Compiler;
    public GameObject Loader;

    // Start is called before the first frame update
    void Start()
    {
        SaveData.LoadGame();
        LoadPuzzleFromName(LevelLoader.GetPuzzleName() ?? "AllOverThePlace");
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

        editor.ClearSelection();

        kitchenState.KitchenState = new KitchenState(puzzle);

        if (SaveData.LoadedGame.Solutions.TryGetValue(puzzle.Name, out Solution solution) && solution != null && !original)
        {
            editor.Text = solution.Text;
            kitchenState.KitchenState.Stations = solution.Stations.Select(x => x.ConvertToStation()).ToList();
            kitchenState.LastSolution = solution;
        }
        else if (puzzle.Name == "R&D: All Together Now")
        {
            string sol = SaveData.LoadedGame.Solutions["R&D: Soup on Soup"].Text;
            editor.Text = sol;
            kitchenState.LastSolution.Text = sol;
        }
        else if (puzzle.Name == "R&D: One More Soup")
        {
            string sol = "func take_order(order) {\r\n\tif (order == \"Tomato Soup\") {\r\n\t\tmake_soup(\"Tomato\")\r\n\t}\r\n\telse {\r\n\t\tmake_soup(\"Squash\")\r\n\t}\r\n}\r\n\r\n"
                + SaveData.LoadedGame.Solutions["R&D: All Together Now"].Text;

            editor.Text = sol;
            kitchenState.LastSolution.Text = sol;
        }
        else
        {
            editor.Text = starterCode;
        }

        kitchenState.LoadedPuzzle = puzzle;

        kitchenState.Definitions = puzzle.DictionaryItems;
        DictionaryState.Current.UnlockDefinitions(puzzle.DictionaryItems);
        Loader.GetComponent<LoadDictionary>().Reload();

        Compiler.Compile(starterCode);

        Constraints.CheckLines(starterCode);
        AdditionalConstraints.DisplayAdditionalConstraint(kitchenState.KitchenState);
        AdditionalConstraints.DisplayAdditionalConstraint(starterCode, Compiler.CompilationStats);

        StoryPopUp.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = puzzle.Name;
        StoryPopUp.transform.Find("Description").Find("Text").GetComponent<TextMeshProUGUI>().text = puzzle.StoryPrompt;
    }
}

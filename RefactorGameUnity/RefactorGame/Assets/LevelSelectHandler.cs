using System.Collections.Generic;
using UnityEngine;
using RefactorLang;
using System;
using TMPro;
using System.Text.Json;
using UnityEngine.UI;

public class LevelSelectHandler : MonoBehaviour
{
    public Camera Camera;
    public GameObject LevelModuleLocations;
    public GameObject LevelDetail;

    private Vector3 CameraTarget;
    private float CameraSmoothSpeed = (1f / 32f);

    public static List<string> puzzleNames = new List<string> {
        "OneSoupTwoSoup",
        "FutureRecipes",
        "AMethodToTheMadness",
        "RAndDSoupOnSoup",
        "RAndDAllTogetherNow",
        "RAndDOneMoreSoup",
        "TakeTwo",
        "BuildABurger",
        "AllOverThePlace",
        "ATightShift",
        "ParameterOverload"
    };

    public event Action<int> OnSelectedLevelChanged;

    private static int _selectedLevel = 1;
    public int SelectedLevel {
        get => _selectedLevel;
        set
        {
            _selectedLevel = value;
            OnSelectedLevelChanged.Invoke(value);
        }
    }

    void Focus(int levelIndex)
    {
        Transform target = LevelModuleLocations.transform.Find($"Level{levelIndex}");

        CameraTarget = new Vector3(target.transform.position.x, Camera.transform.position.y, Camera.transform.position.z);

        transform.Find("Selected").position = target.transform.position;
        
        float detailY = target.transform.position.y - UpdateFactor(levelIndex);
        Vector3 detailTarget = new Vector3(Camera.transform.position.x, detailY, target.transform.position.z);
        LevelDetail.transform.position = detailTarget;

        string puzzleName = puzzleNames[SelectedLevel - 1];
        TextAsset json = Resources.Load<TextAsset>(@$"Puzzles/Json/{puzzleName}");
        Puzzle puzzle = JsonSerializer.Deserialize<Puzzle>(json.text);

        LevelDetail.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = puzzle.Name;
        LevelDetail.transform.Find("Story").GetComponent<TextMeshProUGUI>().text = puzzle.StoryPrompt;
        LayoutRebuilder.ForceRebuildLayoutImmediate(LevelDetail.GetComponent<RectTransform>());
    }

    // Start is called before the first frame update
    void Start()
    {
        Focus(SelectedLevel);

        OnSelectedLevelChanged += Focus;
    }

    // Update is called once per frame
    void Update()
    {
        Camera.transform.position = Vector3.Lerp(Camera.transform.position, CameraTarget, CameraSmoothSpeed);
    }

    public void LoadSelectedLevel()
    {
        LevelLoader.LoadPuzzleScene(puzzleNames[SelectedLevel - 1]);
    }

    private float UpdateFactor(int index)
    {
        switch (index)
        {
            case int n when (n >= 0 && n <= 4):
                return 2f;

            case int n when (n >= 5 && n <= 9):
                return -2.5f;

            case int n when (n >= 10 && n <= 11):
                return 2f;

            default:
                throw new ArgumentException("Invalid level index");
        }
    }
}

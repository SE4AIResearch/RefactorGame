using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RefactorLang;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class LevelSelectHandler : MonoBehaviour
{
    public Camera Camera;
    public GameObject LevelModuleLocations;

    private Vector3 CameraTarget;
    private float CameraSmoothSpeed = 0.125f;

    public static List<string> puzzleNames = new List<string> {
        "OneSoupTwoSoup",
        "AgainAndAgain",
        "FutureRecipes"
    };

    public event Action<int> OnSelectedLevelChanged;

    private int _selectedLevel = 1;
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
        CameraTarget = new Vector3(LevelModuleLocations.transform.Find($"Level{levelIndex}").transform.position.x, Camera.transform.position.y, Camera.transform.position.z);
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

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SelectedLevel++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            SelectedLevel--;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene("UpdatedScene");
        }
    }
}

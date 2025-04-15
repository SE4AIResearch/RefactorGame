using Newtonsoft.Json;
using ParserLibrary;
using RefactorLang;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/CurrentKitchenState")]
public class CurrentKitchenState : ScriptableObject
{
    private KitchenState _state;

    public bool ContextMenuUp = false;

    public bool LevelComplete = false;

    public Solution LastSolution;

    public List<string> Definitions = new List<string>();

    public event Action<KitchenState> OnStateChanged;

    public KitchenState KitchenState
    {
        get => _state;
        set
        {
            _state = value;
            OnStateChanged.Invoke(value);
        }
    }

    public Puzzle LoadedPuzzle;

    public void FinishTestWithStatus(TestStatus status, string text, CompilationStats compilationStats)
    {
        if ((!ApplyConstraints(compilationStats.NumStmts) || !ApplyAdditionalConstraints(text, compilationStats)) && status.Equals(TestStatus.Passed))
        {
            UpdateTestCaseStatus(TestStatus.Warning);
            return;
        }

        UpdateTestCaseStatus(status);
    }

    private bool ApplyAdditionalConstraints(string text, CompilationStats compilationStats)
    {
        switch (LoadedPuzzle.Constraints.AdditionalConstraint)
        {
            case "LessModules":
                if (CurrentKitchenState.ApplyLessModules(_state) > 3)
                    return false;
                break;
            case "IndustrialKitchen":
                if (!CurrentKitchenState.ApplyIndustrialKitchen(_state))
                    return false;
                break;
            case "LessParams":
                if (compilationStats.NumParams > 3)
                    return false;
                break;
            case "maxFiveLinesPerMethodMaxSevenMethods":
                if (compilationStats.NumFDecls < 6)
                    return false;
                break;
            case "maxOneIfComparison":
                Regex Regex = new("^(?!.*(\\|\\||&&| and | or )).*$");
                if (!Regex.IsMatch(text))
                    return false;
                break;
        }

        return true;
    }

    public static int ApplyLessModules(KitchenState state)
    {
        return state.Stations.Aggregate(0, (x, y) => x + y.Modules.Where(x => x.GetType().Name != "None").Count());
    }

    public static bool ApplyIndustrialKitchen(KitchenState state)
    {
        return state.Stations.Aggregate(false, (x, y) => x || y.Modules.Count() > 2);
    }

    private bool ApplyConstraints(int numOfStatements)
    {
        if (!(LoadedPuzzle.Constraints.MaxStatements != -1 && LoadedPuzzle.Constraints.MaxStatements >= numOfStatements))
        {
            return false;
        }

        return true;
    }

    public void UpdateTestCaseStatus(TestStatus status)
    {
        int index = KitchenState.SelectedTestCase;
        KitchenState.TestCaseStatus[index] = status;
        OnStateChanged.Invoke(KitchenState);
    }

    public void UpdateCurrentTestCase(int index)
    {
        KitchenState.SelectedTestCase = index;
        OnStateChanged.Invoke(KitchenState);
    }

    public void ResetTestCaseStatus()
    {
        for (int i = 0; i < KitchenState.TestCases.Count; i++) {
            KitchenState.TestCaseStatus[i] = TestStatus.NotRun;
        }
        OnStateChanged.Invoke(KitchenState);
    }
}

public class Solution
{
    public string Text { get; set; }
    public List<StationSignature> Stations { get; set; }

    [JsonConstructor]
    public Solution(string text, List<StationSignature> stations)
    {
        Text = text;
        Stations = stations;
    }

    public Solution(string text, List<Station> stations)
    {
        Text = text;
        Stations = stations.Select(x => x.CreateSignature()).ToList();
    }
}
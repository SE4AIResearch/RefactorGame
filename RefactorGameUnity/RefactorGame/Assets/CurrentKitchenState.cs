using RefactorLang;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/CurrentKitchenState")]
public class CurrentKitchenState : ScriptableObject
{
    private KitchenState _state;

    public bool ContextMenuUp = false;

    public bool LevelComplete = false;

    public string LastSolution = "placeholder";

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

    public void FinishTestWithStatus(TestStatus status, int numOfStatements)
    {
        if (!ApplyConstraints(numOfStatements) && status.Equals(TestStatus.Passed))
        {
            UpdateTestCaseStatus(TestStatus.Warning);
            return;
        }

        UpdateTestCaseStatus(status);
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

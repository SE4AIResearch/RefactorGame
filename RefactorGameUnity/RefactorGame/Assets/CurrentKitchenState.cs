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

    public bool AllTestsPassed() {
        foreach (var status in KitchenState.TestCaseStatus.Values)
        {
            if (status != TestStatus.Passed)
            {
                return false;
            }
        }
        return true;
    }
}

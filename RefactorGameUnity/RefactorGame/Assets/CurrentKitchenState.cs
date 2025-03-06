using RefactorLang;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/CurrentKitchenState")]
public class CurrentKitchenState : ScriptableObject
{
    private KitchenState _state;

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
}

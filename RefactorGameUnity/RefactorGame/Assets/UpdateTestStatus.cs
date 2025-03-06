using System.Collections;
using System.Collections.Generic;
using RefactorLang;
using UnityEngine;
using UnityEngine.UI;

public class UpdateTestStatus : MonoBehaviour
{
    public CurrentKitchenState kitchen;

    void Start()
    {
        kitchen.OnStateChanged += UpdateButtonColor;

    }

    void Update()
    {
        // kitchen.OnStateChanged += UpdateButtonColor;
    }

    public void UpdateButtonColor(KitchenState newState)
    {
        int index = newState.SelectedTestCase;

        TestStatus status = kitchen.KitchenState.TestCaseStatus[index];

        switch (status) {
            case TestStatus.NotRun:
                UpdateColor(index, Color.white);
                break;
            case TestStatus.Running:
                UpdateColor(index, Color.yellow);
                break;
            case TestStatus.Failed:
                UpdateColor(index, Color.red);
                break;
            case TestStatus.Passed:
                UpdateColor(index, Color.green);
                break;
        }
    }

    void UpdateColor(int index, Color color)
    {
        this.gameObject.transform.Find($"Case {index+1}").GetComponent<Image>().color = color;
    }
}

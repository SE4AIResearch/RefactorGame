using System.Collections;
using System.Collections.Generic;
using RefactorLang;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectOrder : MonoBehaviour
{
    public CurrentKitchenState kitchen;

    public void OnSubmit() {
        string objName = this.gameObject.name;
        string testCase = objName.Substring(objName.Length - 1);

        int testCaseIndex = int.Parse(testCase) - 1;
        kitchen.UpdateCurrentTestCase(testCaseIndex);
    }
}

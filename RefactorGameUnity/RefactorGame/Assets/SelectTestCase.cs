using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectTestCase : MonoBehaviour
{
    public CurrentKitchenState kitchen;

    public void OnSubmit() {
        string objName = this.gameObject.name;
        string testCase = objName.Substring(objName.Length - 1);

        int testCaseIndex = int.Parse(testCase) - 1;

        kitchen.KitchenState.SelectedTestCase = testCaseIndex;
    }
}

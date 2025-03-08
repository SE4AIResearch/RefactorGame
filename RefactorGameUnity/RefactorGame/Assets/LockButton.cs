using System.Collections;
using System.Collections.Generic;
using RefactorLang;
using UnityEngine;
using UnityEngine.UI;

public class LockButton : MonoBehaviour
{
    public CurrentKitchenState kitchen;

    public bool shouldLock;

    // Start is called before the first frame update
    void Start()
    {
        kitchen.OnStateChanged += ButtonAccess;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ButtonAccess(KitchenState newState) {
        int index = newState.SelectedTestCase;

        if (newState.TestCaseStatus[index] == TestStatus.Running) {
            this.GetComponent<Button>().enabled = !shouldLock;

        } else {
            this.GetComponent<Button>().enabled = shouldLock;
        }
    }
}

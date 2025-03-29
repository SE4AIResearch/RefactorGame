using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RefactorLang;
using UnityEngine;

public class LevelCompletionHandler : MonoBehaviour
{
    public CurrentKitchenState kitchen;
    public GameObject overlay;

    // Start is called before the first frame update
    void Start()
    {
        kitchen.OnStateChanged += UpdateAppearance;
        overlay.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateAppearance(KitchenState state)
    {
        bool allPassed = state.TestCaseStatus.Values
            .Select(x => x == TestStatus.Passed)
            .Aggregate((x, y) => x && y);

        if (allPassed)
        {
            this.gameObject.SetActive(true);
            overlay.gameObject.SetActive(true);
        
        } else {
            this.gameObject.SetActive(false);
            overlay.gameObject.SetActive(false);
        }
    }
}

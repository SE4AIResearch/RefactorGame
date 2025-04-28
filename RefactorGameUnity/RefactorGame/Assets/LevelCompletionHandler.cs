using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RefactorLang;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompletionHandler : MonoBehaviour
{
    public CurrentKitchenState kitchen;
    public GameObject overlay;
    private bool bypass = false;

    // Start is called before the first frame update
    void Start()
    {
        kitchen.OnStateChanged += UpdateAppearance;
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateAppearance(KitchenState state)
    {
        if (bypass) { return; }

        bool allPassed = state.TestCaseStatus.Values
            .Select(x => x == TestStatus.Passed)
            .Aggregate((x, y) => x && y);

        if (allPassed)
        {
            if (!SaveData.LoadedGame.LevelsBeaten.Contains(kitchen.LoadedPuzzle.Name))
            {
                SaveData.LoadedGame.LevelsBeaten.Add(kitchen.LoadedPuzzle.Name);
            }

            for (int i = 0; i < kitchen.Definitions.Count; i++)
            {
                var defHandler = this.transform.Find("UIContainer").Find("DefinitionContainer").Find($"Definition{i+1}").GetComponent<DefinitionHandler>();
                defHandler.DisplayDefinition(kitchen.Definitions[i]);
            }

            this.gameObject.SetActive(true);
            overlay.gameObject.SetActive(true);

        }
    }

    public void Dismiss()
    {
        bypass = true;
        this.gameObject.SetActive(false);
        overlay.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        kitchen.OnStateChanged -= UpdateAppearance;
    }
}

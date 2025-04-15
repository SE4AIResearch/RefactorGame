using RefactorLang;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class AdditionalConstraintCounter : MonoBehaviour
{
    public CurrentKitchenState CurrentKitchenState;

    // Start is called before the first frame update
    void Start()
    {
        CurrentKitchenState.OnStateChanged += DisplayAdditionalConstraint;
    }

    private void OnDestroy()
    {
        CurrentKitchenState.OnStateChanged -= DisplayAdditionalConstraint;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayAdditionalConstraint(KitchenState newState)
    {
        this.gameObject.SetActive(true);
        Puzzle puzzle = CurrentKitchenState.LoadedPuzzle;

        if (puzzle == null) return;

        switch (puzzle.Constraints.AdditionalConstraint)
        {
            case "LessModules":
                CheckLessModules(newState);
                break;
            case "IndustrialKitchen": //TODO CHECK NAME OF CONSTRAINT
                CheckIndustrialKitchen(newState);
                break;
            default:
                this.gameObject.SetActive(false);
                break;
        }
    }

    private void CheckLessModules(KitchenState state)
    {
        int moduleCount = CurrentKitchenState.ApplyLessModules(state);

        GetComponent<TextMeshProUGUI>().text = moduleCount + "/3";
    }

    private void CheckIndustrialKitchen(KitchenState state)
    {
        bool stationHasThreeModules = CurrentKitchenState.ApplyIndustrialKitchen(state);

        GetComponent<TextMeshProUGUI>().text = "condition satisfied: " + !stationHasThreeModules;
    }
}

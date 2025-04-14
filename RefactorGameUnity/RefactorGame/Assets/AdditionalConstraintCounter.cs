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
                CheckNumModules(newState);
                break;
            default:
                this.gameObject.SetActive(false);
                break;
        }
    }

    private void CheckNumModules(KitchenState state)
    {
        int moduleCount = state.Stations.Aggregate(0, (x, y) => x + y.Modules.Where(x => x.GetType().Name != "None").Count());

        GetComponent<TextMeshProUGUI>().text = moduleCount.ToString();
    }
}

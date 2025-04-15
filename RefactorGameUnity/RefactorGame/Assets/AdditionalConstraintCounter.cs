using RefactorLang;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using ParserLibrary;
using System.Text.RegularExpressions;

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
            case "LessParams" or "maxFiveLinesPerMethodMaxSevenMethods" or "maxOneIfComparison":
                break;
            default:
                this.gameObject.SetActive(false);
                break;
        }
    }

    public void DisplayAdditionalConstraint(string text, CompilationStats compilationStats)
    {
        this.gameObject.SetActive(true);
        Puzzle puzzle = CurrentKitchenState.LoadedPuzzle;

        if (puzzle == null) return;

        switch (puzzle.Constraints.AdditionalConstraint)
        {
            case "LessModules" or "IndustrialKitchen":
                break;
            case "LessParams":
                CheckLessParams(compilationStats);
                break;
            case "maxFiveLinesPerMethodMaxSevenMethods":
                CheckLessMethods(compilationStats); 
                break;
            case "maxOneIfComparison":
                CheckIfComparison(text);
                break;
            default:
                this.gameObject.SetActive(false);
                break;
        }
    }

    private void CheckLessModules(KitchenState state)
    {
        int moduleCount = CurrentKitchenState.ApplyLessModules(state);

        GetComponent<TextMeshProUGUI>().text = "modules in use: " + moduleCount + "/3";
    }

    private void CheckIfComparison(string text)
    {
        bool match = new Regex("^(?!.*(\\|\\||&&| and | or )).*$").IsMatch(text.Replace("\t","").Replace("\n",""));

        GetComponent<TextMeshProUGUI>().text = "condition satisfied: " + match;
    }

    private void CheckLessParams(CompilationStats compilationStats)
    {
        int paramCount = compilationStats.NumParams;

        GetComponent<TextMeshProUGUI>().text = "parameters: " + paramCount + "/3";
    }    

    private void CheckLessMethods(CompilationStats compilationStats)
    {
        int funcCount = compilationStats.NumFDecls;

        GetComponent<TextMeshProUGUI>().text = "functions: " + funcCount + "(minimum 6)";
    }

    private void CheckIndustrialKitchen(KitchenState state)
    {
        bool stationHasThreeModules = CurrentKitchenState.ApplyIndustrialKitchen(state);

        GetComponent<TextMeshProUGUI>().text = "condition satisfied: " + !stationHasThreeModules;
    }
}

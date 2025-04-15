using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using InGameTextEditor;
using ParserLibrary;
using RefactorLang;
using RefactorLib;
using UnityEngine.UI;
using System;
using UnityEditor;
using Unity.VisualScripting;

public class SubmitText : MonoBehaviour
{
    public InGameTextEditor.TextEditor textEditor;
    public GameObject chef;
    public ScriptCompiler compiler;
    public LineCounter Constraints;
    public AdditionalConstraintCounter AdditionalConstraintCounter;
    public CurrentKitchenState Kitchen;

    public void OnSubmit()
    {        
        var text = textEditor.Text;
        Kitchen.LastSolution = new Solution(text, Kitchen.KitchenState.Stations);

        compiler.Compile(text);

        switch (compiler.Status)
        {
            case CompilationStatus.CompilationError:
                this.transform.parent.parent.Find("ActionDisplay").GetComponent<TextMeshProUGUI>().text = compiler.Message;
                Kitchen.FinishTestWithStatus(TestStatus.Failed, text, null);
                break;

            case CompilationStatus.RuntimeError or CompilationStatus.CompilationError:
                compiler.OutputLog.Add(new UnityPackage(new UnityAction.Failure(), compiler.Message));
                goto case CompilationStatus.Success;

            case CompilationStatus.Success:
                chef.GetComponent<ChefExecute>().Execute(compiler.OutputLog, text, compiler.CompilationStats);
                Constraints.CheckLines();
                AdditionalConstraintCounter.DisplayAdditionalConstraint(text, compiler.CompilationStats);
                break;
        }
    }
}

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

public class SubmitText : MonoBehaviour
{
    public InGameTextEditor.TextEditor textEditor;
    public GameObject chef;
    public ScriptCompiler compiler;
    public LineCounter Constraints;
    public CurrentKitchenState Kitchen;

    public void OnSubmit()
    {
        string text = textEditor.Text;

        compiler.Compile(text);

        switch (compiler.Status)
        {
            case CompilationStatus.CompilationError:
                this.transform.parent.parent.Find("ActionDisplay").GetComponent<TextMeshProUGUI>().text = compiler.Message;
                break;
            case CompilationStatus.RuntimeError:
                compiler.OutputLog.Add(new UnityPackage(new UnityAction.NoAction(), compiler.Message));
                goto case CompilationStatus.Success;
            case CompilationStatus.Success:
                chef.GetComponent<ChefExecute>().Execute(compiler.OutputLog, compiler.NumOfStatements);
                Constraints.CheckLines();
                Kitchen.LastSolution = text;
                break;
        }
    }
}

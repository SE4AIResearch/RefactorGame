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

    public void OnSubmit()
    {
        string text = textEditor.Text;

        compiler.Compile(text);

        chef.GetComponent<ChefExecute>().Execute(compiler.OutputLog, compiler.NumOfStatements);

        Constraints.CheckLines();
    }
}

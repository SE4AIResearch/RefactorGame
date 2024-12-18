using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ParserLibrary;
using RefactorLang;
using RefactorLib;

public class SubmitText : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject chef;

    public void OnSubmit()
    {
        List<UnityPackage> outputLog = ScriptCompiler.Compile(inputField.text);

        chef.GetComponent<ChefExecute>().Execute(outputLog);
    }
}

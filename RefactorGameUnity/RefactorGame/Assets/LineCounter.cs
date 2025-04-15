using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using RefactorLang;

public class LineCounter : MonoBehaviour
{
    public CurrentKitchenState CurrentKitchenState;
    public InGameTextEditor.TextEditor TextEditor;
    public ScriptCompiler Compiler;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckLines()
    {
        string text = TextEditor.Text;

        CheckLines(text);
    }

    public void CheckLines(string text)
    {
        Compiler.Compile(text);

        GetComponent<TextMeshProUGUI>().text = Compiler.NumOfStatements + "/" + CurrentKitchenState.LoadedPuzzle.Constraints.MaxStatements;
    }
}

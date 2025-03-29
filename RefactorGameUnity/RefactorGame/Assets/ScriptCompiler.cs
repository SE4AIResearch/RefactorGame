using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParserLibrary;
using RefactorLang;
using RefactorLib;
using Microsoft.FSharp.Collections;
using System;
using System.IO;
using C5;

public class ScriptCompiler : MonoBehaviour
{
    public CurrentKitchenState KitchenState;

    public List<UnityPackage> OutputLog;
    public int NumOfStatements;

    public void Compile(string input)
    {
        List<Token> tokens = Tokenizer.TokenizeLine(input);

        Tuple<Grammar.prog, int> compilationResult = RefactorLangParser.parseToProg(ListModule.OfSeq(tokens));

        try
        {
            var testCases = KitchenState.LoadedPuzzle.TestCases;
            var pantry = KitchenState.LoadedPuzzle.StarterPantry;
            var stations = KitchenState.KitchenState.Stations;
            
            var testCaseIndex = KitchenState.KitchenState.SelectedTestCase;
            var testCase = testCases[testCaseIndex];

            Interpreter interpreter = new Interpreter(testCase, pantry, stations);
            interpreter.Interpret(compilationResult.Item1);

            OutputLog = interpreter.OutputLog;
            NumOfStatements = compilationResult.Item2;
        }
        catch (ArgumentException ex)
        {
            Debug.Log(ex);
        }
    }
}

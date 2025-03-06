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
    public CurrentKitchenState kitchenState;

    public List<UnityPackage> Compile(string input)
    {
        List<Token> tokens = Tokenizer.TokenizeLine(input);

        string result = RefactorLangParser.parseToString(ListModule.OfSeq(tokens));

        Grammar.prog prog = RefactorLangParser.parseToProg(ListModule.OfSeq(tokens));

        try
        {
            var testCases = kitchenState.LoadedPuzzle.TestCases;
            var pantry = kitchenState.LoadedPuzzle.StarterPantry;
            var stations = kitchenState.KitchenState.Stations;
            
            var testCaseIndex = kitchenState.KitchenState.SelectedTestCase;
            var testCase = testCases[testCaseIndex];

            Interpreter interpreter = new Interpreter(testCase, pantry, stations);
            interpreter.Interpret(prog);

            return interpreter.OutputLog;
        }
        catch (ArgumentException ex)
        {
            Debug.Log(ex);
            return null;
        }
    }
}

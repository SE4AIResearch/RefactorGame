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

public enum CompilationStatus
{
    Success, CompilationError, RuntimeError
}

public class ScriptCompiler : MonoBehaviour
{
    public CurrentKitchenState KitchenState;

    public List<UnityPackage> OutputLog;
    public int NumOfStatements;
    public CompilationStatus Status;
    public string Message;

    public void Compile(string input)
    {
        List<Token> tokens = Tokenizer.TokenizeLine(input);

        Tuple<Grammar.prog, int> compilationResult;

        try
        {
            compilationResult = RefactorLangParser.parseToProg(ListModule.OfSeq(tokens));
        }
        catch {
            Status = CompilationStatus.CompilationError;
            return;
        }

        var testCases = KitchenState.LoadedPuzzle.TestCases;
        var pantry = KitchenState.LoadedPuzzle.StarterPantry;
        var stations = KitchenState.KitchenState.Stations;

        var testCaseIndex = KitchenState.KitchenState.SelectedTestCase;
        var testCase = testCases[testCaseIndex];

        Interpreter interpreter = new Interpreter(testCase, pantry, stations);
        Message = "OK";

        try
        {
            interpreter.Interpret(compilationResult.Item1);
            Status = CompilationStatus.Success;
            Message = "Compilation Error!"; // TODO: more verbose (needs to be from compiler itself)
        }
        catch (ArgumentException ex)
        {
            Status = CompilationStatus.RuntimeError;
            Debug.Log(ex);
            Message = ex.Message;
        }

        OutputLog = interpreter.OutputLog;
        NumOfStatements = compilationResult.Item2;
    }
}

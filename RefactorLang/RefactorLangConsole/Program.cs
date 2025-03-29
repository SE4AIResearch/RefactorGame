using System.Linq;

using RefactorLib;
using ParserLibrary;
using RefactorLang;
using Microsoft.FSharp.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace RefactorLangConsole
{

    public class Program
    {
        static void Main(string[] args)
        {
            TestMain();
        }

        static void TestMain()
        {
            List<StationSignature> stations = [
                new ("Station 1", [ new ModuleSignature("Slicer", "A", true) ] )
            ];

            List<List<string>> testCases = [["Potato Soup"], ["Tomato Soup"]];
            List<string> pantry = ["Broth", "Broth", "Potato", "Tomato"];

            Puzzle puzzle = new Puzzle("One Soup Two Soup", 1, 1, stations, testCases, pantry, File.ReadAllText("script.txt"), "chef go whee");

            //Puzzle.Serialize(puzzle);

            //puzzle = Puzzle.Deserialize(@".\samplePuzzle.json");

            Test(puzzle);
        }
        

        static void Test(Puzzle puzzle)
        {
            string text = puzzle.StarterCode;
            List<Token> tokens = Tokenizer.TokenizeLine(text);

            string result = RefactorLangParser.parseToString(ListModule.OfSeq(tokens));
            Console.WriteLine(result);

            Tuple<Grammar.prog, int> compilationResult = RefactorLangParser.parseToProg(ListModule.OfSeq(tokens));

            Console.WriteLine("STMT COUNT: " + compilationResult.Item2);

            foreach (List<string> testCase in puzzle.TestCases)
            {
                Console.WriteLine("=========================");
                Console.WriteLine("TEST CASE: " + String.Join(", ", testCase));

                Interpreter interpreter = new Interpreter(testCase, puzzle.StarterPantry, puzzle.Stations.Select(x => x.ConvertToStation()).ToList());

                try
                {
                    interpreter.Interpret(compilationResult.Item1);

                    interpreter.PrintOutput();
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("=========================");
                    interpreter.PrintOutput();
                }
            }
        }
    }
}
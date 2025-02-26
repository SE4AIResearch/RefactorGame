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
            List<StationSignature> stations = [
                new ("Station 1", [ new ModuleSignature("SoupMaker", "A", true) ] )
            ];

            Puzzle puzzle = new Puzzle("One Soup Two Soup", 1, 1, stations, [["Potato Soup"], ["Tomato Soup"]], ["Broth", "Broth", "Potato", "Tomato"], File.ReadAllText("script.txt"), "chef go whee");

            Puzzle.Serialize(puzzle);

            puzzle = Puzzle.Deserialize(@".\samplePuzzle.json");

            Test(puzzle);
        }
        

        static void Test(Puzzle puzzle)
        {
            string text = puzzle.StarterCode;
            List<Token> tokens = Tokenizer.TokenizeLine(text);

            string result = RefactorLangParser.parseToString(ListModule.OfSeq(tokens));
            Console.WriteLine(result);

            Grammar.prog prog = RefactorLangParser.parseToProg(ListModule.OfSeq(tokens));

            List<List<string>> testCases = puzzle.TestCases;
            List<string> pantry = puzzle.StarterPantry;

            foreach (List<string> testCase in testCases)
            {
                Console.WriteLine("=========================");
                Console.WriteLine("TEST CASE: " + String.Join(", ", testCase));

                Interpreter interpreter = new Interpreter(testCase, pantry);

                try
                {
                    interpreter.Interpret(prog);

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
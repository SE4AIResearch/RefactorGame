using System.Linq;

using RefactorLib;
using ParserLibrary;
using RefactorLang;
using Microsoft.FSharp.Collections;

namespace RefactorLangConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string text = File.ReadAllText("./script.txt");
            List<Token> tokens = Tokenizer.TokenizeLine(text);

            string result = Parser.parse(ListModule.OfSeq(tokens));
            Console.WriteLine(result);
        }
    }
}
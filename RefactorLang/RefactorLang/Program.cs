using System.Linq;

namespace RefactorLang
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string text = File.ReadAllText("./script.txt");
            List<Token> tokens = Tokenizer.TokenizeLine(text);
            IExp[] expressions = Parser.Parse(tokens.ToArray());
        }
    }
}
namespace RefactorLang
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] lines = File.ReadAllLines("./script.txt");
            TokenizeLine(lines[0]);
        }

        public static List<Token> TokenizeLine(string text)
        {
            List<Token> output = new();

            Dictionary<string, Symbol> tokenLookup = new Dictionary<string, Symbol>
            {
                { "num", Symbol.TNUM },
                { "=", Symbol.EQ },
                { "+", Symbol.PLUS },
                { "-", Symbol.DASH },
            };

            string[] words = text.Split(' ');
            foreach (string word in words)
            {
                if (tokenLookup.TryGetValue(word, out Symbol symbol))
                {
                    output.Add(new Token.TokenSymbol(symbol));
                }
                else if (int.TryParse(word, out int number))
                {
                    output.Add(new Token.TokenNumber(number));
                }
                else
                {
                    output.Add(new Token.TokenIdent(word));
                }
            }

            return output;
        }
    }

    enum Symbol
    {
        TNUM,
        EQ,
        PLUS,
        DASH
    }
    record Token
    {
        public record TokenSymbol(Symbol Symbol) : Token();
        public record TokenIdent(string Ident) : Token();
        public record TokenNumber(int Number) : Token();
    }
}
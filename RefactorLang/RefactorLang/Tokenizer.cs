using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using RefactorLib;

/*
 *      The Tokenizer goes through the provided code in the form of a script and converts the concrete syntax
 *      provided into the abstract syntax defined in the RefactorLang Specification document.
*/

namespace RefactorLang
{
    // Defines all of the reserved symbols and words required for the language.
    
    public partial class Tokenizer
    {
        // Attempts to "tokenize" a line of RefactorLang by converting words and symbols into a list of tokens.
        public static List<Token> TokenizeLine(string text)
        {
            List<Token> output = new();

            // Conversion list from concrete syntax to Symbols.
            Dictionary<string, Symbol> tokenLookup = new()
            {
                // Declarations
                { "var", Symbol.VAR },
                { "func", Symbol.FUNC },
                { "class", Symbol.CLASS },
                { "static", Symbol.STATIC },
                { "field", Symbol.FIELD },

                // Assignment
                { "=", Symbol.EQ },

                // Return
                { "return", Symbol.RETURN },

                // Booleans
                { "true", Symbol.TRUE },
                { "false", Symbol.FALSE },

                // Boolean Operators
                { "==", Symbol.EQEQ },
                { "!=", Symbol.NEQ },
                { "&&", Symbol.AND },
                { "and", Symbol.AND },
                { "||", Symbol.OR },
                { "or", Symbol.OR },
                { "!", Symbol.NOT },
                { "not", Symbol.NOT },

                // Numeric Operators
                { "+", Symbol.PLUS },
                { "-", Symbol.DASH },
                { "*", Symbol.STAR },
                { "/", Symbol.FSLASH },
                { "%", Symbol.MOD },

                // Containers
                { "{", Symbol.LBRACE },
                { "}", Symbol.RBRACE },
                { "(", Symbol.LPAREN },
                { ")", Symbol.RPAREN },
                { "[", Symbol.LBRACK },
                { "]", Symbol.RBRACK },

                // Conditional Statements
                { "if", Symbol.IF },
                { "else", Symbol.ELSE },

                // Loops
                { "while", Symbol.WHILE },
                { "for", Symbol.FOR },
                { "in", Symbol.IN },

                // Puncuation
                { ",", Symbol.COMMA },
                { ".", Symbol.DOT },

                // EOL
                { "\r\n", Symbol.EOL }
            };

            // Modifies tabs or \r\n's
            string replaced = text.Replace("\t", "").Replace("\r\n", " \r\n ");

            // Applies a regex that matches words, numbers, commas (), [], {}
            // Splitting them into an array
            // Experiment with / Learn about regex used: https://regex101.com/r/4zzBu2/6
            string[] words = ParserRegex().Matches(replaced)
                .Cast<Match>()
                .Select(m => m.Value == "\n" ? "\r\n" : m.Value)
                .ToArray();

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
                else if (word.StartsWith("\"") && word.EndsWith("\""))
                {
                    output.Add(new Token.TokenString(word.Remove(word.Length - 1).Remove(0,1)));
                }
                else if (word != "")
                {
                    output.Add(new Token.TokenIdent(word));
                }
            }

            return output.Append(new Token.TokenSymbol(Symbol.EOF)).ToList();
        }

        [GeneratedRegex("((\".*?\")|\\n|[a-zA-Z0-9_]+(\\.[0-9]+)?|[\\(\\)\\[\\]\\{\\}]|==|!=|=|&&|\\|\\||!|\\S+?(?:,\\S+?)*)")]
        private static partial Regex ParserRegex();
    }
}

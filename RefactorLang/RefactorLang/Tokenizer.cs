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
            List<Token> output = new List<Token>();

            // Conversion list from concrete syntax to Symbols.
            Dictionary<string, Symbol> tokenLookup = new Dictionary<string, Symbol>()
            {
                // Declarations
                { "var", Symbol.VAR },
                { "func", Symbol.FUNC },

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
                { ">", Symbol.GT },
                { ">=", Symbol.GTE },
                { "<", Symbol.LT },
                { "<=", Symbol.LTE },

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

                // Puncuation
                { ",", Symbol.COMMA },
                { ".", Symbol.DOT },

                // EOL
                { "\r\n", Symbol.EOL }
            };

            // Conversion list from concrete syntax to Symbols.
            Dictionary<string, Keyword> keywordLookup = new Dictionary<string, Keyword>()
            {
                { "GOTO", Keyword.GOTO },
                { "GET", Keyword.GET },
                { "PUT", Keyword.PUT },
                { "TAKE", Keyword.TAKE },
                { "ACTIVATE", Keyword.ACTIVATE },
                { "DELIVER", Keyword.DELIVER },
            };

            // Modifies tabs or \r\n's
            string replaced = text.Replace("\t", "").Replace("\r\n", " \r\n ");

            Regex ParserRegex = new Regex("((\".*?\")|\\n|[a-zA-Z0-9_]+(\\.[0-9]+)?|[\\(\\)\\[\\]\\{\\}]|>=|>|<=|<|==|!=|=|&&|\\|\\||!|\\S+?(?:,\\S+?)*)");

            // Applies a regex that matches words, numbers, commas (), [], {}
            // Splitting them into an array
            // Note: Accounts for floats, even if not fully implemented
            // Experiment with / Learn about regex used: https://regex101.com/r/4zzBu2/6
            string[] words = ParserRegex.Matches(replaced)
                .Cast<Match>()
                .Select(m => m.Value == "\n" ? "\r\n" : m.Value)
                .ToArray();

            foreach (string word in words)
            {
                if (tokenLookup.TryGetValue(word, out Symbol symbol))
                {
                    output.Add(new Token.TokenSymbol(symbol));
                }
                else if (keywordLookup.TryGetValue(word, out Keyword keyword))
                {
                    output.Add(new Token.TokenKeyword(keyword));
                }
                else if (word.StartsWith("\"") && word.EndsWith("\""))
                {
                    output.Add(new Token.TokenString(word.Remove(word.Length - 1).Remove(0,1)));
                }
                else if (int.TryParse(word, out int number))
                {
                    output.Add(new Token.TokenNumber(number));
                }
                // do NOT delete the second condition, it's not actually an empty string
                else if (word != "" && word != "​")
                {
                    output.Add(new Token.TokenIdent(word));
                }
            }

            return output.Append(new Token.TokenSymbol(Symbol.EOF)).ToList();
        }
    }
}

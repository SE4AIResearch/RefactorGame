using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

/*
 *      The Parser is provided with a list of tokens and recursively builds an AST (abstract syntax tree) out of them.
 *      The AST specification is also found in the companion document.
*/

namespace RefactorLang
{
    // The IExp interface binds all of the following expressions to make them work with a single root parsing function.
    public interface IExp { }

    record IExpList<T>(List<T> IExps) : IExp;

    record Prog(IExpList<Class> Classes) : IExp;

    record Class(string Ident, IExpList<Decl> Decls) : IExp;

    record Decl : IExp
    {
        public record MethodDecl(bool Inst, string Ident, List<string> Args, Block Block) : Decl();

        public record FieldDecl(bool Inst, string Ident) : Decl();
    }

    record Block(List<Stmt> Stmts) : IExp;

    record Stmt : IExp
    {
        public record VDecl(string Ident, Exp Value) : Stmt();
        public record Assn(string Ident, Exp Value) : Stmt();
        public record IfStmt(Exp IfExp, Block IfBlock) : Stmt();
    }

    record Exp : IExp
    {
        public record CNum(int Number) : Exp();
        public record CBool(bool Bool) : Exp();
        public record CVar(string Ident) : Exp();
        public record Binop(BinaryOperator Bop, Exp Left, Exp Right) : Exp();
    }

    public enum BinaryOperator { Add, Sub, Equals, NotEquals }

    // A Terminal is an endpoint of the recursive function, that can't be parsed any further.
    record Terminal : IExp
    {
        public record TBop(BinaryOperator Bop) : Terminal();
    }

    internal class Parser
    {
        // The Emit function pushes one token at a time into the defined "matcher" function, which will eventually match
        // its expected pattern. It will return the new matched token, and update the tokens array to remove all matched tokens.
        // If all tokens are pushed and it fails to match, the matcher should throw ArgumentOutOfRangeException, to be catched here.
        public static T Emit<T>(ref IExp[] tokens, Func<IExp[], T> matcher) where T : IExp
        {
            Queue<IExp> partition = new();
            Queue<IExp> rest = new(tokens);

            hijack:
            try
            {
                partition.Enqueue(rest.Dequeue());
                tokens = rest.ToArray();
                return matcher(partition.ToArray());
            }
            catch (ArgumentOutOfRangeException e)
            {
                if (rest.ToList().Count == 0)
                {
                    tokens = partition.ToArray();
                    throw e;
                }
                goto hijack;
            }
        }

        // EmitList does something similar to Emit, except it will keep going after it has matched once, continuing to match.
        // It can also return an empty list.
        // "separators" is a list of possible symbols that are allowed to be found in-between instances of the searched token.
        public static IExpList<T> EmitList<T>(ref IExp[] tokens, Func<IExp[], T> matcher, List<Symbol>? separators = null) where T : IExp
        {
            List<T> result = new();

            liststep:
            try
            {
                while (tokens.Length > 0)
                {
                    T token = Emit<T>(ref tokens, matcher);
                    result.Add(token);
                    goto liststep;
                }
                return new IExpList<T>(result);
            } 
            catch (ArgumentOutOfRangeException)
            {
                if (separators is not null && tokens is [Token.TokenSymbol(Symbol symbol), .. var rest] && separators.Contains(symbol)) {
                    tokens = tokens.Skip(1).ToArray();
                    goto liststep;
                }
                return new IExpList<T>(result);
            }
        }

        // This function will begin the recursive build of the AST, starting with the root node (Prog).
        public static Prog Parse(List<Token> tokens)
        {
            IExp[] tokensArray = tokens.ToArray();
            Prog prog = Emit(ref tokensArray, MatchProg);

            return prog;
        }

        private static Prog MatchProg(IExp[] tokens) => tokens switch
        {
            [.. var body, Token.TokenSymbol(Symbol.EOF)] =>
                new Prog(EmitList(ref body, MatchClass, new List<Symbol> { Symbol.EOL })),
            _ => throw new ArgumentOutOfRangeException()
        };

        private static Class MatchClass(IExp[] tokens) => tokens switch
        {
            [Token.TokenSymbol(Symbol.CLASS), Token.TokenIdent id, Token.TokenSymbol(Symbol.LBRACE), .. var decls, Token.TokenSymbol(Symbol.RBRACE)] =>
                new Class(id.Ident, EmitList(ref decls, MatchDecl, new List<Symbol> { Symbol.EOL })),
            _ => throw new ArgumentOutOfRangeException()
        };

        private static Decl MatchDecl(IExp[] tokens) => tokens switch
        {
            [Token.TokenSymbol(Symbol.STATIC), Token.TokenSymbol(Symbol.FIELD), Token.TokenIdent id, Token.TokenSymbol(Symbol.EOL)] =>
                new Decl.FieldDecl(true, id.Ident),
            [Token.TokenSymbol(Symbol.FIELD), Token.TokenIdent id, Token.TokenSymbol(Symbol.EOL)] =>
                new Decl.FieldDecl(false, id.Ident),
            [Token.TokenSymbol(Symbol.FUNC), Token.TokenIdent id, Token.TokenSymbol(Symbol.LPAREN), .. var rest]
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

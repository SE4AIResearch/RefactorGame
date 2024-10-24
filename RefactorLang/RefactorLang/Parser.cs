using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

using RefactorLib;

/*
 *      The Parser is provided with a list of tokens and recursively builds an AST (abstract syntax tree) out of them.
 *      Companion Document:
 *      https://stevens0-my.sharepoint.com/:w:/r/personal/dvanhise_stevens_edu/_layouts/15/doc2.aspx?sourcedoc=%7B8e3cb7d8-9aa1-48bd-b697-407ddcaaab71%7D
 *      The AST specification is also found in the companion document.
 *      The root node of the AST is defined as Prog.
*/

namespace RefactorLang
{

    // IExpList binds a group of a single kind of IExp together as a single IExp.
    record IExpList<T>(List<T> IExps) : IExp;

    // The following records are interpretations of the Grammar section in the companion document.
    record Prog(IExpList<Class> Classes) : IExp;

    record Class(string Ident, IExpList<Decl> Decls) : IExp;

    record Decl : IExp
    {
        public record MethodDecl(bool Inst, string Ident, List<string> Args, Block Block) : Decl();

        public record FieldDecl(bool Inst, string Ident) : Decl();
    }

    record Block(IExpList<Stmt> Stmts) : IExp;

    record Stmt : IExp
    {
        public record VDecl(string Ident, Exp Value) : Stmt();
        public record Assn(string Ident, Exp Value) : Stmt();
        public record IfStmt(Exp IfExp, Block IfBlock) : Stmt();
        public record ReturnExp(Exp Exp) : Stmt();
        public record Return() : Stmt();
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
            catch (Exception e) when (e is InvalidOperationException || e is ArgumentOutOfRangeException)
            {
                if (rest.ToList().Count == 0)
                {
                    tokens = partition.ToArray();
                    throw;
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

        private static BinaryOperator? InlineBop(Symbol symbol) => symbol switch
        {
            Symbol.PLUS => BinaryOperator.Add,
            _ => null
        };

        // This function will begin the recursive build of the AST, starting with the root node (Prog).
        public static Prog Parse(List<Token> tokens)
        {
            IExp[] tokensArray = tokens.ToArray();
            Prog prog = Emit(ref tokensArray, MatchProg);

            return prog;
        }

        // The following functions will recursively match more granular parts of the language.
        private static Prog MatchProg(IExp[] tokens) => tokens switch
        {
            [.. var body, Token.TokenSymbol(Symbol.EOF)] =>
                new Prog(EmitList(ref body, MatchClass, new List<Symbol> { Symbol.EOL })),
            _ => throw new ArgumentOutOfRangeException()
        };

        private static Class MatchClass(IExp[] tokens)
        {
            switch (tokens) {
                case [Token.TokenSymbol(Symbol.CLASS), Token.TokenIdent(string id), Token.TokenSymbol(Symbol.LBRACE), .. var rest]:
                    IExpList<Decl> decls = EmitList(ref rest, MatchDecl, new List<Symbol> { Symbol.EOL });
                    if (rest.Where(x => x is not Token.TokenSymbol(Symbol.EOL)).ToArray() is not [Token.TokenSymbol(Symbol.RBRACE)])
                        throw new ArgumentOutOfRangeException();
                    return new Class(id, decls);
                default:
                    throw new ArgumentOutOfRangeException();
             }
        }

        private static Token.TokenIdent MatchArg(IExp[] tokens) => tokens switch
        {
            [Token.TokenIdent id, .. var rest] => id,
            _ => throw new ArgumentOutOfRangeException()
        };

        private static Decl MatchDecl(IExp[] tokens)
        {
            IExpList<Token.TokenIdent> ids;
            switch (tokens)
            {
                case [Token.TokenSymbol(Symbol.STATIC), Token.TokenSymbol(Symbol.FIELD), Token.TokenIdent(string id), Token.TokenSymbol(Symbol.EOL)]:
                    return new Decl.FieldDecl(true, id);
                case [Token.TokenSymbol(Symbol.FIELD), Token.TokenIdent(string id), Token.TokenSymbol(Symbol.EOL)]:
                    return new Decl.FieldDecl(false, id);
                case [Token.TokenSymbol(Symbol.FUNC), Token.TokenIdent(string id), Token.TokenSymbol(Symbol.LPAREN), .. var rest]:
                    ids = EmitList(ref rest, MatchArg, new List<Symbol> { Symbol.COMMA });
                    if (rest is [Token.TokenSymbol(Symbol.RPAREN), .. var block])
                        return new Decl.MethodDecl(false, id, ids.IExps.Select(x => x.Ident).ToList(), Emit(ref block, MatchBlock));
                    else throw new ArgumentOutOfRangeException();
                case [Token.TokenSymbol(Symbol.STATIC), Token.TokenSymbol(Symbol.FUNC), Token.TokenIdent(string id), Token.TokenSymbol(Symbol.LPAREN), .. var rest]:
                    ids = EmitList(ref rest, MatchArg, new List<Symbol> { Symbol.COMMA });
                    if (rest is [Token.TokenSymbol(Symbol.RPAREN), .. var blockS])
                        return new Decl.MethodDecl(true, id, ids.IExps.Select(x => x.Ident).ToList(), Emit(ref blockS, MatchBlock));
                    else throw new ArgumentOutOfRangeException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Block MatchBlock(IExp[] tokens)
        {
            if (tokens.Where(x => x is not Token.TokenSymbol(Symbol.EOL)).ToArray() is not [Token.TokenSymbol(Symbol.LBRACE), ..])
                throw new ArgumentOutOfRangeException();

            IExp[] rest = tokens.SkipWhile(x => x is Token.TokenSymbol(Symbol.EOL)).Skip(1).ToArray();

            IExpList<Stmt> stmts = EmitList(ref rest, MatchStmt, new List<Symbol> { Symbol.EOL });

            if (rest.Where(x => x is not Token.TokenSymbol(Symbol.EOL)).ToArray() is not [Token.TokenSymbol(Symbol.RBRACE)])
                throw new ArgumentOutOfRangeException();

            return new Block(stmts);
        }

        private static Stmt MatchStmt(IExp[] tokens)
        {
            switch (tokens)
            {
                case [Token.TokenSymbol(Symbol.RETURN), Token.TokenSymbol(Symbol.EOL)]:
                    return new Stmt.Return();
                case [Token.TokenSymbol(Symbol.RETURN), .. var exp, Token.TokenSymbol(Symbol.EOL)]:
                    return new Stmt.ReturnExp(MatchExp(exp));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Exp MatchExp(IExp[] tokens)
        {
            switch (tokens)
            {
                case [Token.TokenNumber(int num)]:
                    return new Exp.CNum(num);
                case [Token.TokenIdent(string id)]:
                    return new Exp.CVar(id);
                case [Token.TokenSymbol(Symbol.TRUE)]:
                    return new Exp.CBool(true);
                case [Token.TokenSymbol(Symbol.FALSE)]:
                    return new Exp.CBool(false);
                case [Exp exp1, Token.TokenSymbol(Symbol s), Exp exp2]:
                    BinaryOperator bop = InlineBop(s) ?? throw new ArgumentOutOfRangeException();
                    return new Exp.Binop(bop, exp1, exp2);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

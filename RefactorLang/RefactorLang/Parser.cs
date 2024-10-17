using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 *      The Parser is provided with a list of tokens and recursively builds an AST (abstract syntax tree) out of them.
 *      The AST specification is also found in the companion document.
 *      NOTE: This AST-building technique kind of needs an overhaul, which is the next thing I'm going to do. -DVH
*/

namespace RefactorLang
{
    public enum BinaryOperator { Add, Sub, Equals, NotEquals }
    
    // The IExp interface binds all of the following expressions to make them work with a single root parsing function.
    public interface IExp { }

    // A Terminal is an endpoint of the recursive function, that can't be parsed any further.
    record Terminal : IExp
    {   
        public record TBinaryOperator(BinaryOperator BinaryOperator) : Terminal();
    }

    // An Expression is a bit of code that can be evaluated in-line but that doesn't constitute a whole Statement.
    record Expression : IExp
    {
        public record CNum(int Number) : Expression();
        public record CBool(bool Bool) : Expression();
        public record CStr(string Str) : Expression();
        public record CVar(string Ident) : Expression();
        public record Binop(Terminal.TBinaryOperator BinaryOperator, Expression ExpressionLeft, Expression ExpressionRight) : Expression();
    }

    record VDecl(string Name, Expression Value) : IExp;

    // A Statement is the smallest bit of code that can stand on its own.
    record Stmt : IExp
    {
        public record Decl(VDecl VDecl) : Stmt();
        public record Assn(string Name, Expression Value) : Stmt;
        public record StmtList(List<Stmt> Stmts) : Stmt();
        public record IfStmt(Expression Exp, Block IfBlock, Block ElseBlock) : Stmt();
    }

    // A Block is a collection of Statements.
    record Block(List<Stmt> Stmts) : IExp;

    internal class Parser
    {
        public static IExp[] Parse(IExp[] atoms)
        {
            List<Func<IExp[], IEnumerable<IExp>>> ParseSteps = new List<Func<IExp[], IEnumerable<IExp>>>
            {
                ParseSimples, ParseBinaryOperators, ParseExpressions, 
                ParseVDecl, ParseStmt, ParseBlock, ParseIfStmt, ParseStmt
            };

            IEnumerable<IExp> newExp = atoms;

            foreach (var action in ParseSteps)
            {
                newExp = ParseSingle(action, newExp.ToArray(), Array.Empty<IExp>());
            }

            return newExp.ToArray();
        }

        public static IExp[] ParseSingle(Func<IExp[], IEnumerable<IExp>> parser, IExp[] front, IExp[] back)
        {
            if (front.Length == 0)
                return back;

            IExp[] newExp = parser(front).ToArray();
            if (!newExp.SequenceEqual(front))
            {
                return ParseSingle(parser, back.Concat(newExp).ToArray(), new IExp[0]);
            }
            else
            {
                return ParseSingle(parser, newExp.Skip(1).ToArray(), back.Append(newExp[0]).ToArray());
            }
        }

        public static IEnumerable<IExp> ParseSimples(IExp[] atoms) => atoms.ToArray() switch
        {
            _ => atoms
        };

        public static IEnumerable<IExp> ParseBinaryOperators(IExp[] atoms) => atoms.ToArray() switch
        {
            [Token.TokenSymbol(Symbol.PLUS), .. var end] => end.Prepend(new Terminal.TBinaryOperator(BinaryOperator.Add)),
            [Token.TokenSymbol(Symbol.DASH), .. var end] => end.Prepend(new Terminal.TBinaryOperator(BinaryOperator.Sub)),
            [Token.TokenSymbol(Symbol.EQEQ), .. var end] => end.Prepend(new Terminal.TBinaryOperator(BinaryOperator.Equals)),
            [Token.TokenSymbol(Symbol.NEQ), .. var end] => end.Prepend(new Terminal.TBinaryOperator(BinaryOperator.NotEquals)),
            _ => atoms
        };

        public static IEnumerable<IExp> ParseExpressions(IExp[] atoms) => atoms.ToArray() switch
        {
            [Token.TokenNumber num, .. var end] => end.Prepend(new Expression.CNum(num.Number)),
            [Token.TokenSymbol(Symbol.TRUE), .. var end] => end.Prepend(new Expression.CBool(true)),
            [Token.TokenSymbol(Symbol.FALSE), .. var end] => end.Prepend(new Expression.CBool(false)),
            [Expression exp1, Terminal.TBinaryOperator bo, Expression exp2, .. var end] =>
                end.Prepend(new Expression.Binop(bo, exp1, exp2)),
            _ => atoms
        };

        public static IEnumerable<IExp> ParseVDecl(IExp[] atoms) => atoms.ToArray() switch
        {
            [Token.TokenSymbol(Symbol.VAR), Token.TokenIdent ident, Token.TokenSymbol(Symbol.EQ), Expression exp, .. var end] =>
               end.Prepend(new VDecl(ident.Ident, exp)),
            _ => atoms
        };

        public static IEnumerable<IExp> ParseStmt(IExp[] atoms) => atoms.ToArray() switch
        {
            [VDecl vdecl, Token.TokenSymbol(Symbol.EOL), .. var end] =>
                end.Prepend(new Stmt.Decl(vdecl)),
            [Token.TokenIdent id, Token.TokenSymbol(Symbol.EQ), Expression exp, Token.TokenSymbol(Symbol.EOL), .. var end] =>
                end.Prepend(new Stmt.Assn(id.Ident, exp)),
            [Stmt.StmtList stmts, Stmt stmt, .. var end] =>
                end.Prepend(new Stmt.StmtList(stmts.Stmts.Append(stmt).ToList())),
            [Stmt stmt1, Stmt stmt2, .. var end] =>
                end.Prepend(new Stmt.StmtList(new List<Stmt> { stmt1, stmt2 })),
            [Token.TokenSymbol(Symbol.EOL), .. var end] => end,
            _ => atoms
        };

        public static IEnumerable<IExp> ParseBlock(IExp[] atoms) => atoms.ToArray() switch
        {
            [Token.TokenSymbol(Symbol.LBRACE), Stmt.StmtList stmts, Token.TokenSymbol(Symbol.RBRACE), .. var end] =>
                end.Prepend(new Block(stmts.Stmts)),
            [Token.TokenSymbol(Symbol.LBRACE), Stmt stmt, Token.TokenSymbol(Symbol.RBRACE), .. var end] =>
                end.Prepend(new Block(new List<Stmt> { stmt })),
            _ => atoms
        };

        public static IEnumerable<IExp> ParseIfStmt(IExp[] atoms) => atoms.ToArray() switch
        {
            [Token.TokenSymbol(Symbol.IF), Token.TokenSymbol(Symbol.LPAREN), Expression exp, Token.TokenSymbol(Symbol.RPAREN),
                Block block1, Token.TokenSymbol(Symbol.ELSE), Block block2, .. var end] =>
                end.Prepend(new Stmt.IfStmt(exp, block1, block2)),
            [Token.TokenSymbol(Symbol.IF), Token.TokenSymbol(Symbol.LPAREN), Expression exp, Token.TokenSymbol(Symbol.RPAREN),
                Block block, .. var end] => end.Prepend(new Stmt.IfStmt(exp, block, new Block(new List<Stmt>()))),
            _ => atoms
        };
    }
}

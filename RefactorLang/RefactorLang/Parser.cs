using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorLang
{
    public enum Type { Number }
    public enum BinaryOperator { Add, Sub }
    public interface IExp { }
    record Terminal : IExp
    {   
        public record TType(Type Type) : Terminal();
        public record TBinaryOperator(BinaryOperator BinaryOperator) : Terminal();
    }

    record Expression : IExp
    {
        public record CNum(int Number) : Expression();
        public record CVar(string Ident) : Expression();
        public record Binop(Terminal.TBinaryOperator BinaryOperator, Expression ExpressionLeft, Expression ExpressionRight) : Expression();
    }

    record VDecl(Type Type, string Name, Expression Value) : IExp;

    internal class Parser
    {
        public static IExp[] Parse(IExp[] atoms)
        {
            List<Func<IExp[], IEnumerable<IExp>>> ParseSteps = new List<Func<IExp[], IEnumerable<IExp>>>
            {
                ParseSimples, ParseBinaryOperators, ParseExpressions, ParseVDecl
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
            [Token.TokenSymbol(Symbol.TNUM), .. var end] => end.Prepend(new Terminal.TType(Type.Number)),
            _ => atoms
        };

        public static IEnumerable<IExp> ParseBinaryOperators(IExp[] atoms) => atoms.ToArray() switch
        {
            [Token.TokenSymbol(Symbol.PLUS), .. var end] => end.Prepend(new Terminal.TBinaryOperator(BinaryOperator.Add)),
            [Token.TokenSymbol(Symbol.DASH), .. var end] => end.Prepend(new Terminal.TBinaryOperator(BinaryOperator.Sub)),
            _ => atoms
        };

        public static IEnumerable<IExp> ParseExpressions(IExp[] atoms) => atoms.ToArray() switch
        {
            [Token.TokenNumber num, .. var end] => end.Prepend(new Expression.CNum(num.Number)),
            [Expression exp1, Terminal.TBinaryOperator bo, Expression exp2, .. var end] =>
                end.Prepend(new Expression.Binop(bo, exp1, exp2)),
            _ => atoms
        };

        public static IEnumerable<IExp> ParseVDecl(IExp[] atoms) => atoms.ToArray() switch
        {
            [Terminal.TType type, Token.TokenIdent ident, Token.TokenSymbol(Symbol.EQ), Expression exp, .. var end] =>
               end.Prepend(new VDecl(type.Type, ident.Ident, exp)),
            _ => atoms
        };
    }
}

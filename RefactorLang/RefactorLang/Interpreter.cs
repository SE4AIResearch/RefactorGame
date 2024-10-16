using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorLang
{
    class Variable
    {
        public dynamic Value { get; set; }

        public Variable(dynamic value)
        {
            Value = value;
        }

        public bool TypeCheck()
        {
            return true;
        }
    }
    internal class Interpreter
    {
        public Dictionary<string, Variable> VariableState { get; }
        public List<IExp> Prog { get; }

        public Interpreter(List<IExp> prog)
        {
            VariableState = new();
            Prog = prog;
        }

        void DeclareVariable(string name, dynamic value)
        {
            if (!VariableState.TryGetValue(name, out Variable? variable))
            {
                VariableState.Add(name, new Variable(value));
            }
            else throw new ArgumentException(name + " is already defined.");
        }

        public void InterpretStmt(Stmt exp)
        {
            switch (exp)
            {
                case Stmt.StmtList list:
                    foreach (Stmt stmt in list.Stmts) InterpretStmt(stmt);
                    break;
                case Stmt.Decl(VDecl(string name, Expression value)):
                    this.DeclareVariable(name, InterpretExp(value));
                    break;
                case Stmt.IfStmt(Expression ifExp, Block ifBlock, Block elseBlock):
                    if (InterpretExp(ifExp))
                        InterpretStmt(new Stmt.StmtList(ifBlock.Stmts));
                    else
                        InterpretStmt(new Stmt.StmtList(elseBlock.Stmts));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Not a supported expression");
            }
        }

        public dynamic InterpretExp(Expression exp) => exp switch
        {
            Expression.CNum(int number) => number,
            Expression.CBool(true) => true,
            Expression.CBool(false) => false,

            Expression.Binop(Terminal.TBinaryOperator binop, Expression left, Expression right) => binop.BinaryOperator switch
            {
                BinaryOperator.Add => InterpretExp(left) + InterpretExp(right),
                BinaryOperator.Equals => InterpretExp(left) == InterpretExp(right),
                _ => throw new ArgumentOutOfRangeException("Not a supported binop")
            },

            _ => throw new ArgumentOutOfRangeException("Not a supported exp")
        };
    }
}

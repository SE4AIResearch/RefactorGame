using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefactorLib;
using ParserLibrary;
using RefactorLang;
using C5;
using static ParserLibrary.Grammar.stmt;

namespace RefactorLang
{

    public class Interpreter
    {
        private State State;
        public List<UnityPackage> OutputLog { get; }

        public Interpreter(List<FoodItem> orders, HashBag<FoodItem> shelf)
        {
            State = new State(orders, shelf);
            OutputLog = new List<UnityPackage>();
        }

        public void PrintOutput()
        {
            foreach(UnityPackage package in this.OutputLog)
            {
                Console.WriteLine(package.Message);
            }
        }

        private void RecordAction(UnityAction action, string message)
        {
            this.OutputLog.Add(new UnityPackage(action, message));
        }

        /*
         * In order to interpret the AST that is the output of Parser.fs, we must implement methods to interpret every data type that can be encountered.
         * These include:
         * • Binary operand expressions (Binop)
         * • Single expressions (Exp)
         * • Statements and lists of statements (Stmt)
         * • Special statements (KCall)
         * • Branching statements (ITE, While, etc.)
         */
        private ExpValue InterpretBinop(Grammar.exp.Binop binop)
        {
            // Binops are made up of two Exps and a binary operation. We must interpret all three.
            ExpValue InterpretBinopExpression(Grammar.exp exp1, Grammar.exp exp2, Func<ExpValue, ExpValue, ExpValue> operation)
            {
                ExpValue inp1 = InterpretExp(exp1);
                ExpValue inp2 = InterpretExp(exp2);
                return operation(inp1, inp2);
            }

            // In order to interpret binary expressions, we must substitute the abstract objects from the AST with C#'s notion of math.
            return binop.Item switch
            {
                Grammar.binop.Add b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Num, x.TypeCheckNum() + y.TypeCheckNum())
                                    ),
                Grammar.binop.Sub b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Num, x.TypeCheckNum() - y.TypeCheckNum())
                                    ),
                Grammar.binop.Mul b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Num, x.TypeCheckNum() * y.TypeCheckNum())
                                    ),
                Grammar.binop.Div b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Num, x.TypeCheckNum() / y.TypeCheckNum())
                                    ),
                Grammar.binop.Mod b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Num, x.TypeCheckNum() % y.TypeCheckNum())
                                    ),
                Grammar.binop.And b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Bool, x.TypeCheckBool() && y.TypeCheckBool())
                                    ),
                Grammar.binop.Or b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Bool, x.TypeCheckBool() || y.TypeCheckBool())
                                    ),
                Grammar.binop.Eq b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Bool, (x.TypeDef == y.TypeDef) && x.Value.Equals(y.Value))
                                    ),
                Grammar.binop.Neq b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Bool, (x.TypeDef != y.TypeDef) || !x.Value.Equals(y.Value))
                                    ),
                Grammar.binop.Gt b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Bool, x.TypeCheckNum() > y.TypeCheckNum())
                                    ),
                Grammar.binop.Gte b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Bool, x.TypeCheckNum() >= y.TypeCheckNum())
                                    ),
                Grammar.binop.Lt b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Bool, x.TypeCheckNum() < y.TypeCheckNum())
                                    ),
                Grammar.binop.Lte b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Bool, x.TypeCheckNum() <= y.TypeCheckNum())
                                    ),
                _ => throw new ArgumentOutOfRangeException("BINOP NOT SUPPORTED"),
            };
        }

        private ExpValue InterpretExp(Grammar.exp exp)
        {
            // Most Exps are simplified to variable names, but there are a few more complicated ones.
            switch (exp)
            {
                case Grammar.exp.CBool v:
                    return new ExpValue(ExpValue.Type.Bool, v.Item);
                case Grammar.exp.CStr v:
                    return new ExpValue(ExpValue.Type.Str, v.Item);
                case Grammar.exp.CNum v:
                    return new ExpValue(ExpValue.Type.Num, v.Item);
                case Grammar.exp.Binop b:
                    return InterpretBinop(b);
                case Grammar.exp.CVar v:
                    if (!State.VariableMap.TryGetValue(v.Item, out ExpValue val))
                        throw new ArgumentOutOfRangeException("that variable is not defined");
                    return val;
                case Grammar.exp.Idx v:
                    //TODO: simplified to only work with string lists for now
                    throw new ArgumentOutOfRangeException("getting there");
                case Grammar.exp.FCall v:
                    if (!State.FDecls.TryGetValue(v.Item1, out var fun))
                        throw new ArgumentOutOfRangeException("that function is not defined");

                    State.Stack.Push(State.VariableMap.ToDictionary(x => x.Key, x => x.Value));
                    ExpValue x = InterpretAllStmts(fun.Item3.ToList());
                    State.VariableMap = State.Stack.Pop();

                    return x;
                default:
                    throw new ArgumentOutOfRangeException("EXP NOT SUPPORTED: " + exp.ToString());
            }
        }

        private ExpValue InterpretStmt(Grammar.stmt stmt)
        {
            ExpValue x;

            // Statements are complex and relegated to their own interpretation functions.
            switch (stmt)
            {
                case Grammar.stmt.FDecl fdecl:
                    if (State.VariableMap.ContainsKey(fdecl.Item1) || State.FDecls.ContainsKey(fdecl.Item1))
                        throw new ArgumentException($"variable {fdecl.Item1} is already defined");
                    State.FDecls.Add(fdecl.Item1, fdecl);
                    goto retnone;
                case Grammar.stmt.VDecl vdecl:
                    if (State.VariableMap.ContainsKey(vdecl.Item1))
                        throw new ArgumentException($"variable {vdecl.Item1} is already defined");
                    x = InterpretExp(vdecl.Item2);
                    State.VariableMap.Add(vdecl.Item1, x);
                    goto retnone;
                case Grammar.stmt.Assn assn:
                    if (!State.VariableMap.ContainsKey(assn.Item1))
                        throw new ArgumentException($"variable {assn.Item1} is not defined");
                    x = InterpretExp(assn.Item2);
                    State.VariableMap[assn.Item1] = x;
                    goto retnone;
                case Grammar.stmt.KCall kCall:
                    InterpretKCall(kCall);
                    goto retnone;
                case Grammar.stmt.IfThenElse ite:
                    InterpretITE(ite);
                    goto retnone;
                case Grammar.stmt.While wh:
                    InterpretWhile(wh);
                    goto retnone;
                case Grammar.stmt.RetVal rv:
                    return InterpretExp(rv.Item);
                
                default:
                    throw new ArgumentOutOfRangeException("STMT NOT SUPPORTED");

                retnone:
                    return new ExpValue(ExpValue.Type.None, null);
            }
        }

        // Statements are the only void actions that can be chained so far (called a Block), so we need to make sure to handle that properly.
        private ExpValue InterpretAllStmts(List<Grammar.stmt> stmts)
        {
            foreach (Grammar.stmt stmt in stmts)
            {
                ExpValue x = InterpretStmt(stmt);
                if (!x.TypeDef.Equals(new ExpValue.ExpType.Single(ExpValue.Type.None)))
                    return x;
            }

            return new ExpValue(ExpValue.Type.None, null);
        }

        // KCalls are what eventually become our UnityActions. This is where the actual gameplay takes place.
        // TODO: Clean all of this up, this function is a monster :)
        private void InterpretKCall(Grammar.stmt.KCall stmt)
        {
            void CheckArguments(int num)
            {
                if (stmt.Item2.Length != num) throw new ArgumentException("wrong number of arguments");
            }

            List<ExpValue> EvaluateArguments()
            {
                List<ExpValue> output = new List<ExpValue>();
                foreach (Grammar.exp exp in stmt.Item2)
                {
                    output.Add(InterpretExp(exp));
                }
                return output;
            }
            List<ExpValue> args = EvaluateArguments();

            switch (stmt.Item1)
            {
                case Keyword.PRINT:
                    CheckArguments(1);
                    RecordAction(new UnityAction.NoAction(), args[0].Value.ToString());
                    Console.WriteLine(args[0].Value.ToString());
                    break;
                case Keyword.GOTO:
                    {
                        throw new ArgumentOutOfRangeException("goto was unimplemented");

                        break;
                    }
                case Keyword.GET:
                    {
                        CheckArguments(1);
                        if (!this.State.ChefLocation.Equals(new ChefLocation.Pantry()))
                            throw new ArgumentException("chef not at pantry, can't GET");
                        FoodItem food = new FoodItem.Some(args[0].TypeCheckString());
                        if (!this.State.Shelf.Contains(food))
                            throw new ArgumentException("pantry does not contain " + food + ", can't GET");

                        this.State.ChefHands = food;
                        this.State.Shelf.Remove(food);
                        RecordAction(new UnityAction.PickUp(), "Chef picks up " + food);

                        break;
                    }
                case Keyword.DELIVER:
                    {
                        CheckArguments(0);

                        if (!this.State.ChefLocation.Equals(new ChefLocation.Window()))
                            throw new ArgumentException("can't deliver, chef isn't at the window");
                        if (this.State.ChefHands.Equals(new FoodItem.None()))
                            throw new ArgumentException("can't deliver, chef isn't holding anything");

                        this.State.DeliveredOrders.Add(this.State.ChefHands);
                        RecordAction(new UnityAction.PutDown(), "Chef delivers " + this.State.ChefHands);
                        this.State.ChefHands = new FoodItem.None();

                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException("KCALL NOT SUPPORTED");
            }
        }

        private void InterpretITE(Grammar.stmt.IfThenElse stmt)
        {
            // if block
            ExpValue condition = InterpretExp(stmt.Item1);
            bool conditionResult = condition.TypeCheckBool();

            if (conditionResult)
            {
                InterpretAllStmts(stmt.Item2.ToList());
                return;
            }

            // elseif blocks
            foreach (var item in stmt.Item3)
            {
                ExpValue elseCondition = InterpretExp(item.Item1);
                bool elseConditionResult = elseCondition.TypeCheckBool();

                if (elseConditionResult)
                {
                    InterpretAllStmts(item.Item2.ToList());
                    return;
                }
            }

            // else block
            if (stmt.Item4 != null)
                InterpretAllStmts(stmt.Item4.Value.ToList());
        }

        private void InterpretWhile(Grammar.stmt.While stmt) {
            WhileFlag:
            ExpValue condition = InterpretExp(stmt.Item1);
            bool conditionResult = condition.TypeCheckBool();

            if (conditionResult)
            {
                InterpretAllStmts(stmt.Item2.ToList());
                goto WhileFlag;
            }
        }

        // Front-facing entry point for interpreting a program
        public void Interpret(Grammar.prog prog)
        {
            InterpretAllStmts(prog.Item.ToList());

            if (Enumerable.SequenceEqual(this.State.Orders, this.State.DeliveredOrders))
                RecordAction(new UnityAction.NoAction(), "Success!");
            else RecordAction(new UnityAction.NoAction(), "Failure...");
        }
    }
}

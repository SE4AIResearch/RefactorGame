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
using static RefactorLang.ChefLocation;

namespace RefactorLang
{



    public class Interpreter
    {
        private State State;
        public List<UnityPackage> OutputLog { get; }

        public Interpreter(List<string> orders, List<string> shelf)
        {
            HashBag<FoodItem> foodItems = new HashBag<FoodItem>();

            foodItems.AddAll(shelf.Select(x => new FoodItem.Some(x)).ToList<FoodItem>());

            State = new State(orders.Select(x => new FoodItem.Some(x)).ToList<FoodItem>(), foodItems);
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

        private ExpValue InterpretUnop(Grammar.exp.Unop unop)
        {
            ExpValue InterpretUnopExpression(Grammar.exp exp, Func<ExpValue, ExpValue> operation)
            {
                ExpValue inp = InterpretExp(exp);
                return operation(inp);
            }

            return unop.Item switch
            {
                Grammar.unop.Not b => InterpretUnopExpression(b.Item,
                                        (x) => new ExpValue(ExpValue.Type.Bool, !x.TypeCheckBool())
                                    ),
                Grammar.unop.Neg b => InterpretUnopExpression(b.Item,
                                        (x) => new ExpValue(ExpValue.Type.Num, -x.TypeCheckNum())
                                    ),
                Grammar.unop.Len b => InterpretUnopExpression(b.Item,
                                        (x) => new ExpValue(ExpValue.Type.Num, x.TypeCheckList().GetType().GetProperty("Count").GetValue(x.TypeCheckList()))
                                    ),
                _ => throw new ArgumentOutOfRangeException("UNOP NOT SUPPORTED"),
            };
        }

        private ExpValue InterpretExp(Grammar.exp exp)
        {
            ExpValue val;

            // Most Exps are simplified to variable names, but there are a few more complicated ones.
            switch (exp)
            {
                case Grammar.exp.CBool v:
                    return new ExpValue(ExpValue.Type.Bool, v.Item);
                case Grammar.exp.CStr v:
                    return new ExpValue(ExpValue.Type.Str, v.Item);
                case Grammar.exp.CNum v:
                    return new ExpValue(ExpValue.Type.Num, v.Item);
                case Grammar.exp.Binop v:
                    return InterpretBinop(v);
                case Grammar.exp.Unop v:
                    return InterpretUnop(v);
                case Grammar.exp.CVar v:
                    if (!State.VariableMap.TryGetValue(v.Item, out val))
                        throw new ArgumentOutOfRangeException("that variable is not defined");
                    return val;
                case Grammar.exp.Idx v:
                    if (!State.VariableMap.TryGetValue(v.Item1, out val))
                        throw new ArgumentOutOfRangeException("that variable is not defined");
                    object list = val.TypeCheckList();
                    ExpValue.Type type = val.PokeType();
                    return new ExpValue(type, list.GetType().GetProperty("Item").GetValue(list, new object[] { InterpretExp(v.Item2).TypeCheckNum() }));
                case Grammar.exp.FCall v:
                    if (!State.FDecls.TryGetValue(v.Item1, out var fun))
                        throw new ArgumentOutOfRangeException("that function is not defined");

                    State.Stack.Push(State.VariableMap.ToDictionary(x => x.Key, x => x.Value));

                    for (int i = 0; i < v.Item2.Count(); i++)
                        State.VariableMap.Add(fun.Item2[i], InterpretExp(v.Item2[i]));

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

            ChefLocation StringToLocation(string str)
            {
                switch (str)
                {
                    case "Pantry":
                        return new ChefLocation.Pantry();
                    case "Window":
                        return new ChefLocation.Window();
                    default:
                        break;
                }

                try
                {
                    Station station = StringToStation(str);
                    return new ChefLocation.Station(station.Name);
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException($"No locations by that name ({str})");
                }
            }

            Station StringToStation(string str)
            {
                return State.Stations.Find(station => station.Name == str);

                throw new ArgumentException($"No stations by that name ({str})");
            }

            Module StringToModule(string str, Station station)
            {
                return station.Modules.Find(module => module.Name == str);

                throw new ArgumentException($"No modules by that name ({str}) in this station ({station.Name})");
            }

            string StringOfLocation(ChefLocation loc)
            {
                return loc switch
                {
                    ChefLocation.Station s => s.Name,
                    ChefLocation.Pantry => "Pantry",
                    ChefLocation.Window => "Window",
                    _ => throw new ArgumentException("what")
                };
            }

            string StringOfFoodItem(FoodItem foodItem)
            {
                return foodItem switch
                {
                    FoodItem.Some food => food.Food,
                    FoodItem.None => "None",
                    _ => throw new ArgumentException("what")
                };
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
                    CheckArguments(1);
                    ChefLocation location = StringToLocation(args[0].TypeCheckString());
                    State.ChefLocation = location;
                    RecordAction(new UnityAction.ChefMove(location), $"chef moves to {StringOfLocation(location)}");
                    break;
                case Keyword.GET:
                    {
                        CheckArguments(1);
                        if (State.ChefLocation is not ChefLocation.Pantry)
                            throw new ArgumentException("chef not at pantry, can't GET");
                        FoodItem food = new FoodItem.Some(args[0].TypeCheckString());
                        if (!this.State.Shelf.Contains(food))
                            throw new ArgumentException("pantry does not contain " + food + ", can't GET");

                        this.State.ChefHands = food;
                        this.State.Shelf.Remove(food);
                        RecordAction(new UnityAction.PickUp(food), "Chef picks up " + StringOfFoodItem(food));

                        break;
                    }
                case Keyword.DELIVER:
                    {
                        CheckArguments(0);

                        if (State.ChefLocation is not ChefLocation.Window)
                            throw new ArgumentException("can't deliver, chef isn't at the window");
                        if (State.ChefHands is FoodItem.None)
                            throw new ArgumentException("can't deliver, chef isn't holding anything");

                        this.State.DeliveredOrders.Add(this.State.ChefHands);
                        RecordAction(new UnityAction.PutDown(), "Chef delivers " + StringOfFoodItem(this.State.ChefHands));
                        this.State.ChefHands = new FoodItem.None();

                        break;
                    }
                case Keyword.PLACE:
                    {
                        Station station;

                        CheckArguments(2);

                        if (State.ChefLocation is ChefLocation.Station stationLoc)
                            station = StringToStation(stationLoc.Name);
                        else
                            throw new ArgumentException("can't place, chef isn't at a workstation");

                        if (State.ChefHands is FoodItem.None)
                            throw new ArgumentException("can't place, chef isn't holding anything");

                        Module module = StringToModule(args[0].TypeCheckString(), station);

                        FoodItem food = State.ChefHands;
                        module.Place(State.ChefHands, args[1].TypeCheckNum());
                        State.ChefHands = new FoodItem.None();

                        RecordAction(new UnityAction.PutDown(), $"chef puts {StringOfFoodItem(food)} in {module.Name}");

                        break;
                    }
                case Keyword.ACTIVATE:
                    {
                        CheckArguments(1);

                        Station station;

                        if (State.ChefLocation is ChefLocation.Station stationLoc)
                            station = StringToStation(stationLoc.Name);
                        else
                            throw new ArgumentException("can't activate, chef isn't at a workstation");

                        Module module = StringToModule(args[0].TypeCheckString(), station);

                        module.Activate();

                        RecordAction(new UnityAction.Use(), $"chef activates module {module.Name}");

                        break;
                    }
                case Keyword.TAKE:
                    {
                        CheckArguments(1);

                        Station station;

                        if (State.ChefLocation is ChefLocation.Station stationLoc)
                            station = StringToStation(stationLoc.Name);
                        else
                            throw new ArgumentException("can't activate, chef isn't at a workstation");

                        Module module = StringToModule(args[0].TypeCheckString(), station);

                        FoodItem output = module.Take();

                        State.ChefHands = output;

                        RecordAction(new UnityAction.PickUp(output), $"chef picks up {StringOfFoodItem(output)} from {module.Name}");

                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException("KCALL NOT SUPPORTED: " + stmt.ToString());
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

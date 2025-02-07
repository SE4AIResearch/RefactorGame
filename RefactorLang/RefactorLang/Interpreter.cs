using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefactorLib;
using ParserLibrary;
using RefactorLang;
using C5;

namespace RefactorLang
{
 
    // Contains all possible locations that the chef can be standing at any timestep.
    public enum ChefLocation
    {
        Pantry, Stove, Window
    }

    // All possible food items that can be prepared, including ingredients and unintentional products (Garbage, etc.)
    public enum FoodItem
    {
        None, Garbage, Pasta, BoiledPasta, Sauce, PastaWithSauce, Potato, BoiledPotato
    }

    // The UnityPackage record is just a pair of a UnityAction and a debug message to be logged and shown to the player as a record of chef actions.
    // The final output of compilation is a list of UnityPackages as a list of instructions for the chef.
    public record UnityPackage(UnityAction Action, string Message);
    
    // A UnityAction is directly interpreted by the Unity frontend of the game to be shown to the player.
    public record UnityAction
    {
        // The chef moves to a new location.
        public record ChefMove(ChefLocation Destination) : UnityAction;

        // The chef picks up a food item and visually holds it.
        public record PickUp(FoodItem Food) : UnityAction;

        // The chef plays a generic "Use" animation. (This might eventually change to contextual Use animations, like "Use Stove".)
        public record Use() : UnityAction;

        // The chef puts down whatever it is holding.
        public record PutDown() : UnityAction;

        // The chef loses whatever it is holding forever.
        public record DropOnFloor() : UnityAction;

        // Default: The chef doesn't do any visible actions.
        public record NoAction() : UnityAction;
    }

    /*
     *  The ExpValue class handles all possible variable values that can be used in the game.
     *  Each ExpValue holds the Type (for typechecking purposes), as well as the Value.
     */
    public class ExpValue
    {
        public enum Type { Bool, Str, Num }

        public Type ExpType { get; set; }
        public object Value { get; set; }

        public ExpValue(Type type, object value)
        {
            ExpType = type;
            Value = value;
        }

        private object TypeCheck(Type type)
        {
            if (this.ExpType != type)
                throw new ArgumentException("Type error: Expected " + type.ToString() + ", got " + this.ExpType.ToString());

            return this.Value;
        }
        public string TypeCheckString()
        {
            return (string)TypeCheck(Type.Str);
        }
        public bool TypeCheckBool()
        {
            return (bool)TypeCheck(Type.Bool);
        }
        public int TypeCheckNum()
        {
            return (int)TypeCheck(Type.Num);
        }
    }

    /*
     *  The State class holds all of the information that is used by the game to maintain the current puzzle state.
     *  The current information needed is:
     *      - The orders that must be fulfilled by the program (the test case)
     *      - The orders that have been fulfilled by the program so far (the current output)
     *      - The current contents of the pantry shelf
     *      - A map relating names of declared variables to their values
     *      - The current location of the Chef
     *      - The food item currently held by the Chef
     */
    public class State
    {
        public List<FoodItem> Orders { get; }
        public List<FoodItem> DeliveredOrders { get; set; } = new List<FoodItem>();
        public HashBag<FoodItem> Shelf { get; set; }

        // The list of all variable references currently tracked by the backend.
        public Dictionary<string, object> VariableMap { get; set; } = new Dictionary<string, object>();

        public ChefLocation ChefLocation { get; set; } = ChefLocation.Pantry;
        public FoodItem ChefHands { get; set; } = FoodItem.None;

        public State(List<FoodItem> orders, HashBag<FoodItem> shelf)
        {
            Orders = orders;
            Shelf = shelf;
            VariableMap.Add("orders", orders.Select(x => x.ToString()).ToList());
        }

        public override string ToString()
        {
            // TODO: pretty-print current state
            return base.ToString();
        }
    }
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
                                        (x, y) => new ExpValue(ExpValue.Type.Bool, (x.ExpType == y.ExpType) && x.Value.Equals(y.Value))
                                    ),
                Grammar.binop.Neq b => InterpretBinopExpression(b.Item1, b.Item2,
                                        (x, y) => new ExpValue(ExpValue.Type.Bool, (x.ExpType == y.ExpType) && !x.Value.Equals(y.Value))
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
                case Grammar.exp.Idx v:
                    //TODO: simplified to only work with string lists for now
                    if (!this.State.VariableMap.TryGetValue(v.Item1, out var value))
                        throw new ArgumentOutOfRangeException("no variable by that name exists");
                    if (value is not List<string>)
                        throw new ArgumentOutOfRangeException("that variable is not a list of strings");
                    return new ExpValue(ExpValue.Type.Str, ((List<string>)value)[InterpretExp(v.Item2).TypeCheckNum()]);
                default:
                    throw new ArgumentOutOfRangeException("EXP NOT SUPPORTED");
            }
        }

        private void InterpretStmt(Grammar.stmt stmt)
        {
            // Statements are complex and relegated to their own interpretation functions.
            switch (stmt)
            {
                case Grammar.stmt.KCall kCall:
                    InterpretKCall(kCall);
                    break;
                case Grammar.stmt.IfThenElse ite:
                    InterpretITE(ite);
                    break;
                case Grammar.stmt.While wh:
                    InterpretWhile(wh);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("STMT NOT SUPPORTED");
            }
        }

        // Statements are the only void actions that can be chained so far (called a Block), so we need to make sure to handle that properly.
        private void InterpretAllStmts(List<Grammar.stmt> stmts)
        {
            foreach (Grammar.stmt stmt in stmts)
                InterpretStmt(stmt);
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
                case Keyword.GOTO:
                    {
                        CheckArguments(1);
                        if (!ChefLocation.TryParse(args[0].TypeCheckString(), out ChefLocation newLocation))
                            throw new ArgumentException("expected a location string for GOTO, failed");

                        this.State.ChefLocation = newLocation;
                        RecordAction(new UnityAction.ChefMove(newLocation), "Chef goes to " + newLocation);

                        break;
                    }
                case Keyword.GET:
                    {
                        CheckArguments(1);
                        if (this.State.ChefLocation != ChefLocation.Pantry)
                            throw new ArgumentException("chef not at pantry, can't GET");
                        if (!FoodItem.TryParse(args[0].TypeCheckString(), out FoodItem food))
                            throw new ArgumentException("expected a food string for GET, failed");
                        if (!this.State.Shelf.Contains(food))
                            throw new ArgumentException("pantry does not contain " + food + ", can't GET");

                        this.State.ChefHands = food;
                        this.State.Shelf.Remove(food);
                        RecordAction(new UnityAction.PickUp(food), "Chef picks up " + food);

                        break;
                    }
                case Keyword.DELIVER:
                    {
                        CheckArguments(0);
                        if (this.State.ChefHands == FoodItem.None)
                            throw new ArgumentException("can't deliver, chef isn't holding anything");
                        if (this.State.ChefLocation != ChefLocation.Window)
                        {
                            this.State.ChefLocation = ChefLocation.Window;
                            RecordAction(new UnityAction.ChefMove(ChefLocation.Window), "Chef goes to " + ChefLocation.Window);
                        }

                        this.State.DeliveredOrders.Add(this.State.ChefHands);
                        RecordAction(new UnityAction.PutDown(), "Chef delivers " + this.State.ChefHands);
                        this.State.ChefHands = FoodItem.None;

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

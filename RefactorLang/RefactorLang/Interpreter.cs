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
    public enum ChefLocation
    {
        Pantry, Stove, Window
    }

    public enum FoodItem
    {
        None, Garbage, Pasta, BoiledPasta, Sauce, PastaWithSauce, Potato, BoiledPotato
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
        public float TypeCheckNum()
        {
            return (float)TypeCheck(Type.Num);
        }
    }

    /*
     *  The State class holds all of the information that is used by the game to maintain the current puzzle state.
     *  The current information needed is:
     *      - The orders that must be fulfilled by the program (the test case)
     *      - The orders that have been fulfilled by the program so far (the current output)
     *      - The current contents of the pantry shelf
     *      - A map relating names of declared variables to their values
     *      - A map relating bags of ingredients to their resulting food items for each method of cooking (so far, just boiling)
     *      - The current location of the Chef
     *      - The food item currently held by the Chef
     *      - The contents of each appliance (so far, just one stove with three burners)
     */
    public class State
    {
        public List<FoodItem> Orders { get; }
        public List<FoodItem> DeliveredOrders { get; set; } = new List<FoodItem>();
        public HashBag<FoodItem> Shelf { get; set; }

        public Dictionary<string, object> VariableMap { get; set; } = new Dictionary<string, object>();

        public Dictionary<HashBag<FoodItem>, FoodItem> BoilRecipes { get; } = new Dictionary<HashBag<FoodItem>, FoodItem>()
        {
            { new HashBag<FoodItem> { }, FoodItem.None },
            { new HashBag<FoodItem> { FoodItem.Pasta }, FoodItem.BoiledPasta },
            { new HashBag<FoodItem> { FoodItem.Pasta, FoodItem.Sauce }, FoodItem.PastaWithSauce },
            { new HashBag<FoodItem> { FoodItem.Potato }, FoodItem.BoiledPotato },
        };

        public ChefLocation ChefLocation { get; set; } = ChefLocation.Pantry;
        public FoodItem ChefHands { get; set; } = FoodItem.None;

        public HashBag<FoodItem>[] StoveContents { get; set; } = new HashBag<FoodItem>[3] { new HashBag<FoodItem>(), new HashBag<FoodItem>(), new HashBag<FoodItem>() };

        public FoodItem RecipeLookup(HashBag<FoodItem> ingredients)
        {
            foreach (KeyValuePair<HashBag<FoodItem>, FoodItem> kv in BoilRecipes)
            {
                if (kv.Key.SequenceEqual(ingredients))
                {
                    return kv.Value;
                }
            }

            return FoodItem.Garbage;
        }

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

        public Interpreter(List<FoodItem> orders, HashBag<FoodItem> shelf)
        {
            State = new State(orders, shelf);
        }

        private void RecordAction(string action)
        {
            Console.WriteLine(action);
        }

        private ExpValue InterpretBinop(Grammar.exp.Binop binop)
        {
            ExpValue InterpretBinopExpression(Grammar.exp exp1, Grammar.exp exp2, Func<ExpValue, ExpValue, ExpValue> operation)
            {
                ExpValue inp1 = InterpretExp(exp1);
                ExpValue inp2 = InterpretExp(exp2);
                return operation(inp1, inp2);
            }

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
                    return new ExpValue(ExpValue.Type.Str, ((List<string>)value)[(int)InterpretExp(v.Item2).TypeCheckNum()]);
                default:
                    throw new ArgumentOutOfRangeException("EXP NOT SUPPORTED");
            }
        }

        private void InterpretStmt(Grammar.stmt stmt)
        {
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

        private void InterpretAllStmts(List<Grammar.stmt> stmts)
        {
            foreach (Grammar.stmt stmt in stmts)
                InterpretStmt(stmt);
        }

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
                        RecordAction("Chef goes to " + newLocation);

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
                        RecordAction("Chef picks up " + food);

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
                            RecordAction("Chef goes to " + ChefLocation.Window);
                        }

                        this.State.DeliveredOrders.Add(this.State.ChefHands);
                        RecordAction("Chef delivers " + this.State.ChefHands);
                        this.State.ChefHands = FoodItem.None;

                        break;
                    }
                case Keyword.POTADD:
                    {
                        CheckArguments(1);
                        if (this.State.ChefLocation != ChefLocation.Stove)
                            throw new ArgumentException("chef not at stove, can't POTADD");
                        if (!new List<float> { 0f, 1f, 2f }.Contains(args[0].TypeCheckNum()))
                            throw new ArgumentException("must add to pot 0, 1, or 2");

                        string currentFood = this.State.ChefHands.ToString();
                        int potNum = (int)args[0].TypeCheckNum();
                        this.State.StoveContents[potNum].Add(this.State.ChefHands);
                        this.State.ChefHands = FoodItem.None;
                        RecordAction("Chef deposits " + currentFood + " into stove pot #" + potNum);

                        break;
                    }
                case Keyword.POTREMOVE:
                    {
                        CheckArguments(1);
                        if (this.State.ChefLocation != ChefLocation.Stove)
                            throw new ArgumentException("chef not at stove, can't POTREMOVE");
                        if (!new List<float> { 0f, 1f, 2f }.Contains(args[0].TypeCheckNum()))
                            throw new ArgumentException("must add to pot 0, 1, or 2");

                        int potNum = (int)args[0].TypeCheckNum();

                        if (!this.State.StoveContents[potNum].Any())
                            throw new ArgumentException("pot #" + potNum + "doesn't have anything in it, can't POTREMOVE");

                        if (this.State.StoveContents[potNum].Count == 1)
                        {
                            this.State.ChefHands = this.State.StoveContents[potNum].Last();
                            this.State.StoveContents[potNum] = new HashBag<FoodItem>();
                            RecordAction("Chef takes " + this.State.ChefHands + " out of stove pot #" + potNum);
                        }
                        else
                        {
                            this.State.StoveContents[potNum] = new HashBag<FoodItem>();
                            RecordAction("Everything in pot #" + potNum + " fell on the floor!");
                        }
                        

                        break;
                    }
                case Keyword.BOIL:
                    {
                        CheckArguments(1);
                        if (this.State.ChefLocation != ChefLocation.Stove)
                            throw new ArgumentException("chef not at stove, can't BOIL");
                        if (!new List<float> { 0f, 1f, 2f }.Contains(args[0].TypeCheckNum()))
                            throw new ArgumentException("must boil in pot 0, 1, or 2");


                        int potNum = (int)args[0].TypeCheckNum();
                        HashBag<FoodItem> ingredients = new HashBag<FoodItem>();
                        ingredients.AddAll(this.State.StoveContents[potNum]);

                        this.State.StoveContents[potNum] = new HashBag<FoodItem> { this.State.RecipeLookup(ingredients) };
                        RecordAction("Chef boils pot #" + potNum);


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
                RecordAction("Success!");
            else RecordAction("Failure...");
        }
    }
}

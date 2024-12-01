using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefactorLib;
using ParserLibrary;
using RefactorLang;

namespace RefactorLang
{
    enum ChefLocation
    {
        Pantry, Stove1, Stove2, Window
    }

    enum FoodItem
    {
        Pasta, BoiledPasta
    }

    public class ExpValue
    {
        public enum Type { Bool, Str, Num }

        public object Value { get; set; }
        public Type ExpType { get; set; }

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

    class State
    {
        public List<string> Orders { get; } = new List<string>();
        public Dictionary<FoodItem, int> PantryContents { get; set; } = new Dictionary<FoodItem, int>();
        public ChefLocation ChefLocation { get; set; }
        public FoodItem ChefHands { get; set; }
        
    }
    internal class Interpreter
    {
        public void RecordAction(string action)
        {
            Console.WriteLine(action);
        }

        public ExpValue InterpretBinop(Grammar.exp.Binop binop)
        {
            ExpValue inp1;
            ExpValue inp2;

            switch (binop.Item)
            {  
                case Grammar.binop.Add b:
                    inp1 = InterpretExp(b.Item1);
                    inp2 = InterpretExp(b.Item2);
                    return new ExpValue(ExpValue.Type.Num, inp1.TypeCheckNum() + inp2.TypeCheckNum());
                case Grammar.binop.Eq b:
                    inp1 = InterpretExp(b.Item1);
                    inp2 = InterpretExp(b.Item2);
                    return new ExpValue(ExpValue.Type.Bool, (inp1.ExpType == inp2.ExpType) && inp1.Value.Equals(inp2.Value));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ExpValue InterpretExp(Grammar.exp exp)
        {
            switch (exp)
            {
                case Grammar.exp.CBool v:
                    return new ExpValue(ExpValue.Type.Bool, v);
                case Grammar.exp.CStr v:
                    return new ExpValue(ExpValue.Type.Str, v);
                case Grammar.exp.CNum v:
                    return new ExpValue(ExpValue.Type.Num, v);
                case Grammar.exp.Binop b:
                    return InterpretBinop(b);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void InterpretStmt(Grammar.stmt stmt, State state)
        {
            switch (stmt)
            {
                case Grammar.stmt.KCall kCall:
                    HandleKCall(kCall, state);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void HandleKCall(Grammar.stmt.KCall stmt, State state)
        {
            void CheckArguments(int num)
            {
                if (stmt.Item2.Length != num) throw new ArgumentException();
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
                    CheckArguments(1);
                    if (!ChefLocation.TryParse(args[0].TypeCheckString(), out ChefLocation newLocation))
                        throw new ArgumentException("expected a location string for GOTO, failed");
                    
                    state.ChefLocation = newLocation;
                    RecordAction("Chef goes to " + newLocation);
                        
                    break;
                case Keyword.GET:
                    CheckArguments(1);
                    if (state.ChefLocation != ChefLocation.Pantry)
                        throw new ArgumentException("chef not at pantry, can't GET");
                    if (FoodItem.TryParse(args[0].TypeCheckString(), out FoodItem food))
                        throw new ArgumentException("expected a food string for GET, failed");

                    state.ChefHands = food;
                    RecordAction("Chef picks up " + food);

                    break;
            }
        }

        public void InterpretITE(Grammar.stmt.IfThenElse stmt)
        {
            
        }
    }
}

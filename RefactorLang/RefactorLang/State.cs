using C5;
using ParserLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RefactorLang
{
    // Contains all possible locations that the chef can be standing at any timestep.
    public record ChefLocation
    {
        public record Pantry() : ChefLocation;
        public record Window() : ChefLocation;

        public record Station(string Name) : ChefLocation;
    }

    public record FoodItem
    {
        public record Some(string Food) : FoodItem;
        public record None() : FoodItem;
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
        public record PickUp() : UnityAction;

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
        public enum Type { Bool, Str, Num, Void, None }

        public record ExpType
        {
            public record Single(Type Type) : ExpType;
            public record List(Type Type) : ExpType;
        }

        public ExpType TypeDef { get; set; }
        public object Value { get; set; }

        public ExpValue(ExpType type, object value)
        {
            TypeDef = type;
            Value = value;
        }

        public ExpValue(Type type, object value)
        {
            TypeDef = new ExpType.Single(type);
            Value = value;
        }

        private object TypeCheck(ExpType type)
        {
            if (!type.Equals(TypeDef))
                throw new ArgumentException("Type error: Expected " + type.ToString() + ", got " + this.TypeDef.ToString());

            return this.Value;
        }

        public Type PokeType()
        {
            switch (this.TypeDef)
            {
                case ExpType.Single e:
                    return e.Type;
                case ExpType.List e:
                    return e.Type;
                default:
                    throw new ArgumentOutOfRangeException("what");
            }
        }

        public string TypeCheckString()
        {
            return (string)TypeCheck(new ExpType.Single(Type.Str));
        }
        public bool TypeCheckBool()
        {
            return (bool)TypeCheck(new ExpType.Single(Type.Bool));
        }
        public int TypeCheckNum()
        {
            return (int)TypeCheck(new ExpType.Single(Type.Num));
        }

        public object TypeCheckList()
        {
            switch (this.TypeDef)
            {
                case ExpType.List l:
                    return this.Value;
                default:
                    throw new ArgumentException("Type error: Expected list, got " + this.TypeDef.ToString());
            }
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

        public Station Station { get; } = new Station("A", new List<Module> { new Slicer() });

        // The list of all variable references currently tracked by the backend.
        public Dictionary<string, ExpValue> VariableMap { get; set; } = new Dictionary<string, ExpValue>();

        // the list of defined global functions
        public Dictionary<string, Grammar.stmt.FDecl> FDecls { get; set; } = new Dictionary<string, Grammar.stmt.FDecl>();

        // Stack model for handling scopes in function calls
        public Stack<Dictionary<string, ExpValue>> Stack { get; set; } = new Stack<Dictionary<string, ExpValue>>();

        public ChefLocation ChefLocation { get; set; } = new ChefLocation.Pantry();
        public FoodItem ChefHands { get; set; } = new FoodItem.None();

        public State(List<FoodItem> orders, HashBag<FoodItem> shelf)
        {
            Orders = orders;
            Shelf = shelf;
            VariableMap.Add("orders", new ExpValue(new ExpValue.ExpType.List(ExpValue.Type.Str), orders.Select(x => x.ToString()).ToList()));
        }

        public override string ToString()
        {
            // TODO: pretty-print current state
            return base.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RefactorLang
{
    public class Station
    {
        public List<Module> Modules { get; }
        public string Name { get; }

        public Station(string name, List<Module> modules)
        {
            Name = name;
            Modules = modules;
        }
    }

    public abstract class Module
    {
        public FoodItem Output { get; set; }
        public abstract ICollection<FoodItem> Slots { get; }

        public string Name { get; set; }
        public abstract int Size { get; }

        public abstract void Activate();

        public abstract void Place(FoodItem food, int slot);

        protected FoodItem.Some CheckSlotNotNone(int slotnum)
        {
            if (this.Slots.ToList()[slotnum] is FoodItem.None or null)
                throw new ArgumentException($"slot number {slotnum} has no food!");

            return (FoodItem.Some)this.Slots.ToList()[slotnum];
        }

        protected void CheckSlotFood(int slotnum, string foodName)
        {
            CheckSlotNotNone(slotnum);

            if (!(this.Slots.ToList()[slotnum] is FoodItem.Some food && food.Food == foodName))
                throw new ArgumentException($"slot number {slotnum} does not have {foodName}");

        }

        public FoodItem Take()
        {
            FoodItem food = Output;
            Output = new FoodItem.None();

            return food;
        }
    }

    public abstract class ArrayModule : Module
    {
        protected FoodItem[] _slots;

        public override ICollection<FoodItem> Slots
        {
            get => _slots.ToList();
        }

        public override void Place(FoodItem food, int slot)
        {
            if (slot >= Size || slot < 0) throw new ArgumentOutOfRangeException($"attempted to put item in slot {slot}, which is out of bounds for {Name}");
            _slots[slot] = food;
        }

        protected void Empty()
        {
            _slots = new FoodItem[Size];
        }

        public ArrayModule(string name)
        {
            _slots = new FoodItem[Size];
            Name = name;
        }
    }

    public class Slicer : ArrayModule
    {
        public Slicer(string name) : base(name)
        {
            
        }

        public override int Size { get; } = 1;

        public override void Activate()
        {
            FoodItem food = CheckSlotNotNone(0);

            Output = new FoodItem.Some("Sliced " + food);

            Empty();
        }
    }

    public class SoupMaker : ArrayModule
    {
        public override int Size { get; } = 2;

        public SoupMaker (string name) : base(name)
        { 
            
        }

        public override void Activate()
        {
            CheckSlotFood(0, "Broth");
            FoodItem.Some food = CheckSlotNotNone(1);

            Output = new FoodItem.Some(food.Food + " Soup");
            Empty();
        }
    }
}

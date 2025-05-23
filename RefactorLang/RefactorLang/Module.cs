﻿using System;
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
        public string Name { get; set; }

        public Station(string name, List<Module> modules)
        {
            Name = name;
            Modules = modules;
        }

        public StationSignature CreateSignature()
        {
            return new StationSignature(this.Name, this.Modules.Select(x => x.CreateSignature()).Where(x => x != null).ToList());
        }
    }

    public abstract class Module
    {
        public FoodItem Output { get; set; } = new FoodItem.None();
        public abstract ICollection<FoodItem> Slots { get; }

        public string Name { get; set; }
        public abstract int Size { get; }

        public abstract void Activate();

        public abstract ModuleSignature CreateSignature();

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

        protected void CheckSlotFoodContains(int slotnum, string foodParam)
        {
            CheckSlotNotNone(slotnum);

            if (this.Slots.ToList()[slotnum] is FoodItem.Some food && !food.Food.Contains(foodParam))
                throw new ArgumentException($"slot number {slotnum} is not a {foodParam}");
        }

        protected void CheckSlotFoodDoesNotContain(int slotnum, string foodParam)
        {
            CheckSlotNotNone(slotnum);

            if (this.Slots.ToList()[slotnum] is FoodItem.Some food && food.Food.Contains(foodParam))
                throw new ArgumentException($"slot number {slotnum} is a {foodParam}");
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

    public class None : ArrayModule
    {
        public None(string name) : base(name) { }
        public override int Size { get; } = 0;

        public override void Activate()
        {
            throw new ArgumentException("Cannot activate, no module here");
        }

        public override void Place(FoodItem food, int slot)
        {
            throw new ArgumentException("Cannot place, no module here");
        }

        public override ModuleSignature CreateSignature()
        {
            return null;
        }
    }

    public class Slicer : ArrayModule
    {
        public Slicer(string name) : base(name) { }

        public override int Size { get; } = 1;

        public override void Activate()
        {
            FoodItem.Some food = CheckSlotNotNone(0);
            CheckSlotFoodDoesNotContain(0, "Sliced");
            CheckSlotFoodContains(0, "Loaf of");

            Output = new FoodItem.Some(food.Food.Replace("Loaf of ", "Sliced "));

            Empty();
        }

        public override ModuleSignature CreateSignature()
        {
            return new ModuleSignature("Slicer", Name);
        }
    }

    public class SoupMaker : ArrayModule
    {
        public override int Size { get; } = 2;

        public SoupMaker(string name) : base(name) { }

        public override void Activate()
        {
            CheckSlotFood(0, "Broth");
            FoodItem.Some food = CheckSlotNotNone(1);
            CheckSlotFoodDoesNotContain(1, "Soup");

            Output = new FoodItem.Some(food.Food + " Soup");
            Empty();
        }

        public override ModuleSignature CreateSignature()
        {
            return new ModuleSignature("SoupMaker", Name);
        }
    }

    public class Grinder : ArrayModule
    {
        public override int Size { get; } = 1;

        public Grinder(string name) : base(name) { }

        public override void Activate()
        {
            FoodItem.Some food = CheckSlotNotNone(0);
            CheckSlotFoodDoesNotContain(0, "Ground");
            CheckSlotFoodContains(0, "Raw");

            Output = new FoodItem.Some(food.Food.Replace("Raw ", "Ground "));
            Empty();
        }

        public override ModuleSignature CreateSignature()
        {
            return new ModuleSignature("Grinder", Name);
        }
    }

    public class Fryer : ArrayModule
    {
        public override int Size { get; } = 2;

        public Fryer(string name) : base(name) { }

        public override void Activate()
        {
            CheckSlotFood(0, "Ground Chicken");
            CheckSlotFood(1, "Sliced Bread");

            Output = new FoodItem.Some("Chicken Tenders");
            Empty();
        }

        public override ModuleSignature CreateSignature()
        {
            return new ModuleSignature("Fryer", Name);
        }
    }

    public class BarbecueSaucer : ArrayModule
    {
        public override int Size { get; } = 3;

        public BarbecueSaucer(string name) : base(name) { }

        public override void Activate()
        {
            CheckSlotFood(0, "Tomato");
            CheckSlotFood(1, "Sugar");
            CheckSlotFood(2, "Vinegar");

            Output = new FoodItem.Some("Barbecue Sauce");
            Empty();
        }

        public override ModuleSignature CreateSignature()
        {
            return new ModuleSignature("BarbecueSaucer", Name);
        }
    }

    public class Griddle : ArrayModule
    {
        public override int Size { get; } = 2;

        public Griddle(string name) : base(name) { }

        public override void Activate()
        {
            CheckSlotFood(0, "Egg");
            CheckSlotFood(1, "Pepper");

            Output = new FoodItem.Some("Fried Egg");
            Empty();
        }

        public override ModuleSignature CreateSignature()
        {
            return new ModuleSignature("Griddle", Name);
        }
    }

    public class BurgerBuilder : ArrayModule
    {
        public override int Size { get; } = 4;

        public BurgerBuilder(string name) : base(name) { }

        public override void Activate()
        {
            CheckSlotFood(0, "Sliced Bread");
            FoodItem.Some food = CheckSlotNotNone(1);
            CheckSlotFood(2, "Ground Beef");
            CheckSlotFood(3, "Sliced Bread");

            Output = new FoodItem.Some(food.Food + " Burger");
            Empty();
        }

        public override ModuleSignature CreateSignature()
        {
            return new ModuleSignature("BurgerBuilder", Name);
        }
    }
}
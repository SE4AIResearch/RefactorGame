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

        public abstract int Size { get; }

        public abstract void Activate();

        public abstract void Put(FoodItem food, int slot);

        public FoodItem Take()
        {
            FoodItem food = Output;
            Output = new FoodItem.None();

            return food;
        }
    }

    public class Slicer : Module
    {
        public override int Size { get; } = 1;

        private FoodItem[] _slots = new FoodItem[1];

        public override ICollection<FoodItem> Slots {
            get => _slots.ToList();
        }

        public override void Put(FoodItem food, int slot)
        {
            if (slot != 0) throw new ArgumentOutOfRangeException("slot must be 0 for slicer");
            _slots[slot] = food;
        }

        public override void Activate()
        {
            switch (this.Slots.First())
            {
                case FoodItem.None: break;
                case FoodItem.Some(string food):
                    this.Output = new FoodItem.Some("sliced " + food);
                    break;
            }
        }
    }
}

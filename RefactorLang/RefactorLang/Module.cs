using System;
using System.Collections.Generic;
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
        public List<FoodItem> Slots { get; set; }
        public FoodItem Output { get; set; }

        public abstract int Size { get; set; }

        public abstract void Activate();

        public void Put(FoodItem food, int slot)
        {

        }

        public FoodItem Take()
        {
            FoodItem food = Output;
            Output = new FoodItem.None();

            return food;
        }
    }

    public class Slicer : Module
    {
        public override int Size { get; set; } = 1;

        public override void Activate()
        {
            switch (this.Slots[0])
            {
                case FoodItem.None: break;
                case FoodItem.Some(string food):
                    this.Output = new FoodItem.Some("sliced " + food);
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RefactorLang
{
    public class Puzzle
    {
        public static readonly JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("difficulty")]
        public int Difficulty { get; }

        [JsonPropertyName("testCases")]
        public List<List<string>> TestCases { get; }

        [JsonPropertyName("starterPantry")]
        public List<string> StarterPantry { get; }

        [JsonPropertyName("starterCode")]
        public string StarterCode { get; }

        [JsonPropertyName("numOfStations")]
        public int NumOfStations { get; }

        [JsonPropertyName("stations")]
        public List<StationSignature> Stations { get; }

        [JsonPropertyName("modulesLocked")]
        public bool ModulesLocked { get; }

        [JsonPropertyName("constraints")]
        public ConstraintSignature Constraints { get; }

        [JsonPropertyName("storyPrompt")]
        public string StoryPrompt { get; }

        public Puzzle(string name, int difficulty, int numOfStations, List<StationSignature> stations, bool modulesLocked, List<List<string>> testCases, List<string> starterPantry, 
            string starterCode, ConstraintSignature constraints, string storyPrompt)
        {
            Name = name;
            Difficulty = difficulty;
            NumOfStations = numOfStations;
            Stations = stations;
            ModulesLocked = modulesLocked;
            TestCases = testCases;
            StarterPantry = starterPantry;
            StarterCode = starterCode;
            StoryPrompt = storyPrompt;
            Constraints = constraints;
        }

        public static void Serialize(Puzzle puzzle, string fileName = @"./samplePuzzle.json")
        {
            string json = JsonSerializer.Serialize(puzzle, options);

            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.Write(json);

                    Console.WriteLine($"Json file written to {fileName}");
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }
        }

        public static Puzzle Deserialize(string fileName)
        {
            return JsonSerializer.Deserialize<Puzzle>(File.ReadAllText(fileName));
        }
    }

    public class StationSignature
    {
        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("modules")]
        public List<ModuleSignature> Modules { get; }

        public StationSignature(string name, List<ModuleSignature> modules)
        {
            Name = name;
            Modules = modules;
        }

        public Station ConvertToStation()
        {
            List<string> defaultNames = new() { "A", "B", "C" };

            Station station = new Station(Name, Modules.Select(x => x.ConvertToModule()).ToList());

            while (station.Modules.Count < 3)
            {
                station.Modules.Add(new None(defaultNames[station.Modules.Count - 1]));
            }

            return station;
        }
    }

    public class ModuleSignature
    {

        [JsonPropertyName("module")]
        public string Module { get; }

        [JsonPropertyName("name")]
        public string Name { get; }

        public ModuleSignature(string module, string name)
        {
            Module = module;
            Name = name;
        }

        public Module ConvertToModule()
        {
            return this.Module switch
            {
                "SoupMaker" => new SoupMaker(Name),
                "Slicer" => new Slicer(Name),
                "Grinder" => new Grinder(Name),
                "Fryer" => new Fryer(Name),
                "BarbecueSaucer" => new BarbecueSaucer(Name),
                "Griddle" => new Griddle(Name),
                "BurgerBuilder" => new BurgerBuilder(Name),
                "None" => null,
                _ => throw new ArgumentException("not a module string"),
            };
        }
    }

    public class ConstraintSignature
    {
        [JsonPropertyName("maxStatements")]
        public int MaxStatements { get; }

        [JsonPropertyName("maxActions")]
        public int MaxActions { get; }

        [JsonPropertyName("additionalConstraint")]
        public string AdditionalConstraint { get; }

        public ConstraintSignature(int maxStatements, int maxActions, string additionalConstraint)
        {
            MaxStatements = maxStatements;
            MaxActions = maxActions;
            AdditionalConstraint = additionalConstraint;
        }
    }
}

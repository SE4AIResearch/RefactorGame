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

        [JsonPropertyName("storyPrompt")]
        public string StoryPrompt { get; }

        public Puzzle(string name, int difficulty, int numOfStations, List<StationSignature> stations, List<List<string>> testCases, List<string> starterPantry, string starterCode, string storyPrompt)
        {
            Name = name;
            Difficulty = difficulty;
            NumOfStations = numOfStations;
            Stations = stations;
            TestCases = testCases;
            StarterPantry = starterPantry;
            StarterCode = starterCode;
            StoryPrompt = storyPrompt;
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
            return new Station(Name, Modules.Select(x => x.ConvertToModule()).ToList());
        }
    }

    public class ModuleSignature
    {

        [JsonPropertyName("module")]
        public string Module { get; }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("locked")]
        public bool Locked { get; }

        public ModuleSignature(string module, string name, bool locked = false)
        {
            Module = module;
            Name = name;
            Locked = locked;
        }

        public Module ConvertToModule()
        {
            return this.Module switch
            {
                "SoupMaker" => new SoupMaker(Name, Locked),
                "Slicer" => new Slicer(Name, Locked),
                "Grinder" => new Grinder(Name, Locked),
                "Fryer" => new Fryer(Name, Locked),
                "BarbecueSaucer" => new BarbecueSaucer(Name, Locked),
                "Griddle" => new Griddle(Name, Locked),
                "BurgerBuilder" => new BurgerBuilder(Name, Locked),
                _ => throw new ArgumentException("not a module string"),
            };
        }
    }
}

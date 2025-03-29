using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RefactorLang
{
    public class KitchenState
    {
        public List<string> Pantry { get; set; }
        public List<Station> Stations { get; set; }
        public int NumStations { get; }
        public List<List<string>> TestCases { get; set; }
        public int SelectedTestCase { get; set; } = 0;
        public Dictionary<int, TestStatus> TestCaseStatus { get; set; } = new Dictionary<int, TestStatus>();

        public KitchenState(Puzzle puzzle)
        {
            Stations = puzzle.Stations.Select(x => x.ConvertToStation()).ToList();
            NumStations = puzzle.NumOfStations;
            TestCases = puzzle.TestCases;
            Pantry = puzzle.StarterPantry;

            if (Stations.Count != NumStations)
                throw new ArgumentException("mismatch between parameters");

            for (int i = 0; i < TestCases.Count; i++)
            {
                TestCaseStatus.Add(i, TestStatus.NotRun);
            }
        }

        public void RenameStation(int index, string newName)
        {
            if (index < 0 || index >= NumStations)
                throw new ArgumentOutOfRangeException("index was out of range");

            Stations[index].Name = newName;
        }
    }

    public enum TestStatus {
        NotRun,
        Running,
        Passed,
        Failed,
        Warning
    }
}

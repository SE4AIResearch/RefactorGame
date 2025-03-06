using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RefactorLang
{
    public class KitchenState
    {
        public List<Station> Stations { get; set; }
        public List<List<string>> TestCases { get; set; }
        public int SelectedTestCase { get; set; } = 0;
        public int NumStations { get; }

        public KitchenState(Puzzle puzzle)
        {
            Stations = puzzle.Stations.Select(x => x.ConvertToStation()).ToList();
            NumStations = puzzle.NumOfStations;
            TestCases = puzzle.TestCases;

            if (Stations.Count != NumStations)
                throw new ArgumentException("mismatch between parameters");
        }

        public void RenameStation(int index, string newName)
        {
            if (index < 0 || index >= NumStations)
                throw new ArgumentOutOfRangeException("index was out of range");

            Stations[index].Name = newName;
        }
    }
}

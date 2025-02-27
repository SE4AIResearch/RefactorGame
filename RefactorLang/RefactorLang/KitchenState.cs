using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RefactorLang
{
    public class KitchenState
    {
        List<Station> Stations { get; set; }
        int NumStations { get; }

        public KitchenState(List<StationSignature> stations, int numStations)
        {
            Stations = stations.Select(x => x.ConvertToStation()).ToList();
            NumStations = numStations;

            if (Stations.Count != NumStations)
                throw new ArgumentException("mismatch between parameters");
        }

        public void RenameStation(int index, string newName)
        {
            if (index < 0 || index >= NumStations)
                throw new ArgumentOutOfRangeException("index was out of range");

            Stations[index].Name = newName;
        }

        public void RenameModule(int stationIndex, int moduleIndex, string newName)
        {
            if (stationIndex < 0 || stationIndex >= NumStations)
                throw new ArgumentOutOfRangeException("station index was out of range");

            if (moduleIndex < 0 || moduleIndex > Stations[stationIndex].Modules.Count)
                throw new ArgumentOutOfRangeException("module index was out of range");

            Stations[stationIndex].Modules[moduleIndex].Name = newName;
        }
    }
}

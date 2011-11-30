using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class PlantSpecies
    {
        private static int SpeciesIdCounter = 0;

        public int Reward { get; set; }
        public int Radius { get; set; }
        public string Name { get; set; }
        public readonly int SpeciesId;

        public PlantSpecies(int id)
        {
            SpeciesId = id;
        }
    }
}

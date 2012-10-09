using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class Plant : EdibleThingy<IAgent>
    {
        public PlantSpecies Species { get; set; }
        public Plant(PlantSpecies species) : base(species.Radius, species.Reward)
        {
            Species = species;
            Radius = species.Radius;
            Reward = species.Reward;
        }
    }
}

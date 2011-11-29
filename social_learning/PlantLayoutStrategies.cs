using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public enum PlantLayoutStrategies
    {
        /// <summary>
        /// Lays out the plants using a uniformly random strategy.
        /// </summary>
        Uniform,
        /// <summary>
        /// Lays out the plants along a randomly generated spiral pattern.
        /// </summary>
        Spiral,
        /// <summary>
        /// Creates a random number of clusters of plants.
        /// </summary>
        Clustered
    }
}

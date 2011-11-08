using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class Plant
    {
        public int X { get; set; }
        public int Y { get; set; }
        public PlantSpecies Species { get; set; }
        private int _lastEaten = 0;
        private Dictionary<IAgent, int> whosEatenMe = new Dictionary<IAgent, int>();

        public Plant(PlantSpecies species)
        {
            Species = species;
        }

        public bool AvailableForEating(IAgent agent)
        {
            return !whosEatenMe.ContainsKey(agent);
            //return whosEatenMe.Count == 0;
        }

        public void EatenBy(IAgent agent, int step)
        {
            whosEatenMe.Add(agent, step);
            _lastEaten = step;
        }

        public void Reset()
        {
            whosEatenMe.Clear();
            _lastEaten = -1;
        }

        /// <summary>
        /// Returns true if anyone has eaten this plant in the last <i>window</i> time steps from <i>curStep</i>.
        /// </summary>
        /// <param name="curStep"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public bool EatenRecently(int curStep, int window)
        {
            return _lastEaten >= 0 && (curStep - _lastEaten) <= window;
        }

        public bool EatenByRecently(int curStep, int window, IAgent agent)
        {
            return _lastEaten >= 0 && whosEatenMe.ContainsKey(agent) && (curStep - whosEatenMe[agent]) < window;
        }
    }
}

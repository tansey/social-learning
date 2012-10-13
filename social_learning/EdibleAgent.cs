using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public abstract class EdibleAgent<PredatorType> : Agent, IEdibleThingy<PredatorType>
    {
        const int DEFAULT_AGENT_RADIUS = 5;
        const int DEFAULT_AGENT_REWARD = 100;

        public int Reward { get; set; }
        public int Radius { get; set; }
        public int EaterCount { get { return whosEatenMe.Values.Count; } }
        private int _lastEaten = -1;
        private Dictionary<PredatorType, int> whosEatenMe = new Dictionary<PredatorType, int>();

        public EdibleAgent(int id) : base(id)
        {
            Reward = DEFAULT_AGENT_REWARD;
            Radius = DEFAULT_AGENT_RADIUS;
        }

        public EdibleAgent(int id, int radius, int reward) : base(id)
        {
            Radius = radius;
            Reward = reward;
        }

        public bool AvailableForEating(PredatorType pred)
        {
            return !whosEatenMe.ContainsKey(pred);
            //return whosEatenMe.Count == 0;
        }

        public void EatenBy(PredatorType pred, int step)
        {
            if (whosEatenMe.ContainsKey(pred))
                whosEatenMe[pred] = step;
            else
                whosEatenMe.Add(pred, step);
            _lastEaten = step;
        }

        public override void Reset()
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

        public bool EatenByRecently(int curStep, int window, PredatorType pred)
        {
            return _lastEaten >= 0 && whosEatenMe.ContainsKey(pred) && (curStep - whosEatenMe[pred]) < window;
        }
    }
}

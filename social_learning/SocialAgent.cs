using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;

namespace social_learning
{
    public class SocialAgent : NeuralAgent
    {
        const int DEFAULT_MEMORY_SIZE = 5;

        /// <summary>
        /// The maximum number of timesteps to remember.
        /// </summary>
        public int MemorySize { get; set; }

        /// <summary>
        /// A sliding window of state-action pairs for this agent.
        /// </summary>
        public LinkedList<InputOutputPair> Memory { get; set; }

        public SocialAgent(int id, IBlackBox brain) : base(id, brain)
        {
            MemorySize = DEFAULT_MEMORY_SIZE;
            Memory = new LinkedList<InputOutputPair>();
        }

        protected override ISignalArray activateNetwork(double[] sensors)
        {
            var results = base.activateNetwork(sensors);
            double[] outputs = new double[results.Length];
            results.CopyTo(outputs, 0);

            if (Memory.Count >= MemorySize)
                Memory.RemoveFirst();

            Memory.AddLast(new InputOutputPair(sensors, outputs));

            return results;
        }
    }
}

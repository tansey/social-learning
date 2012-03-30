using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;

namespace social_learning
{
    public class SocialAgent : NeuralAgent
    {
        public static int DEFAULT_MEMORY_SIZE = 3;
        private const double DEFAULT_LEARNING_RATE = 0.1;
        private const double DEFAULT_MOMENTUM_RATE = 0.9;

        /// <summary>
        /// The maximum number of timesteps to remember.
        /// </summary>
        public int MemorySize { get; set; }

        /// <summary>
        /// A sliding window of stateActionPair-action pairs for this agent.
        /// </summary>
        public LinkedList<InputOutputPair> Memory { get; set; }

        /// <summary>
        /// The learning rate for backpropping on this agent.
        /// </summary>
        public double LearningRate { get; set; }

        /// <summary>
        /// The momentum rate for backpropping on this agent.
        /// </summary>
        public double Momentum { get; set; }

        public SocialAgent(int id, int speciesId, IBlackBox brain) : base(id, speciesId, brain)
        {
            MemorySize = DEFAULT_MEMORY_SIZE;
            Memory = new LinkedList<InputOutputPair>();
            LearningRate = DEFAULT_LEARNING_RATE;
            Momentum = DEFAULT_MOMENTUM_RATE;
        }

        public override ISignalArray activateNetwork(double[] sensors)
        {
            var results = base.activateNetwork(sensors);
            double[] outputs = new double[results.Length];
            results.CopyTo(outputs, 0);

            if (Memory.Count >= MemorySize)
                Memory.RemoveFirst();

            Memory.AddLast(new InputOutputPair(sensors, outputs));

            return results;
        }

        public override void Reset()
        {
            base.Reset();
            Memory.Clear();
        }
    }
}

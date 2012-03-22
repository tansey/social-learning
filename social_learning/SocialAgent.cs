using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;
using social_learning.Acceptability;

namespace social_learning
{
    public class SocialAgent : NeuralAgent
    {
        public static int DEFAULT_MEMORY_SIZE = 1;
        private const double DEFAULT_LEARNING_RATE = 0.1;
        private const double DEFAULT_MOMENTUM_RATE = 0.9;

        /// <summary>
        /// The maximum number of timesteps to remember.
        /// </summary>
        public int MemorySize { get; set; }

        /// <summary>
        /// A sliding window of stateActionPair-action pairs for this agent.
        /// </summary>
        public LinkedList<StateActionReward> Memory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IAcceptabilityFunction AcceptabilityFn { get; set; }

        /// <summary>
        /// The learning rate for backpropping on this agent.
        /// </summary>
        public double LearningRate { get; set; }

        /// <summary>
        /// The momentum rate for backpropping on this agent.
        /// </summary>
        public double Momentum { get; set; }

        /// <summary>
        /// Constructs a social learning agent controlled by the brain and using the give acceptability function.
        /// </summary>
        public SocialAgent(int id, int speciesId, IBlackBox brain, IAcceptabilityFunction accept) : base(id, speciesId, brain)
        {
            MemorySize = DEFAULT_MEMORY_SIZE;
            Memory = new LinkedList<StateActionReward>();
            LearningRate = DEFAULT_LEARNING_RATE;
            Momentum = DEFAULT_MOMENTUM_RATE;
            AcceptabilityFn = accept;
        }

        /// <summary>
        /// Constructs a social agent using a lambda function for the acceptability function.
        /// </summary>
        public SocialAgent(int id, int speciesId, IBlackBox brain, Predicate<LinkedList<StateActionReward>> accept)
            : this(id, speciesId, brain, new DynamicAcceptability(accept))
        {
        }

        public override ISignalArray activateNetwork(double[] sensors)
        {
            var results = base.activateNetwork(sensors);
            double[] outputs = new double[results.Length];
            results.CopyTo(outputs, 0);

            if (Memory.Count >= MemorySize)
                Memory.RemoveFirst();

            Memory.AddLast(new StateActionReward(sensors, outputs, 0));

            return results;
        }

        public override void Reset()
        {
            base.Reset();
            Memory.Clear();
        }

        protected override void ProcessReward(double r)
        {
            base.ProcessReward(r);
            Memory.Last.Value.Reward += r;
        }
    }
}

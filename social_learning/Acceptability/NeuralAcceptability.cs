using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;
using System.Diagnostics;

namespace social_learning.Acceptability
{
    /// <summary>
    /// A neural network based acceptability function. The network only looks at the most recent
    /// element in the memory and uses recurrent connections to handle multiple SAR actions.
    /// </summary>
    public class RecurrentNeuralAcceptability :IAcceptabilityFunction
    {
        public IBlackBox Brain { get; set; }
        public double RewardNormalizer { get; set; }
        public double AcceptThreshold { get; set; }

        public RecurrentNeuralAcceptability(IBlackBox brain, double acceptThreshold = 0.8, double rewardNormalizer = 100)
        {
            Brain = brain;
            AcceptThreshold = acceptThreshold;
            RewardNormalizer = rewardNormalizer;

            Debug.Assert(brain.OutputCount == 1);
        }

        public bool Accept(LinkedList<StateActionReward> memory)
        {
            var last = memory.Last.Value;
            Debug.Assert(last.State.Length + last.Action.Length + 1 == Brain.InputCount);

            for (int i = 0; i < last.State.Length; i++)
                Brain.InputSignalArray[i] = last.State[i];

            for (int i = 0; i < last.Action.Length; i++)
                Brain.InputSignalArray[i + last.State.Length] = last.Action[i];

            Brain.InputSignalArray[last.State.Length + last.Action.Length] = 
                                        Math.Min(1,Math.Max(0,(RewardNormalizer + last.Reward) / (2*RewardNormalizer)));
            
            // Activate the network
            Brain.Activate();

            return Brain.OutputSignalArray[0] >= AcceptThreshold;
        }

        public void Reset()
        {
            Brain.ResetState();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;

namespace social_learning
{
    //TODO: Finish implementing this agent.
    public class QLearningAgent : NeuralAgent
    {
        private readonly int _numVelocityActions, _numOrientationActions;
        private readonly Random _random;
        public double LearningRate { get; set; }
        public double DiscountFactor { get; set; }
        public double Epsilon { get; set; }

        public QLearningAgent(int id, IBlackBox brain, int numVelocityActions, int numOrientationActions)
            : base(id, brain)
        {
            _numVelocityActions = numVelocityActions;
            _numOrientationActions = numOrientationActions;
            _random = new Random();
        }

        protected override SharpNeat.Phenomes.ISignalArray activateNetwork(double[] sensors)
        {
            var values = base.activateNetwork(sensors);

            var results = selectEpsilonGreedy(values);

            return results;
        }

        private ISignalArray selectEpsilonGreedy(ISignalArray values)
        {
            double[] results;
            if (_random.Next() < Epsilon)
                results = selectRandom(values);
            else
                results = selectGreedy(values);
            return new SignalArray(results, 0, 2);
        }

        private double[] selectRandom(ISignalArray values)
        {
            return new double[] { values[_random.Next(0, _numOrientationActions)], 
                                      values[_random.Next(_numOrientationActions, values.Length)] };
        }

        private double[] selectGreedy(ISignalArray values)
        {
            double[] results = new double[2];

            int maxOrientation = 0;
            for (int i = 1; i < _numOrientationActions; i++)
                if (values[i] > values[maxOrientation])
                    maxOrientation = i;
            results[0] = values[maxOrientation];

            int maxVelocity = _numOrientationActions;
            for (int i = _numOrientationActions + 1; i < values.Length; i++)
                if (values[i] > values[maxVelocity])
                    maxVelocity = i;
            results[1] = values[maxVelocity];

            return results;
        }
    }
}

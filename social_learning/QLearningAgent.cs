using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;

namespace social_learning
{
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
                results = new double[] { values[_random.Next(0, _numOrientationActions)], 
                                      values[_random.Next(_numOrientationActions, values.Length)] };
            else
                results = selectGreedy(values);
            return new SignalArray(results, 0, 2);
        }

        private double[] selectGreedy(ISignalArray values)
        {
            double[] results = new double[2];
            for (int i = 0; i < _numOrientationActions; i++)
            {

            }
            return results;
        }
    }
}

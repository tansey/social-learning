using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;
using SharpNeat.Phenomes.NeuralNets;
using System.Diagnostics;
using SharpNeat.Utility;

namespace social_learning
{
    //TODO: Finish implementing this agent.
    public class QLearningAgent : NeuralAgent
    {
        const double DEFAULT_LEARNING_RATE = 0.1;
        const double DEFAULT_DISCOUNT_FACTOR = 0.9;
        const double DEFAULT_EPSILON = 1;

        readonly int _numVelocityActions, _numOrientationActions;
        readonly Random _random;
        readonly double[] _actions;

        int[] _prevActions;
        double[] _prevState;
        double[] _prevValues;
        double reward;

        public double MaxReward { get; set; }
        public double LearningRate
        {
            get { return ((FastCyclicNetwork)Brain).BackpropLearningRate; }
            set { ((FastCyclicNetwork)Brain).BackpropLearningRate = value; }
        }
        public double DiscountFactor { get; set; }
        public double Epsilon { get; set; }

        public QLearningAgent(int id, IBlackBox brain, int numOrientationActions, int numVelocityActions, World world)
            : base(id, brain)
        {
            Debug.Assert(brain.OutputCount == (numOrientationActions + numVelocityActions), 
                        "Incorrect number of outputs in neural network!");

            _numVelocityActions = numVelocityActions;
            _numOrientationActions = numOrientationActions;
            _random = new Random();
            _prevActions = new int[2];
            _prevState = new double[brain.InputCount];
            _prevValues = new double[brain.OutputCount];
            world.PlantEaten += new World.PlantEatenHandler(world_PlantEaten);
            MaxReward = 100;
            LearningRate = DEFAULT_LEARNING_RATE;
            DiscountFactor = DEFAULT_DISCOUNT_FACTOR;
            Epsilon = DEFAULT_EPSILON;

            ((FastCyclicNetwork)Brain).BackpropLearningRate = LearningRate;

            _actions = new double[brain.OutputCount];
            for (int i = 0; i < numOrientationActions; i++)
                _actions[i] = 1.0 / (double)numOrientationActions * i;
            for (int i = 0; i < numVelocityActions; i++)
                _actions[i + numOrientationActions] = 1.0 / (double)numVelocityActions * i;
        }

        void world_PlantEaten(object sender, IAgent eater, Plant eaten)
        {
            if (eater != this)
                return;

            // If we receive a reward for taking a step, record it
            reward += eaten.Species.Reward;
        }

        protected override ISignalArray activateNetwork(double[] sensors)
        {
            // Update the value function for the previously-chosen actions
            updateValueFunction(sensors);

            // Save the state for next time
            sensors.CopyTo(_prevState, 0);

            // Calculate the value function for this state
            var values = base.activateNetwork(sensors);

            // Save the values for next time
            values.CopyTo(_prevValues, 0);

            // Select the actions to take
            _prevActions = selectEpsilonGreedy(values);

            // Return the result
            var results = new SignalArray(new double[] { _actions[_prevActions[0]], _actions[_prevActions[1]] }, 0, 2);

            //Console.WriteLine("Selecting: ({0},{1}) -> ({2:N2},{3:N2})", _prevActions[0], _prevActions[1], results[0], results[1]);
            
            return results;
        }

        private void updateValueFunction(double[] sensors)
        {
            // Add the discounted maximum reward we expect for the current state
            var nextValues = base.activateNetwork(sensors);
            var bestValues = selectGreedy(nextValues);

            // Set the reward for the action we took plus the discounted look-ahead reward
            _prevValues[_prevActions[0]] = Math.Max(-1, Math.Min(1, reward / MaxReward + DiscountFactor * nextValues[bestValues[0]]));
            _prevValues[_prevActions[1]] = Math.Max(-1, Math.Min(1, reward / MaxReward + DiscountFactor * nextValues[bestValues[1]]));

            // Run a backprop epoch
            ((FastCyclicNetwork)Brain).Train(_prevState, _prevValues);

            // Reset the reward
            reward = 0;
        }

        private int[] selectEpsilonGreedy(ISignalArray values)
        {
            int[] results;
            if (_random.Next() < Epsilon)
                results = selectRandom(values);
            else
                results = selectGreedy(values);
            return results;
        }

        private int[] selectRandom(ISignalArray values)
        {
            return new int[] { _random.Next(0, _numOrientationActions), 
                                      _random.Next(_numOrientationActions, values.Length) };
        }

        private int[] selectGreedy(ISignalArray values)
        {
            int[] results = new int[2];

            int maxOrientation = 0;
            for (int i = 1; i < _numOrientationActions; i++)
                if (values[i] > values[maxOrientation])
                    maxOrientation = i;
            results[0] = maxOrientation;

            int maxVelocity = _numOrientationActions;
            for (int i = _numOrientationActions + 1; i < values.Length; i++)
                if (values[i] > values[maxVelocity])
                    maxVelocity = i;
            results[1] = maxVelocity;

            return results;
        }
    }
}

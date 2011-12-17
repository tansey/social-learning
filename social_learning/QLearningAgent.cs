using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;
using SharpNeat.Phenomes.NeuralNets;
using System.Diagnostics;
using SharpNeat.Utility;
using System.Threading;

namespace social_learning
{
    /// <summary>
    /// A Q-Learning agent. The agent learns via the Q-Learning Temporal Difference Algorithm.
    /// </summary>
    public class QLearningAgent : NeuralAgent
    {
        const double DEFAULT_LEARNING_RATE = 0.1;
        const double DEFAULT_DISCOUNT_FACTOR = 0.9;
        const double DEFAULT_EPSILON = 0.1;

        readonly int _numVelocityActions, _numOrientationActions;
        readonly Random _random;
        double[] _observedValue;
        public double[] _prevState;
        double reward;

        /// <summary>
        /// The maximum observed reward. This is used to scale the observed reward into the range [0,1]
        /// when updating the value function.
        /// </summary>
        public double MaxReward { get; set; }

        /// <summary>
        /// The learning rate for updating the value function.
        /// </summary>
        public double LearningRate
        {
            get { return ((FastCyclicNetwork)Brain).BackpropLearningRate; }
            set { ((FastCyclicNetwork)Brain).BackpropLearningRate = value; }
        }

        /// <summary>
        /// The discount factor for future rewards.
        /// </summary>
        public double DiscountFactor { get; set; }

        /// <summary>
        /// The probability of making a random action instead of the greedy action.
        /// </summary>
        public double Epsilon { get; set; }

        /// <summary>
        /// Creates a new Q-Learning agent.
        /// </summary>
        /// <param name="id">The unique ID of this agent.</param>
        /// <param name="brain">The neural network value function for this agent. It should have (2 + # of sensors) input nodes and 1 output node.</param>
        /// <param name="numOrientationActions">The number of buckets to discretize the orientation action spacer into.</param>
        /// <param name="numVelocityActions">The number of buckets to discretize the velocity action spacer into.</param>
        /// <param name="world">The world this agent will be evaluated in.</param>
        public QLearningAgent(int id, IBlackBox brain, int numOrientationActions, int numVelocityActions, World world)
            : base(id, brain)
        {
            Debug.Assert(brain.OutputCount == 1, "Incorrect number of outputs in neural network!");

            _numVelocityActions = numVelocityActions;
            _numOrientationActions = numOrientationActions;
            _random = new Random();
            _prevState = new double[brain.InputCount];
            _observedValue = new double[1];
            world.PlantEaten += new World.PlantEatenHandler(world_PlantEaten);
            MaxReward = 200;
            LearningRate = DEFAULT_LEARNING_RATE;
            DiscountFactor = DEFAULT_DISCOUNT_FACTOR;
            Epsilon = DEFAULT_EPSILON;

            // The backprop learning rate is equivalent to the Q-Learning learning rate.
            ((FastCyclicNetwork)Brain).BackpropLearningRate = LearningRate;
        }

        void world_PlantEaten(object sender, IAgent eater, Plant eaten)
        {
            // Only update the observed reward if it was a result of our action
            if (eater != this)
                return;

            // If we receive a reward for taking a step, record it
            reward += eaten.Species.Reward;
        }

        /// <summary>
        /// Called at every step in the world. Given the sensor input, returns the change in orientation and velocity
        /// in the range [0,1].
        /// </summary>
        protected override ISignalArray activateNetwork(double[] sensors)
        {
            // Update the value function for the previously-chosen actions
            updateValueFunction(sensors);

            // Select the actions to take
            _prevState = selectEpsilonGreedy(sensors);

            // Return the result
            var results = new SignalArray(new double[] { _prevState[_prevState.Length - 2], _prevState[_prevState.Length - 1] }, 0, 2);

            //Console.WriteLine("Selecting: ({0},{1})", results[0], results[1]);
            
            return results;
        }

        private void updateValueFunction(double[] sensors)
        {
            // Add the discounted maximum reward we expect for the current stateActionPair
            var bestValue = greedyValue(sensors);

            // Scale the reward in the range [0,1]
            var scaledReward = (reward + MaxReward) / (2.0 * MaxReward);

            // Set the reward for the action we took plus the discounted look-ahead reward
            _observedValue[0] = Math.Max(0, Math.Min(1, scaledReward + DiscountFactor * bestValue));

            // Run a backprop epoch
            ((FastCyclicNetwork)Brain).Train(_prevState, _observedValue);

            // Reset the reward
            reward = 0;
        }

        private double[] selectEpsilonGreedy(double[] sensors)
        {
            double[] results;
            if (_random.NextDouble() < Epsilon)
                results = selectRandom(sensors);
            else
                results = selectGreedy(sensors);
            return results;
        }

        private double[] selectRandom(double[] sensors)
        {
            double[] results = new double[sensors.Length + 2];
            sensors.CopyTo(results, 0);

            // Randomly select an orientation and a velocity
            results[sensors.Length - 2] = _random.Next(_numOrientationActions) / (double)(_numOrientationActions-1);
            results[sensors.Length - 1] = _random.Next(_numVelocityActions) / (double)(_numVelocityActions-1);

            return results;
        }

        private double[] selectGreedy(double[] sensors)
        {
            double[] stateActionPair = new double[sensors.Length + 2], results = new double[sensors.Length + 2];
            sensors.CopyTo(stateActionPair, 0);
            sensors.CopyTo(results, 0);
            double max = -1;

            for (int i = 0; i < _numOrientationActions; i++)
                for (int j = 0; j < _numVelocityActions; j++)
                {
                    stateActionPair[stateActionPair.Length - 2] = i / (double)(_numOrientationActions-1);
                    stateActionPair[stateActionPair.Length - 1] = j / (double)(_numVelocityActions-1);
                    double value = base.activateNetwork(stateActionPair)[0];
                    if (value > max)
                    {
                        max = value;
                        results[results.Length - 2] = stateActionPair[results.Length - 2];
                        results[results.Length - 1] = stateActionPair[results.Length - 1];
                    }
                }

            return results;
        }

        private double greedyValue(double[] sensors)
        {
            double[] stateActionPair = new double[sensors.Length + 2];
            sensors.CopyTo(stateActionPair, 0);
            double max = -1;

            for (int i = 0; i < _numOrientationActions; i++)
                for (int j = 0; j < _numVelocityActions; j++)
                {
                    stateActionPair[stateActionPair.Length - 2] = i / (double)(_numOrientationActions-1);
                    stateActionPair[stateActionPair.Length - 1] = j / (double)(_numVelocityActions-1);
                    double value = base.activateNetwork(stateActionPair)[0];
                    if (value > max)
                        max = value;
                }

            return max;
        }

    }
}

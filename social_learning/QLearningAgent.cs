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
    /// <summary>
    /// A Q-Learning agent. The agent learns via the Q-Learning Temporal Difference Algorithm.
    /// </summary>
    public class QLearningAgent : NeuralAgent
    {
        const double DEFAULT_LEARNING_RATE = 0.1;
        const double DEFAULT_DISCOUNT_FACTOR = 0.9;
        const double DEFAULT_EPSILON = 1;

        readonly int _numVelocityActions, _numOrientationActions;
        readonly Random _random;
        readonly double[] _actions;

        public int[] _prevActions;
        public double[] _prevState;
        public double[] _prevValues;
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
        /// <param name="brain">The neural network value function for this agent.</param>
        /// <param name="numOrientationActions">The number of buckets to discretize the orientation action spacer into.</param>
        /// <param name="numVelocityActions">The number of buckets to discretize the velocity action spacer into.</param>
        /// <param name="world">The world this agent will be evaluated in.</param>
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

            // The backprop learning rate is equivalent to the Q-Learning learning rate.
            ((FastCyclicNetwork)Brain).BackpropLearningRate = LearningRate;

            // Create a mapping from the output neurons to the actual action taken
            _actions = new double[brain.OutputCount];
            for (int i = 0; i < numOrientationActions; i++)
                _actions[i] = 1.0 / (double)(numOrientationActions-1) * i;
            for (int i = 0; i < numVelocityActions; i++)
                _actions[i + numOrientationActions] = 1.0 / (double)(numVelocityActions-1) * i;

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

            //if (reward != 0)
            //    Console.WriteLine("Before: ({0},{1})", _prevValues[_prevActions[0]], _prevValues[_prevActions[1]]);

            // Scale the reward in the range [0,1]
            var scaledReward = (reward + MaxReward) / (2 * MaxReward);

            // Set the reward for the action we took plus the discounted look-ahead reward
            _prevValues[_prevActions[0]] = Math.Max(0, Math.Min(1, scaledReward + DiscountFactor * nextValues[bestValues[0]]));
            _prevValues[_prevActions[1]] = Math.Max(0, Math.Min(1, scaledReward + DiscountFactor * nextValues[bestValues[1]]));

            //if (reward != 0)
            //{
            //    Console.WriteLine("Observed: {0}", reward);
            //    Console.WriteLine("Scaled: {0}", scaledReward);
            //    Console.WriteLine("Updating with: ({0},{1})", _prevValues[_prevActions[0]], _prevValues[_prevActions[1]]);
            //}

            // Run a backprop epoch
            ((FastCyclicNetwork)Brain).Train(_prevState, _prevValues);

            //if (reward != 0)
            //{
            //    base.activateNetwork(_prevState).CopyTo(_prevValues, 0);
            //    Console.WriteLine("Updated: ({0},{1})", _prevValues[_prevActions[0]], _prevValues[_prevActions[1]]);
            //}

            // Reset the reward
            reward = 0;
        }

        private int[] selectEpsilonGreedy(ISignalArray values)
        {
            int[] results;
            if (_random.NextDouble() < Epsilon)
                results = selectRandom(values);
            else
                results = selectGreedy(values);
            return results;
        }

        private int[] selectRandom(ISignalArray values)
        {
            // Randomly select an orientation and a velocity
            return new int[] { _random.Next(0, _numOrientationActions), 
                                      _random.Next(_numOrientationActions, values.Length) };
        }

        private int[] selectGreedy(ISignalArray values)
        {
            int[] results = new int[2];

            // Find the orientation action with the highest value
            int maxOrientation = 0;
            for (int i = 1; i < _numOrientationActions; i++)
                if (values[i] > values[maxOrientation])
                    maxOrientation = i;
            results[0] = maxOrientation;

            // Find the velocity action with the highest value
            int maxVelocity = _numOrientationActions;
            for (int i = _numOrientationActions + 1; i < values.Length; i++)
                if (values[i] > values[maxVelocity])
                    maxVelocity = i;
            results[1] = maxVelocity;

            return results;
        }
    }
}

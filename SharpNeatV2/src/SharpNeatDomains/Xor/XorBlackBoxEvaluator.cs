/* ***************************************************************************
 * This file is part of SharpNEAT - Evolution of Neural Networks.
 * 
 * Copyright 2004-2006, 2009-2010 Colin Green (sharpneat@gmail.com)
 *
 * SharpNEAT is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * SharpNEAT is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with SharpNEAT.  If not, see <http://www.gnu.org/licenses/>.
 */
using System.Diagnostics;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using SharpNeat.Phenomes.NeuralNets;
using System;

namespace SharpNeat.Domains
{
    /// <summary>
    /// A black box evaluator for the XOR logic gate problem domain. 
    /// 
    /// XOR (also known as Exclusive OR) is a type of logical disjunction on two operands that results in
    /// a value of true if and only if exactly one of the operands has a value of 'true'. A simple way 
    /// to state this is 'one or the other but not both.'.
    /// 
    /// This evaulator therefore requires that the black box to be evaluated has has two inputs and one 
    /// output all using the range 0..1
    /// 
    /// In turn each of the four possible test cases are applied to the two inputs, the network is activated
    /// and the output is evaulated. If a 'false' response is requried we expect an output of zero, for true
    /// we expect a 1.0. Fitness for each test case is the difference between the output and the wrong output, 
    /// thus a maximum of 1 can be scored on each test case giving a maximum of 4. In addition each outputs is
    /// compared against a threshold of 0.5, if all four outputs are on teh correct side of the threshold then
    /// 10.0 is added to the total fitness. Therefore a black box that answers correctly but very close to the
    /// threshold will score just above 10, and a black box that answers correctly with perfect 0.0 and 1.0 
    /// answers will score a maximum of 14.0.
    /// 
    /// The first type of evaulation punishes for difference from the required outputs and therefore represents
    /// a smooth fitness space (we can evolve gradually towards better scores). The +10 score for 4 correct
    /// responses is 'all or nothing', in other words it is a fitness space with a large step and no indication
    /// of where the step is, which on it's own would be a poor fitness space as it required evolution to stumble
    /// on the correct network by random rather than ascending a gradient in the fitness space. If however we do 
    /// stumble on a black box that answers correctly but close to the threshold, then we would like that box to 
    /// obtain a higher score than a network with, say, 3 strong correct responses and but wrong overall. We can
    /// improve the correct box's output difference from threshold value gradually, while the box with 3 correct
    /// responses may actually be in the wrong area of the fitness space alltogether - in the wrong 'ballpark'.
    /// </summary>
    public class XorBlackBoxEvaluator : IPhenomeEvaluator<IBlackBox>
    {
        const double StopFitness = 10.0;
        ulong _evalCount;
        bool _stopConditionSatisfied;

        #region IPhenomeEvaluator<IBlackBox> Members

        /// <summary>
        /// Gets the total number of evaluations that have been performed.
        /// </summary>
        public ulong EvaluationCount
        {
            get { return _evalCount; }
        }

        /// <summary>
        /// Gets a value indicating whether some goal fitness has been achieved and that
        /// the the evolutionary algorithm/search should stop. This property's value can remain false
        /// to allow the algorithm to run indefinitely.
        /// </summary>
        public bool StopConditionSatisfied
        {
            get { return _stopConditionSatisfied; }
        }

        /// <summary>
        /// Evaluate the provided IBlackBox against the XOR problem domain and return its fitness score.
        /// </summary>
        public FitnessInfo Evaluate(IBlackBox box)
        {
            double fitness = 0;
            double output;
            double pass = 1.0;
            ISignalArray inputArr = box.InputSignalArray;
            ISignalArray outputArr = box.OutputSignalArray;

            _evalCount++;

            // Train the network
            FastCyclicNetwork net = (FastCyclicNetwork)box;

            //----- Test 0,0
            box.ResetState();

            // Set the input values
            inputArr[0] = 0.0;
            inputArr[1] = 0.0;

            // Activate the black box.
            box.Activate();
            if (!box.IsStateValid)
            {   // Any black box that gets itself into an invalid state is unlikely to be
                // any good, so lets just bail out here.
                return FitnessInfo.Zero;
            }

            // Read output signal.
            output = Math.Min(1, Math.Max(0, outputArr[0]));
            var output0 = output;
            Debug.Assert(output >= 0.0, "Unexpected negative output.");

            // Calculate this test case's contribution to the overall fitness score.
            //fitness += 1.0 - output; // Use this line to punish absolute error instead of squared error.
            fitness += 1.0 - (output * output);
            if (output > 0.5)
            {
                pass = 0.0;
            }

            //----- Test 1,1
            // Reset any black box state from the previous test case.
            box.ResetState();

            // Set the input values
            inputArr[0] = 1.0;
            inputArr[1] = 1.0;

            // Activate the black box.
            box.Activate();
            if (!box.IsStateValid)
            {   // Any black box that gets itself into an invalid state is unlikely to be
                // any good, so lets just bail out here.
                return FitnessInfo.Zero;
            }

            // Read output signal.
            output = Math.Min(1, Math.Max(0, outputArr[0]));
            var output1 = output;
            Debug.Assert(output >= 0.0, "Unexpected negative output.");

            // Calculate this test case's contribution to the overall fitness score.
            //fitness += 1.0 - output; // Use this line to punish absolute error instead of squared error.
            fitness += 1.0 - (output * output);
            if (output > 0.5)
            {
                pass = 0.0;
            }

            //----- Test 0,1
            // Reset any black box state from the previous test case.
            box.ResetState();

            // Set the input values
            inputArr[0] = 0.0;
            inputArr[1] = 1.0;

            // Activate the black box.
            box.Activate();
            if (!box.IsStateValid)
            {   // Any black box that gets itself into an invalid state is unlikely to be
                // any good, so lets just bail out here.
                return FitnessInfo.Zero;
            }

            // Read output signal.
            output = Math.Min(1, Math.Max(0, outputArr[0]));
            var output2 = output;
            Debug.Assert(output >= 0.0, "Unexpected negative output.");

            // Calculate this test case's contribution to the overall fitness score.
            // fitness += output; // Use this line to punish absolute error instead of squared error.
            fitness += 1.0 - ((1.0 - output) * (1.0 - output));
            if (output <= 0.5)
            {
                pass = 0.0;
            }

            //----- Test 1,0
            // Reset any black box state from the previous test case.
            box.ResetState();

            // Set the input values
            inputArr[0] = 1.0;
            inputArr[1] = 0.0;

            // Activate the black box.
            box.Activate();
            if (!box.IsStateValid)
            {   // Any black box that gets itself into an invalid state is unlikely to be
                // any good, so lets just bail out here.
                return FitnessInfo.Zero;
            }

            // Read output signal.
            output = Math.Min(1, Math.Max(0, outputArr[0]));
            var output3 = output;
            Debug.Assert(output >= 0.0, "Unexpected negative output.");

            // Calculate this test case's contribution to the overall fitness score.
            // fitness += output; // Use this line to punish absolute error instead of squared error.
            fitness += 1.0 - ((1.0 - output) * (1.0 - output));
            if (output <= 0.5)
            {
                pass = 0.0;
            }

            // If all four outputs were correct, that is, all four were on the correct side of the
            // threshold level - then we add 10 to the fitness.
            fitness += pass * 10.0;

            if (fitness > 3.6)
                Console.WriteLine("Before Nodes={5} Connections={6} [0,0]={0:N3} [1,1]={1:N3} [0,1]={2:N3} [1,0]={3:N3} fitness={4:N3}", output0, output1, output2, output3, fitness, net.HiddenCount, net.ConnectionCount);
            fitness = 0;

            net.BackpropLearningRate = 0.1;
            net.Momentum = 0.9;
            double[][] exampleInputs = new double[][] { 
                new double[] { 0, 0 },
                new double[] { 0, 1 },
                new double[] { 1, 0 },
                new double[] { 1, 1 }
            };
            double[][] exampleOutputs = new double[][] { 
                new double[] { 0 },
                new double[] { 1 },
                new double[] { 1 },
                new double[] { 0 }
                };
            net.RandomizeWeights();
            for (int i = 0; i < 4000; i++)
            {
                net.Train(exampleInputs[i % 4], exampleOutputs[i % 4]);
            }

        //----- Test 0,0
            box.ResetState();

            // Set the input values
            inputArr[0] = 0.0;
            inputArr[1] = 0.0;

            // Activate the black box.
            box.Activate();
            if(!box.IsStateValid) 
            {   // Any black box that gets itself into an invalid state is unlikely to be
                // any good, so lets just bail out here.
                return FitnessInfo.Zero;
            }

            // Read output signal.
            output = Math.Min(1, Math.Max(0, outputArr[0]));
             output0 = output;
            Debug.Assert(output >= 0.0, "Unexpected negative output.");

            // Calculate this test case's contribution to the overall fitness score.
            //fitness += 1.0 - output; // Use this line to punish absolute error instead of squared error.
            fitness += 1.0-(output*output);
            if(output > 0.5) {
                pass = 0.0;
            }

        //----- Test 1,1
            // Reset any black box state from the previous test case.
            box.ResetState();

            // Set the input values
            inputArr[0] = 1.0;
            inputArr[1] = 1.0;

            // Activate the black box.
            box.Activate();
            if(!box.IsStateValid) 
            {   // Any black box that gets itself into an invalid state is unlikely to be
                // any good, so lets just bail out here.
                return FitnessInfo.Zero;
            }

            // Read output signal.
            output = Math.Min(1, Math.Max(0, outputArr[0]));
            output1 = output;
            Debug.Assert(output >= 0.0, "Unexpected negative output.");

            // Calculate this test case's contribution to the overall fitness score.
            //fitness += 1.0 - output; // Use this line to punish absolute error instead of squared error.
            fitness += 1.0-(output*output);
            if(output > 0.5) {
                pass = 0.0;
            }

        //----- Test 0,1
            // Reset any black box state from the previous test case.
            box.ResetState();

            // Set the input values
            inputArr[0] = 0.0;
            inputArr[1] = 1.0;

            // Activate the black box.
            box.Activate();
            if(!box.IsStateValid) 
            {   // Any black box that gets itself into an invalid state is unlikely to be
                // any good, so lets just bail out here.
                return FitnessInfo.Zero;
            }

            // Read output signal.
            output = Math.Min(1, Math.Max(0, outputArr[0]));
            output2 = output;
            Debug.Assert(output >= 0.0, "Unexpected negative output.");

            // Calculate this test case's contribution to the overall fitness score.
            // fitness += output; // Use this line to punish absolute error instead of squared error.
            fitness += 1.0-((1.0-output)*(1.0-output));
            if(output <= 0.5) {
                pass = 0.0;
            }

        //----- Test 1,0
            // Reset any black box state from the previous test case.
            box.ResetState();

            // Set the input values
            inputArr[0] = 1.0;
            inputArr[1] = 0.0;

            // Activate the black box.
            box.Activate();
            if(!box.IsStateValid) 
            {   // Any black box that gets itself into an invalid state is unlikely to be
                // any good, so lets just bail out here.
                return FitnessInfo.Zero;
            }

            // Read output signal.
            output = Math.Min(1, Math.Max(0, outputArr[0]));
            output3 = output;
            Debug.Assert(output >= 0.0, "Unexpected negative output.");

            // Calculate this test case's contribution to the overall fitness score.
            // fitness += output; // Use this line to punish absolute error instead of squared error.
            fitness += 1.0-((1.0-output)*(1.0-output));
            if(output <= 0.5) {
                pass = 0.0;
            }

            // If all four outputs were correct, that is, all four were on the correct side of the
            // threshold level - then we add 10 to the fitness.
            fitness += pass * 10.0;


            if(fitness >= StopFitness) {
                _stopConditionSatisfied = true;
            }


            if (fitness > 3.6)
                Console.WriteLine("After Nodes={5} Connections={6} [0,0]={0:N3} [1,1]={1:N3} [0,1]={2:N3} [1,0]={3:N3} fitness={4:N3}", output0, output1, output2, output3, fitness, net.HiddenCount, net.ConnectionCount);

            return new FitnessInfo(fitness, fitness);
        }
        Random random = new Random();
        private double gaussianMutation(double mean, double stddev)
        {
            double x1 = random.NextDouble();
            double x2 = random.NextDouble();

            // The method requires sampling from a uniform random of (0,1]
            // but Random.NextDouble() returns a sample of [0,1).
            // Thanks to Colin Green for catching this.
            if (x1 == 0)
                x1 = 1;
            if (x2 == 0)
                x2 = 1;

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return y1 * stddev + mean;
        }

        private double clamp(double val, double min, double max)
        {
            if (val >= max)
                return max;
            if (val <= min)
                return min;
            return val;
        }


        /// <summary>
        /// Reset the internal state of the evaluation scheme if any exists.
        /// Note. The XOR problem domain has no internal state. This method does nothing.
        /// </summary>
        public void Reset()
        {   
        }

        #endregion
    }
}

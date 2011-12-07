using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SharpNeat.Utility;
using System.IO;
using System.Xml;
using SharpNeat.Decoders.Neat;
using SharpNeat.Phenomes.NeuralNets;
using SharpNeat.Genomes.Neat;
using SharpNeat.Core;
using SharpNeat.Phenomes;

namespace TestBackprop
{
    class Program
    {
        const string NEURAL_CONFIG_FILE = @"..\..\..\..\..\experiments\backprop_test.config.xml";
        static void Main(string[] args)
        {
            LearningRateExperiment _experiment = new LearningRateExperiment();
            // Load config XML.
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(NEURAL_CONFIG_FILE);
            _experiment.Initialize("blah", xmlConfig.DocumentElement);

            LearningRateExperiment.CreateNetwork("temp", 2, 3, 1);

            var genome = _experiment.LoadPopulation(XmlReader.Create("temp"))[0];

            var decoder = _experiment.CreateGenomeDecoder();

            Console.WriteLine("Original Network");
            Backprop(genome, decoder, 0);

            Console.WriteLine("Backpropped Learning Rate = 1");
            Backprop(genome, decoder, 1);

            int epochs = 1000000;
            double learningRate = 0.1;
            Console.WriteLine("Backpropped Learning Rate = {0}, {1} epochs", learningRate, epochs);
            BackpropEpochs(genome, decoder, 0.01, 100000);
        }

        private static void Backprop(NeatGenome genome, IGenomeDecoder<NeatGenome, IBlackBox> decoder, double learningRate)
        {
            var phenome = decoder.Decode(genome);

            var network = (FastCyclicNetwork)phenome;
            network.Momentum = 0;
            network.BackpropLearningRate = learningRate;

            //Console.WriteLine("Weights Before:");
            //PrintWeights(network);
            //Console.WriteLine();

            network.Train(new double[] { 0.3, 0.3 }, new double[] { 0.9 });

            Console.WriteLine("Weights:");
            PrintWeights(network);
            Console.WriteLine();

            network.Train(new double[] { 0.3, 0.3 }, new double[] { 0.9 });

            Console.WriteLine("Weights:");
            PrintWeights(network);
            Console.WriteLine();

        }

        private static void BackpropEpochs(NeatGenome genome, IGenomeDecoder<NeatGenome, IBlackBox> decoder, double learningRate, int epochs)
        {
            var phenome = decoder.Decode(genome);

            var network = (FastCyclicNetwork)phenome;
            network.Momentum = 0;
            network.BackpropLearningRate = learningRate;

            //Console.WriteLine("Weights Before:");
            //PrintWeights(network);
            //Console.WriteLine();

            //double[][] inputs = TrainXOR(epochs, network);
            
            for(int i = 0; i < epochs; i++)
                network.Train(new double[] { 0.3, 0.3 }, new double[] { 0.9 });

            Console.WriteLine("Weights After:");
            PrintWeights(network);
            Console.WriteLine();

            //RunXor(network, inputs);
        }

        private static double[][] TrainXOR(int epochs, FastCyclicNetwork network)
        {
            double[][] inputs = new double[][] { 
                new double[] { 0, 0 },
                new double[] { 1, 0 },
                new double[] { 0, 1 },
                new double[] { 1, 1 }
            };
            double[][] outputs = new double[][] { 
                new double[] { 0 }, 
                new double[] { 1 }, 
                new double[] { 1 }, 
                new double[] { 0 } };

            for (int i = 0; i < epochs; i++)
                network.Train(inputs[i % inputs.Length], outputs[i % outputs.Length]);
            return inputs;
        }

        private static void RunXor(FastCyclicNetwork network, double[][] inputs)
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                network.ResetState();

                // Convert the sensors into an input array for the network
                for (int j = 0; j < inputs[i].Length; j++)
                    network.InputSignalArray[j] = inputs[i][j];

                // Activate the network
                network.Activate();

                Console.WriteLine("[{0},{1}] -> {2:N4}", inputs[i][0], inputs[i][1], network.OutputSignalArray[0]);
            }
        }

        static void PrintWeights(FastCyclicNetwork network)
        {
            foreach (var conn in network.ConnectionArray)
                Console.WriteLine("[{0}] -> [{1}]: {2:N4}", conn._srcNeuronIdx, conn._tgtNeuronIdx, conn._weight);
        }

    }
}

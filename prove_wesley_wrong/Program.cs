using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SharpNeat.Utility;
using System.IO;
using VisualizeWorld;
using System.Xml;
using SharpNeat.Decoders.Neat;
using SharpNeat.Phenomes.NeuralNets;
using SharpNeat.Genomes.Neat;
using SharpNeat.Core;
using SharpNeat.Phenomes;

namespace prove_wesley_wrong
{
    class Program
    {
        const string NEURAL_CONFIG_FILE = @"..\..\..\experiments\neural.config.xml";
        static void Main(string[] args)
        {
            LearningRateExperiment _experiment = new LearningRateExperiment();
            // Load config XML.
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(NEURAL_CONFIG_FILE);
            _experiment.Initialize("blah", xmlConfig.DocumentElement);

            LearningRateExperiment.CreateNetwork("temp", 2, 3, 3, 1);

            var genome = _experiment.LoadPopulation(XmlReader.Create("temp"))[0];

            var decoder = _experiment.CreateGenomeDecoder();

            Console.WriteLine("Original Network");
            Backprop(genome, decoder, 0);

            //Console.WriteLine("Backpropped Learning Rate = 1");
            //Backprop(genome, decoder, 1);

            Console.WriteLine("Backpropped Learning Rate = 0.1, 100000 epochs");
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

            double[][] inputs = new double[][] { 
                new double[] { 0, 0 },
                new double[] { 1, 0 },
                new double[] { 0, 1 },
                new double[] { 1, 1 }
            };
            double[][] outputs = new double[][] { new double[] { 0 }, new double[] { 1 }, new double[] { 1 }, new double[] { 0 } };

            network.Train(inputs[1], outputs[1]);

            PrintWeights(network);
            Console.WriteLine();

            network.Train(inputs[1], outputs[1]);

            PrintWeights(network);
        }

        private static void BackpropEpochs(NeatGenome genome, IGenomeDecoder<NeatGenome, IBlackBox> decoder, double learningRate, int epochs)
        {
            var phenome = decoder.Decode(genome);

            var network = (FastCyclicNetwork)phenome;
            //network.Momentum = 0.9;
            network.BackpropLearningRate = learningRate;

            //Console.WriteLine("Weights Before:");
            //PrintWeights(network);
            //Console.WriteLine();

            double[][] inputs = new double[][] { 
                //new double[] { 0, 0 },
                new double[] { 1, 0 },
                new double[] { 0, 1 },
                new double[] { 1, 1 }
            };
            double[][] outputs = new double[][] { 
                //new double[] { 1 }, 
                new double[] { 0 }, 
                new double[] { 0 }, 
                new double[] { 1 } };

            for(int i = 0; i < epochs; i++)
                network.Train(inputs[epochs/100%3], outputs[epochs/100%3]);

            Console.WriteLine("Weights After:");
            PrintWeights(network);
            Console.WriteLine();

            for(int i = 0; i < 3; i++)
            {
                network.ResetState();

                // Convert the sensors into an input array for the network
                for (int j = 0; j < inputs[i].Length; j++)
                    network.InputSignalArray[j] = inputs[i][j];

                // Activate the network
                network.Activate();

                Console.WriteLine("[{0},{1}] -> {2}", inputs[i][0], inputs[i][1], network.OutputSignalArray[0]);
            }
        }

        static void PrintWeights(FastCyclicNetwork network)
        {
            foreach(var conn in network.ConnectionArray)
                Console.WriteLine("[{0}] -> [{1}]: {2:N4}", conn._srcNeuronIdx, conn._tgtNeuronIdx, conn._weight);
        }

    }
}

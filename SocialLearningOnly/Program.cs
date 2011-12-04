using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using VisualizeWorld;
using System.Xml;
using SharpNeat.Core;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes.NeuralNets;
using SharpNeat.Phenomes;
using SharpNeat.Utility;
using social_learning;

namespace SocialLearningOnly
{
    class Program
    {
        const string CONFIG_FILE = @"..\..\..\experiments\social_only.config.xml";
        const string FEED_FORWARD_NETWORK_FILE = @"..\..\..\experiments\feedforward_network.xml";
        const string RESULTS_FILE = @"..\..\..\experiments\social_only_results.csv";
        static SimpleExperiment _experiment;
        static SimpleEvaluator<NeatGenome> _evaluator;
        static FastRandom _random;

        static void Main(string[] args)
        {
            _random = new FastRandom();

            _experiment = new SimpleExperiment();
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(CONFIG_FILE);
            _experiment.Initialize("SimpleEvolution", xmlConfig.DocumentElement);
            _experiment.NeatGenomeParameters.AddConnectionMutationProbability = 0;
            _experiment.NeatGenomeParameters.AddNodeMutationProbability = 0;
            _experiment.NeatGenomeParameters.DeleteConnectionMutationProbability = 0;

            CreateNetwork(FEED_FORWARD_NETWORK_FILE, _experiment.InputCount, 20, 2);

            // Record the changes at each step
            _experiment.World.Stepped += new social_learning.World.StepEventHandler(World_Stepped);

            // Read in the seed genome from file. This is the prototype for our other population of networks.
            var seed = _experiment.LoadPopulation(XmlReader.Create(FEED_FORWARD_NETWORK_FILE))[0];

            // Create a genome factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
            IGenomeFactory<NeatGenome> genomeFactory = _experiment.CreateGenomeFactory();

            // Create an initial population of randomly generated genomes.
            List<NeatGenome> genomeList = genomeFactory.CreateGenomeList(_experiment.DefaultPopulationSize, 0, seed);

            // Randomize the genomes
            RandomizeGenomes(genomeList);

            // Create genome decoder.
            IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = _experiment.CreateGenomeDecoder();

            // Create the evaluator that will handle the simulation
            _evaluator = new SimpleEvaluator<NeatGenome>(genomeDecoder, _experiment.World, AgentTypes.Social)
                {
                    MaxTimeSteps = 50000000UL,
                    BackpropEpochsPerExample = 1
                };

            using (TextWriter writer = new StreamWriter(RESULTS_FILE))
                writer.WriteLine("Step,Best,Average");

            // Start the simulation
            _evaluator.Evaluate(genomeList);
        }

        private static void RandomizeGenomes(List<NeatGenome> genomeList)
        {
            foreach (var genome in genomeList)
                foreach (var connection in genome.ConnectionGeneList)
                    connection.Weight = _random.NextDouble() * 10.0 - 5.0;
        }

        static bool stepReady = false;
        static void World_Stepped(object sender, EventArgs e)
        {
            Console.WriteLine("Step {0} Best: {1} Avg: {2} Memory: {3}", 
                                _experiment.World.CurrentStep,
                                _experiment.World.Agents.Max(f => f.Fitness),
                                _experiment.World.Agents.Average(f => f.Fitness),
                                SocialAgent.DEFAULT_MEMORY_SIZE
                                );

            if (_experiment.World.CurrentStep > 0 && _experiment.World.CurrentStep % 3000 == 0)
            {
                using (TextWriter writer = new StreamWriter(RESULTS_FILE, true))
                    writer.WriteLine("{0},{1},{2}", _evaluator.CurrentTimeStep, 
                                                    _experiment.World.Agents.Max(f => f.Fitness),
                                                    _experiment.World.Agents.Average(f => f.Fitness));
                _experiment.World.Reset();
            }

            if (_evaluator.CurrentTimeStep % 30000 == 0)
            {
                if (stepReady)
                    SocialAgent.DEFAULT_MEMORY_SIZE++;
                else
                    stepReady = true;
            }
        }

        static void CreateNetwork(string filename, params int[] layers)
        {
            Debug.Assert(layers.Length >= 2);
            Debug.Assert(layers.Min() > 0);

            const string inputNodeFormat = @"        <Node type=""in"" id=""{0}"" />";
            const string outputNodeFormat = @"        <Node type=""out"" id=""{0}"" />";
            const string hiddenNodeFormat = @"        <Node type=""hid"" id=""{0}"" />";
            int nextId = 1;

            StringBuilder nodes = new StringBuilder();

            int[][] layerIds = new int[layers.Length][];
            for (int i = 0; i < layers.Length; i++)
                layerIds[i] = new int[layers[i]];

            // Add the input neurons
            for (int i = 0; i < layers[0]; i++)
            {
                int id = nextId++;
                nodes.AppendLine(string.Format(inputNodeFormat, id));
                layerIds[0][i] = id;
            }

            // Add the output neurons (have to do this now because of conventions in SharpNEAT)
            for (int i = 0; i < layers.Last(); i++)
            {
                int id = nextId++;
                nodes.AppendLine(string.Format(outputNodeFormat, id));
                layerIds[layers.Length - 1][i] = id;
            }

            // Add the hidden neurons
            for (int i = 1; i < layers.Length - 1; i++)
                for (int j = 0; j < layers[i]; j++)
                {
                    int id = nextId++;
                    nodes.AppendLine(string.Format(hiddenNodeFormat, id));
                    layerIds[i][j] = id;
                }

            const string connectionFormat = @"        <Con id=""{0}"" src=""{1}"" tgt=""{2}"" wght=""{3}"" />";
            StringBuilder connections = new StringBuilder();

            // Add the bias connections to all non-input nodes
            int lastInput = nextId;
            for (int i = layers[0] + 1; i < lastInput; i++)
                connections.AppendLine(string.Format(connectionFormat, nextId++, 0, i, _random.NextDouble() * 10.0 - 5.0));

            // Add the connections
            // Note that you need to randomize these connections externally
            for (int i = 1; i < layers.Length; i++)
                for (int j = 0; j < layers[i]; j++)
                    for (int k = 0; k < layers[i - 1]; k++)
                        connections.AppendLine(string.Format(connectionFormat, nextId++, layerIds[i - 1][k], layerIds[i][j], _random.NextDouble() * 10.0 - 5.0));

            using (TextWriter writer = new StreamWriter(filename))
                writer.WriteLine(string.Format(NETWORK_FORMAT, nodes.ToString(), connections.ToString(), nextId++));

        }

        const string NETWORK_FORMAT =
@"<Root>
  <ActivationFunctions>
    <Fn id=""0"" name=""PlainSigmoid"" prob=""1"" />
  </ActivationFunctions>
  <Networks>
    <Network id=""{2}"" birthGen=""0"" fitness=""0"">
      <Nodes>
        <Node type=""bias"" id=""0"" />
{0}
      </Nodes>
      <Connections>
{1}
      </Connections>
    </Network>
  </Networks>
</Root>
";
    }
}

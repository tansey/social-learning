using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
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
        const string FEED_FORWARD_NETWORK_FILE = @"..\..\..\experiments\social_only_feedforward_network.xml";
        const string RESULTS_FILE = @"..\..\..\experiments\social_only_results.csv";
        static SocialExperiment _experiment;
        static ForagingEvaluator<NeatGenome> _evaluator;
        static FastRandom _random;

        static void Main(string[] args)
        {
            _random = new FastRandom();

            _experiment = new SocialExperiment();
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(CONFIG_FILE);
            _experiment.Initialize("SimpleEvolution", xmlConfig.DocumentElement);
            _experiment.NeatGenomeParameters.AddConnectionMutationProbability = 0;
            _experiment.NeatGenomeParameters.AddNodeMutationProbability = 0;
            _experiment.NeatGenomeParameters.DeleteConnectionMutationProbability = 0;
            
            SocialExperiment.CreateNetwork(FEED_FORWARD_NETWORK_FILE, _experiment.InputCount, 20, _experiment.OutputCount);

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
            _evaluator = new ForagingEvaluator<NeatGenome>(genomeDecoder, _experiment.World, AgentTypes.Social)
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

        static void World_Stepped(object sender, EventArgs e)
        {
            Console.WriteLine("Step {0} Best: {1} Avg: {2} Memory: {3}", 
                                _experiment.World.CurrentStep,
                                _experiment.World.Agents.Max(f => f.Fitness),
                                _experiment.World.Agents.Average(f => f.Fitness),
                                SocialAgent.DEFAULT_MEMORY_SIZE
                                );

            if (_experiment.World.CurrentStep > 0 && _experiment.World.CurrentStep % 5000 == 0)
            {
                using (TextWriter writer = new StreamWriter(RESULTS_FILE, true))
                    writer.WriteLine("{0},{1},{2}", _evaluator.CurrentTimeStep, 
                                                    _experiment.World.Agents.Max(f => f.Fitness),
                                                    _experiment.World.Agents.Average(f => f.Fitness));
                _experiment.World.Reset();
            }

            if (_evaluator.CurrentTimeStep > 0 && _evaluator.CurrentTimeStep % 50000 == 0)
                SocialAgent.DEFAULT_MEMORY_SIZE++;
        }

        
    }
}

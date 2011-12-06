using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VisualizeWorld;
using SharpNeat.Genomes.Neat;
using SharpNeat.Utility;
using System.Xml;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using System.IO;
using social_learning;

namespace QLearningExperiment
{
    public class Program
    {
        const string CONFIG_FILE = @"..\..\..\experiments\qlearning.config.xml";
        const string FEED_FORWARD_NETWORK_FILE = @"..\..\..\experiments\qlearning_network.xml";
        const string RESULTS_FILE = @"..\..\..\experiments\qlearning_results.csv";
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

            SimpleExperiment.CreateNetwork(FEED_FORWARD_NETWORK_FILE, _experiment.InputCount, 20, _experiment.OutputCount);

            // Record the changes at each step
            _experiment.World.Stepped += new social_learning.World.StepEventHandler(World_Stepped);

            // Read in the agent genome from file.
            var agentGenome = _experiment.LoadPopulation(XmlReader.Create(FEED_FORWARD_NETWORK_FILE));

            // Create genome decoder.
            IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = _experiment.CreateGenomeDecoder();

            // Create the evaluator that will handle the simulation
            _evaluator = new SimpleEvaluator<NeatGenome>(genomeDecoder, _experiment.World, AgentTypes.QLearning)
            {
                MaxTimeSteps = 50000000UL,
                BackpropEpochsPerExample = 1
            };

            using (TextWriter writer = new StreamWriter(RESULTS_FILE))
                writer.WriteLine("Step,Score");

            // Start the simulation
            _evaluator.Evaluate(agentGenome);
        }

        static void World_Stepped(object sender, EventArgs e)
        {
            var step = _evaluator.CurrentTimeStep;
            //if (step == 0)
            //{
            //    var agent = (QLearningAgent)_experiment.World.Agents.First();
            //    agent.LearningRate = 0.7;
            //    agent.DiscountFactor = 0.2;
            //}
            if (step > 0 && step % _experiment.TimeStepsPerGeneration == 0)
            {
                var agent = (QLearningAgent)_experiment.World.Agents.First();
                Console.WriteLine("Step {0} Score: {1} LearningRate: {2} DiscountFactor: {3}",
                                _experiment.World.CurrentStep,
                                _experiment.World.Agents.First().Fitness,
                                agent.LearningRate,
                                agent.DiscountFactor);

                using (TextWriter writer = new StreamWriter(RESULTS_FILE, true))
                    writer.WriteLine("{0},{1}", _evaluator.CurrentTimeStep,
                                                    _experiment.World.Agents.First().Fitness);
                _experiment.World.Reset();

                if (agent.LearningRate > 0.1)
                    agent.LearningRate = Math.Max(0.1, agent.LearningRate - 0.1);
                if (agent.DiscountFactor < 0.9)
                    agent.DiscountFactor = Math.Min(0.9, agent.DiscountFactor + 0.1);
                
            }
        }
    }
}

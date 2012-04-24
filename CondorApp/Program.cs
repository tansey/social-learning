using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using social_learning;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using System.IO;
using System.Xml;
using System.Threading;

namespace CondorApp
{
    class Program
    {
        static int numRuns;
        static SensorDictionary sensorDict;
        string _name;
        SocialExperiment _experiment;
        NeatEvolutionAlgorithm<NeatGenome> _ea;
        const int DEFAULT_MAX_GENS = 200;
        static int MaxGenerations = 200;
        string _filename;
        public bool finished = false;
        static string ROOT_DIR = @"../../../experiments/";
        int _trialNum;

        static void Main(string[] args)
        {
            ROOT_DIR = args[0];
            MaxGenerations = int.Parse(args[1]);
            int offset = int.Parse(args[2]);

            Program p = new Program(offset.ToString(), offset);
            p.RunExperiment(ROOT_DIR + "config.xml", ROOT_DIR + offset + ".csv");

            while (!p.finished)
                Thread.Sleep(1000);
        }

        public Program(string name, int trialNum)
        {
            _name = name;
            _trialNum = trialNum;
        }

        void RunExperiment(string XMLFile, string filename)
        {
            _filename = filename;
            _experiment = new SocialExperiment();

            // Write the header for the results file in CSV format.
            using (TextWriter writer = new StreamWriter(_filename))
                writer.WriteLine("Generation,Average,Best,Updates");

            using (TextWriter writer = new StreamWriter(_filename.Replace(".csv", "_diversity_after.csv")))
                writer.WriteLine("Generation,Orientation Variance,Velocity Variance");

            using (TextWriter writer = new StreamWriter(_filename.Replace(".csv", "_diversity_before.csv")))
                writer.WriteLine("Generation,Orientation Variance,Velocity Variance");

            // Load the XML configuration file
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(XMLFile);
            _experiment.Initialize("EgalitarianSocialLearning", xmlConfig.DocumentElement);
            _experiment.World.SensorLookup = sensorDict;
            _experiment.TrialId = _trialNum;

            // Create the evolution algorithm and attach the update event.
            _ea = _experiment.CreateEvolutionAlgorithm();
            _ea.UpdateScheme = new SharpNeat.Core.UpdateScheme(1);
            _ea.UpdateEvent += new EventHandler(_ea_UpdateEvent);

            _experiment.Evaluator.TrialId = _trialNum;
            _experiment.Evaluator.DiversityFile = _filename.Replace(".csv", "_diversity.csv");

            // Start algorithm (it will run on a background thread).
            _ea.StartContinue();
        }

        // Called by the EA at the end of every generation.
        // Saves the results to file and checks if we're ready to stop.
        void _ea_UpdateEvent(object sender, EventArgs e)
        {
            // If this run has already finished, don't log anything.
            // This is needed because SharpNEAT calls this an extra
            // time when the algorithm is stopped.
            if (finished)
                return;

            Console.WriteLine(_experiment.Evaluator.TeachParadigm);
            Console.WriteLine(_experiment.Evaluator.EvoParadigm);
            Console.WriteLine(_experiment.Evaluator.AgentType);

            // The average fitness of each genome.
            double averageFitness = _ea.GenomeList.Average(x => x.EvaluationInfo.Fitness);

            // The fitness of the best individual in the population.
            double topFitness = _ea.CurrentChampGenome.EvaluationInfo.Fitness;

            // The generation that just completed.
            int generation = (int)_ea.CurrentGeneration;

            // Write the progress to the console.
            Console.WriteLine("{0} Generation: {1} Best: {2} Avg: {3} Updates: {4}",
                               _name, generation, topFitness, averageFitness, _experiment.Evaluator.UpdatesThisGeneration);

            // Append the progress to the results file in CSV format.
            using (TextWriter writer = new StreamWriter(_filename, true))
                writer.WriteLine(generation + "," + averageFitness + "," + topFitness + "," + _experiment.Evaluator.UpdatesThisGeneration);


            // Stop if we've evolved for enough generations
            if (_ea.CurrentGeneration >= MaxGenerations)
            {
                _ea.Stop();
                finished = true;
                Console.WriteLine("{0} Finished!", _name);
            }
        }
    }
}

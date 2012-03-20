using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using System.IO;
using System.Threading;
using social_learning;


namespace BatchExperiment
{
    class Program
    {
        static int numRuns;
        static SensorDictionary sensorDict;
        string _name;
        SocialExperiment _experiment;
        NeatEvolutionAlgorithm<NeatGenome> _ea;
        const int DEFAULT_MAX_GENS = 500;
        const int DEFAULT_NUM_RUNS = 30;
        static int MaxGenerations = 500;
        string _filename;
        bool finished = false;
        const string ROOT_DIR = @"../../../experiments/";
        int _trialNum;

        static void Main(string[] args)
        {
            Console.Write("Number runs (default {0}): ", DEFAULT_NUM_RUNS);
            numRuns = GetCommandLineArg(DEFAULT_NUM_RUNS);

            Console.Write("Number of generations (default {0}): ", DEFAULT_MAX_GENS);
            MaxGenerations = GetCommandLineArg(DEFAULT_MAX_GENS);

            Console.Write("File offset (default 0): ", 0);
            int offset = GetCommandLineArg(0);

            Console.Write("Use simple rewards (default n)? ");
            bool simple = GetCommandLineArg(false);

            Console.Write("Use all positive rewards (default n)? ");
            bool allPositive = GetCommandLineArg(false);

            Console.Write("Run baseline? ");
            bool runBaseline = GetCommandLineArg(false);

            Console.Write("Run social darwinian? ");
            bool runSocialDarwin = GetCommandLineArg(false);

            Console.Write("Run social lamarkian? ");
            bool runSocialLamark = GetCommandLineArg(false);

            Console.Write("Run same species darwinian? ");
            bool runSameSpeciesDarwin = GetCommandLineArg(false);

            Console.Write("Run same species lamarkian? ");
            bool runSameSpeciesLamark = GetCommandLineArg(false);

            Console.Write("Run same species reward-proportional darwinian? ");
            bool runSameSpeciesRewardProportionalDarwin = GetCommandLineArg(false);

            Console.Write("Run same species reward-proportional lamarkian? ");
            bool runSameSpeciesRewardProportionalLamark = GetCommandLineArg(false);

            Console.Write("Run student/teacher darwinian? ");
            bool runStudentTeacherDarwin = GetCommandLineArg(false);

            Console.Write("Run student/teacher lamarkian? ");
            bool runStudentTeacherLamark = GetCommandLineArg(false);

            sensorDict = new SensorDictionary(100, 500, 500);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("*** Settings ***");
            Console.WriteLine("Number of runs: {0}", numRuns);
            Console.WriteLine("Number of generations: {0}", MaxGenerations);
            Console.WriteLine("Run baseline? {0}", runBaseline ? "Yes" : "No");
            Console.WriteLine("Run social darwinian? {0}", runSocialDarwin ? "Yes" : "No");
            Console.WriteLine("Run social lamarkian? {0}", runSocialLamark ? "Yes" : "No");
            Console.WriteLine("Run same-species darwinian? {0}", runSameSpeciesDarwin ? "Yes" : "No");
            Console.WriteLine("Run same-species lamarkian? {0}", runSameSpeciesLamark ? "Yes" : "No");
            Console.WriteLine("Run same-species reward-proportional darwinian? {0}", runSameSpeciesRewardProportionalDarwin ? "Yes" : "No");
            Console.WriteLine("Run same-species reward-proportional lamarkian? {0}", runSameSpeciesRewardProportionalLamark ? "Yes" : "No");
            Console.WriteLine("Run student/teacher darwinian? {0}", runStudentTeacherDarwin ? "Yes" : "No");
            Console.WriteLine("Run student/teacher lamarkian? {0}", runStudentTeacherLamark ? "Yes" : "No");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("*** Starting evolution ***");

            if(simple)
            {
                if (runBaseline)
                    RunTrials("Simple Baseline", "simple_neural.config.xml", offset);

                if (runSocialDarwin)
                    RunTrials("Simple Social Learning (Darwinian)", "simple_social_darwin.config.xml", offset);

                if (runSocialLamark)
                    RunTrials("Simple Social Learning (Lamarkian)", "simple_social_lamark.config.xml", offset);

                if (runSameSpeciesDarwin)
                    RunTrials("Simple Same Species (Darwinian)", "simple_same_species_darwin.config.xml", offset);

                if (runSameSpeciesLamark)
                    RunTrials("Simple 20 Equal Groups (Lamarkian)", "simple_same_species_lamark.config.xml", offset);

                if (runStudentTeacherDarwin)
                    RunTrials("Simple Student-Teacher (Darwinian)", "simple_student_teacher_darwin.config.xml", offset);

                if (runStudentTeacherLamark)
                    RunTrials("Simple Student-Teacher Lowest Energy (Lamarkian)", "simple_student_teacher_lamark.config.xml", offset);
            }
            else if (allPositive)
            {
                if (runBaseline)
                    RunTrials("All Positive Baseline", "positive_neural.config.xml", offset);

                if (runSocialDarwin)
                    RunTrials("All Positive Social Learning (Darwinian)", "positive_social_darwin.config.xml", offset);

                if (runSocialLamark)
                    RunTrials("All Positive Social Learning (Lamarkian)", "positive_social_lamark.config.xml", offset);

                if (runSameSpeciesDarwin)
                    RunTrials("All Positive Same Species (Darwinian)", "positive_same_species_darwin.config.xml", offset);

                if (runSameSpeciesLamark)
                    RunTrials("All Positive Same Species (Lamarkian)", "positive_same_species_lamark.config.xml", offset);

                if (runSameSpeciesRewardProportionalDarwin)
                    RunTrials("All Positive Same Species Reward Proportional (Darwinian)", "positive_same_species_reward_proportional_darwin.config.xml", offset);

                if (runSameSpeciesRewardProportionalLamark)
                    RunTrials("All Positive Same Species Reward Proportional (Lamarkian)", "positive_same_species_reward_proportional_lamark.config.xml", offset);

                if (runStudentTeacherDarwin)
                    RunTrials("All Positive Student-Teacher (Darwinian)", "positive_student_teacher_darwin.config.xml", offset);

                if (runStudentTeacherLamark)
                    RunTrials("All Positive Student-Teacher (Lamarkian)", "positive_student_teacher_lamark.config.xml", offset);
            }
            else
            {
                if (runBaseline)
                    RunTrials("Baseline", "neural.config.xml", offset);

                if (runSocialDarwin)
                    RunTrials("Social Learning (Darwinian)", "social_darwin.config.xml", offset);

                if (runSocialLamark)
                    RunTrials("Social Learning (Lamarkian)", "social_lamark.config.xml", offset);

                if (runSameSpeciesDarwin)
                    RunTrials("Same Species (Darwinian)", "same_species_darwin.config.xml", offset);

                if (runSameSpeciesLamark)
                    RunTrials("Same Species (Lamarkian)", "same_species_lamark.config.xml", offset);

                if (runStudentTeacherDarwin)
                    RunTrials("Student-Teacher (Darwinian)", "student_teacher_darwin.config.xml", offset);

                if (runStudentTeacherLamark)
                    RunTrials("Student-Teacher Lowest Energy (Lamarkian)", "student_teacher_lamark.config.xml", offset);
            }
            Console.WriteLine();
            Console.WriteLine("All runs completed!");
        }

        private static void RunTrials(string name, string config, int offset)
        {
            List<Program> trials = new List<Program>();
            Console.WriteLine("*** Starting {0} Runs ***", name);
            
            for (int i = 0; i < numRuns; i++)
                trials.Add(StartTrial(name, config, i+offset));
            
            while (!AllFinished(trials)) Thread.Sleep(1000);

            Console.WriteLine("*** Finished {0} Runs ***", name);
        }

        private static Program StartTrial(string name, string config, int trialNum)
        {
            Program p = new Program(name + "_" + trialNum, trialNum);
            p.RunExperiment(ROOT_DIR + config, ROOT_DIR + name.ToLower().Replace("(", "").Replace(")", "").Replace(' ', '_').Replace('-', '_') + "_results" + trialNum + ".csv");
            return p;
        }

        private static int GetCommandLineArg(int defaultValue)
        {
            string line = Console.ReadLine();
            int result;
            if (int.TryParse(line, out result))
                return result;
            return defaultValue;
        }

        private static bool GetCommandLineArg(bool defaultValue)
        {
            string line = Console.ReadLine();
            line = line.ToLower();
            if (line == "yes" || line == "y" || line == "true")
                return true;
            if (line == "no" || line == "n" || line == "false")
                return false;
            return defaultValue;
        }

        public Program(string name, int trialNum)
        {
            _name = name;
            _trialNum = trialNum;
        }

        static bool AllFinished(IEnumerable<Program> programs)
        {
            foreach (Program p in programs)
                if (p != null && !p.finished)
                    return false;
            return true;
        }

        void RunExperiment(string XMLFile, string filename)
        {
            _filename = filename;
            _experiment = new SocialExperiment();
            
            // Write the header for the results file in CSV format.
            using (TextWriter writer = new StreamWriter(_filename))
                writer.WriteLine("Generation,Average,Best,Updates");

            using (TextWriter writer = new StreamWriter(_filename.Replace(".csv","_diversity_after.csv")))
                writer.WriteLine("Generation,Orientation Variance,Velocity Variance");

            using (TextWriter writer = new StreamWriter(_filename.Replace(".csv", "_diversity_before.csv")))
                writer.WriteLine("Generation,Orientation Variance,Velocity Variance");

            // Load the XML configuration file
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(XMLFile);
            _experiment.Initialize("SimpleEvolution", xmlConfig.DocumentElement);
            _experiment.World.SensorLookup = sensorDict;

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

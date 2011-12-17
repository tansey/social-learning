using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using VisualizeWorld;
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
        string _name;
        SocialExperiment _experiment;
        NeatEvolutionAlgorithm<NeatGenome> _ea;
        const int MaxGenerations = 200;
        string _filename;
        bool finished = false;
        List<double>[] _averageFitness;
        List<double>[] _bestFitness;


        static void Main(string[] args)
        {
            numRuns = args.Length > 0 ? int.Parse(args[0]) : 30;
            Program[] r = new Program[numRuns * 3];
            List<double>[] neuralAvg = createAllCollection(), neuralBest = createAllCollection(),
                         socialDarwinAvg = createAllCollection(), socialDarwinBest = createAllCollection(),
                         socialLamarkAvg = createAllCollection(), socialLamarkBest = createAllCollection(); 
            SensorDictionary sensorDict = new SensorDictionary(100, 500, 500);
            Console.WriteLine("WARNING: Skipping baseline results");
            for (int i = 0; i < numRuns; i++)
            {
                Program p = new Program("Baseline_" + i, neuralAvg, neuralBest);
                //p.RunExperiment(@"..\..\..\experiments\neural.config.xml", @"..\..\..\experiments\neural_results" + i + ".csv", sensorDict);
                Program q = new Program("Social_Darwin_" + i, socialDarwinAvg, socialDarwinBest);
                //q.RunExperiment(@"..\..\..\experiments\social_darwin.config.xml", @"..\..\..\experiments\social_darwin_results" + i + ".csv", sensorDict);
                Program m = new Program("Social_Lamark_" + i, socialLamarkAvg, socialLamarkBest);
                m.RunExperiment(@"..\..\..\experiments\social_lamark.config.xml", @"..\..\..\experiments\social_lamark_results" + i + ".csv", sensorDict);
                //r[3 * i] = p;
                r[3 * i + 1] = q;
                r[3 * i + 2] = m;
            }
            while (!AllFinished(r)) Thread.Sleep(1000);

            using (TextWriter writer = new StreamWriter(@"..\..\..\experiments\average_logs.csv"))
            {
                writer.WriteLine("Generation,"
                    + "Baseline-Average,Baseline-Average-Stdev,"
                    + "Baseline-Champion,Baseline-Champion-Stdev,"
                    + "Social-Average (Darwinian),Social-Average-Stdev (Darwinian),"
                    + "Social-Champion (Darwinian),Social-Champion-Stdev (Darwinian),"
                    + "Social-Average (Lamarkian),Social-Average-Stdev (Lamarkian)," 
                    + "Social-Champion (Lamarkian),Social-Champion-Stdev (Lamarkian)");
                for (int i = 0; i < r[0]._averageFitness.Length; i++)
                    writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", i,
                        neuralAvg[i].Average(), neuralAvg[i].Stdev(), 
                        neuralBest[i].Average(), neuralBest[i].Stdev(),
                        socialDarwinAvg[i].Average(), socialDarwinAvg[i].Stdev(),
                        socialDarwinBest[i].Average(), socialDarwinBest[i].Stdev(),
                        socialLamarkAvg[i].Average(), socialLamarkAvg[i].Stdev(),
                        socialLamarkBest[i].Average(), socialLamarkBest[i].Stdev()
                        ));
            }
        }

        private static List<double>[] createAllCollection()
        {
            List<double>[] allNeural = new List<double>[MaxGenerations+1];
            for (int i = 0; i < allNeural.Length; i++)
                allNeural[i] = new List<double>();
            return allNeural;
        }

        public Program(string name, List<double>[] avgFitness, List<double>[] bestFitness)
        {
            _name = name;
            _averageFitness = avgFitness;
            _bestFitness = bestFitness;
        }

         static bool AllFinished(Program[] programs)
         {
             foreach (Program p in programs)
                 if (p != null && !p.finished)
                     return false;
             return true;
         }

         void RunExperiment(string XMLFile, string filename, SensorDictionary sensorDict)
         {
             _filename = filename;
            _experiment = new SocialExperiment();
             // Load config XML.

             using (TextWriter writer = new StreamWriter(_filename))
                 writer.WriteLine("generation,averageFitness,topFitness");

             XmlDocument xmlConfig = new XmlDocument();
             xmlConfig.Load(XMLFile);
             _experiment.Initialize("SimpleEvolution", xmlConfig.DocumentElement);
             _experiment.World.PlantLayoutStrategy = social_learning.PlantLayoutStrategies.Clustered;
             _experiment.World.dictionary = sensorDict;
             startEvolution();

             
         }

        void startEvolution()
        {

            // Create evolution algorithm and attach update event.
            _ea = _experiment.CreateEvolutionAlgorithm();
            _ea.UpdateEvent += new EventHandler(_ea_UpdateEvent);
            // Start algorithm (it will run on a background thread).
            _ea.StartContinue();
        }

        void _ea_UpdateEvent(object sender, EventArgs e)
        {
            if (finished)
                return;
            using (TextWriter writer = new StreamWriter(_filename, true))
            {
                double averageFitness = _ea.GenomeList.Average(x => x.EvaluationInfo.Fitness);
                double topFitness = _ea.CurrentChampGenome.EvaluationInfo.Fitness;
                int generation = (int)_ea.CurrentGeneration;

                Console.WriteLine("{0} Generation: {1} Best: {2} Avg: {3} MemorySize: {4}",
                                   _name, generation, topFitness, averageFitness, _experiment.Evaluator.CurrentMemorySize);
                writer.WriteLine(generation + "," + averageFitness + "," + topFitness);

                lock (_bestFitness)
                    _bestFitness[generation].Add(topFitness);
                lock (_averageFitness)
                    _averageFitness[generation].Add(averageFitness);

                if (_ea.CurrentGeneration >= MaxGenerations)
                {
                    _ea.Stop();
                    finished = true;
                    Console.WriteLine("{0} Finished!", _name);
                }
            }

            if (_ea.CurrentGeneration == 5)
            {
                Console.WriteLine("Switching to Darwinian evolution with no learning");
                _experiment.Evaluator.AgentType = AgentTypes.Neural;
            }
        }
        
    }
}

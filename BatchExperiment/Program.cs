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


namespace BatchExperiment
{
    class Program
    {
        static int numRuns;
        SimpleExperiment _experiment;
        NeatEvolutionAlgorithm<NeatGenome> _ea;
        int MaxGenerations = 20;
        string _filename;
        bool finished = false;
        List<double> _averageFitness;
        List<double> _bestFitness;


        static void Main(string[] args)
        {
            numRuns = args.Length > 0 ? int.Parse(args[0]) : 20;
            Program[] r = new Program[numRuns * 3];
            List<double> neuralAvg = new List<double>(), neuralBest = new List<double>(),
                         socialDarwinAvg = new List<double>(), socialDarwinBest = new List<double>(),
                         socialLamarkAvg = new List<double>(), socialLamarkBest = new List<double>();
            for (int i = 0; i < numRuns; i++)
            {
                Program p = new Program(neuralAvg, neuralBest);
                p.RunExperiment(@"..\..\..\experiments\neural.config.xml", @"..\..\..\experiments\neural_results" + i + ".csv");
                Program q = new Program(socialDarwinAvg, socialDarwinBest);
                q.RunExperiment(@"..\..\..\experiments\social_darwin.config.xml", @"..\..\..\experiments\social_darwin_results" + i + ".csv");
                Program m = new Program(socialLamarkAvg, socialLamarkBest);
                m.RunExperiment(@"..\..\..\experiments\social_lamark.config.xml", @"..\..\..\experiments\social_lamark_results" + i + ".csv");
                r[3 * i] = p;
                r[3 * i + 1] = q;
                r[3 * i + 2] = m;
            }
            while (!AllFinished(r)) Thread.Sleep(1000);

            using (TextWriter writer = new StreamWriter(@"..\..\..\experiments\average_logs.csv"))
            {
                writer.WriteLine("Generation,Baseline-Average,Baseline-Champion,Social-Average (Darwinian),Social-Champion (Darwinian),Social-Average (Lamarkian),Social-Champion (Lamarkian)");
                for (int i = 0; i < r[0]._averageFitness.Count; i++)
                    writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", i,
                        neuralAvg[i], neuralBest[i],
                        socialDarwinAvg[i], socialDarwinBest[i],
                        socialLamarkAvg[i], socialLamarkBest[i]));
            }
        }

        public Program(List<double> avgFitness, List<double> bestFitness)
        {
            _averageFitness = avgFitness;
            _bestFitness = bestFitness;
        }

         static bool AllFinished(Program[] programs)
         {
             foreach (Program p in programs)
                 if (!p.finished)
                     return false;
             return true;
         }

         void RunExperiment(string XMLFile, string filename)
         {
             _filename = filename;
             _experiment = new SimpleExperiment();
             // Load config XML.

             using (TextWriter writer = new StreamWriter(_filename))
             {

                 writer.WriteLine("generation,averageFitness,topFitness");
             }

             XmlDocument xmlConfig = new XmlDocument();
             xmlConfig.Load(XMLFile);
             _experiment.Initialize("SimpleEvolution", xmlConfig.DocumentElement);
             _experiment.World.PlantLayoutStrategy = social_learning.PlantLayoutStrategies.Spiral;

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
                
                lock (_bestFitness)
                {
                    if (_bestFitness.Count <= generation)
                        _bestFitness.Add(0);
                    _bestFitness[generation] += topFitness / (double)numRuns;
                }
                lock (_averageFitness)
                {
                    if (_averageFitness.Count <= generation)
                        _averageFitness.Add(0);
                    _averageFitness[generation] += averageFitness / (double)numRuns;
                }
                Console.WriteLine(generation + "," + averageFitness + "," + topFitness);
                writer.WriteLine( generation + "," +  averageFitness + "," + topFitness);
                if (_ea.CurrentGeneration >= MaxGenerations)
                {
                    _ea.Stop();
                    finished = true;
                }
            }
        }
        
    }
}

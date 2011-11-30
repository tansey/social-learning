using System.Collections.Generic;
using SharpNeat.Domains;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Decoders;
using System.Xml;
using SharpNeat.Core;
using SharpNeat.DistanceMetrics;
using SharpNeat.SpeciationStrategies;
using SharpNeat.EvolutionAlgorithms.ComplexityRegulation;
using SharpNeat.Phenomes;
using SharpNeat.Decoders.Neat;
using social_learning;
using System.Threading.Tasks;
using System;
using System.Windows.Forms;
using System.Linq;
using SharpNeat.Network;

namespace VisualizeWorld
{
    public class SimpleExperiment : INeatExperiment
    {
        NeatEvolutionAlgorithmParameters _eaParams;
        NeatGenomeParameters _neatGenomeParams;
        string _name;
        int _populationSize;
        int _specieCount;
        NetworkActivationScheme _activationScheme;
        string _complexityRegulationStr;
        int? _complexityThreshold;
        string _description;
        World _world;
        ulong _maxTimeSteps;
        AgentTypes _agentType;
        PlantLayoutStrategies _plantLayout;
        EvolutionParadigm _paradigm;

        const int PLANT_TYPES = 5;

        public int InputCount { get { return _world.PlantTypes.Count() * (World.SENSORS_PER_PLANT_TYPE) + 1 + World.SENSORS_PER_PLANT_TYPE; } }
        public int OutputCount { get { return 2; } }

        public string Description
        {
            get { return _description; }
        }

        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the default population size to use for the experiment.
        /// </summary>
        public int DefaultPopulationSize
        {
            get { return _populationSize; }
        }

        public World World
        {
            get { return _world; }
        }

        /// <summary>
        /// Gets the NeatEvolutionAlgorithmParameters to be used for the experiment. Parameters on this object can be 
        /// modified. Calls to CreateEvolutionAlgorithm() make a copy of and use this object in whatever state it is in 
        /// at the time of the call.
        /// </summary>
        public NeatEvolutionAlgorithmParameters NeatEvolutionAlgorithmParameters
        {
            get { return _eaParams; }
        }

        /// <summary>
        /// Gets the NeatGenomeParameters to be used for the experiment. Parameters on this object can be modified. Calls
        /// to CreateEvolutionAlgorithm() make a copy of and use this object in whatever state it is in at the time of the call.
        /// </summary>
        public NeatGenomeParameters NeatGenomeParameters
        {
            get { return _neatGenomeParams; }
        }

        public PlantLayoutStrategies PlantLayout { get { return _plantLayout; } set { _plantLayout = value; if (_world != null) _world.PlantLayoutStrategy = value; } }
        public EvolutionParadigm EvoParadigm { get { return _paradigm; } set { _paradigm = value; } }
        public SimpleExperiment()
        {
            
        }

        /// <summary>
        /// Initialize the experiment with some optional XML configutation data.
        /// </summary>
        public void Initialize(string name, XmlElement xmlConfig)
        {
            _name = name;
            _populationSize = XmlUtils.GetValueAsInt(xmlConfig, "PopulationSize");
            _specieCount = XmlUtils.GetValueAsInt(xmlConfig, "SpecieCount");
            _activationScheme = ExperimentUtils.CreateActivationScheme(xmlConfig, "Activation");
            _complexityRegulationStr = XmlUtils.TryGetValueAsString(xmlConfig, "ComplexityRegulationStrategy");
            _complexityThreshold = XmlUtils.TryGetValueAsInt(xmlConfig, "ComplexityThreshold");
            _description = XmlUtils.TryGetValueAsString(xmlConfig, "Description");
            _maxTimeSteps = (ulong)XmlUtils.TryGetValueAsInt(xmlConfig, "MaxTimeSteps");
            _agentType =(AgentTypes) Enum.Parse(typeof(AgentTypes), XmlUtils.TryGetValueAsString(xmlConfig, "AgentType"));
            _plantLayout = (PlantLayoutStrategies)Enum.Parse(typeof(PlantLayoutStrategies), XmlUtils.TryGetValueAsString(xmlConfig, "PlantLayout"));
            _paradigm = (EvolutionParadigm)Enum.Parse(typeof(EvolutionParadigm), XmlUtils.TryGetValueAsString(xmlConfig, "EvolutionParadigm"));
            var species = new List<PlantSpecies>();
            for (int i = 0; i < XmlUtils.TryGetValueAsInt(xmlConfig, "PlantSpecies"); i++)
                species.Add(new PlantSpecies(i) { Name = "Species_" + i, 
                                                 Radius = XmlUtils.GetValueAsInt(xmlConfig, "PlantRadius"), 
                                                 Reward = 100 });

            Random random = new Random();
            var agents = new List<IAgent>();
            const int NUM_AGENTS = 10;
            for (int i = 0; i < NUM_AGENTS; i++)
            {
                agents.Add(new SpinningAgent(i) { X = random.Next(500), Y = random.Next(500), Orientation = random.Next(360) });
            }

            _world = new World(agents, species, XmlUtils.GetValueAsInt(xmlConfig, "WorldHeight"), 
                                                XmlUtils.GetValueAsInt(xmlConfig, "WorldHeight"),
                                                XmlUtils.GetValueAsInt(xmlConfig, "PlantsPerSpecies"))
            {
                AgentHorizon = XmlUtils.GetValueAsInt(xmlConfig, "AgentHorizon"),
                PlantLayoutStrategy = _plantLayout
            };
            
            _eaParams = new NeatEvolutionAlgorithmParameters();
            _eaParams.SpecieCount = _specieCount;
            _neatGenomeParams = new NeatGenomeParameters()
            {
                ActivationFn = PlainSigmoid.__DefaultInstance,
                InitialInterconnectionsProportion = 1,
                
            };
        }

        /// <summary>
        /// Load a population of genomes from an XmlReader and returns the genomes in a new list.
        /// The genome factory for the genomes can be obtained from any one of the genomes.
        /// </summary>
        public List<NeatGenome> LoadPopulation(XmlReader xr)
        {
            NeatGenomeFactory genomeFactory = (NeatGenomeFactory)CreateGenomeFactory();
            return NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, genomeFactory);
        }

        /// <summary>
        /// Save a population of genomes to an XmlWriter.
        /// </summary>
        public void SavePopulation(XmlWriter xw, IList<NeatGenome> genomeList)
        {
            // Writing node IDs is not necessary for NEAT.
            NeatGenomeXmlIO.WriteComplete(xw, genomeList, false);
        }

        /// <summary>
        /// Create a genome factory for the experiment.
        /// Create a genome factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
        /// </summary>
        public IGenomeFactory<NeatGenome> CreateGenomeFactory()
        {
            return new NeatGenomeFactory(InputCount, OutputCount, _neatGenomeParams);
        }

        /// <summary>
        /// Create and return a NeatEvolutionAlgorithm object ready for running the NEAT algorithm/search. Various sub-parts
        /// of the algorithm are also constructed and connected up.
        /// This overload requires no parameters and uses the default population size.
        /// </summary>
        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm()
        {
            return CreateEvolutionAlgorithm(DefaultPopulationSize);
        }

        /// <summary>
        /// Create and return a NeatEvolutionAlgorithm object ready for running the NEAT algorithm/search. Various sub-parts
        /// of the algorithm are also constructed and connected up.
        /// This overload accepts a population size parameter that specifies how many genomes to create in an initial randomly
        /// generated population.
        /// </summary>
        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(int populationSize)
        {
            // Create a genome2 factory with our neat genome parameters object and the appropriate number of input and output neuron genes.
            IGenomeFactory<NeatGenome> genomeFactory = CreateGenomeFactory();

            // Create an initial population of randomly generated genomes.
            List<NeatGenome> genomeList = genomeFactory.CreateGenomeList(populationSize, 0);

            // Create evolution algorithm.
            return CreateEvolutionAlgorithm(genomeFactory, genomeList);
        }

        /// <summary>
        /// Create and return a NeatEvolutionAlgorithm object ready for running the NEAT algorithm/search. Various sub-parts
        /// of the algorithm are also constructed and connected up.
        /// This overload accepts a pre-built genome2 population and their associated/parent genome2 factory.
        /// </summary>
        public NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(IGenomeFactory<NeatGenome> genomeFactory, List<NeatGenome> genomeList)
        {
            // Create distance metric. Mismatched genes have a fixed distance of 10; for matched genes the distance is their weigth difference.
            IDistanceMetric distanceMetric = new ManhattanDistanceMetric(1.0, 0.0, 10.0);
            ISpeciationStrategy<NeatGenome> speciationStrategy = new ParallelKMeansClusteringStrategy<NeatGenome>(distanceMetric, new ParallelOptions());

            // Create complexity regulation strategy.
            IComplexityRegulationStrategy complexityRegulationStrategy = ExperimentUtils.CreateComplexityRegulationStrategy(_complexityRegulationStr, _complexityThreshold);

            // Create the evolution algorithm.
            NeatEvolutionAlgorithm<NeatGenome> ea = new NeatEvolutionAlgorithm<NeatGenome>(_eaParams, speciationStrategy, complexityRegulationStrategy);

            // Create genome decoder.
            IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = CreateGenomeDecoder();

            // Create a genome list evaluator. This packages up the genome decoder with the phenome evaluator.
            IGenomeListEvaluator<NeatGenome> genomeListEvaluator = new SimpleEvaluator<NeatGenome>(genomeDecoder, _world)
            {
                MaxTimeSteps = _maxTimeSteps,
                AgentType = _agentType,
                EvoParadigm = _paradigm
            };
            
            // Initialize the evolution algorithm.
            ea.Initialize(genomeListEvaluator, genomeFactory, genomeList);

            // Finished. Return the evolution algorithm
            return ea;
        }

        /// <summary>
        /// Creates a new genome decoder that can be used to convert a genome into a phenome.
        /// </summary>
        public IGenomeDecoder<NeatGenome, IBlackBox> CreateGenomeDecoder()
        {
            return new NeatGenomeDecoder(_activationScheme);
        }
    }
}

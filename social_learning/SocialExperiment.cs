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
using System.Threading.Tasks;
using System;
using System.Linq;
using SharpNeat.Network;
using System.Text;
using System.IO;
using System.Diagnostics;
using SharpNeat.Utility;

namespace social_learning
{
    public class SocialExperiment : INeatExperiment
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
        ulong _timeStepsPerGeneration;
        AgentTypes _agentType;
        PlantLayoutStrategies _plantLayout;
        EvolutionParadigm _paradigm;
        MemoryParadigm _memory;
        int _memGens;
        int _maxMemorySize;
        TeachingParadigm _teaching;
        ForagingEvaluator<NeatGenome> _evaluator;
        static FastRandom _random = new FastRandom();
        int _outputs;
        int _inputs;
        bool _logDiversity;
        int _stepReward;

        const int PLANT_TYPES = 5;

        public int InputCount { get { return _inputs; } }
        public int OutputCount { get { return _outputs; } }

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
        /// modified. Calls to CreateEvolutionAlgorithm() make a copy of and use this object in whatever stateActionPair it is in 
        /// at the time of the call.
        /// </summary>
        public NeatEvolutionAlgorithmParameters NeatEvolutionAlgorithmParameters
        {
            get { return _eaParams; }
        }

        /// <summary>
        /// Gets the NeatGenomeParameters to be used for the experiment. Parameters on this object can be modified. Calls
        /// to CreateEvolutionAlgorithm() make a copy of and use this object in whatever stateActionPair it is in at the time of the call.
        /// </summary>
        public NeatGenomeParameters NeatGenomeParameters
        {
            get { return _neatGenomeParams; }
        }

        public PlantLayoutStrategies PlantLayout { get { return _plantLayout; } set { _plantLayout = value; if (_world != null) _world.PlantLayoutStrategy = value; } }
        public EvolutionParadigm EvoParadigm { get { return _paradigm; } set { _paradigm = value; } }
        public MemoryParadigm MemParadigm { get { return _memory; }  set { _memory = value; } }
        public int MemoryGenerations { get { return _memGens; } set { _memGens = value; } }
        public int MaxMemorySize { get { return _maxMemorySize; } set { _maxMemorySize = value; } }
        public TeachingParadigm TeachParadigm { get; set; }
        public ForagingEvaluator<NeatGenome> Evaluator { get { return _evaluator; } set { _evaluator = value; } }
        public ulong TimeStepsPerGeneration { get { return _timeStepsPerGeneration; } set { _timeStepsPerGeneration = value; } }
        public int TrialId { get; set; }
        
        public SocialExperiment()
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
            _timeStepsPerGeneration = (ulong)XmlUtils.GetValueAsInt(xmlConfig, "TimeStepsPerGeneration");
            _stepReward = XmlUtils.GetValueAsInt(xmlConfig, "StepReward");
            _agentType =(AgentTypes) Enum.Parse(typeof(AgentTypes), XmlUtils.TryGetValueAsString(xmlConfig, "AgentType"));
            _plantLayout = (PlantLayoutStrategies)Enum.Parse(typeof(PlantLayoutStrategies), XmlUtils.TryGetValueAsString(xmlConfig, "PlantLayout"));
            _paradigm = (EvolutionParadigm)Enum.Parse(typeof(EvolutionParadigm), XmlUtils.TryGetValueAsString(xmlConfig, "EvolutionParadigm"));
            bool? diverse = XmlUtils.TryGetValueAsBool(xmlConfig, "LogDiversity");
            if (diverse.HasValue && diverse.Value)
                _logDiversity = true;
            if (_agentType == AgentTypes.Social)
            {
                var memSection = xmlConfig.GetElementsByTagName("Memory")[0] as XmlElement;
                _memory = (MemoryParadigm)Enum.Parse(typeof(MemoryParadigm), XmlUtils.TryGetValueAsString(memSection, "Paradigm"));
                SocialAgent.DEFAULT_MEMORY_SIZE = XmlUtils.GetValueAsInt(memSection, "Size");
                if (_memory == MemoryParadigm.IncrementalGrowth)
                {
                    _memGens = XmlUtils.GetValueAsInt(memSection, "GrowthGenerations");
                    _maxMemorySize = XmlUtils.GetValueAsInt(memSection, "MaxSize");
                }
                _teaching = (TeachingParadigm)Enum.Parse(typeof(TeachingParadigm), XmlUtils.TryGetValueAsString(xmlConfig, "TeachingParadigm"));
				if(_teaching == null)
					throw new Exception("Failed to read file");
            }
            var species = new List<PlantSpecies>();

            var plants = xmlConfig.GetElementsByTagName("Plant");
            for (int i = 0; i < plants.Count; i++)
            {
                var plant = plants[i] as XmlElement;
                species.Add(new PlantSpecies(i)
                {
                    Name = XmlUtils.GetValueAsString(plant, "Name"),
                    Radius = XmlUtils.GetValueAsInt(plant, "Radius"),
                    Reward = XmlUtils.GetValueAsInt(plant, "Reward"),
                    Count = XmlUtils.GetValueAsInt(plant, "Count")
                });
            }
           
            Random random = new Random();
            var agents = new List<ForagingAgent>();
            const int NUM_AGENTS = 10;
            for (int i = 0; i < NUM_AGENTS; i++)
            {
                agents.Add(new SpinningAgent(i) { X = random.Next(500), Y = random.Next(500), Orientation = random.Next(360) });
            }

            List<Predator> predators = new List<Predator>();
            int preds = XmlUtils.GetValueAsInt(xmlConfig, "Predators");
            for (int i = 1; i <= preds; i++)
            {
                predators.Add(new Predator(_populationSize * 2, i));
            }

            _world = new World(agents, XmlUtils.GetValueAsInt(xmlConfig, "WorldHeight"), XmlUtils.GetValueAsInt(xmlConfig, "WorldHeight"), species, predators)
            {
                AgentHorizon = XmlUtils.GetValueAsInt(xmlConfig, "AgentHorizon"),
                PlantLayoutStrategy = _plantLayout,
                StepReward = _stepReward
            };

            var outputs = XmlUtils.TryGetValueAsInt(xmlConfig, "Outputs");
            _outputs = outputs.HasValue ? outputs.Value : 2;
            var inputs = XmlUtils.TryGetValueAsInt(xmlConfig, "Inputs");
            _inputs = inputs.HasValue ? inputs.Value : _world.PlantTypes.Count() * World.SENSORS_PER_OBJECT_TYPE + _world.Predators.Count() * World.SENSORS_PER_OBJECT_TYPE + 1;

            _eaParams = new NeatEvolutionAlgorithmParameters();
            _eaParams.SpecieCount = _specieCount;
            _neatGenomeParams = new NeatGenomeParameters()
            {
                ActivationFn = PlainSigmoid.__DefaultInstance
            };
            if (_teaching != TeachingParadigm.EgalitarianEvolvedAcceptability)
                _neatGenomeParams.InitialInterconnectionsProportion = 1;
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
            IComplexityRegulationStrategy complexityRegulationStrategy = new NullComplexityRegulationStrategy();// ExperimentUtils.CreateComplexityRegulationStrategy(_complexityRegulationStr, _complexityThreshold);

            // Create the evolution algorithm.
            NeatEvolutionAlgorithm<NeatGenome> ea = new NeatEvolutionAlgorithm<NeatGenome>(_eaParams, speciationStrategy, complexityRegulationStrategy);

            // Create genome decoder.
            IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = CreateGenomeDecoder();

            // Create a genome list evaluator. This packages up the genome decoder with the phenome evaluator.
            _evaluator = new ForagingEvaluator<NeatGenome>(genomeDecoder, _world)
            {
                MaxTimeSteps = _timeStepsPerGeneration,
                AgentType = _agentType,
                EvoParadigm = _paradigm,
                MemParadigm = _memory,
                GenerationsPerMemorySize = _memGens,
                MaxMemorySize = _maxMemorySize,
                TeachParadigm = _teaching,
                TrialId = TrialId
            };
            
            // Initialize the evolution algorithm.
            ea.Initialize(_evaluator, genomeFactory, genomeList);

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

        public static void CreateNetwork(string filename, params int[] layers)
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

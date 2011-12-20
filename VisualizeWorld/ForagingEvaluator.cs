using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Core;
using social_learning;
using SharpNeat.Phenomes;
using SharpNeat.Phenomes.NeuralNets;
using System.Windows.Forms;
using System.IO;
using SharpNeat.Genomes.Neat;
using System.Diagnostics;
using System.Threading;
using SharpNeat.Utility;
using System.Threading.Tasks;

namespace VisualizeWorld
{
    public class ForagingEvaluator<TGenome> : IGenomeListEvaluator<TGenome>
        where TGenome : SharpNeat.Genomes.Neat.NeatGenome, global::SharpNeat.Core.IGenome<TGenome>
    {
        readonly IGenomeDecoder<TGenome, IBlackBox> _genomeDecoder;
        private ulong _evaluationCount;
        private World _world;
        private IAgent[] _agents;
        private IList<TGenome> _genomeList;
        private int _generations;
        private HashSet<IAgent> _teachers;
        private IList<IAgent> _students;
        private FastRandom _random;

        public AgentTypes AgentType { get; set; }

        /// <summary>
        /// Construct with the provided IGenomeDecoder and ICoevolutionPhenomeEvaluator. 
        /// The number of parallel threads defaults to Environment.ProcessorCount.
        /// </summary>
        public ForagingEvaluator(IGenomeDecoder<TGenome,IBlackBox> genomeDecoder, World environment, AgentTypes agent_type = AgentTypes.Spinning)
        {
            AgentType = agent_type;
            _genomeDecoder = genomeDecoder;
            _world = environment;
            _world.PlantEaten += new World.PlantEatenHandler(_world_PlantEaten);
            _world.Stepped += new World.StepEventHandler(_world_Stepped);
            BackpropEpochsPerExample = 1;
            MemParadigm = MemoryParadigm.Fixed;
            CurrentMemorySize = 1;
            GenerationsPerMemorySize = 20;
            _random = new FastRandom();
        }

        public int BackpropEpochsPerExample { get; set; }

        /// <summary>
        /// Gets the total number of individual genome evaluations that have been performed by this evaluator.
        /// </summary>
        public ulong EvaluationCount
        {
            get { return _evaluationCount; }
        }

        /// <summary>
        /// Gets a value indicating whether some goal fitness has been achieved and that
        /// the the evolutionary algorithm/search should stop. This property's value can remain false
        /// to allow the algorithm to run indefinitely.
        /// </summary>
        public bool StopConditionSatisfied
        {
            get { return false; }
        }

        /// <summary>
        /// The maximum number of time steps per generational evaluation.
        /// </summary>
        public ulong MaxTimeSteps { get; set; }

        /// <summary>
        /// The current time step that the simulation is currently at.
        /// </summary>
        public ulong CurrentTimeStep { get; set; }

        /// <summary>
        /// Reset the internal stateActionPair of the evaluation scheme if any exists.
        /// </summary>
        public void Reset()
        {
            _world.Reset();
        }

        /// <summary>
        /// The evaluation environment.
        /// </summary>
        public World Environment { get { return _world; } }

        public EvolutionParadigm EvoParadigm { get; set; }
        public MemoryParadigm MemParadigm { get; set; }
        public int GenerationsPerMemorySize { get; set; }
        public int CurrentMemorySize { get; set; }
        public int MaxMemorySize { get; set; }
        public TeachingParadigm TeachParadigm { get; set; }

        /// <summary>
        /// Main genome evaluation loop with no phenome caching (decode on each evaluation).
        /// </summary>
        public void Evaluate(IList<TGenome> genomeList)
        {
            _genomeList = genomeList;
            _agents = new IAgent[genomeList.Count];
            if (TeachParadigm == TeachingParadigm.StudentTeacherActions)
            {
                _teachers = new HashSet<IAgent>();
                _students = new List<IAgent>();
            }
            for(int i = 0; i < _agents.Length; i++)
            {
                // Decode the genome.
                IBlackBox phenome = _genomeDecoder.Decode(genomeList[i]);

                // Check that the genome is valid.
                if (phenome == null)
                {
                    Console.WriteLine("Couldn't decode genome {0}!", i);
                    _agents[i] = new SpinningAgent(i);
                }
                else
                    switch (AgentType)
                    {
                        case AgentTypes.Neural:
                            _agents[i] = new NeuralAgent(i, phenome);
                            break;
                        case AgentTypes.Social:
                            _agents[i] = new SocialAgent(i, phenome)
                            {
                                MemorySize = CurrentMemorySize
                            };
                            var network = (FastCyclicNetwork)phenome;
                            network.Momentum = ((SocialAgent)_agents[i]).Momentum;
                            network.BackpropLearningRate = ((SocialAgent)_agents[i]).LearningRate;

                            // In the Student/Teacher paradigm, each agent either teachers or learns.
                            // We assume that the survivors from the previous generation act as teachers
                            // and children are students.
                            if (TeachParadigm == TeachingParadigm.StudentTeacherActions)
                            {
                                if (_genomeList[i].BirthGeneration != _generations)
                                    _teachers.Add(_agents[i]);
                                else
                                    _students.Add(_agents[i]);
                            }
                            break;
                        case AgentTypes.QLearning:
                            _agents[i] = new QLearningAgent(i, phenome, 8, 4, _world);
                            break;
                        case AgentTypes.Spinning:
                            _agents[i] = new SpinningAgent(i);
                            break;
                        default:
                            break;
                    }
            }

            _world.Agents = _agents;
            _world.Reset();

            for (CurrentTimeStep = 0; CurrentTimeStep < MaxTimeSteps; CurrentTimeStep++)
            {
                // Move the world forward one step
                _world.Step();
            }

            // Set the fitness of each genome to the fitness its phenome earned in the world.
            for(int i = 0; i < _agents.Length; i++)
            {
                // NEAT requires fitness to be >= 0, so if the agent had negative fitness, we cap it at 0.
                genomeList[i].EvaluationInfo.SetFitness(Math.Max(0, _agents[i].Fitness));

                // This alternate fitness is purely for logging purposes, so we use the actual fitness
                genomeList[i].EvaluationInfo.AlternativeFitness = _agents[i].Fitness;
            }

            _evaluationCount += (ulong)_agents.Length;
            _generations++;
            _world.Reset();

            // Lamarkian Evolution
            if (EvoParadigm == EvolutionParadigm.Lamarkian)
                for (int i = 0; i < _agents.Length; i++)
                {
                    var agent = _agents[i];

                    // Get the network for this agent
                    var network = ((FastCyclicNetwork)((NeuralAgent)agent).Brain);

                    // Get the genome for this agent
                    var genome = (NeatGenome)genomeList[i];

                    // Update the genome to match the phenome weights
                    foreach (var conn in network.ConnectionArray)
                    {
                        var genomeConn = (ConnectionGene)genome.ConnectionList.First(g => g.SourceNodeId == genome.NodeList[conn._srcNeuronIdx].Id && g.TargetNodeId == genome.NodeList[conn._tgtNeuronIdx].Id);
                        genomeConn.Weight = conn._weight;
                    }
                }

            // If enabled and it's time, grow the size of the agents' memory window.
            if (MemParadigm == MemoryParadigm.IncrementalGrowth 
                && _generations % GenerationsPerMemorySize == 0
                && CurrentMemorySize < MaxMemorySize)
                CurrentMemorySize++;
        }

        // Handle the reward-based teaching paradigms whenever a plant is eaten.
        void _world_PlantEaten(object sender, IAgent eater, Plant eaten)
        {
            // if we're not dealing with a social agent, then skip this notification.
            if (!(eater is SocialAgent))
                return;

            // Only learn from rewards if we're using a reward-based social learning paradigm
            if (TeachParadigm != TeachingParadigm.EveryoneRewards && TeachParadigm != TeachingParadigm.SameSpeciesRewards)
                return;

            // Only learn from positive rewards.
            if (eaten.Species.Reward > 0)
            {
                // Train all the agents in parallel
                Parallel.For(0, _agents.Length, i =>
                {
                    var agent = _agents[i];

                    // Do not try to teach yourself
                    if (agent == eater)
                        return;//continue;

                    // Only update individuals in your species
                    if (TeachParadigm == TeachingParadigm.SameSpeciesRewards
                        && _genomeList[i].SpecieIdx != _genomeList[eater.Id].SpecieIdx)
                        return;//continue;

                    // Teach the agent to act like the eater
                    TeachAgent(eater, agent);
                });
            }
        }

        // Handle the Student/Teacher teaching paradigm at each step.
        void _world_Stepped(object sender, EventArgs e)
        {
            // Only learn from every step if we're using a student/teacher paradigm
            if (TeachParadigm != TeachingParadigm.StudentTeacherActions)
                return;

            // Probabilistically teach and probabilistically learn.
            foreach (var teacher in _teachers)
            {
                if (_random.NextDouble() > 0.2)
                    continue;

                Parallel.ForEach(_students, student =>
                {
                    if (_random.NextDouble() < 0.2)
                        TeachAgent(teacher, student);
                });
            }
        }

        private void TeachAgent(IAgent teacher, IAgent student)
        {
            // Get the trajectory to learn from
            var memory = ((SocialAgent)teacher).Memory;

            // Get the neural network controlling this agent
            var network = ((FastCyclicNetwork)((NeuralAgent)student).Brain);

            // Perform a fixed number of backprop epochs to train this agent
            for (int iteration = 0; iteration < BackpropEpochsPerExample; iteration++)
                foreach (var example in memory)
                    network.Train(example.Inputs, example.Outputs);
        }

    }
}

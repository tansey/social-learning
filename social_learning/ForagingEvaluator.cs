using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using SharpNeat.Phenomes.NeuralNets;
using System.IO;
using SharpNeat.Genomes.Neat;
using System.Diagnostics;
using System.Threading;
using SharpNeat.Utility;
using System.Threading.Tasks;

namespace social_learning
{
    public class ForagingEvaluator<TGenome> : IGenomeListEvaluator<TGenome>
        where TGenome : NeatGenome, global::SharpNeat.Core.IGenome<TGenome>
    {
        readonly IGenomeDecoder<TGenome, IBlackBox> _genomeDecoder;
        private ulong _evaluationCount;
        private World _world;
        private IAgent[] _agents;
        private IList<NeatGenome> _genomeList;
        private int _generations;
        private FastRandom _random;
        private int _minReward;
        private List<int> _rewards;
        private double _rewardThreshold;
        private int[] _agentGroups;

        public AgentTypes AgentType { get; set; }
        public int TrialId { get; set; }
        public string DiversityFile { get; set; }

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
            _world.PlantEaten += new World.PlantEatenHandler(addRewardToStatisticsList);
            _world.Stepped += new World.StepEventHandler(updateRewardStatistics);
            BackpropEpochsPerExample = 1;
            MemParadigm = MemoryParadigm.Fixed;
            CurrentMemorySize = 1;
            GenerationsPerMemorySize = 20;
            _random = new FastRandom();
        }

        void addRewardToStatisticsList(object sender, IAgent eater, Plant eaten)
        {
            if (TeachParadigm != TeachingParadigm.SameSpeciesRewardFiltering)
                return;
            _rewards.Add(eaten.Species.Reward);
        }


        void updateRewardStatistics(object sender, EventArgs e)
        {
            if (TeachParadigm != TeachingParadigm.SameSpeciesRewardFiltering)
                return;

            if (_rewards.Count == 0)
                return;

            _rewardThreshold = _rewards.Average() + _rewards.Stdev();
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
        public int UpdatesThisGeneration { get; set; }


        /// <summary>
        /// Main genome evaluation loop with no phenome caching (decode on each evaluation).
        /// </summary>
        public void Evaluate(IList<TGenome> genomeList)
        {
            //var groups = genomeList.GroupBy(g => g.SpecieIdx);
            //foreach (var g in groups)
            //    Console.WriteLine("{0}: {1}", g.First().SpecieIdx, g.Count());
            //var champSpeci = genomeList.First(t => t.EvaluationInfo.Fitness == genomeList.Max(b => b.EvaluationInfo.Fitness));
            //Console.WriteLine("Best: {0} -> {1}", champSpeci.Id, champSpeci.SpecieIdx);
            //Console.WriteLine("Biggest: {0}", groups.First(x => x.Count() == groups.Max(y => y.Count())).First().SpecieIdx);
            _genomeList = (IList<NeatGenome>)genomeList;
            _agents = new IAgent[genomeList.Count];
            UpdatesThisGeneration = 0;
            if (TeachParadigm == TeachingParadigm.SameSpeciesRewardProportional)
            {
                _minReward = _world.PlantTypes.Min(p => p.Reward);
            }
            else if (TeachParadigm == TeachingParadigm.SameSpeciesRewardFiltering)
            {
                _rewards = new List<int>();
                _rewardThreshold = 0;
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
                            _agents[i] = new NeuralAgent(i, _genomeList[i].SpecieIdx, phenome);
                            break;
                        case AgentTypes.Social:
                            _agents[i] = new SocialAgent(i, _genomeList[i].SpecieIdx, phenome)
                            {
                                MemorySize = CurrentMemorySize
                            };
                            var network = (FastCyclicNetwork)phenome;
                            network.Momentum = ((SocialAgent)_agents[i]).Momentum;
                            network.BackpropLearningRate = ((SocialAgent)_agents[i]).LearningRate;
                            break;
                        case AgentTypes.QLearning:
                            _agents[i] = new QLearningAgent(i, _genomeList[i].SpecieIdx, phenome, 8, 4, _world);
                            break;
                        case AgentTypes.Spinning:
                            _agents[i] = new SpinningAgent(i);
                            break;
                        default:
                            break;
                    }
            }

            #region Create random groups of proportional size
            _agentGroups = new int[_agents.Length];
            //var allSpecies = _agents.GroupBy(g => ((NeuralAgent)g).SpeciesId);
            List<int> agentIds = new List<int>();
            for (int i = 0; i < _agentGroups.Length; i++)
            {
                _agentGroups[i] = -1;
                agentIds.Add(i);
            }
            
            const int NUM_GROUPS = 10;
            for (int i = 0; i < _agentGroups.Length; i++)
			{
				int temp = _random.Next(agentIds.Count);
				int idx = agentIds[temp];
				_agentGroups[idx] = i % NUM_GROUPS;
				agentIds.RemoveAt(temp);
			}
			if(_agentGroups.GroupBy(g => g).Min(ag => ag.Count()) < 10)
				throw new Exception("Improper initialization");
			if(_agentGroups.GroupBy(g => g).Count() < 10)
				throw new Exception("Improper initialization");
            #endregion

            // Analyze diversity before
            if (_generations > 0)
            {
                learningEnabled = false;
                DiversityAnalyzer analyser = new DiversityAnalyzer(_world);
                double[][] readings = analyser.getSensorReadings();
                using (TextWriter writer = new StreamWriter(DiversityFile.Replace(".csv", "_before.csv"), true))
                {
                    List<double> orientationVariances = new List<double>();
                    List<double> velocityVariances = new List<double>();
                    foreach (double[] reading in readings)
                    {
                        var variances = analyser.getResponseVariance(reading);
                        orientationVariances.Add(variances[0]);
                        velocityVariances.Add(variances[1]);
                    }
                    writer.WriteLine("{0},{1},{2}", _generations, orientationVariances.Average(), velocityVariances.Average());
                }
                _world.Reset();
                learningEnabled = true;
            }

            if (TeachParadigm == TeachingParadigm.GenerationalChampionOfflineTraining)
                trainPopulationUsingGenerationalChampion();


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
                _genomeList[i].EvaluationInfo.SetFitness(Math.Max(0, _agents[i].Fitness));

                // This alternate fitness is purely for logging purposes, so we use the actual fitness
                _genomeList[i].EvaluationInfo.AlternativeFitness = _agents[i].Fitness;
            }

            // Analyze diversity after
            if (_generations > 0)
            {
                learningEnabled = false;
                DiversityAnalyzer analyser = new DiversityAnalyzer(_world);
                double[][] readings = analyser.getSensorReadings();
                using (TextWriter writer = new StreamWriter(DiversityFile.Replace(".csv", "_after.csv"), true))
                {
                    List<double> orientationVariances = new List<double>();
                    List<double> velocityVariances = new List<double>();
                    foreach (double[] reading in readings)
                    {
                        var variances = analyser.getResponseVariance(reading);
                        orientationVariances.Add(variances[0]);
                        velocityVariances.Add(variances[1]);
                    }
                    writer.WriteLine("{0},{1},{2}", _generations, orientationVariances.Average(), velocityVariances.Average());
                }
                _world.Reset();
                learningEnabled = true;
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

        void trainPopulationUsingGenerationalChampion()
        {
            if (TeachParadigm != TeachingParadigm.GenerationalChampionOfflineTraining)
                return;

            if (_generations == 0)
                return;

            var teacherIdx = getTeacherIndexes();
            var teachers = new List<IAgent>();
            foreach(var i in teacherIdx)
                teachers.Add(_agents[i]);
            
            var updateFn = new World.StepEventHandler(generationalChampionTrain);
            _world.Stepped += updateFn;
            _world.Agents = teachers;
            _world.Reset();

            for (int trainingTimestep = 0; trainingTimestep < 200; trainingTimestep++)
                _world.Step();

            _world.Stepped -= updateFn;
        }

        void generationalChampionTrain(object sender, EventArgs e)
        {
            foreach (var student in _agents)
            {
                if (_world.Agents.Contains(student))
                    continue;

                var teacher = _world.Agents.ElementAt(_random.NextInt() % _world.Agents.Count());

                TeachAgent(teacher, student, 0.05);
            }
        }

        const int NUM_TEACHERS = 1;
        List<int> getTeacherIndexes()
        {
            List<int> champIndexes = new List<int>();
            for (int i = 0; i < NUM_TEACHERS; i++)
                champIndexes.Add(i);

            for (int i = NUM_TEACHERS; i < _genomeList.Count; i++)
                if (_genomeList[i].EvaluationInfo.Fitness > champIndexes.Max(t => _genomeList[t].EvaluationInfo.Fitness))
                {
                    champIndexes.Add(i);
                    champIndexes.RemoveAt(argMin(champIndexes));
                }
            return champIndexes;
        }

        private int argMin(List<int> champs)
        {
            int min = 0;
            for (int i = 1; i < champs.Count; i++)
            {
                if (_genomeList[i].EvaluationInfo.Fitness < _genomeList[min].EvaluationInfo.Fitness)
                    min = i;
            }
            return min;
        }

        private void TeachAgent(IAgent teacher, IAgent student, double gaussianNoiseStdev)
        {
            UpdatesThisGeneration++;

            // Get the trajectory to learn from
            var memory = ((SocialAgent)teacher).Memory;

            // Get the neural network controlling this agent
            var network = ((FastCyclicNetwork)((NeuralAgent)student).Brain);

            // Perform a fixed number of backprop epochs to train this agent
            for (int iteration = 0; iteration < BackpropEpochsPerExample; iteration++)
                foreach (var example in memory)
                {
                    double[] outputs = new double[example.Outputs.Length];
                    example.Outputs.CopyTo(outputs, 0);
                    for (int i = 0; i < outputs.Length; i++)
                        outputs[i] = clamp(gaussianMutation(outputs[i], gaussianNoiseStdev), 0, 1);
                    network.Train(example.Inputs, outputs);
                }
        }

        private double gaussianMutation(double mean, double stddev)
        {
            double x1 = 1 - _random.NextDouble();
            double x2 = 1 - _random.NextDouble();

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return y1 * stddev + mean;
        }

        private double clamp(double val, double min, double max)
        {
            if (val >= max)
                return max;
            if (val <= min)
                return min;
            return val;
        }

        bool learningEnabled = true;
        // Handle the reward-based teaching paradigms whenever a plant is eaten.
        void _world_PlantEaten(object sender, IAgent eater, Plant eaten)
        {
            if (!learningEnabled)
                return;
            // if we're not dealing with a social agent, then skip this notification.
            if (!(eater is SocialAgent))
                return;

            // Only learn from rewards if we're using a reward-based social learning paradigm
            if (TeachParadigm != TeachingParadigm.EveryoneRewards 
                && TeachParadigm != TeachingParadigm.SameSpeciesRewards
                && TeachParadigm != TeachingParadigm.SameSpeciesRewardProportional
                && TeachParadigm != TeachingParadigm.SameSpeciesRewardFiltering)
                return;

            // Only learn from positive rewards.
            if (eaten.Species.Reward > 0)
            {
                // Train all the agents in parallel
                //Parallel.For(0, _agents.Length, i =>
				for(int i = 0; i < _agents.Length; i++)
                {
                    var agent = _agents[i];

                    // Do not try to teach yourself
                    if (agent == eater)
                        //return;
					    continue;
                    
                    // Only update individuals in your species
                    if ((TeachParadigm == TeachingParadigm.SameSpeciesRewards
                        || TeachParadigm == TeachingParadigm.SameSpeciesRewardProportional
                        || TeachParadigm == TeachingParadigm.SameSpeciesRewardFiltering)
                        && _agentGroups[eater.Id] != _agentGroups[i])// ******* TEMPORARY *******
                        //&& _genomeList[i].SpecieIdx != _genomeList[eater.Id].SpecieIdx)
                        //return;
						continue;
                    

                    // Only learn from high-valued actions
                    if (TeachParadigm == TeachingParadigm.SameSpeciesRewardFiltering
                        && eaten.Species.Reward < _rewardThreshold
                        && _rewards.Count > 20)
                        //return;
						continue;

                    // Only learn from better agens
                    //if (_agents[i].Fitness > _agents[eater.Id].Fitness)
                    //    return;


                    // Teach the agent to act like the eater
                    TeachAgent(eater, agent);

                    // If we're using reward-proportional updating, update the appropriate number of times
                    if (TeachParadigm == TeachingParadigm.SameSpeciesRewardProportional)
                    {
                        int updates = Math.Min(20, eaten.Species.Reward / _minReward);
                        for (int update = 1; update < updates; update++)
                            TeachAgent(eater, agent);
                    }
                //});
				}
            }
        }

        // Handle the Student/Teacher teaching paradigm at each step.
        void _world_Stepped(object sender, EventArgs e)
        {
            // Only learn from every step if we're using a student/teacher paradigm
            if (TeachParadigm != TeachingParadigm.SpeciesChampionOnlineTraining)
                return;

            //if (_world.CurrentStep > 500)
            //    return;

            var allSpecies = _agents.GroupBy(g => _agentGroups[g.Id]);
            foreach (var species in allSpecies)
            {
                var ordered = species.OrderByDescending(a => a.Fitness);
                var best = ordered.First();
                var worst = ordered.Last();

                TeachAgent(best, worst);
            }
            
            // Mimic the Eiben paper with probabilistic, distance-based teaching
            //const double MAX_DISTANCE = 100;
            //foreach (var agent in _agents)
            //{
            //    if (_random.NextDouble() > 0.2)
            //        continue;
            //    var student = _agents.Where(a => 
            //        Math.Sqrt((a.X - agent.X) * (a.X - agent.X) + (a.Y - agent.Y) * (a.Y - agent.Y)) <= MAX_DISTANCE)
            //        .OrderBy(a => a.Fitness)
            //        .First();
            //    TeachAgent(agent, student);
            //}

            //var orderedAgents = _agents.OrderByDescending(a => a.Fitness);

            //// Take the top 10 agents and label them as teachers
            //var teachers = orderedAgents.Take(1);
            
            //// Take the remaining agents and label them as students
            //var students = orderedAgents.Skip(1);

            //// Students probabilistically learn from teachers
            //foreach (var teacher in teachers)
            //{
            //    if (teacher.Fitness <= 0)
            //        continue;

            //    Parallel.ForEach(students, student =>
            //    {
            //        TeachAgent(teacher, student);
            //    });
            //}

            // Probabilistically teach and probabilistically learn.
            //foreach (var teacher in _teachers)
            //{
            //    //if (_random.NextDouble() > 0.2)
            //        //continue;

            //    Parallel.ForEach(_students, student =>
            //    {
            //        lock(_random)
            //            if (_random.NextDouble() < 0.2)
            //                TeachAgent(teacher, student);
            //    });
            //}
        }

        private void TeachAgent(IAgent teacher, IAgent student)
        {
            UpdatesThisGeneration++;

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

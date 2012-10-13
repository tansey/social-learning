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
using SharpNeat.Network;
using System.Xml;
using social_learning.Acceptability;

namespace social_learning
{
    public class ForagingEvaluator<TGenome> : IGenomeListEvaluator<TGenome>
        where TGenome : NeatGenome, global::SharpNeat.Core.IGenome<TGenome>
    {
        readonly IGenomeDecoder<TGenome, IBlackBox> _genomeDecoder;
        private ulong _evaluationCount;
        private World _world;
        private ForagingAgent[] _agents;
        private IList<NeatGenome> _genomeList;
        private int _generations;
        private FastRandom _random;
        private int _minReward;
        private List<int> _rewards;
        private double _rewardThreshold;
        private int[] _agentGroups;
		private bool _learningEnabled = true;
		
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
            _world.Stepped += new World.StepEventHandler(esl_step);
            BackpropEpochsPerExample = 1;
            MemParadigm = MemoryParadigm.Fixed;
            CurrentMemorySize = 1;
            GenerationsPerMemorySize = 20;
            LogDiversity = true;
            _random = new FastRandom();
        }

        

        void addRewardToStatisticsList(object sender, IAgent eater, Plant eaten)
        {
            if (TeachParadigm != TeachingParadigm.SameSpeciesRewardFiltering)
                return;
            _rewards.Add(eaten.Reward);
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
        public bool LogDiversity { get; set; }


        /// <summary>
        /// Main genome evaluation loop with no phenome caching (decode on each evaluation).
        /// </summary>
        public void Evaluate(IList<TGenome> genomeList)
        {
            _genomeList = (IList<NeatGenome>)genomeList;
            _agents = new ForagingAgent[genomeList.Count];
            UpdatesThisGeneration = 0;

            if (TeachParadigm == TeachingParadigm.SameSpeciesRewardFiltering)
            {
                _rewards = new List<int>();
                _rewardThreshold = 0;
            }

            if (TeachParadigm == TeachingParadigm.EgalitarianEvolvedAcceptability)
                // Creates acceptability functions from each genome
                GenomesToAcceptability(genomeList);
            else
                // Creates policy networks from each genome
                GenomesToPolicies(genomeList);

            // Divides the population into subcultures
            CreateSubcultures();

            // Logs the diversity of the population before evaluation
            if(LogDiversity)
                writeDiversityStats(true);
            
            // If we're in a student-teacher model, pre-train the agents
            if (TeachParadigm == TeachingParadigm.GenerationalChampionOfflineTraining)
                trainPopulationUsingGenerationalChampion();

            _world.Agents = _agents;
            _world.Reset();

            for (CurrentTimeStep = 0; CurrentTimeStep < MaxTimeSteps; CurrentTimeStep++)
                // Move the world forward one step
                _world.Step();

            // Set the fitness of each genome to the fitness its phenome earned in the world.
            for(int i = 0; i < _agents.Length; i++)
            {
                // NEAT requires fitness to be >= 0, so if the teacher had negative fitness, we cap it at 0.
                _genomeList[i].EvaluationInfo.SetFitness(Math.Max(0, _agents[i].Fitness));

                // This alternate fitness is purely for logging purposes, so we use the actual fitness
                _genomeList[i].EvaluationInfo.AlternativeFitness = _agents[i].Fitness;
            }

            // Analyze diversity after evaluation
            if(LogDiversity)
                writeDiversityStats(false);

            // Lamarkian Evolution
            if (TeachParadigm == TeachingParadigm.EgalitarianEvolvedAcceptability)
                PerformLamarkianEvolution(genomeList, a => (FastCyclicNetwork)((RecurrentNeuralAcceptability)((SocialAgent)a).AcceptabilityFn).Brain);
            else if (EvoParadigm == EvolutionParadigm.Lamarkian)
                PerformLamarkianEvolution(genomeList);

            // If enabled and it's time, grow the size of the agents' memory window.
            if (MemParadigm == MemoryParadigm.IncrementalGrowth 
                && _generations % GenerationsPerMemorySize == 0
                && CurrentMemorySize < MaxMemorySize)
                CurrentMemorySize++;

            _generations++;
            _evaluationCount += (ulong) genomeList.Count;
        }

        private void GenomesToAcceptability(IList<TGenome> genomeList)
        {
            string TEMP_NETWORK_FILE = string.Format("____temp{0}____network.xml", TrialId);

            var neatGenomeParams = new NeatGenomeParameters()
            {
                ActivationFn = PlainSigmoid.__DefaultInstance,
                InitialInterconnectionsProportion = 1
            };

            int inputs = _world.PlantTypes.Count() * World.SENSORS_PER_OBJECT_TYPE
                        + _world.Predators.Count() * World.SENSORS_PER_OBJECT_TYPE
                        + 1;
            int outputs = 2;

            var factory = new NeatGenomeFactory(inputs, outputs, neatGenomeParams);
            for (int i = 0; i < _agents.Length; i++)
            {
                // Decode the genome.
                IBlackBox phenome = _genomeDecoder.Decode(genomeList[i]);
                IAcceptabilityFunction accept = new RecurrentNeuralAcceptability(phenome);

                // Check that the genome is valid.
                if (phenome == null)
                {
                    Console.WriteLine("Couldn't decode genome {0}!", i);
                    _agents[i] = new SpinningAgent(i);
                    continue;
                }

                // Create a feed forward network with 10 hidden nodes and random weights
                SocialExperiment.CreateNetwork(TEMP_NETWORK_FILE, inputs, outputs);
                using (var xr = XmlReader.Create(TEMP_NETWORK_FILE))
                {
                    var controllerGenome = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, factory)[0];
                    var controllerPhenome = _genomeDecoder.Decode((TGenome)controllerGenome);
                    _agents[i] = new SocialAgent(i, _genomeList[i].SpecieIdx, controllerPhenome, accept) { MemorySize = CurrentMemorySize };
                    var network = (FastCyclicNetwork)controllerPhenome;
                    network.Momentum = ((SocialAgent)_agents[i]).Momentum;
                    network.BackpropLearningRate = ((SocialAgent)_agents[i]).LearningRate;
                }
            }

            File.Delete(TEMP_NETWORK_FILE);
        }

        private void GenomesToPolicies(IList<TGenome> genomeList)
        {
            for (int i = 0; i < _agents.Length; i++)
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
                    _agents[i] = getAgent(i, phenome);
            }
        }

        /// <summary>
        /// Saves all phenotypic progress back to the genomes.
        /// </summary>
        private void PerformLamarkianEvolution(IList<TGenome> genomeList)
        {
            PerformLamarkianEvolution(genomeList, a => ((FastCyclicNetwork)((NeuralAgent)a).Brain));
        }

        /// <summary>
        /// Saves all phenotypic progress back to the genomes.
        /// </summary>
        private void PerformLamarkianEvolution(IList<TGenome> genomeList, Func<IAgent, FastCyclicNetwork> networkSelector)
        {
            for (int i = 0; i < _agents.Length; i++)
            {
                var agent = _agents[i];

                // Get the network for this teacher
                var network = networkSelector(agent);

                // Get the genome for this teacher
                var genome = (NeatGenome)genomeList[i];

                // Update the genome to match the phenome weights
                foreach (var conn in network.ConnectionArray)
                {
                    var genomeConn = (ConnectionGene)genome.ConnectionList.First(g => g.SourceNodeId == genome.NodeList[conn._srcNeuronIdx].Id && g.TargetNodeId == genome.NodeList[conn._tgtNeuronIdx].Id);
                    genomeConn.Weight = conn._weight;
                }
            }
        }

        /// <summary>
        /// Puts the agents into equal-sized subcultural groups.
        /// 
        /// <param name="numGroups">The number of groups to divide the population into</param>
        /// </summary>
        private void CreateSubcultures(int numGroups = 10)
        {
            int minGroupSize = _agents.Length / numGroups;

            _agentGroups = new int[_agents.Length];

            // Create a list of all the teacher IDs
            List<int> agentIds = new List<int>();
            for (int i = 0; i < _agentGroups.Length; i++)
            {
                _agentGroups[i] = -1;
                agentIds.Add(i);
            }

            // Select each ID for each subculture randomly
            for (int i = 0; i < _agentGroups.Length; i++)
            {
                int temp = _random.Next(agentIds.Count);
                int idx = agentIds[temp];
                _agentGroups[idx] = i % numGroups;
                agentIds.RemoveAt(temp);
            }

            Debug.Assert(_agentGroups.GroupBy(g => g).Min(ag => ag.Count()) >= minGroupSize);
            Debug.Assert(_agentGroups.GroupBy(g => g).Max(ag => ag.Count()) <= minGroupSize+1);
            Debug.Assert(_agentGroups.GroupBy(g => g).Count() == numGroups);
        }

        void trainPopulationUsingGenerationalChampion()
        {
            if (_generations == 0)
                return;
			int num_teachers = 1;
            var teacherIdx = getTeacherIndexes(num_teachers);
            var teachers = new List<ForagingAgent>();
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
		
		private ForagingAgent getAgent(int i, IBlackBox phenome){
			switch (AgentType)
                    {
                        case AgentTypes.Neural:
                            //return new NeuralAgent(i, _genomeList[i].SpecieIdx, phenome);
                    // TODO: DO NOT KEEP THIS CODE
                            return new ForagingAgent(i);
                        case AgentTypes.Social:
                            var a = new SocialAgent(i, _genomeList[i].SpecieIdx, phenome, sar => sar.Last.Value.Reward > 0)
                            {
                                MemorySize = CurrentMemorySize
                            };
                            var network = (FastCyclicNetwork)phenome;
                            network.Momentum = ((SocialAgent) a).Momentum;
                            network.BackpropLearningRate = ((SocialAgent) a).LearningRate;
                            return a ;
                        case AgentTypes.QLearning:
                            return new QLearningAgent(i, _genomeList[i].SpecieIdx, phenome, 8, 4, _world);
                        case AgentTypes.Spinning:
                            return new SpinningAgent(i);
                        default:
                            return null;
                    }
		}
		
        List<int> getTeacherIndexes(int num_teachers)
        {
            List<int> champIndexes = new List<int>();
            for (int i = 0; i < num_teachers; i++)
                champIndexes.Add(i);

            for (int i = num_teachers; i < _genomeList.Count; i++)
                if (_genomeList[i].EvaluationInfo.Fitness > champIndexes.Max(t => _genomeList[t].EvaluationInfo.Fitness))
                {
                    champIndexes.Add(i);
                    champIndexes.RemoveAt(argMin(champIndexes));
                }
            return champIndexes;
        }
		
        private void TeachAgent(IAgent teacher, IAgent student, double gaussianNoiseStdev)
        {
            UpdatesThisGeneration++;

            // Get the trajectory to learn from
            var memory = ((SocialAgent)teacher).Memory;

            // Get the neural network controlling this teacher
            var network = ((FastCyclicNetwork)((NeuralAgent)student).Brain);

            // Perform a fixed number of backprop epochs to train this teacher
            for (int iteration = 0; iteration < BackpropEpochsPerExample; iteration++)
                foreach (var example in memory)
                {
                    double[] outputs = new double[example.Action.Length];
                    example.Action.CopyTo(outputs, 0);
                    for (int i = 0; i < outputs.Length; i++)
                        outputs[i] = clamp(gaussianMutation(outputs[i], gaussianNoiseStdev), 0, 1);
                    network.Train(example.State, outputs);
                }
        }
		
        // Handle the reward-based teaching paradigms whenever a plant is eaten.
        void _world_PlantEaten(object sender, IAgent eaterAgent, Plant eaten)
        {
            if (!_learningEnabled)
                return;
            // if we're not dealing with a social teacher, then skip this notification.
            if (!(eaterAgent is SocialAgent))
                return;

            // Only learn from rewards if we're using a reward-based social learning paradigm
            if (TeachParadigm != TeachingParadigm.EveryoneRewards
                && TeachParadigm != TeachingParadigm.SameSpeciesRewards
                && TeachParadigm != TeachingParadigm.SameSpeciesRewardFiltering)
                return;

            SocialAgent eater = (SocialAgent)eaterAgent;
            


            // Only learn from positive rewards.
            //if (eaten.Species.Reward > 0)
            //{
                // Train all the agents in parallel
				for(int i = 0; i < _agents.Length; i++)
                {
                    SocialAgent agent = (SocialAgent)_agents[i];

                    // Do not try to teach yourself
                    if (agent == eater)
					    continue;
                    
                    // Only update individuals in your subculture
                    if ((TeachParadigm == TeachingParadigm.SameSpeciesRewards
                        || TeachParadigm == TeachingParadigm.SameSpeciesRewardFiltering)
                        && _agentGroups[eater.Id] != _agentGroups[i])
						continue;
                    
                    // Only learn from high-valued actions
                    //if (TeachParadigm == TeachingParadigm.SameSpeciesRewardFiltering
                    //    && eaten.Species.Reward < _rewardThreshold
                    //    && _rewards.Count > 20)
                    //    //return;
                    //    continue;

                    // Teach the teacher to act like the eater
                    if (agent.AcceptabilityFn.Accept(eater.Memory))
                            TeachAgent(eater, agent);

				}
            //}
        }

        // The social learning step for ESL
        void esl_step(object sender, EventArgs e)
        {
            if (!_learningEnabled)
                return;

            // if we're not dealing with evolved acceptability functions, then skip this notification.
            if (TeachParadigm != TeachingParadigm.EgalitarianEvolvedAcceptability)
                return;

            for(int i = 0; i < _agents.Length; i++)
            {
                var teacher = (SocialAgent)_agents[i];

                var subculture = _agents.Where((a, idx) => i != idx && _agentGroups[idx] == _agentGroups[i]).Select(s => (SocialAgent)s);

                foreach (var observer in subculture)
                    if (observer.AcceptabilityFn.Accept(teacher.Memory))
                        TeachAgent(teacher, observer);
            }
            
        }

        // Handle the Student/Teacher teaching paradigm at each step.
        void _world_Stepped(object sender, EventArgs e)
        {
            // Only learn from every step if we're using a student/teacher paradigm
            if (TeachParadigm != TeachingParadigm.SpeciesChampionOnlineTraining)
                return;

            var allSpecies = _agents.GroupBy(g => _agentGroups[g.Id]);
            foreach (var species in allSpecies)
            {
                var ordered = species.OrderByDescending(a => a.Fitness);
                var best = ordered.First();
                var worst = ordered.Last();

                TeachAgent(best, worst);
            }
            
			#region eiben
            // Mimic the Eiben paper with probabilistic, distance-based teaching
            //const double MAX_DISTANCE = 100;
            //foreach (var teacher in _agents)
            //{
            //    if (_random.NextDouble() > 0.2)
            //        continue;
            //    var student = _agents.Where(a => 
            //        Math.Sqrt((a.X - teacher.X) * (a.X - teacher.X) + (a.Y - teacher.Y) * (a.Y - teacher.Y)) <= MAX_DISTANCE)
            //        .OrderBy(a => a.Fitness)
            //        .First();
            //    TeachAgent(teacher, student);
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
			#endregion
        }
		
		void writeDiversityStats(bool before){
			if (_generations > 0)
            {
				
                _learningEnabled = false;
                DiversityAnalyzer analyser = new DiversityAnalyzer(_world);
                double[][] readings = analyser.getSensorReadings();
                string diverseFile = before ? DiversityFile.Replace(".csv", "_before.csv") : DiversityFile.Replace(".csv", "_after.csv");
						
                using (TextWriter writer = new StreamWriter(diverseFile, true))
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
                _learningEnabled = true;
            }
		}
		
        private void TeachAgent(IAgent teacher, IAgent student)
        {
            UpdatesThisGeneration++;

            // Get the trajectory to learn from
            var memory = ((SocialAgent)teacher).Memory;

            // Get the neural network controlling this teacher
            var network = ((FastCyclicNetwork)((NeuralAgent)student).Brain);
            
            // Perform a fixed number of backprop epochs to train this teacher
            for (int iteration = 0; iteration < BackpropEpochsPerExample; iteration++)
                foreach (var example in memory)
                    network.Train(example.State, example.Action);
        }
		
		#region mathutils
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


		#endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Core;
using social_learning;
using SharpNeat.Phenomes;

namespace VisualizeWorld
{
    public class SimpleEvaluator<TGenome> : IGenomeListEvaluator<TGenome>
        where TGenome : class, global::SharpNeat.Core.IGenome<TGenome>
    {
        readonly IGenomeDecoder<TGenome, IBlackBox> _genomeDecoder;
        private ulong _evaluationCount;
        private World _world;

        /// <summary>
        /// Construct with the provided IGenomeDecoder and ICoevolutionPhenomeEvaluator. 
        /// The number of parallel threads defaults to Environment.ProcessorCount.
        /// </summary>
        public SimpleEvaluator(IGenomeDecoder<TGenome,IBlackBox> genomeDecoder, World environment)
        {
            _genomeDecoder = genomeDecoder;
            _world = environment;
        }

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
        /// Reset the internal state of the evaluation scheme if any exists.
        /// </summary>
        public void Reset()
        {
            _world.Reset();
        }

        /// <summary>
        /// The evaluation environment.
        /// </summary>
        public World Environment { get { return _world; } }

        /// <summary>
        /// Main genome evaluation loop with no phenome caching (decode on each evaluation).
        /// Individuals are competed pairwise against every other individual in the population.
        /// Evaluations are summed to get the final genome fitness.
        /// </summary>
        public void Evaluate(IList<TGenome> genomeList)
        {
            IAgent[] agents = new IAgent[genomeList.Count];
            for(int i = 0; i < agents.Length; i++)
            {
                // Decode the genome.
                IBlackBox phenome = _genomeDecoder.Decode(genomeList[i]);

                // Check that the genome is valid.
                if (phenome == null)
                    agents[i] = new SpinningAgent();
                else
                    agents[i] = new NeuralAgent(phenome);
            }

            _world.Agents = agents;
            _world.Reset();

            for (CurrentTimeStep = 0; CurrentTimeStep < MaxTimeSteps; CurrentTimeStep++)
            {
                // Move the world forward one step
                _world.Step();
            }

            for(int i = 0; i < agents.Length; i++)
            {
                genomeList[i].EvaluationInfo.SetFitness(agents[i].Fitness);
                genomeList[i].EvaluationInfo.AlternativeFitness = agents[i].Fitness;
            }

            _evaluationCount += (ulong)agents.Length;
            _world.Reset();
        }
    }
}

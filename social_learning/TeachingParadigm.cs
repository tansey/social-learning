using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public enum TeachingParadigm
    {
        /// <summary>
        /// Everyone observes and learns from everyone else whenever an action leads to a reward.
        /// </summary>
        EveryoneRewards,
        /// <summary>
        /// Everyone observes and learns only from others in their species whenever an action leads to a reward.
        /// </summary>
        SameSpeciesRewards,
        /// <summary>
        /// Everyone observes and learns only from others in their species whenever an action leads to a reward.
        /// Updates are done in proportion to the reward received.
        /// </summary>
        SameSpeciesRewardProportional,
        /// <summary>
        /// Everyone observes and learns only from others in their species whenever an action leads to a reward
        /// where the reward is at least 1 stdev above the average observed reward for this generation.
        /// </summary>
        SameSpeciesRewardFiltering,
        /// <summary>
        /// All champions from the previous generation teach the children of the current generation on every action (gaussian noise).
        /// </summary>
        GenerationalChampionOfflineTraining,
        /// <summary>
        /// At each step, the current species member with highest fitness teaches the current species member with lowest fitness.
        /// </summary>
        SpeciesChampionOnlineTraining
    }
}

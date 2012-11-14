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
        /// Everyone polls and learns from everyone else whenever an actions leads to a poor reward, in attempt to
        /// find a better action.
        /// </summary>
        EveryonePolling,
        /// <summary>
        /// Everyone learns from everyone else, using a combination of Rewards and Polling strategies.
        /// </summary>
        EveryoneRewardsAndPolling,
        /// <summary>
        /// Everyone observes and learns only from others in their subculture whenever an action leads to a reward.
        /// </summary>
        SubcultureRewards,
        /// <summary>
        /// Everyone observes and learns only from others in their subculture whenever an action leads to a reward.
        /// Updates are done in proportion to the reward received.
        /// </summary>
        SubcultureRewardProportional,
        /// <summary>
        /// Everyone observes and learns only from others in their subculture whenever an action leads to a reward
        /// where the reward is at least 1 stdev above the average observed reward for this generation.
        /// </summary>
        SubcultureRewardFiltering,
        /// <summary>
        /// Everyone polls the others in their subculture whenever a poor reward occurs, in attempt to find a
        /// better action.
        /// </summary>
        SubculturePolling,
        /// <summary>
        /// Everyone learns from their own subculture, using a combination of Rewards and Polling strategies.
        /// </summary>
        SubcultureRewardsAndPolling,
        /// <summary>
        /// All champions from the previous generation teach the children of the current generation on every action (gaussian noise).
        /// </summary>
        GenerationalChampionOfflineTraining,
        /// <summary>
        /// At each step, the current species member with highest fitness teaches the current species member with lowest fitness.
        /// </summary>
        SpeciesChampionOnlineTraining,
        /// <summary>
        /// Uses the ESL algorithm but evolves a neural network for the acceptability function and uses a randomly connected NN
        /// as the controller network.
        /// </summary>
        EgalitarianEvolvedAcceptability
        
    }
}

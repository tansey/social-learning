using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualizeWorld
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
        /// All champions from the previous generation teach the children of the current generation on every action (probabilistically).
        /// </summary>
        StudentTeacherActions
    }
}

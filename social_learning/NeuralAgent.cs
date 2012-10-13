using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;
using SharpNeat.Phenomes.NeuralNets;

namespace social_learning
{
    public class NeuralAgent : ForagingAgent
    {
        const float MAX_TURNING_RADIUS = 30f;
        const float MAX_SPEED_CHANGE = 1f;
        public IBlackBox Brain { get; set; }
        public int SpeciesId { get; set; }
        public bool NavigationEnabled { get; set; }
        public bool HidingEnabled { get; set; }
        
        public NeuralAgent(int id, int speciesId, IBlackBox brain, 
                           bool navigationEnabled, bool hidingEnabled) : base(id)
        {
            Brain = brain;
            SpeciesId = speciesId;
            NavigationEnabled = navigationEnabled;
            HidingEnabled = hidingEnabled;
        }

        protected override float[] getRotationAndVelocity(double[] sensors)
        {
            var outputs = activateNetwork(sensors);

            int outputIdx = 0;

            // If the ANN is controlling the ability of the agent to hide
            // from predators, let it determine the hiding strategy.
            if (HidingEnabled)
            {
                // 0 is forage (no hiding)
                int max = 0;

                // Figure out how many of the outputs are hiding neurons
                outputIdx = NavigationEnabled ? outputs.Length - 2 : outputs.Length;

                // Find the maximum output.
                for (int i = 1; i < outputIdx; i++)
                    if (outputs[i] > outputs[max])
                        max = i;

                // Set the hiding mode to the highest output.
                HidingMode = max;

                // If we are hiding, come to a full stop.
                if (HidingMode > 0)
                    return new float[] { 0, -MaxVelocity };
            }

            // If the ANN is controlling navigation, let it determine its 
            // orientation and velocity changes.
            if (NavigationEnabled)
            {
                // [0,1] -> [-90,90]
                var orientation = (float)(outputs[outputIdx++] - 0.5) * 2f * MAX_TURNING_RADIUS;

                // [0,1] -> [-1,1]
                var velocityDelta = (float)(outputs[outputIdx++] - 0.5) * 2f * MAX_SPEED_CHANGE;

                if (velocityDelta > 1 || velocityDelta < -1)
                    throw new Exception("Velocity outside of bounds! Must be in range [-1,1] but was "
                                            + velocityDelta);

                return new float[] { orientation, velocityDelta };
            }

            // Otherwise, just make a silly random walk.
            return base.getRotationAndVelocity(sensors);
        }

        public virtual ISignalArray activateNetwork(double[] sensors)
        {
            Brain.ResetState();

            // Convert the sensors into an input array for the network
            for (int i = 0; i < sensors.Length; i++)
                Brain.InputSignalArray[i] = sensors[i];

            // Activate the network
            Brain.Activate();

            return Brain.OutputSignalArray;
        }

        public override void Reset()
        {
            Brain.ResetState();
        }

        protected override void ProcessReward(double r)
        {
        }
    }
}

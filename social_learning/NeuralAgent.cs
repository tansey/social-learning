using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;
using SharpNeat.Phenomes.NeuralNets;

namespace social_learning
{
    public class NeuralAgent : Agent
    {
        const float MAX_TURNING_RADIUS = 90f;
        const float MAX_SPEED_CHANGE = 1f;
        public IBlackBox Brain { get; set; }
        
        public NeuralAgent(int id, IBlackBox brain) : base(id)
        {
            Brain = brain;
        }

        protected override float[] getRotationAndVelocity(double[] sensors)
        {
            var outputs = activateNetwork(sensors);

            // [0,1] -> [-90,90]
            var orientation = (float)(outputs[0] - 0.5) * 2f * MAX_TURNING_RADIUS;

            // [0,1] -> [-1,1]
            var velocityDelta = (float)(outputs[1] - 0.5) * 2f * MAX_SPEED_CHANGE;

            if (velocityDelta > 1 || velocityDelta < -1)
                throw new Exception("Velocity outside of bounds! Must be in range [-1,1] but was " 
                                        + velocityDelta);

            return new float[] { orientation, velocityDelta };
        }

        protected virtual ISignalArray activateNetwork(double[] sensors)
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

    }
}

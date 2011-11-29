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
        const float MAX_TURNING_RADIUS = 60f;
        const float MAX_SPEED_CHANGE = 1f;
        public IBlackBox Brain { get; set; }
        
        public NeuralAgent(IBlackBox brain)
        {
            Brain = brain;
        }

        protected override float[] getRotationAndVelocity(double[] sensors)
        {
            var outputs = activateNetwork(sensors);

            // [0,1] -> [-60,60]
            var orientation = (float)(outputs[0] - 0.5) * 2f * MAX_TURNING_RADIUS;

            // [0,1] -> [-1,1]
            var velocityDelta = (float)(outputs[1] - 0.5) * 2f * MAX_SPEED_CHANGE;

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

            var outputs = Brain.OutputSignalArray;

            double[] exampleOutputs = new double[2];

            return outputs;
        }

        public override void Reset()
        {
            Brain.ResetState();
        }

    }
}

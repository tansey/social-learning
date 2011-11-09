using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeat.Phenomes;

namespace social_learning
{
    public class NeuralAgent : Agent
    {
        public IBlackBox Brain { get; set; }
        
        public NeuralAgent(IBlackBox brain)
        {
            Brain = brain;
        }

        protected override float getRotation(double[] sensors)
        {
            // Convert the sensors into an input array for the network
            for (int i = 0; i < sensors.Length; i++)
                Brain.InputSignalArray[i] = sensors[i];

            // Activate the network
            Brain.Activate();

            // TODO: Something based on what the output of the neural network tells us
            var output = Brain.OutputSignalArray[0];

            // [0,1] -> [-180,180]
            var result = (float)(output-0.5) * 360;

            //MessageBox.Show(result.ToString());

            return result;
        }

        public override void Reset()
        {
            Brain.ResetState();
        }
    }
}

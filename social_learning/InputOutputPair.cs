using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class InputOutputPair
    {
        public readonly double[] Inputs;
        public readonly double[] Outputs;

        public InputOutputPair(double[] inputs, double[] outputs)
        {
            Inputs = inputs;
            Outputs = outputs;
        }
    }
}

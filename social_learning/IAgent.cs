using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public interface IAgent
    {
        float X { get; set; }
        float Y { get; set; }
        float Orientation { get; set; }
        double Fitness { get; set; }

        void Step(double[] sensors);
    }
}

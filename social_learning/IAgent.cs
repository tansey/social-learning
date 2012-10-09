using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public interface IAgent : IEdibleThingy<Predator> // TODO: Fix inheritance here
    {
        float Orientation { get; set; }
        float Velocity { get; set; }
        double Fitness { get; set; }
        float MaxVelocity { get; }
        int Id { get; }
        int HidingMode { get; set; }

        void Step(double[] sensors);
        void ReceiveReward(double r);
    }
}

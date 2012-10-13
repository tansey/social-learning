using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class ForagingAgent : EdibleAgent<Predator>
    {
        static int seed = 5;
        Random _rand = new Random(seed++);
        public int HidingMode { get; set; } // TODO: Implement hiding from predators

        public ForagingAgent(int id)
            : base(id)
        {
        }

        protected override float[] getRotationAndVelocity(double[] sensors)
        {
            if (HidingMode > 0)
                return new float[] { 0, 0 };

            if(_rand.Next(0, 2) == 0)
                return new float[] { 0, MaxVelocity };
            
            float orientation =  _rand.Next(-45, 45);
            return new float[] { orientation, -MaxVelocity };
        }

        public override void Reset()
        {
            base.Reset();
        }

        protected override void ProcessReward(double r)
        {

        }
    }
}

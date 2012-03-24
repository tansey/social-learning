using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    // A spinning teacher does nothing but sit there and spin.
    public class SpinningAgent : Agent
    {
        public SpinningAgent(int id) : base(id) { }

        protected override float[] getRotationAndVelocity(double[] sensors)
        {
            return new float[] { 15, -5 };
        }
        public override void Reset()
        {
            
        }
        protected override void ProcessReward(double r)
        {
            
        }
    }
}

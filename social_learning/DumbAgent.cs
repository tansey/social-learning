using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class DumbAgent : Agent
    {
        Random random = new Random();

        public DumbAgent(int id) : base(id) { }

        protected override float[] getRotationAndVelocity(double[] sensors)
        {
            return new float[] { (float)random.Next(45) - 20f, (float)random.Next(2) };
        }

        public override void Reset()
        {
        }
    }
}

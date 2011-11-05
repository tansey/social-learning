using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class DumbAgent : Agent
    {
        Random random = new Random();

        protected override float getRotation(double[] sensors)
        {
            return (float)random.Next(45) - 20f;
        }

        public override void Reset()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    class ControlledAgent : Agent
    {
        public ControlledAgent(int id) : base(id) { }

        public override void Reset()
        {
        }

        protected override float[] getRotationAndVelocity(double[] sensors){
            return new float[]{0,MaxVelocity};   
        }
    }
}

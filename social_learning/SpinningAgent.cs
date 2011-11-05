using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    // A spinning agent does nothing but sit there and spin.
    public class SpinningAgent : Agent
    {
        protected override float getRotation(double[] sensors)
        {
            return 180f;
        }

        public override void Reset()
        {
            
        }
    }
}

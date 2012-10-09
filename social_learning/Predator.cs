using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace social_learning
{
    public class Predator : Agent
    {
        static int seed = 5;
        Random _rand = new Random(seed++);
        public int AttackType { get; set; }

        public Predator(int id, int attackType)
            : base(id)
        {
            Debug.Assert(attackType > 0, "Attack type must be positive and non-zero.");
            AttackType = attackType;
        }

        protected override float[] getRotationAndVelocity(double[] sensors)
        {
            float orientation = _rand.Next(-5, 5);


            return new float[] { orientation, MaxVelocity };
        }

        public override void Reset()
        {
            
        }

        protected override void ProcessReward(double r)
        {
            
        }
    }
}

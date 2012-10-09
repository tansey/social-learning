using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public abstract class Agent : EdibleThingy<Predator>, IAgent 
    {
        const int AGENT_RADIUS = 20;
        const int AGENT_REWARD = 100;

        private readonly int _id;
        public float Orientation { get; set; }
        public float Velocity { get; set; }
        public double Fitness { get; set; }
        public float MaxVelocity { get { return 5f; } }
        public int Id { get { return _id; } }
        public int HidingMode { get; set; } // TODO: Implement hiding from predators

        public Agent(int id) : base(AGENT_RADIUS, AGENT_REWARD)
        {
            _id = id;
        }

        protected abstract float[] getRotationAndVelocity(double[] sensors);

        public void Step(double[] sensors)
        {
            var output = getRotationAndVelocity(sensors);

            Orientation += output[0];
            Orientation += 360;
            Orientation %= 360;

            Velocity += output[1];
            Velocity = Math.Min(MaxVelocity, Math.Max(0, Velocity));

            X += Velocity * (float)(Math.Cos(Orientation * Math.PI / 180.0));
            Y += Velocity * (float)(Math.Sin(Orientation * Math.PI / 180.0));
        }

        public virtual void ReceiveReward(double r)
        {
            Fitness += r;
            ProcessReward(r);
        }

        protected abstract void ProcessReward(double r);
    }
}

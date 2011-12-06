using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public abstract class Agent : IAgent
    {
        private readonly int _id;
        public float X { get; set; }
        public float Y { get; set; }
        public float Orientation { get; set; }
        public float Velocity { get; set; }
        public double Fitness { get; set; }
        public float MaxVelocity { get { return 5f; } }
        public int Id { get { return _id; } }

        public Agent(int id)
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

        public abstract void Reset();
    }
}

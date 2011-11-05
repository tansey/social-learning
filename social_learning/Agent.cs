using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public abstract class Agent : IAgent
    {
        public float StepSize { get { return 5f; } }
        public float X { get; set; }
        public float Y { get; set; }
        public float Orientation { get; set; }
        public double Fitness { get; set; }

        protected abstract float getRotation(double[] sensors);

        public void Step(double[] sensors)
        {
            Orientation += getRotation(sensors);
            Orientation %= 360;

            X += StepSize * (float)(Math.Cos(Orientation * Math.PI / 180.0));
            Y += StepSize * (float)(Math.Sin(Orientation * Math.PI / 180.0));
        }

        public abstract void Reset();
    }
}

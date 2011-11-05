using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class DumbAgent : IAgent
    {
        private float turningDirection = 0;
        Random random = new Random();
        const float stepSize = 5f;

        public float X { get; set; }
        public float Y { get; set; }
        public float Orientation { get; set; }
        public double Fitness { get; set; }

        public void Step(double[] sensors)
        {
            turningDirection = random.Next(45) - 20;
            Orientation += turningDirection;
            Orientation %= 360;

            X += stepSize * (float)(Math.Sin(Orientation * Math.PI / 180.0));
            Y += stepSize * (float)(Math.Cos(Orientation * Math.PI / 180.0));
        }
    }
}

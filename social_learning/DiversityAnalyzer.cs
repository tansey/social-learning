using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class DiversityAnalyzer
    {
        private World _world;
        int[][] locations;
        int[] orientations;
        float[] velocities;
        private int numlocations = 100;
        private Random _random = new Random();

        public DiversityAnalyzer(World world)
        {
            _world = world;
            locations = new int[numlocations][];
            for (int i = 0; i < numlocations; i++)
                locations[i] = new int[2];
            orientations = new int[100];
            velocities = new float[100];
            for (int i = 0; i < numlocations; i++)
            {
                locations[i][0] = _random.Next() % world.Width;
                locations[i][1] = _random.Next() % world.Height;
                orientations[i] = _random.Next() % 360;
                velocities[i] = (float)_random.NextDouble() * 5f;
            }
            
        }



        public double[][] getSensorReadings()
        {
            IAgent a = _world.Agents.First();
            double[][] readings = new double[numlocations][];
            for (int i = 0; i < readings.Length; i++)
                readings[i] = new double[3];
            int j = 0;
            foreach (int[] location in locations)
            {
                a.X = location[0];
                a.Y = location[1];
                a.Orientation = orientations[j];
                a.Velocity = velocities[j];
                readings[j] = _world.calculateForagingAgentSensors(a);
                j++;
            }
            return readings;
        }

        public double[][] getAgentResponses(double[] reading)
        {
            double[][] responses = new double[_world.Agents.Count()][];
            for (int i = 0; i < _world.Agents.Count(); i++)
                responses[i] = new double[2];
            for (int i = 0; i < _world.Agents.Count(); i++)
            {
                NeuralAgent currentAgent = (NeuralAgent)_world.Agents.ElementAt(i);
                var results = currentAgent.activateNetwork(reading);
                results.CopyTo(responses[i], 0);
            }
            return responses;
        }

        public double[] getResponseVariance(double[] reading)
        {
            var responses = getAgentResponses(reading);
            double[] result = new double[2];
            result[0] = Variance(responses.Select(d => d[0]));
            result[1] = Variance(responses.Select(d => d[1]));

            return result;
        }

        /// <summary>
        /// Calculates the sample standard deviation.
        /// </summary>
        public static double Variance(IEnumerable<double> data)
        {
            if (data.Count() < 2)
                return 0;

            double avg = data.Average();
            double numerator = 0;
            foreach (double d in data)
                numerator += (d - avg) * (d - avg);

            double variance = numerator / (double)(data.Count() - 1);

            return variance;
        }
    }
}

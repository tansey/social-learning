using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace social_learning
{
    public class World
    {
        private Random random = new Random();
        const int SENSORS_PER_PLANT_TYPE = 8;
        private int _step;

        public IEnumerable<IAgent> Agents { get; set; }
        public double AgentHorizon { get; set; }

        /// <summary>
        /// nom nom nom
        /// </summary>
        public IList<Plant> Plants { get; set; }

        public IEnumerable<PlantSpecies> PlantTypes { get; set; }

        public int Height { get; set; }
        public int Width { get; set; }
        public int CurrentStep { get { return _step; } }

        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler Changed;

        public World(IEnumerable<IAgent> agents, IEnumerable<PlantSpecies> species, int height, int width, int numPlantsPerSpecies)
        {
            Agents = agents;
            PlantTypes = species;
            Height = height;
            Width = width;

            // Randomly populate the world with plants
            Plants = new List<Plant>();
            foreach (var s in species)
                for (int i = 0; i < numPlantsPerSpecies; i++)
                    Plants.Add(new Plant(s) { X = random.Next() % width, Y = random.Next() % height });
        }

        /// <summary>
        /// Moves all agents in the world forward by one step and collects food for any agents on top of
        /// a plant.
        /// </summary>
        public void Step()
        {
            foreach (var agent in Agents)
            {
                var sensors = calculateSensors(agent);
                agent.Step(sensors);
                if (agent.X >= Height)
                    agent.X = Width;   
                if (agent.Y > Height)
                    agent.Y = Height;
                if (agent.X < 0)
                    agent.X = 0;
                if (agent.Y < 0)
                    agent.Y = 0;

                if (agent.X == 0 || agent.Y == 0 || agent.X == Width || agent.Y == Height)
                    agent.Orientation = (agent.Orientation - 180) % 360;

                foreach (var plant in Plants)
                    if (calculateDistance(agent, plant) < plant.Species.Radius && plant.AvailableForEating(agent))
                    {
                        // Eat the plant
                        plant.EatenBy(agent, _step);
                        agent.Fitness += plant.Species.Reward;

                        // TODO: Update the population if the reward was good/bad
                    }
            }
            onChanged(EventArgs.Empty);
            _step++;
        }

        // Notify any listeners that the world state has changed
        private void onChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        /// <summary>
        /// Resets the world to the initial state.
        /// </summary>
        public void Reset()
        {
            foreach (var agent in Agents)
            {
                agent.X = Width / 2; //random.Next(Width);
                agent.Y = Height / 2; //random.Next(Height);
                agent.Orientation = 0;// random.Next(360);
                agent.Fitness = 0;
            }

            foreach (var plant in Plants)
            {
                plant.Reset();
                plant.X = random.Next(Width);
                plant.Y = random.Next(Height);
            }

            _step = 0;

            // Notify any listeners that the world state has changed
            onChanged(EventArgs.Empty);
        }

        #region Helper methods for calculating mathy things
        private double[] calculateSensors(IAgent agent)
        {
            double[] sensors = new double[PlantTypes.Count() * SENSORS_PER_PLANT_TYPE];

            // For every plant
            foreach (var plant in Plants)
            {
                // if the plant isn't available for eating then we do not activate the sensors
                if (!plant.AvailableForEating(agent))
                    continue;

                // Calculate the distance to the object from the agent
                var dist = calculateDistance(agent, plant);

                // If it's too far away for the agent to see
                if (dist > AgentHorizon)
                    continue;

                // Identify the appropriate sensor
                var sIdx = getSensorIndex(agent, plant);

                if (sIdx == -1)
                    continue;

                // Add 1/distance to the sensor
                sensors[sIdx] += 1.0 / dist;
            }

            foreach (var sensor in sensors)
                if (sensor > 1)
                    throw new Exception("Invalid sensor value: " + sensor);

            return sensors;
        }

        private int getSensorIndex(IAgent agent, Plant plant)
        {
            double pos = Math.Atan((plant.Y - agent.Y) / (plant.X - agent.X));

            double startSensor = (agent.Orientation - 90) % 360;
            double sensorWidth = 180.0 / (double)SENSORS_PER_PLANT_TYPE;

            for (int i = 0; i < SENSORS_PER_PLANT_TYPE; i++)
                if ((startSensor + i * sensorWidth) % 360 < pos && (startSensor + (i + 1) * sensorWidth) % 360 >= pos)
                    return plant.Species.SpeciesId * SENSORS_PER_PLANT_TYPE + i;

            return -1;
            //throw new Exception("Something went wrong! (Eli screwed up the formula)");
        }

        private static double calculateDistance(IAgent agent, Plant plant)
        {
            return Math.Sqrt((agent.X - plant.X) * (agent.X - plant.X) + (agent.Y - plant.Y) * (agent.Y - plant.Y));
        }
        #endregion
    }
}

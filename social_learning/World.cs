using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace social_learning
{
    public class World
    {
        private Random random = new Random();
        public const int SENSORS_PER_PLANT_TYPE = 8;
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
        public PlantLayoutStrategies PlantLayoutStrategy { get; set; }

        // Event called when the state of the world is changed
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler Changed;

        // Event called when the state of the world is advanced one step
        public delegate void StepEventHandler(object sender, EventArgs e);
        public event StepEventHandler Stepped;

        // Event called when an agent eats a piece of food.
        public delegate void PlantEatenHandler(object sender, IAgent eater, Plant eaten);
        public event PlantEatenHandler PlantEaten;

        public World(IEnumerable<IAgent> agents, int height, int width, IEnumerable<PlantSpecies> species, PlantLayoutStrategies layout = PlantLayoutStrategies.Uniform)
        {
            Agents = agents;
            PlantTypes = species;
            Height = height;
            Width = width;

            // Randomly populate the world with plants
            Plants = new List<Plant>();
            foreach (var s in species)
                for (int i = 0; i < s.Count; i++)
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
                if (agent.X >= Width)
                    agent.X -= Width;
                if (agent.Y > Height)
                    agent.Y -= Height;
                if (agent.X < 0)
                    agent.X += Width;
                if (agent.Y < 0)
                    agent.Y += Height;

                //if (agent.X == 0 || agent.Y == 0 || agent.X == Width || agent.Y == Height)
                //    agent.Orientation = (agent.Orientation - 180) % 360;
            }

            // Make a separate pass over all the agents, now that they're in new locations
            // and determine who is on top of a plant.
            foreach (var agent in Agents)
                foreach (var plant in Plants)
                    if (calculateDistance(agent, plant) < plant.Species.Radius && plant.AvailableForEating(agent))
                    {
                        // Eat the plant
                        plant.EatenBy(agent, _step);
                        agent.Fitness += plant.Species.Reward;

                        // Update the population if the reward was good/bad
                        onPlantEaten(agent, plant);
                    }

            onStepped(EventArgs.Empty);
            onChanged(EventArgs.Empty);
            _step++;
        }

        // Notify any listeners that the world state has changed
        private void onChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        // Notify any listeners that the world state has stepped forward
        private void onStepped(EventArgs e)
        {
            if (Stepped != null)
                Stepped(this, e);
        }

        private void onPlantEaten(IAgent eater, Plant plant)
        {
            if (PlantEaten != null)
                PlantEaten(this, eater, plant);
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

            layoutPlants();

            _step = 0;

            // Notify any listeners that the world state has changed
            onChanged(EventArgs.Empty);
        }

        #region Plant Layouts
        private void layoutPlants()
        {
            switch (PlantLayoutStrategy)
            {
                case PlantLayoutStrategies.Uniform:
                    uniformLayout();
                    break;
                case PlantLayoutStrategies.Spiral:
                    spiralLayout();
                    break;
                case PlantLayoutStrategies.Clustered:
                    clusteredLayout();
                    break;
                default:
                    break;
            }
        }

        private void uniformLayout()
        {
            int curX = Width / 2, curY = Height / 2;
            int lowerY = random.Next(30), upperY = random.Next(30);
            foreach (var plant in Plants)
            {
                plant.Reset();
                // Uniform random
                plant.X = random.Next(Width);
                plant.Y = random.Next(Height);
            }
        }

        private void spiralLayout()
        {
            int speciesIdx = 0;
            foreach (var species in PlantTypes)
            {
                double theta = random.NextDouble() * 2 * Math.PI;
                double dtheta = (random.NextDouble()) / 15 + .1;
                dtheta *= random.Next(2) == 1 ? -1 : 1;
                int x;
                int y;
                double r = 6;

                for (int i = 0; i < species.Count; i++)
                {
                    r += (Height - 6) / (double)(2 * species.Count);
                    theta += dtheta;
                    x = (int)(Width / 2 + r * Math.Cos(theta));
                    y = (int)(Width / 2 + r * Math.Sin(theta));
                    var plant = Plants[speciesIdx * species.Count + i];
                    plant.Reset();
                    plant.X = x;
                    plant.Y = y;
                }
                speciesIdx++;
            }
        }

        private void clusteredLayout()
        {
            int speciesIdx = 0;
            foreach (var species in PlantTypes)
            {
                int clusterRadius = random.Next(20) + 20;
                int k = random.Next(5) + 5;
                int curx = random.Next(Width - 2 * clusterRadius) + clusterRadius;
                int cury = random.Next(Height - 2 * clusterRadius) + clusterRadius;
                for (int i = 0; i < species.Count; i++)
                {
                    if (i % k == 0)
                    {
                        curx = random.Next(Width - 2 * clusterRadius) + clusterRadius;
                        cury = random.Next(Height - 2 * clusterRadius) + clusterRadius;
                    }

                    int x = curx + random.Next(clusterRadius) - clusterRadius / 2;
                    int y = cury + random.Next(clusterRadius) - clusterRadius / 2;
                    var plant = Plants[speciesIdx * species.Count + i];
                    plant.Reset();
                    plant.X = x;
                    plant.Y = y;

                }
                speciesIdx++;
            }
        }
        #endregion

        #region Helper methods for calculating mathy things
        public double[] calculateSensors(IAgent agent)
        {
            double[] sensors = new double[PlantTypes.Count() * (SENSORS_PER_PLANT_TYPE) + 1 + SENSORS_PER_PLANT_TYPE];

            sensors[0] = agent.Velocity / agent.MaxVelocity;

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
                sensors[sIdx] += 1.0 / dist * 5.0;
            }

            // Calculate wall sensors
            for (int i = 0; i < SENSORS_PER_PLANT_TYPE; i++)
            {
                var dist = canSeeWallAlongLine(agent, i);
                sensors[PlantTypes.Count() * SENSORS_PER_PLANT_TYPE + 1 + i] = dist;
            }

            return sensors;
        }

        private double canSeeWallAlongLine(IAgent agent, int dir)
        {
            var x_orig = agent.X;
            var y_orig = agent.Y;
            var dirOfSensor = agent.Orientation - 90 + dir * 180.0 / (double)SENSORS_PER_PLANT_TYPE;
            var endX = x_orig + Math.Cos(dirOfSensor);
            var endY = y_orig + Math.Sin(dirOfSensor) * AgentHorizon;

            if (endX > 0 && endY > 0 && endX < Width && endY < Height)
                return 0;
            for (int j = 1; j <= AgentHorizon; j++)
            {
                var new_x = x_orig + Math.Cos(dirOfSensor) * AgentHorizon;
                var new_y = y_orig + Math.Sin(dirOfSensor) * AgentHorizon;
                if (new_x < 0 || new_y < 0 || new_x > Width || new_y > Height)
                    return 1 / (double)j;
            }
            return 0;
        }

        private int getSensorIndex(IAgent agent, Plant plant)
        {
            double[] newCoords = getClosestCoordinates(agent, plant);
            double agentX = newCoords[0];
            double agentY = newCoords[1];
            double plantX = newCoords[2];
            double plantY = newCoords[3];
            double pos = Math.Atan((plantY - agentY) / (plantX - agentX)) * 180.0 / Math.PI + 360;
            if (plantX < agentX)
                pos += 180;
            pos %= 360;
            double sensorWidth = 180.0 / (double)SENSORS_PER_PLANT_TYPE;
            double dtheta = pos - agent.Orientation;
            if (Math.Abs(pos - agent.Orientation) > Math.Abs(pos - (agent.Orientation + 360)))
                dtheta = pos - (agent.Orientation + 360);
            int where = (int)(dtheta / sensorWidth);
            where += 4;
            if (0 <= where && where < SENSORS_PER_PLANT_TYPE)
            {
                Console.WriteLine("plant x, y {0} {1} Dude x, y {2} {3}, pos, {4}, orientation {5} sensor: {6} ",
                    plant.X, plant.Y, agent.X, agent.Y, pos, agent.Orientation, where);
                return plant.Species.SpeciesId * SENSORS_PER_PLANT_TYPE + where + 1;
            }
            Console.WriteLine("plant x, y {0} {1} Dude x, y {2} {3}, pos, {4}, orientation {5} sensor: {6} ",
            plant.X, plant.Y, agent.X, agent.Y, pos, agent.Orientation, -1);
            return -1;
        }

        private double calculateDistance(IAgent agent, Plant plant)
        {
            double[] coords = getClosestCoordinates(agent, plant);
            return calculateDistance(coords);
        }

        private double calculateDistance(double[] coords)
        {
            return Math.Sqrt((coords[0] - coords[2]) * (coords[0] - coords[2]) + (coords[1] - coords[3]) * (coords[1] - coords[3]));
        }

        private double[] getClosestCoordinates(IAgent agent, Plant plant)
        {
            double[] coords;
            int newPlantX = plant.X + Width;
            int newPlantY = plant.Y + Height;
            var newAgentX = agent.X + Width;
            var newAgentY = agent.Y + Height;
            double[] regularArgs = { agent.X, agent.Y, plant.X, plant.Y };
            double[] belowArgs = { agent.X, agent.Y, plant.X, newPlantY };
            double[] rightArgs = { agent.X, agent.Y, newPlantX, plant.Y };
            double[] leftArgs = { newAgentX, newAgentY, plant.X, plant.Y };
            double[] aboveArgs = { agent.X, newAgentY, plant.X, plant.Y };
            var regular = calculateDistance(regularArgs);
            var min = regular;
            coords = regularArgs;
            var below = calculateDistance(belowArgs);
            if (below < min)
            {
                min = below;
                coords = belowArgs;
            }
            var right = calculateDistance(rightArgs);
            if (right < min)
            {
                min = right;
                coords = rightArgs;
            }
            var above = calculateDistance(aboveArgs);
            if (above < min)
            {
                min = above;
                coords = aboveArgs;
            }
            var left = calculateDistance(leftArgs);
            if (left < min)
            {
                coords = leftArgs;
            }
            return coords;
        }
        #endregion
    }
}

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
        const int DEFAULT_AGENT_HORIZON = 50;
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
            AgentHorizon = DEFAULT_AGENT_HORIZON;

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
            double[] sensors = new double[PlantTypes.Count() * (SENSORS_PER_PLANT_TYPE) + 1];

            sensors[0] = agent.Velocity / agent.MaxVelocity;

            // For every plant
            foreach (var plant in Plants)
            {
                //Console.WriteLine("Plant: {0}", plant.Species.Name);
                // if the plant isn't available for eating then we do not activate the sensors
                if (!plant.AvailableForEating(agent))
                    continue;

                // Calculate the distance to the object from the agent
                var dist = calculateDistance(agent, plant);

                //Console.WriteLine("Dist: {0}", dist);

                // If it's too far away for the agent to see
                if (dist > AgentHorizon)
                    continue;

                // Identify the appropriate sensor
                int sIdx = getSensorIndex(agent, plant);

                //Console.WriteLine("Sensor: {0}", sIdx);

                if (sIdx == -1)
                    continue;

                // Add 1/distance to the sensor
                sensors[sIdx] += 1.0 - dist / AgentHorizon;
            }

            return sensors;
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

            Console.WriteLine("Plant (x, y): ({0}, {1}) Agent (x, y): ({2}, {3}) pos: {4}, dtheta: {5} orientation: {6} Sensor width: {7}",
                    plant.X, plant.Y, agent.X, agent.Y, pos, dtheta, agent.Orientation, sensorWidth);
            
            // If the plant's behind us
            if(dtheta < -90 || dtheta > 90)
                return -1;

            int idx = 1;
            for (double degrees = -90 + sensorWidth; degrees <= 90 + double.Epsilon; degrees += sensorWidth, idx++)
                if (degrees > dtheta)
                    return idx + plant.Species.SpeciesId * SENSORS_PER_PLANT_TYPE;
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
            double[][] args = new double[][]{new double[]{ agent.X, agent.Y, plant.X, plant.Y },
            new double[]{ agent.X, agent.Y, plant.X, newPlantY },
            new double[]{ agent.X, agent.Y, newPlantX, plant.Y },
            new double[]{ agent.X, newAgentY, plant.X, plant.Y },
            new double[]{ agent.X, newAgentY, plant.X, plant.Y },
            new double[]{ newAgentX, agent.Y, plant.X, newPlantY },
            new double[]{ agent.X, agent.Y, newPlantX, newPlantY },
            new double[]{ newAgentX, newAgentY, plant.X, plant.Y },
            new double[]{ agent.X, newAgentY, newPlantX, plant.Y },
            };
            var min = calculateDistance(args[0]);
            coords = args[0];
            for (int i = 1; i < args.Length; i++)
                if (calculateDistance(args[i]) < min)
                {
                    min = calculateDistance(args[i]);
                    coords = args[i];
                }
            return coords;
        }
        #endregion
    }
}

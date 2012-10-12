using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace social_learning
{
    public class World
    {
        private Random _random = new Random();
        public const int SENSORS_PER_OBJECT_TYPE = 8;
        const int DEFAULT_AGENT_HORIZON = 100;
        private int _step;
        SensorDictionary _sensorDictionary;

        #region Properties
        /// <summary>
        /// The cached lookup table that helps speed up the sensor calculations.
        /// </summary>
        public SensorDictionary SensorLookup
        {
            get { return _sensorDictionary; }
            set { _sensorDictionary = value; }
        }

        /// <summary>
        /// All of the agents in the world.
        /// </summary>
        public IEnumerable<IAgent> Agents { get; set; }

        /// <summary>
        /// The radius of the agents' field of vision.
        /// </summary>
        public double AgentHorizon { get; set; }

        /// <summary>
        /// nom nom nom
        /// </summary>
        public IList<Plant> Plants { get; set; }

        /// <summary>
        /// Run!
        /// </summary>
        public IEnumerable<Predator> Predators { get; set; }

        /// <summary>
        /// The types of plants in the world.
        /// </summary>
        public IEnumerable<PlantSpecies> PlantTypes { get; set; }

        /// <summary>
        /// The height of the world.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The width of the world.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The number of steps that have been taken in this world since the last reset.
        /// </summary>
        public int CurrentStep { get { return _step; } }

        /// <summary>
        /// The strategy used to layout the plants upon each reset.
        /// </summary>
        public PlantLayoutStrategies PlantLayoutStrategy { get; set; }

        /// <summary>
        /// The reward every agent receives at every step.
        /// </summary>
        public int StepReward { get; set; }
        #endregion

        #region Events and Delegates
        /// <summary>
        /// Event called when the state of the world is changed.
        /// </summary>
        public event ChangedEventHandler Changed;
        public delegate void ChangedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Event called when the state of the world is advanced one step.
        /// </summary>
        public event StepEventHandler Stepped;
        public delegate void StepEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Event called when an teacher eats a piece of food.
        /// </summary>
        public event PlantEatenHandler PlantEaten;
        public delegate void PlantEatenHandler(object sender, IAgent eater, Plant eaten);

        /// <summary>
        /// Event called when a predator catches and eats an pred.
        /// </summary>
        public event AgentEatenHandler AgentEaten;
        public delegate void AgentEatenHandler(object sender, Predator eater, IAgent eaten);
        #endregion


        public World(IEnumerable<IAgent> agents, int height, int width, 
             IEnumerable<PlantSpecies> species, IEnumerable<Predator> predators,
             PlantLayoutStrategies layout = PlantLayoutStrategies.Uniform, int agentHorizon = DEFAULT_AGENT_HORIZON)
        {
            Agents = agents;
            PlantTypes = species;
            Height = height;
            Width = width;
            AgentHorizon = agentHorizon;
            Predators = predators;

            // Randomly populate the world with plants
            Plants = new List<Plant>();
            foreach (var s in species)
                for (int i = 0; i < s.Count; i++)
                    Plants.Add(new Plant(s));
        }

        /// <summary>
        /// Moves all agents in the world forward by one step and collects food for any agents on top of
        /// a plant.
        /// </summary>
        public void Step()
        {
            // If no one set a lookup table yet, generate one.
            if (_sensorDictionary == null)
                _sensorDictionary = new SensorDictionary((int)AgentHorizon, Height, Width);

            // Advance each teacher by one step
            foreach (var agent in Agents)
            {
                var sensors = calculateAgentSensors(agent);

                agent.Step(sensors);
                applyToroidalAgentLocationRules(agent);
            }

            // Advance each predator by one step
            foreach (var predator in Predators)
            {
                var sensors = calculatePredatorSensors(predator);
                predator.Step(sensors);
                applyToroidalAgentLocationRules(predator);
            }

            // Make a separate pass over all the agents, now that they're in new locations
            // and determine who is on top of a plant and who is on top of a predator.
            foreach (var agent in Agents)
            {
                foreach (var predator in Predators)
                {
                    if (_sensorDictionary.getDistanceAndOrientation((int)agent.X, (int)agent.Y, (int)predator.X, (int)predator.Y)[0]
                        < agent.Radius && !agent.EatenByRecently(_step, 25, predator))
                    {
                        // Eat the agent
                        agent.EatenBy(predator, _step);

                        // Notify the predator that it received a reward
                        predator.ReceiveReward(agent.Reward);

                        // Notify the agent that it's been eaten.
                        agent.ReceiveReward(-agent.Reward);

                        // Notify listeners that we gobbled up this agent
                        onAgentEaten(predator, agent);
                    }
                }
                foreach (var plant in Plants)
                    if (_sensorDictionary.getDistanceAndOrientation((int)agent.X, (int)agent.Y, (int)plant.X, (int)plant.Y)[0]
                        < plant.Radius && plant.AvailableForEating(agent))
                    {
                        // Eat the plant
                        plant.EatenBy(agent, _step);

                        // Notify the teacher that it received a reward
                        agent.ReceiveReward(plant.Reward);

                        // Notify listeners that someone has eaten a plant.
                        onPlantEaten(agent, plant);
                    }

                agent.Fitness += StepReward;
            }

            // Notify listeners that the world has stepped and changed.
            onStepped(EventArgs.Empty);
            onChanged(EventArgs.Empty);
            _step++;
        }

        /// <summary>
        /// Forces the pred to stay on screen by wrapping them around the other side of
        /// the world if they run over an edge.
        /// </summary>
        private void applyToroidalAgentLocationRules(IAgent agent)
        {
            if (agent.X >= Width)
                agent.X -= Width;
            if (agent.Y > Height)
                agent.Y -= Height;
            if (agent.X < 0)
                agent.X += Width;
            if (agent.Y < 0)
                agent.Y += Height;
        }

        // Notify any listeners that the world stateActionPair has changed
        private void onChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        // Notify any listeners that the world stateActionPair has stepped forward
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

        private void onAgentEaten(Predator eater, IAgent agent)
        {
            if (AgentEaten != null)
                AgentEaten(this, eater, agent);
        }

        /// <summary>
        /// Resets the world by calling reset on each agents and plant,
        /// then generating a new layout for the plants.
        /// </summary>
        public void Reset()
        {
            foreach (var agent in Agents)
            {
                agent.X = Width / 2;
                agent.Y = Height / 2;
                agent.Orientation = 0;
                agent.Fitness = 0;
            }

            layoutPlants();

            _step = 0;

            // Notify any listeners that the world stateActionPair has changed
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

        // Lays out the plants uniformly random.
        private void uniformLayout()
        {
            foreach (var plant in Plants)
            {
                plant.Reset();
                // Uniform random
                plant.X = _random.Next(Width);
                plant.Y = _random.Next(Height);
            }
        }

        // Lays out the plants in a spiral pattern starting from the origin.
        private void spiralLayout()
        {
            int speciesIdx = 0;
            foreach (var species in PlantTypes)
            {
                double theta = _random.NextDouble() * 2 * Math.PI;
                double dtheta = (_random.NextDouble()) / 15 + .1;
                dtheta *= _random.Next(2) == 1 ? -1 : 1;
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

        // Lays out the plants in clusters of the same species.
        private void clusteredLayout()
        {
            int speciesIdx = 0;
            foreach (var species in PlantTypes)
            {
                int clusterRadius = _random.Next(20) + 20;
                int k = _random.Next(5) + 5;
                int curx = _random.Next(Width - 2 * clusterRadius) + clusterRadius;
                int cury = _random.Next(Height - 2 * clusterRadius) + clusterRadius;
                for (int i = 0; i < species.Count; i++)
                {
                    if (i % k == 0)
                    {
                        curx = _random.Next(Width - 2 * clusterRadius) + clusterRadius;
                        cury = _random.Next(Height - 2 * clusterRadius) + clusterRadius;
                    }

                    int x = curx + _random.Next(clusterRadius) - clusterRadius / 2;
                    int y = cury + _random.Next(clusterRadius) - clusterRadius / 2;
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
        public double[] calculateAgentSensors(IAgent agent)
        {
            // Each plant type has its own set of sensors, plus we have one sensor for the velocity input.
            double[] sensors = new double[PlantTypes.Count() * SENSORS_PER_OBJECT_TYPE + Predators.Count() * SENSORS_PER_OBJECT_TYPE + 1];

            sensors[0] = agent.Velocity / agent.MaxVelocity;

            // For every plant
            foreach (var plant in Plants)
            {
                // if the plant isn't available for eating then we do not activate the sensors
                if (!plant.AvailableForEating(agent))
                    continue;

                // Calculate the distance to the object from the teacher
                int[] distanceAndOrientation = _sensorDictionary.getDistanceAndOrientation((int)agent.X, (int)agent.Y, (int)plant.X, (int)plant.Y);
                int dist = distanceAndOrientation[0];
                int pos = distanceAndOrientation[1];

                // If it's too far away for the pred to see
                if (dist > AgentHorizon)
                    continue;

                // Identify the appropriate sensor
                int sIdx = getSensorIndex(agent, plant.Species.SpeciesId * SENSORS_PER_OBJECT_TYPE + 1, pos);

                if (sIdx == -1)
                    continue;

                // Add the signal strength for this plant to the sensor
                sensors[sIdx] += 1.0 - dist / AgentHorizon;
            }

            // For every predator
            foreach (var predator in Predators)
            {
                // Calculate the distance to the predator from the pred
                int[] distanceAndOrientation = _sensorDictionary.getDistanceAndOrientation((int)agent.X, (int)agent.Y, (int)predator.X, (int)predator.Y);
                int dist = distanceAndOrientation[0];
                int pos = distanceAndOrientation[1];

                // If it's too far away for the teacher to see
                if (dist > AgentHorizon)
                    continue;

                // Identify the appropriate sensor
                int sIdx = getSensorIndex(agent, PlantTypes.Count() * SENSORS_PER_OBJECT_TYPE + predator.AttackType, pos);

                if (sIdx == -1)
                    continue;

                // Add the signal strength for this plant to the sensor
                sensors[sIdx] += 1.0 - dist / AgentHorizon;
            }

            return sensors;
        }

        public double[] calculatePredatorSensors(Predator predator)
        {
            // Agents are sensed by predators, plus they have 1 velocity sensor
            double[] sensors = new double[SENSORS_PER_OBJECT_TYPE + 1];

            sensors[0] = predator.Velocity / predator.MaxVelocity;

            // For every plant
            foreach (var agent in Agents)
            {
                // if the plant isn't available for eating then we do not activate the sensors
                if (agent.HidingMode == predator.AttackType)
                    continue;

                // Calculate the distance to the pred from the predator
                int[] distanceAndOrientation = _sensorDictionary.getDistanceAndOrientation((int)predator.X, (int)predator.Y, (int)agent.X, (int)agent.Y);
                int dist = distanceAndOrientation[0];
                int pos = distanceAndOrientation[1];

                // If it's too far away for the predator to see
                if (dist > AgentHorizon)
                    continue;

                // Identify the appropriate sensor
                int sIdx = getSensorIndex(predator, 1, pos);

                if (sIdx == -1)
                    continue;

                // Add the signal strength for this plant to the sensor
                sensors[sIdx] += 1.0 - dist / AgentHorizon;
            }

            return sensors;
        }

        private int getSensorIndex(IAgent agent, int offset, int pos)
        {
            double sensorWidth = 180.0 / (double)SENSORS_PER_OBJECT_TYPE;
            double dtheta = pos - agent.Orientation;
            if (Math.Abs(pos - agent.Orientation) > Math.Abs(pos - (agent.Orientation + 360)))
                dtheta = pos - (agent.Orientation + 360);

            // If the plant's behind us
            if(dtheta < -90 || dtheta > 90)
                return -1;

            int idx = 0;
            for (double degrees = -90 + sensorWidth; degrees <= 90 + double.Epsilon; degrees += sensorWidth, idx++)
                if (degrees > dtheta)
                    return idx + offset;
            return -1;
        }
        #endregion
    }
}

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
        public const int SENSORS_PER_PLANT_TYPE = 8;
        public const int SENSORS_PER_WALL = 8;
        const int DEFAULT_AGENT_HORIZON = 100;
        private int _step;
        SensorDictionary _sensorDictionary;
		private const int wallRadius = 100;
        private const int MAX_NUM_WALLS = 50;
        private const bool isFlippingWalls = true;

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
        /// nom nom nom
        /// </summary>
        public IList<Wall> Walls { get; set; }

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
        #endregion

        #region Events and Delegates
        /// <summary>
        /// Event called when the stateActionPair of the world is changed.
        /// </summary>
        public event ChangedEventHandler Changed;
        public delegate void ChangedEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Event called when the stateActionPair of the world is advanced one step.
        /// </summary>
        public event StepEventHandler Stepped;
        public delegate void StepEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Event called when an agent eats a piece of food.
        /// </summary>
        public event PlantEatenHandler PlantEaten;
        public delegate void PlantEatenHandler(object sender, IAgent eater, Plant eaten);
        #endregion


        public World(IEnumerable<IAgent> agents, int height, int width, IEnumerable<PlantSpecies> species, 
             PlantLayoutStrategies layout = PlantLayoutStrategies.Uniform, int agentHorizon = DEFAULT_AGENT_HORIZON)
        {
            Agents = agents;
            PlantTypes = species;
            Height = height;
            Width = width;
            AgentHorizon = agentHorizon;
            

            // Randomly populate the world with plants
            Plants = new List<Plant>();
            Walls = new List<Wall>();
            foreach (var s in species)
                //for (int i = 0; i < s.Count; i++)
                for (int i = 0; i < 5; i++)
                    Plants.Add(new Plant(s));
            for (int n = 0; n < MAX_NUM_WALLS; n++)
                Walls.Add(new Wall(n));
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

            // Advance each agent by one step
            foreach (var agent in Agents)
            {
                var sensors = calculateSensors(agent);
				//calculateWallSensors(agent);

                float prevX = agent.X;
                float prevY = agent.Y;
                agent.Step(sensors);
                if (agent.X >= Width)
                    agent.X -= Width;
                if (agent.Y > Height)
                    agent.Y -= Height;
                if (agent.X < 0)
                    agent.X += Width;
                if (agent.Y < 0)
                    agent.Y += Height;

		        foreach(var wall in Walls){
			        if(wall.checkCollision(agent)){
                        agent.X = prevX;
                        agent.Y = prevY;
                        //agent.X -= agent.Velocity * (float)(Math.Cos(agent.Orientation * Math.PI / 180.0));
            	        //agent.Y -= agent.Velocity * (float)(Math.Sin(agent.Orientation * Math.PI / 180.0));
			        }
		}

            }

            // Make a separate pass over all the agents, now that they're in new locations
            // and determine who is on top of a plant.
            foreach (var agent in Agents)
                foreach (var plant in Plants)
                    if (_sensorDictionary.getDistanceAndOrientation((int)agent.X, (int)agent.Y, plant.X, plant.Y)[0]
                        < plant.Species.Radius && plant.AvailableForEating(agent))
                    {
                        // Eat the plant
                        plant.EatenBy(agent, _step);
                        agent.Fitness += plant.Species.Reward;

                        // Notify listeners that someone has eaten a plant.
                        onPlantEaten(agent, plant);
                    }

            // Notify listeners that the world has stepped and changed.
            onStepped(EventArgs.Empty);
            onChanged(EventArgs.Empty);
            _step++;
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
            layoutWalls();

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

        #region Walls Layouts
        private void layoutWalls()
        {
            bool createWall = true;
            int numWalls = 0;
            foreach (var plant in Plants)
            {
	            if(createWall && numWalls < MAX_NUM_WALLS){
					Wall wall = Walls[numWalls];
	                wall.Reset();
					// theta1.x = r * Cos(angle) + plant.x;
					// theta1.y = r * Sin(angle) + plant.y;
                    double theta = 0.0;
                    double theta2 = theta + 90;
                    wall.X1 = (float)(wallRadius * Math.Cos(theta) + plant.X);
                    wall.Y1 = (float)(wallRadius * Math.Sin(theta) + plant.Y);
                    wall.X2 = (float)(wallRadius * Math.Cos(theta2) + plant.X);
                    wall.Y2 = (float)(wallRadius * Math.Sin(theta2) + plant.Y);
	                numWalls++;
	            }
                if(isFlippingWalls)
	                createWall = !createWall;
            }
        }
        #endregion

        #region Helper methods for calculating mathy things
        public double[] calculateSensors(IAgent agent)
        {
            // Each plant type has its own set of sensors, plus we have one sensor for the velocity input.
            double[] sensors = new double[PlantTypes.Count() * (SENSORS_PER_PLANT_TYPE) + 1 + SENSORS_PER_WALL ];

            sensors[0] = agent.Velocity / agent.MaxVelocity;

            // For every plant
            foreach (var plant in Plants)
            {
                // if the plant isn't available for eating then we do not activate the sensors
                if (!plant.AvailableForEating(agent))
                    continue;

                // Calculate the distance to the object from the agent
                int[] distanceAndOrientation = _sensorDictionary.getDistanceAndOrientation((int)agent.X, (int)agent.Y, plant.X, plant.Y);
                int dist = distanceAndOrientation[0];
                int pos = distanceAndOrientation[1];

                // If it's too far away for the agent to see
                if (dist > AgentHorizon)
                    continue;

                // Identify the appropriate sensor
                int sIdx = getSensorIndex(agent, plant, pos);

                if (sIdx == -1)
                    continue;

                // Add the signal strength for this plant to the sensor
                sensors[sIdx] += 1.0 - dist / AgentHorizon;
            }

            foreach (var wall in Walls)
            {
                //get shortest point of a wall to the agent
                float shortestX = 0;
                float shortestY = 0;
                getShortestDistance(agent, wall, ref shortestX, ref shortestY);

                int[] distanceAndOrientation = _sensorDictionary.getDistanceAndOrientation((int)agent.X, (int)agent.Y, (int)shortestX, (int)shortestY);
                int dist = distanceAndOrientation[0];
                int pos = distanceAndOrientation[1];

                // If it's too far away for the agent to see
                if (dist > AgentHorizon)
                    continue;

                // Identify the appropriate sensor
                int sIdx = getWallSensorIndex(agent, wall, pos);

                if (sIdx == -1)
                    continue;

                // Add the signal strength for this wall to the sensor
                sensors[sIdx] += 1.0 - dist / AgentHorizon;
            }


            return sensors;
        }

        private int getSensorIndex(IAgent agent, Plant plant, int pos)
        {
            double sensorWidth = 180.0 / (double)SENSORS_PER_PLANT_TYPE;
            double dtheta = pos - agent.Orientation;
            if (Math.Abs(pos - agent.Orientation) > Math.Abs(pos - (agent.Orientation + 360)))
                dtheta = pos - (agent.Orientation + 360);

            // If the plant's behind us
            if(dtheta < -90 || dtheta > 90)
                return -1;

            int idx = 1;
            for (double degrees = -90 + sensorWidth; degrees <= 90 + double.Epsilon; degrees += sensorWidth, idx++)
                if (degrees > dtheta)
                    return idx + plant.Species.SpeciesId * SENSORS_PER_PLANT_TYPE;
            return -1;
        }

		private int getWallSensorIndex(IAgent agent, Wall wall, int pos)
        {
            double sensorWidth = 180.0 / (double)SENSORS_PER_WALL;
            double dtheta = pos - agent.Orientation;
            if (Math.Abs(pos - agent.Orientation) > Math.Abs(pos - (agent.Orientation + 360)))
                dtheta = pos - (agent.Orientation + 360);

            // If the plant's behind us
            if(dtheta < -90 || dtheta > 90)
                return -1;

			//might need to change index to start from end of the index of plant sensors
            int idx = PlantTypes.Count() * (SENSORS_PER_PLANT_TYPE) + 1;
            for (double degrees = -90 + sensorWidth; degrees <= 90 + double.Epsilon; degrees += sensorWidth, idx++)
                if (degrees > dtheta)
                    return idx;
            return -1;
        }

		private void getShortestDistance(IAgent agent, Wall wall, ref float shortestX, ref float shortestY)
		{
			wall.getFormula();
			shortestX = (wall.slope * agent.Y + agent.X - wall.slope * wall.b)/(wall.slope*wall.slope + 1);
			shortestY = wall.slope * shortestX + wall.b;
			if(!wall.checkRegion(shortestX, shortestY, 0, 0)){
				int[] distanceAndOrientation = _sensorDictionary.getDistanceAndOrientation((int)agent.X, (int)agent.Y, (int)wall.X1, (int)wall.Y1);	
				int[] distanceAndOrientation2 = _sensorDictionary.getDistanceAndOrientation((int)agent.X, (int)agent.Y, (int)wall.X2, (int)wall.Y2);
				int distanceXY1 = distanceAndOrientation[0];
				int distanceXY2 = distanceAndOrientation2[0];
				if(distanceXY1 > distanceXY2){
					shortestX = wall.X1;
					shortestY = wall.Y1;
				}
				else{
					shortestX = wall.X2;
					shortestY = wall.Y2;
				}
			}
		}

        #endregion
    }
}

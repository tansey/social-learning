using social_learning;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestWorld
{
    
    
    /// <summary>
    ///This is a test class for WorldTest and is intended
    ///to contain all WorldTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WorldTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //

        //Use TestInitialize to run code before running each test
        World _world;
        ActionListAgent _agent;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            int height = 500;
            int width = 500;
            List<PlantSpecies> species = new List<PlantSpecies>() { new PlantSpecies(0) { Radius = 5, Reward = 100, Count = 1 } };

            _agent = new ActionListAgent(0, new List<float[]>());
            List<IAgent> agents = new List<IAgent>() { _agent };

            _world = new World(agents, height, width, species, PlantLayoutStrategies.Uniform);

            
        }

        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for the sensor calculation
        ///</summary>
        [TestMethod()]
        public void BasicSensorTest()
        {
            /*
             * Layout the plant like so:
             *           P    
             *           
             *           x    
             *                 
             */
            double degrees = 5;
            _world.Plants[0] = new Plant(_world.PlantTypes.First()) { X = 250, Y = 270 };
            double[] sensors = _world.calculateSensors(_agent);
            // not moving
            Assert.AreEqual(sensors[0], 0);
            // on the far left
            Assert.AreEqual(sensors[1], 0.4, 0.03);
            Assert.AreEqual(sensors[2], 0);
            Assert.AreEqual(sensors[3], 0);
            Assert.AreEqual(sensors[4], 0);
            Assert.AreEqual(sensors[5], 0);
            Assert.AreEqual(sensors[6], 0);
            Assert.AreEqual(sensors[7], 0);
            Assert.AreEqual(sensors[8], 0);

            /*
             * Layout the plant like so (sensor 2):
             *              P 
             *              
             *           x    
             *                 
             */
            degrees += 22.5;
            _world.Plants[0] = new Plant(_world.PlantTypes.First()) { X = 250 + xOffset(degrees, 20), Y = 250 + yOffset(degrees, 20) };
            sensors = _world.calculateSensors(_agent);
            // not moving
            Assert.AreEqual(sensors[0], 0);
            Assert.AreEqual(sensors[1], 0);
            Assert.AreEqual(sensors[2], 0.4, 0.03);
            Assert.AreEqual(sensors[3], 0);
            Assert.AreEqual(sensors[4], 0);
            Assert.AreEqual(sensors[5], 0);
            Assert.AreEqual(sensors[6], 0);
            Assert.AreEqual(sensors[7], 0);
            Assert.AreEqual(sensors[8], 0);

            /*
             * Layout the plant like so (sensor 3):
             *                   P
             *                   
             *           x    
             *                 
             */
            degrees += 22.5;
            _world.Plants[0] = new Plant(_world.PlantTypes.First()) { X = 250 + xOffset(degrees, 20), Y = 250 + yOffset(degrees, 20) };
            sensors = _world.calculateSensors(_agent);
            // not moving
            Assert.AreEqual(sensors[0], 0);
            Assert.AreEqual(sensors[1], 0);
            Assert.AreEqual(sensors[2], 0);
            Assert.AreEqual(sensors[3], 0.4, 0.03);
            Assert.AreEqual(sensors[4], 0);
            Assert.AreEqual(sensors[5], 0);
            Assert.AreEqual(sensors[6], 0);
            Assert.AreEqual(sensors[7], 0);
            Assert.AreEqual(sensors[8], 0);

            /*
             * Layout the plant like so (sensor 4):
             *                   
             *                    P
             *           x    
             *                 
             */
            degrees += 22.5;
            _world.Plants[0] = new Plant(_world.PlantTypes.First()) { X = 250 + xOffset(degrees, 20), Y = 250 + yOffset(degrees, 20) };
            sensors = _world.calculateSensors(_agent);
            // not moving
            Assert.AreEqual(sensors[0], 0);
            Assert.AreEqual(sensors[1], 0);
            Assert.AreEqual(sensors[2], 0);
            Assert.AreEqual(sensors[3], 0);
            Assert.AreEqual(sensors[4], 0.4, 0.03);
            Assert.AreEqual(sensors[5], 0);
            Assert.AreEqual(sensors[6], 0);
            Assert.AreEqual(sensors[7], 0);
            Assert.AreEqual(sensors[8], 0);

            /*
             * Layout the plant like so (sensor 5):
             *                   
             *                    
             *           x    
             *                   P
             */
            degrees += 22.5;
            _world.Plants[0] = new Plant(_world.PlantTypes.First()) { X = 250 + xOffset(degrees, 20), Y = 250 + yOffset(degrees, 20) };
            sensors = _world.calculateSensors(_agent);
            // not moving
            Assert.AreEqual(sensors[0], 0);
            Assert.AreEqual(sensors[1], 0);
            Assert.AreEqual(sensors[2], 0);
            Assert.AreEqual(sensors[3], 0);
            Assert.AreEqual(sensors[4], 0);
            Assert.AreEqual(sensors[5], 0.4, 0.03);
            Assert.AreEqual(sensors[6], 0);
            Assert.AreEqual(sensors[7], 0);
            Assert.AreEqual(sensors[8], 0);

            /*
             * Layout the plant like so (sensor 6):
             *                   
             *                    
             *           x    
             *                 
             * 
             *                  P
             */                
            degrees += 22.5;
            _world.Plants[0] = new Plant(_world.PlantTypes.First()) { X = 250 + xOffset(degrees, 20), Y = 250 + yOffset(degrees, 20) };
            sensors = _world.calculateSensors(_agent);
            // not moving
            Assert.AreEqual(sensors[0], 0);
            Assert.AreEqual(sensors[1], 0);
            Assert.AreEqual(sensors[2], 0);
            Assert.AreEqual(sensors[3], 0);
            Assert.AreEqual(sensors[4], 0);
            Assert.AreEqual(sensors[5], 0);
            Assert.AreEqual(sensors[6], 0.4, 0.03);
            Assert.AreEqual(sensors[7], 0);
            Assert.AreEqual(sensors[8], 0);

            /*
             * Layout the plant like so (sensor 7):
             *                   
             *                    
             *           x    
             *                 
             * 
             *               P  
             */
            degrees += 22.5;
            _world.Plants[0] = new Plant(_world.PlantTypes.First()) { X = 250 + xOffset(degrees, 20), Y = 250 + yOffset(degrees, 20) };
            sensors = _world.calculateSensors(_agent);
            // not moving
            Assert.AreEqual(sensors[0], 0);
            Assert.AreEqual(sensors[1], 0);
            Assert.AreEqual(sensors[2], 0);
            Assert.AreEqual(sensors[3], 0);
            Assert.AreEqual(sensors[4], 0);
            Assert.AreEqual(sensors[5], 0);
            Assert.AreEqual(sensors[6], 0);
            Assert.AreEqual(sensors[7], 0.4, 0.03);
            Assert.AreEqual(sensors[8], 0);

            /*
             * Layout the plant like so (sensor 8):
             *                   
             *                    
             *           x    
             *                 
             * 
             *           P
             */
            degrees = 175;
            _world.Plants[0] = new Plant(_world.PlantTypes.First()) { X = 250 + xOffset(degrees, 20), Y = 250 + yOffset(degrees, 20) };
            sensors = _world.calculateSensors(_agent);
            // not moving
            Assert.AreEqual(sensors[0], 0);
            Assert.AreEqual(sensors[1], 0);
            Assert.AreEqual(sensors[2], 0);
            Assert.AreEqual(sensors[3], 0);
            Assert.AreEqual(sensors[4], 0);
            Assert.AreEqual(sensors[5], 0);
            Assert.AreEqual(sensors[6], 0);
            Assert.AreEqual(sensors[7], 0);
            Assert.AreEqual(sensors[8], 0.4, 0.03);
        }

        private static int xOffset(double degrees, double distance)
        {
            return (int)Math.Round(Math.Sin(degrees.ToRadians()) * distance);
        }

        private static int yOffset(double degrees, double distance)
        {
            return (int)Math.Round(Math.Cos(degrees.ToRadians()) * distance);
        }
    }
}

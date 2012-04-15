using social_learning;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestWall
{
    
    
    /// <summary>
    ///This is a test class for WallTest and is intended
    ///to contain all WallTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WallTest
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
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for getFormula
        ///</summary>
        [TestMethod()]
        public void getFormulaTestPostiveSlope()
        {
            int id = 0;
            Wall target = new Wall(id); // TODO: Initialize to an appropriate value
            float X1 = 0F; // TODO: Initialize to an appropriate value
            float Y1 = 0F; // TODO: Initialize to an appropriate value
            float X2 = 100F; // TODO: Initialize to an appropriate value
            float Y2 = 100F;
            
            float[] expected = new float[2]{1F,0F};
            float[] actual;
            actual = target.getFormula(X1, Y1, X2, Y2);

            Assert.IsTrue(expected[0] - actual[0] < float.Epsilon * 5);
            Assert.IsTrue(expected[1] - actual[1] < float.Epsilon * 5);

            X1 = 100F;
            Y1 = 100F;
            X2 = 0F;
            Y2 = 0F;
            expected = new float[2] { 1F, 0F };
            actual = target.getFormula(X1, Y1, X2, Y2);

            Assert.IsTrue(expected[0] - actual[0] < float.Epsilon * 5);
            Assert.IsTrue(expected[1] - actual[1] < float.Epsilon * 5);

        }

        /// <summary>
        ///A test for getFormula
        ///</summary>
        [TestMethod()]
        public void getFormulaTestNegativeSlope()
        {
            int id = 0; 
            Wall target = new Wall(id); 
            float X1 = 100F; 
            float Y1 = 0F; 
            float X2 = 0F; 
            float Y2 = 100F; 

            float[] expected = new float[2] { -1F, 100F }; 
            float[] actual;
            actual = target.getFormula(X1, Y1, X2, Y2);

            Assert.IsTrue(expected[0] - actual[0] < float.Epsilon * 5);
            Assert.IsTrue(expected[1] - actual[1] < float.Epsilon * 5);

        }

        /// <summary>
        ///A test for getFormula
        ///</summary>
        [TestMethod()]
        public void getFormulaTestInfinitySlope()
        {
            int id = 0;
            Wall target = new Wall(id);
            float X1 = 0F;
            float Y1 = 0F;
            float X2 = 0F;
            float Y2 = 100F;

            float[] actual;
            actual = target.getFormula(X1, Y1, X2, Y2);

            Assert.IsTrue(float.IsPositiveInfinity(actual[0]));
            //0 * Infinity is not a number.
            Assert.IsTrue(float.IsNaN(actual[1]));

            X1 = 5F;
            Y1 = 0F;
            X2 = 5F;
            Y2 = 100F;
            actual = target.getFormula(X1, Y1, X2, Y2);

            Assert.IsTrue(float.IsPositiveInfinity(actual[0]));
            Assert.IsTrue(float.IsNegativeInfinity(actual[1]));

            X1 = 100F;
            Y1 = 100F;
            X2 = 100F;
            Y2 = 1F;
            actual = target.getFormula(X1, Y1, X2, Y2);

            Assert.IsTrue(float.IsNegativeInfinity(actual[0]));
            Assert.IsTrue(float.IsPositiveInfinity(actual[1]));

        }

        /// <summary>
        ///A test for getFormula
        ///</summary>
        [TestMethod()]
        public void getFormulaTestZeroSlope()
        {
            int id = 0;
            Wall target = new Wall(id);
            float X1 = 0F;
            float Y1 = 0F;
            float X2 = 100F;
            float Y2 = 0F;

            float[] expected = new float[2] { 0F, 0F };
            float[] actual;
            actual = target.getFormula(X1, Y1, X2, Y2);

            Assert.IsTrue(expected[0] - actual[0] < float.Epsilon * 5);
            Assert.IsTrue(expected[1] - actual[1] < float.Epsilon * 5);
        }

        /// <summary>
        ///A test for checkCollision
        ///</summary>
        [TestMethod()]
        public void checkCollisionTest()
        {
            //move to right
            int id = 0;
            Wall target = new Wall(id);
            target.X1 = 10f;
            target.Y1 = 10f;
            target.X2 = 20f;
            target.Y2 = 20f;
            bool expected = true;
            bool actual;
            float agentX = 15f;
            float agentY = 15f;
            float agentPrevX = 14f;
            float agentPrevY = 15f;

            actual = target.checkCollision(agentX,agentY,agentPrevX,agentPrevY);
            Assert.AreEqual(expected, actual);
        }
    }
}

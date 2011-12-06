﻿using social_learning;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SharpNeat.Phenomes;

namespace TestWorld
{
    
    
    /// <summary>
    ///This is a test class for NeuralAgentTest and is intended
    ///to contain all NeuralAgentTest Unit Tests
    ///</summary>
    [TestClass()]
    public class NeuralAgentTest
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
        ///A test for getRotationAndVelocity
        ///</summary>
        [TestMethod()]
        [DeploymentItem("social_learning.dll")]
        public void getRotationAndVelocityTest()
        {
            PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            NeuralAgent_Accessor target = new NeuralAgent_Accessor(param0); // TODO: Initialize to an appropriate value
            double[] sensors = null; // TODO: Initialize to an appropriate value
            float[] expected = null; // TODO: Initialize to an appropriate value
            float[] actual;
            actual = target.getRotationAndVelocity(sensors);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
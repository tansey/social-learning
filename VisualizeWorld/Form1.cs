using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using social_learning;
using System.Threading;
using System.Xml;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using System.IO;
using SharpNeat.Core;
using SharpNeat.Phenomes;

namespace VisualizeWorld
{
    public partial class Form1 : Form
    {
        Color[] plantColors;
        Color[] agentColors;
        Random random = new Random();
        static NeatEvolutionAlgorithm<NeatGenome> _ea;
        const string CHAMPION_FILE = @"..\..\..\experiments\simple_evolution_champion.xml";
        static SocialExperiment _experiment;
        static PlantLayoutStrategies _plantLayout;
        const string NEURAL_CONFIG_FILE = @"..\..\..\experiments\neural.config.xml";
        const string QLEARNING_CONFIG_FILE = @"..\..\..\experiments\qlearning.config.xml";
        const string QLEARNING_FEED_FORWARD_NETWORK_FILE = @"..\..\..\experiments\qlearning_network.xml";
        const string SOCIAL_DARWIN_CONFIG_FILE = @"..\..\..\experiments\social_darwin.config.xml";
        const string SOCIAL_LAMARK_CONFIG_FILE = @"..\..\..\experiments\social_lamark.config.xml";
        const string CONTROLLED_CONFIG_FILE = @"..\..\..\experiments\controlled.config.xml";
        string _configFile = NEURAL_CONFIG_FILE;
        Thread qLearningThread;
        bool running = false;
        bool _debugOutputs = false;

        public Form1()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.DoubleBuffer, true);
            
            const int NUM_AGENTS = 500;
            plantColors = new Color[NUM_AGENTS];
            agentColors = new Color[NUM_AGENTS];
            for (int i = 0; i < NUM_AGENTS; i++)
            {
                agentColors[i] = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
                plantColors[i] = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
            }
            plantColors[0] = Color.Pink;
            plantColors[1] = Color.Lavender;
            plantColors[2] = Color.Gray;
            plantColors[3] = Color.ForestGreen;
            plantColors[4] = Color.LimeGreen;

            this.Disposed += new EventHandler(Form1_Disposed);
        }

        void Form1_Disposed(object sender, EventArgs e)
        {
            if (qLearningThread != null)
                qLearningThread.Abort();
        }

        int gens = 0;

        // If the world has changed either because of a new generation or because the world has stepped forward,
        // redraw the world and any other stats we are displaying.
        void world_Changed(object sender, EventArgs e)
        {
            if (this.InvokeRequired == false)
                this.Invalidate();
            else
            {
                //!!!!!SPEED: Slow down
                Thread.Sleep(200);
                this.BeginInvoke(new worldChangedDelegate(world_Changed), new object[] { sender, e });
            }
        }

        delegate void worldChangedDelegate(object s, EventArgs e);

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (_experiment == null || _experiment.World == null || !running)
                return;
            
            Graphics g = e.Graphics;
            World world = _experiment.World;

            float scaleX = this.ClientRectangle.Width / (float)world.Width;
            float scaleY = this.ClientRectangle.Height / (float)world.Height;

            //!!!!!agent number
            var agents = world.Agents.Take(1);

            // Draw the plants
            foreach (var plant in world.Plants)
            {
                // Skip drawing a plant if it's been eaten. This is to help us debug the sensor values.
                if (_configFile == QLEARNING_CONFIG_FILE && plant.EaterCount > 0)
                    continue;

                SolidBrush brush = null;

                foreach(var agent in agents)
                    try
                    {
                        if (plant.EatenByRecently(world.CurrentStep, 10, agent))
                        {
                            brush = new SolidBrush(Color.Red);
                            break;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                if(brush == null)
                    brush = new SolidBrush(plantColors[plant.Species.SpeciesId]);

                g.FillEllipse(brush, new Rectangle((int)((plant.X - plant.Species.Radius) * scaleX),
                                                   (int)((plant.Y - plant.Species.Radius) * scaleY),
                                                   (int)((plant.Species.Radius * 2) * scaleX),
                                                   (int)((plant.Species.Radius * 2) * scaleY)));
                brush.Dispose();
            }

			// Draw the walls
			foreach (var wall in world.Walls)
			{
				Pen myPen = new Pen(Color.Black, 5);

		    	g.DrawLine(myPen, (float)wall.X1 * scaleX, (float)wall.Y1 * scaleY, (float)wall.X2 * scaleX, (float)wall.Y2 * scaleY);
			}

            // Draw the _agents
            int i = -1;
            foreach (var agent in agents)
            {
                i++;
                //if (i % 10 != 0)
                //    continue;

                //var agent = world.Agents.First();
                g.FillPie(new SolidBrush(agentColors[i]), new Rectangle((int)((agent.X - _experiment.World.AgentHorizon / 4.0) * scaleX),
                                                    (int)((agent.Y - _experiment.World.AgentHorizon / 4.0) * scaleY),
                                                    (int)(_experiment.World.AgentHorizon * 2 / 4.0 * scaleX),
                                                    (int)(_experiment.World.AgentHorizon * 2 / 4.0 * scaleY)),
                                                    agent.Orientation - 90,
                                                    180);
                g.DrawEllipse(new Pen(Color.Black), new Rectangle((int)((agent.X - 3) * scaleX),
                                    (int)((agent.Y - 3) * scaleY),
                                    (int)(6 * scaleX),
                                    (int)(6 * scaleY)));
            }

            // Draw the fitness scores
            g.FillRectangle(Brushes.White, 0, 30, 150, 50);
            g.DrawString(string.Format("Gen: {0} Best: {1} Agent1: {2} Average: {3} Orien: {4}", gens, world.Agents.Max(a => a.Fitness), world.Agents.First().Fitness, world.Agents.Average(a => a.Fitness), world.Agents.First().Orientation), 
                                            DefaultFont, Brushes.Black, 0, 30);
            g.DrawString(string.Format("\r\nX: {0} Y: {1} wallX1: {2} wallX2: {3} intersect: {4} collide: {5}", Math.Ceiling(world.Agents.First().X), Math.Ceiling(world.Agents.First().Y), world.Walls.First().X1, world.Walls.First().X2, Math.Ceiling(world.Walls.First().intersect), world.collide),
                                            DefaultFont, Brushes.Black, 0, 30);
            g.DrawString(_ea == null ? "" : _ea.ComplexityRegulationMode.ToString(), DefaultFont, Brushes.Black, 0, 50);


            // Draw the network inputs and outputs for the Q-Learning agent
            if (_debugOutputs)
            {
                g.FillRectangle(Brushes.White, 0, 50, 100, 150);
                var agent = (QLearningAgent)_experiment.World.Agents.First();
                for (int x = 0; x < agent._prevState.Length; x++)
                    g.DrawString(string.Format("[{0}] = {1:N4}", x, agent._prevState[x]), DefaultFont,
                        Brushes.Black, 0, 50 + x * 15);
                g.DrawString(string.Format("Orientation: {0}", agent.Orientation), DefaultFont, Brushes.Black, 0, 50 + 15 * agent._prevState.Length);
            }
        }


        private void evolve_Click(object sender, EventArgs e)
        {
            if (btnEvolve.Text == "Stop!")
            {
                stopEvolution();
                return;
            }
            running = true;
            _experiment = new SocialExperiment();
            
            // Load config XML.
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(_configFile);
            _experiment.Initialize("SimpleEvolution", xmlConfig.DocumentElement);
            _experiment.PlantLayout = _plantLayout;

            _experiment.World.Changed += new World.ChangedEventHandler(world_Changed);

            btnEvolve.Text = "Stop!";

            // Start the evolution
            if (_configFile == QLEARNING_CONFIG_FILE)
            {
                qLearningThread = new Thread(new ThreadStart(startQLearning));
                qLearningThread.Start();
            }
            else
            {
                qLearningThread = new Thread(new ThreadStart(startEvolution));
                qLearningThread.Start();
            }
        }

        private void pause_Click(object sender, EventArgs e)
        {
            if (btnPause.Text == "Pause")
            {
                pauseEvolution();
                return;
            }
        }

        private void startQLearning()
        {
            SocialExperiment.CreateNetwork(QLEARNING_FEED_FORWARD_NETWORK_FILE, _experiment.InputCount, 20, _experiment.OutputCount);

            // Read in the agent genome from file.
            var agentGenome = _experiment.LoadPopulation(XmlReader.Create(QLEARNING_FEED_FORWARD_NETWORK_FILE));

            // Create genome decoder.
            IGenomeDecoder<NeatGenome, IBlackBox> genomeDecoder = _experiment.CreateGenomeDecoder();

            // Create the evaluator that will handle the simulation
            _experiment.Evaluator = new ForagingEvaluator<NeatGenome>(genomeDecoder, _experiment.World, AgentTypes.QLearning)
            {
                MaxTimeSteps = 50000000UL,
                BackpropEpochsPerExample = 1
            };

            // Add the world reset
            _experiment.World.Stepped += new World.StepEventHandler(World_Stepped);

            // Start the simulation
            _experiment.Evaluator.Evaluate(agentGenome);
        }

        
        void World_Stepped(object sender, EventArgs e)
        {
            if (_experiment.World.CurrentStep > 0 && _experiment.World.CurrentStep % (int)_experiment.TimeStepsPerGeneration == 0)
                _experiment.World.Reset();
        }

        private void startEvolution()
        {
            gens = 0;


            // Create evolution algorithm and attach update event.
            _ea = _experiment.CreateEvolutionAlgorithm();
            _ea.UpdateEvent += new EventHandler(ea_UpdateEvent);

            // Start algorithm (it will run on a background thread).
            _ea.StartContinue();
        }

        void ea_UpdateEvent(object sender, EventArgs e)
        {
            Console.WriteLine(string.Format("gen={0:N0} bestFitness={1:N6}", _ea.CurrentGeneration, _ea.Statistics._maxFitness));
            
            this.BeginInvoke(new incDel(incrementGens));
            //MessageBox.Show("New Gen! Best individual: " + _ea.CurrentChampGenome.EvaluationInfo.AlternativeFitness);

            // Save the best genome to file
            var doc = NeatGenomeXmlIO.SaveComplete(new List<NeatGenome>() { _ea.CurrentChampGenome }, false);
            doc.Save(CHAMPION_FILE);
        }
        delegate void incDel();
        private void incrementGens()
        {
            gens++;

        }

        private void stopEvolution()
        {
            if (!running)
                return;

            if (_ea != null)
                _ea.Stop();
            if(qLearningThread != null)
                qLearningThread.Abort();
            btnEvolve.Text = "Evolve!";
            running = false;
        }

        // TODO
        private void pauseEvolution()
        {
            
        }

        #region Change the plant layout strategy
        private void clusterToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (clusterToolStripMenuItem.Checked)
            {
                _plantLayout = PlantLayoutStrategies.Clustered;
                if(_experiment != null)
                    _experiment.World.PlantLayoutStrategy = PlantLayoutStrategies.Clustered;
                uniformdefaultToolStripMenuItem.Checked = false;
                spiralToolStripMenuItem.Checked = false;
            }
        }

        private void uniformdefaultToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (uniformdefaultToolStripMenuItem.Checked)
            {
                _plantLayout = PlantLayoutStrategies.Uniform;
                if(_experiment != null)
                    _experiment.World.PlantLayoutStrategy = PlantLayoutStrategies.Uniform;
                clusterToolStripMenuItem.Checked = false;
                spiralToolStripMenuItem.Checked = false;
            }
        }

        private void spiralToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (spiralToolStripMenuItem.Checked)
            {
                _plantLayout = PlantLayoutStrategies.Spiral;
                if(_experiment != null)
                    _experiment.World.PlantLayoutStrategy = PlantLayoutStrategies.Spiral;
                uniformdefaultToolStripMenuItem.Checked = false;
                clusterToolStripMenuItem.Checked = false;
            }
        }
        #endregion

        #region Change the experiment setup
        private void qLearningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllExperimentMenusAndStopEvolution();
            _configFile = QLEARNING_CONFIG_FILE;
            qLearningToolStripMenuItem.Checked = true;
        }

        private void basicNEATdefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllExperimentMenusAndStopEvolution();
            _configFile = NEURAL_CONFIG_FILE;
            basicNEATdefaultToolStripMenuItem.Checked = true;
        }

        private void socialLearningDarwinianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllExperimentMenusAndStopEvolution();
            _configFile = SOCIAL_DARWIN_CONFIG_FILE;
            socialLearningDarwinianToolStripMenuItem.Checked = true;
        }

        private void socialLearningLamarkianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            uncheckAllExperimentMenusAndStopEvolution();
            _configFile = SOCIAL_LAMARK_CONFIG_FILE;
            socialLearningLamarkianToolStripMenuItem.Checked = true;
        }

        private void Controlled_Click(object sender, EventArgs e)
        {
            uncheckAllExperimentMenusAndStopEvolution();
            _configFile = CONTROLLED_CONFIG_FILE;
            Controlled.Checked = true;
        }

        private void uncheckAllExperimentMenusAndStopEvolution()
        {
            stopEvolution();

            basicNEATdefaultToolStripMenuItem.Checked = false;
            qLearningToolStripMenuItem.Checked = false;
            socialLearningDarwinianToolStripMenuItem.Checked = false;
            socialLearningLamarkianToolStripMenuItem.Checked = false;
            Controlled.Checked = false;
        }
        #endregion

        private void debugOutputsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _debugOutputs = debugOutputsToolStripMenuItem.Checked;
        }
    }
}

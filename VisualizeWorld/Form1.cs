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

namespace VisualizeWorld
{
    public partial class Form1 : Form
    {
        Color[] plantColors;
        Color[] agentColors;
        Random random = new Random();
        static NeatEvolutionAlgorithm<NeatGenome> _ea;
        const string CHAMPION_FILE = @"..\..\..\experiments\simple_evolution_champion.xml";
        static SimpleExperiment _experiment;

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
            
        }

        int gens = 0;
        const int STARTER_GENS = 0;

        // If the world has changed either because of a new generation or because the world has stepped forward,
        // redraw the world and any other stats we are displaying.
        void world_Changed(object sender, EventArgs e)
        {
            if (this.InvokeRequired == false)
            {
                //if (gens > STARTER_GENS)
                    this.Invalidate();
            }
            else
            {
                if (gens > STARTER_GENS)
                    Thread.Sleep(10);
                this.BeginInvoke(new worldChangedDelegate(world_Changed), new object[] { sender, e });
            }
        }

        delegate void worldChangedDelegate(object s, EventArgs e);

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (_experiment == null || _experiment.World == null)
                return;

            Graphics g = e.Graphics;
            World world = _experiment.World;

            if (gens < STARTER_GENS)
            {
                g.FillRectangle(Brushes.White, 0, 0, 200, 15);

                g.DrawString(string.Format("Gen: {0} Best: {1} Agent1: {2} Average: {3}", gens, world.Agents.Max(a => a.Fitness), world.Agents.First().Fitness, world.Agents.Average(a => a.Fitness)),
                                                DefaultFont, Brushes.Black, 0, 0);
                return;
            }

            float scaleX = e.ClipRectangle.Width / (float)world.Width;
            float scaleY = e.ClipRectangle.Height / (float)world.Height;

            var agents = world.Agents.Take(15);

            // Draw the plants
            foreach (var plant in world.Plants)
            {
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

            g.FillRectangle(Brushes.White, 0, 30, 150, 15);

            g.DrawString(string.Format("Gen: {0} Best: {1} Agent1: {2} Average: {3}", gens, world.Agents.Max(a => a.Fitness), world.Agents.First().Fitness, world.Agents.Average(a => a.Fitness)),
                                            DefaultFont, Brushes.Black, 0, 30);
        
        }

        private void stepButton_Click(object sender, EventArgs e)
        {
            _experiment.World.Step();
            this.Invalidate();
        }

        private void evolve_Click(object sender, EventArgs e)
        {
            if (btnEvolve.Text == "Stop!")
            {
                stopEvolution();
                return;
            }

            _experiment = new SimpleExperiment();
            
            // Load config XML.
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load(@"..\..\..\experiments\social.config.xml");
            _experiment.Initialize("SimpleEvolution", xmlConfig.DocumentElement);

            _experiment.World.Changed += new World.ChangedEventHandler(world_Changed);
            
            btnEvolve.Text = "Stop!";
            btnStep.Enabled = false;
            // Start the evolution
            //evoThread = new Thread(new ThreadStart(startEvolution));
            //evoThread.Start();
            startEvolution();
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
            btnEvolve.Text = "Evolve!";
            btnStep.Enabled = true;
            _ea.Stop();
        }

        #region Change the plant layout strategy
        private void clusterToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (clusterToolStripMenuItem.Checked)
            {
                _experiment.World.PlantLayoutStrategy = PlantLayoutStrategies.Clustered;
                uniformdefaultToolStripMenuItem.Checked = false;
                spiralToolStripMenuItem.Checked = false;
            }
        }

        private void uniformdefaultToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (uniformdefaultToolStripMenuItem.Checked)
            {
                _experiment.World.PlantLayoutStrategy = PlantLayoutStrategies.Uniform;
                clusterToolStripMenuItem.Checked = false;
                spiralToolStripMenuItem.Checked = false;
            }
        }

        private void spiralToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (spiralToolStripMenuItem.Checked)
            {
                _experiment.World.PlantLayoutStrategy = PlantLayoutStrategies.Spiral;
                uniformdefaultToolStripMenuItem.Checked = false;
                clusterToolStripMenuItem.Checked = false;
            }
        }
        #endregion
    }
}

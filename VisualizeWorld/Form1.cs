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

namespace VisualizeWorld
{
    public partial class Form1 : Form
    {
        Color[] plantColors;
        Color[] agentColors;
        Random random = new Random();
        static NeatEvolutionAlgorithm<NeatGenome> _ea;
        const string CHAMPION_FILE = @"simple_evolution_champion.xml";
        static SimpleExperiment _experiment;

        public Form1()
        {
            InitializeComponent();
            _experiment = new SimpleExperiment();
            _experiment.World.Changed += new World.ChangedEventHandler(world_Changed);
            const int NUM_AGENTS = 500;
            plantColors = new Color[NUM_AGENTS]; 
            agentColors = new Color[NUM_AGENTS];
            for (int i = 0; i < NUM_AGENTS; i++)
            {
                agentColors[i] = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
                plantColors[i] = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
            }
            /*
            var species = new List<PlantSpecies>();
            for (int i = 0; i < 5; i++)
                species.Add(new PlantSpecies() { Name = "Species_" + i, Radius = 10, Reward = i });
            plantColors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Orange, Color.HotPink };

            var agents = new List<IAgent>();
            const int NUM_AGENTS = 10;
            agentColors = new Color[NUM_AGENTS];
            for (int i = 0; i < NUM_AGENTS; i++)
            {
                agents.Add(new SpinningAgent() { X = random.Next(500), Y = random.Next(500), Orientation = random.Next(360) });
                agentColors[i] = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
            }
            world = new World(agents, species, 500, 500, 10);

            world.Changed += new World.ChangedEventHandler(world_Changed);
             */

        }

        // If the world has changed either because of a new generation or because the world has stepped forward,
        // redraw the world and any other stats we are displaying.
        void world_Changed(object sender, EventArgs e)
        {
            if (this.InvokeRequired == false)
                this.Invalidate();
            else
            {
                Thread.Sleep(10);
                this.BeginInvoke(new worldChangedDelegate(world_Changed), new object[] { sender, e });
            }
        }

        delegate void worldChangedDelegate(object s, EventArgs e);

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            World world = _experiment.World;

            float scaleX = e.ClipRectangle.Width / (float)world.Width;
            float scaleY = e.ClipRectangle.Height / (float)world.Height;

            // Draw the plants
            foreach (var plant in world.Plants)
            {
                if (!plant.AvailableForEating(world.Agents.First()))
                    continue;

                var brush = new SolidBrush(plantColors[plant.Species.SpeciesId]);
                g.FillEllipse(brush, new Rectangle((int)((plant.X - plant.Species.Radius) * scaleX),
                                                   (int)((plant.Y - plant.Species.Radius) * scaleY),
                                                   (int)((plant.Species.Radius * 2) * scaleX),
                                                   (int)((plant.Species.Radius * 2) * scaleY)));
                brush.Dispose();
            }

            // Draw the agents
            int i = 0;
            foreach (var agent in world.Agents)
            {
                g.FillPie(new SolidBrush(agentColors[i]), new Rectangle((int)((agent.X - 10) * scaleX),
                                                   (int)((agent.Y - 10) * scaleY),
                                                   (int)(20 * scaleX),
                                                   (int)(20 * scaleY)),
                                                   agent.Orientation - 90,
                                                   180);
                g.DrawEllipse(new Pen(Color.Black), new Rectangle((int)((agent.X - 3) * scaleX),
                                   (int)((agent.Y - 3) * scaleY),
                                   (int)(6 * scaleX),
                                   (int)(6 * scaleY)));
                i++;
            }
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
            
            btnEvolve.Text = "Stop!";
            btnStep.Enabled = false;
            // Start the evolution
            //evoThread = new Thread(new ThreadStart(startEvolution));
            //evoThread.Start();
            startEvolution();
        }

        private void startEvolution()
        {

            // Load config XML.
            XmlDocument xmlConfig = new XmlDocument();
            xmlConfig.Load("simple.config.xml");
            _experiment.Initialize("SimpleEvolution", xmlConfig.DocumentElement);

            // Create evolution algorithm and attach update event.
            _ea = _experiment.CreateEvolutionAlgorithm();
            _ea.UpdateEvent += new EventHandler(ea_UpdateEvent);

            // Start algorithm (it will run on a background thread).
            _ea.StartContinue();
        }

        static void ea_UpdateEvent(object sender, EventArgs e)
        {
            Console.WriteLine(string.Format("gen={0:N0} bestFitness={1:N6}", _ea.CurrentGeneration, _ea.Statistics._maxFitness));
            MessageBox.Show("New Gen!");
            // Save the best genome to file
            var doc = NeatGenomeXmlIO.SaveComplete(new List<NeatGenome>() { _ea.CurrentChampGenome }, false);
            doc.Save(CHAMPION_FILE);
        }

        private void stopEvolution()
        {
            btnEvolve.Text = "Evolve!";
            btnStep.Enabled = true;
            _ea.Stop();
        }
    }
}

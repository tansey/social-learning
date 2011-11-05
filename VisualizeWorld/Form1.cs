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

namespace VisualizeWorld
{
    public partial class Form1 : Form
    {
        World world;
        Color[] plantColors;
        Color[] agentColors;
        Random random = new Random();

        public Form1()
        {
            InitializeComponent();

            var species = new List<PlantSpecies>();
            for (int i = 0; i < 5; i++)
                species.Add(new PlantSpecies() { Name = "Species_" + i, Radius = 10, Reward = i });
            plantColors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Orange, Color.HotPink };

            var agents = new List<IAgent>();
            const int NUM_AGENTS = 1;
            agentColors = new Color[NUM_AGENTS];
            for (int i = 0; i < NUM_AGENTS; i++)
            {
                agents.Add(new DumbAgent() { X = random.Next(500), Y = random.Next(500), Orientation = random.Next(360) });
                agentColors[i] = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
            }
            world = new World(agents, species, 500, 500, 10);

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

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
                var brush = new SolidBrush(agentColors[i]);
                g.FillClosedCurve(brush, new PointF[] { new PointF(agent.X * scaleX, agent.Y * scaleY),
                new PointF((agent.X - 5) * scaleX, (agent.Y - 10) * scaleY),
                new PointF((agent.X + 5) * scaleX, (agent.Y - 10) * scaleY)
                });
                //g.RotateTransform(agent.Orientation);
                i++;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            world.Step();
            this.Invalidate();
        }
    }
}

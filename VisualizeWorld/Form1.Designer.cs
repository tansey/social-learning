namespace VisualizeWorld
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnEvolve = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadPopulationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setAgentTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.basicNEATdefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.qLearningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.socialLearningDarwinianToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.socialLearningLamarkianToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox2 = new System.Windows.Forms.ToolStripTextBox();
            this.basicNEATnoLearningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.socialPreyOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.socialPredatorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.socialPreyAndPredatorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.proportionalLearningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setPlantLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uniformdefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spiralToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clusterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugOutputsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnEvolve
            // 
            this.btnEvolve.BackColor = System.Drawing.Color.SkyBlue;
            this.btnEvolve.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEvolve.Location = new System.Drawing.Point(377, 27);
            this.btnEvolve.Name = "btnEvolve";
            this.btnEvolve.Size = new System.Drawing.Size(95, 48);
            this.btnEvolve.TabIndex = 1;
            this.btnEvolve.Text = "Evolve!";
            this.btnEvolve.UseVisualStyleBackColor = false;
            this.btnEvolve.Click += new System.EventHandler(this.evolve_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(484, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadPopulationToolStripMenuItem,
            this.setAgentTypeToolStripMenuItem,
            this.setPlantLayoutToolStripMenuItem,
            this.debugOutputsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // loadPopulationToolStripMenuItem
            // 
            this.loadPopulationToolStripMenuItem.Name = "loadPopulationToolStripMenuItem";
            this.loadPopulationToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.loadPopulationToolStripMenuItem.Text = "&Load Population...";
            // 
            // setAgentTypeToolStripMenuItem
            // 
            this.setAgentTypeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBox1,
            this.basicNEATdefaultToolStripMenuItem,
            this.qLearningToolStripMenuItem,
            this.socialLearningDarwinianToolStripMenuItem,
            this.socialLearningLamarkianToolStripMenuItem,
            this.toolStripSeparator1,
            this.toolStripTextBox2,
            this.basicNEATnoLearningToolStripMenuItem,
            this.socialPreyOnlyToolStripMenuItem,
            this.socialPredatorsToolStripMenuItem,
            this.socialPreyAndPredatorsToolStripMenuItem,
            this.proportionalLearningToolStripMenuItem});
            this.setAgentTypeToolStripMenuItem.Name = "setAgentTypeToolStripMenuItem";
            this.setAgentTypeToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.setAgentTypeToolStripMenuItem.Text = "Set &Experiment";
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Enabled = false;
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(160, 23);
            this.toolStripTextBox1.Text = "Foraging Evolution";
            // 
            // basicNEATdefaultToolStripMenuItem
            // 
            this.basicNEATdefaultToolStripMenuItem.Checked = true;
            this.basicNEATdefaultToolStripMenuItem.CheckOnClick = true;
            this.basicNEATdefaultToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.basicNEATdefaultToolStripMenuItem.Name = "basicNEATdefaultToolStripMenuItem";
            this.basicNEATdefaultToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.basicNEATdefaultToolStripMenuItem.Text = "Basic NEAT (default)";
            this.basicNEATdefaultToolStripMenuItem.Click += new System.EventHandler(this.basicNEATdefaultToolStripMenuItem_Click);
            // 
            // qLearningToolStripMenuItem
            // 
            this.qLearningToolStripMenuItem.CheckOnClick = true;
            this.qLearningToolStripMenuItem.Name = "qLearningToolStripMenuItem";
            this.qLearningToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.qLearningToolStripMenuItem.Text = "Q-Learning (no evolution)";
            this.qLearningToolStripMenuItem.Click += new System.EventHandler(this.qLearningToolStripMenuItem_Click);
            // 
            // socialLearningDarwinianToolStripMenuItem
            // 
            this.socialLearningDarwinianToolStripMenuItem.CheckOnClick = true;
            this.socialLearningDarwinianToolStripMenuItem.Name = "socialLearningDarwinianToolStripMenuItem";
            this.socialLearningDarwinianToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.socialLearningDarwinianToolStripMenuItem.Text = "Social Learning (Darwinian)";
            this.socialLearningDarwinianToolStripMenuItem.Click += new System.EventHandler(this.socialLearningDarwinianToolStripMenuItem_Click);
            // 
            // socialLearningLamarkianToolStripMenuItem
            // 
            this.socialLearningLamarkianToolStripMenuItem.CheckOnClick = true;
            this.socialLearningLamarkianToolStripMenuItem.Name = "socialLearningLamarkianToolStripMenuItem";
            this.socialLearningLamarkianToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.socialLearningLamarkianToolStripMenuItem.Text = "Social Learning (Lamarkian)";
            this.socialLearningLamarkianToolStripMenuItem.Click += new System.EventHandler(this.socialLearningLamarkianToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(217, 6);
            // 
            // toolStripTextBox2
            // 
            this.toolStripTextBox2.Enabled = false;
            this.toolStripTextBox2.Name = "toolStripTextBox2";
            this.toolStripTextBox2.Size = new System.Drawing.Size(160, 23);
            this.toolStripTextBox2.Text = "Predator/Prey Co-Evolution";
            // 
            // basicNEATnoLearningToolStripMenuItem
            // 
            this.basicNEATnoLearningToolStripMenuItem.Name = "basicNEATnoLearningToolStripMenuItem";
            this.basicNEATnoLearningToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.basicNEATnoLearningToolStripMenuItem.Text = "Basic NEAT (no learning)";
            // 
            // socialPreyOnlyToolStripMenuItem
            // 
            this.socialPreyOnlyToolStripMenuItem.Name = "socialPreyOnlyToolStripMenuItem";
            this.socialPreyOnlyToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.socialPreyOnlyToolStripMenuItem.Text = "Social Prey";
            // 
            // socialPredatorsToolStripMenuItem
            // 
            this.socialPredatorsToolStripMenuItem.Name = "socialPredatorsToolStripMenuItem";
            this.socialPredatorsToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.socialPredatorsToolStripMenuItem.Text = "Social Predators";
            // 
            // socialPreyAndPredatorsToolStripMenuItem
            // 
            this.socialPreyAndPredatorsToolStripMenuItem.Name = "socialPreyAndPredatorsToolStripMenuItem";
            this.socialPreyAndPredatorsToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.socialPreyAndPredatorsToolStripMenuItem.Text = "Social Prey and Predators";
            // 
            // proportionalLearningToolStripMenuItem
            // 
            this.proportionalLearningToolStripMenuItem.Name = "proportionalLearningToolStripMenuItem";
            this.proportionalLearningToolStripMenuItem.Size = new System.Drawing.Size(220, 22);
            this.proportionalLearningToolStripMenuItem.Text = "Proportional Learning";
            this.proportionalLearningToolStripMenuItem.Click += new System.EventHandler(this.proportionalLearningToolStripMenuItem_Click);
            // 
            // setPlantLayoutToolStripMenuItem
            // 
            this.setPlantLayoutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.uniformdefaultToolStripMenuItem,
            this.spiralToolStripMenuItem,
            this.clusterToolStripMenuItem});
            this.setPlantLayoutToolStripMenuItem.Name = "setPlantLayoutToolStripMenuItem";
            this.setPlantLayoutToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.setPlantLayoutToolStripMenuItem.Text = "Set &Plant Layout";
            // 
            // uniformdefaultToolStripMenuItem
            // 
            this.uniformdefaultToolStripMenuItem.Checked = true;
            this.uniformdefaultToolStripMenuItem.CheckOnClick = true;
            this.uniformdefaultToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.uniformdefaultToolStripMenuItem.Name = "uniformdefaultToolStripMenuItem";
            this.uniformdefaultToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.uniformdefaultToolStripMenuItem.Text = "Uniform (default)";
            this.uniformdefaultToolStripMenuItem.CheckedChanged += new System.EventHandler(this.uniformdefaultToolStripMenuItem_CheckedChanged);
            // 
            // spiralToolStripMenuItem
            // 
            this.spiralToolStripMenuItem.CheckOnClick = true;
            this.spiralToolStripMenuItem.Name = "spiralToolStripMenuItem";
            this.spiralToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.spiralToolStripMenuItem.Text = "Spiral";
            this.spiralToolStripMenuItem.CheckedChanged += new System.EventHandler(this.spiralToolStripMenuItem_CheckedChanged);
            // 
            // clusterToolStripMenuItem
            // 
            this.clusterToolStripMenuItem.CheckOnClick = true;
            this.clusterToolStripMenuItem.Name = "clusterToolStripMenuItem";
            this.clusterToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.clusterToolStripMenuItem.Text = "Cluster";
            this.clusterToolStripMenuItem.CheckedChanged += new System.EventHandler(this.clusterToolStripMenuItem_CheckedChanged);
            // 
            // debugOutputsToolStripMenuItem
            // 
            this.debugOutputsToolStripMenuItem.CheckOnClick = true;
            this.debugOutputsToolStripMenuItem.Name = "debugOutputsToolStripMenuItem";
            this.debugOutputsToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.debugOutputsToolStripMenuItem.Text = "Debug Outputs";
            this.debugOutputsToolStripMenuItem.Click += new System.EventHandler(this.debugOutputsToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(484, 462);
            this.Controls.Add(this.btnEvolve);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Social Learning Platform";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnEvolve;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadPopulationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setAgentTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem basicNEATdefaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem socialLearningDarwinianToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem socialLearningLamarkianToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setPlantLayoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uniformdefaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spiralToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clusterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem qLearningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugOutputsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ToolStripMenuItem basicNEATnoLearningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem socialPreyOnlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem socialPredatorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem socialPreyAndPredatorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox2;
        private System.Windows.Forms.ToolStripMenuItem proportionalLearningToolStripMenuItem;
    }
}


/*
 * MapEditor
 * Пользователь: AngryKirC
 * Дата: 01.12.2014
 */
namespace MapEditor.newgui
{
	partial class MapGeneratorDlg
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.progressBarGeneration = new System.Windows.Forms.ProgressBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxAction = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxMapType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numericMapSeed = new System.Windows.Forms.NumericUpDown();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboWall = new System.Windows.Forms.ComboBox();
            this.lblWall = new System.Windows.Forms.Label();
            this.comboEdgeTile = new System.Windows.Forms.ComboBox();
            this.checkBoxPopulate = new System.Windows.Forms.CheckBox();
            this.comboPathTile = new System.Windows.Forms.ComboBox();
            this.comboSecondTile = new System.Windows.Forms.ComboBox();
            this.comboBaseTile = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblBase = new System.Windows.Forms.Label();
            this.lblEdge = new System.Windows.Forms.Label();
            this.checkBoxSmoothWalls = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxRandomSeed = new System.Windows.Forms.CheckBox();
            this.buttonAbort = new System.Windows.Forms.Button();
            this.lblDisclaimer = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericMapSeed)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBarGeneration
            // 
            this.progressBarGeneration.Location = new System.Drawing.Point(16, 24);
            this.progressBarGeneration.Name = "progressBarGeneration";
            this.progressBarGeneration.Size = new System.Drawing.Size(264, 15);
            this.progressBarGeneration.Step = 1;
            this.progressBarGeneration.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarGeneration.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxAction);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.progressBarGeneration);
            this.groupBox1.Location = new System.Drawing.Point(72, 211);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(296, 80);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Progress";
            // 
            // textBoxAction
            // 
            this.textBoxAction.Location = new System.Drawing.Point(72, 48);
            this.textBoxAction.Name = "textBoxAction";
            this.textBoxAction.ReadOnly = true;
            this.textBoxAction.Size = new System.Drawing.Size(208, 20);
            this.textBoxAction.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Action:";
            // 
            // comboBoxMapType
            // 
            this.comboBoxMapType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMapType.FormattingEnabled = true;
            this.comboBoxMapType.Items.AddRange(new object[] {
            "Crossroads"});
            this.comboBoxMapType.Location = new System.Drawing.Point(88, 28);
            this.comboBoxMapType.Name = "comboBoxMapType";
            this.comboBoxMapType.Size = new System.Drawing.Size(96, 21);
            this.comboBoxMapType.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 23);
            this.label2.TabIndex = 3;
            this.label2.Text = "Map Type";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 23);
            this.label3.TabIndex = 4;
            this.label3.Text = "Map Seed";
            // 
            // numericMapSeed
            // 
            this.numericMapSeed.Enabled = false;
            this.numericMapSeed.Location = new System.Drawing.Point(88, 52);
            this.numericMapSeed.Name = "numericMapSeed";
            this.numericMapSeed.Size = new System.Drawing.Size(96, 20);
            this.numericMapSeed.TabIndex = 5;
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Location = new System.Drawing.Point(55, 101);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(86, 34);
            this.buttonGenerate.TabIndex = 6;
            this.buttonGenerate.Text = "Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.ButtonGenerateClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboWall);
            this.groupBox2.Controls.Add(this.lblWall);
            this.groupBox2.Controls.Add(this.comboEdgeTile);
            this.groupBox2.Controls.Add(this.checkBoxPopulate);
            this.groupBox2.Controls.Add(this.comboPathTile);
            this.groupBox2.Controls.Add(this.comboSecondTile);
            this.groupBox2.Controls.Add(this.comboBaseTile);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.lblBase);
            this.groupBox2.Controls.Add(this.lblEdge);
            this.groupBox2.Controls.Add(this.checkBoxSmoothWalls);
            this.groupBox2.Location = new System.Drawing.Point(224, 8);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(208, 197);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Optional settings";
            // 
            // comboWall
            // 
            this.comboWall.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboWall.FormattingEnabled = true;
            this.comboWall.Location = new System.Drawing.Point(53, 22);
            this.comboWall.Name = "comboWall";
            this.comboWall.Size = new System.Drawing.Size(149, 21);
            this.comboWall.TabIndex = 13;
            // 
            // lblWall
            // 
            this.lblWall.AutoSize = true;
            this.lblWall.Location = new System.Drawing.Point(14, 26);
            this.lblWall.Name = "lblWall";
            this.lblWall.Size = new System.Drawing.Size(31, 13);
            this.lblWall.TabIndex = 12;
            this.lblWall.Text = "Wall:";
            // 
            // comboEdgeTile
            // 
            this.comboEdgeTile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboEdgeTile.FormattingEnabled = true;
            this.comboEdgeTile.Location = new System.Drawing.Point(53, 121);
            this.comboEdgeTile.Name = "comboEdgeTile";
            this.comboEdgeTile.Size = new System.Drawing.Size(149, 21);
            this.comboEdgeTile.TabIndex = 5;
            // 
            // checkBoxPopulate
            // 
            this.checkBoxPopulate.Location = new System.Drawing.Point(16, 148);
            this.checkBoxPopulate.Name = "checkBoxPopulate";
            this.checkBoxPopulate.Size = new System.Drawing.Size(168, 24);
            this.checkBoxPopulate.TabIndex = 1;
            this.checkBoxPopulate.Text = "Populate with objects";
            this.checkBoxPopulate.UseVisualStyleBackColor = true;
            // 
            // comboPathTile
            // 
            this.comboPathTile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboPathTile.FormattingEnabled = true;
            this.comboPathTile.Location = new System.Drawing.Point(53, 98);
            this.comboPathTile.Name = "comboPathTile";
            this.comboPathTile.Size = new System.Drawing.Size(149, 21);
            this.comboPathTile.TabIndex = 4;
            // 
            // comboSecondTile
            // 
            this.comboSecondTile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSecondTile.FormattingEnabled = true;
            this.comboSecondTile.Location = new System.Drawing.Point(53, 75);
            this.comboSecondTile.Name = "comboSecondTile";
            this.comboSecondTile.Size = new System.Drawing.Size(149, 21);
            this.comboSecondTile.TabIndex = 3;
            // 
            // comboBaseTile
            // 
            this.comboBaseTile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBaseTile.FormattingEnabled = true;
            this.comboBaseTile.Location = new System.Drawing.Point(53, 52);
            this.comboBaseTile.Name = "comboBaseTile";
            this.comboBaseTile.Size = new System.Drawing.Size(149, 21);
            this.comboBaseTile.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 78);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Floor2:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 101);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Path:";
            // 
            // lblBase
            // 
            this.lblBase.AutoSize = true;
            this.lblBase.Location = new System.Drawing.Point(14, 56);
            this.lblBase.Name = "lblBase";
            this.lblBase.Size = new System.Drawing.Size(39, 13);
            this.lblBase.TabIndex = 8;
            this.lblBase.Text = "Floor1:";
            // 
            // lblEdge
            // 
            this.lblEdge.AutoSize = true;
            this.lblEdge.Location = new System.Drawing.Point(14, 124);
            this.lblEdge.Name = "lblEdge";
            this.lblEdge.Size = new System.Drawing.Size(35, 13);
            this.lblEdge.TabIndex = 11;
            this.lblEdge.Text = "Edge:";
            // 
            // checkBoxSmoothWalls
            // 
            this.checkBoxSmoothWalls.Enabled = false;
            this.checkBoxSmoothWalls.Location = new System.Drawing.Point(16, 168);
            this.checkBoxSmoothWalls.Name = "checkBoxSmoothWalls";
            this.checkBoxSmoothWalls.Size = new System.Drawing.Size(168, 24);
            this.checkBoxSmoothWalls.TabIndex = 0;
            this.checkBoxSmoothWalls.Text = "Make triple-sided walls";
            this.checkBoxSmoothWalls.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.buttonAbort);
            this.groupBox3.Controls.Add(this.buttonGenerate);
            this.groupBox3.Controls.Add(this.checkBoxRandomSeed);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.comboBoxMapType);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.numericMapSeed);
            this.groupBox3.Location = new System.Drawing.Point(8, 8);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(200, 197);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Main settings";
            // 
            // checkBoxRandomSeed
            // 
            this.checkBoxRandomSeed.AutoSize = true;
            this.checkBoxRandomSeed.Checked = true;
            this.checkBoxRandomSeed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRandomSeed.Location = new System.Drawing.Point(19, 78);
            this.checkBoxRandomSeed.Name = "checkBoxRandomSeed";
            this.checkBoxRandomSeed.Size = new System.Drawing.Size(79, 17);
            this.checkBoxRandomSeed.TabIndex = 7;
            this.checkBoxRandomSeed.Text = "Randomize";
            this.checkBoxRandomSeed.UseVisualStyleBackColor = true;
            this.checkBoxRandomSeed.CheckedChanged += new System.EventHandler(this.checkBoxRandomSeed_CheckedChanged);
            // 
            // buttonAbort
            // 
            this.buttonAbort.Enabled = false;
            this.buttonAbort.Location = new System.Drawing.Point(55, 138);
            this.buttonAbort.Name = "buttonAbort";
            this.buttonAbort.Size = new System.Drawing.Size(86, 34);
            this.buttonAbort.TabIndex = 8;
            this.buttonAbort.Text = "Abort";
            this.buttonAbort.UseVisualStyleBackColor = true;
            this.buttonAbort.Click += new System.EventHandler(this.buttonAbort_Click);
            // 
            // lblDisclaimer
            // 
            this.lblDisclaimer.ForeColor = System.Drawing.Color.Red;
            this.lblDisclaimer.Location = new System.Drawing.Point(1, 298);
            this.lblDisclaimer.Name = "lblDisclaimer";
            this.lblDisclaimer.Size = new System.Drawing.Size(440, 27);
            this.lblDisclaimer.TabIndex = 14;
            this.lblDisclaimer.Text = "WARNING: This utility is not finished, use with caution. Deletes current map.";
            this.lblDisclaimer.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // MapGeneratorDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 319);
            this.Controls.Add(this.lblDisclaimer);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MapGeneratorDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Random Map Generator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MapGeneratorDlgFormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericMapSeed)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.CheckBox checkBoxPopulate;
		private System.Windows.Forms.CheckBox checkBoxSmoothWalls;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button buttonGenerate;
		private System.Windows.Forms.NumericUpDown numericMapSeed;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboBoxMapType;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxAction;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ProgressBar progressBarGeneration;
        private System.Windows.Forms.CheckBox checkBoxRandomSeed;
        private System.Windows.Forms.ComboBox comboEdgeTile;
        private System.Windows.Forms.ComboBox comboPathTile;
        private System.Windows.Forms.ComboBox comboSecondTile;
        private System.Windows.Forms.ComboBox comboBaseTile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblBase;
        private System.Windows.Forms.Label lblEdge;
        private System.Windows.Forms.ComboBox comboWall;
        private System.Windows.Forms.Label lblWall;
        private System.Windows.Forms.Button buttonAbort;
        private System.Windows.Forms.Label lblDisclaimer;
    }
}

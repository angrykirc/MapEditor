/*
 * MapEditor
 * Пользователь: AngryKirC
 * Дата: 01.12.2014
 */
using System;
using MapEditor.MapInt;
using System.Windows.Forms;
using System.ComponentModel;
using MapEditor.mapgen;
using NoxShared;
using System.Collections.Generic;

namespace MapEditor.newgui
{
	/// <summary>
	/// Final version
	/// </summary>
	public partial class MapGeneratorDlg : Form
	{
		private MapView parent;
		private Map restoreMap;

        const string GENERATING_MESSAGE = "Sorry, you cannot close this window while the map is being generated.";
		
		public MapGeneratorDlg(MapView view)
		{
			InitializeComponent();

			parent = view;
			numericMapSeed.Minimum = int.MinValue;
			numericMapSeed.Maximum = int.MaxValue;
			comboBoxMapType.SelectedIndex = 0;

            var sortedWallNames = new List<string>(ThingDb.WallNames.ToArray());
            sortedWallNames.Sort();
            comboWall.Items.AddRange(sortedWallNames.ToArray());
            comboWall.SelectedIndex = sortedWallNames.IndexOf("DecidiousWallGreen");

            var sortedTileNames = new List<string>(ThingDb.FloorTileNames.ToArray());
            sortedTileNames.Sort();
            comboBaseTile.Items.AddRange(sortedTileNames.ToArray());
            comboSecondTile.Items.AddRange(sortedTileNames.ToArray());
            comboPathTile.Items.AddRange(sortedTileNames.ToArray());
            comboBaseTile.SelectedIndex = sortedTileNames.IndexOf("GrassSparse2");
            comboSecondTile.SelectedIndex = sortedTileNames.IndexOf("GrassDense");
            comboPathTile.SelectedIndex = sortedTileNames.IndexOf("DirtDark2");

            var sortedEdgeNames = new List<string>(ThingDb.EdgeTileNames.ToArray());
            sortedEdgeNames.Sort();
            comboEdgeTile.Items.AddRange(sortedEdgeNames.ToArray());
            comboEdgeTile.SelectedIndex = sortedEdgeNames.IndexOf("BlendEdge");
        }
		
		void MapGeneratorDlgFormClosing(object sender, FormClosingEventArgs e)
		{
            // Oh no, please wait
            if (Generator.IsGenerating)
            {
                MessageBox.Show(GENERATING_MESSAGE, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
            }
        }

        void ButtonGenerateClick(object sender, EventArgs e)
		{
            var disclmr = MessageBox.Show("This will overwrite the existing map.  Are you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (disclmr == DialogResult.No)
                return;

			restoreMap = MapInterface.TheMap;
			MapInterface.TheMap = null; // To avoid multithread problems
			// Disable button
			buttonGenerate.Enabled = false;
            buttonAbort.Enabled = true;
			// Setup config
			GeneratorConfig config = new GeneratorConfig();
			if (comboBoxMapType.SelectedIndex < 0) return;
			config.MapType = (GeneratorConfig.MapPreset) comboBoxMapType.SelectedIndex + 1;
			config.RandomSeed = (int) numericMapSeed.Value;
            config.Randomize = checkBoxRandomSeed.Checked;
			config.Allow3SideWalls = checkBoxSmoothWalls.Checked;
			config.PopulateMap = checkBoxPopulate.Checked;
            config.BASE_FLOOR = comboBaseTile.SelectedItem.ToString();
            config.DENSE_FLOOR = comboSecondTile.SelectedItem.ToString();
            config.PATH_FLOOR = comboPathTile.SelectedItem.ToString();
            config.BLEND_EDGE = comboEdgeTile.SelectedItem.ToString();
            config.WALL = comboWall.SelectedItem.ToString();
			Generator.SetConfig(config);
			// Setup worker handlers
			Generator.Worker.ProgressChanged += new ProgressChangedEventHandler(Generator_Worker_ProgressChanged);
			Generator.Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Generator_Worker_RunWorkerCompleted);
            Generator.Worker.WorkerSupportsCancellation = true;
            // Generate
            Generator.GenerateMap(restoreMap);
        }
		void Generator_Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
            if (Generator.IsCancelled)
                textBoxAction.Text = "Aborted";
            else
            {
                textBoxAction.Text = "Map generated successfully";
                MapInterface.TheMap = restoreMap;
            }
            buttonGenerate.Enabled = true;
        }

		void Generator_Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
            progressBarGeneration.Value = e.ProgressPercentage;
            textBoxAction.Text = Generator.GetStatus();
        }

        private void checkBoxRandomSeed_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxRandomSeed.Checked)
                numericMapSeed.Enabled = false;
            else
                numericMapSeed.Enabled = true;
        }

        private void buttonAbort_Click(object sender, EventArgs e)
        {
            Generator.Cancel();
            DialogResult = DialogResult.Abort;
            Dispose();
        }
    }
}

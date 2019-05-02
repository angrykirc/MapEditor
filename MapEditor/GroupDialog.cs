using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NoxShared;

namespace MapEditor
{
    partial class GroupDialog : Form
    {
        private List<MapGroup> groups;
        private string lastGroupName;

        public GroupDialog(Map.GroupData groupData)
        {
            InitializeComponent();

            groups = new List<MapGroup>();
            foreach (var g in groupData.Values)
            {
                var mg = new MapGroup(g.name, g.id, (int)g.type, g.ExtentsToString());
                groups.Add(mg);
            }
            ReloadUI();
        }

        private void newButton_Click(object sender, EventArgs e)
        {
            // First see if user typed in a name, otherwise make one up
            var newName = txtGroupName.Text.Trim();
            if ((newName == "") || (lstGroups.Items.Contains(newName)))
            {
                var i = 1;
                while (lstGroups.Items.Contains("NewGroup" + i))
                    i++;
                newName = "NewGroup" + i;
            }

            var newGroup = new MapGroup(newName, GetNextGroupID(), GetGroupType(), "");
            groups.Add(newGroup);
            ReloadUI();
            lstGroups.SelectedIndex = lstGroups.Items.Count - 1;
        }
        private void delButton_Click(object sender, EventArgs e)
        {
            var g = GetGroup();
            if (g != null)
                groups.Remove(g);
            ReloadUI();
        }
        private void saveButton_Click(object sender, EventArgs e)
        {
            var gd = GenerateGroupData();
            if (gd != null)
            {
                MainWindow.Instance.SetGroups(gd);
                Close();
            }
            else
                lblHelpStatus.Text = "Save failed";
        }
        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void objectRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (objectRadio.Checked)
                SetGroupType();
        }
        private void wallRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (wallRadio.Checked)
                SetGroupType();
        }
        private void waypointRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (waypointRadio.Checked)
                SetGroupType();
        }

        private void lstGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            var g = GetGroup();
            if (g == null)
                return;
            
            lastGroupName = g.Name;
            txtGroupName.Text = g.Name;
            txtGroupID.Text = g.ID.ToString();
            txtExtents.Text = g.Extents;

            if (g.Type == 0)
                objectRadio.Checked = true;
            else if (g.Type == 1)
                waypointRadio.Checked = true;
            else if (g.Type == 2)
            {
                wallRadio.Checked = true;
                if (!txtGroupName.Text.ToUpper().EndsWith("WALLS"))
                    lblHelpStatus.Text = "Wall Group names must end with 'Walls'";
                else
                    lblHelpStatus.Text = "";
            }
        }
        private void txtGroupName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                var g = GetGroup();
                if (g == null)
                    return;

                var newName = txtGroupName.Text.Trim();
                if (newName == "")
                    return;
                if (lstGroups.Items.Contains(newName))
                {
                    lblHelpStatus.Text = "That name already exists";
                    return;
                }

                g.Name = newName;
                ReloadUI();
                txtGroupName.Select(txtGroupName.Text.Length, 0);
                e.SuppressKeyPress = true;
            }
        }
        private void txtGroupName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastGroupName))
                return;
            if (txtGroupName.Text != lastGroupName)
                lblHelpStatus.Text = "Press ENTER to rename";
            else
                lblHelpStatus.Text = "";
        }
        private void txtExtents_TextChanged(object sender, EventArgs e)
        {
            var g = GetGroup();
            if (g != null)
                g.Extents = txtExtents.Text;
        }

        private MapGroup GetGroup()
        {
            // Perform all necessary validation
            if (lstGroups.Items.Count == 0)
                return null;
            if (lstGroups.SelectedItem == null)
                return null;
            var selection = lstGroups.SelectedItem.ToString().Trim();
            if (selection == "")
                return null;

            foreach (var g in groups)
                if (g.Name == selection)
                    return g;

            return null;
        }
        private void SetGroupType()
        {
            var g = GetGroup();
            if (g != null)
                g.Type = GetGroupType();
        }
        private int GetGroupType()
        {
            if (waypointRadio.Checked)
                return 1;
            else if (wallRadio.Checked)
                return 2;
            else
                return 0;
        }
        private int GetNextGroupID()
        {
            if (groups.Count == 0)
                return 0;

            var ids = new List<int>();
            foreach (var g in groups)
                ids.Add(g.ID);

            int i = 0;
            while (i < 99999)
            {
                if (!ids.Contains(i))
                    return i;
                i++;
            }

            return 0;
        }
        private Map.GroupData GenerateGroupData()
        {
            // List<MapGroup> ==> Map.GroupData
            var result = new Map.GroupData();
            foreach (var g in groups)
            {
                // Don't save groups without any extents
                if (g.Extents.Trim() == "")
                    continue;

                var newGroup = GenerateGroup(g);
                // Ensure no errors trying to parse
                if (newGroup == null)
                    return null;

                result.Add(g.Name, newGroup);
            }

            return result;
        }
        private Map.Group GenerateGroup(MapGroup g)
        {
            // MapGroup ==> Map.Group
            var newGroup = new Map.Group(g.Name, (Map.Group.GroupTypes)g.Type, g.ID);
            var extents = g.Extents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var extent in extents)
            {
                try
                {
                    switch (g.Type)
                    {
                        case 0:
                        case 1:
                            newGroup.Add(int.Parse(extent.Trim()));
                            break;
                        case 2:
                            var point = extent.Trim().Split(',');
                            newGroup.Add(new Point(int.Parse(point[0]), int.Parse(point[1])));
                            break;
                    }
                }
                catch
                {
                    if (lstGroups.Items.Contains(g.Name))
                        lstGroups.SelectedItem = g.Name;
                    MessageBox.Show("Failed to parse extents:\n\nGroup: " + g.Name + "\nExtent: " + extent, "Invalid Argument", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return null;
                }
            }

            return newGroup;
        }

        private void ReloadUI()
        {
            // Clear everything, save selection, reload groups
            lblHelpStatus.Text = "";
            txtGroupName.Text = "";
            txtGroupID.Text = "";
            var save = lstGroups.SelectedIndex;
            lstGroups.Items.Clear();

            foreach (var g in groups)
                lstGroups.Items.Add(g.Name);

            if (lstGroups.Items.Count > 0)
            {
                if (save >= 0)
                {
                    if (save >= lstGroups.Items.Count)
                        lstGroups.SelectedIndex = save - 1;
                    else
                        lstGroups.SelectedIndex = save;
                }
            }
            else
                lblHelpStatus.Text = "Click New";
        }
    }

    public class MapGroup
    {
        // This is the only class that gets modified during UI events; Map.GroupData is a read-only sorted Dictionary
        public string Name { get; set; }
        public int ID { get; set; }
        public int Type { get; set; }
        public string Extents { get; set; }

        public MapGroup(string name, int id, int type, string extents)
        {
            Name = name;
            ID = id;
            Type = type;
            Extents = extents;
        }
    }
}
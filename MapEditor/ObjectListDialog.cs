using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MapEditor.MapInt;
using NoxShared;
using System.Collections.Generic;
using NoxShared.ObjDataXfer;

namespace MapEditor
{
	public class ObjectListDialog : Form
    {
        private IContainer components;

        private DataGridViewColumn setting;
		protected DataTable objList;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem goToObjectToolStripMenuItem;
        private ToolStripMenuItem editObjectToolStripMenuItem;
        private DataGridView dataGrid1;
		public Map.ObjectTable objTable
		{
			set
			{
				objList = new DataTable("objList");
				objList.Columns.Add("Extent",Type.GetType("System.UInt32"));
				objList.Columns.Add("X-Coor.",Type.GetType("System.Single"));
				objList.Columns.Add("Y-Coor.",Type.GetType("System.Single"));
                objList.Columns.Add("Name", Type.GetType("System.Object"));
				objList.Columns.Add("Scr. Name", Type.GetType("System.String"));
                objList.Columns.Add("Enchant1", Type.GetType("System.String"));
                objList.Columns.Add("Enchant2", Type.GetType("System.String"));
                objList.Columns.Add("Enchant3", Type.GetType("System.String"));
                objList.Columns.Add("Enchant4", Type.GetType("System.String"));

                foreach (Map.Object obj in value)
                {
                    var enchants = GetEnchants(obj);
                    if (enchants == null)
                        objList.Rows.Add(new object[] { obj.Extent, obj.Location.X, obj.Location.Y, obj, obj.Scr_Name, "", "", "", "" });
                    else
                        objList.Rows.Add(new object[] { obj.Extent, obj.Location.X, obj.Location.Y, obj, obj.Scr_Name, enchants[0], enchants[1], enchants[2], enchants[3] });

                    dataGrid1.DataSource = objList;
                }
			}
		}
        public Map.ObjectTable objTable2;
        private Timer Helpmark;
        private ToolStripMenuItem applyChangesToolStripMenuItem;
        private ToolStripMenuItem onlyAppliesToScriptEnchantsToolStripMenuItem;
        public MapView Map;
        public Map.ObjectTable Result { get; set; }

        public ObjectListDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            dataGrid1.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(dataGrid1_ColumnHeaderMouseClick);
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		
		}

		protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectListDialog));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.goToObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGrid1 = new System.Windows.Forms.DataGridView();
            this.Helpmark = new System.Windows.Forms.Timer(this.components);
            this.onlyAppliesToScriptEnchantsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.goToObjectToolStripMenuItem,
            this.editObjectToolStripMenuItem,
            this.applyChangesToolStripMenuItem,
            this.onlyAppliesToScriptEnchantsToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // goToObjectToolStripMenuItem
            // 
            this.goToObjectToolStripMenuItem.Name = "goToObjectToolStripMenuItem";
            resources.ApplyResources(this.goToObjectToolStripMenuItem, "goToObjectToolStripMenuItem");
            this.goToObjectToolStripMenuItem.Click += new System.EventHandler(this.goToObjectToolStripMenuItem_Click);
            // 
            // editObjectToolStripMenuItem
            // 
            this.editObjectToolStripMenuItem.Name = "editObjectToolStripMenuItem";
            resources.ApplyResources(this.editObjectToolStripMenuItem, "editObjectToolStripMenuItem");
            this.editObjectToolStripMenuItem.Click += new System.EventHandler(this.editObjectToolStripMenuItem_Click);
            // 
            // applyChangesToolStripMenuItem
            // 
            this.applyChangesToolStripMenuItem.Name = "applyChangesToolStripMenuItem";
            resources.ApplyResources(this.applyChangesToolStripMenuItem, "applyChangesToolStripMenuItem");
            this.applyChangesToolStripMenuItem.Click += new System.EventHandler(this.applyChangesToolStripMenuItem_Click);
            // 
            // dataGrid1
            // 
            this.dataGrid1.AllowUserToAddRows = false;
            this.dataGrid1.AllowUserToDeleteRows = false;
            this.dataGrid1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGrid1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(this.dataGrid1, "dataGrid1");
            this.dataGrid1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGrid1.Name = "dataGrid1";
            // 
            // Helpmark
            // 
            this.Helpmark.Interval = 120;
            this.Helpmark.Tick += new System.EventHandler(this.Helpmark_Tick);
            // 
            // onlyAppliesToScriptEnchantsToolStripMenuItem
            // 
            resources.ApplyResources(this.onlyAppliesToScriptEnchantsToolStripMenuItem, "onlyAppliesToScriptEnchantsToolStripMenuItem");
            this.onlyAppliesToScriptEnchantsToolStripMenuItem.Name = "onlyAppliesToScriptEnchantsToolStripMenuItem";
            // 
            // ObjectListDialog
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.dataGrid1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ObjectListDialog";
            this.ShowInTaskbar = false;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
        
		private void dataGrid1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Map.CenterAtPoint(new Point((int)((float)objList.Rows[dataGrid1.CurrentRow.Index]["X-Coor."]), (int)((float)objList.Rows[dataGrid1.CurrentRow.Index]["Y-Coor."])));
		
        }
        private void dataGrid1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DatatableSync();

        }

        private void DatatableSync()
        {

            if (dataGrid1.SortedColumn == null || dataGrid1.SortedColumn.Name.Length <= 0) return;

            if (dataGrid1.SortOrder == SortOrder.Descending)
            {
                objList.DefaultView.Sort = dataGrid1.SortedColumn.Name + " DESC";
            }
            else
            {
                objList.DefaultView.Sort = dataGrid1.SortedColumn.Name + " ASC";
            }
            objList = objList.DefaultView.ToTable();
        }

        private void goToObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Point target = new Point((int)((float)objList.Rows[dataGrid1.CurrentRow.Index]["X-Coor."]), (int)((float)objList.Rows[dataGrid1.CurrentRow.Index]["Y-Coor."]));
            Map.CenterAtPoint(target);
            Helpmark.Enabled = true;
            Map.highlightUndoRedo = target;
            Map.Object P = (Map.Object)(objList.Rows[dataGrid1.CurrentRow.Index][3]);
            Map.SelectedObjects.Items.Clear();
            Map.SelectedObjects.Items.Add(P);
        }
        private void editObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListSortDirection sorted = ListSortDirection.Ascending;
            int curIndex = dataGrid1.CurrentRow.Index;
            int vscroll = dataGrid1.VerticalScrollingOffset;
            if (dataGrid1.SortOrder == SortOrder.Ascending) sorted = ListSortDirection.Ascending;
            if (dataGrid1.SortOrder == SortOrder.Descending) sorted = ListSortDirection.Descending;
            setting = dataGrid1.SortedColumn;
            Map.Object P = (Map.Object)(objList.Rows[dataGrid1.CurrentRow.Index][3]);
            Map.ShowObjectProperties(P);
            this.objTable = objTable2;

            if (setting != null)
            {
                if (dataGrid1.Columns[setting.Name] != null)
                    dataGrid1.Sort(dataGrid1.Columns[setting.Name], sorted);

            }
            if (curIndex >= 0)
            {
                dataGrid1.ClearSelection();
                dataGrid1.Rows[curIndex].Selected = true;
                dataGrid1.CurrentCell = dataGrid1.Rows[curIndex].Cells[0];
            }

            DatatableSync();
        }
        private void deleteObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // map.RemoveObject(new Point((int)((float)objList.Rows[dataGrid1.CurrentRowIndex]["X-Coor."]), (int)((float)objList.Rows[dataGrid1.CurrentRowIndex]["Y-Coor."])));
            /*
            int i = 0;
            foreach (Map.Object obj in MapInterface.TheMap.Objects)
            {
                i++;
                int bob = Convert.ToInt32((objList.Rows[dataGrid1.CurrentRow.Index][0]));
                if (i == bob)
                {
                    MapInterface.ObjectRemove(obj);
                    break;

                }
            }
            */
        }

        private void transparentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opacity = 0.50;
        }
        private void Helpmark_Tick(object sender, EventArgs e)
        {
            Map.higlightRad -= 30;

            if (Map.higlightRad > 40) return;
            Map.highlightUndoRedo = new Point();
            Map.higlightRad = 150;
            Helpmark.Enabled = false;
        }

        private void applyChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGrid1.CommitEdit(DataGridViewDataErrorContexts.CurrentCellChange);

            // Find and return changes
            int c = 0;
            for (int i = 0; i < dataGrid1.Rows.Count; i++)
            {
                var name = dataGrid1[3, i].Value.ToString();
                var newScrName = dataGrid1[4, i].Value.ToString();
                Map.Object newObj = (Map.Object) objList.Rows[i][3];
                int j = objTable2.IndexOf(newObj);
                Map.Object oldObj = (Map.Object) objTable2[j];

                // Set new script name
                if (oldObj.Scr_Name != newScrName)
                {
                    ((Map.Object) objTable2[j]).Scr_Name = newScrName;
                    // Check 'Extra Bytes'
                    if (oldObj.Terminator == 0x00)
                        ((Map.Object)objTable2[j]).Terminator = 0xFF;
                    c++;
                }

                var enchants = GetEnchants(oldObj);
                if (enchants == null)
                    continue;

                string[] ench = new string[4];
                ench[0] = dataGrid1[5, i].Value.ToString();
                ench[1] = dataGrid1[6, i].Value.ToString();
                ench[2] = dataGrid1[7, i].Value.ToString();
                ench[3] = dataGrid1[8, i].Value.ToString();

                for (int k = 0; k < 4; k++)
                {
                    // Ignore all whitespace
                    if (ench[k] == "")
                        continue;
                    if (ench[k].Trim() == "")
                    {
                        ench[k] = "";
                        continue;
                    }

                    // Ensure valid enchant and proper case
                    var isValid = GetValidEnchant(ench[k]);
                    if (isValid != "-1")
                        ench[k] = isValid;
                    else
                        ench[k] = "";
                }

                // Set new enchants
                if (enchants != ench)
                    objTable2[j] = SetEnchants(oldObj, ench);
            }

            // Pass new object table back to MainWindow
            Result = null;
            if (c > 0)
                Result = objTable2;

            DialogResult = DialogResult.OK;
            Hide();
        }
        private string[] GetEnchants(Map.Object obj)
        {
            ThingDb.Thing tt = ThingDb.Things[obj.Name];
            WeaponXfer weapon; ArmorXfer armor; AmmoXfer ammo; TeamXfer team;
            string[] enchants = new string[4];
            switch (tt.Xfer)
            {
                case "WeaponXfer":
                    weapon = obj.GetExtraData<WeaponXfer>();
                    enchants[0] = weapon.Enchantments[0];
                    enchants[1] = weapon.Enchantments[1];
                    enchants[2] = weapon.Enchantments[2];
                    enchants[3] = weapon.Enchantments[3];
                    break;
                case "ArmorXfer":
                    armor = obj.GetExtraData<ArmorXfer>();
                    enchants[0] = armor.Enchantments[0];
                    enchants[1] = armor.Enchantments[1];
                    enchants[2] = armor.Enchantments[2];
                    enchants[3] = armor.Enchantments[3];
                    break;
                case "AmmoXfer":
                    ammo = obj.GetExtraData<AmmoXfer>();
                    enchants[0] = ammo.Enchantments[0];
                    enchants[1] = ammo.Enchantments[1];
                    enchants[2] = ammo.Enchantments[2];
                    enchants[3] = ammo.Enchantments[3];
                    break;
                case "TeamXfer":
                    team = obj.GetExtraData<TeamXfer>();
                    enchants[0] = team.Enchantments[0];
                    enchants[1] = team.Enchantments[1];
                    enchants[2] = team.Enchantments[2];
                    enchants[3] = team.Enchantments[3];
                    break;
                default:
                    enchants = null;
                    break;
            }
            return enchants;
        }
        private Map.Object SetEnchants(Map.Object obj, string[] enchants)
        {
            ThingDb.Thing tt = ThingDb.Things[obj.Name];
            WeaponXfer weapon; ArmorXfer armor; AmmoXfer ammo; TeamXfer team;

            switch (tt.Xfer)
            {
                case "WeaponXfer":
                    weapon = obj.GetExtraData<WeaponXfer>();
                    weapon.Enchantments[0] = enchants[0];
                    weapon.Enchantments[1] = enchants[1];
                    weapon.Enchantments[2] = enchants[2];
                    weapon.Enchantments[3] = enchants[3];
                    break;
                case "ArmorXfer":
                    armor = obj.GetExtraData<ArmorXfer>();
                    armor.Enchantments[0] = enchants[0];
                    armor.Enchantments[1] = enchants[1];
                    armor.Enchantments[2] = enchants[2];
                    armor.Enchantments[3] = enchants[3];
                    break;
                case "AmmoXfer":
                    ammo = obj.GetExtraData<AmmoXfer>();
                    ammo.Enchantments[0] = enchants[0];
                    ammo.Enchantments[1] = enchants[1];
                    ammo.Enchantments[2] = enchants[2];
                    ammo.Enchantments[3] = enchants[3];
                    break;
                case "TeamXfer":
                    team = obj.GetExtraData<TeamXfer>();
                    team.Enchantments[0] = enchants[0];
                    team.Enchantments[1] = enchants[1];
                    team.Enchantments[2] = enchants[2];
                    team.Enchantments[3] = enchants[3];
                    break;
            }
            return obj;
        }
        private string GetValidEnchant(string enchant)
        {
            foreach (var validEnch in XferGui.EquipmentEdit.ENCHANTMENTS)
            {
                if (validEnch.ToUpper() == enchant.ToUpper())
                    return validEnch;
            }
            return "-1";
        }
    }
}

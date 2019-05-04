/*
 * MapEditor
 * Пользователь: AngryKirC
 * Дата: 12.02.2015
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using MapEditor.MapInt;
using NoxShared;

namespace MapEditor.newgui
{
    public partial class PolygonEditor : Form
    {
        private Map subjMap
        {
            get
            {
                return MapInterface.TheMap;
            }
        }
        public PolygonDialog polygonDlg = new PolygonDialog();
        public Map.Polygon SelectedPolygon = null;
        public Map.Polygon SuperPolygon = null;
        public PolygonEditor()
        {
            InitializeComponent();
        }

        private void PolygonEditorLoad(object sender, EventArgs e)
        {
            if (subjMap != null)
            {
                UpdatePolygonList();
                StatusChanged();
            }
        }
        private void PolygonEditor_Activated(object sender, EventArgs e)
        {
            if (subjMap != null)
            {
                UpdatePolygonList();
                StatusChanged();
            }
        }
        private void PolygonEditor_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
                MapInterface.CurrentMode = EditMode.POLYGON_RESHAPE;
            else
            {
                SuperPolygon = null;
                MainWindow.Instance.mapView.TabMapToolsSelectedIndexChanged(sender, e);
            }
        }
        private void PolygonEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Visible = false;
        }

        private void UpdatePolygonList()
        {
            int oldIndex = listBoxPolygons.SelectedIndex;
            int oldCount = listBoxPolygons.Items.Count;
            // Update items
            listBoxPolygons.Items.Clear();
            foreach (Map.Polygon p in subjMap.Polygons)
            {
                listBoxPolygons.Items.Add(p.Name);
            }
            // Restore selection
            if (oldIndex >= 0)
            {
                int index = listBoxPolygons.Items.Count - (oldCount - oldIndex);
                listBoxPolygons.SelectedIndex = index;
            }

        }
        private void StatusChanged()
        {
            if (listBoxPolygons.Items.Count > 0 && listBoxPolygons.SelectedIndex < 0)
                listBoxPolygons.SelectedIndex = 0;

            if (listBoxPolygons.SelectedIndex < 0)
            {
                buttonPoints.Enabled = false;
                buttonDelete.Enabled = false;
                buttonModify.Enabled = false;
                buttonCopyMap.Enabled = false;
                return;
            }
            buttonPoints.Enabled = true;
            buttonDelete.Enabled = true;
            buttonModify.Enabled = true;
            buttonCopyMap.Enabled = true;
            MapInterface.CurrentMode = EditMode.POLYGON_RESHAPE;
        }

        public void ButtonModifyClick(object sender, EventArgs e)
        {
            if (listBoxPolygons.SelectedIndex < 0) return;

            polygonDlg.Polygon = (Map.Polygon)subjMap.Polygons[listBoxPolygons.SelectedIndex];
            polygonDlg.ShowDialog();
            subjMap.Polygons[listBoxPolygons.SelectedIndex] = polygonDlg.Polygon;
            UpdatePolygonList();
            StatusChanged();
        }
        private void ButtonNewClick(object sender, EventArgs e)
        {
            polygonDlg.Polygon = null;
            if (polygonDlg.ShowDialog() == DialogResult.OK && polygonDlg.Polygon != null)
            {
                subjMap.Polygons.Insert(0, polygonDlg.Polygon);
                UpdatePolygonList();
                listBoxPolygons.SelectedIndex = 0;
            }
        }
        private void ButtonDeleteClick(object sender, EventArgs e)
        {
            if (listBoxPolygons.SelectedIndex < 0) return;

            // Fix logic
            Map.Polygon poly = (Map.Polygon)subjMap.Polygons[listBoxPolygons.SelectedIndex];
            if (poly == SelectedPolygon) SelectedPolygon = null;

            subjMap.Polygons.RemoveAt(listBoxPolygons.SelectedIndex);
            UpdatePolygonList();
            StatusChanged();
            MainWindow.Instance.Reload();
        }
        private void ButtonPointsClick(object sender, EventArgs e)
        {
            if (listBoxPolygons.SelectedIndex < 0) return;

            // update mapinterface
            MapInterface.CurrentMode = EditMode.POLYGON_RESHAPE;
            SelectedPolygon = (Map.Polygon)subjMap.Polygons[listBoxPolygons.SelectedIndex];
            MapInterface.SelectedPolyPoint = new PointF();
            MainWindow.Instance.Focus();
            //double alpha = Math.Round(SelectedPolygon.AmbientLightColor.GetBrightness() * 200);
            //MessageBox.Show(alpha.ToString());
            MainWindow.Instance.Reload();
        }

        private void ButtonUpClick(object sender, EventArgs e)
        {
            int index = listBoxPolygons.SelectedIndex;
            if (index < 0) return;

            object poly = subjMap.Polygons[index];
            subjMap.Polygons.RemoveAt(index);
            // Decrement index
            if (index > 0) { index = index - 1; }
            subjMap.Polygons.Insert(index, poly);
            listBoxPolygons.SelectedIndex = index;
            UpdatePolygonList();
        }
        private void ButtonDownClick(object sender, EventArgs e)
        {
            int index = listBoxPolygons.SelectedIndex;
            if (index < 0) return;

            object poly = subjMap.Polygons[index];
            subjMap.Polygons.RemoveAt(index);
            // Increment index
            if (index < subjMap.Polygons.Count) { index = index + 1; }
            subjMap.Polygons.Insert(index, poly);
            listBoxPolygons.SelectedIndex = index;
            UpdatePolygonList();
        }
        private void ButtonDoneClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Hide();

            MainWindow.Instance.Focus();
            MainWindow.Instance.Reload();
        }

        private void listBoxPolygons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxPolygons.SelectedIndex < 0) return;

            MapInterface.CurrentMode = EditMode.POLYGON_RESHAPE;
            SelectedPolygon = (Map.Polygon)subjMap.Polygons[listBoxPolygons.SelectedIndex];
            MapInterface.SelectedPolyPoint = new PointF();
            if (LockedBox.Checked) SuperPolygon = SelectedPolygon;
  		    else SuperPolygon = null;
            MainWindow.Instance.Reload();
            StatusChanged();
        }
        private void listBoxPolygons_ControlAdded(object sender, ControlEventArgs e)
        {
            if (listBoxPolygons.SelectedIndex < 0)
            {

                buttonDelete.Enabled = false;
                buttonModify.Enabled = false;
                return;
            }
            buttonDelete.Enabled = true;
            buttonModify.Enabled = true;
        }
        private void listBoxPolygons_ControlRemoved(object sender, ControlEventArgs e)
        {
            if (listBoxPolygons.SelectedIndex < 0)
            {

                buttonDelete.Enabled = false;
                buttonModify.Enabled = false;
                return;
            }
            buttonDelete.Enabled = true;
            buttonModify.Enabled = true;
        }
        private void LockedBox_CheckedChanged(object sender, EventArgs e)
        {
            if (LockedBox.Checked)
                SuperPolygon = SelectedPolygon;

            MainWindow.Instance.Reload();
        }

        private void ButtonCopyMapClick(object sender, EventArgs e)
        {
            if (listBoxPolygons.SelectedIndex < 0)
                return;

            var pts = ((Map.Polygon)subjMap.Polygons[listBoxPolygons.SelectedIndex]).Points;
            var newPts = new System.Collections.Generic.List<Point>();
            foreach (var p in pts)
                newPts.Add(p.ToPoint());

            MainWindow.Instance.mapView.CopyArea(newPts.ToArray());
        }
    }
}
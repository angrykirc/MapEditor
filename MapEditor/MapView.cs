using System;
using System.Drawing;
using System.Windows.Forms;
using NoxShared;
//using TileEdgeDirection = NoxShared.Map.Tile.EdgeTile.Direction;
using Mode = MapEditor.MapInt.EditMode;
using MapEditor.render;
using MapEditor.newgui;
using MapEditor.objgroups;
using MapEditor.MapInt;
using NoxShared.ObjDataXfer;
using System.Collections.Generic;
using System.Collections;
using MapEditor.mapgen;
using System.Linq;

namespace MapEditor
{
    public class MapView : UserControl
    {
        #region Globals
        public WallProperties secprops = new WallProperties();
        public bool TileTabLoaded = false;
        public bool mouseMove = false;
        private bool renderingOk = true;
        private int WidthMod = 0;
        private int winX = 0;
        private int winY = 0;
        public List<Map.Wall> LastWalls = new List<Map.Wall>();
        public bool picking = false;
        public Point mouseKeep;
        public Point mouseKeepOff;
        public int arrowPoly = 0;
        private float relXX = 0;
        private float relYY = 0;
        public int higlightRad = 150;
        private StatusbarHelper statusHelper = new StatusbarHelper();
        public const int squareSize = 23;
        public const int objectSelectionRadius = 8;
        protected Button currentButton;
        public MapObjectCollection SelectedObjects { get { return MapInterface.SelectedObjects; } }
        public MapViewRenderer MapRenderer;
        public PolygonEditor PolygonEditDlg = null;
        public ScriptFunctionDialog strSd = new ScriptFunctionDialog();
        public Point mouseLocation;
        public Point destLinePoint;
        public float delta;
        private int tilecount;
        private bool added;
        private int wallcount;
        public bool BlockTime;
        public Point copyPoint;
        public bool UpdateCanvasOKMiniMap = true;
        public static int[] directions = new int[8] { 0, 6, 2, 7, 1, 4, 5, 3 };
        public List<PointF> PolyPointOffset = new List<PointF>();
        private Map Map
        {
            get
            {
                return MapInterface.TheMap;
            }
        }

        public class FlickerFreePanel : Panel
        {
            // 16-bit windows needs the deprectated version of DoubleBufferring to work. Otherwise it crashes :\
            public FlickerFreePanel() : base()
            { SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Opaque, true); }
        }
        #endregion

        public MapView()
        {
            InitializeComponent();

            WidthMod = groupAdv.Width;
            MapRenderer = new MapViewRenderer(this);

            // setup window handlers
            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.KeyDown += new KeyEventHandler(MapView_KeyPressed);
                MainWindow.Instance.KeyDown += new KeyEventHandler(StartShiftSelecting);
                MainWindow.Instance.KeyUp += new KeyEventHandler(StopShiftSelecting);
                MainWindow.Instance.KeyDown += new KeyEventHandler(TabsShortcuts);
                MainWindow.Instance.MouseWheel += new MouseEventHandler(MouseWheelEventHandler);
                cboObjCreate.MouseWheel += new MouseEventHandler(cboObjMouseWheel);
                objectCategoriesBox.MouseWheel += new MouseEventHandler(cboObjMouseWheel);
                contexMenu.Items.Add("Copy");
                contexMenu.Items.Add("Paste");
                contexMenu.Items.Add("Delete");
                contexMenu.Items.Add(new ToolStripSeparator());
                contexMenu.Items.Add("Properties");
                contexMenu.Items.Add("Copy X,Y");
                contexMenu.Items.Add("Copy Extents");
                contexMenu.Items[6].Visible = false;
                contexMenu.ItemClicked += new ToolStripItemClickedEventHandler(contexMenu_ItemClicked);
                contexMenu.Opened += new EventHandler(contextMenu_Popup);

                if (EditorSettings.Default.Minimap_Show)
                {
                    MainWindow.Instance.minimap.Show(MainWindow.Instance);
                    MainWindow.Instance.minimap.setPos();
                    MainWindow.Instance.minimap.Invalidate();
                }
            }

            // initialize tabs
            buttons[0] = SelectObjectBtn;
            buttons[1] = PlaceObjectBtn;
            buttons[2] = selectWPBtn;
            buttons[3] = placeWPBtn;
            buttons[4] = pathWPBtn;

            SelectObjectBtn.Tag = Mode.OBJECT_SELECT;
            PlaceObjectBtn.Tag = Mode.OBJECT_PLACE;
            selectWPBtn.Tag = Mode.WAYPOINT_SELECT;
            placeWPBtn.Tag = Mode.WAYPOINT_PLACE;
            pathWPBtn.Tag = Mode.WAYPOINT_CONNECT;

            WallMakeNewCtrl.AutoWalltBtn.Tag = Mode.WALL_BRUSH;
            WallMakeNewCtrl.PlaceWalltBtn.Tag = Mode.WALL_PLACE;
            TileMakeNewCtrl.AutoTileBtn.Tag = Mode.FLOOR_BRUSH;
            TileMakeNewCtrl.PlaceTileBtn.Tag = Mode.FLOOR_PLACE;
            WallMakeNewCtrl.SetMapView(this);
            TileMakeNewCtrl.SetMapView(this);
            EdgeMakeNewCtrl.SetMapView(this);
            PolygonEditDlg = new PolygonEditor();

            // initialize buttons
            cmdQuickPreview.Checked = EditorSettings.Default.Edit_PreviewMode;
            radioExtentsShowAll.Checked = EditorSettings.Default.Draw_Extents_3D;
            // alter initial mode
            tabMapTools.SelectedTab = tabObjectWps;
            MapInterface.CurrentMode = Mode.OBJECT_SELECT;

            SelectObjectBtn.Checked = true;
        }

        public void TabsShortcuts(object sender, KeyEventArgs e)
        {
            int page = MainWindow.Instance.panelTabs.SelectedIndex;
            int mode = tabMapTools.SelectedIndex;

            var activeControl = WallMakeNewCtrl.ActiveControl;
            if (mode == 1)
                activeControl = TileMakeNewCtrl.ActiveControl;
            else if (mode == 2)
                activeControl = EdgeMakeNewCtrl.ActiveControl;
            else if (mode == 3)
                activeControl = ActiveControl;


            if (activeControl is TextBox || activeControl is NumericUpDown)
                return;

            if (page != 0)
                return;

            if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.NumPad1)
            {
                tabMapTools.SelectedTab = tabWalls;
                TabMapToolsSelectedIndexChanged(sender, e);
                return;
            }
            else if (e.KeyCode == Keys.D2 || e.KeyCode == Keys.NumPad2)
            {

                tabMapTools.SelectedTab = tabTiles;
                TabMapToolsSelectedIndexChanged(sender, e);
                return;
            }
            else if (e.KeyCode == Keys.D3 || e.KeyCode == Keys.NumPad3)
            {

                tabMapTools.SelectedTab = tabEdges;
                TabMapToolsSelectedIndexChanged(sender, e);
                return;
            }
            else if (e.KeyCode == Keys.D4 || e.KeyCode == Keys.NumPad4)
            {
                tabMapTools.SelectedTab = tabObjectWps;
                TabMapToolsSelectedIndexChanged(sender, e);
                return;
            }
            else if (e.KeyCode == Keys.Oemtilde || e.KeyCode == Keys.OemSemicolon || e.KeyCode == Keys.NumPad0)
            {
                ModeSwitcher();
                return;
            }
        }
        public void TabMapToolsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (PolygonEditDlg.Visible)
                return;

            var page = tabMapTools.SelectedTab;

            if (tileBucket && (page != tabTiles))
                TileMakeNewCtrl.Bucket.Checked = false;
            if (wallBucket && (page != tabWalls))
                WallMakeNewCtrl.Bucket.Checked = false;

            // Alter current mode depending on the tab testudo
            if (page == tabTiles)
                MapInterface.CurrentMode = (Mode)GetSelectedMode(TileMakeNewCtrl.buttons).Tag;
            else if (page == tabWalls)
            {
                if (WallMakeNewCtrl.WallProp.Visible)
                {
                    MapInterface.CurrentMode = Mode.WALL_CHANGE;
                    return;
                }

                MapInterface.CurrentMode = (Mode)GetSelectedMode(WallMakeNewCtrl.buttons).Tag;
            }
            else if (page == tabEdges)
                MapInterface.CurrentMode = Mode.EDGE_PLACE;
            else
                MapInterface.CurrentMode = (Mode)GetSelectedMode(buttons).Tag;
        }
        private void tabEdges_Enter(object sender, EventArgs e)
        {
            int selectedIndex = MainWindow.Instance.mapView.TileMakeNewCtrl.comboTileType.SelectedIndex;
            string tileName = MainWindow.Instance.mapView.TileMakeNewCtrl.comboTileType.Items[selectedIndex].ToString();
            EdgeMakeNewCtrl.ignoreAllBox.Text = "Ignore all but " + tileName;
            EdgeMakeNewCtrl.preserveBox.Text = "Preserve " + tileName;
            EdgeMakeNewCtrl.UpdateListView(sender, e);
        }

        private void mapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (!done) return;
            BlockTime = true;
            var page = tabMapTools.SelectedTab;
            var mouseCoords = new Point(e.X, e.Y);

            if (picking)
            {
                Map.Object obj = MapInterface.ObjectSelect(mouseCoords);

                // Alter current mode depending on the tab testudo
                if (page == tabTiles || page == tabEdges)
                {
                    Map.Tile tile = Map.Tiles.ContainsKey(GetNearestTilePoint(mouseCoords)) ? Map.Tiles[MapView.GetNearestTilePoint(mouseCoords)] : null;
                    if (tile == null) return;
                    TileMakeNewCtrl.findTileInList(tile.Graphic);
                    if (page == tabEdges) tabEdges_Enter(sender, e);
                }
                else if (page == tabWalls)
                {
                    Map.Wall wall = Map.Walls.ContainsKey(GetNearestWallPoint(mouseCoords)) ? Map.Walls[MapView.GetNearestWallPoint(mouseCoords)] : null;
                    if (wall == null) return;
                    if (MapInterface.KeyHelper.ShiftKey)
                    {
                        Button o = WallMakeNewCtrl.WallSelectButtons[(int)wall.Facing + (wall.Window ? 11 : 0)];
                        o.PerformClick();
                        o.Focus();
                    }
                    WallMakeNewCtrl.FindWallInList(wall.Material);
                    if (MapInterface.KeyHelper.ShiftKey)
                        WallMakeNewCtrl.numWallVari.Value = wall.Variation;
                }
                else if (page == tabEdges)
                {
                    return;
                }
                else
                {
                    if (MapInterface.CurrentMode > Mode.OBJECT_SELECT) return;
                    Map.Object obj0 = MapInterface.ObjectSelect(mouseCoords);
                    if (obj0 == null) return;
                    FindObjectInList(obj0.Name);
                }
                return;
            }
            if ((tileBucket) && (page == tabTiles))
            {
                if (e.Button.Equals(MouseButtons.Left))
                    TileBucketPaint(mouseCoords);
                else if (e.Button.Equals(MouseButtons.Right))
                    TileBucketDelete(mouseCoords);
                return;
            }
            if ((wallBucket) && (page == tabWalls))
            {
                if (e.Button.Equals(MouseButtons.Left))
                    WallBucketPaint(mouseCoords);
                //else if (e.Button.Equals(MouseButtons.Right))
                //    WallBucketDelete(mouseCoords);
                return;
            }

            Point pt = new Point(e.X, e.Y);
            Point ptAligned = pt;

            if (e.Button.Equals(MouseButtons.Middle))
            {
                // re-center camera
                CenterAtPoint(pt);
                mapPanel.Invalidate();
            }
            // Open properties if shift is hold, show context menu otherwise
            if (MapInterface.CurrentMode == Mode.OBJECT_SELECT && e.Button.Equals(MouseButtons.Right))
            {
                // TODO: ShowObjectProperties(MapInterface.ObjectSelect(pt));
                contexMenu.Show(mapPanel, pt);
            }

            if (MapInterface.CurrentMode == Mode.OBJECT_SELECT || MapInterface.CurrentMode == Mode.OBJECT_PLACE || MapInterface.CurrentMode == Mode.POLYGON_RESHAPE || MapInterface.CurrentMode == Mode.WAYPOINT_PLACE)
            {
                string objName = cboObjCreate.Text;
                if (ThingDb.Things.ContainsKey(objName))
                {
                    // Snap to grid
                    if (EditorSettings.Default.Edit_SnapGrid || ThingDb.Things[objName].Xfer == "DoorXfer")
                        ptAligned = new Point((int)Math.Round((decimal)(pt.X / squareSize)) * squareSize, (int)Math.Round((decimal)(pt.Y / squareSize)) * squareSize);
                    if (EditorSettings.Default.Edit_SnapHalfGrid)
                        ptAligned = new Point((int)Math.Round((decimal)((pt.X / (squareSize)) * squareSize) + squareSize / 2), (int)Math.Round((decimal)((pt.Y / (squareSize)) * squareSize) + squareSize / 2));
                    if (EditorSettings.Default.Edit_SnapCustom)
                    {
                        int snap = (int)customSnapValue.Value;
                        ptAligned = new Point((int)Math.Round((decimal)(pt.X / snap)) * snap, (int)Math.Round((decimal)(pt.Y / snap)) * snap);
                    }
                }
            }
            wallcount = Map.Walls.Count;
            tilecount = Map.Tiles.Count;
            added = false;
            if ((MapInterface.CurrentMode <= Mode.OBJECT_PLACE || MapInterface.CurrentMode == Mode.WAYPOINT_PLACE || MapInterface.CurrentMode == Mode.POLYGON_RESHAPE))
            {
                if (MapInterface.CurrentMode == Mode.WALL_CHANGE && MapInterface.KeyHelper.ShiftKey)
                    goto done;

                if (e.Button.Equals(MouseButtons.Left))
                {
                    if (MapInterface.CurrentMode == Mode.POLYGON_RESHAPE && MapInterface.KeyHelper.ShiftKey)
                        added = ApplyStore();
                    else if (MapInterface.CurrentMode != Mode.POLYGON_RESHAPE)
                        added = ApplyStore();

                }
                else if (e.Button.Equals(MouseButtons.Right))
                {
                    //vyradit zdi z podminky stejne jako tiles
                    if (MapInterface.CurrentMode != Mode.FLOOR_PLACE && MapInterface.CurrentMode != Mode.FLOOR_BRUSH && MapInterface.CurrentMode != Mode.WALL_BRUSH && !GetEdgeUnderCursor() && GetWallUnderCursor() == null && GetObjectUnderCursor() == null && GetWPUnderCursor() == null)
                        goto done;

                    added = ApplyStore();
                }
            }

            done:
            // reshape polygon if special mode is active
            if (MapInterface.CurrentMode == Mode.POLYGON_RESHAPE)
            {
                if (PolygonEditDlg.SelectedPolygon != null && e.Button.Equals(MouseButtons.Left))
                {

                    if (MapInterface.KeyHelper.ShiftKey == true)
                    {
                        int numPoints = PolygonEditDlg.SelectedPolygon.Points.Count;
                        if (arrowPoly > numPoints) arrowPoly = numPoints;
                        PolygonEditDlg.SelectedPolygon.Points.Insert(arrowPoly, new PointF(ptAligned.X, ptAligned.Y));
                        if (numPoints > 2) MapInterface.OpUpdatedPolygons = true;
                    }
                    else
                        arrowPoly = MapInterface.PolyPointSelect(pt);
                }
            }

            // pass into mapinterface handlers (operations)
            if (e.Button.Equals(MouseButtons.Left))
            {
                if (MapInterface.CurrentMode == Mode.POLYGON_RESHAPE && PolygonEditDlg.SelectedPolygon != null && !MapInterface.KeyHelper.ShiftKey)
                {
                    if (MapInterface.SelectedPolyPoint.IsEmpty && PolygonEditDlg.SelectedPolygon.Points.Count > 2)
                    {
                        if (PolygonEditDlg.SelectedPolygon == PolygonEditDlg.SuperPolygon && PolygonEditDlg.SelectedPolygon != null && !PolygonEditDlg.LockedBox.Checked && !PolygonEditDlg.SelectedPolygon.IsPointInside(pt))
                            PolygonEditDlg.SuperPolygon = null;
                        else if (PolygonEditDlg.Visible && PolygonEditDlg.SelectedPolygon != null && PolygonEditDlg.SelectedPolygon.IsPointInside(pt))
                            PolygonEditDlg.SuperPolygon = PolygonEditDlg.SelectedPolygon;
                        else if (PolygonEditDlg.SuperPolygon != PolygonEditDlg.SelectedPolygon)
                            PolygonEditDlg.SelectedPolygon = null;
                    }
                }
                mouseKeep = new Point(e.X, e.Y);
                MapInterface.HandleLMouseClick(MapInterface.CurrentMode == Mode.OBJECT_PLACE ? ptAligned : pt);

            }
            else if (e.Button.Equals(MouseButtons.Right))
                MapInterface.HandleRMouseClick(pt);

            MapRenderer.UpdateCanvas(MapInterface.OpUpdatedObjects, MapInterface.OpUpdatedTiles);
            moved = false;
        }
        private void mapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (!done) return;
            mouseLocation = new Point(e.X, e.Y);
            label2.Text = currentStep.ToString();

            if (PolygonEditDlg.Visible)
            {
                Point mousePt = PolygonEditDlg.PointToScreen(mouseLocation);
                mousePt = MainWindow.Instance.PointToClient(mousePt);
                if (PolygonEditDlg.ClientRectangle.Contains(mousePt))
                    return;

                MainWindow.Instance.Focus();
            }

            if (MainWindow.Instance != null && MainWindow.Instance.minimap.big)
            {
                Point screen = MainWindow.Instance.minimap.PointToScreen(mouseLocation);
                Point client = MainWindow.Instance.PointToClient(screen);
                if (!MainWindow.Instance.minimap.ClientRectangle.Contains(client))
                {
                    if (EditorSettings.Default.Minimap_Autohide && MainWindow.Instance.minimap.Width >= 100)
                    {
                        UpdateCanvasOKMiniMap = false;
                        MainWindow.Instance.minimap.big = false;
                        MainWindow.Instance.minimap.minimapBox.Invalidate();
                    }
                    if (EditorSettings.Default.Minimap_Autoalpha && MainWindow.Instance.minimap.Opacity > 0.5)
                        MainWindow.Instance.minimap.Opacity = 0.4;
                    if (Form.ActiveForm != MainWindow.Instance)
                        MainWindow.Instance.Focus();
                }
            }

            if (pasteAreaMode)
            {
                pasteDest = new Point(e.X, e.Y);
                // Move preview of copied area relative to cursor
                OffsetFakeCoords(pasteDest);
                MapInterface.OpUpdatedTiles = true;
            }

            if (!e.Button.Equals(MouseButtons.Left))
            {
                statusHelper.Update(mouseLocation);
                statusLocation.Text = statusHelper.StatusLocation;
                if (statusHelper.ValuesChanged())
                {

                    statusMapItem.Text = statusHelper.StatusMapItem;
                    statusPolygon.Text = statusHelper.StatusPolygon;
                }
                if (!mouseKeep.IsEmpty)
                {
                    mouseKeepOff = mouseKeep;
                    mouseKeep = new Point();
                }
            }

            if (MapInterface.CurrentMode == Mode.WALL_BRUSH)
            {
                if (e.Button.Equals(MouseButtons.Left))
                {
                    if (!WallMakeNewCtrl.WallSelectButtons[0].Focused && !WallMakeNewCtrl.WallSelectButtons[11].Focused && !WallMakeNewCtrl.WallSelectButtons[12].Focused)
                    {
                        Button o = WallMakeNewCtrl.WallSelectButtons[0];

                    }
                }
                if (WallMakeNewCtrl.RecWall.Checked)
                    MapInterface.WallRectangle(mouseLocation);
                else if (WallMakeNewCtrl.LineWall.Checked)
                    MapInterface.WallLine(GetNearestWallPoint(mouseLocation, true));

            }

            if (MapInterface.CurrentMode == Mode.OBJECT_PLACE)
            {
                if (UpdateCanvasOKMiniMap)
                {
                    MapRenderer.UpdateCanvas(true, false, false);
                    mapPanel.Invalidate();
                }
                tmrInvalidate.Interval = 500;
            }
            else
                tmrInvalidate.Interval = 100;

            if (MapInterface.CurrentMode == Mode.OBJECT_SELECT && !picking && !mouseKeep.IsEmpty)
            {
                MapInterface.RecSelected.Clear();
                MapInterface.ObjectSelect45Rectangle(mouseLocation);
                mapPanel.Invalidate();
            }

            if (e.Button.Equals(MouseButtons.Left) && !picking)
            {
                mouseMove = true;
                if (contextMenuOpen || moved) goto nah;

                if (!mouseKeep.IsEmpty && !SelectedObjects.IsEmpty && (MapInterface.CurrentMode == Mode.OBJECT_SELECT))
                {
                    ApplyStore();
                    moved = true;
                }
                else if (!mouseKeep.IsEmpty && MapInterface.SelectedWaypoint != null && (MapInterface.CurrentMode == Mode.WAYPOINT_SELECT))
                {
                    ApplyStore();
                    moved = true;
                }
                else if (!mouseKeep.IsEmpty && (!MapInterface.SelectedPolyPoint.IsEmpty || PolygonEditDlg.SuperPolygon != null) && (MapInterface.CurrentMode == Mode.POLYGON_RESHAPE))
                {
                    if (PolygonEditDlg.SuperPolygon != null)
                    {

                        if (PolygonEditDlg.SuperPolygon.IsPointInside(mouseLocation))
                        {
                            ApplyStore();
                            moved = true;
                        }
                        else if (!MapInterface.SelectedPolyPoint.IsEmpty)
                        {
                            ApplyStore();
                            moved = true;
                        }
                    }
                    else if (!MapInterface.SelectedPolyPoint.IsEmpty)
                    {
                        ApplyStore();
                        moved = true;

                    }
                }
                nah:
                if (!contexMenu.Visible)
                    contextMenuOpen = false;

                if (Get45RecSize() >= 5) moved = false;
                Point pt = mouseLocation;
                Point ptAligned = pt;

                // call handlers for some mouse operations
                if (MapInterface.CurrentMode == Mode.FLOOR_BRUSH || MapInterface.CurrentMode == Mode.FLOOR_PLACE || MapInterface.CurrentMode == Mode.WALL_BRUSH || (EdgeMakeNewCtrl.chkAutoEdge.Checked && MapInterface.CurrentMode == Mode.EDGE_PLACE))
                    MapInterface.HandleLMouseClick(pt);

                // Snap to grid
                if (EditorSettings.Default.Edit_SnapGrid)
                    ptAligned = new Point((int)Math.Round((decimal)(pt.X / squareSize)) * squareSize, (int)Math.Round((decimal)(pt.Y / squareSize)) * squareSize);
                if (EditorSettings.Default.Edit_SnapHalfGrid)
                    ptAligned = new Point((int)Math.Round((decimal)((pt.X / (squareSize)) * squareSize) + squareSize / 2), (int)Math.Round((decimal)((pt.Y / (squareSize)) * squareSize) + squareSize / 2));
                if (EditorSettings.Default.Edit_SnapCustom)
                {
                    int snap = (int)customSnapValue.Value;
                    ptAligned = new Point((int)Math.Round((decimal)(pt.X / snap)) * snap, (int)Math.Round((decimal)(pt.Y / snap)) * snap);
                }

                // moving waypoints
                if (MapInterface.CurrentMode == Mode.WAYPOINT_SELECT)
                {
                    // multi-move
                    if (MapInterface.SelectedWaypoints.Count > 0)
                    {
                        foreach (var wp in MapInterface.SelectedWaypoints)
                        {
                            wp.Point.X += e.X - mouseKeep.X;
                            wp.Point.Y += e.Y - mouseKeep.Y;
                        }
                        mouseKeep = new Point(e.X, e.Y);
                        mapPanel.Invalidate();
                    }
                    // single-move
                    else if (MapInterface.SelectedWaypoint != null)
                    {
                        MapInterface.SelectedWaypoint.Point.X = ptAligned.X; // Move the waypoint
                        MapInterface.SelectedWaypoint.Point.Y = ptAligned.Y;

                        mapPanel.Invalidate(); // Repaint the screen
                    }
                }
                // moving polypoints tudo
                if (MapInterface.CurrentMode == Mode.POLYGON_RESHAPE)
                {
                    if (!MapInterface.SelectedPolyPoint.IsEmpty && !MapInterface.KeyHelper.ShiftKey && e.Y < 5870 && e.X < 5885 && e.X > 10 && e.Y > 10)
                    {
                        PointF AlignedPt = ptAligned;
                        if (PolygonEditDlg.snapPoly.Checked)
                            AlignedPt = MapInterface.PolyPointSnap(mouseLocation).IsEmpty ? ptAligned : MapInterface.PolyPointSnap(mouseLocation);

                        Map.Polygon poly = PolygonEditDlg.SelectedPolygon;
                        poly.Points[arrowPoly] = AlignedPt;
                        MapInterface.SelectedPolyPoint = AlignedPt;
                        mapPanel.Invalidate();

                    }
                    if (PolygonEditDlg.SuperPolygon != null && MapInterface.SelectedPolyPoint.IsEmpty)
                    {
                        if (PolygonEditDlg.SuperPolygon.IsPointInside(mouseLocation))
                        {
                            for (int i = 0; i < PolygonEditDlg.SuperPolygon.Points.Count; i++)
                            {
                                PointF pts = PolygonEditDlg.SuperPolygon.Points[i];
                                if (PolyPointOffset.Count <= PolygonEditDlg.SuperPolygon.Points.Count)
                                {
                                    float polyrelX = (pts.X - mouseLocation.X) * -1;
                                    float polyrelY = (pts.Y - mouseLocation.Y) * -1;
                                    PolyPointOffset.Add(new PointF(polyrelX, polyrelY));
                                }
                                PolygonEditDlg.SuperPolygon.Points[i] = new PointF(mouseLocation.X - PolyPointOffset[i].X, mouseLocation.Y - PolyPointOffset[i].Y);
                            }
                            mapPanel.Cursor = Cursors.SizeAll;
                            mapPanel.Invalidate();
                        }
                    }
                }
                // moving objects
                bool aligned = false;

                if (!SelectedObjects.IsEmpty && Get45RecSize() < 5 && e.Y < 5870 && e.X < 5885 && e.X > 10 && e.Y > 10)
                {
                    if (MapInterface.CurrentMode == Mode.OBJECT_SELECT)
                    {
                        if (SelectedObjects.Origin != null)
                        {
                            float closestX = SelectedObjects.Origin.Location.X;
                            float closestY = SelectedObjects.Origin.Location.Y;

                            // update position of all objects relative
                            if (relXX == 0 && relYY == 0)
                            {
                                relXX = closestX - pt.X;
                                relYY = closestY - pt.Y;
                            }

                            if (EditorSettings.Default.Edit_SnapGrid)
                            {
                                aligned = true;
                                ptAligned = new Point((int)Math.Round((decimal)((pt.X + (int)relXX) / squareSize)) * squareSize, (int)Math.Round((decimal)((pt.Y + (int)relYY) / squareSize)) * squareSize);
                            }
                            if (EditorSettings.Default.Edit_SnapHalfGrid)
                            {
                                aligned = true;
                                ptAligned = new Point((int)Math.Round((decimal)(((pt.X + (int)relXX) / squareSize) * squareSize) + squareSize / 2), (int)Math.Round((decimal)(((pt.Y + (int)relYY) / squareSize) * squareSize) + squareSize / 2));
                            }
                            if (EditorSettings.Default.Edit_SnapCustom)
                            {
                                aligned = true;
                                int snap = (int)customSnapValue.Value;
                                ptAligned = new Point((int)Math.Round((decimal)((pt.X + relXX) / snap)) * snap, (int)Math.Round((decimal)((pt.Y + relYY) / snap)) * snap);
                            }
                            foreach (Map.Object co in SelectedObjects)
                            {
                                float relX = (closestX - co.Location.X) - (!aligned ? (float)relXX : 0);
                                float relY = (closestY - co.Location.Y) - (!aligned ? (float)relYY : 0);
                                PointF ResultLoc = new PointF(ptAligned.X - relX, ptAligned.Y - relY);
                                if (!(ResultLoc.Y < 5870 && ResultLoc.X < 5885 && ResultLoc.X > 10 && ResultLoc.Y > 10)) continue;
                                co.Location = ResultLoc;
                            }
                            MapRenderer.UpdateCanvas(true, false, false);
                        }
                    }
                }
            }
            else
            {
                if ((mapPanel.Cursor == Cursors.SizeAll) && (!pasteAreaMode))
                    mapPanel.Cursor = Cursors.Default;
                if (PolyPointOffset.Count > 0) PolyPointOffset.Clear();
                relXX = 0;
                relYY = 0;
            }
            if (e.Button.Equals(MouseButtons.Right) && !picking)
            {
                if (MapInterface.CurrentMode == Mode.FLOOR_BRUSH || MapInterface.CurrentMode == Mode.FLOOR_PLACE || MapInterface.CurrentMode == Mode.WALL_BRUSH)
                    MapInterface.HandleRMouseClick(mouseLocation);
            }

            // update the visible map part
            MapRenderer.UpdateCanvas(MapInterface.OpUpdatedObjects, MapInterface.OpUpdatedTiles);
            if (WallMakeNewCtrl.RecWall.Checked || WallMakeNewCtrl.LineWall.Checked && mouseKeep.IsEmpty && MapInterface.CurrentMode == Mode.WALL_BRUSH)
                MapInterface.ResetUpdateTracker();
        }
        private void mapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            mouseMove = false;

            if (copyAreaMode)
            {
                if (select45Box.Checked)
                    MapInterface.SetSelect45Area(e.Location);
                    
                CopyArea(MapInterface.selected45Area);
            }
            if (pasteAreaMode)
                PasteArea(pasteDest);

            foreach (Map.Object objct in MapInterface.RecSelected)
            {
                if (SelectedObjects.Items.Contains(objct) && MapInterface.KeyHelper.ShiftKey)
                    DeletefromSelected(objct);
                else if (!SelectedObjects.Items.Contains(objct))
                    SelectedObjects.Items.Add(objct);
            }
            
            MapInterface.RecSelected.Clear();
            MapInterface.selected45Area = new Point[4];

            //////////////delete rnpty time///////////////////////úú

            if (!MapInterface.OpUpdatedTiles && !MapInterface.OpUpdatedWalls && !MapInterface.OpUpdatedObjects && !MapInterface.OpUpdatedPolygons && !MapInterface.OpUpdatedWaypoints && !moved && added)
            {
                if (MapInterface.CurrentMode == Mode.WALL_BRUSH)
                {
                    if (WallMakeNewCtrl.LineWall.Checked || WallMakeNewCtrl.RecWall.Checked) goto noPre;
                }
                while (TimeManager.Count > 0 && TimeManager[(TimeManager.Count - 1) - currentStep].Event == TimeEvent.PRE)
                {
                    if (TimeManager[(TimeManager.Count - 1) - currentStep].Event == TimeEvent.PRE && TimeManager.Count > 0)
                    {
                        TimeManager.RemoveAt((TimeManager.Count - 1) - currentStep);
                    }
                }
                if (TimeManager.Count <= 1)
                {
                    StopUndo = true;
                    MainWindow.Instance.miniUndo.Enabled = false;
                    cmdUndo.Enabled = false;
                }
            }
            BlockTime = false;
            noPre:

            //////////////////////////////////////////////////

            if (WallMakeNewCtrl.LineWall.Checked || WallMakeNewCtrl.RecWall.Checked)
                LastWalls.Clear();

            if (picking) goto hop;

            //  MessageBox.Show(MapInterface.OpUpdatedObjects.ToString() + " menu:" + contextMenuOpen.ToString());
            if (!MapInterface.OpUpdatedTiles && !MapInterface.OpUpdatedWalls && !MapInterface.OpUpdatedObjects && !MapInterface.OpUpdatedPolygons && !MapInterface.OpUpdatedWaypoints && (!moved))
                goto hop;

            if (MapInterface.CurrentMode <= Mode.OBJECT_SELECT || MapInterface.CurrentMode == Mode.WAYPOINT_CONNECT || MapInterface.CurrentMode == Mode.WAYPOINT_PLACE || MapInterface.CurrentMode == Mode.POLYGON_RESHAPE || MapInterface.CurrentMode == Mode.WAYPOINT_SELECT)
            {
                if ((WallMakeNewCtrl.LineWall.Checked || WallMakeNewCtrl.RecWall.Checked) && MapInterface.CurrentMode == Mode.WALL_BRUSH)
                    goto hop;

                if (MapInterface.CurrentMode >= Mode.WALL_PLACE && MapInterface.CurrentMode < Mode.OBJECT_PLACE)
                {
                    if (MapInterface.OpUpdatedTiles || MapInterface.OpUpdatedWalls || MapInterface.OpUpdatedObjects)
                    {
                        Store(MapInterface.CurrentMode, TimeEvent.POST);
                        if (MainWindow.Instance != null)
                        {
                            MainWindow.Instance.minimap.Reload();
                        }
                    }
                    goto hop;
                }

                if (moved)
                {
                    if (!SelectedObjects.IsEmpty && MapInterface.CurrentMode == Mode.OBJECT_SELECT)
                        Store(MapInterface.CurrentMode, TimeEvent.POST);
                    else if (MapInterface.SelectedWaypoint != null && MapInterface.CurrentMode == Mode.WAYPOINT_SELECT)
                        Store(MapInterface.CurrentMode, TimeEvent.POST);
                    else if ((!MapInterface.SelectedPolyPoint.IsEmpty || PolygonEditDlg.SuperPolygon != null) && !MapInterface.KeyHelper.ShiftKey && MapInterface.CurrentMode == Mode.POLYGON_RESHAPE)
                        Store(MapInterface.CurrentMode, TimeEvent.POST);
                }
                if (MapInterface.KeyHelper.ShiftKey && MapInterface.CurrentMode == Mode.POLYGON_RESHAPE && MapInterface.OpUpdatedPolygons)
                    Store(MapInterface.CurrentMode, TimeEvent.POST);
                if (MapInterface.CurrentMode == Mode.OBJECT_PLACE && MapInterface.OpUpdatedObjects)
                    Store(MapInterface.CurrentMode, TimeEvent.POST);
                else if (MapInterface.CurrentMode == Mode.WAYPOINT_PLACE && MapInterface.OpUpdatedWaypoints)
                    Store(MapInterface.CurrentMode, TimeEvent.POST);
                else if (MapInterface.SelectedWaypoint != null && MapInterface.CurrentMode == Mode.WAYPOINT_CONNECT)
                    Store(MapInterface.CurrentMode, TimeEvent.POST);

            }
            hop:
            moved = false;
            MapRenderer.FakeWalls.Clear();
            mapPanel.Invalidate();
            MainWindow.Instance.minimap.Reload();

            if (!mouseKeep.IsEmpty)
            {
                mouseKeepOff = mouseKeep;
                mouseKeep = new Point();
            }
            if (MapInterface.CurrentMode != Mode.WALL_PLACE && MapInterface.CurrentMode != Mode.WALL_BRUSH)
                mouseKeepOff = new Point();

            if (picking)
                Picker.Checked = false;

            MapInterface.ResetUpdateTracker();
        }
        private void mapPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (PolygonEditDlg.Visible && MapInterface.CurrentMode == Mode.POLYGON_RESHAPE && PolygonEditDlg.SelectedPolygon != null && PolygonEditDlg.SelectedPolygon.Points.Count > 2)
            {
                if (PolygonEditDlg.SelectedPolygon.IsPointInside(new Point(e.X, e.Y)))
                    PolygonEditDlg.ButtonModifyClick(sender, e);
            }

            if (MapInterface.CurrentMode == Mode.OBJECT_SELECT)
                ShowObjectProperties(MapInterface.ObjectSelect(new Point(e.X, e.Y)));
        }
        private void mapPanel_Paint(object sender, PaintEventArgs e)
        {
            // Something goes wrong / there is no map
            if (Map == null || !renderingOk) return;

            try
            {
                MapRenderer.RenderTo(e.Graphics);
            }
            catch (Exception ex)
            {
                renderingOk = false;
                new ExceptionDialog(ex, "Exception in rendering routine").ShowDialog();
                Environment.Exit(-1);
            }
        }
        private void scrollPanel_Scroll(object sender, ScrollEventArgs e)
        {
            if (MainWindow.Instance != null)
                MainWindow.Instance.minimap.minimapBox.Invalidate();
            tmrInvalidate.Interval = 1000;
            // update visible area
            MapRenderer.UpdateCanvas(true, true, false);
            mapPanel.Invalidate(new Rectangle(scrollPanel.HorizontalScroll.Value, scrollPanel.VerticalScroll.Value, scrollPanel.Width, scrollPanel.Height));
        }
        private void tmrInvalidate_Tick(object sender, EventArgs e)
        {
            if (!done && MainWindow.Instance != null)
                MainWindow.Instance.minimap.setPos();
            done = true;
            if (MapInterface.ModeIsUpdated)
            {
                // Update mode text.
                statusMode.Text = string.Format("Mode: {0}", MapInterface.CurrentMode);
                MapInterface.ModeIsUpdated = false;
            }

            if (!UpdateCanvasOKMiniMap)
                return;
            mapPanel.Invalidate();
        }

        private void MapView_KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedObjects();
                mapPanel.Invalidate();
            }
            if (e.KeyCode == Keys.Escape)
            {
                Picker.Checked = false;
                TileMakeNewCtrl.Bucket.Checked = false;
                WallMakeNewCtrl.Bucket.Checked = false;
                WallMakeNewCtrl.LineWall.Checked = false;
                WallMakeNewCtrl.RecWall.Checked = false;
                cmdCopyArea.Checked = false;
                cmdPasteArea.Checked = false;
                if (MapInterface.CurrentMode == Mode.WALL_CHANGE)
                    WallMakeNewCtrl.ExitProperties();
            }
        }
        private void StartShiftSelecting(object sender, KeyEventArgs e)
        {
            if (e.Shift)
            {
                if (MapInterface.CurrentMode == Mode.WALL_BRUSH)
                    WallMakeNewCtrl.smartDraw.Checked = true;
                MapInterface.KeyHelper.ShiftKey = true;
            }
        }
        private void StopShiftSelecting(object sender, KeyEventArgs e)
        {
            if (!e.Shift)
            {
                if (MapInterface.CurrentMode == Mode.WALL_BRUSH)
                    WallMakeNewCtrl.smartDraw.Checked = false;
                MapInterface.KeyHelper.ShiftKey = false;
            }
        }
        private void MouseWheelEventHandler(object sender, MouseEventArgs e)
        {
            if (MapInterface.CurrentMode == Mode.OBJECT_PLACE || MapInterface.CurrentMode == Mode.OBJECT_SELECT)
            {
                object thingName = cboObjCreate.SelectedItem;
                // Update object image
                if (thingName != null)
                {

                    if (ThingDb.Things[(string)thingName].Xfer == "DoorXfer")
                    {
                        sbyte facing = (sbyte)delta;
                        if (e.Delta >= 90) facing += 8;
                        if (e.Delta <= 90) facing -= 8;

                        if (facing > 24) facing = 0;
                        if (facing < 0) facing = 24;
                        delta = facing;
                    }
                    else if (ThingDb.Things[(string)thingName].Xfer == "MonsterXfer" || ThingDb.Things[(string)thingName].Xfer == "NPCXfer")
                    {
                        int facing = Array.IndexOf(directions, (int)delta);

                        if (e.Delta >= 90) facing += 1;
                        if (e.Delta <= 90) facing -= 1;

                        if (facing > 7) facing = 0;
                        if (facing < 0) facing = 7;
                        delta = (byte)directions[facing];
                    }
                    else if (ThingDb.Things[(string)thingName].Xfer == "SentryXfer")
                    {
                        int rotatDegrees = (int)(delta * 180 / Math.PI);
                        rotatDegrees += e.Delta / 30;
                        if (rotatDegrees > 360) rotatDegrees -= 360;
                        if (e.Delta >= 90)
                            rotatDegrees = 5 * (int)(decimal.Ceiling((decimal)(rotatDegrees - 0.1) / 5));

                        if (e.Delta <= 90)
                            rotatDegrees = -5 * (int)(decimal.Ceiling((decimal)(rotatDegrees + 0.1) / -5));

                        // deg2rad

                        float kagor = (float)(rotatDegrees * Math.PI / 180);
                        delta = kagor;
                    }
                }

                // rotate some objects
                if (MapInterface.CurrentMode == Mode.OBJECT_SELECT && !SelectedObjects.IsEmpty)
                {
                    foreach (Map.Object obj in SelectedObjects)
                    {
                        if (obj.CanBeRotated())
                        {
                            // doors
                            if (ThingDb.Things[obj.Name].Xfer == "DoorXfer")
                            {
                                DoorXfer door = obj.GetExtraData<DoorXfer>();
                                sbyte facing = (sbyte)door.Direction;
                                if (e.Delta >= 90) facing += 8;
                                if (e.Delta <= 90) facing -= 8;

                                if (facing > 24) facing = 0;
                                if (facing < 0) facing = 24;
                                door.Direction = (DoorXfer.DOORS_DIR)facing;

                            }
                            // sentry beams
                            else if (ThingDb.Things[obj.Name].Xfer == "SentryXfer")
                            {
                                SentryXfer xfer = obj.GetExtraData<SentryXfer>();
                                // rad2deg
                                int rotatDegrees = (int)(xfer.BasePosRadian * 180 / Math.PI);
                                // rotate(lel)
                                rotatDegrees += e.Delta / 30;
                                if (rotatDegrees > 360) rotatDegrees -= 360;
                                if (e.Delta >= 90)
                                    rotatDegrees = 5 * (int)(decimal.Ceiling((decimal)(rotatDegrees - 0.1) / 5));

                                if (e.Delta <= 90)
                                    rotatDegrees = -5 * (int)(decimal.Ceiling((decimal)(rotatDegrees + 0.1) / -5));

                                // deg2rad

                                float kagor = (float)(rotatDegrees * Math.PI / 180);
                                xfer.BasePosRadian = kagor;
                            }

                            //monsters, NPC

                            else if (ThingDb.Things[obj.Name].Xfer == "MonsterXfer")
                            {
                                MonsterXfer monster = obj.GetExtraData<MonsterXfer>();
                                int facing = Array.IndexOf(directions, monster.DirectionId);

                                if (e.Delta >= 90) facing += 1;
                                if (e.Delta <= 90) facing -= 1;

                                if (facing > 7) facing = 0;
                                if (facing < 0) facing = 7;
                                monster.DirectionId = (byte)directions[facing];

                            }
                            else if (ThingDb.Things[obj.Name].Xfer == "NPCXfer")
                            {
                                NPCXfer npc = obj.GetExtraData<NPCXfer>();
                                int facing = Array.IndexOf(directions, npc.DirectionId);

                                if (e.Delta >= 90) facing += 1;
                                if (e.Delta <= 90) facing -= 1;

                                if (facing > 7) facing = 0;
                                if (facing < 0) facing = 7;
                                npc.DirectionId = (byte)directions[facing];

                            }
                        }
                    }
                }
            }
            else if (MapInterface.CurrentMode == Mode.FLOOR_BRUSH || MapInterface.CurrentMode == Mode.FLOOR_PLACE)
            {
                if (e.Delta >= 90 && TileMakeNewCtrl.BrushSize.Value < 6) TileMakeNewCtrl.BrushSize.Value += 1;
                if (e.Delta <= 90 && TileMakeNewCtrl.BrushSize.Value > 1) TileMakeNewCtrl.BrushSize.Value -= 1;
            }
            else if (MapInterface.CurrentMode == Mode.WALL_PLACE)
            {
                int index = WallMakeNewCtrl.wallFacing;
                if (e.Delta >= 90)
                    ++index;
                if (e.Delta < 90)
                    --index;
                if (index > 10)
                    index = 0;
                if (index < 0)
                    index = 10;
                Button wallSelectButton = WallMakeNewCtrl.WallSelectButtons[index];
                wallSelectButton.PerformClick();
                wallSelectButton.Focus();
                mapPanel.Invalidate();
            }
        }
        private void cboObjMouseWheel(object sender, MouseEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            Point mousePt = new Point(e.X, e.Y);
            mousePt = combo.PointToScreen(mousePt);
            mousePt = MainWindow.Instance.PointToClient(mousePt);

            if (groupAdv.ClientRectangle.Contains(mousePt)) return;
            object thingName = cboObjCreate.SelectedItem;
            if (ThingDb.Things[(string)thingName].Xfer == "DoorXfer" ||
                ThingDb.Things[(string)thingName].Xfer == "NPCXfer" ||
                ThingDb.Things[(string)thingName].Xfer == "MonsterXfer" ||
                ThingDb.Things[(string)thingName].Xfer == "SentryXfer")
            {
                ((HandledMouseEventArgs)e).Handled = true;
                MouseWheelEventHandler(sender, e);
            }
            else
            {
                MapRenderer.UpdateCanvas(true, false);
                mapPanel.Invalidate();
            }
        }
        private void CboObjCreateSelectedIndexChanged(object sender, EventArgs e)
        {
            object thingName = cboObjCreate.SelectedItem;
            // Update object image
            delta = 0;
            if (thingName != null)
            {
                PlaceObjectBtn.Checked = true;

                ThingDb.Thing tt = ThingDb.Things[(string)thingName];
                Bitmap icon = null;
                if (tt.SpritePrettyImage > 0 && (tt.Class & ThingDb.Thing.ClassFlags.MONSTER) == 0)
                {
                    icon = MapRenderer.VideoBag.GetBitmap(tt.SpritePrettyImage);
                }
                else if (tt.SpriteMenuIcon > 0)
                {
                    icon = MapRenderer.VideoBag.GetBitmap(tt.SpriteMenuIcon);

                    if (tt.Xfer != "InvisibleLightXfer" && !tt.Name.StartsWith("Amb"))
                        if (tt.SpriteAnimFrames.Count > 0) icon = MapRenderer.VideoBag.GetBitmap(tt.SpriteAnimFrames[tt.SpriteAnimFrames.Count-1]);
                }
                objectPreview.BackgroundImage = icon;
            }
        }
        internal void LoadObjectCategories()
        {
            var filePath = System.IO.Path.Combine(Application.StartupPath, "categories.xml");
            try
            {
                if (!System.IO.File.Exists(filePath))
                    System.IO.File.WriteAllText(filePath, Properties.Resources.categories);

                // Load object list file
                ObjectCategory[] categories = XMLCategoryListReader.ReadCategories(filePath);
                objectCategoriesBox.Items.AddRange(categories);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to load object category listfile: " + ex.Message);
                Logger.Log(" Looking for: " + filePath);
            }
            // If object list file is either empty or failed to load, create "All Objects"
            if (objectCategoriesBox.Items.Count < 1)
            {
                ObjectCategory catDefault = new ObjectCategory("All Objects");
                catDefault.Rules.Add(new IncludeRule("", IncludeRule.IncludeRuleType.ANYTHING));
                objectCategoriesBox.Items.Add(catDefault);
            }
            objectCategoriesBox.SelectedIndex = 0;
        }
        private void ObjectCategoriesBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            object selItem = objectCategoriesBox.SelectedItem;
            delta = 0;
            if (selItem != null)
            {
                object prewItem = cboObjCreate.SelectedItem;
                cboObjCreate.Items.Clear();
                ObjectCategory category = (ObjectCategory)selItem;
                // update object list contents
                cboObjCreate.Items.AddRange(category.GetThings());
                // update selection
                if (prewItem != null)
                {
                    if (!FindObjectInList(prewItem.ToString(), true))
                        cboObjCreate.SelectedIndex = 0;
                }
            }
            if (cboObjCreate.Items.Count > 0)
            {
                cboObjCreate.SelectedIndex = 0;
                cboObjCreate.Select();
            }
        }
        public void ObjectModesChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            radioButton.Font = new Font(radioButton.Font.Name, radioButton.Font.Size, FontStyle.Regular);
            if (!radioButton.Checked) return;
            radioButton.Font = new Font(radioButton.Font.Name, radioButton.Font.Size, FontStyle.Bold);

            Picker.Checked = false;
            MapInterface.CurrentMode = (Mode)radioButton.Tag;

            if (radioButton.Name == "selectWPBtn" || radioButton.Name == "placeWPBtn" || radioButton.Name == "pathWPBtn")
            {
                PlaceObjectBtn.Checked = false;
                SelectObjectBtn.Checked = false;
            }
            else
            {
                selectWPBtn.Checked = false;
                placeWPBtn.Checked = false;
                pathWPBtn.Checked = false;
            }
            MapRenderer.UpdateCanvas(true, false);
            mapPanel.Invalidate();
        }

        #region Quick Menu
        private void cmdQuickSave_Click(object sender, EventArgs e)
        {
            MainWindow.Instance.menuSave.PerformClick();
        }
        private void cmdQuickPreview_Click(object sender, EventArgs e)
        {
            MainWindow.Instance.menuVisualPreviewMode.PerformClick();
        }
        private void cmdQuickSettings_Click(object sender, EventArgs e)
        {
            MainWindow.Instance.menuSettings.PerformClick();
        }
        private void cmdQuickPreview_CheckedChanged(object sender, EventArgs e)
        {
            if (cmdQuickPreview.Checked)
                cmdQuickPreview.BackgroundImage = Properties.Resources.showPreview;
            else
                cmdQuickPreview.BackgroundImage = Properties.Resources.hidePreview;
        }
        public void cmdRedo_Click(object sender, EventArgs e)
        {
            if (StopRedo || BlockTime) return;
            Redo();
        }
        public void cmdUndo_Click(object sender, EventArgs e)
        {
            if (StopUndo || BlockTime) return;
            Undo();
        }
        private void cmdUndo_EnabledChanged(object sender, EventArgs e)
        {
            if (cmdUndo.Enabled)
                cmdUndo.BackgroundImage = Properties.Resources.undo;
            else
                cmdUndo.BackgroundImage = Properties.Resources.undoDisabled;
        }
        private void cmdRedo_EnabledChanged(object sender, EventArgs e)
        {
            if (cmdRedo.Enabled)
                cmdRedo.BackgroundImage = Properties.Resources.redo;
            else
                cmdRedo.BackgroundImage = Properties.Resources.redoDisabled;
        }

        private double colorFadePercentage = 0.05;
        public void ShowMapStatus(string message)
        {
            lblMapStatus.Visible = true;
            lblMapStatus.Text = message;
            colorFadePercentage = 0;
            tmrFade.Start();
        }
        private void tmrFade_Tick(object sender, EventArgs e)
        {
            tmrFadeTicker.Start();
            tmrFade.Stop();
        }
        private void tmrFadeTicker_Tick(object sender, EventArgs e)
        {
            colorFadePercentage += 0.02;
            if (colorFadePercentage > 0.45)
            {
                lblMapStatus.Visible = false;
                lblMapStatus.ForeColor = Color.Green;
                tmrFadeTicker.Stop();
            }
            else
                lblMapStatus.ForeColor = lblMapStatus.ForeColor.Interpolate(BackColor, colorFadePercentage);
        }
        #endregion
        #region Right-click Menu
        private void contextMenu_Popup(object sender, EventArgs e)
        {
            contextMenuOpen = true;
            bool enable = true;

            if (SelectedObjects.IsEmpty)
                enable = false;

            // These are inaccessible if there are no selected objects
            contexMenu.Items[0].Enabled = enable;
            contexMenu.Items[2].Enabled = enable;
            contexMenu.Items[3].Enabled = enable;

            if (SelectedObjects.Items.Count > 1)
                contexMenu.Items[6].Visible = true;
            else
                contexMenu.Items[6].Visible = false;
        }
        private void contextMenuCopy_Click(object sender, EventArgs e)
        {
            if (!SelectedObjects.IsEmpty)
                Clipboard.SetDataObject(SelectedObjects.Clone(), false);
        }
        private void contextMenuStrip_Open(object sender, EventArgs e)
        {
            ToolStripItem clickedItem = sender as ToolStripItem;
            if (clickedItem != null)
                MessageBox.Show(clickedItem.ToString());
        }
        private void contextMenuPaste_Click(object sender, EventArgs e)
        {
            if (Clipboard.GetDataObject().GetDataPresent(typeof(MapObjectCollection)))
            {
                MapObjectCollection collection = (MapObjectCollection)Clipboard.GetDataObject().GetData(typeof(MapObjectCollection));
                // find closest object
                double dist = double.MaxValue;
                float closestX = 0, closestY = 0;
                //Store(MapInterface.CurrentMode, TimeEvent.PRE);
                ApplyStore();
                foreach (Map.Object ot in collection)
                {
                    float dx = ot.Location.X - mouseLocation.X;
                    float dy = ot.Location.Y - mouseLocation.Y;
                    double ndist = Math.Sqrt(dx * dx + dy * dy);
                    if (ndist < dist)
                    {
                        dist = ndist;
                        closestX = ot.Location.X;
                        closestY = ot.Location.Y;
                    }
                }
                // clear (old) selection, duplicate objects
                SelectedObjects.Items.Clear();
                SelectedObjects.Origin = null;
                Map.Object clone;
                foreach (Map.Object o in collection)
                {
                    clone = (Map.Object)o.Clone();
                    // calc relative coordinates
                    float relX = closestX - o.Location.X;
                    float relY = closestY - o.Location.Y;
                    clone.Location = new PointF(mouseLocation.X - relX, mouseLocation.Y - relY);
                    clone.Extent = MapInterface.GetNextObjectExtent();
                    Map.Objects.Add(clone);
                    // add into selection
                    SelectedObjects.Items.Add(clone);
                }
                // update canvas
                Store(MapInterface.CurrentMode, TimeEvent.POST);
                MapRenderer.UpdateCanvas(true, false);
                mapPanel.Invalidate();
            }
        }
        private void contextMenuProperties_Click(object sender, EventArgs e)
        {
            if (!SelectedObjects.IsEmpty)
                ShowObjectProperties(SelectedObjects.Items[0]);
        }
        private void contexMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;

            if (item.Text == "Paste")
                contextMenuPaste_Click(sender, e);
            else if (item.Text == "Copy")
                contextMenuCopy_Click(sender, e);
            else if (item.Text == "Delete")
                contextMenuDelete_Click(sender, e);
            else if (item.Text == "Properties")
                contextMenuProperties_Click(sender, e);
            else if (item.Text == "Copy X,Y")
                Copy_Coords(sender, e);
            else if (item.Text == "Copy Extents")
                menuItem1_Click(sender, e);
        }
        private void contextMenuDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedObjects();
            mapPanel.Invalidate();
        }
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mapPanel.Cursor = Cursors.Default;
        }
        private void menuItem1_Click(object sender, EventArgs e)
        {
            string content = "";
            foreach (Map.Object obj in SelectedObjects.Items)
            {
                content = content + obj.Extent.ToString() + Environment.NewLine;

            }
            Clipboard.SetDataObject(content, false);

        }
        private void Copy_Coords(object sender, EventArgs e)
        {
            copyPoint = mouseLocation;
            string content = copyPoint.X.ToString() + ", " + copyPoint.Y.ToString();
            Clipboard.SetDataObject(content, true);
        }
        #endregion
        #region 3D Extents/Grid Snap Settings
        private void radHideExtents_CheckedChanged(object sender, EventArgs e)
        {
            EditorSettings.Default.Draw_Extents = false;
            EditorSettings.Default.Draw_AllExtents = false;
        }
        private void radShowColliding_CheckedChanged(object sender, EventArgs e)
        {
            EditorSettings.Default.Draw_Extents = true;
            EditorSettings.Default.Draw_AllExtents = false;
        }
        private void radShowExtents_CheckedChanged(object sender, EventArgs e)
        {
            EditorSettings.Default.Draw_Extents = true;
            EditorSettings.Default.Draw_AllExtents = true;
        }
        private void radFullSnap_CheckedChanged(object sender, EventArgs e)
        {
            customSnapValue.Enabled = false;
            EditorSettings.Default.Edit_SnapCustom = false;
            EditorSettings.Default.Edit_SnapGrid = false;
            EditorSettings.Default.Edit_SnapHalfGrid = radFullSnap.Checked;
        }
        private void radCenterSnap_CheckedChanged(object sender, EventArgs e)
        {
            customSnapValue.Enabled = false;
            EditorSettings.Default.Edit_SnapCustom = false;
            EditorSettings.Default.Edit_SnapGrid = radCenterSnap.Checked;
            EditorSettings.Default.Edit_SnapHalfGrid = false;
        }
        private void radNoSnap_CheckedChanged(object sender, EventArgs e)
        {
            customSnapValue.Enabled = false;
            EditorSettings.Default.Edit_SnapCustom = false;
            EditorSettings.Default.Edit_SnapGrid = false;
            EditorSettings.Default.Edit_SnapHalfGrid = false;
        }
        private void radCustom_CheckedChanged(object sender, EventArgs e)
        {
            customSnapValue.Enabled = true;
            EditorSettings.Default.Edit_SnapCustom = radCustom.Checked;
            EditorSettings.Default.Edit_SnapGrid = false;
            EditorSettings.Default.Edit_SnapHalfGrid = false;
        }
        #endregion

        public Bitmap MapToImage()
        {
            // Renders entire map to a new bitmap, using current settings
            if (Map == null)
                return null;

            Bitmap bitmap = new Bitmap(5880, 5880);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                MapRenderer.RenderTo(g, true);
            }
            return bitmap;
        }
        public string RemoveSpace(string spaceChar)
        {
            string temp = spaceChar.Substring(0, 1);

            if (temp.IndexOf("*") != -1)
                return spaceChar.Substring(1, spaceChar.Length - 1);
            else
                return spaceChar;
        }
        public void OpenScripts()
        {
            strSd = new ScriptFunctionDialog();
            strSd.Scripts = Map.Scripts;
            strSd.ShowDialog(this);
            Map.Scripts = strSd.Scripts;
        }
        public void PlayerStartSelect()
        {
            tabMapTools.SelectedTab = tabObjectWps;
            PlaceObjectBtn.PerformClick();
            objectCategoriesBox.SelectedIndex = 0;
            FindObjectInList("PlayerStart", false);
        }
        public void ModeSwitcher()
        {
            var page = tabMapTools.SelectedTab;
            if (page == tabTiles)
            {
                TileMakeNewCtrl.PlaceTileBtn.Checked = !TileMakeNewCtrl.PlaceTileBtn.Checked;
                TileMakeNewCtrl.AutoTileBtn.Checked = !TileMakeNewCtrl.PlaceTileBtn.Checked;
            }
            else if (page == tabWalls)
            {
                WallMakeNewCtrl.PlaceWalltBtn.Checked = !WallMakeNewCtrl.PlaceWalltBtn.Checked;
                WallMakeNewCtrl.AutoWalltBtn.Checked = !WallMakeNewCtrl.PlaceWalltBtn.Checked;
            }
            else if (page != tabEdges)
            {

                if (PlaceObjectBtn.Checked || SelectObjectBtn.Checked)
                {
                    PlaceObjectBtn.Checked = !PlaceObjectBtn.Checked;
                    SelectObjectBtn.Checked = !PlaceObjectBtn.Checked;
                }
                else
                {
                    placeWPBtn.Checked = !placeWPBtn.Checked;
                    selectWPBtn.Checked = !placeWPBtn.Checked;
                }
            }
        }
        public void SetRadioDraw()
        {
            if (!EditorSettings.Default.Draw_Extents_3D)
                radioExtentsHide.Checked = true;
            else if (EditorSettings.Default.Draw_AllExtents)
                radioExtentsShowAll.Checked = true;
            else
                radioExtentShowColl.Checked = true;
        }
        private RadioButton GetSelectedMode(Array group)
        {
            foreach (RadioButton rb in group)
            {
                if (rb.Checked)
                    return rb;
            }
            return null;
        }
        private static int Distance(Point a, Point b)
        {
            return (int)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
        public void CenterAtPoint(Point centerAt)
        {
            if (mapPanel.ClientRectangle.Contains(centerAt))
            {
                int Y = centerAt.Y - scrollPanel.Height / 2;
                int X = centerAt.X - scrollPanel.Width / 2;
                if (Y < 0)
                    Y = 0;
                if (X < 0)
                    X = 0;
                scrollPanel.VerticalScroll.Value = Y;
                scrollPanel.HorizontalScroll.Value = X;
                winX = centerAt.X - WidthMod;
                winY = centerAt.Y;
                scrollPanel.PerformLayout();
                MapRenderer.UpdateCanvas(true, true, false);
                mapPanel.Invalidate();
            }
        }
        public bool FindObjectInList(string data, bool sec = false)
        {
            for (int i = 0; i < cboObjCreate.Items.Count; i++)
            {
                if (RemoveSpace(cboObjCreate.Items[i].ToString()) == data)
                {
                    cboObjCreate.SelectedIndex = i;
                    return true;
                }
            }
            if (!sec)
            {
                objectCategoriesBox.SelectedIndex = 0;
                if (FindObjectInList(data, true)) return true;
            }
            return false;
        }
        public void ShowObjectProperties(Map.Object obj)
        {
            if (obj == null) return;

            // working on the object clone (we should be able to rollback changes)
            int ndx = Map.Objects.IndexOf(obj);
            //MessageBox.Show(ndx.ToString());
            var propDlg = new ObjectPropertiesDialog();
            propDlg.Object = (Map.Object)Map.Objects[ndx];

            if (propDlg.ShowDialog() == DialogResult.OK)
            {
                // update object reference to updated version
                Map.Objects[ndx] = propDlg.Object;
                if (SelectedObjects.Items.Count > 0) SelectedObjects.Items[0] = propDlg.Object;

                // hint renderer
                MapRenderer.UpdateCanvas(true, false);
                mapPanel.Invalidate();
            }
        }
        public void DeletefromSelected(Map.Object item)
        {
            int indx = Array.IndexOf(SelectedObjects.Items.ToArray(), item);
            if (indx >= 0)
                SelectedObjects.Items.RemoveAt(indx);

        }
        public void DeleteSelectedObjects()
        {
            // Óäàëÿåò ïðåäìåòû, ñîäåðæàùèå? ?SelectedObjects, ?êàðò?
            if (MapInterface.CurrentMode == Mode.POLYGON_RESHAPE && !MapInterface.SelectedPolyPoint.IsEmpty)
            {
                ApplyStore();
                Map.Polygon poly = PolygonEditDlg.SelectedPolygon;
                poly.Points.RemoveAt(arrowPoly);
                MapInterface.SelectedPolyPoint = new PointF();
                Store(MapInterface.CurrentMode, TimeEvent.POST);
                mapPanel.Invalidate();
            }

            if (!SelectedObjects.IsEmpty && MapInterface.CurrentMode == Mode.OBJECT_SELECT)
            {
                ApplyStore();
                foreach (Map.Object o in SelectedObjects)
                {
                    // Ensure Xfer data is saved by replacing stored object with deleted object
                    foreach (var oContent in TimeManager)
                    {
                        if (oContent.Mode != Mode.OBJECT_PLACE)
                            continue;
                        foreach (var timeObj in oContent.StoredObjects)
                        {
                            if (o.ToString() == timeObj.Object.ToString())
                            {
                                timeObj.Object = o;
                                break;
                            }
                        }
                    }
                    MapInterface.ObjectRemove(o);
                }
                SelectedObjects.Items.Clear();
                Store(MapInterface.CurrentMode, TimeEvent.POST);
            }
            if (MapInterface.CurrentMode == Mode.WAYPOINT_SELECT && MapInterface.SelectedWaypoint != null)
            {
                ApplyStore();
                MapInterface.WaypointRemove(MapInterface.SelectedWaypoint);
                Store(MapInterface.CurrentMode, TimeEvent.POST);
            }

            MapRenderer.UpdateCanvas(true, false);
        }

        private void WaypointName_TextChanged(object sender, EventArgs e)
        {
            if (MapInterface.SelectedWaypoint != null)
            {
                MapInterface.SelectedWaypoint.Name = waypointName.Text;
            }
        }
        private void WaypointEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (MapInterface.SelectedWaypoint != null)
            {
                MapInterface.SelectedWaypoint.Flags = waypointEnabled.Checked ? 1 : 0;
            }
        }
        private void WaypointSelectAll_Click(object sender, EventArgs e)
        {
            selectWPBtn.PerformClick();
            MapInterface.WaypointSelectAll();
        }

        private void Picker_CheckedChanged(object sender, EventArgs e)
        {
            if (Picker.Checked)
            {
                TileMakeNewCtrl.Picker.Checked = true;
                WallMakeNewCtrl.Picker.Checked = true;
                EdgeMakeNewCtrl.Picker.Checked = true;
                picking = true;
                Cursor myCursor = Cursors.Cross;
                if (System.IO.File.Exists("picker.cur"))
                    myCursor = new Cursor("picker.cur");

                mapPanel.Cursor = myCursor;
            }
            else
            {
                picking = false;
                TileMakeNewCtrl.Picker.Checked = false;
                EdgeMakeNewCtrl.Picker.Checked = false;
                WallMakeNewCtrl.Picker.Checked = false;
                mapPanel.Cursor = Cursors.Default;
            }
        }
        private void select45Box_CheckedChanged(object sender, EventArgs e)
        {
            if (select45Box.Checked)
                select45Box.BackgroundImage = Properties.Resources._45deg;
            else
                select45Box.BackgroundImage = Properties.Resources._0deg;
        }
        public int Get45RecSize()
        {
            int x1 = MapInterface.selected45Area[0].X;
            int x2 = MapInterface.selected45Area[2].X;
            int y1 = MapInterface.selected45Area[0].Y;
            int y2 = MapInterface.selected45Area[2].Y;
            return ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        public void Switch45Area()
        {
            MapInterface.RecSelected.Clear();
            MapInterface.selected45Area = new Point[4];
            select45Box.Checked = !select45Box.Checked;
            MapInterface.ObjectSelect45Rectangle(mouseLocation);

            mapPanel.Invalidate();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            MapRenderer.FakeWalls.Clear();
            MapRenderer.UpdateCanvas(false, true);
        }

        #region Wall/Tile Position Functions
        public Map.Tile GetCurrentTileVar(Point tilePt)
        {
            return TileMakeNewCtrl.GetTile(tilePt);
        }
        public Map.Object GetObjectUnderCursor()
        {
            if (MapInterface.CurrentMode != Mode.OBJECT_SELECT && MapInterface.CurrentMode != Mode.OBJECT_PLACE) return null;
            return MapInterface.ObjectSelect(mouseLocation);
        }
        public Map.Waypoint GetWPUnderCursor()
        {
            if (MapInterface.CurrentMode != Mode.WAYPOINT_PLACE && MapInterface.CurrentMode != Mode.WAYPOINT_PLACE) return null;
            return MapInterface.WaypointSelect(mouseLocation);
        }
        public Map.Wall GetWallUnderCursor(Point proxyPt = new Point())
        {
            Point pt;
            if (proxyPt.IsEmpty)
                pt = mouseLocation;
            else
                pt = proxyPt;

            if (picking && MapInterface.CurrentMode == Mode.WALL_BRUSH) goto nocheck;

            if (MapInterface.CurrentMode != Mode.WALL_PLACE && MapInterface.CurrentMode != Mode.WALL_CHANGE && MapInterface.CurrentMode != Mode.WALL_BRUSH) return null;
        nocheck:
            Point wallPt = GetNearestWallPoint(pt);
            if (!Map.Walls.ContainsKey(wallPt)) return null;
            return Map.Walls[wallPt];
        }
        public bool GetTileUnderCursor()
        {
            if (MapInterface.CurrentMode != Mode.FLOOR_BRUSH && MapInterface.CurrentMode != Mode.FLOOR_PLACE && MapInterface.CurrentMode != Mode.EDGE_PLACE) return false;
            Point tilePt = GetNearestTilePoint(mouseLocation);
            if (!Map.Tiles.ContainsKey(tilePt)) return false;
            return true;
        }
        public bool GetEdgeUnderCursor()
        {
            if (MapInterface.CurrentMode != Mode.EDGE_PLACE) return false;
            Point tilePt = GetNearestTilePoint(mouseLocation);
            if (!Map.Tiles.ContainsKey(tilePt)) return false;
            if (Map.Tiles[tilePt].EdgeTiles.Count <= 0) return false;

            return true;
        }
        public Point GetPosTileUnderCursor2()
        {
            Point tilePt = GetNearestTilePoint(mouseLocation);
            if (!Map.Tiles.ContainsKey(tilePt)) return Map.Tiles[tilePt].Location;
            return new Point();
        }
        public Point GetNearestTile(Point pt)
        {
            pt.Offset(0, -squareSize);
            return GetNearestWallPoint(pt);
        }

        public static Point GetNearestTilePoint(Point pt)
        {
            pt.Offset(0, -squareSize);
            return GetNearestWallPoint(pt);
        }
        public static Point GetCenterPoint(Point pt, bool wallPt = false)
        {
            Point pti = GetNearestTilePoint(pt);
            int x = (pti.X * squareSize);
            int y = (pti.Y * squareSize) + squareSize / 2;

            if (!wallPt)
                return new Point(x + squareSize / 2, y + (3 / 2) * squareSize);
            else
                return new Point((x + squareSize / 2) / squareSize, (y + (3 / 2) * squareSize) / squareSize);
        }
        public static Point GetNearestWallPoint(Point pt, bool cart = false)
        {
            int sqSize = squareSize;
            if (cart) sqSize = 1;

            Point tl = new Point((pt.X / squareSize) * squareSize, (pt.Y / squareSize) * squareSize);
            if (tl.X / squareSize % 2 == tl.Y / squareSize % 2)
                return new Point(tl.X / sqSize, tl.Y / sqSize);
            else
            {
                Point left = new Point(tl.X, tl.Y + squareSize / 2);
                Point right = new Point(tl.X + squareSize, tl.Y + squareSize / 2);
                Point top = new Point(tl.X + squareSize / 2, tl.Y);
                Point bottom = new Point(tl.X + squareSize / 2, tl.Y + squareSize);
                Point closest = left;
                foreach (Point point in new Point[] { left, right, top, bottom })
                    if (Distance(point, pt) < Distance(closest, pt))
                        closest = point;

                if (closest == left)
                    return new Point(tl.X / sqSize - 1, tl.Y / sqSize);
                else if (closest == right)
                    return new Point(tl.X / sqSize + 1, tl.Y / sqSize);
                else if (closest == top)
                    return new Point(tl.X / sqSize, tl.Y / sqSize - 1);
                else
                    return new Point(tl.X / sqSize, tl.Y / sqSize + 1);
            }
        }
        public static Point GetWallMapCoords(Point wallCoords)
        {
            // Works with both walls and tiles; i.e. 10, 15 => 230, 345
            var x = wallCoords.X * squareSize;
            var y = wallCoords.Y * squareSize;
            return new Point(x, y);
        }
        #endregion

        #region Paint Bucket
        public bool tileBucket = false;
        public bool wallBucket = false;
        private bool[,] baseFloorMap; 
        private bool tileOverflow;
        private int nTiles;
        private int dir;

        // Tile Bucket
        private void TileBucketPaint(Point origin)
        {
            var selTile = GetNearestTilePoint(origin);
            if (Map.Tiles.ContainsKey(selTile))
                return;

            baseFloorMap = new bool[Generator.BOUNDARY, Generator.BOUNDARY];
            var hmap = new MapHelper(Map);
            hmap.SetTileMaterial(TileMakeNewCtrl.comboTileType.SelectedItem.ToString());
            // Populate baseFloorMap with points
            dir = 0;
            do
            {
                var skip = tileOverflow;
                nTiles = 0;
                tileOverflow = false;
                PaintTilesFromOrigin(selTile, skip);
                dir++;
                if (dir == 4)
                    break;
            }
            while (tileOverflow);

            ApplyStore();
            // Copy floor onto map
            for (int x = 0; x < Generator.BOUNDARY; x++)
                for (int y = 0; y < Generator.BOUNDARY; y++)
                    if (baseFloorMap[x, y])
                        hmap.PlaceTileSnap(x, y);

            MapRenderer.UpdateCanvas(MapInterface.OpUpdatedObjects, MapInterface.OpUpdatedTiles);
        }
        private void TileBucketDelete(Point origin)
        {
            var selTile = GetNearestTilePoint(origin);
            if (!Map.Tiles.ContainsKey(selTile))
                return;

            baseFloorMap = new bool[Generator.BOUNDARY, Generator.BOUNDARY];
            var hmap = new MapHelper(Map);

            // Populate baseFloorMap with points
            dir = 0;
            do
            {
                var skip = tileOverflow;
                nTiles = 0;
                tileOverflow = false;
                DeleteTilesFromOrigin(selTile, skip);
                dir++;
                if (dir == 4)
                    break;
            }
            while (tileOverflow);

            ApplyStore();
            // Copy floor onto map
            for (int x = 0; x < Generator.BOUNDARY; x++)
                for (int y = 0; y < Generator.BOUNDARY; y++)
                    if (baseFloorMap[x, y])
                        hmap.RemoveTile(x, y);

            MapRenderer.UpdateCanvas(MapInterface.OpUpdatedObjects, MapInterface.OpUpdatedTiles);
        }
        private void PaintTilesFromOrigin(Point tile, bool skipCheck = false)
        {
            if (nTiles > 3500)
            {
                tileOverflow = true;
                return;
            }
            if (!skipCheck)
            {
                if ((tile.X < 0) || (tile.Y < 0)
                    || (tile.X >= Generator.BOUNDARY) || (tile.Y >= Generator.BOUNDARY)
                    || (baseFloorMap[tile.X, tile.Y]) || Map.Tiles.ContainsKey(tile))
                    return;
            }

            var numWalls = GetNumAdjacentWalls(tile);
            var branchOut = GetAdjacentTiles(tile, dir);
            // No walls nearby, paint tile unless already one there, then branch out in S-W-N-E fashion
            if (numWalls < 2)
            {
                baseFloorMap[tile.X, tile.Y] = true;
                nTiles++;

                foreach (var t in branchOut)
                    PaintTilesFromOrigin(t);
            }
            // Too many walls, don't branch out
            else
            {
                baseFloorMap[tile.X, tile.Y] = true;
                nTiles++;

                foreach (var t in branchOut)
                {
                    var numWalls2 = GetNumAdjacentWalls(t);
                    var numCorners = GetNumCorners(t);
                    if ((numWalls2 == 3) && (numCorners == 1))
                        baseFloorMap[t.X, t.Y] = true;
                }
            }
        }
        private void DeleteTilesFromOrigin(Point tile, bool skipCheck = false)
        {
            if (nTiles > 3500)
            {
                tileOverflow = true;
                return;
            }
            if (!skipCheck)
            {
                if ((tile.X < 0) || (tile.Y < 0)
                    || (tile.X >= Generator.BOUNDARY) || (tile.Y >= Generator.BOUNDARY)
                    || (baseFloorMap[tile.X, tile.Y]) || !Map.Tiles.ContainsKey(tile))
                    return;
            }

            var numWalls = GetNumAdjacentWalls(tile);
            var branchOut = GetAdjacentTiles(tile, dir);
            if (numWalls < 2)
            {
                baseFloorMap[tile.X, tile.Y] = true;
                nTiles++;

                foreach (var t in branchOut)
                    DeleteTilesFromOrigin(t);
            }
            else
            {
                baseFloorMap[tile.X, tile.Y] = true;
                nTiles++;

                foreach (var t in branchOut)
                {
                    var numWalls2 = GetNumAdjacentWalls(t);
                    var numCorners = GetNumCorners(t);
                    if ((numWalls2 == 3) && (numCorners == 1))
                        baseFloorMap[t.X, t.Y] = true;

                }
            }
        }
        private bool IsCornerWall(Point wallPt)
        {
            if (!Map.Walls.ContainsKey(wallPt))
                return false;

            switch (Map.Walls[wallPt].Facing)
            {
                case Map.Wall.WallFacing.NE_CORNER:
                case Map.Wall.WallFacing.NW_CORNER:
                case Map.Wall.WallFacing.SE_CORNER:
                case Map.Wall.WallFacing.SW_CORNER:
                case Map.Wall.WallFacing.CROSS:
                    return true;
            }
            return false;
        }
        private int GetNumCorners(Point tilePt)
        {
            var walls = GetAdjacentWalls(tilePt);
            int count = 0;

            if (IsCornerWall(walls[0]))
                count++;
            if (IsCornerWall(walls[1]))
                count++;
            if (IsCornerWall(walls[2]))
                count++;
            if (IsCornerWall(walls[3]))
                count++;

            return count;
        }    
        private int GetNumAdjacentWalls(Point tilePt)
        {
            int count = 0;
            if (Map.Walls.ContainsKey(tilePt))                                      // Top Wall
                count++;
            if (Map.Walls.ContainsKey(new Point(tilePt.X - 1, tilePt.Y + 1)))       // Left
                count++;
            if (Map.Walls.ContainsKey(new Point(tilePt.X + 1, tilePt.Y + 1)))       // Right
                count++;
            if (Map.Walls.ContainsKey(new Point(tilePt.X, tilePt.Y + 2)))           // Bottom
                count++;

            return count;
        }
        private Point[] GetAdjacentWalls(Point tile)
        {
            return new Point[]
            {
                tile,                                    // Top Wall
                new Point(tile.X, tile.Y + 2),           // Bottom
                new Point(tile.X - 1, tile.Y + 1),       // Left
                new Point(tile.X + 1, tile.Y + 1)        // Right
            };
        }
        private Point[] GetAdjacentTiles(Point tile, int firstDirection = 0)
        {
            var result = new List<Point>();
            var tileSE = new Point(tile.X + 1, tile.Y + 1);
            var tileSW = new Point(tile.X - 1, tile.Y + 1);
            var tileNW = new Point(tile.X - 1, tile.Y - 1);
            var tileNE = new Point(tile.X + 1, tile.Y - 1);

            if (firstDirection == 0)
                result.AddRange(new Point[] { tileSE, tileSW, tileNW, tileNE });
            if (firstDirection == 1)
                result.AddRange(new Point[] { tileSW, tileNW, tileNE, tileSE });
            else if (firstDirection == 2)
                result.AddRange(new Point[] { tileNW, tileNE, tileSE, tileSW });
            else if (firstDirection == 3)
                result.AddRange(new Point[] { tileNE, tileSE, tileSW, tileNW });

            // Cardinal
            //return new Point[] {
            //    new Point(tile.X, tile.Y + 2),   // S
            //    new Point(tile.X - 2, tile.Y),   // W
            //    new Point(tile.X, tile.Y - 2),   // N
            //    new Point(tile.X + 2, tile.Y),   // E
            //};

            return result.ToArray();
        }

        // Wall Bucket
        private void WallBucketPaint(Point coords)
        {
            var selTile = GetNearestTilePoint(coords);
            if (!Map.Tiles.ContainsKey(selTile))
                return;

            ApplyStore();
            Map saveMap = MapInterface.TheMap;

            var newMap = new MapHelper(saveMap);
            var tileGroup = newMap.DetermineTileCluster2(selTile.X, selTile.Y);
            if (tileGroup.Count == 0)
            {
                MessageBox.Show("Selection too large. Try a smaller tile cluster.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var outerEdge = GetOuterEdgeTiles(newMap, tileGroup);
            outerEdge.AddRange(tileGroup);
            outerEdge = outerEdge.Distinct().ToList();
            newMap.SetWallMaterial(WallMakeNewCtrl.comboWallSet.SelectedItem.ToString());
            MakeWallsAroundTiles(newMap, tileGroup);
            ReorientWalls(newMap, outerEdge);
            MapInterface.TheMap = saveMap;
            Store(MapInterface.CurrentMode, TimeEvent.POST);
            MapRenderer.UpdateCanvas(MapInterface.OpUpdatedObjects, MapInterface.OpUpdatedTiles);
        }
        private void MakeWallsAroundTiles(MapHelper hmap, List<Point> tiles)
        {
            Map.Tile tile;
            foreach (var t in tiles)
            {
                var x = t.X; var y = t.Y;
                tile = hmap.GetTile(x, y);
                if (tile != null)
                {
                    // Simple logic: if there is no tile from specified direction, place wall
                    bool tileLeft = (hmap.GetTile(x - 2, y) == null);
                    bool tileRight = (hmap.GetTile(x + 2, y) == null);
                    bool tileUp = (hmap.GetTile(x, y - 2) == null);
                    bool tileDown = (hmap.GetTile(x, y + 2) == null);

                    if (tileLeft) hmap.PlaceWall(x - 1, y + 1);
                    if (tileRight) hmap.PlaceWall(x + 1, y + 1);
                    if (tileUp) hmap.PlaceWall(x, y);
                    if (tileDown) hmap.PlaceWall(x, y + 2);

                    // There are some few special cases however that need attention
                    if (!tileLeft && hmap.GetTile(x - 1, y + 1) == null && hmap.GetTile(x - 1, y - 1) == null)
                        hmap.PlaceWall(x - 1, y + 1);
                    if (!tileDown && hmap.GetTile(x - 1, y + 1) == null && hmap.GetTile(x + 1, y + 1) == null)
                        hmap.PlaceWall(x, y + 2);
                }
            }
        }
        private void ReorientWalls(MapHelper hmap, List<Point> tiles, bool allowTri = false)
        {
            Map.Wall wall;
            foreach (var t in tiles)
            {
                var x = t.X; var y = t.Y;
                wall = hmap.GetWall(x, y);

                if (wall != null)
                {
                    bool tileLeft = (hmap.GetTile(x - 1, y - 1) != null);
                    bool tileRight = (hmap.GetTile(x + 1, y - 1) != null);
                    bool tileUp = (hmap.GetTile(x, y - 2) != null);
                    bool tileDown = (hmap.GetTile(x, y) != null);
                    int facing = -1;

                    if (!tileLeft)
                    {
                        if (tileUp && tileRight && tileDown) facing = 10;
                        else if (tileDown && tileRight)
                        {
                            if (hmap.GetWall(x - 1, y - 1) != null && allowTri) facing = 3;
                            else facing = 0;
                        }
                        else if (tileUp && tileRight)
                        {
                            if (hmap.GetWall(x - 1, y + 1) != null && allowTri) facing = 6;
                            else facing = 1;
                        }
                        else if (!tileUp && !tileDown) facing = 8;
                    }
                    if (!tileUp && facing < 0)
                    {
                        if (tileLeft && tileRight && tileDown) facing = 7;
                        else if (tileDown && tileLeft)
                        {
                            if (hmap.GetWall(x + 1, y - 1) != null && allowTri) facing = 4;
                            else facing = 1;
                        }
                        else if (tileDown && tileRight)
                        {
                            if (hmap.GetWall(x + 1, y + 1) != null && allowTri) facing = 5;
                            else facing = 0;
                        }
                        else if (!tileLeft && !tileRight) facing = 9;
                    }
                    if (!tileDown && facing < 0)
                    {
                        if (tileLeft && tileRight && tileUp) facing = 9;
                        else if (tileUp && tileLeft)
                        {
                            if (hmap.GetWall(x + 1, y + 1) != null && allowTri) facing = 5;
                            else facing = 0;
                        }
                        else if (tileUp && tileRight)
                        {
                            if (hmap.GetWall(x - 1, y + 1) != null && allowTri) facing = 6;
                            else facing = 1;
                        }
                        else if (!tileLeft && !tileRight) facing = 7;
                    }
                    if (!tileRight && facing < 0)
                    {
                        if (tileUp && tileLeft && tileDown) facing = 8;
                        else if (tileDown && tileLeft)
                        {
                            if (hmap.GetWall(x + 1, y - 1) != null && allowTri) facing = 4;
                            else facing = 1;
                        }
                        else if (tileUp && tileLeft)
                        {
                            if (hmap.GetWall(x + 1, y + 1) != null && allowTri) facing = 5;
                            else facing = 0;
                        }
                        else if (!tileUp && !tileDown) facing = 10;
                    }

                    if (facing < 0) facing = 2;
                    wall.Facing = (Map.Wall.WallFacing)facing;
                }
            }
        }
        private Point[] GetAllAdjacentTiles(Point tile)
        {
            var x = tile.X; var y = tile.Y;
            return new Point[]
            {
                new Point(x + 2, y),
                new Point(x + 1, y - 1),
                new Point(x, y - 2),
                new Point(x - 1, y - 1),
                new Point(x - 2, y),
                new Point(x - 1, y + 1),
                new Point(x, y + 2),
                new Point(x + 1, y + 1)
            };
        }
        private List<Point> GetOuterEdgeTiles(MapHelper hmap, List<Point> tileGroup)
        {
            var result = new List<Point>();
            foreach (var tile in tileGroup)
            {
                var borderTiles = GetAllAdjacentTiles(tile);
                foreach (var b in borderTiles)
                {
                    if ((hmap.GetTile(b) == null) && (!result.Contains(b)))
                        result.Add(b);
                }
            }
            return result;
        }
        #endregion

        #region Undo/Redo
        public int currentStep = 0;
        public bool StopUndo;
        public bool StopRedo;
        public Point highlightUndoRedo;
        private List<TimeTile> TimeTiles = new List<TimeTile>();
        private List<TimeObject> TimeObjects = new List<TimeObject>();
        private List<TimeWaypoint> TimeWaypoints = new List<TimeWaypoint>();
        private List<TimeWall> TimeWalls = new List<TimeWall>();
        private List<TimePolygon> TimePolygons = new List<TimePolygon>();
        public List<TimeContent> TimeManager = new List<TimeContent>();

        private string GetBaseMode(Mode mode)
        {
            string modeString = mode.ToString();
            modeString = modeString.Substring(0, modeString.IndexOf("_")).Trim();

            return modeString;
        }
        public bool ApplyStore()
        {
            int steps = (TimeManager.Count - 1) - currentStep;
            if (TimeManager.Count == 0 || ((TimeManager.Count - 1) - currentStep) < 1 && currentStep > 0)
            {
                Store(MapInterface.CurrentMode, TimeEvent.PRE);
                return true;

            }
            else if (TimeManager.Count > 1)
            {
                if (GetBaseMode(TimeManager[steps - 1].Mode) != GetBaseMode(MapInterface.CurrentMode))
                {
                    Store(MapInterface.CurrentMode, TimeEvent.PRE);
                    return true;
                }
            }
            return false;
        }
        public void Store(Mode mode, TimeEvent Event, bool forceMode = false)
        {
            DebugTime();
            int steps = TimeManager.Count - 1;

            if (TimeManager.Count > 1 && currentStep > 0)
            {
                TimeManager.RemoveRange((steps - currentStep) + 1, TimeManager.Count - ((steps - currentStep) + 1));
                currentStep = 0;
                cmdRedo.Enabled = false;
                StopRedo = true;
                MainWindow.Instance.miniRedo.Enabled = false;
            }

            if (TimeManager.Count > 50)
                TimeManager.RemoveAt(0);

            if (steps - currentStep >= 0)
            {
                StopUndo = false;
                cmdUndo.Enabled = true;
                MainWindow.Instance.miniUndo.Enabled = true;
            }

            if (steps < TimeManager.Count - 1)
            {
                StopRedo = false;
                cmdRedo.Enabled = true;
                MainWindow.Instance.miniRedo.Enabled = true;

            }
            /////////////polygons///////////////
            if (mode == Mode.POLYGON_RESHAPE)
            {
                TimePolygons.Clear();
                TimePolygons = new List<TimePolygon>();

                TimeContent content = new TimeContent();
                content.Mode = forceMode? mode : MapInterface.CurrentMode;
                content.Event = Event;


                foreach (Map.Polygon polygon in Map.Polygons)
                {
                    TimePolygon subject = new TimePolygon();
                    subject.Points = new List<PointF>();

                    foreach (PointF point in polygon.Points)
                    {
                        subject.Points.Add(point);
                    }
                    subject.Polygon = polygon;

                    TimePolygons.Add(subject);

                    content.StoredPolygons.Add(subject);
                }
                int page = MainWindow.Instance.panelTabs.SelectedIndex;
                if (page == 1)
                {
                    Point MouseLocInMini = MainWindow.Instance.mouseLocation;
                    content.Location = new Point(MouseLocInMini.X / MainWindow.Instance.mapZoom * squareSize, MouseLocInMini.Y / MainWindow.Instance.mapZoom * squareSize);

                }
                else
                    content.Location = new Point(mouseLocation.X, mouseLocation.Y);
                TimeManager.Add(content);

                return;
            }
            ///////////Waypoints/////////////
            if (mode == Mode.WAYPOINT_CONNECT || mode == Mode.WAYPOINT_SELECT || mode == Mode.WAYPOINT_PLACE)
            {
                TimeWaypoints.Clear();
                TimeWaypoints = new List<TimeWaypoint>();

                TimeContent content = new TimeContent();
                content.Mode = forceMode ? mode : MapInterface.CurrentMode;
                content.Event = Event;
                foreach (Map.Waypoint wp in Map.Waypoints)
                {
                    TimeWaypoint subject = new TimeWaypoint();
                    subject.Connections = new ArrayList();

                    foreach (Map.Waypoint.WaypointConnection wpc in wp.connections)
                    {
                        subject.Connections.Add(wpc);
                    }
                    subject.Name = wp.Name;
                    subject.Waypoint = wp;
                    subject.Location = wp.Point;

                    // TimeWPs.Add(subject);

                    content.StoredWPs.Add(subject);

                }
                content.Location = new Point(mouseLocation.X, mouseLocation.Y);
                TimeManager.Add(content);

                return;
            }

            //////////Objects/////////////
            if (mode == Mode.OBJECT_SELECT || mode == Mode.OBJECT_PLACE)
            {
                TimeContent content = new TimeContent();
                content.Mode = forceMode ? mode : MapInterface.CurrentMode;
                content.Event = Event;
                TimeObjects.Clear();
                TimeObjects = new List<TimeObject>();
                foreach (Map.Object item in Map.Objects)
                {
                    TimeObject subject = new TimeObject();
                    subject.Location = item.Location;
                    subject.Object = item;
                    TimeObjects.Add(subject);
                    content.StoredObjects.Add(subject);
                }
                content.Location = new Point(mouseLocation.X, mouseLocation.Y);
                TimeManager.Add(content);

                return;
            }
            ////////////Tiles//////////////
            if (mode == Mode.FLOOR_BRUSH || mode == Mode.EDGE_PLACE || mode == Mode.FLOOR_PLACE)
            {
                TimeContent content = new TimeContent();
                content.Mode = forceMode ? mode : MapInterface.CurrentMode;
                content.Event = Event;
                foreach (Map.Tile tila in Map.Tiles.Values)
                {

                    TimeTile subject = new TimeTile();
                    subject.EdgeTiles = new ArrayList();

                    foreach (Map.Tile.EdgeTile edga in tila.EdgeTiles)
                    {
                        subject.EdgeTiles.Add(edga);
                    }

                    subject.Tile = tila;

                    content.StoredTiles.Add(subject);


                }
                int page = MainWindow.Instance.panelTabs.SelectedIndex;
                if (page == 1)
                {
                    Point MouseLocInMini = MainWindow.Instance.mouseLocation;
                    content.Location = new Point(MouseLocInMini.X / MainWindow.Instance.mapZoom * squareSize, MouseLocInMini.Y / MainWindow.Instance.mapZoom * squareSize);

                }
                else
                    content.Location = new Point(mouseLocation.X, mouseLocation.Y);
                TimeManager.Add(content);

                return;
            }

            ///////////Walls/////////////
            if (mode == Mode.WALL_PLACE || mode == Mode.WALL_CHANGE || mode == Mode.WALL_BRUSH)
            {
                TimeContent content = new TimeContent();
                content.Mode = forceMode ? mode : MapInterface.CurrentMode;
                content.Event = Event;
                TimeWalls.Clear();
                TimeWalls = new List<TimeWall>();
                foreach (Map.Wall wall in Map.Walls.Values)
                {
                    TimeWall subject = new TimeWall();

                    subject.Wall = wall;
                    subject.Facing = wall.Facing;
                    subject.SFlags = (Map.Wall.SecretScanFlags)wall.Secret_ScanFlags;
                    subject.Minimap = wall.Minimap;
                    subject.Destructable = wall.Destructable;
                    subject.Window = wall.Window;
                    subject.Secret_WallState = wall.Secret_WallState;
                    subject.Secret_OpenWaitSeconds = wall.Secret_OpenWaitSeconds;
                    TimeWalls.Add(subject);
                    content.StoredWalls.Add(subject);
                }

                int page = MainWindow.Instance.panelTabs.SelectedIndex;
                if (page == 1)
                {
                    Point MouseLocInMini = MainWindow.Instance.mouseLocation;
                    content.Location = new Point(MouseLocInMini.X / MainWindow.Instance.mapZoom * squareSize, MouseLocInMini.Y / MainWindow.Instance.mapZoom * squareSize);

                }
                else
                    content.Location = new Point(mouseLocation.X, mouseLocation.Y);
                TimeManager.Add(content);

                return;
            }
        }
        private void Release(int step)
        {
            Mode mode = TimeManager[step].Mode;

            if (mode == Mode.POLYGON_RESHAPE)
            {
                foreach (Map.Polygon polygon in Map.Polygons)
                {
                    polygon.Points.Clear();
                }
                Map.Polygons.Clear();
                Map.Polygons = new Map.PolygonList();
                foreach (TimePolygon polygon in TimeManager[step].StoredPolygons)
                {
                    Map.Polygons.Add(polygon.Polygon);
                    Map.Polygon itema = (Map.Polygon)Map.Polygons[Map.Polygons.Count - 1];

                    foreach (PointF polypoint in polygon.Points)
                        itema.Points.Add(polypoint);
                }
            }

            if (mode == Mode.WAYPOINT_CONNECT || mode == Mode.WAYPOINT_SELECT || mode == Mode.WAYPOINT_PLACE)
            {
                foreach (Map.Waypoint wp in Map.Waypoints)
                    wp.connections.Clear();

                Map.Waypoints.num_wp.Clear();
                Map.Waypoints.Clear();

                foreach (TimeWaypoint wps in TimeManager[step].StoredWPs)
                {
                    Map.Waypoints.Add(wps.Waypoint);
                    Map.Waypoint releasedWP = (Map.Waypoint)Map.Waypoints[Map.Waypoints.Count - 1];
                    releasedWP.Name = wps.Name;
                    releasedWP.Point = wps.Location;
                    Map.Waypoints.num_wp.Add(wps.Waypoint.Number, wps.Waypoint);

                    foreach (Map.Waypoint.WaypointConnection wpcon in wps.Connections)
                        releasedWP.connections.Add(wpcon);
                }
            }
            if (mode == Mode.OBJECT_SELECT || mode == Mode.OBJECT_PLACE)
            {
                Map.Objects.Clear();
                Map.Objects = new Map.ObjectTable();

                foreach (TimeObject item in TimeManager[step].StoredObjects)
                {
                    Map.Objects.Add(item.Object);
                    Map.Object releasedObj = (Map.Object)Map.Objects[Map.Objects.Count - 1];
                    releasedObj.Location = item.Location; // Location could have changed
                }

                MapRenderer.UpdateCanvas(true, false);
                mapPanel.Invalidate();
            }

            if (mode == Mode.WALL_PLACE || mode == Mode.WALL_CHANGE || mode == Mode.WALL_BRUSH)
            {
                Map.Walls.Clear();
                foreach (TimeWall wall in TimeManager[step].StoredWalls)
                {
                    Map.Walls.Add(wall.Wall.Location, wall.Wall);
                    Map.Walls[wall.Wall.Location].Facing = wall.Facing;
                    Map.Walls[wall.Wall.Location].Secret_ScanFlags = (byte)wall.SFlags;
                    Map.Walls[wall.Wall.Location].Destructable = wall.Destructable;
                    Map.Walls[wall.Wall.Location].Window = wall.Window;
                    Map.Walls[wall.Wall.Location].Minimap = wall.Minimap;
                    Map.Walls[wall.Wall.Location].Secret_WallState = wall.Secret_WallState;
                    Map.Walls[wall.Wall.Location].Secret_OpenWaitSeconds = wall.Secret_OpenWaitSeconds;
                }
            }
            if (mode == Mode.FLOOR_BRUSH || mode == Mode.EDGE_PLACE || mode == Mode.FLOOR_PLACE)
            {
                foreach (Map.Tile tila in Map.Tiles.Values)
                {
                    tila.EdgeTiles.Clear();
                }

                Map.Tiles.Clear();
                Map.Tiles = new Map.FloorMap();

                foreach (TimeTile item in TimeManager[step].StoredTiles)
                {
                    Map.Tiles.Add(item.Tile.Location, item.Tile);

                    foreach (Map.Tile.EdgeTile edga in item.EdgeTiles)
                        Map.Tiles[item.Tile.Location].EdgeTiles.Add(edga);
                }
            }
            if (MainWindow.Instance == null || !MainWindow.Instance.minimap.Visible)
                return;
            MainWindow.Instance.minimap.Reload();
        }

        public void Undo(bool timed = true)
        {
            if (timed && !UndoTimer.Enabled)
            {
                int stepsT = TimeManager.Count - 1 - (currentStep + 1);
                Rectangle visibleArea = new Rectangle(-mapPanel.Location.X, -mapPanel.Location.Y + 5, scrollPanel.Width - 5, scrollPanel.Height - 5);
                int x = (int)TimeManager[stepsT + 1].Location.X;
                int y = (int)TimeManager[stepsT + 1].Location.Y;

                if (stepsT < TimeManager.Count - 1)
                {
                    StopRedo = false;
                    cmdRedo.Enabled = true;
                    MainWindow.Instance.miniRedo.Enabled = true;
                }
                if (stepsT < 1)
                {
                    StopUndo = true;
                    cmdUndo.Enabled = false;
                    MainWindow.Instance.miniUndo.Enabled = false;
                }
                if (!visibleArea.Contains(x, y))
                {
                    highlightUndoRedo = new Point(x, y);
                    CenterAtPoint(new Point(x, y));
                    UndoTimer.Enabled = true;
                    return;
                }
            }
            highlightUndoRedo = new Point();
            UndoTimer.Enabled = false;
            int steps = TimeManager.Count - 1;
            currentStep++;

            steps -= currentStep;

            Release(steps);

            MapRenderer.UpdateCanvas(true, true);
            mapPanel.Invalidate();

            DebugTime();

            if (steps < TimeManager.Count - 1)
            {
                StopRedo = false;
                cmdRedo.Enabled = true;
                MainWindow.Instance.miniRedo.Enabled = true;
            }

            if (steps < 1)
            {
                StopUndo = true;
                cmdUndo.Enabled = false;
                MainWindow.Instance.miniUndo.Enabled = false;
                //TimeManager.Clear();
                //currentStep = 0;
                return;
            }

            if (TimeManager[steps].Event == TimeEvent.PRE)
                Undo(false);
        }
        public void Redo(bool timed = true)
        {
            if (timed && !RedoTimer.Enabled)
            {
                int stepsT = TimeManager.Count - 1;
                int currentStepT = currentStep - 1;
                stepsT -= currentStepT;
                Rectangle visibleArea = new Rectangle(-mapPanel.Location.X, -mapPanel.Location.Y + 5, scrollPanel.Width - 5, scrollPanel.Height - 5);
                int x = (int)TimeManager[stepsT].Location.X;
                int y = (int)TimeManager[stepsT].Location.Y;

                if (currentStepT >= 0)
                {
                    StopUndo = false;
                    cmdUndo.Enabled = true;
                    MainWindow.Instance.miniUndo.Enabled = true;
                }
                if (stepsT >= TimeManager.Count - 1)
                {
                    StopRedo = true;
                    cmdRedo.Enabled = false;
                    MainWindow.Instance.miniRedo.Enabled = false;
                }

                if (!visibleArea.Contains(x, y))
                {
                    highlightUndoRedo = new Point(x, y);
                    CenterAtPoint(new Point(x, y));

                    RedoTimer.Enabled = true;
                    StopRedo = false;
                    return;
                }
            }

            RedoTimer.Enabled = false;
            highlightUndoRedo = new Point();
            int steps = TimeManager.Count - 1;
            currentStep--;

            steps -= currentStep;

            Release(steps);

            MapRenderer.UpdateCanvas(true, true);
            mapPanel.Invalidate();

            DebugTime();

            if (currentStep >= 0)
            {
                StopUndo = false;
                cmdUndo.Enabled = true;
                MainWindow.Instance.miniUndo.Enabled = true;
            }
            if (steps >= TimeManager.Count - 1)
            {
                StopRedo = true;
                cmdRedo.Enabled = false;
                MainWindow.Instance.miniRedo.Enabled = false;
                return;
            }

            if (TimeManager[steps].Event == TimeEvent.PRE)
                Redo(false);
        }
        private void UndoTimer_Tick(object sender, EventArgs e)
        {
            higlightRad -= 30;
            mapPanel.Invalidate();
            if (higlightRad > 40) return;

            Undo(false);
            UndoTimer.Enabled = false;
            highlightUndoRedo = new Point();
            higlightRad = 150;
        }
        private void RedoTimer_Tick(object sender, EventArgs e)
        {
            higlightRad -= 30;
            mapPanel.Invalidate();
            if (higlightRad > 40) return;

            Redo(false);
            RedoTimer.Enabled = false;
            highlightUndoRedo = new Point();
            higlightRad = 150;
        }
        private void DebugTime()
        {
            if (lstDebug.Visible != true)
                return;

            lstDebug.Items.Clear();
            int i = 0;
            foreach (TimeContent tm in TimeManager)
            {
                string info = tm.Mode.ToString() + " e:" + tm.Event;
                lstDebug.Items.Add(info);
                i++;
            }
            lstDebug.SelectedIndex = (TimeManager.Count - 1) - currentStep;
        }
        #endregion
        #region TimeManager (Undo/Redo)
        public enum TimeEvent
        {
            PRE, POST
        };
        [Serializable]
        public class TimeTile
        {
            //public Point Location;
            public Map.Tile.EdgeTile Edge;
            public ArrayList EdgeTiles;
            public Map.Tile Tile;
        }
        [Serializable]
        public class TimeObject
        {
            public PointF Location;
            public Map.Object Object;
            public Point NearestMapTile;
            public PointF CartesianMapTile
            {
                get { return new PointF(NearestMapTile.X * squareSize + (squareSize / 2f), NearestMapTile.Y * squareSize + ((3 / 2f) * squareSize)); }
            }
        }
        [Serializable]
        public class TimeWaypoint
        {
            public Map.Waypoint Waypoint;
            public ArrayList Connections;
            public string Name;
            public int Number;
            public PointF Location;
            public Point NearestMapTile;
            public PointF CartesianMapTile
            {
                get { return new PointF(NearestMapTile.X * squareSize + (squareSize / 2f), NearestMapTile.Y * squareSize + ((3 / 2f) * squareSize)); }
            }
        }
        [Serializable]
        public class TimeWall
        {
            public Map.Wall Wall;
            public Map.Wall.WallFacing Facing;
            public Map.Wall.SecretScanFlags SFlags;
            public bool Destructable;
            public bool Window;
            public int Secret_OpenWaitSeconds;
            public byte Minimap;
            public byte Secret_WallState;
        }
        [Serializable]
        public class TimePolygon
        {
            public Map.Polygon Polygon;
            public List<PointF> Points;
        }
        [Serializable]
        public class TimeContent
        {
            public Mode Mode;
            public TimeEvent Event;
            public PointF Location;
            public Point StoreTile = Point.Empty;
            public List<TimeWall> StoredWalls = new List<TimeWall>();
            public List<TimeTile> StoredTiles = new List<TimeTile>();
            public List<TimeWaypoint> StoredWPs = new List<TimeWaypoint>();
            public List<TimeObject> StoredObjects = new List<TimeObject>();
            public List<TimePolygon> StoredPolygons = new List<TimePolygon>();

            public TimeContent Clone()
            {
                var stream = new System.IO.MemoryStream();
                var binFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                binFormatter.Serialize(stream, this);

                stream.Position = 0;
                object obj = binFormatter.Deserialize(stream);
                stream.Close();

                return obj as TimeContent;
            }
        }
        #endregion

        #region Copy/Paste Area
        public bool copyAreaMode = false;
        public bool pasteAreaMode = false;
        public Point pasteDest;
        public TimeContent CopiedArea;
        public List<Point> selectionPoly = new List<Point>();

        private void cmdCopyArea_CheckedChanged(object sender, EventArgs e)
        {
            if (cmdCopyArea.Checked)
            {
                SelectObjectBtn.PerformClick();
                mapPanel.Cursor = Cursors.Cross;
                selectionPoly = new List<Point>();
            }
            else
                mapPanel.Cursor = Cursors.Default;

            copyAreaMode = cmdCopyArea.Checked;
        }
        private void cmdCopyAll_Click(object sender, EventArgs e)
        {
            var wholeMap = new Point[] {
                new Point(0, 0), new Point(0, mapPanel.Height),
                new Point(mapPanel.Width, mapPanel.Height), new Point(mapPanel.Width, 0)
            };

            CopyArea(wholeMap);
        }
        private void cmdPasteArea_CheckedChanged(object sender, EventArgs e)
        {
            MapRenderer.FakeWalls.Clear();
            MapRenderer.FakeTiles.Clear();

            if (cmdPasteArea.Checked)
            {
                if (Clipboard.ContainsData("MapCopy"))
                    CopiedArea = (TimeContent)Clipboard.GetData("MapCopy");
                
                if (CopiedArea == null)
                {
                    MessageBox.Show("Nothing to paste.", "Error", MessageBoxButtons.OK);
                    cmdPasteArea.Checked = false;
                    return;
                }

                SelectObjectBtn.PerformClick();
                mapPanel.Cursor = Cursors.SizeAll;

                foreach (var tile in CopiedArea.StoredTiles)
                    MapRenderer.FakeTiles.Add(tile.Tile.Location, tile.Tile);
                foreach (var wall in CopiedArea.StoredWalls)
                    MapRenderer.FakeWalls.Add(wall.Wall.Location, wall.Wall);
            }
            else
            {
                mapPanel.Cursor = Cursors.Default;
                selectionPoly = new List<Point>();
            }

            MapRenderer.UpdateCanvas(false, true, false);
            mapPanel.Invalidate();
            pasteAreaMode = cmdPasteArea.Checked;
        }
        public void CopyArea(Point[] poly, int polyIndex = -1)
        {
            Clipboard.Clear();
            GC.Collect();
            CopiedArea = new TimeContent();
            StoreTiles(poly);
            StoreWalls(poly);
            StoreObjects(poly);
            StoreWaypoints(poly);
            StorePolygons(poly, polyIndex);

            if (CopiedArea.StoredTiles.Count != 0)
                CopiedArea.StoreTile = CopiedArea.StoredTiles[0].Tile.Location;

            Clipboard.SetData("MapCopy", CopiedArea.Clone());
            selectionPoly = poly.ToList();
            selectionPoly.Add(poly[0]);

            cmdCopyArea.Checked = false;
            cmdPasteArea.Enabled = true;
            ShowMapStatus("COPIED");
        }
        private void PasteArea(Point mouseCoords)
        {
            // Time Walls and Tiles already offset at this point
            if (chkCopyTiles.Checked) ReleaseTiles();
            if (chkCopyWalls.Checked) ReleaseWalls();
            if (chkCopyObjects.Checked)
            {
                OffsetObjects(mouseCoords);
                ReleaseObjects();
            }
            if (chkCopyWaypoints.Checked)
            {
                OffsetWaypoints(mouseCoords);
                ReleaseWaypoints();
            }
            if (chkCopyPolygons.Checked)
            {
                OffsetPolygons(mouseCoords);
                ReleasePolygons();
            }

            cmdPasteArea.Checked = false;
            MapRenderer.UpdateCanvas(true, true, true);
            MainWindow.Instance.minimap.Reload();
            MainWindow.Instance.Reload();
        }
        private void OffsetFakeCoords(Point mouseCoords)
        {
            Point offset;
            if (MapRenderer.FakeWalls.Count != 0)
                offset = GetFakeWallOffset(mouseCoords);
            else if (MapRenderer.FakeTiles.Count != 0)
                offset = GetFakeTileOffset(mouseCoords);
            else
                return;

            var newWalls = new Dictionary<Point, Map.Wall>();
            foreach (var wall in MapRenderer.FakeWalls.Values)
            {
                wall.Location.Offset(offset);
                newWalls.Add(wall.Location, wall);
            }
            MapRenderer.FakeWalls = newWalls;

            var newTiles = new Dictionary<Point, Map.Tile>();
            foreach (var tile in MapRenderer.FakeTiles.Values)
            {
                tile.Location.Offset(offset);
                newTiles.Add(tile.Location, tile);
            }
            MapRenderer.FakeTiles = newTiles;
        }
        private void OffsetObjects(Point mouseCoords)
        {
            // Relies on first tile for relative position
            if (CopiedArea.StoredTiles.Count == 0)
                return;

            var movedTile = CopiedArea.StoredTiles[0].Tile.Location;
            var tileOffset = new Point(movedTile.X - CopiedArea.StoreTile.X, movedTile.Y - CopiedArea.StoreTile.Y);

            // Offset Objects
            foreach (var obj in CopiedArea.StoredObjects)
            {
                // Current location
                obj.NearestMapTile = GetNearestTilePoint(obj.Object.Location.ToPoint());
                var distFromTile = new PointF(obj.Object.Location.X - obj.CartesianMapTile.X, obj.Object.Location.Y - obj.CartesianMapTile.Y);

                // New location
                obj.NearestMapTile.Offset(tileOffset);
                obj.Object.Location = obj.CartesianMapTile;
                var offsetPt = new PointF(obj.CartesianMapTile.X + distFromTile.X, obj.CartesianMapTile.Y + distFromTile.Y);
                obj.Object.Location = offsetPt;
            }
        }
        private void OffsetWaypoints(Point mouseCoords)
        {
            if (CopiedArea.StoredTiles.Count == 0)
                return;

            var movedTile = CopiedArea.StoredTiles[0].Tile.Location;
            var tileOffset = new Point(movedTile.X - CopiedArea.StoreTile.X, movedTile.Y - CopiedArea.StoreTile.Y);

            foreach (var wp in CopiedArea.StoredWPs)
            {
                wp.NearestMapTile = GetNearestTilePoint(wp.Waypoint.Point.ToPoint());
                var distFromTile = new PointF(wp.Waypoint.Point.X - wp.CartesianMapTile.X, wp.Waypoint.Point.Y - wp.CartesianMapTile.Y);

                wp.NearestMapTile.Offset(tileOffset);
                wp.Waypoint.Point = wp.CartesianMapTile;
                var offsetPt = new PointF(wp.CartesianMapTile.X + distFromTile.X, wp.CartesianMapTile.Y + distFromTile.Y);
                wp.Waypoint.Point = offsetPt;
            }
        }
        private void OffsetPolygons(Point mouseCoords)
        {
            if (CopiedArea.StoredTiles.Count == 0)
                return;

            var movedTile = CopiedArea.StoredTiles[0].Tile.Location;
            var tileOffset = new Point(movedTile.X - CopiedArea.StoreTile.X, movedTile.Y - CopiedArea.StoreTile.Y);

            foreach (var poly in CopiedArea.StoredPolygons)
            {
                var newPoints = new List<PointF>();
                foreach (var pt in poly.Polygon.Points)
                {
                    var nearestMapTile = GetNearestTilePoint(pt.ToPoint());
                    var cartesianMapTile = new PointF(nearestMapTile.X * squareSize + (squareSize / 2f), nearestMapTile.Y * squareSize + ((3 / 2f) * squareSize));
                    var distFromTile = new PointF(pt.X - cartesianMapTile.X, pt.Y - cartesianMapTile.Y);

                    nearestMapTile.Offset(tileOffset);
                    cartesianMapTile = new PointF(nearestMapTile.X * squareSize + (squareSize / 2f), nearestMapTile.Y * squareSize + ((3 / 2f) * squareSize));
                    var newPt = new PointF(cartesianMapTile.X + distFromTile.X, cartesianMapTile.Y + distFromTile.Y);
                    newPoints.Add(newPt);
                }
                poly.Polygon.Points = newPoints;
            }
        }
        private Point GetFakeWallOffset(Point mouseCoords)
        {
            var cursorWallPoint = GetNearestWallPoint(mouseCoords);

            Point leftWall = MapRenderer.FakeWalls.Keys.First();
            foreach (var wall in MapRenderer.FakeWalls)
            {
                if (wall.Key.X < leftWall.X)
                    leftWall = wall.Key;
            }

            // Relative mouse-to-wall coords
            return new Point(cursorWallPoint.X - leftWall.X, cursorWallPoint.Y - leftWall.Y);
        }
        private Point GetFakeTileOffset(Point mouseCoords)
        {
            var cursorTilePoint = GetNearestTilePoint(mouseCoords);

            Point leftTile = MapRenderer.FakeTiles.Keys.First();
            foreach (var tile in MapRenderer.FakeTiles)
            {
                if (tile.Key.X < leftTile.X)
                    leftTile = tile.Key;
            }
            // Relative mouse-to-wall coords
            return new Point(cursorTilePoint.X - leftTile.X, cursorTilePoint.Y - leftTile.Y);
        }

        private bool IsInsideMap(Point coords)
        {
            if ((coords.X < 0) || (coords.X > 252))
                return false;
            if ((coords.Y < 0) || (coords.Y > 252))
                return false;

            return true;
        }
        private bool IsInsideMap(PointF pixelCoords)
        {
            if ((pixelCoords.X < 0) || (pixelCoords.X > 5880))
                return false;
            if ((pixelCoords.Y < 0) || (pixelCoords.Y > 5880))
                return false;

            return true;
        }

        private void StoreWalls(Point[] poly)
        {
            foreach (Map.Wall wall in Map.Walls.Values)
            {
                if (!MapInterface.PointInPolygon(GetWallMapCoords(wall.Location), poly))
                    continue;

                StoreWall(wall);
            }
            // Make another pass around the points
            for (int i = 0; i < poly.Length; i++)
            {
                var w = GetNearestWallPoint(poly[i]);
                if (!Map.Walls.ContainsKey(w))
                    continue;
                if (CopiedArea.StoredWalls.Select(x => x.Wall.Location).Contains(w))
                    continue;

                StoreWall(Map.Walls[w]);
            }
            // Make one final pass between the points
            for (int i = 0; i < poly.Length; i++)
            {
                var a = poly[i];
                Point b;
                if (i == poly.Length - 1)
                    b = poly[0];
                else
                    b = poly[i + 1];

                var xDif = b.X - a.X;
                var yDif = b.Y - a.Y;
                // If distance is more than 1 tile
                if ((Math.Abs(xDif) > squareSize) || (Math.Abs(yDif) > squareSize))
                {
                    var numXPts = Math.Abs(xDif) / 2;
                    var numYPts = Math.Abs(yDif) / 2;

                    var numPts = numXPts;
                    if (numYPts > numXPts)
                        numPts = numYPts;

                    float xInc = (numPts != 0) ? (float)xDif / numPts : 0;
                    float yInc = (numPts != 0) ? (float)yDif / numPts : 0;

                    float x = a.X;
                    float y = a.Y;
                    for (int j = 0; j < numPts; j++)
                    {
                        x += xInc;
                        y += yInc;
                        var nextPt = new PointF(x, y);

                        var w = GetNearestWallPoint(nextPt.ToPoint());
                        if (!Map.Walls.ContainsKey(w))
                            continue;
                        if (CopiedArea.StoredWalls.Select(z => z.Wall.Location).Contains(w))
                            continue;

                        StoreWall(Map.Walls[w]);
                    }
                }
            }
        }
        private void StoreWall(Map.Wall wall)
        {
            TimeWall subject = new TimeWall();
            subject.Wall = wall.Clone();
            subject.Facing = wall.Facing;
            CopiedArea.StoredWalls.Add(subject);
        }
        private void ReleaseWalls()
        {
            if (CopiedArea.StoredWalls.Count == 0)
                return;

            Store(Mode.WALL_PLACE, TimeEvent.PRE, true);
            foreach (TimeWall wall in CopiedArea.StoredWalls)
            {
                if (!IsInsideMap(wall.Wall.Location))
                    continue;

                if (Map.Walls.ContainsKey(wall.Wall.Location))
                    Map.Walls[wall.Wall.Location] = wall.Wall;
                else
                    Map.Walls.Add(wall.Wall.Location, wall.Wall);

                Map.Walls[wall.Wall.Location].Facing = wall.Facing;
            }
            Store(Mode.WALL_PLACE, TimeEvent.POST, true);
        }
        private void StoreTiles(Point[] poly)
        {
            foreach (Map.Tile tile in Map.Tiles.Values)
            {
                var shiftTile = tile.Location;
                shiftTile.Offset(1, 1);
                if (!MapInterface.PointInPolygon(GetWallMapCoords(shiftTile), poly))
                    continue;

                StoreTile(tile);
            }
            // Make another pass around the points
            for (int i = 0; i < poly.Length; i++)
            {
                var t = GetNearestTilePoint(poly[i]);
                if (!Map.Tiles.ContainsKey(t))
                    continue;
                if (CopiedArea.StoredTiles.Select(x => x.Tile.Location).Contains(t))
                    continue;
                    
                StoreTile(Map.Tiles[t]);
            }
            // Make one final pass between the points
            for (int i = 0; i < poly.Length; i++)
            {
                var a = poly[i];
                Point b;
                if (i == poly.Length - 1)
                    b = poly[0];
                else
                    b = poly[i + 1];

                var xDif = b.X - a.X;
                var yDif = b.Y - a.Y;
                // If distance is more than 1 tile
                if ((Math.Abs(xDif) > squareSize) || (Math.Abs(yDif) > squareSize))
                {
                    var numXPts = Math.Abs(xDif) / 2;
                    var numYPts = Math.Abs(yDif) / 2;
                    
                    var numPts = numXPts;
                    if (numYPts > numXPts)
                        numPts = numYPts;

                    float xInc = (numPts != 0) ? (float)xDif / numPts : 0;
                    float yInc = (numPts != 0) ? (float)yDif / numPts : 0;

                    float x = a.X;
                    float y = a.Y;
                    for (int j = 0; j < numPts; j++)
                    {
                        x += xInc;
                        y += yInc;
                        var nextPt = new PointF(x, y);

                        var t = GetNearestTilePoint(nextPt.ToPoint());
                        if (!Map.Tiles.ContainsKey(t))
                            continue;
                        if (CopiedArea.StoredTiles.Select(z => z.Tile.Location).Contains(t))
                            continue;

                        StoreTile(Map.Tiles[t]);
                    }
                }
            }
        }
        private void StoreTile(Map.Tile tile)
        {
            TimeTile subject = new TimeTile();
            subject.Tile = tile.Clone();
            subject.EdgeTiles = tile.EdgeTiles;
            subject.Tile.Location = tile.Location;
            CopiedArea.StoredTiles.Add(subject);
        }
        private void ReleaseTiles()
        {
            if (CopiedArea.StoredTiles.Count == 0)
                return;

            Store(Mode.FLOOR_PLACE, TimeEvent.PRE, true);
            foreach (TimeTile tile in CopiedArea.StoredTiles)
            {
                if (!IsInsideMap(tile.Tile.Location))
                    continue;

                if (Map.Tiles.ContainsKey(tile.Tile.Location))
                    Map.Tiles[tile.Tile.Location] = tile.Tile;
                else
                    Map.Tiles.Add(tile.Tile.Location, tile.Tile);

                Map.Tiles[tile.Tile.Location].EdgeTiles = tile.EdgeTiles;
            }
            Store(Mode.FLOOR_PLACE, TimeEvent.POST, true);
        }
        private void StoreObjects(Point[] poly)
        {
            foreach (Map.Object item in Map.Objects)
            {
                if (!MapInterface.PointInPolygon(new Point((int)item.Location.X, (int)item.Location.Y), poly))
                    continue;

                TimeObject subject = new TimeObject();
                subject.Location = item.Location;
                subject.Object = (Map.Object)item.Clone();
                CopiedArea.StoredObjects.Add(subject);
            }
        }
        private void ReleaseObjects()
        {
            if (CopiedArea.StoredObjects.Count == 0)
                return;

            Store(Mode.OBJECT_SELECT, TimeEvent.PRE, true);
            foreach (TimeObject item in CopiedArea.StoredObjects)
            {
                if (!IsInsideMap(item.Object.Location))
                    continue;

                Map.Objects.Add(item.Object);
            }

            MapInterface.FixObjectExtents();
            Store(Mode.OBJECT_SELECT, TimeEvent.POST, true);
        }
        private void StoreWaypoints(Point[] poly)
        {
            foreach (Map.Waypoint wp in Map.Waypoints)
            {
                if (!MapInterface.PointInPolygon(new Point((int)wp.Point.X, (int)wp.Point.Y), poly))
                    continue;

                TimeWaypoint subject = new TimeWaypoint();
                subject.Waypoint = wp.Clone();
                CopiedArea.StoredWPs.Add(subject);
            }

        }
        private void ReleaseWaypoints()
        {
            if (CopiedArea.StoredWPs.Count == 0)
                return;

            FixTimeWaypointExtents();

            Store(Mode.WAYPOINT_PLACE, TimeEvent.PRE, true);
            foreach (TimeWaypoint wp in CopiedArea.StoredWPs)
            {
                if (!IsInsideMap(wp.Waypoint.Point))
                    continue;

                Map.Waypoints.Add(wp.Waypoint);
            }
            Store(Mode.WAYPOINT_PLACE, TimeEvent.POST, true);
        }
        private void FixTimeWaypointExtents()
        {
            // <Original Extent, Offset Extent>
            var numMap = new Dictionary<int, int>();

            var i = 0;
            if (Map.Waypoints.Count != 0)
                i = Map.Waypoints.ToArray().Select(x => ((Map.Waypoint)x).Number).Max();

            // Set new extent and populate offset dictionary
            foreach (TimeWaypoint wp in CopiedArea.StoredWPs)
            {
                i++;
                numMap.Add(wp.Waypoint.Number, i);
                wp.Waypoint.Number = i;
            }

            // Set new connection extents and wipe broken connections
            foreach (TimeWaypoint wp in CopiedArea.StoredWPs)
            {
                var newConns = new ArrayList();
                foreach (Map.Waypoint.WaypointConnection conn in wp.Waypoint.connections)
                {
                    if (numMap.ContainsKey(conn.wp_num))
                    {
                        conn.wp_num = numMap[conn.wp_num];
                        conn.wp = GetTimeWaypointByNum(conn.wp_num);
                        newConns.Add(conn);
                    }
                }

                wp.Waypoint.connections = newConns;
            }
        }
        private Map.Waypoint GetTimeWaypointByNum(int num)
        {
            foreach (TimeWaypoint wp in CopiedArea.StoredWPs)
                if (wp.Waypoint.Number == num)
                    return wp.Waypoint;

            return null;
        }
        private void StorePolygons(Point[] poly, int polyIndex = -1)
        {
            int i = -1;
            foreach (Map.Polygon polygon in Map.Polygons)
            {
                i++;
                var isInside = true;
                foreach (PointF point in polygon.Points)
                    if (!MapInterface.PointInPolygon(new Point((int)point.X, (int)point.Y), poly))
                        isInside = false;

                if ((!isInside) && (i != polyIndex))
                    continue;

                TimePolygon subject = new TimePolygon();
                subject.Polygon = polygon.Clone();
                CopiedArea.StoredPolygons.Add(subject);
            }
        }
        private void ReleasePolygons()
        {
            if (CopiedArea.StoredPolygons.Count == 0)
                return;
            Store(Mode.POLYGON_RESHAPE, TimeEvent.PRE, true);
            foreach (TimePolygon polygon in CopiedArea.StoredPolygons)
            {
                var isInside = true;
                foreach (PointF polyPoint in polygon.Polygon.Points)
                    if (!IsInsideMap(polyPoint))
                        isInside = false;

                if (isInside)
                    Map.Polygons.Add(polygon.Polygon);
            }
            Store(Mode.POLYGON_RESHAPE, TimeEvent.POST, true);
        }
        #endregion

        //--------------
        #region Windows Form Designer generated code
        private MenuItem contextMenuDelete;
        private MenuItem contextMenuProperties;
        private MenuItem menuItem3;
        private RadioButton[] buttons = new RadioButton[5];
        private StatusBarPanel statusMapItem;
        private StatusBar statusBar;
        public StatusBarPanel statusLocation;
        private StatusBarPanel statusMode;
        public Panel scrollPanel; //WARNING: the form designer is not happy with this
        public FlickerFreePanel mapPanel;
        private MenuItem contextMenuCopy;
        private MenuItem contextMenuPaste;
        private ContextMenu contextMenu;
        private Timer tmrInvalidate;
        private System.ComponentModel.IContainer components;
        private GroupBox groupAdv;
        private TabControl tabMapTools;
        private TabPage tabObjectWps;
        private GroupBox objectGroup;
        private RadioButton radFullSnap;
        private RadioButton radCenterSnap;
        private RadioButton radNoSnap;
        private GroupBox extentsGroup;
        private RadioButton radioExtentShowColl;
        private RadioButton radioExtentsShowAll;
        private RadioButton radCustom;
        public NumericUpDown customSnapValue;
        private Button waypointSelectAll;
        private RadioButton pathWPBtn;
        private RadioButton placeWPBtn;
        private RadioButton selectWPBtn;
        private ToolTip toolTip1;
        public Button cmdUndo;
        public Button cmdRedo;
        private Button cmdQuickSave;
        private RadioButton radioExtentsHide;
        public CheckBox cmdQuickPreview;
        private Timer UndoTimer;
        private ListBox lstDebug;
        private Timer RedoTimer;
        public CheckBox doubleWp;
        private MenuItem contextcopyContent;
        private bool moved = false;
        public Label label2;
        public bool done;
        public bool contextMenuOpen;
        private ContextMenuStrip contextMenuStrip;
        private GroupBox groupGridSnap;
        public CheckBox waypointEnabled;
        private Label label1;
        public TextBox waypointName;
        private GroupBox waypointGroup;
        public EdgeMakeTab EdgeMakeNewCtrl;
        private TabPage tabEdges;
        public TileMakeTab TileMakeNewCtrl;
        private TabPage tabTiles;
        public WallMakeTab WallMakeNewCtrl;
        private TabPage tabWalls;
        private StatusBarPanel statusPolygon;
        private Label lblMapStatus;
        private Timer tmrFade;
        private Timer tmrFadeTicker;
        private Button cmdCopyAll;
        private CheckBox cmdCopyArea;
        private Label label3;
        private CheckBox cmdPasteArea;
        private GroupBox groupMapCopy;
        public CheckBox chkCopyObjects;
        public CheckBox chkCopyWalls;
        public CheckBox chkCopyTiles;
        public CheckBox chkCopyWaypoints;
        public CheckBox chkCopyPolygons;
        private Button cmdQuickSettings;
        public RadioButton SelectObjectBtn;
        public CheckBox Picker;
        private RadioButton PlaceObjectBtn;
        private ComboBox objectCategoriesBox;
        public CheckBox select45Box;
        public ComboBox cboObjCreate;
        public PictureBox objectPreview;
        private Label label6;
        private ContextMenuStrip contexMenu = new ContextMenuStrip();

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapView));
            this.contextMenuDelete = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.contextMenuProperties = new System.Windows.Forms.MenuItem();
            this.statusBar = new System.Windows.Forms.StatusBar();
            this.statusMode = new System.Windows.Forms.StatusBarPanel();
            this.statusLocation = new System.Windows.Forms.StatusBarPanel();
            this.statusMapItem = new System.Windows.Forms.StatusBarPanel();
            this.statusPolygon = new System.Windows.Forms.StatusBarPanel();
            this.groupAdv = new System.Windows.Forms.GroupBox();
            this.cmdQuickSettings = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cmdQuickPreview = new System.Windows.Forms.CheckBox();
            this.cmdQuickSave = new System.Windows.Forms.Button();
            this.cmdRedo = new System.Windows.Forms.Button();
            this.tabMapTools = new System.Windows.Forms.TabControl();
            this.tabWalls = new System.Windows.Forms.TabPage();
            this.WallMakeNewCtrl = new MapEditor.newgui.WallMakeTab();
            this.tabTiles = new System.Windows.Forms.TabPage();
            this.TileMakeNewCtrl = new MapEditor.newgui.TileMakeTab();
            this.tabEdges = new System.Windows.Forms.TabPage();
            this.EdgeMakeNewCtrl = new MapEditor.newgui.EdgeMakeTab();
            this.tabObjectWps = new System.Windows.Forms.TabPage();
            this.groupGridSnap = new System.Windows.Forms.GroupBox();
            this.customSnapValue = new System.Windows.Forms.NumericUpDown();
            this.radFullSnap = new System.Windows.Forms.RadioButton();
            this.radNoSnap = new System.Windows.Forms.RadioButton();
            this.radCenterSnap = new System.Windows.Forms.RadioButton();
            this.radCustom = new System.Windows.Forms.RadioButton();
            this.waypointGroup = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.waypointSelectAll = new System.Windows.Forms.Button();
            this.waypointName = new System.Windows.Forms.TextBox();
            this.placeWPBtn = new System.Windows.Forms.RadioButton();
            this.doubleWp = new System.Windows.Forms.CheckBox();
            this.selectWPBtn = new System.Windows.Forms.RadioButton();
            this.waypointEnabled = new System.Windows.Forms.CheckBox();
            this.pathWPBtn = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.extentsGroup = new System.Windows.Forms.GroupBox();
            this.radioExtentsShowAll = new System.Windows.Forms.RadioButton();
            this.radioExtentsHide = new System.Windows.Forms.RadioButton();
            this.radioExtentShowColl = new System.Windows.Forms.RadioButton();
            this.objectGroup = new System.Windows.Forms.GroupBox();
            this.SelectObjectBtn = new System.Windows.Forms.RadioButton();
            this.objectPreview = new System.Windows.Forms.PictureBox();
            this.select45Box = new System.Windows.Forms.CheckBox();
            this.Picker = new System.Windows.Forms.CheckBox();
            this.objectCategoriesBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cboObjCreate = new System.Windows.Forms.ComboBox();
            this.PlaceObjectBtn = new System.Windows.Forms.RadioButton();
            this.groupMapCopy = new System.Windows.Forms.GroupBox();
            this.chkCopyObjects = new System.Windows.Forms.CheckBox();
            this.chkCopyWalls = new System.Windows.Forms.CheckBox();
            this.cmdPasteArea = new System.Windows.Forms.CheckBox();
            this.cmdCopyAll = new System.Windows.Forms.Button();
            this.cmdCopyArea = new System.Windows.Forms.CheckBox();
            this.chkCopyTiles = new System.Windows.Forms.CheckBox();
            this.chkCopyWaypoints = new System.Windows.Forms.CheckBox();
            this.chkCopyPolygons = new System.Windows.Forms.CheckBox();
            this.cmdUndo = new System.Windows.Forms.Button();
            this.lblMapStatus = new System.Windows.Forms.Label();
            this.lstDebug = new System.Windows.Forms.ListBox();
            this.scrollPanel = new System.Windows.Forms.Panel();
            this.mapPanel = new MapEditor.MapView.FlickerFreePanel();
            this.contextMenu = new System.Windows.Forms.ContextMenu();
            this.contextMenuCopy = new System.Windows.Forms.MenuItem();
            this.contextMenuPaste = new System.Windows.Forms.MenuItem();
            this.contextcopyContent = new System.Windows.Forms.MenuItem();
            this.tmrInvalidate = new System.Windows.Forms.Timer(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.UndoTimer = new System.Windows.Forms.Timer(this.components);
            this.RedoTimer = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tmrFade = new System.Windows.Forms.Timer(this.components);
            this.tmrFadeTicker = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.statusMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusLocation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusMapItem)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusPolygon)).BeginInit();
            this.groupAdv.SuspendLayout();
            this.tabMapTools.SuspendLayout();
            this.tabWalls.SuspendLayout();
            this.tabTiles.SuspendLayout();
            this.tabEdges.SuspendLayout();
            this.tabObjectWps.SuspendLayout();
            this.groupGridSnap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.customSnapValue)).BeginInit();
            this.waypointGroup.SuspendLayout();
            this.extentsGroup.SuspendLayout();
            this.objectGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.objectPreview)).BeginInit();
            this.groupMapCopy.SuspendLayout();
            this.scrollPanel.SuspendLayout();
            this.mapPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuDelete
            // 
            this.contextMenuDelete.Index = 2;
            this.contextMenuDelete.Text = "Delete";
            this.contextMenuDelete.Click += new System.EventHandler(this.contextMenuDelete_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 3;
            this.menuItem3.Text = "-";
            // 
            // contextMenuProperties
            // 
            this.contextMenuProperties.Index = 4;
            this.contextMenuProperties.Text = "Properties";
            this.contextMenuProperties.Click += new System.EventHandler(this.contextMenuProperties_Click);
            // 
            // statusBar
            // 
            this.statusBar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.statusBar.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.statusBar.Location = new System.Drawing.Point(0, 682);
            this.statusBar.Name = "statusBar";
            this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statusMode,
            this.statusLocation,
            this.statusMapItem,
            this.statusPolygon});
            this.statusBar.ShowPanels = true;
            this.statusBar.Size = new System.Drawing.Size(859, 22);
            this.statusBar.SizingGrip = false;
            this.statusBar.TabIndex = 1;
            // 
            // statusMode
            // 
            this.statusMode.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
            this.statusMode.MinWidth = 0;
            this.statusMode.Name = "statusMode";
            this.statusMode.ToolTipText = "Currently active editor mode";
            this.statusMode.Width = 10;
            // 
            // statusLocation
            // 
            this.statusLocation.MinWidth = 0;
            this.statusLocation.Name = "statusLocation";
            this.statusLocation.ToolTipText = "Location (depends on mode)";
            // 
            // statusMapItem
            // 
            this.statusMapItem.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
            this.statusMapItem.MinWidth = 0;
            this.statusMapItem.Name = "statusMapItem";
            this.statusMapItem.ToolTipText = "Info about the map element";
            this.statusMapItem.Width = 10;
            // 
            // statusPolygon
            // 
            this.statusPolygon.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
            this.statusPolygon.MinWidth = 0;
            this.statusPolygon.Name = "statusPolygon";
            this.statusPolygon.ToolTipText = "Name of the polygon";
            this.statusPolygon.Width = 10;
            // 
            // groupAdv
            // 
            this.groupAdv.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupAdv.Controls.Add(this.cmdQuickSettings);
            this.groupAdv.Controls.Add(this.label2);
            this.groupAdv.Controls.Add(this.cmdQuickPreview);
            this.groupAdv.Controls.Add(this.cmdQuickSave);
            this.groupAdv.Controls.Add(this.cmdRedo);
            this.groupAdv.Controls.Add(this.tabMapTools);
            this.groupAdv.Controls.Add(this.cmdUndo);
            this.groupAdv.Controls.Add(this.lblMapStatus);
            this.groupAdv.Location = new System.Drawing.Point(-1, 0);
            this.groupAdv.Name = "groupAdv";
            this.groupAdv.Size = new System.Drawing.Size(250, 680);
            this.groupAdv.TabIndex = 28;
            this.groupAdv.TabStop = false;
            // 
            // cmdQuickSettings
            // 
            this.cmdQuickSettings.BackgroundImage = global::MapEditor.Properties.Resources.gear;
            this.cmdQuickSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdQuickSettings.Location = new System.Drawing.Point(79, 8);
            this.cmdQuickSettings.Name = "cmdQuickSettings";
            this.cmdQuickSettings.Size = new System.Drawing.Size(25, 25);
            this.cmdQuickSettings.TabIndex = 36;
            this.toolTip1.SetToolTip(this.cmdQuickSettings, "Settings (Ctrl + G)");
            this.cmdQuickSettings.UseVisualStyleBackColor = true;
            this.cmdQuickSettings.Click += new System.EventHandler(this.cmdQuickSettings_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(129, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 13);
            this.label2.TabIndex = 34;
            this.label2.Visible = false;
            // 
            // cmdQuickPreview
            // 
            this.cmdQuickPreview.Appearance = System.Windows.Forms.Appearance.Button;
            this.cmdQuickPreview.BackgroundImage = global::MapEditor.Properties.Resources.hidePreview;
            this.cmdQuickPreview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdQuickPreview.Location = new System.Drawing.Point(103, 8);
            this.cmdQuickPreview.Name = "cmdQuickPreview";
            this.cmdQuickPreview.Size = new System.Drawing.Size(25, 25);
            this.cmdQuickPreview.TabIndex = 32;
            this.toolTip1.SetToolTip(this.cmdQuickPreview, "Toggle Preview (Ctrl + F)");
            this.cmdQuickPreview.UseVisualStyleBackColor = true;
            this.cmdQuickPreview.CheckedChanged += new System.EventHandler(this.cmdQuickPreview_CheckedChanged);
            this.cmdQuickPreview.Click += new System.EventHandler(this.cmdQuickPreview_Click);
            // 
            // cmdQuickSave
            // 
            this.cmdQuickSave.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cmdQuickSave.BackgroundImage")));
            this.cmdQuickSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdQuickSave.Location = new System.Drawing.Point(7, 8);
            this.cmdQuickSave.Name = "cmdQuickSave";
            this.cmdQuickSave.Size = new System.Drawing.Size(25, 25);
            this.cmdQuickSave.TabIndex = 31;
            this.toolTip1.SetToolTip(this.cmdQuickSave, "Save (F2)");
            this.cmdQuickSave.UseVisualStyleBackColor = true;
            this.cmdQuickSave.Click += new System.EventHandler(this.cmdQuickSave_Click);
            // 
            // cmdRedo
            // 
            this.cmdRedo.BackgroundImage = global::MapEditor.Properties.Resources.redoDisabled;
            this.cmdRedo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdRedo.Enabled = false;
            this.cmdRedo.Location = new System.Drawing.Point(55, 8);
            this.cmdRedo.Name = "cmdRedo";
            this.cmdRedo.Size = new System.Drawing.Size(25, 25);
            this.cmdRedo.TabIndex = 30;
            this.toolTip1.SetToolTip(this.cmdRedo, "Redo (Ctrl + Y)");
            this.cmdRedo.UseVisualStyleBackColor = true;
            this.cmdRedo.EnabledChanged += new System.EventHandler(this.cmdRedo_EnabledChanged);
            this.cmdRedo.Click += new System.EventHandler(this.cmdRedo_Click);
            // 
            // tabMapTools
            // 
            this.tabMapTools.Controls.Add(this.tabWalls);
            this.tabMapTools.Controls.Add(this.tabTiles);
            this.tabMapTools.Controls.Add(this.tabEdges);
            this.tabMapTools.Controls.Add(this.tabObjectWps);
            this.tabMapTools.Location = new System.Drawing.Point(7, 33);
            this.tabMapTools.Name = "tabMapTools";
            this.tabMapTools.SelectedIndex = 0;
            this.tabMapTools.Size = new System.Drawing.Size(236, 641);
            this.tabMapTools.TabIndex = 29;
            this.tabMapTools.SelectedIndexChanged += new System.EventHandler(this.TabMapToolsSelectedIndexChanged);
            // 
            // tabWalls
            // 
            this.tabWalls.BackColor = System.Drawing.Color.LightGray;
            this.tabWalls.Controls.Add(this.WallMakeNewCtrl);
            this.tabWalls.Location = new System.Drawing.Point(4, 22);
            this.tabWalls.Name = "tabWalls";
            this.tabWalls.Padding = new System.Windows.Forms.Padding(3);
            this.tabWalls.Size = new System.Drawing.Size(228, 615);
            this.tabWalls.TabIndex = 4;
            this.tabWalls.Text = "Walls";
            // 
            // WallMakeNewCtrl
            // 
            this.WallMakeNewCtrl.Location = new System.Drawing.Point(6, 0);
            this.WallMakeNewCtrl.Name = "WallMakeNewCtrl";
            this.WallMakeNewCtrl.SelectedWallFacing = 0;
            this.WallMakeNewCtrl.Size = new System.Drawing.Size(216, 614);
            this.WallMakeNewCtrl.TabIndex = 0;
            this.WallMakeNewCtrl.Tag = "d";
            // 
            // tabTiles
            // 
            this.tabTiles.BackColor = System.Drawing.Color.LightGray;
            this.tabTiles.Controls.Add(this.TileMakeNewCtrl);
            this.tabTiles.Location = new System.Drawing.Point(4, 22);
            this.tabTiles.Name = "tabTiles";
            this.tabTiles.Size = new System.Drawing.Size(192, 74);
            this.tabTiles.TabIndex = 5;
            this.tabTiles.Text = "Tiles";
            // 
            // TileMakeNewCtrl
            // 
            this.TileMakeNewCtrl.Location = new System.Drawing.Point(6, 0);
            this.TileMakeNewCtrl.Name = "TileMakeNewCtrl";
            this.TileMakeNewCtrl.Size = new System.Drawing.Size(216, 612);
            this.TileMakeNewCtrl.TabIndex = 0;
            // 
            // tabEdges
            // 
            this.tabEdges.BackColor = System.Drawing.Color.LightGray;
            this.tabEdges.Controls.Add(this.EdgeMakeNewCtrl);
            this.tabEdges.Location = new System.Drawing.Point(4, 22);
            this.tabEdges.Name = "tabEdges";
            this.tabEdges.Size = new System.Drawing.Size(192, 74);
            this.tabEdges.TabIndex = 6;
            this.tabEdges.Text = "Edges";
            this.tabEdges.Enter += new System.EventHandler(this.tabEdges_Enter);
            // 
            // EdgeMakeNewCtrl
            // 
            this.EdgeMakeNewCtrl.BackColor = System.Drawing.Color.Transparent;
            this.EdgeMakeNewCtrl.Location = new System.Drawing.Point(6, 0);
            this.EdgeMakeNewCtrl.Name = "EdgeMakeNewCtrl";
            this.EdgeMakeNewCtrl.Size = new System.Drawing.Size(216, 536);
            this.EdgeMakeNewCtrl.TabIndex = 1;
            this.EdgeMakeNewCtrl.TabStop = false;
            // 
            // tabObjectWps
            // 
            this.tabObjectWps.BackColor = System.Drawing.Color.LightGray;
            this.tabObjectWps.Controls.Add(this.groupGridSnap);
            this.tabObjectWps.Controls.Add(this.waypointGroup);
            this.tabObjectWps.Controls.Add(this.extentsGroup);
            this.tabObjectWps.Controls.Add(this.objectGroup);
            this.tabObjectWps.Controls.Add(this.groupMapCopy);
            this.tabObjectWps.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.tabObjectWps.Location = new System.Drawing.Point(4, 22);
            this.tabObjectWps.Name = "tabObjectWps";
            this.tabObjectWps.Size = new System.Drawing.Size(228, 615);
            this.tabObjectWps.TabIndex = 2;
            this.tabObjectWps.Text = "Objects/Waypoints";
            // 
            // groupGridSnap
            // 
            this.groupGridSnap.Controls.Add(this.customSnapValue);
            this.groupGridSnap.Controls.Add(this.radFullSnap);
            this.groupGridSnap.Controls.Add(this.radNoSnap);
            this.groupGridSnap.Controls.Add(this.radCenterSnap);
            this.groupGridSnap.Controls.Add(this.radCustom);
            this.groupGridSnap.Location = new System.Drawing.Point(113, 406);
            this.groupGridSnap.Name = "groupGridSnap";
            this.groupGridSnap.Size = new System.Drawing.Size(110, 100);
            this.groupGridSnap.TabIndex = 32;
            this.groupGridSnap.TabStop = false;
            this.groupGridSnap.Text = "Grid Snap";
            // 
            // customSnapValue
            // 
            this.customSnapValue.Enabled = false;
            this.customSnapValue.Location = new System.Drawing.Point(65, 72);
            this.customSnapValue.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.customSnapValue.Name = "customSnapValue";
            this.customSnapValue.Size = new System.Drawing.Size(39, 20);
            this.customSnapValue.TabIndex = 29;
            this.customSnapValue.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // radFullSnap
            // 
            this.radFullSnap.AutoSize = true;
            this.radFullSnap.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radFullSnap.Location = new System.Drawing.Point(6, 55);
            this.radFullSnap.Name = "radFullSnap";
            this.radFullSnap.Size = new System.Drawing.Size(69, 17);
            this.radFullSnap.TabIndex = 28;
            this.radFullSnap.Text = "Full Snap";
            this.radFullSnap.UseVisualStyleBackColor = true;
            this.radFullSnap.CheckedChanged += new System.EventHandler(this.radFullSnap_CheckedChanged);
            // 
            // radNoSnap
            // 
            this.radNoSnap.AutoSize = true;
            this.radNoSnap.Checked = true;
            this.radNoSnap.Location = new System.Drawing.Point(6, 19);
            this.radNoSnap.Name = "radNoSnap";
            this.radNoSnap.Size = new System.Drawing.Size(51, 17);
            this.radNoSnap.TabIndex = 26;
            this.radNoSnap.TabStop = true;
            this.radNoSnap.Text = "None";
            this.radNoSnap.UseVisualStyleBackColor = true;
            this.radNoSnap.CheckedChanged += new System.EventHandler(this.radNoSnap_CheckedChanged);
            // 
            // radCenterSnap
            // 
            this.radCenterSnap.AutoSize = true;
            this.radCenterSnap.BackColor = System.Drawing.Color.Transparent;
            this.radCenterSnap.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.radCenterSnap.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radCenterSnap.Location = new System.Drawing.Point(6, 37);
            this.radCenterSnap.Name = "radCenterSnap";
            this.radCenterSnap.Size = new System.Drawing.Size(84, 17);
            this.radCenterSnap.TabIndex = 27;
            this.radCenterSnap.Text = "Center/Door";
            this.radCenterSnap.UseVisualStyleBackColor = false;
            this.radCenterSnap.CheckedChanged += new System.EventHandler(this.radCenterSnap_CheckedChanged);
            // 
            // radCustom
            // 
            this.radCustom.AutoSize = true;
            this.radCustom.Location = new System.Drawing.Point(6, 73);
            this.radCustom.Name = "radCustom";
            this.radCustom.Size = new System.Drawing.Size(63, 17);
            this.radCustom.TabIndex = 30;
            this.radCustom.TabStop = true;
            this.radCustom.Text = "Custom:";
            this.radCustom.UseVisualStyleBackColor = true;
            this.radCustom.CheckedChanged += new System.EventHandler(this.radCustom_CheckedChanged);
            // 
            // waypointGroup
            // 
            this.waypointGroup.Controls.Add(this.label3);
            this.waypointGroup.Controls.Add(this.waypointSelectAll);
            this.waypointGroup.Controls.Add(this.waypointName);
            this.waypointGroup.Controls.Add(this.placeWPBtn);
            this.waypointGroup.Controls.Add(this.doubleWp);
            this.waypointGroup.Controls.Add(this.selectWPBtn);
            this.waypointGroup.Controls.Add(this.waypointEnabled);
            this.waypointGroup.Controls.Add(this.pathWPBtn);
            this.waypointGroup.Controls.Add(this.label1);
            this.waypointGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.waypointGroup.Location = new System.Drawing.Point(7, 265);
            this.waypointGroup.Name = "waypointGroup";
            this.waypointGroup.Size = new System.Drawing.Size(215, 138);
            this.waypointGroup.TabIndex = 30;
            this.waypointGroup.TabStop = false;
            this.waypointGroup.Text = " Waypoints";
            // 
            // label3
            // 
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label3.Location = new System.Drawing.Point(13, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(189, 2);
            this.label3.TabIndex = 30;
            // 
            // waypointSelectAll
            // 
            this.waypointSelectAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.waypointSelectAll.Location = new System.Drawing.Point(56, 46);
            this.waypointSelectAll.Name = "waypointSelectAll";
            this.waypointSelectAll.Size = new System.Drawing.Size(63, 21);
            this.waypointSelectAll.TabIndex = 21;
            this.waypointSelectAll.Text = "Select All";
            this.waypointSelectAll.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.toolTip1.SetToolTip(this.waypointSelectAll, "Selects all waypoints");
            this.waypointSelectAll.UseVisualStyleBackColor = true;
            this.waypointSelectAll.Click += new System.EventHandler(this.WaypointSelectAll_Click);
            // 
            // waypointName
            // 
            this.waypointName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.waypointName.Location = new System.Drawing.Point(58, 22);
            this.waypointName.Name = "waypointName";
            this.waypointName.Size = new System.Drawing.Size(136, 20);
            this.waypointName.TabIndex = 0;
            this.waypointName.TextChanged += new System.EventHandler(this.WaypointName_TextChanged);
            // 
            // placeWPBtn
            // 
            this.placeWPBtn.Appearance = System.Windows.Forms.Appearance.Button;
            this.placeWPBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.placeWPBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.placeWPBtn.Location = new System.Drawing.Point(12, 78);
            this.placeWPBtn.Name = "placeWPBtn";
            this.placeWPBtn.Size = new System.Drawing.Size(95, 23);
            this.placeWPBtn.TabIndex = 23;
            this.placeWPBtn.TabStop = true;
            this.placeWPBtn.Text = "Create";
            this.placeWPBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.placeWPBtn, "Creating new waypoints (Switch: ~)");
            this.placeWPBtn.UseVisualStyleBackColor = true;
            this.placeWPBtn.CheckedChanged += new System.EventHandler(this.ObjectModesChanged);
            // 
            // doubleWp
            // 
            this.doubleWp.AutoSize = true;
            this.doubleWp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.doubleWp.Location = new System.Drawing.Point(113, 109);
            this.doubleWp.Name = "doubleWp";
            this.doubleWp.Size = new System.Drawing.Size(85, 17);
            this.doubleWp.TabIndex = 25;
            this.doubleWp.Text = "Double Way";
            this.toolTip1.SetToolTip(this.doubleWp, "Set bi-directional paths");
            this.doubleWp.UseVisualStyleBackColor = true;
            // 
            // selectWPBtn
            // 
            this.selectWPBtn.Appearance = System.Windows.Forms.Appearance.Button;
            this.selectWPBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.selectWPBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.selectWPBtn.Location = new System.Drawing.Point(106, 78);
            this.selectWPBtn.Name = "selectWPBtn";
            this.selectWPBtn.Size = new System.Drawing.Size(95, 23);
            this.selectWPBtn.TabIndex = 22;
            this.selectWPBtn.TabStop = true;
            this.selectWPBtn.Text = "Select";
            this.selectWPBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.selectWPBtn, "Selecting waypoints (Switch: ~)");
            this.selectWPBtn.UseVisualStyleBackColor = true;
            this.selectWPBtn.CheckedChanged += new System.EventHandler(this.ObjectModesChanged);
            // 
            // waypointEnabled
            // 
            this.waypointEnabled.AutoSize = true;
            this.waypointEnabled.Checked = true;
            this.waypointEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.waypointEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.waypointEnabled.Location = new System.Drawing.Point(126, 48);
            this.waypointEnabled.Name = "waypointEnabled";
            this.waypointEnabled.Size = new System.Drawing.Size(65, 17);
            this.waypointEnabled.TabIndex = 2;
            this.waypointEnabled.Text = "Enabled";
            this.waypointEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.waypointEnabled.UseVisualStyleBackColor = true;
            this.waypointEnabled.CheckedChanged += new System.EventHandler(this.WaypointEnabled_CheckedChanged);
            // 
            // pathWPBtn
            // 
            this.pathWPBtn.Appearance = System.Windows.Forms.Appearance.Button;
            this.pathWPBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.pathWPBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.pathWPBtn.Location = new System.Drawing.Point(12, 105);
            this.pathWPBtn.Name = "pathWPBtn";
            this.pathWPBtn.Size = new System.Drawing.Size(95, 23);
            this.pathWPBtn.TabIndex = 24;
            this.pathWPBtn.TabStop = true;
            this.pathWPBtn.Text = "Make Path";
            this.pathWPBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.pathWPBtn, "Create path between 2 waypoints");
            this.pathWPBtn.UseVisualStyleBackColor = true;
            this.pathWPBtn.CheckedChanged += new System.EventHandler(this.ObjectModesChanged);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(15, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 23);
            this.label1.TabIndex = 1;
            this.label1.Text = "Name:";
            // 
            // extentsGroup
            // 
            this.extentsGroup.Controls.Add(this.radioExtentsShowAll);
            this.extentsGroup.Controls.Add(this.radioExtentsHide);
            this.extentsGroup.Controls.Add(this.radioExtentShowColl);
            this.extentsGroup.Location = new System.Drawing.Point(8, 406);
            this.extentsGroup.Name = "extentsGroup";
            this.extentsGroup.Size = new System.Drawing.Size(104, 80);
            this.extentsGroup.TabIndex = 29;
            this.extentsGroup.TabStop = false;
            this.extentsGroup.Text = "Extent Box";
            // 
            // radioExtentsShowAll
            // 
            this.radioExtentsShowAll.AutoSize = true;
            this.radioExtentsShowAll.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioExtentsShowAll.Location = new System.Drawing.Point(6, 55);
            this.radioExtentsShowAll.Name = "radioExtentsShowAll";
            this.radioExtentsShowAll.Size = new System.Drawing.Size(66, 17);
            this.radioExtentsShowAll.TabIndex = 1;
            this.radioExtentsShowAll.Text = "Show All";
            this.radioExtentsShowAll.UseVisualStyleBackColor = true;
            this.radioExtentsShowAll.CheckedChanged += new System.EventHandler(this.radShowExtents_CheckedChanged);
            // 
            // radioExtentsHide
            // 
            this.radioExtentsHide.AutoSize = true;
            this.radioExtentsHide.Checked = true;
            this.radioExtentsHide.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioExtentsHide.Location = new System.Drawing.Point(6, 19);
            this.radioExtentsHide.Name = "radioExtentsHide";
            this.radioExtentsHide.Size = new System.Drawing.Size(61, 17);
            this.radioExtentsHide.TabIndex = 0;
            this.radioExtentsHide.TabStop = true;
            this.radioExtentsHide.Text = "Hide All";
            this.radioExtentsHide.UseVisualStyleBackColor = true;
            this.radioExtentsHide.CheckedChanged += new System.EventHandler(this.radHideExtents_CheckedChanged);
            // 
            // radioExtentShowColl
            // 
            this.radioExtentShowColl.AutoSize = true;
            this.radioExtentShowColl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.radioExtentShowColl.Location = new System.Drawing.Point(6, 37);
            this.radioExtentShowColl.Name = "radioExtentShowColl";
            this.radioExtentShowColl.Size = new System.Drawing.Size(88, 17);
            this.radioExtentShowColl.TabIndex = 0;
            this.radioExtentShowColl.Text = "Only Colliding";
            this.radioExtentShowColl.UseVisualStyleBackColor = true;
            this.radioExtentShowColl.CheckedChanged += new System.EventHandler(this.radShowColliding_CheckedChanged);
            // 
            // objectGroup
            // 
            this.objectGroup.Controls.Add(this.SelectObjectBtn);
            this.objectGroup.Controls.Add(this.objectPreview);
            this.objectGroup.Controls.Add(this.select45Box);
            this.objectGroup.Controls.Add(this.Picker);
            this.objectGroup.Controls.Add(this.objectCategoriesBox);
            this.objectGroup.Controls.Add(this.label6);
            this.objectGroup.Controls.Add(this.cboObjCreate);
            this.objectGroup.Controls.Add(this.PlaceObjectBtn);
            this.objectGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.objectGroup.Location = new System.Drawing.Point(7, 7);
            this.objectGroup.Name = "objectGroup";
            this.objectGroup.Size = new System.Drawing.Size(215, 255);
            this.objectGroup.TabIndex = 24;
            this.objectGroup.TabStop = false;
            this.objectGroup.Text = " Objects";
            // 
            // SelectObjectBtn
            // 
            this.SelectObjectBtn.Appearance = System.Windows.Forms.Appearance.Button;
            this.SelectObjectBtn.BackColor = System.Drawing.Color.LightGray;
            this.SelectObjectBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SelectObjectBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.SelectObjectBtn.Location = new System.Drawing.Point(106, 221);
            this.SelectObjectBtn.Name = "SelectObjectBtn";
            this.SelectObjectBtn.Size = new System.Drawing.Size(95, 23);
            this.SelectObjectBtn.TabIndex = 33;
            this.SelectObjectBtn.TabStop = true;
            this.SelectObjectBtn.Text = "Select";
            this.SelectObjectBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.SelectObjectBtn, "Selecting Objects (Switch: ~)");
            this.SelectObjectBtn.UseVisualStyleBackColor = false;
            this.SelectObjectBtn.CheckedChanged += new System.EventHandler(this.ObjectModesChanged);
            // 
            // objectPreview
            // 
            this.objectPreview.BackColor = System.Drawing.Color.Black;
            this.objectPreview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.objectPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.objectPreview.Location = new System.Drawing.Point(40, 22);
            this.objectPreview.Name = "objectPreview";
            this.objectPreview.Size = new System.Drawing.Size(133, 133);
            this.objectPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.objectPreview.TabIndex = 25;
            this.objectPreview.TabStop = false;
            // 
            // select45Box
            // 
            this.select45Box.Appearance = System.Windows.Forms.Appearance.Button;
            this.select45Box.BackgroundImage = global::MapEditor.Properties.Resources._0deg;
            this.select45Box.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.select45Box.Location = new System.Drawing.Point(174, 52);
            this.select45Box.Name = "select45Box";
            this.select45Box.Size = new System.Drawing.Size(30, 30);
            this.select45Box.TabIndex = 31;
            this.toolTip1.SetToolTip(this.select45Box, "Rotates selection area 45 degrees (Ctrl+D)");
            this.select45Box.UseVisualStyleBackColor = true;
            this.select45Box.CheckedChanged += new System.EventHandler(this.select45Box_CheckedChanged);
            // 
            // Picker
            // 
            this.Picker.Appearance = System.Windows.Forms.Appearance.Button;
            this.Picker.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Picker.BackgroundImage")));
            this.Picker.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.Picker.Location = new System.Drawing.Point(174, 23);
            this.Picker.Name = "Picker";
            this.Picker.Size = new System.Drawing.Size(30, 30);
            this.Picker.TabIndex = 36;
            this.toolTip1.SetToolTip(this.Picker, "Item Picker (Ctrl+A)");
            this.Picker.UseVisualStyleBackColor = true;
            this.Picker.CheckedChanged += new System.EventHandler(this.Picker_CheckedChanged);
            // 
            // objectCategoriesBox
            // 
            this.objectCategoriesBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.objectCategoriesBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.objectCategoriesBox.FormattingEnabled = true;
            this.objectCategoriesBox.Location = new System.Drawing.Point(84, 192);
            this.objectCategoriesBox.Name = "objectCategoriesBox";
            this.objectCategoriesBox.Size = new System.Drawing.Size(104, 21);
            this.objectCategoriesBox.TabIndex = 30;
            this.objectCategoriesBox.SelectedIndexChanged += new System.EventHandler(this.ObjectCategoriesBoxSelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label6.Location = new System.Drawing.Point(28, 195);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 23);
            this.label6.TabIndex = 31;
            this.label6.Text = "Category:";
            // 
            // cboObjCreate
            // 
            this.cboObjCreate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.cboObjCreate.FormattingEnabled = true;
            this.cboObjCreate.Location = new System.Drawing.Point(29, 165);
            this.cboObjCreate.MaxDropDownItems = 15;
            this.cboObjCreate.Name = "cboObjCreate";
            this.cboObjCreate.Size = new System.Drawing.Size(160, 21);
            this.cboObjCreate.TabIndex = 23;
            this.cboObjCreate.SelectedIndexChanged += new System.EventHandler(this.CboObjCreateSelectedIndexChanged);
            // 
            // PlaceObjectBtn
            // 
            this.PlaceObjectBtn.Appearance = System.Windows.Forms.Appearance.Button;
            this.PlaceObjectBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.PlaceObjectBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.PlaceObjectBtn.Location = new System.Drawing.Point(12, 221);
            this.PlaceObjectBtn.Name = "PlaceObjectBtn";
            this.PlaceObjectBtn.Size = new System.Drawing.Size(95, 23);
            this.PlaceObjectBtn.TabIndex = 32;
            this.PlaceObjectBtn.TabStop = true;
            this.PlaceObjectBtn.Text = "Create";
            this.PlaceObjectBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.PlaceObjectBtn, "Placing Objects (Switch: ~)");
            this.PlaceObjectBtn.UseVisualStyleBackColor = true;
            this.PlaceObjectBtn.CheckedChanged += new System.EventHandler(this.ObjectModesChanged);
            // 
            // groupMapCopy
            // 
            this.groupMapCopy.Controls.Add(this.chkCopyObjects);
            this.groupMapCopy.Controls.Add(this.chkCopyWalls);
            this.groupMapCopy.Controls.Add(this.cmdPasteArea);
            this.groupMapCopy.Controls.Add(this.cmdCopyAll);
            this.groupMapCopy.Controls.Add(this.cmdCopyArea);
            this.groupMapCopy.Controls.Add(this.chkCopyTiles);
            this.groupMapCopy.Controls.Add(this.chkCopyWaypoints);
            this.groupMapCopy.Controls.Add(this.chkCopyPolygons);
            this.groupMapCopy.Location = new System.Drawing.Point(7, 503);
            this.groupMapCopy.Name = "groupMapCopy";
            this.groupMapCopy.Size = new System.Drawing.Size(215, 99);
            this.groupMapCopy.TabIndex = 38;
            this.groupMapCopy.TabStop = false;
            this.groupMapCopy.Text = "Map Copy";
            // 
            // chkCopyObjects
            // 
            this.chkCopyObjects.AutoSize = true;
            this.chkCopyObjects.Checked = true;
            this.chkCopyObjects.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCopyObjects.Location = new System.Drawing.Point(67, 49);
            this.chkCopyObjects.Name = "chkCopyObjects";
            this.chkCopyObjects.Size = new System.Drawing.Size(62, 17);
            this.chkCopyObjects.TabIndex = 40;
            this.chkCopyObjects.Text = "Objects";
            this.chkCopyObjects.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCopyObjects.UseVisualStyleBackColor = true;
            // 
            // chkCopyWalls
            // 
            this.chkCopyWalls.AutoSize = true;
            this.chkCopyWalls.Checked = true;
            this.chkCopyWalls.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCopyWalls.Location = new System.Drawing.Point(12, 49);
            this.chkCopyWalls.Name = "chkCopyWalls";
            this.chkCopyWalls.Size = new System.Drawing.Size(52, 17);
            this.chkCopyWalls.TabIndex = 38;
            this.chkCopyWalls.Text = "Walls";
            this.chkCopyWalls.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCopyWalls.UseVisualStyleBackColor = true;
            // 
            // cmdPasteArea
            // 
            this.cmdPasteArea.Appearance = System.Windows.Forms.Appearance.Button;
            this.cmdPasteArea.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdPasteArea.Location = new System.Drawing.Point(144, 20);
            this.cmdPasteArea.Name = "cmdPasteArea";
            this.cmdPasteArea.Size = new System.Drawing.Size(65, 23);
            this.cmdPasteArea.TabIndex = 37;
            this.cmdPasteArea.Text = "Paste";
            this.cmdPasteArea.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.cmdPasteArea, "Paste copied area anywhere on current/new/existing map");
            this.cmdPasteArea.UseVisualStyleBackColor = true;
            this.cmdPasteArea.CheckedChanged += new System.EventHandler(this.cmdPasteArea_CheckedChanged);
            // 
            // cmdCopyAll
            // 
            this.cmdCopyAll.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdCopyAll.Location = new System.Drawing.Point(78, 20);
            this.cmdCopyAll.Name = "cmdCopyAll";
            this.cmdCopyAll.Size = new System.Drawing.Size(67, 23);
            this.cmdCopyAll.TabIndex = 28;
            this.cmdCopyAll.Text = "Copy All";
            this.toolTip1.SetToolTip(this.cmdCopyAll, "Copy everything on the map");
            this.cmdCopyAll.UseVisualStyleBackColor = true;
            this.cmdCopyAll.Click += new System.EventHandler(this.cmdCopyAll_Click);
            // 
            // cmdCopyArea
            // 
            this.cmdCopyArea.Appearance = System.Windows.Forms.Appearance.Button;
            this.cmdCopyArea.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cmdCopyArea.Location = new System.Drawing.Point(10, 20);
            this.cmdCopyArea.Name = "cmdCopyArea";
            this.cmdCopyArea.Size = new System.Drawing.Size(69, 23);
            this.cmdCopyArea.TabIndex = 29;
            this.cmdCopyArea.Text = "Copy Area";
            this.cmdCopyArea.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.cmdCopyArea, "Select an area on the map to copy");
            this.cmdCopyArea.UseVisualStyleBackColor = true;
            this.cmdCopyArea.CheckedChanged += new System.EventHandler(this.cmdCopyArea_CheckedChanged);
            // 
            // chkCopyTiles
            // 
            this.chkCopyTiles.AutoSize = true;
            this.chkCopyTiles.Checked = true;
            this.chkCopyTiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCopyTiles.Location = new System.Drawing.Point(12, 66);
            this.chkCopyTiles.Name = "chkCopyTiles";
            this.chkCopyTiles.Size = new System.Drawing.Size(48, 17);
            this.chkCopyTiles.TabIndex = 39;
            this.chkCopyTiles.Text = "Tiles";
            this.chkCopyTiles.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCopyTiles.UseVisualStyleBackColor = true;
            // 
            // chkCopyWaypoints
            // 
            this.chkCopyWaypoints.AutoSize = true;
            this.chkCopyWaypoints.Checked = true;
            this.chkCopyWaypoints.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCopyWaypoints.Location = new System.Drawing.Point(67, 66);
            this.chkCopyWaypoints.Name = "chkCopyWaypoints";
            this.chkCopyWaypoints.Size = new System.Drawing.Size(76, 17);
            this.chkCopyWaypoints.TabIndex = 41;
            this.chkCopyWaypoints.Text = "Waypoints";
            this.chkCopyWaypoints.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCopyWaypoints.UseVisualStyleBackColor = true;
            // 
            // chkCopyPolygons
            // 
            this.chkCopyPolygons.AutoSize = true;
            this.chkCopyPolygons.Checked = true;
            this.chkCopyPolygons.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCopyPolygons.Location = new System.Drawing.Point(140, 49);
            this.chkCopyPolygons.Name = "chkCopyPolygons";
            this.chkCopyPolygons.Size = new System.Drawing.Size(69, 17);
            this.chkCopyPolygons.TabIndex = 42;
            this.chkCopyPolygons.Text = "Polygons";
            this.chkCopyPolygons.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCopyPolygons.UseVisualStyleBackColor = true;
            // 
            // cmdUndo
            // 
            this.cmdUndo.BackgroundImage = global::MapEditor.Properties.Resources.undoDisabled;
            this.cmdUndo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.cmdUndo.Enabled = false;
            this.cmdUndo.Location = new System.Drawing.Point(31, 8);
            this.cmdUndo.Name = "cmdUndo";
            this.cmdUndo.Size = new System.Drawing.Size(25, 25);
            this.cmdUndo.TabIndex = 29;
            this.toolTip1.SetToolTip(this.cmdUndo, "Undo (Ctrl + Z)");
            this.cmdUndo.UseVisualStyleBackColor = true;
            this.cmdUndo.EnabledChanged += new System.EventHandler(this.cmdUndo_EnabledChanged);
            this.cmdUndo.Click += new System.EventHandler(this.cmdUndo_Click);
            // 
            // lblMapStatus
            // 
            this.lblMapStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMapStatus.ForeColor = System.Drawing.Color.Green;
            this.lblMapStatus.Location = new System.Drawing.Point(128, 10);
            this.lblMapStatus.Name = "lblMapStatus";
            this.lblMapStatus.Size = new System.Drawing.Size(116, 20);
            this.lblMapStatus.TabIndex = 35;
            this.lblMapStatus.Text = "MAP STATUS";
            this.lblMapStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblMapStatus.Visible = false;
            // 
            // lstDebug
            // 
            this.lstDebug.FormattingEnabled = true;
            this.lstDebug.Location = new System.Drawing.Point(388, 549);
            this.lstDebug.Name = "lstDebug";
            this.lstDebug.Size = new System.Drawing.Size(201, 108);
            this.lstDebug.TabIndex = 0;
            this.lstDebug.Visible = false;
            // 
            // scrollPanel
            // 
            this.scrollPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scrollPanel.AutoScroll = true;
            this.scrollPanel.Controls.Add(this.mapPanel);
            this.scrollPanel.Location = new System.Drawing.Point(249, 0);
            this.scrollPanel.Name = "scrollPanel";
            this.scrollPanel.Size = new System.Drawing.Size(608, 678);
            this.scrollPanel.TabIndex = 6;
            this.scrollPanel.Scroll += new System.Windows.Forms.ScrollEventHandler(this.scrollPanel_Scroll);
            // 
            // mapPanel
            // 
            this.mapPanel.Controls.Add(this.lstDebug);
            this.mapPanel.Location = new System.Drawing.Point(-8, 0);
            this.mapPanel.Name = "mapPanel";
            this.mapPanel.Size = new System.Drawing.Size(5888, 5888);
            this.mapPanel.TabIndex = 0;
            this.mapPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.mapPanel_Paint);
            this.mapPanel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mapPanel_MouseDoubleClick);
            this.mapPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mapPanel_MouseDown);
            this.mapPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mapPanel_MouseMove);
            this.mapPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mapPanel_MouseUp);
            // 
            // contextMenu
            // 
            this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.contextMenuCopy,
            this.contextMenuPaste,
            this.contextMenuDelete,
            this.menuItem3,
            this.contextMenuProperties,
            this.contextcopyContent});
            this.contextMenu.Popup += new System.EventHandler(this.contextMenu_Popup);
            // 
            // contextMenuCopy
            // 
            this.contextMenuCopy.Index = 0;
            this.contextMenuCopy.Text = "Copy";
            this.contextMenuCopy.Click += new System.EventHandler(this.contextMenuCopy_Click);
            // 
            // contextMenuPaste
            // 
            this.contextMenuPaste.Index = 1;
            this.contextMenuPaste.Text = "Paste";
            this.contextMenuPaste.Click += new System.EventHandler(this.contextMenuPaste_Click);
            // 
            // contextcopyContent
            // 
            this.contextcopyContent.Index = 5;
            this.contextcopyContent.Text = "Copy Extents";
            this.contextcopyContent.Visible = false;
            this.contextcopyContent.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // tmrInvalidate
            // 
            this.tmrInvalidate.Enabled = true;
            this.tmrInvalidate.Tick += new System.EventHandler(this.tmrInvalidate_Tick);
            // 
            // toolTip1
            // 
            this.toolTip1.ShowAlways = true;
            // 
            // UndoTimer
            // 
            this.UndoTimer.Interval = 120;
            this.UndoTimer.Tick += new System.EventHandler(this.UndoTimer_Tick);
            // 
            // RedoTimer
            // 
            this.RedoTimer.Interval = 120;
            this.RedoTimer.Tick += new System.EventHandler(this.RedoTimer_Tick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(61, 4);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            this.contextMenuStrip.Opened += new System.EventHandler(this.contextMenu_Popup);
            this.contextMenuStrip.Click += new System.EventHandler(this.contextMenuStrip_Open);
            // 
            // tmrFade
            // 
            this.tmrFade.Interval = 7000;
            this.tmrFade.Tick += new System.EventHandler(this.tmrFade_Tick);
            // 
            // tmrFadeTicker
            // 
            this.tmrFadeTicker.Interval = 75;
            this.tmrFadeTicker.Tick += new System.EventHandler(this.tmrFadeTicker_Tick);
            // 
            // MapView
            // 
            this.Controls.Add(this.groupAdv);
            this.Controls.Add(this.scrollPanel);
            this.Controls.Add(this.statusBar);
            this.Name = "MapView";
            this.Size = new System.Drawing.Size(859, 704);
            ((System.ComponentModel.ISupportInitialize)(this.statusMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusLocation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusMapItem)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusPolygon)).EndInit();
            this.groupAdv.ResumeLayout(false);
            this.groupAdv.PerformLayout();
            this.tabMapTools.ResumeLayout(false);
            this.tabWalls.ResumeLayout(false);
            this.tabTiles.ResumeLayout(false);
            this.tabEdges.ResumeLayout(false);
            this.tabObjectWps.ResumeLayout(false);
            this.groupGridSnap.ResumeLayout(false);
            this.groupGridSnap.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.customSnapValue)).EndInit();
            this.waypointGroup.ResumeLayout(false);
            this.waypointGroup.PerformLayout();
            this.extentsGroup.ResumeLayout(false);
            this.extentsGroup.PerformLayout();
            this.objectGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.objectPreview)).EndInit();
            this.groupMapCopy.ResumeLayout(false);
            this.groupMapCopy.PerformLayout();
            this.scrollPanel.ResumeLayout(false);
            this.mapPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
    public static class ExtensionColor
    {
        public static Color Interpolate(this Color source, Color target, double percent)
        {
            var r = (byte)(source.R + (target.R - source.R) * percent);
            var g = (byte)(source.G + (target.G - source.G) * percent);
            var b = (byte)(source.B + (target.B - source.B) * percent);

            return Color.FromArgb(255, r, g, b);
        }
    }
    public static class ExtensionPointF
    {
        public static Point ToPoint(this PointF source)
        {
            return new Point((int)source.X, (int)source.Y);
        }
    }
}
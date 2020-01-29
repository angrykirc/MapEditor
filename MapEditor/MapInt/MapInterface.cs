/*
 * MapEditor
 * Пользователь: AngryKirC
 * Дата: 06.07.2015
 */

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using MapEditor.XferGui;
using MapEditor.newgui;
using MapEditor.mapgen;

using OpenNoxLibrary;
using OpenNoxLibrary.Util;
using OpenNoxLibrary.Compression;
using OpenNoxLibrary.Files;

namespace MapEditor.MapInt
{
    /// <summary>
    /// Wrapper providing complex Map-related operations
    /// </summary>
    public class MapInterface
    {
        // Singleton
        private static MapInterface _instance = new MapInterface();
        public static ArrayList RecSelected = new ArrayList();
        public static PointF SelectedPolyPoint;
        public static Rectangle selectedArea;
        public static Point[] selected45Area = new Point[4];

        protected NoxMap _Map;
        /// <summary>
        /// The map currently being edited.
        /// </summary>
        public static NoxMap TheMap
        {
            get
            {
                return _instance._Map;
            }
            set
            {
                _instance._Map = value;
            }
        }
        protected MapHelper _mapHelper;

        /// <summary>
        /// Enables auto-removal of already existing tiles/walls in placement mode.
        /// </summary>
        public static bool AllowPlaceOverride
        {
            get
            {
                return EditorSettings.Default.Edit_AllowOverride;
            }
        }

        

        


        private bool _ModeIsUpdated = false;

        /// <summary>
        /// Forces MapView to update current mode in statusbar.
        /// </summary>
        public static bool ModeIsUpdated
        {
            get
            {
                return _instance._ModeIsUpdated;
            }
            set
            {
                _instance._ModeIsUpdated = value;
            }
        }

        private 


        /// <summary>
        /// Instance of class that stores key states.
        /// </summary>
        public static KeyHelper KeyHelper
        {
            get
            {
                return _instance._keyHelper;
            }
        }

        private MapView _mapView = null;

        /// <summary>
        /// Reference to MapView class with editor tools. TODO: remove redundant calls to this
        /// </summary>
        private static MapView mapView
        {
            get
            {
                if (_instance._mapView == null)
                    _instance._mapView = MainWindow.Instance.mapView;

                return _instance._mapView;
            }
        }

        const string BLANK_MAP_NAME = "blankmap.map";

        public MapInterface()
        {
            //CurrentMode = Mode.OBJECT_SELECT;
            //history = new Queue<Operation>();
        }

        /// <summary>
        /// Attempts to load a map by its filename. Passing null string will result in loading a blank map
        /// </summary>
        public static void SwitchMap(string fileName)
        {
            mapView.done = false;
            Stream stream = null;

            // Check if requested loading a blank map
            if (fileName == null)
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                // Find BlankMap resource name.
                string name = null;
                foreach (string file in asm.GetManifestResourceNames())
                {
                    if (file.EndsWith(BLANK_MAP_NAME, StringComparison.InvariantCultureIgnoreCase))
                    {
                        name = file;
                        break;
                    }
                }
                // Open resource stream
                stream = asm.GetManifestResourceStream(name);

            }
            else
            {
                MainWindow.Instance.Cursor = Cursors.WaitCursor;
                //MainWindow.Instance.Invalidate();
                // Open filestream from name
                stream = new FileStream(fileName, FileMode.Open);
                // attempt to decompress .NXZ (if specified nxz file)
                if (Path.GetExtension(fileName).Equals(".nxz", StringComparison.InvariantCultureIgnoreCase))
                {
                    int length = 0; byte[] data = null;
                    using (var br = new BinaryReader(stream))
                    {
                        length = br.ReadInt32();
                        data = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
                    }
                    byte[] mapData = new byte[length];
                    NoxLz.Decompress(data, mapData);
                    stream = new MemoryStream(mapData);
                }
            }

            // load the map
            var map = new NoxMap();
            map.FileName = fileName;
            map.
            // create helper class
            _instance._mapHelper = new MapHelper(map);
            _instance._Map = map;
            // update mapinfo
            MainWindow.Instance.UpdateMapInfo();

            //// clear Undo slate
            mapView.TimeManager.Clear();
            mapView.currentStep = 0;
            mapView.cmdUndo.Enabled = false;
            mapView.cmdRedo.Enabled = false;
            MainWindow.Instance.miniUndo.Enabled = false;
            MainWindow.Instance.miniRedo.Enabled = false;

            SelectedWaypoint = null;
            SelectedWaypoints = new List<Map.Waypoint>();
            MainWindow.Instance.Cursor = Cursors.Default;
            if (mapView.PolygonEditDlg.Visible) mapView.PolygonEditDlg.Visible = false;
            MainWindow.Instance.Reload();
            if (fileName != null) MainWindow.Instance.StoreRecentItem(fileName);
        }

        
        


        public static int PolyPointSelect(Point pt)
        {
            double closestDistance = Double.MaxValue;
            int i = 0;
            int page = MainWindow.Instance.panelTabs.SelectedIndex;

            double selRadius = (page == 1 ? 1500 : selectRadius);
            foreach (Map.Polygon poly in TheMap.Polygons)
            {

                if (poly == MainWindow.Instance.mapView.PolygonEditDlg.SelectedPolygon)
                {

                    foreach (PointF points in poly.Points)
                    {
                        i++;
                        double distance = Math.Pow(pt.X - points.X, 2) + Math.Pow(pt.Y - points.Y, 2);

                        if (distance < selRadius && distance < closestDistance)
                        {

                            closestDistance = distance;
                            SelectedPolyPoint = points;
                            return i - 1;
                        }
                    }
                }
            }
            SelectedPolyPoint = new PointF();
            return 0;
        }
        public static PointF PolyPointSnap(Point pt)
        {
            double closestDistance = Double.MaxValue;
            int page = MainWindow.Instance.panelTabs.SelectedIndex;
            Map.Polygon SelectedPolygon = MainWindow.Instance.mapView.PolygonEditDlg.SelectedPolygon;
            double selRadius = (page == 1 ? 100 : 200);
            List<PointF> pointsMini = new List<PointF>();
            foreach (Map.Polygon poly in TheMap.Polygons)
            {

                if (poly != SelectedPolygon)
                {

                    foreach (PointF points in poly.Points)
                    {
                        PointF center2 = new Point();
                        PointF center = points;
                        if (page == 1)
                        {
                            int mapZoom = MainWindow.Instance.mapZoom;
                            int squareSize = MapView.squareSize;
                            float pointX = (points.X / squareSize) * mapZoom;
                            float pointY = (points.Y / squareSize) * mapZoom;
                            PointF SelectedPolyPointMini = new PointF((SelectedPolyPoint.X / squareSize) * mapZoom, (SelectedPolyPoint.Y / squareSize) * mapZoom);
                            center = new PointF(pointX, pointY);
                            pointsMini.Add(center);
                            if (center == SelectedPolyPointMini && Array.IndexOf(pointsMini.ToArray(), center) == -1) continue;
                            center2 = new PointF((center.X * 2) * squareSize / (mapZoom * 2), (center.Y * 2) * squareSize / (mapZoom * 2));
                        }
                        else if (points == SelectedPolyPoint && Array.IndexOf(SelectedPolygon.Points.ToArray(), points) == -1) continue;

                        double distance = Math.Pow(pt.X - center.X, 2) + Math.Pow(pt.Y - center.Y, 2);

                        if (distance < selRadius && distance < closestDistance)
                        {
                            closestDistance = distance;

                            return page == 1 ? center2 : center;
                        }
                    }
                }
            }

            return new Point();
        }

        /// <summary>
        /// Marks that latest operation has modified some tiles on the map
        /// </summary>
        public static bool OpUpdatedTiles = false;
        public static bool OpUpdatedWalls = false;
        public static bool OpUpdatedWaypoints = false;
        public static bool OpUpdatedPolygons = false;
        /// <summary>
        /// Marks that latest operation has modified some objects on the map
        /// </summary>
        public static bool OpUpdatedObjects = false;

        public static void ResetUpdateTracker()
        {
            OpUpdatedObjects = false;
            OpUpdatedTiles = false;
            OpUpdatedWalls = false;
            OpUpdatedWaypoints = false;
            OpUpdatedPolygons = false;
        }

        public static void HandleLMouseClick(Point pt)
        {
            Point wallPt = MapView.GetNearestWallPoint(pt);
            Point tilePt = MapView.GetNearestTilePoint(pt);
            Point pt2 = pt;
            pt2.Y += ((MainWindow.Instance.mapZoom * 2));
            pt2.X += 2;
            Point wallPt2 = MapView.GetNearestWallPoint(pt2);
            // Perform an action depending on current editing mode.
            switch (CurrentMode)
            {
                case EditMode.WALL_PLACE:
                    WallPlace(wallPt);
                    break;
                case EditMode.WALL_CHANGE:
                    WallChange(wallPt);
                    break;
                case EditMode.WALL_BRUSH:

                    if (!mapView.WallMakeNewCtrl.LineWall.Checked && !mapView.WallMakeNewCtrl.RecWall.Checked)
                        WallAutoBrush(wallPt, true);
                    break;
                case EditMode.FLOOR_PLACE:
                    FloorPlace(tilePt);
                    break;
                case EditMode.FLOOR_BRUSH:
                    FloorAutoBrush(tilePt);
                    break;
                case EditMode.EDGE_PLACE:
                    EdgePlace(tilePt);
                    break;
                case EditMode.OBJECT_PLACE:
                    ObjectPlace(mapView.cboObjCreate.Text, pt.X, pt.Y);
                    break;
                case EditMode.OBJECT_SELECT:
                    var obj = ObjectSelect(pt);
                    if (obj != null)
                    {
                        if (!SelectedObjects.Items.Contains(obj))
                        {
                            // clear selection if not multiselecting
                            if (!KeyHelper.ShiftKey) SelectedObjects.Items.Clear();
                            // put into selection
                            SelectedObjects.Items.Add(obj);
                        }
                        else if (KeyHelper.ShiftKey)
                            mapView.DeletefromSelected(obj);
                        SelectedObjects.Origin = obj;
                    }
                    else
                    {
                        if (!KeyHelper.ShiftKey)
                            SelectedObjects.Items.Clear();
                        SelectedObjects.Origin = null;
                    }
                    break;
                case EditMode.WAYPOINT_PLACE:
                    mapView.waypointName.Text = "";
                    WaypointPlace(mapView.waypointName.Text, new PointF(pt.X, pt.Y), mapView.waypointEnabled.Checked);

                    break;
                case EditMode.WAYPOINT_CONNECT:
                case EditMode.WAYPOINT_SELECT:
                    // Connect previously selected waypoint and one that is under cursor currently
                    if (CurrentMode == EditMode.WAYPOINT_CONNECT)
                    {
                        if (KeyHelper.ShiftKey) // Shift unconnects (reverse)
                            WaypointUnconnect(WaypointSelect(pt));
                        else
                            WaypointConnect(WaypointSelect(pt));
                    }
                    // Mark waypoint under cursor as selected, or reset
                    SelectedWaypoint = WaypointSelect(pt);
                    if (SelectedWaypoint != null)
                    {
                        // update info box
                        mapView.waypointName.Text = SelectedWaypoint.Name;
                        mapView.waypointEnabled.Checked = SelectedWaypoint.Flags > 0;
                    }
                    else
                        SelectedWaypoints.Clear();
                    break;
            }
        }
        public static void HandleRMouseClick(Point pt)
        {
            Point wallPt = MapView.GetNearestWallPoint(pt);
            Point tilePt = MapView.GetNearestTilePoint(pt);

            switch (CurrentMode)
            {

                case EditMode.WALL_CHANGE:
                    WallChange(wallPt, true);
                    break;
                case EditMode.FLOOR_BRUSH:
                    FloorRemove(tilePt);
                    break;
                case EditMode.WALL_BRUSH:
                    if (!mapView.WallMakeNewCtrl.LineWall.Checked && !mapView.WallMakeNewCtrl.RecWall.Checked)
                        WallRemove(wallPt);
                    break;
                case EditMode.FLOOR_PLACE:
                    FloorRemove(tilePt);
                    break;
                case EditMode.WALL_PLACE:
                    WallRemove(wallPt);
                    break;
                case EditMode.OBJECT_PLACE:
                    var obj = ObjectSelect(pt);
                    if (obj != null) ObjectRemove(obj);
                    break;
                case EditMode.WAYPOINT_PLACE:
                    var way = WaypointSelect(pt);
                    if (way != null) WaypointRemove(way);
                    break;
                case EditMode.EDGE_PLACE:
                    EdgeRemove(tilePt);
                    break;

            }
        }
    }
}
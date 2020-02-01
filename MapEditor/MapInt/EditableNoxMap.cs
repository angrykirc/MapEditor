using System;
using System.Collections.Generic;
using System.Text;

using OpenNoxLibrary.Util;
using OpenNoxLibrary.Files;

namespace MapEditor.MapInt
{
    class EditableNoxMap : NoxMap
    {
        public Action OnUpdatedWalls = delegate { };
        public Action OnUpdatedTiles = delegate { };
        public Action OnUpdatedObjects = delegate { };
        public Action OnUpdatedWaypoints = delegate { };

        KeyHelper _KeyHelper = new KeyHelper();

        /// <summary>
        /// Enables auto-removal of already existing tiles/walls in placement mode.
        /// </summary>
        protected static bool AllowPlaceOverride
        {
            get
            {
                return EditorSettings.Default.Edit_AllowOverride;
            }
        }

        #region Wall Operations
        public bool WallPlace(NoxMap.Wall newWall, bool smartDraw = false, bool silentFromBrush = false)
        {
            var pt = newWall.Location;
            if (pt.X <= 0 || pt.Y <= 0 || pt.X >= 255 || pt.Y >= 255) return false;

            NoxMap.Wall oldWall = null;
            bool remove = false;

            if (Walls.ContainsKey(pt))
            {
                oldWall = Walls[pt];
                if (!AllowPlaceOverride) return false;
                remove = true;
            }

            if (oldWall != null)
            {
                if (!smartDraw)
                {
                    if (oldWall.matId == newWall.matId && oldWall.Facing == newWall.Facing && oldWall.Variation == newWall.Variation)
                        return false;
                }
                else
                {
                    if ((oldWall.matId == newWall.matId && oldWall.Facing == newWall.Facing)) //|| mapView.LastWalls.Contains(oldWall))
                        return false;
                }
            }

            if (remove) WallRemove(pt);
            Walls.Add(pt, newWall);
            if (!silentFromBrush) OnUpdatedWalls();
            return true;
        }

        public bool WallRemove(int x, int y)
        {
            return WallRemove(new PointS32(x, y));
        }

        public bool WallRemove(PointS32 pt)
        {
            if (Walls.ContainsKey(pt))
            {
                Walls.Remove(pt);
                OnUpdatedWalls();
                return true;
            }
            return false;
        }

        public bool WallChange(PointS32 pt, int newFlags, bool removeProp = false)
        {
            if (Walls.ContainsKey(pt))
            {
                if (removeProp)
                {
                    if (!Walls[pt].Destructable
                        && !Walls[pt].Window
                        && Walls[pt].Secret_ScanFlags == 0
                        && Walls[pt].Secret_WallState == 0)
                        return false;

                    Walls[pt].Destructable = false;
                    Walls[pt].Window = false;
                    Walls[pt].Secret_ScanFlags = 0;
                    Walls[pt].Secret_WallState = 0;
                    OnUpdatedWalls();
                }
                else
                {
                    var wall = Walls[pt];

                    bool ok = true;
                    int flagsSelected = 0;
                    int flagsChecked = newFlags;/*mapView.WallMakeNewCtrl.openWallBox.Checked ? 4 : 0;
                    if (mapView.WallMakeNewCtrl.checkListFlags.GetItemChecked(0)) flagsSelected += 1;
                    if (mapView.WallMakeNewCtrl.checkListFlags.GetItemChecked(1)) flagsSelected += 2;
                    if (mapView.WallMakeNewCtrl.checkListFlags.GetItemChecked(2)) flagsSelected += 4;
                    if (mapView.WallMakeNewCtrl.checkListFlags.GetItemChecked(3)) flagsSelected += 8;*/

                    if (mapView.WallMakeNewCtrl.checkDestructable.Checked == wall.Destructable &&
                        mapView.WallMakeNewCtrl.checkWindow.Checked == wall.Window &&
                        mapView.WallMakeNewCtrl.polygonGroup.Value == wall.Minimap &&
                        mapView.WallMakeNewCtrl.numericCloseDelay.Value == wall.Secret_OpenWaitSeconds &&
                        flagsSelected == wall.Secret_ScanFlags &&
                        wall.Secret_WallState == flagsChecked) ok = false;

                    if (ok)
                    {
                        mapView.WallMakeNewCtrl.SetWall(Walls[pt], _KeyHelper.ShiftKey);

                        if (!_KeyHelper.ShiftKey)
                            OpUpdatedWalls = true;
                        return true;
                    }
                }
            }
            return false;
        }

        public Wall WallGet(PointS32 pt)
        {
            if (!Walls.ContainsKey(pt)) return null;
            return Walls[pt];
        }

        public static PointS32 WallSnap(PointS32 pt)
        {
            return new PointS32((pt.X / 23) * 23, (pt.Y / 23) * 23);
        }

        public void WallRectangle(PointS32 start, PointS32 end)
        {
            PointS32 MouseKeepOff = start.GetNearestWallPoint(true);
            PointS32 MousePoint = end.GetNearestWallPoint(true);
           
            if (!MousePoint.IsEmpty)
                MousePoint = start;
            else
            {
                if (MouseKeepOff.IsEmpty)
                    return;

                MousePoint = end;
            }

            var pt = new Point();
            pt.Rotate(start, MousePoint, -45);
            pt = pt.GetCenterPoint();

            Point a = MousePoint;
            Point b = new Point(pt.X, MousePoint.Y);
            Point c = pt;
            Point d = new Point(MousePoint.X, pt.Y);

            b = Rotate(b, a, 45);
            c = Rotate(c, a, 45);
            d = Rotate(d, a, 45);
            // moved to mapview
            //mapView.MapRenderer.FakeWalls.Clear();
            WallLine(a, true, b, false);
            WallLine(b, true, c, false);
            WallLine(c, true, d, false);
            WallLine(d, true, a);
        }

        public static void WallLine(Point pt, bool proxy = false, Point proxyDest = new Point(), bool dumb = true)
        {
            Point MouseKeep = MapView.GetNearestWallPoint(mapView.mouseKeep, true);
            Point MouseKeepOff = MapView.GetNearestWallPoint(mapView.mouseKeepOff, true);

            bool fake = true;
            Point mousePos;
            Point mouseDest = new Point();
            mousePos = pt;
            //mousePos = MapView.GetCenterPoint(pt);
            if (!MouseKeep.IsEmpty)
            {
                fake = true;
                //mouseDest = proxyDest.IsEmpty ? MouseKeep : MapView.GetNearestWallPoint(proxyDest, true);
                mouseDest = proxyDest.IsEmpty ? MouseKeep : proxyDest;
            }
            else
            {
                if (MouseKeepOff.IsEmpty)
                    return;

                //if (!proxy)
                // mapView.MapRenderer.FakeWalls.Clear();

                fake = false;
                //mouseDest = proxyDest.IsEmpty ? MouseKeepOff : MapView.GetNearestWallPoint(proxyDest, true);
                mouseDest = proxyDest.IsEmpty ? MouseKeepOff : proxyDest;
            }
            //mouseDest = MapView.GetNearestWallPoint(mouseDest, true);
            double dX = mouseDest.X - mousePos.X;
            double dY = mouseDest.Y - mousePos.Y;

            double multi = dX * dX + dY * dY;

            double distance = Math.Round(Math.Sqrt(multi) / 23);

            double rotationDirection = 180 - ((Math.Atan2(mousePos.X - mouseDest.X, mousePos.Y - mouseDest.Y)) * 180 / Math.PI);
            double vectorAngle = ((rotationDirection - 90) * Math.PI / 180);

            double xStep = (Math.Cos(vectorAngle)) * 23;
            double yStep = (Math.Sin(vectorAngle)) * 23;

            int xStepT = Convert.ToInt32(Math.Round(xStep));
            int yStepT = Convert.ToInt32(Math.Round(yStep));

            Point wallStep = new Point(mouseDest.X, mouseDest.Y);


            if (!proxy)
            {
                distance++;
                mapView.MapRenderer.FakeWalls.Clear();

            }
            for (int i = 0; i < distance; i++)
            {

                if (i > 0)
                {
                    wallStep.X += xStepT;
                    wallStep.Y += yStepT;
                }

                Point ptAligned1 = MapView.GetNearestWallPoint(wallStep);

                Map.Wall fakePiece = WallAutoBrush(ptAligned1, true, fake);

                if (fakePiece == null)
                    continue;

                if (!mapView.MapRenderer.FakeWalls.ContainsKey(fakePiece.Location) && fake)
                {
                    mapView.MapRenderer.FakeWalls.Add(fakePiece.Location, fakePiece);

                }
            }

            if (dumb)
            {
                mapView.mouseKeepOff = new Point();
                if (!fake && OpUpdatedWalls)
                    mapView.Store(CurrentMode, MapEditor.MapView.TimeEvent.POST);
            }

        }

        public static Map.Wall GetWallInList(Point pt)
        {
            foreach (var wall in mapView.MapRenderer.FakeWalls)
            {
                if (wall.Value.Location == pt)
                {
                    return wall.Value;
                }
            }
            return null;
        }
        public static bool GetLastWalls(Map.Wall wall)
        {
            foreach (Map.Wall thatwall in mapView.LastWalls)
            {
                if (thatwall == null) continue;

                if (thatwall.Equals(wall))
                {
                    return true;
                }
            }
            return false;
        }

        public static Map.Wall WallAutoBrush(Point pt, bool recur, bool fake = false, Point fix = new Point(), Point fxOrigin = new Point())
        {
            int maxWallList = 3;
            maxWallList = mapView.WallMakeNewCtrl.RecWall.Checked ? 300 : 3;


            if (mapView.WallMakeNewCtrl.smartDraw.Checked)
            {

                maxWallList = mapView.WallMakeNewCtrl.LineWall.Checked ? 6 : mapView.WallMakeNewCtrl.RecWall.Checked ? 300 : 3;
            }

            Map.Wall wallc;
            Map.Wall wall;
            Map.Wall OldWall = null;
            string OldWall2 = null;
            wall = WallGet(pt);

            if (TheMap.Walls.ContainsKey(pt))
            {
                OldWall = TheMap.Walls[pt];


                OldWall2 = OldWall.Facing.ToString();
            }
            //Map.Wall OldWall = null;

            // if(recur && fix.IsEmpty)
            //OldWall = WallGet(pt);


            ArrayList OldWall3 = new ArrayList();

            if (wall != null)
            {
                OldWall3.Add((byte)WallGet(pt).matId);
                OldWall3.Add((byte)WallGet(pt).Variation);
                OldWall3.Add((Map.Wall.WallFacing)WallGet(pt).Facing);
            }

            // if(OldWall != null)

            // MessageBox.Show(OldWall.Facing.ToString() + " " + OldWall.ToString());
            if (fake)
            {
                if (!fix.IsEmpty)
                {
                    Map.Wall wallfix = GetWallInList(pt);

                    var wmmf = GetWallInList(new Point(fxOrigin.X - 1, fxOrigin.Y - 1));
                    var wppf = GetWallInList(new Point(fxOrigin.X + 1, fxOrigin.Y + 1));
                    var wpmf = GetWallInList(new Point(fxOrigin.X + 1, fxOrigin.Y - 1));
                    var wmpf = GetWallInList(new Point(fxOrigin.X - 1, fxOrigin.Y + 1));

                    //&& wallfix.Facing != Map.Wall.WallFacing.SE_CORNER && wallfix.Facing != Map.Wall.WallFacing.SW_CORNER && wallfix.Facing != Map.Wall.WallFacing.NE_CORNER && wallfix.Facing != Map.Wall.WallFacing.NW_CORNER
                    if (wallfix != null && wmmf == null && wppf == null && wpmf == null && wmpf == null)
                    {
                        wall = mapView.WallMakeNewCtrl.NewWall(fix, true);
                        if (wall != null && !mapView.MapRenderer.FakeWalls.ContainsKey(wall.Location))
                            mapView.MapRenderer.FakeWalls.Add(wall.Location, wall);
                        pt = fix;
                        wall = wallfix;
                    }
                }
                else if (recur)
                {
                    wall = mapView.WallMakeNewCtrl.NewWall(pt, true);
                    if (wall != null && !mapView.MapRenderer.FakeWalls.ContainsKey(wall.Location))
                        mapView.MapRenderer.FakeWalls.Add(wall.Location, wall);

                }
                wall = GetWallInList(pt);
            }
            else
            {
                if (!fix.IsEmpty)
                {
                    Map.Wall wallfix = WallGet(pt);

                    var wmmf = WallGet(new Point(fxOrigin.X - 1, fxOrigin.Y - 1));
                    var wppf = WallGet(new Point(fxOrigin.X + 1, fxOrigin.Y + 1));
                    var wpmf = WallGet(new Point(fxOrigin.X + 1, fxOrigin.Y - 1));
                    var wmpf = WallGet(new Point(fxOrigin.X - 1, fxOrigin.Y + 1));

                    //&& wallfix.Facing != Map.Wall.WallFacing.SE_CORNER && wallfix.Facing != Map.Wall.WallFacing.SW_CORNER && wallfix.Facing != Map.Wall.WallFacing.NE_CORNER && wallfix.Facing != Map.Wall.WallFacing.NW_CORNER
                    if (wallfix != null && wmmf == null && wppf == null && wpmf == null && wmpf == null)
                    {
                        if (WallPlace(fix))
                        {
                            pt = fix;
                            wall = wallfix;
                            wallc = WallGet(fix);
                            if (!mapView.LastWalls.Contains(wallc))
                                mapView.LastWalls.Add(wallc);//mapView.LastWalls.Insert(0, wallc);
                            if (mapView.LastWalls.Count > maxWallList + 5) mapView.LastWalls.RemoveAt(0);
                        }
                    }
                }
                else if (recur)
                {

                    wallc = WallGet(pt);

                    if (WallPlace(pt, true))
                    {
                        wallc = WallGet(pt);

                    }
                    if (wallc != null)
                    {
                        if (!mapView.LastWalls.Contains(wallc))
                            mapView.LastWalls.Add(wallc);
                        if (mapView.LastWalls.Count > maxWallList) mapView.LastWalls.RemoveAt(0);
                    }
                }
                wall = WallGet(pt);
            }
            Map.Wall wmm;
            Map.Wall wpp;
            Map.Wall wpm;
            Map.Wall wmp;

            if (wall == null || (!GetLastWalls(wall) && mapView.WallMakeNewCtrl.smartDraw.Checked && !fake))
            {

                return wall;

            }
            if (!fake)
            {

                wmm = WallGet(new Point(pt.X - 1, pt.Y - 1));
                wpp = WallGet(new Point(pt.X + 1, pt.Y + 1));
                wpm = WallGet(new Point(pt.X + 1, pt.Y - 1));
                wmp = WallGet(new Point(pt.X - 1, pt.Y + 1));
                if (mapView.WallMakeNewCtrl.smartDraw.Checked)
                {

                    if (wmm != null)
                    {
                        if (!GetLastWalls(wmm)) wmm = null;
                    }
                    if (wpp != null)
                    {
                        if (!GetLastWalls(wpp)) wpp = null;
                    }
                    if (wpm != null)
                    {
                        if (!GetLastWalls(wpm)) wpm = null;
                    }

                    if (wmp != null)
                    {
                        if (!GetLastWalls(wmp)) wmp = null;
                    }
                }
            }
            else
            {
                Map.Wall wallin;
                Point point = new Point(pt.X - 1, pt.Y - 1);
                wmm = mapView.MapRenderer.FakeWalls.TryGetValue(point, out wallin) ? wallin : null;

                point = new Point(pt.X + 1, pt.Y + 1);
                wpp = mapView.MapRenderer.FakeWalls.TryGetValue(point, out wallin) ? wallin : null;

                point = new Point(pt.X + 1, pt.Y - 1);
                wpm = mapView.MapRenderer.FakeWalls.TryGetValue(point, out wallin) ? wallin : null;

                point = new Point(pt.X - 1, pt.Y + 1);
                wmp = mapView.MapRenderer.FakeWalls.TryGetValue(point, out wallin) ? wallin : null;
            }

            if (recur)
            {
                if (fix.IsEmpty)
                {
                    WallAutoBrush(new Point(pt.X + 0, pt.Y + 2), true, fake, new Point(pt.X - 1, pt.Y + 1), pt);
                    WallAutoBrush(new Point(pt.X + 0, pt.Y - 2), true, fake, new Point(pt.X + 1, pt.Y - 1), pt);
                    WallAutoBrush(new Point(pt.X - 2, pt.Y + 0), true, fake, new Point(pt.X - 1, pt.Y + 1), pt);
                    WallAutoBrush(new Point(pt.X + 2, pt.Y + 0), true, fake, new Point(pt.X + 1, pt.Y - 1), pt);
                }
                WallAutoBrush(new Point(pt.X - 1, pt.Y - 1), false, fake);
                WallAutoBrush(new Point(pt.X + 1, pt.Y + 1), false, fake);
                WallAutoBrush(new Point(pt.X + 1, pt.Y - 1), false, fake);
                WallAutoBrush(new Point(pt.X - 1, pt.Y + 1), false, fake);
            }

            int seed = Environment.TickCount & Int32.MaxValue;
            Random rnd = new Random((wall.Location.X + wall.Location.Y + (int)DateTime.Now.Ticks + seed));
            int wall_Variation = (byte)rnd.Next((int)mapView.WallMakeNewCtrl.numWallVari.Value, (int)mapView.WallMakeNewCtrl.numWallVariMax.Value + 1);

            if (wmm != null && wpm != null && wmp != null && wpp != null)
            {
                wall.Facing = Map.Wall.WallFacing.CROSS;
                wall.Variation = 0;

                if (OldWall == null)
                {
                    OpUpdatedWalls = true;
                    return wall;//false
                }
                else if (wall.Facing != (Map.Wall.WallFacing)OldWall3[2] || wall.matId != (byte)OldWall3[0] || (byte)OldWall3[1] != wall.Variation) OpUpdatedWalls = true;
                return wall;//false
            }
            else if (wmm != null && wpm != null && wmp == null)
            {
                if (wpp == null)
                {
                    wall.Facing = Map.Wall.WallFacing.SW_CORNER;
                    wall.Variation = 0;
                }
                else
                {
                    wall.Facing = Map.Wall.WallFacing.EAST_T;

                    wall.Variation = 0;
                }
                if (OldWall == null)
                {
                    OpUpdatedWalls = true;
                    return wall;//false
                }
                else if (wall.Facing != (Map.Wall.WallFacing)OldWall3[2] || wall.matId != (byte)OldWall3[0] || (byte)OldWall3[1] != wall.Variation) OpUpdatedWalls = true;
                return wall;//false
            }
            else if (wpp != null && wpm != null && wmm == null)
            {
                if (wmp == null)
                {
                    wall.Facing = Map.Wall.WallFacing.NW_CORNER;
                    wall.Variation = 0;
                }
                else
                {
                    wall.Facing = Map.Wall.WallFacing.NORTH_T;
                    wall.Variation = 0;
                }
                if (OldWall == null)
                {
                    OpUpdatedWalls = true;
                    return wall;//false
                }
                else if (wall.Facing != (Map.Wall.WallFacing)OldWall3[2] || wall.matId != (byte)OldWall3[0] || (byte)OldWall3[1] != wall.Variation) OpUpdatedWalls = true;
                return wall;//false
            }
            else if (wpp != null && wmp != null)
            {
                if (wmm == null)
                {
                    wall.Facing = Map.Wall.WallFacing.NE_CORNER;
                    wall.Variation = 0;
                }
                else
                {
                    wall.Facing = Map.Wall.WallFacing.WEST_T;
                    wall.Variation = 0;
                }
                if (OldWall == null)
                {
                    OpUpdatedWalls = true;
                    return wall;//false
                }
                else if (wall.Facing != (Map.Wall.WallFacing)OldWall3[2] || wall.matId != (byte)OldWall3[0] || (byte)OldWall3[1] != wall.Variation) OpUpdatedWalls = true;
                return wall;//false
            }
            else if (wmm != null && wmp != null)
            {
                if (wpm == null)
                {
                    wall.Facing = Map.Wall.WallFacing.SE_CORNER;
                    wall.Variation = 0;
                }
                else
                {
                    wall.Facing = Map.Wall.WallFacing.SOUTH_T;
                    wall.Variation = 0;
                }
                if (OldWall == null)
                {
                    OpUpdatedWalls = true;
                    return wall;//false
                }
                else if (wall.Facing != (Map.Wall.WallFacing)OldWall3[2] || wall.matId != (byte)OldWall3[0] || (byte)OldWall3[1] != wall.Variation) OpUpdatedWalls = true;
                return wall;//false
            }

            // Normal
            // if (OldWall != null)
            // MessageBox.Show("PO RECUR "+OldWall.Facing.ToString() + " " + OldWall.ToString());


            if (wmp != null || wmp != null)
            {

                if (wall != null && mapView.WallMakeNewCtrl.smartDraw.Checked || (!recur && OldWall != null))
                {
                    if (wall.Facing == Map.Wall.WallFacing.NE_CORNER ||
                        wall.Facing == Map.Wall.WallFacing.NW_CORNER ||
                        wall.Facing == Map.Wall.WallFacing.SW_CORNER ||
                        wall.Facing == Map.Wall.WallFacing.SE_CORNER)
                    {
                        if (OldWall == null)
                        {
                            OpUpdatedWalls = true;
                            return wall;//false
                        }
                        else if (wall.Facing != OldWall.Facing || wall.Material != OldWall.Material) OpUpdatedWalls = true;
                        return wall;//false
                    }
                }

                //if (OldWall != null)
                // MessageBox.Show("PO RECUR1 " + OldWall.Facing.ToString() + " " + OldWall.ToString());


                wall.Facing = Map.Wall.WallFacing.NORTH;

                // if (OldWall != null)
                //  MessageBox.Show("PO RECUR2 " + OldWall.Facing.ToString() + " " + OldWall.ToString());

                // if (OldWall2 != null)
                // MessageBox.Show("PO RECUR :WALL2:: " + OldWall2);

                //  if (OldWall != null)
                //MessageBox.Show("PO RECUR :WALL3333:: " + OldWall3[3].ToString());

                if (mapView.WallMakeNewCtrl.wallFacing > 1 && wall.Variation < 1 && !wall.Window && mapView.WallMakeNewCtrl.autovari.Checked && !mapView.WallMakeNewCtrl.started)
                    wall.Variation = (byte)wall_Variation;

                // if (OldWall != null)
                // MessageBox.Show(OldWall.Facing.ToString() + " " + OldWall.ToString() + " " + wall.Facing.ToString());



                if (OldWall == null)
                {
                    OpUpdatedWalls = true;
                    return wall;//false
                }
                else if (wall.Facing != (Map.Wall.WallFacing)OldWall3[2] || wall.matId != (byte)OldWall3[0] || (byte)OldWall3[1] != wall.Variation) OpUpdatedWalls = true;
                return wall;//false
            }
            if (wmm != null || wpp != null)
            {
                if (wall != null && mapView.WallMakeNewCtrl.smartDraw.Checked || (!recur && OldWall != null))
                {
                    if (wall.Facing == Map.Wall.WallFacing.NE_CORNER ||
                        wall.Facing == Map.Wall.WallFacing.NW_CORNER ||
                        wall.Facing == Map.Wall.WallFacing.SW_CORNER ||
                        wall.Facing == Map.Wall.WallFacing.SE_CORNER)
                    {

                        if (OldWall == null)
                        {
                            OpUpdatedWalls = true;
                            return wall;//false
                        }
                        else if (wall.Facing != OldWall.Facing || wall.Material != OldWall.Material) OpUpdatedWalls = true;
                        return wall;//false
                    }
                }
                wall.Facing = Map.Wall.WallFacing.WEST;
                if (mapView.WallMakeNewCtrl.wallFacing > 1 && wall.Variation < 1 && !wall.Window && mapView.WallMakeNewCtrl.autovari.Checked && !mapView.WallMakeNewCtrl.started)
                    wall.Variation = (byte)wall_Variation;


                // if (OldWall != null)
                // MessageBox.Show(OldWall.Facing.ToString() + " " + OldWall.ToString() + " " + wall.Facing.ToString());
                if (OldWall == null)
                {
                    OpUpdatedWalls = true;
                    return wall;//false
                }
                else if (wall.Facing != (Map.Wall.WallFacing)OldWall3[2] || wall.matId != (byte)OldWall3[0] || (byte)OldWall3[1] != wall.Variation) OpUpdatedWalls = true;
                return wall;//false
            }
            if (OldWall == null)
            {
                OpUpdatedWalls = true;
                return wall;//false
            }
            else if (wall.Facing != (Map.Wall.WallFacing)OldWall3[2] || wall.matId != (byte)OldWall3[0] || (byte)OldWall3[1] != wall.Variation) OpUpdatedWalls = true;
            return wall;//false
        }
        #endregion


        #region Floor Operations
        public bool ContainsTile(Point tilePoint)
        {
            int panelVisibleH = mapView.scrollPanel.Height;
            int panelVisibleW = mapView.scrollPanel.Width;
            Rectangle visibleArea = new Rectangle(-mapView.mapPanel.Location.X - 48, -mapView.mapPanel.Location.Y - 48, panelVisibleW + 48, panelVisibleH + 48);
            foreach (Point tila in TheMap.Tiles.Keys)
            {
                if (!visibleArea.Contains(tila.X, tila.Y)) continue;


                if (tilePoint.Equals(tila))
                    return true;

            }
            return false;
        }
        public static bool FloorPlace(int x, int y)
        {
            return FloorPlace(new Point(x, y));
        }
        public static bool FloorPlace(Point pt)
        {
            bool added = false;
            if (pt.X < 0 || pt.Y < 0 || pt.X > 252 || pt.Y > 252) return false;
            if (mapView.TileMakeNewCtrl.BrushSize.Value >= 2)
            {
                Point tilePt = new Point();
                tilePt.X = pt.X;
                tilePt.Y = pt.Y;
                Point pat = new Point();
                int i = 0;

                int cols = (int)mapView.TileMakeNewCtrl.BrushSize.Value;
                int rows = cols;//(int)mapView.TileMakeNewCtrl.BrushSize.Value;
                for (pat = tilePt; i < rows; i++, pat.X--, pat.Y++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        Point pat2 = new Point();
                        pat2 = pat;
                        pat2.X += j * 1;
                        pat2.Y += j * 1;
                        pat2.Y -= ((cols - 1) + ((cols % 2 == 0) ? 1 : 0));
                        //pat2.Y -= 2;
                        //(re)place tile + auto edge + auto vari

                        if (pat2.X < 1 || pat2.Y < 1 || pat2.X > 251 || pat2.Y > 251) continue;
                        Map.Tile newTile = mapView.TileMakeNewCtrl.GetTile(pat2);
                        if (TheMap.Tiles.ContainsKey(pat2))
                        {
                            if (!AllowPlaceOverride) continue;
                            if (TheMap.Tiles[pat2].Variation == newTile.Variation && TheMap.Tiles[pat2].graphicId == newTile.graphicId && TheMap.Tiles[pat2].EdgeTiles.Count == 0) continue;
                            TheMap.Tiles.Remove(pat2);
                        }
                        TheMap.Tiles.Add(pat2, newTile);
                        added = true;
                    }
                }
                if (added)
                {
                    OpUpdatedTiles = true;
                    return true;
                }
            }
            else
            {
                Map.Tile newTile = mapView.TileMakeNewCtrl.GetTile(pt);
                if (TheMap.Tiles.ContainsKey(pt))
                {
                    if (!AllowPlaceOverride) return false;
                    if (TheMap.Tiles[pt].Variation == newTile.Variation && TheMap.Tiles[pt].graphicId == newTile.graphicId && TheMap.Tiles[pt].EdgeTiles.Count == 0) return false;
                    TheMap.Tiles.Remove(pt);
                }

                TheMap.Tiles.Add(pt, mapView.TileMakeNewCtrl.GetTile(pt));
                OpUpdatedTiles = true;
                return true;
            }

            return false;
        }
        public static bool FloorRemove(int x, int y)
        {
            return FloorRemove(new Point(x, y));
        }
        public static bool FloorRemove(Point pt)
        {
            bool removed = false;
            if (CurrentMode == EditMode.FLOOR_BRUSH || CurrentMode == EditMode.FLOOR_PLACE)
            {
                // Remove multiple tiles if Auto Brush is enabled
                if (mapView.TileMakeNewCtrl.BrushSize.Value >= 2)
                {
                    Point pat = new Point();
                    int i = 0;

                    int cols = (int)mapView.TileMakeNewCtrl.BrushSize.Value;
                    int rows = (int)mapView.TileMakeNewCtrl.BrushSize.Value;
                    for (pat = pt; i < rows; i++, pat.X--, pat.Y++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            Point pat2 = new Point();
                            pat2 = pat;
                            pat2.X += j * 1;
                            pat2.Y += j * 1;
                            pat2.Y -= ((cols - 1) + ((cols % 2 == 0) ? 1 : 0));
                            //pat2.Y -= 2;

                            if (TheMap.Tiles.ContainsKey(pat2))
                            {
                                TheMap.Tiles[pat2].EdgeTiles.Clear();
                                TheMap.Tiles.Remove(pat2);
                                removed = true;
                            }
                        }
                    }
                    if (removed)
                    {
                        OpUpdatedTiles = true;
                        return true;
                    }
                }
            }
            // Remove singular tile
            if (TheMap.Tiles.ContainsKey(pt))
            {
                TheMap.Tiles[pt].EdgeTiles.Clear();
                TheMap.Tiles.Remove(pt);
                OpUpdatedTiles = true;
                return true;
            }
            return false;
        }
        public static bool FloorAutoBrush(int x, int y)
        {
            var edge = mapView.EdgeMakeNewCtrl.GetEdge();
            // TODO: move stuff from MapHelper to this class
            _instance._mapHelper.SetTileMaterial(ThingDb.FloorTileNames[edge.Graphic]);
            _instance._mapHelper.SetEdgeMaterial(ThingDb.EdgeTileNames[edge.Edge]);

            if (mapView.TileMakeNewCtrl.BrushSize.Value >= 2)
            {
                Point tilePt = new Point();
                tilePt.X = x;
                tilePt.Y = y;
                Point pat = new Point();
                int i = 0;

                int cols = (int)mapView.TileMakeNewCtrl.BrushSize.Value;
                int rows = (int)mapView.TileMakeNewCtrl.BrushSize.Value;
                for (pat = tilePt; i < rows; i++, pat.X--, pat.Y++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        Point pat2 = new Point();
                        pat2 = pat;
                        pat2.X += j * 1;
                        pat2.Y += j * 1;
                        pat2.Y -= ((cols - 1) + ((cols % 2 == 0) ? 1 : 0));
                        // pat2.Y -= 2;
                        //(re)place tile + auto edge + auto vari
                        if (!(pat2.X < 0 || pat2.Y < 0 || pat2.X > 252 || pat2.Y > 252))
                        {
                            // _instance._mapHelper.RemoveTile(pat2.X, pat2.Y);
                            _instance._mapHelper.PlaceTile(pat2.X, pat2.Y);
                            _instance._mapHelper.BrushAutoBlend(pat2.X, pat2.Y);
                        }


                    }
                }
            }
            else
            {
                //_instance._mapHelper.RemoveTile(x, y);
                _instance._mapHelper.PlaceTile(x, y);
                _instance._mapHelper.BrushAutoBlend(x, y);
            }
            //  OpUpdatedTiles = true;
            return true;
        }
        public static bool FloorAutoBrush(Point pt)
        {
            return FloorAutoBrush(pt.X, pt.Y);
        }
        #endregion

        #region Edge Operations
        public static bool EdgePlace(int x, int y)
        {
            return EdgePlace(new Point(x, y));
        }
        public static bool EdgePlace(Point pt)
        {
            if (TheMap.Tiles.ContainsKey(pt))
            {
                Map.Tile.EdgeTile edge = mapView.EdgeMakeNewCtrl.GetEdge();
                if (MainWindow.Instance.mapView.EdgeMakeNewCtrl.chkAutoEdge.Checked)
                {
                    _instance._mapHelper.SetTileMaterial(ThingDb.FloorTileNames[edge.Graphic]);
                    _instance._mapHelper.SetEdgeMaterial(ThingDb.EdgeTileNames[edge.Edge]);
                    _instance._mapHelper.AutoEdge(pt);
                }
                else
                {
                    Map.Tile tile = TheMap.Tiles[pt];

                    foreach (Map.Tile.EdgeTile ex in tile.EdgeTiles)
                    {
                        // don't create edges with equal EdgeType, Direction and CoverTile
                        if (ex.Edge == edge.Edge && ex.Dir == edge.Dir && ex.Graphic == edge.Graphic) return false;
                    }

                    tile.EdgeTiles.Add(edge);
                }

                OpUpdatedTiles = true;
                return true;
            }
            return false;
        }
        public static bool EdgeRemove(int x, int y)
        {
            return EdgeRemove(new Point(x, y));
        }
        public static bool EdgeRemove(Point pt)
        {
            if (TheMap.Tiles.ContainsKey(pt))
            {

                Map.Tile tile = TheMap.Tiles[pt];
                if (tile.EdgeTiles.Count <= 0) return false;
                byte edgeTypeID = mapView.EdgeMakeNewCtrl.GetEdge().Edge;
                // filter edges with specific type (selected)
                ArrayList newlist = new ArrayList();
                if (!KeyHelper.ShiftKey)
                {

                    int i = tile.EdgeTiles.Count - 1;
                    foreach (Map.Tile.EdgeTile edge in tile.EdgeTiles)
                    {
                        if (i > 0) newlist.Add(edge);
                        i--;
                    }
                    tile.EdgeTiles = newlist;
                }
                else
                {
                    foreach (Map.Tile.EdgeTile edge in tile.EdgeTiles)
                    {
                        if (edge.Edge != edgeTypeID) newlist.Add(edge);
                    }
                    tile.EdgeTiles = newlist;
                }

                OpUpdatedTiles = true;
                return true;
            }
            return false;
        }
        #endregion

        #region Object Operations
        private MapObjectCollection _SelectedObjects = new MapObjectCollection();
        public static MapObjectCollection SelectedObjects
        {
            get { return _instance._SelectedObjects; }
        }

        public static Map.Object ObjectSelect(Point pt)
        {
            double closestDistance = Double.MaxValue;
            Map.Object closest = null;
            int panelVisibleH = mapView.scrollPanel.Height;
            int panelVisibleW = mapView.scrollPanel.Width;
            Rectangle visibleArea = new Rectangle(-mapView.mapPanel.Location.X - 48, -mapView.mapPanel.Location.Y - 48, panelVisibleW + 48, panelVisibleH + 48);

            foreach (Map.Object obj in TheMap.Objects)
            {

                int x = (int)obj.Location.X;
                int y = (int)obj.Location.Y;

                if (!visibleArea.Contains(x, y)) continue;

                double distance = Math.Pow(pt.X - obj.Location.X, 2) + Math.Pow(pt.Y - obj.Location.Y, 2);
                ThingDb.Thing tt = ThingDb.Things[obj.Name];
                PointF center = obj.Location;
                int radius = 0;
                bool hitTest = false;
                int Zsize = tt.ZSizeY;
                int ExtentX = tt.ExtentX;
                int ExtentY = tt.ExtentY;
                if (!EditorSettings.Default.Edit_PreviewMode && tt.DrawType != "TriggerDraw" && tt.DrawType != "PressurePlateDraw")
                {
                    Point topLeft = new Point((int)center.X - 8, (int)center.Y - 8);
                    Rectangle smallRec = new Rectangle(topLeft, new Size(2 * 8, 2 * 8));//55


                    if (mapView.Get45RecSize() >= 5)
                    {

                        if (PointInPolygon(new Point((int)center.X, (int)center.Y), selected45Area) && !RecSelected.Contains(obj))
                            RecSelected.Add(obj);
                        else
                        {
                            for (int i = 0; i <= 2; i++)
                            {
                                if (LineIntersectsRect(selected45Area[i], selected45Area[i + 1], smallRec))
                                {
                                    if (!RecSelected.Contains(obj))
                                    {
                                        RecSelected.Add(obj);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        hitTest = smallRec.Contains(pt);
                    }
                }
                else
                {
                    if (tt.ExtentType == "CIRCLE")
                    {
                        PointF t = new PointF(center.X - ExtentX, center.Y - tt.ExtentX);
                        PointF p = new PointF((center.X) - ExtentX, (center.Y - Zsize) - ExtentX);
                        radius = tt.DrawType == "DoorDraw" ? (int)selectRadius * 3 : (ExtentX * ExtentX);
                        Rectangle r1 = new Rectangle((int)t.X, (int)t.Y - (Zsize * 1), ExtentX * 2, Zsize + ExtentX);
                        //hitTest = tt.ExtentX <= 12 ? r1.Contains(pt) : false;


                        if (mapView.Get45RecSize() >= 5)
                        {
                            if (PointInPolygon(new Point((int)center.X, (int)center.Y), selected45Area) && !RecSelected.Contains(obj))
                                RecSelected.Add(obj);
                            else
                            {
                                for (int i = 0; i <= 2; i++)
                                {
                                    if (LineIntersectsRect(selected45Area[i], selected45Area[i + 1], r1))
                                    {
                                        if (!RecSelected.Contains(obj))
                                        {
                                            RecSelected.Add(obj);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            hitTest = tt.ExtentX <= 15 ? r1.Contains(pt) : tt.ZSizeY >= 25 ? r1.Contains(pt) : false;

                            if (tt.ExtentX <= 15)
                                hitTest = r1.Contains(pt);
                            else if (tt.ZSizeY >= 20 && tt.ExtentX <= 30)
                                hitTest = r1.Contains(pt);
                            else
                                hitTest = false;

                        }
                    }
                    else
                    {
                        /////////////////////////////////////////////////////////////úúú
                        Point t = new Point((int)(center.X - (tt.ExtentX / 2)), (int)(center.Y - (tt.ExtentY / 2)));
                        Point p = new Point((int)((center.X - (Zsize / 2)) - (tt.ExtentX / 2)), (int)((center.Y - (Zsize / 2)) - (tt.ExtentY / 2)));
                        if (tt.DrawType == "TriggerDraw" || tt.DrawType == "PressurePlateDraw")
                        {
                            if (tt.HasClassFlag(ThingDb.Thing.ClassFlags.TRIGGER))
                            {

                                NoxShared.ObjDataXfer.TriggerXfer trigger = obj.GetExtraData<NoxShared.ObjDataXfer.TriggerXfer>();

                                t = new Point((int)(center.X - (trigger.SizeX / 2)), (int)(center.Y - (trigger.SizeY / 2)));
                                p = new Point((int)((center.X - (Zsize / 2)) - (trigger.SizeX / 2)), (int)((center.Y - (Zsize / 2)) - (trigger.SizeY / 2)));
                                ExtentY = trigger.SizeY;
                                ExtentX = trigger.SizeX;
                            }
                        }

                        Point[] pointss = new Point[6];
                        Point point1 = new Point(t.X, t.Y);
                        Point point2 = new Point(p.X, p.Y);
                        pointss[0] = Rotate(point2, center, 45);
                        point1 = new Point(t.X, t.Y);
                        point2 = new Point(p.X, p.Y);
                        point1.Y += ExtentY;
                        point2.Y += ExtentY;

                        pointss[1] = Rotate(point2, center, 45);
                        pointss[2] = Rotate(point1, center, 45);

                        point1 = new Point(t.X, t.Y);
                        point2 = new Point(p.X, p.Y);
                        point1.X += ExtentX;
                        point2.X += ExtentX;

                        pointss[4] = Rotate(point1, center, 45);
                        pointss[5] = Rotate(point2, center, 45);

                        point1 = new Point(t.X, t.Y);
                        point2 = new Point(p.X, p.Y);
                        point1.X += ExtentX;
                        point2.X += ExtentX;

                        point1.Y += ExtentY;
                        point2.Y += ExtentY;

                        pointss[3] = Rotate(point1, center, 45);

                        if (mapView.Get45RecSize() >= 5)
                        {

                            if (PointInPolygon(new Point((int)center.X, (int)center.Y), selected45Area) && !RecSelected.Contains(obj))
                                RecSelected.Add(obj);
                            else
                            {
                                for (int i = 0; i <= 3; i++)
                                {
                                    if (LineIntersectsPolygon(pointss[i], pointss[i + 1], selected45Area) && !RecSelected.Contains(obj))
                                    {
                                        RecSelected.Add(obj);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                            hitTest = PointInPolygon(pt, pointss);
                    }
                }
                /////////////////////////////////////////////////////////////////

                if ((distance < selectRadius || (distance < radius) || hitTest) && distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = obj;
                }
            }

            return closest;
        }
        public static Map.Object ObjectPlace(string type, float x, float y)
        {
            OpUpdatedObjects = true;
            return ObjectPlace(type, new PointF(x, y));
        }
        public static Map.Object ObjectPlace(string type, PointF loc)
        {
            if (!ThingDb.Things.ContainsKey(type)) return null;

            Map.Object result = new Map.Object();
            result.Name = type;
            result.Location = loc;
            result.Extent = GetNextObjectExtent();

            // смотрим нету ли редактора, устанавливаем стандартные значения
            XferEditor editor = XferEditors.GetEditorForXfer(ThingDb.Things[type].Xfer);
            if (editor != null) editor.SetDefaultData(result);
            else result.NewDefaultExtraData();


            if (ThingDb.Things[type].Xfer == "DoorXfer")
            {
                int dorDir = (int)mapView.delta;
                NoxShared.ObjDataXfer.DoorXfer door = result.GetExtraData<NoxShared.ObjDataXfer.DoorXfer>();
                door.Direction = (NoxShared.ObjDataXfer.DoorXfer.DOORS_DIR)dorDir;
            }
            else if (ThingDb.Things[type].Xfer == "MonsterXfer")
            {
                int dir = (int)mapView.delta;
                NoxShared.ObjDataXfer.MonsterXfer m = result.GetExtraData<NoxShared.ObjDataXfer.MonsterXfer>();
                m.DirectionId = (byte)dir;
            }
            else if (ThingDb.Things[type].Xfer == "NPCXfer")
            {
                int dir = (int)mapView.delta;
                NoxShared.ObjDataXfer.NPCXfer npc = result.GetExtraData<NoxShared.ObjDataXfer.NPCXfer>();
                npc.DirectionId = (byte)dir;
            }
            else if (ThingDb.Things[type].Xfer == "SentryXfer")
            {
                float dir = mapView.delta;
                NoxShared.ObjDataXfer.SentryXfer s = result.GetExtraData<NoxShared.ObjDataXfer.SentryXfer>();
                s.BasePosRadian = (float)dir;
            }


            TheMap.Objects.Add(result);
            return result;
        }
        public static bool ObjectRemove(Map.Object obj)
        {
            if (TheMap.Objects.Contains(obj))
            {
                TheMap.Objects.Remove(obj);
                OpUpdatedObjects = true;
                return true;
            }
            return false;
        }
        public static void ObjectSelect45Rectangle(Point pt)
        {
            if (SelectedObjects.Origin != null)
                return;

            Point MousePoint = mapView.mouseKeep;
            if (MapInterface.CurrentMode == EditMode.OBJECT_SELECT && !mapView.picking && !MousePoint.IsEmpty)
            {
                SetSelect45Area(pt);
                //int size = Math.Abs(MapInterface.selected45Area[0].X - MapInterface.selected45Area[2].X);
                if (mapView.Get45RecSize() >= 5)
                    ObjectSelect(pt);
            }
        }
        public static void SetSelect45Area(Point pt)
        {
            var origin = mapView.mouseKeep;
            if (mapView.select45Box.Checked)
                pt = Rotate(pt, origin, -45);

            Point a = origin;
            Point b = new Point(pt.X, origin.Y);
            Point c = pt;
            Point d = new Point(origin.X, pt.Y);

            if (mapView.select45Box.Checked)
            {
                b = Rotate(b, a, 45);
                c = Rotate(c, a, 45);
                d = Rotate(d, a, 45);
            }
            selected45Area[0] = a;
            selected45Area[1] = b;
            selected45Area[2] = c;
            selected45Area[3] = d;
        }

        public static int GetNextObjectExtent()
        {
            int result = 3; // 2 = host player
            while (result != int.MaxValue)
            {
                bool found = false;

                // check if there are no objects with this extent
                foreach (Map.Object obj in TheMap.Objects)
                {
                    if (obj.Extent == result)
                    {
                        found = true; break;
                    }
                }

                if (found) result++;
                else break; // found unused
            }
            return result;
        }
        public static int FixObjectExtents()
        {
            Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
            int num1 = 0;
            foreach (Map.Object obj in TheMap.Objects)
            {
                int extent = obj.Extent;
                if (dictionary1.ContainsKey(extent))
                {
                    Dictionary<int, int> dictionary2;
                    int index;
                    (dictionary2 = dictionary1)[index = extent] = dictionary2[index] + 1;
                }
                else
                    dictionary1[extent] = 1;
            }
            foreach (KeyValuePair<int, int> keyValuePair in dictionary1)
            {
                if (keyValuePair.Value > 1)
                {
                    bool flag = false;
                    foreach (Map.Object object1 in TheMap.Objects)
                    {
                        if (object1.Extent == keyValuePair.Key)
                        {
                            if (!flag && (ThingDb.Things[object1.Name].Xfer == "TransporterXfer"
                                || ThingDb.Things[object1.Name].Xfer == "ElevatorXfer"
                                || ThingDb.Things[object1.Name].Xfer == "ElevatorShaftXfer"))
                            {
                                foreach (Map.Object object2 in TheMap.Objects)
                                {
                                    if ((ThingDb.Things[object2.Name].Xfer == "ElevatorXfer"
                                        || ThingDb.Things[object2.Name].Xfer == "ElevatorShaftXfer")
                                        && object2.GetExtraData<NoxShared.ObjDataXfer.ElevatorXfer>().ExtentLink > 0)
                                    {
                                        flag = true;
                                        break;
                                    }
                                    if (ThingDb.Things[object2.Name].Xfer == "TransporterXfer"
                                        && object2.GetExtraData<NoxShared.ObjDataXfer.TransporterXfer>().ExtentLink > 0)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (flag)
                                    continue;
                            }
                            int extent = object1.Extent;
                            object1.Extent = GetNextObjectExtent();
                            ++num1;
                        }
                    }
                }
            }
            return num1;
        }

        public static bool LineIntersectsRect(Point p1, Point p2, Rectangle r)
        {
            return LineIntersectsLine(p1, p2, new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y + r.Height), new Point(r.X, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X, r.Y + r.Height), new Point(r.X, r.Y)) ||
                   (r.Contains(p1) && r.Contains(p2));
        }
        public static bool LineIntersectsPolygon(Point p1, Point p2, Point[] polygon)
        {
            for (int i = 0; i <= polygon.Length - 2; i++)
            {
                if (LineIntersectsLine(polygon[i], polygon[i + 1], p1, p2))
                    return true;

            }

            return false;

        }
        private static bool LineIntersectsLine(Point l1p1, Point l1p2, Point l2p1, Point l2p2)
        {
            float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
            float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

            if (d == 0)
            {
                return false;
            }

            float r = q / d;

            q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
            float s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }
        public static bool PointInPolygon(Point p, Point[] poly)
        {
            Point p1, p2;
            bool inside = false;
            if (poly.Length < 3)
            {
                return inside;
            }
            Point oldPoint = new Point(
            poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            for (int i = 0; i < poly.Length; i++)
            {
                Point newPoint = new Point(poly[i].X, poly[i].Y);

                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                && ((long)p.Y - (long)p1.Y) * (long)(p2.X - p1.X)
                 < ((long)p2.Y - (long)p1.Y) * (long)(p.X - p1.X))
                {
                    inside = !inside;
                }
                oldPoint = newPoint;
            }

            return inside;
        }
        public static Point Rotate(Point point, PointF pivot, double angleSet)
        {
            double angle = angleSet * Math.PI / 180;
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            float dx = point.X - pivot.X;
            float dy = point.Y - pivot.Y;
            double x = cos * dx - sin * dy + pivot.X;
            double y = sin * dx + cos * dy + pivot.Y;

            Point result = new Point((int)Math.Round(x), (int)Math.Round(y));
            return result;
        }
        #endregion

        #region Waypoint Operations
        const byte WaypointFlag = 128;
        const double selectRadius = MapView.objectSelectionRadius * MapView.objectSelectionRadius;

        private Map.Waypoint _SelectedWaypoint = null;
        public static Map.Waypoint SelectedWaypoint
        {
            get { return _instance._SelectedWaypoint; }
            set { _instance._SelectedWaypoint = value; }
        }
        public static List<Map.Waypoint> SelectedWaypoints
        {
            get;
            set;
        }

        public static Map.Waypoint WaypointSelect(Point pt)
        {
            double closestDistance = Double.MaxValue;
            Map.Waypoint closest = null;

            foreach (Map.Waypoint wp in TheMap.Waypoints)
            {
                double distance = Math.Pow(pt.X - wp.Point.X, 2) + Math.Pow(pt.Y - wp.Point.Y, 2);

                if (distance < selectRadius && distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = wp;
                }
            }

            return closest;
        }
        public static bool WaypointRemove(Map.Waypoint wp)
        {
            if (TheMap.Waypoints.Contains(wp))
            {
                if (wp == SelectedWaypoint) SelectedWaypoint = null;
                SelectedWaypoints.Remove(wp);
                TheMap.Waypoints.Remove(wp);
                TheMap.Waypoints.num_wp.Remove(wp.Number);
                OpUpdatedWaypoints = true;
                return true;
            }
            return false;
        }
        public static Map.Waypoint WaypointPlace(string name, PointF loc, bool enabled)
        {
            Map.Waypoint wp = new Map.Waypoint("", loc, GetNextWaypointNumber());
            wp.Flags = enabled ? 1 : 0;
            TheMap.Waypoints.Add(wp);
            TheMap.Waypoints.num_wp.Add(wp.Number, wp);
            OpUpdatedWaypoints = true;
            return wp;
        }
        public static int GetNextWaypointNumber()
        {
            int i;
            for (i = 1; TheMap.Waypoints.num_wp.ContainsKey(i); i++) ;
            return i;
        }

        public static void WaypointSelectAll()
        {
            SelectedWaypoints.Clear();
            foreach (Map.Waypoint wp in TheMap.Waypoints)
                SelectedWaypoints.Add(wp);
        }
        public static void WaypointEnableAll()
        {
            foreach (Map.Waypoint wp in TheMap.Waypoints)
                wp.Flags = 1;
        }

        public static void WaypointClearPaths()
        {
            foreach (Map.Waypoint wp in TheMap.Waypoints)
                wp.connections.Clear();
        }
        public static bool WaypointConnect(Map.Waypoint wp, Map.Waypoint proxyWP = null)
        {

            Map.Waypoint destWaypoint = proxyWP == null ? SelectedWaypoint : proxyWP;

            if (wp != null && destWaypoint != null && !wp.Equals(destWaypoint))
            {
                bool ok = true;

                foreach (Map.Waypoint.WaypointConnection wpc in wp.connections)
                {

                    foreach (Map.Waypoint.WaypointConnection wpcs in destWaypoint.connections)//Checks if the waypoint connection is connecting to wp
                    {
                        if (wpcs.wp.Equals(wp))
                        {
                            ok = false;
                            break;
                        }
                    }
                }

                if (ok)
                {
                    mapView.ApplyStore();
                    destWaypoint.AddConnByNum(wp, WaypointFlag);
                    OpUpdatedWaypoints = true;
                    // MessageBox.Show("sdsd");

                }
                if (mapView.doubleWp.Checked && proxyWP == null)
                {
                    WaypointConnect(SelectedWaypoint, wp);
                }
                if (ok) return true;

            }
            return false;
        }
        public static bool WaypointUnconnect(Map.Waypoint wp)
        {


            if (wp != null && SelectedWaypoint != null && !wp.Equals(SelectedWaypoint))
            {
                bool ok = false;
                foreach (Map.Waypoint.WaypointConnection wpc in wp.connections)
                {

                    if (wpc.wp.Equals(SelectedWaypoint))
                    {
                        ok = true;
                        break;
                    }

                }

                if (ok)
                {
                    mapView.ApplyStore();
                    wp.RemoveConnByNum(SelectedWaypoint);
                    OpUpdatedWaypoints = true;
                    return true;
                }

            }
            return false;
        }
        #endregion

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
    }
}

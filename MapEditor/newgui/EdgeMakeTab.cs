/*
 * MapEditor
 * Пользователь: AngryKirC
 * Дата: 23.01.2015
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using MapEditor.videobag;
using NoxShared;

namespace MapEditor.newgui
{
	/// <summary>
	/// Edge creation GUI
	/// </summary>
    public partial class EdgeMakeTab : UserControl
    {
        private List<string> sortedEdgeNames;
        private MapView mapView;
        private VideoBagCachedProvider videoBag = null;
        int edgeDirection = 0;
        int edgeTypeID = 0;

        public EdgeMakeTab()
        {
            InitializeComponent();

            //blackTileSprite = (int) ThingDb.FloorTiles[ThingDb.FloorTileNames.IndexOf("Black")].Variations[0];
            // listview ставим обработчики
            listEdgeImages.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(listEdgeImages_RetrieveVirtualItem);
        }

        private void listEdgeImages_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            ListViewItem item = new ListViewItem("", e.ItemIndex);
            item.BackColor = Color.White;
            e.Item = item;
        }

        private int GetSelTileTypeIndex()
        {
            return ThingDb.EdgeTileNames.IndexOf(sortedEdgeNames[comboEdgeType.SelectedIndex]);
        }

        public void SetMapView(MapView view)
        {
            mapView = view;
            // необходимо чтобы картинки доставать
            videoBag = mapView.MapRenderer.VideoBag;
            // названия плиток сортируем и добавляем в список
            sortedEdgeNames = new List<string>(ThingDb.EdgeTileNames.ToArray());
            sortedEdgeNames.Sort();
            comboEdgeType.Items.AddRange(sortedEdgeNames.ToArray());
            comboEdgeType.SelectedIndex = 0;
        }

        /// <summary>
        /// Возвращает новый экземпляр EdgeTile в соответствии с настройками пользователя
        /// </summary>
        public Map.Tile.EdgeTile GetEdge()
        {
            // как покрытие юзаем тот тайл что выбран во вкладке Tiles
            var tile = mapView.GetNearestTile(mapView.mouseLocation);
            Map.Tile coverTile = mapView.TileMakeNewCtrl.GetTile(tile);

            var edgeDir = (Map.Tile.EdgeTile.Direction)edgeDirection;
            if ((chkAutoVariation.Checked) && (!chkAutoEdge.Checked))
                edgeDir = GetRandomVariation(edgeDir);

            return new Map.Tile.EdgeTile(coverTile.graphicId, coverTile.Variation, edgeDir, (byte)edgeTypeID);
        }
        private Map.Tile.EdgeTile.Direction GetRandomVariation(Map.Tile.EdgeTile.Direction dir)
        {
            // Variation is actually Direction, 3 variations for N, S, E, W
            var r = new Random();

            switch (dir)
            {
                case Map.Tile.EdgeTile.Direction.East:
                case Map.Tile.EdgeTile.Direction.East_D:
                case Map.Tile.EdgeTile.Direction.East_E:
                    return (Map.Tile.EdgeTile.Direction)r.Next(12, 15);
                case Map.Tile.EdgeTile.Direction.West:
                case Map.Tile.EdgeTile.Direction.West_02:
                case Map.Tile.EdgeTile.Direction.West_03:
                    return (Map.Tile.EdgeTile.Direction)r.Next(1, 4);
                case Map.Tile.EdgeTile.Direction.North:
                case Map.Tile.EdgeTile.Direction.North_08:
                case Map.Tile.EdgeTile.Direction.North_0A:
                    return OddRandom(true, r);
                case Map.Tile.EdgeTile.Direction.South:
                case Map.Tile.EdgeTile.Direction.South_07:
                case Map.Tile.EdgeTile.Direction.South_09:
                    return OddRandom(false, r);
                default:
                    return dir;
            }
        }
        private Map.Tile.EdgeTile.Direction OddRandom(bool north, Random r)
        {
            if (north)
            {
                int i = r.Next(1, 4);
                if (i == 1) return Map.Tile.EdgeTile.Direction.North;
                if (i == 2) return Map.Tile.EdgeTile.Direction.North_08;
                return Map.Tile.EdgeTile.Direction.North_0A;
            }
            else
            {
                int i = r.Next(1, 4);
                if (i == 1) return Map.Tile.EdgeTile.Direction.South;
                if (i == 2) return Map.Tile.EdgeTile.Direction.South_07;
                return Map.Tile.EdgeTile.Direction.South_09;
            }
        }

        public void UpdateListView(object sender, EventArgs e)
        {
            // force update data
            edgeDirection = 0;
            edgeTypeID = GetSelTileTypeIndex();
            listEdgeImages.VirtualListSize = 0;
            List<uint> variations = ThingDb.EdgeTiles[edgeTypeID].Variations;
            listEdgeImages.VirtualListSize = variations.Count;
            // not yet created
            if (listEdgeImages.LargeImageList == null)
                listEdgeImages.LargeImageList = new ImageList();
            // clear ImageList
            ImageList imglist = listEdgeImages.LargeImageList;
            foreach (Image img in imglist.Images)
            	img.Dispose();
            imglist.Images.Clear();
            imglist.ImageSize = new Size(46, 46);
            // update ImageList showing edge type selected by user
            Map.Tile coverTile = mapView.TileMakeNewCtrl.GetTile(Point.Empty);
            int coverSprite = (int)coverTile.Variations[coverTile.Variation];
            int varns = variations.Count;
            if (mapView.TileMakeNewCtrl.edgeBox.Items.Count > 0)
            mapView.TileMakeNewCtrl.edgeBox.SelectedIndex = comboEdgeType.SelectedIndex;
         
            for (int varn = 0; varn < varns; varn++)
            {
            	Bitmap edge = new Bitmap(46, 46);
            	videoBag.ApplyEdgeMask(edge, (int)variations[varn], coverSprite);
            	imglist.Images.Add(edge);
            }
        }

        private void SelectEdgeDirection(object sender, EventArgs e)
        {
            if (listEdgeImages.SelectedIndices.Count > 0)
                edgeDirection = listEdgeImages.SelectedIndices[0];
            
            // update mapinterface
            //mapView.GetMapInt().EdgeSetData((byte) edgeTypeID, (byte) edgeDirection);
        }

        private void chkAutoEdge_CheckedChanged(object sender, EventArgs e)
        {
            AutoEdgeGrp.Enabled = chkAutoEdge.Checked;
            chkAutoVariation.Enabled = !chkAutoEdge.Checked;
        }

        private void listEdgeImages_MouseClick(object sender, MouseEventArgs e)
        {
            chkAutoEdge.Checked = false;
        }

        private void Picker_CheckedChanged(object sender, EventArgs e)
        {
            if (Picker.Checked)
                mapView.Picker.Checked = true;
            else
            {
                mapView.Picker.Checked = false;
                mapView.picking = false;
            }
        }
    }   
}

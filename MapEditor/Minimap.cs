using MapEditor.MapInt;
using MapEditor.render;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using OpenNoxLibrary.Files;

namespace MapEditor
{
    public class Minimap : Form
    {
        public bool big = true;
        private Point MouseLocation = new Point();
        private Point relpoint = new Point();
        private Point relXYpoint = new Point();
        private int sqSize = 23;
        private IContainer components;
        private Timer timer1;
        public PictureBox minimapBox;
        private bool mouseDown;
        private Bitmap bitmap2;
        private Rectangle screenRect;
        private bool mouseMove;
        public bool stopMap;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Minimap));
            components = new Container();
            minimapBox = new PictureBox();
            timer1 = new Timer(components);
            ((ISupportInitialize)minimapBox).BeginInit();
            SuspendLayout();
            minimapBox.BackColor = Color.Black;
            minimapBox.BorderStyle = BorderStyle.FixedSingle;
            minimapBox.Image = (Image)resources.GetObject("minimapBox.Image");
            minimapBox.Location = new Point(0, 0);
            minimapBox.Margin = new Padding(0);
            minimapBox.Name = "minimapBox";
            minimapBox.Size = new Size(256, 256);
            minimapBox.TabIndex = 0;
            minimapBox.TabStop = false;
            minimapBox.Paint += new PaintEventHandler(minimapBox_Paint);
            minimapBox.MouseEnter += new EventHandler(minimapBox_MouseEnter);
            minimapBox.MouseMove += new MouseEventHandler(minimapBox_MouseMove);
            minimapBox.MouseUp += new MouseEventHandler(minimapBox_MouseUp);
            timer1.Enabled = true;
            timer1.Interval = 35;
            timer1.Tick += new EventHandler(timer1_Tick);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(256, 256);
            ControlBox = false;
            Controls.Add(minimapBox);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Minimap";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "Minimap";
            KeyDown += new KeyEventHandler(Minimap_KeyDown);
            ((ISupportInitialize)minimapBox).EndInit();
            ResumeLayout(false);
        }

        public Minimap()
        {
            InitializeComponent();
        }

        protected NoxMap map
        {
            get
            {
                return MapInterface.TheMap;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (MainWindow.Instance.ProcessCmdKeyFromChildForm(ref msg, keyData))
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void minimapBox_Paint(object sender, PaintEventArgs e)
        {
            if (map == null || !big)
                return;
            Graphics graphics = e.Graphics;
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.Clear(Color.Black);
            Bitmap bitmap1;
            if (bitmap2 != null)
            {
                bitmap1 = bitmap2;
            }
            else
            {
                Bitmap bitmap2 = new Bitmap(256, 256);
                Rectangle clip = new Rectangle(new Point(0, 0), new Size(256, 256));
                MinimapRenderer minimapRenderer = new MinimapRenderer(bitmap2, map, MainWindow.Instance.mapView.MapRenderer.FakeWalls);
                minimapRenderer.LockBitmap();
                minimapRenderer.DrawMinimap(1, clip);
                bitmap1 = minimapRenderer.UnlockBitmap();
            }
            bitmap2 = bitmap1;
            graphics.DrawImage(bitmap1, 0, 0, bitmap1.Width, bitmap1.Height);
            int num1 = MainWindow.Instance.mapView.scrollPanel.HorizontalScroll.Value + sqSize;
            int num2 = MainWindow.Instance.mapView.scrollPanel.VerticalScroll.Value + sqSize;
            int x = num1 / sqSize;
            int y = num2 / sqSize;
            int width = MainWindow.Instance.mapView.scrollPanel.Width / sqSize;
            int height = MainWindow.Instance.mapView.scrollPanel.Height / sqSize;
            screenRect = new Rectangle(new Point(x, y), new Size(width, height));
            screenRect = new Rectangle(new Point(x, y), new Size(width, height));
            relpoint = new Point(x + width / 2, y + height / 2);
            Pen pen = Opacity < 0.5 ? new Pen(Color.Gray, 2f) : new Pen(Color.Aqua, 2f);
            graphics.DrawRectangle(pen, screenRect);
        }

        private void minimapBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Left))
            {
                mouseDown = true;
                Point point = new Point(e.X, e.Y);
                if (!point.Equals(MouseLocation))
                    mouseMove = true;
                point = MouseLocation;
            }
            MouseLocation = new Point(e.X, e.Y);
            Point point1 = new Point()
            {
                X = relpoint.X - MouseLocation.X,
                Y = relpoint.Y - MouseLocation.Y
            };
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!big)
            {
                if (stopMap)
                    return;
                if (EditorSettings.Default.Minimap_Autohide)
                {
                    double num = (80 - Width) * 0.8;
                    Height += (int)num;
                    Width += (int)num;
                    Left -= (int)num;
                    if (num > -2.0)
                    {
                        Height = 80;
                        Width = 80;
                        setPos();
                        stopMap = true;
                        MainWindow.Instance.mapView.UpdateCanvasOKMiniMap = true;
                        minimapBox.Invalidate();
                    }
                }
            }
            else if (EditorSettings.Default.Minimap_Autohide)
            {
                double num = (256 - Width) * 0.7;
                Left -= (int)num;
                Height += (int)num;
                Width += (int)num;
                if (num < 2.0)
                {
                    Height = 256;
                    Width = 256;
                    setPos();
                    minimapBox.Invalidate();
                }
                stopMap = false;
                Invalidate();
            }
            if (mouseDown)
            {
                if (relXYpoint.IsEmpty)
                {
                    relXYpoint.X = relpoint.X - MouseLocation.X;
                    relXYpoint.Y = relpoint.Y - MouseLocation.Y;
                }
                GoToPoint(new Point((MouseLocation.X + relXYpoint.X) * sqSize, (MouseLocation.Y + relXYpoint.Y) * sqSize));
                mouseMove = false;
            }
            else
            {
                relXYpoint = new Point();
                mouseMove = false;
            }
            minimapBox.Invalidate();
        }

        public void GoToPoint(Point centerAt)
        {
            int height = centerAt.Y - MainWindow.Instance.mapView.scrollPanel.Height / 2;
            int width = centerAt.X - MainWindow.Instance.mapView.scrollPanel.Width / 2;
            if (height < 0)
                height = 0;
            if (width < 0)
                width = 0;
            if (width > 5880)
                width = 5880;
            if (height > 5880)
                height = 5880;
            float aX = width - MainWindow.Instance.mapView.scrollPanel.HorizontalScroll.Value;
            float aY = height - MainWindow.Instance.mapView.scrollPanel.VerticalScroll.Value;
            float bX = aX * 0.2f;
            float bY = aY * 0.2f;
            float x = bX * 2.8f;
            float y = bY * 2.8f;
            if (!mouseMove)
            {
                x *= 1.6f;
                y *= 1.6f;
            }
            if (x < 1.0 && x > 0.0)
                x = 0.0f;
            if (x > -1.0 && x < 0.0)
                x = 0.0f;
            if (y < 1.0 && y > 0.0)
                y = 0.0f;
            if (y > -1.0 && y < 0.0)
                y = 0.0f;
            MainWindow.Instance.mapView.scrollPanel.VerticalScroll.Value += (int)y;
            MainWindow.Instance.mapView.scrollPanel.HorizontalScroll.Value += (int)x;
            MainWindow.Instance.mapView.scrollPanel.PerformLayout();
            MainWindow.Instance.mapView.MapRenderer.UpdateCanvas(true, true, false);
            MainWindow.Instance.mapView.mapPanel.Invalidate();
        }
        public void setPos()
        {
            Left = MainWindow.Instance.Location.X + MainWindow.Instance.Width - Width - 31;
            Top = MainWindow.Instance.Location.Y + 76;
        }
        public void applySettings()
        {
            Visible = EditorSettings.Default.Minimap_Show;
            if (EditorSettings.Default.Minimap_Autohide)
            {
                Size = new Size(80, 80);
                big = false;
            }
            else
                Size = new Size(256, 256);
            Opacity = EditorSettings.Default.Minimap_Autoalpha ? 0.4 : 1.0;
            minimapBox.Invalidate();
        }
        public void Reload()
        {
            bitmap2 = null;
            big = !EditorSettings.Default.Minimap_Autohide;
            minimapBox.Invalidate();
            Invalidate(true);
            //Update();
            //SetStyle(ControlStyles.UserPaint, true);
        }

        private void minimapBox_MouseEnter(object sender, EventArgs e)
        {
            setPos();
            big = true;
            Opacity = 1.0;
            Focus();
            minimapBox.Invalidate();
            MainWindow.Instance.mapView.mapPanel.Invalidate();
        }
        private void minimapBox_MouseUp(object sender, MouseEventArgs e)
        {
            MouseLocation = new Point();
            if (!mouseDown)
                MainWindow.Instance.mapView.CenterAtPoint(new Point(e.X * sqSize, e.Y * sqSize));
            mouseDown = false;
        }
        private void Minimap_KeyDown(object sender, KeyEventArgs e)
        {
            MainWindow.Instance.mapView.TabsShortcuts(sender, e);
        }
    }
}

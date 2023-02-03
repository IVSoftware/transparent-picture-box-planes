using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace transparent_picture_box_00
{
    public partial class MainForm : Form
    {
        public MainForm()=> InitializeComponent();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DoubleBuffered = true;
            #region G L Y P H
            var path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Fonts",
                "glyphs.ttf");
            privateFontCollection.AddFontFile(path);
            var fontFamily = privateFontCollection.Families[0];
            Glyphs = new Font(fontFamily, 24F);
            #endregion G L Y P H

            BackgroundImageLayout = ImageLayout.Stretch;
            makePlanes();
            _ = execFlyPlanes();
        }

        private void makePlanes()
        {
            makePlane(glyph: "\uE800", Color.Blue, rotateFlip: RotateFlipType.RotateNoneFlipNone);
            makePlane(glyph: "\uE801", Color.Green, rotateFlip: RotateFlipType.RotateNoneFlipNone);
            makePlane(glyph: "\uE802", Color.DarkGoldenrod, rotateFlip: RotateFlipType.RotateNoneFlipX);
            makePlane(glyph: "\uE803", Color.Gray, rotateFlip: RotateFlipType.RotateNoneFlipNone);
            makePlane(glyph: "\uE804", Color.Salmon, rotateFlip: RotateFlipType.RotateNoneFlipX);
        }

        const int DIM = 60;
        int _planeId = 0;
        private void makePlane(string glyph, Color color, RotateFlipType rotateFlip)
        {            
            var plane = new PictureBox
            {
                Name = $"plane{_planeId++}",
                Size = new Size(DIM, DIM),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(_rando.Next(Width), _rando.Next(Height)),
                BackColor = Color.Transparent,
                Region = new Region(new Rectangle(0, 0, DIM, DIM)),
                Image = new Bitmap(DIM, DIM)
            };

            using (Graphics graphics = Graphics.FromImage(plane.Image))
            using (SolidBrush brush = new SolidBrush(color))
            {
                graphics.Clear(Color.Transparent);
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                graphics.DrawString(glyph, Glyphs, brush, new PointF());
            }
            plane.Image.RotateFlip(rotateFlip);

            for (int x = 0; x < DIM; x++) for (int y = 0; y < DIM; y++)
            {
                if (((Bitmap)plane.Image).GetPixel(x, y).A == 0)
                {
                    plane.Region.Exclude(new Rectangle(x, y, 1, 1));
                }
            }
            Controls.Add(plane);
        }
        Random _rando = new Random(1);

        private async Task execFlyPlanes()
        {
            while(true)
            {
                _count++;
                // Screenshots
                switch (_count)
                {
                    // case 190: await new SemaphoreSlim(0).WaitAsync(); break;
                    // case 400: await new SemaphoreSlim(0).WaitAsync(); break;
                    // case 465: await new SemaphoreSlim(0).WaitAsync(); break;
                }
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        var plane = Controls[$"plane{i}"];
                        if (plane != null)
                        {
                            switch (i)
                            {
                                case 0: plane.Location = plane.Location.FlyNE(1, Size); break;
                                case 1: plane.Location = plane.Location.FlyE(5, Size); break;
                                case 2: plane.Location = plane.Location.FlyW(5, Size); break;
                                case 3: plane.Location = plane.Location.FlyN(3, Size); break;
                                case 4:; plane.Location = plane.Location.FlyNW(5, Size); break; 
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                    }
                    catch { }
                    await Task.Delay(1);
                }
            }
        }
        int _count = 0;
        public static Font Glyphs { get; private set; }
        PrivateFontCollection privateFontCollection = new PrivateFontCollection();
    }

    static class Extensions
    {
        public static Point FlyN(this Point point, int distance, Size canvas) =>
            new Point(point.X, point.Y - distance).ensureCanvas(canvas);
        public static Point FlyNE(this Point point, int distance, Size canvas) =>
            new Point(point.X + distance, point.Y - distance).ensureCanvas(canvas);
        public static Point FlyNW(this Point point, int distance, Size canvas) =>
            new Point(point.X - distance, point.Y - distance).ensureCanvas(canvas);
        public static Point FlyE(this Point point, int distance, Size canvas) =>
            new Point(point.X + distance, point.Y).ensureCanvas(canvas);
        public static Point FlyW(this Point point, int distance, Size canvas) =>
            new Point(point.X - distance, point.Y).ensureCanvas(canvas);
        private static Point ensureCanvas(this Point point, Size canvas)
        {
            if (point.X < 0) point.X = canvas.Width;
            else if (point.X >= canvas.Width) point.X = 0;
            if (point.Y < 0) point.Y = canvas.Height;
            else if (point.Y >= canvas.Height) point.Y = 0;
            return point;
        }
    }
}

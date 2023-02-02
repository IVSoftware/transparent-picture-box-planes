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

            _ = execFlyPlanes();
        }

        private async Task execFlyPlanes()
        {
            const int DIM = 60;
            for (int i = 0; i < 5; i++)
            {
                string glyph;
                Color color;
                switch (i)
                {
                    case 0: glyph = "\uE800"; color = Color.Blue; break;
                    case 1: glyph = "\uE801"; color = Color.Green; break;
                    case 2: glyph = "\uE802"; color = Color.DarkGoldenrod; break;
                    case 3: glyph = "\uE803"; color = Color.Gray; break;
                    case 4: glyph = "\uE804"; color = Color.Salmon; break;
                    default:
                        throw new NotImplementedException();
                }
                Bitmap image = new Bitmap(DIM, DIM);
                using (Graphics graphics = Graphics.FromImage(image))
                using (SolidBrush brush = new SolidBrush(color))
                {
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    graphics.DrawString(glyph, Glyphs, brush, new PointF());
                }
                switch (i)
                {
                    case 4:
                    case 2: image.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                }
                Region rgn = new Region();
                rgn.Union(new Rectangle(0, 0, image.Width, image.Height));
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        if (image.GetPixel(x, y).A == 0)
                        {
                            rgn.Exclude(new Rectangle(x, y, 1, 1));
                        }
                    }
                }
                var plane = new PictureBox
                {
                    Name = $"plane{i}",
                    Size = new Size(DIM, DIM),
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(_rando.Next(Width), _rando.Next(Height)),
                    BackColor = Color.Transparent,
                    BackgroundImage = image,
                };
                plane.Region = rgn;
                Controls.Add(plane);
            }
            while(true)
            {
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
        Random _rando = new Random(1);
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
        public static Rectangle InflateInline (this Rectangle rectangle, int width, int height)
        {
            rectangle.Inflate(width, height);
            return rectangle;
        }
    }
}

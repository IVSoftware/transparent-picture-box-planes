You have described the lack of transparency problem when one airplane `PictureBox` flies over another and this issue was easy to reproduce. The cause is this: The MS [documentation](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-give-your-control-a-transparent-background) states that "the background of a transparent Windows Forms control is painted by its parent". This would include a background image on the parent but what it _doesn't_ take into account is when another control comes between the parent and the control being drawn.

> Is there a way to fix this?

The answer is Yes and the approach put forward by Idle_Mind can be easily applied to your specific situation. 

[![screenshot][1]][1]


Another way of explaining the process is that once the `plane.Image` is assigned we go through it pixel-by-pixel looking at the Alpha Channel (the A of the ARGB color). One could think of this as affecting the opacity of the control, and if its value for a given pixel is zero it is considered transparent. Excluding those transparent pixels from the picture box's `Region` changes the actual shape of the control and drawing no longer occurs at that position.

***
**Make a single plane PictureBox**

    const int DIM = 60;
    int _planeId = 0;
    private void makePlane(string glyph, Color color, RotateFlipType rotateFlip)
    {            
        var plane = new PictureBox
        {
            Name = $"plane{_planeId++}",
            Size = new Size(DIM, DIM),
            BorderStyle = BorderStyle.FixedSingle,
            // Random starting position
            Location = new Point(_rando.Next(Width), _rando.Next(Height)),
            BackColor = Color.Transparent,
            Region = new Region(new Rectangle(0, 0, DIM, DIM)),
            Image = new Bitmap(DIM, DIM)
        };
        // For purposes of testing, this draws a plane glyph on a transparent canvas.
        using (Graphics graphics = Graphics.FromImage(plane.Image))
        using (SolidBrush brush = new SolidBrush(color))
        {
            graphics.Clear(Color.Transparent);
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.DrawString(glyph, Glyphs, brush, new PointF());
        }
        // Point the plane in the direction it's going to fly
        plane.Image.RotateFlip(rotateFlip);
        // Change the actual shape of the picture box so that it 
        // no longer includes pixels designated as transparent.

        for (int x = 0; x < DIM; x++) for (int y = 0; y < DIM; y++)
        {
            if (((Bitmap)plane.Image).GetPixel(x, y).A == 0)
            {
                plane.Region.Exclude(new Rectangle(x, y, 1, 1));
            }
        }
        Controls.Add(plane);
    }
    // Seeded random for repeatability for testing.
    Random _rando = new Random(1);

***
**Testing**

This minimal code for main form makes 5 planes then executes an async loop to fly them by calling extension methods on the current location corresponding to the intended direction.

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
        public static Font Glyphs { get; private set; }
        PrivateFontCollection privateFontCollection = new PrivateFontCollection();

        private void makePlanes()
        {
            makePlane(glyph: "\uE800", Color.Blue, rotateFlip: RotateFlipType.RotateNoneFlipNone);
            makePlane(glyph: "\uE801", Color.Green, rotateFlip: RotateFlipType.RotateNoneFlipNone);
            makePlane(glyph: "\uE802", Color.DarkGoldenrod, rotateFlip: RotateFlipType.RotateNoneFlipX);
            makePlane(glyph: "\uE803", Color.Gray, rotateFlip: RotateFlipType.RotateNoneFlipNone);
            makePlane(glyph: "\uE804", Color.Salmon, rotateFlip: RotateFlipType.RotateNoneFlipX);
        }

        private async Task execFlyPlanes()
        {
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
        .
        .
        .
        // makePlane{...}
    }

***
**Extension methods for `Point`**

Get the next location for plane based on the vector and ensure that the new location results in a visible location on the main canvas.

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

  [1]: https://i.stack.imgur.com/j23sV.png
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows;
using System.IO;

namespace CropCap
{
    class ScreenShot
    {

        private Point StartPoint { get; set; }
        private Point EndPoint { get; set; }
        public Form form { get; set; }
        public Form rectForm { get; set; }
        private Configuration config { get; set; }
        private Timer loopTimer;

        public ScreenShot(Configuration con)
        {
            config = con;
        }

        public void TakeScreenShot()
        {
            if (form == null || form.ActiveControl == null)
            {
                form = new Form();
                form.WindowState = FormWindowState.Maximized;
                form.FormBorderStyle = FormBorderStyle.None;
                form.AllowTransparency = true;
                form.TopMost = true;
                form.BackColor = Color.WhiteSmoke;
                form.Opacity = .005;
                form.MouseDown += new MouseEventHandler(onMouseDown);
                form.MouseUp += new MouseEventHandler(onMouseUp);
                form.Show();

                try
                {
                    ImageConverter converter = new ImageConverter();
                    Stream stream = new MemoryStream(Properties.Resources.Cursor);
                    form.Cursor = new Cursor(stream);
                }
                catch (System.ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            Action resizeForm = (Action) delegate
            {
                rectForm.BackColor = Color.Red;
                rectForm.Opacity = .15;
                rectForm.FormBorderStyle = FormBorderStyle.None;

                Point startLoc = StartPoint;
                Point endLoc = Cursor.Position;

                switchPoints(startLoc, endLoc, out startLoc, out endLoc);

                rectForm.Location = startLoc;
                rectForm.Size = new Size(endLoc.X - startLoc.X, endLoc.Y - startLoc.Y);

                if (rectForm.Visible)
                    rectForm.Refresh();
                else
                    rectForm.Show();
            };

            loopTimer = new Timer();
            loopTimer.Interval = 5;
            loopTimer.Enabled = false;

            loopTimer.Elapsed += (Object sourse, ElapsedEventArgs e) =>
            {

                if (rectForm == null)
                    rectForm = new Form();


                form.Invoke(resizeForm);
            };


            loopTimer.AutoReset = true;

            
        }
        public Bitmap getBmp()
        {

            Size sz = new Size(EndPoint.X - StartPoint.X, EndPoint.Y - StartPoint.Y);
            IntPtr hDesk = GetDesktopWindow();
            IntPtr hSrce = GetWindowDC(hDesk);
            IntPtr hDest = CreateCompatibleDC(hSrce);
            IntPtr hBmp = CreateCompatibleBitmap(hSrce, sz.Width, sz.Height);
            IntPtr hOldBmp = SelectObject(hDest, hBmp);
            bool b = BitBlt(hDest, 0, 0, sz.Width, sz.Height, hSrce, StartPoint.X, StartPoint.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            Bitmap bmp = Bitmap.FromHbitmap(hBmp);
            SelectObject(hDest, hOldBmp);
            DeleteObject(hBmp);
            DeleteDC(hDest);
            return bmp;
        }

        private void saveImage(Bitmap image)
        {
            String name = "img";
            int i;

            for(i = 0; File.Exists(config.SaveLocation + "\\" + name + i + ".png"); i++)
                continue;

            image.Save(config.SaveLocation + "\\" + name + i + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }

        private void switchPoints(Point start, Point end, out Point startPoint, out Point endPoint)
        {
            if (start.X > end.X)
            {
                Point tempPoint = new Point(start.X, start.Y);
                start = new Point(end.X, tempPoint.Y);
                end = new Point(tempPoint.X, end.Y);
            }

            if (start.Y > end.Y)
            {
                Point tempPoint = new Point(start.X, start.Y);
                start = new Point(start.X, end.Y);
                end = new Point(end.X, tempPoint.Y);
            }

            startPoint = start;
            endPoint = end;
        }

        private void onMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

                EndPoint = e.Location;

                if (loopTimer.Enabled)
                {
                    loopTimer.Stop();
                    rectForm.Dispose();
                }
                Point temp1, temp2;
                switchPoints(StartPoint, EndPoint, out temp1, out temp2);
                StartPoint = temp1;
                EndPoint = temp2;

                Cursor.Current = Cursors.Default;
                form.Cursor = Cursors.Default;
                Bitmap image = getBmp();

                if (config.CopyClipboard)
                    Clipboard.SetImage(image);

                saveImage(image);
            }

            form.Dispose();

        }

        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo aProp =
                  typeof(System.Windows.Forms.Control).GetProperty(
                        "DoubleBuffered",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

            aProp.SetValue(c, true, null);
        }

        private void onMouseDown(object sender, MouseEventArgs e)
        {
            SetDoubleBuffered(form);

            if (e.Button == MouseButtons.Left)
            {
                StartPoint = e.Location;


                loopTimer.Start();
            }

        }


        // P/Invoke declarations
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
        wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);
    }
}

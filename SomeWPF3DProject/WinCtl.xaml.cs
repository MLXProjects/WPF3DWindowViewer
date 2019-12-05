using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FormsDraw = System.Drawing;
using SWF = System.Windows.Forms;

namespace SomeWPF3DProject
{
    public partial class WinCtl : Image
    {
        FormsDraw.Rectangle WinBounds;
        string Title = "";
        public WinCtl(IntPtr hwin, Rect rect)
        {
            InitializeComponent();
            this.Tag = hwin;
            WinBounds = new FormsDraw.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            Title = GetWindowTitle(hwin);
            this.Width = WinBounds.Width;
            this.Height = WinBounds.Height;
            if (hwin == IntPtr.Zero)
                this.Source = CopyScreen();
            else
            {
                this.Source = GetWindowImage(hwin, WinBounds);
                DispatcherTimer dt = new DispatcherTimer(TimeSpan.FromMilliseconds(200), DispatcherPriority.Render, new EventHandler(TimerTick), this.Dispatcher);
                //dt.Start();
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public void TimerTick(object sender, EventArgs e)
        {            
            Rect rect = new Rect();
            GetWindowRect((IntPtr)this.Tag, ref rect);
            WinBounds = new FormsDraw.Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            this.Width = WinBounds.Width;
            this.Height = WinBounds.Height;
            this.Source = GetWindowImage((IntPtr)this.Tag, WinBounds);
        }

        public string GetWindowTitle(IntPtr hWnd)
        {
            StringBuilder strbTitle = new StringBuilder(255);
            GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
            return strbTitle.ToString();
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public ImageSource GetWindowImage(IntPtr hWnd, FormsDraw.Rectangle bounds)
        {
            try
            {
                using (FormsDraw.Bitmap result = new FormsDraw.Bitmap(bounds.Width, bounds.Height))
                {//creates a new Bitmap which will contain the window's image
                    using (FormsDraw.Graphics MemG = FormsDraw.Graphics.FromImage(result))
                    {//makes a Graphics from the Bitmap whose HDC will hold the window's RAW image
                        IntPtr dc = MemG.GetHdc();//gets the Bitmap's HDC 
                        try
                        {
                            PrintWindow(hWnd, dc, 0);//Writes the window's image to the HDC created above
                        }
                        finally
                        {
                            MemG.ReleaseHdc(dc);
                        }
                        var handle = result.GetHbitmap();
                        try
                        {
                            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        }
                        finally { DeleteObject(handle); }
                    }
                }
            }
            catch { /*this try/catch block is only here to hide any function's errors :P (bad practice? where?) 
                     It also creates an empty bitmap to avoid returning null and thus prevent the entire program from going to hell */
                FormsDraw.Bitmap nullbmp = new FormsDraw.Bitmap(bounds.Width, bounds.Height);
                var handle = nullbmp.GetHbitmap();
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        public BitmapSource GetScreenImage()
        {
        using (FormsDraw.Bitmap screenBmp = new FormsDraw.Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, FormsDraw.Imaging.PixelFormat.Format32bppRgb))
            {
                using (var bmpGraphics = FormsDraw.Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);
                    MessageBox.Show(screenBmp.Size.ToString());
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        public ImageSource CopyScreen()
        {
            var left = SWF.Screen.AllScreens.Min(screen => screen.Bounds.X);
            var top = SWF.Screen.AllScreens.Min(screen => screen.Bounds.Y);
            var right = SWF.Screen.AllScreens.Max(screen => screen.Bounds.X + screen.Bounds.Width);
            var bottom = SWF.Screen.AllScreens.Max(screen => screen.Bounds.Y + screen.Bounds.Height);
            var width = right - left;
            var height = bottom - top;

            using (var screenBmp = new FormsDraw.Bitmap(width, height, FormsDraw.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = FormsDraw.Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height));
                    //return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(screenBmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    return ImageSourceFromBitmap(screenBmp);
                }
            }
        }

        public ImageSource ImageSourceFromBitmap(FormsDraw.Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                bmp.Save(@"C:\img.png");
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }
    }
}

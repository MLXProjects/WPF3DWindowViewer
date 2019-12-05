using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FormsDraw = System.Drawing;

namespace SomeWPF3DProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IntPtr hWnd;

        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(string lpClassName, string lpWindowName);

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            MeshGeometry3D mesh = new MeshGeometry3D();
            PointCollection textures = new PointCollection();
            textures.Add(new Point(0, 0));
            textures.Add(new Point(0, 1));
            textures.Add(new Point(1, 1));
            textures.Add(new Point(1, 0));
            Point3DCollection asd = new Point3DCollection();
            asd.Add(new Point3D(-1, 1, 0));
            asd.Add(new Point3D(-1, -1, 0));
            asd.Add(new Point3D(1, -1, 0));
            asd.Add(new Point3D(1, 1, 0));
            Int32Collection TIndex = new Int32Collection();
            TIndex.Add(0);
            TIndex.Add(1);
            TIndex.Add(2);
            TIndex.Add(0);
            TIndex.Add(2);
            TIndex.Add(3);
            mesh.Positions = asd;
            mesh.TriangleIndices = TIndex;
            mesh.TextureCoordinates = textures;
            v2dv3d.Geometry = mesh;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ListWindows();
        }

        public void ChangeTransformAxis()
        {
            RotateTransform3D Rotation = new RotateTransform3D();
            AxisAngleRotation3D RAxis = new AxisAngleRotation3D();
            RAxis.Axis = new Vector3D(0, 1, 0);
            RAxis.Angle = slider.Value;
            Rotation.Rotation = RAxis;
            v2dv3d.Transform = Rotation;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChangeTransformAxis();
        }

        public void ChangeWindow()
        {
            int handle;
            Int32.TryParse(tb.Text, out handle);
            if (String.IsNullOrWhiteSpace(tb.Text)){
                hWnd = IntPtr.Zero;
            }
            else
                hWnd = (IntPtr)handle;
            WinCtl.Rect rc = new WinCtl.Rect();
            WinCtl.GetWindowRect(hWnd, ref rc);
            WinCtl img = new WinCtl(hWnd, rc);
            v2dv3d.Visual = img;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }



        public void ListWindows()
        {
            IntPtr thishWnd = new WindowInteropHelper(this).Handle;
            winList.Items.Clear();
            var hWnds = new List<IntPtr>();
            WinUtils.EnumDelegate filter = delegate(IntPtr hWnd, int lParam)
            {
                string[] m_astrFilter = new string[] { "Start", "Program Manager" };
                string title = WinUtils.GetWindowTitle(hWnd);
                if (WinUtils.IsWindowVisible(hWnd) && !string.IsNullOrEmpty(title) && !m_astrFilter.Contains(title) && hWnd != thishWnd)
                    hWnds.Add(hWnd);
                return true;
            };

            if (WinUtils.EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero))
            {
                foreach (var hWnd in hWnds)
                {
                    ComboBoxItem win = new ComboBoxItem();
                    win.Tag = hWnd;
                    win.Content = WinUtils.GetWindowTitle(hWnd);
                    winList.Items.Add(win);
                }
                winList.SelectedIndex = 0;
            }
        }

        private void winList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (winList.SelectedItem as ComboBoxItem != null)
            {
                tb.Text = (winList.SelectedItem as ComboBoxItem).Tag.ToString();
                ChangeWindow();
            }

        }
    }

    public class WinUtils
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        public struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public static ImageSource ImageSourceFromBitmap(FormsDraw.Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        public static string GetWindowTitle(IntPtr hWnd)
        {
            StringBuilder strbTitle = new StringBuilder(255);
            int nLength = WinUtils.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
            string strTitle = strbTitle.ToString();
            return strTitle;
        }

    }
}

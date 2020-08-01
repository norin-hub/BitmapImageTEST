using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;

namespace Win32APIBitmap
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapfileheader
        [StructLayout(LayoutKind.Sequential,Pack = 1)]
        struct BITMAPFILEHEADER
        {
            public ushort bftype;
            public uint bfSize;
            public ushort bfReserved1;
            public ushort bfReserved2;
            public uint bfOffBits;
        }

        //BITMAPINFOHEADER
        [StructLayout(LayoutKind.Sequential,Pack = 1)]
        struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-loadimagea
        [DllImport("USER32.DLL")]
        extern static IntPtr LoadImage(
            IntPtr hInst,
            string name,
            uint type,
            int cx,
            int cy,
            uint fuLoad
        );

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
            CAPTUREBLT = 0x40000000
        }
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight,
          IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        static extern int StretchBlt(IntPtr hDestDC,
            int nLeftDest, int nTopDest,
            int nWidthDest, int nHeightDest,
            IntPtr hDCSrc,
            int nLeftSrc, int nTopSrc,
            int nWidthSrc, int nHeightSrc,
            int dwRop);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdC,int cx,int cy);

        //TEST
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string fname = @"D:\movie\壁紙\test.bmp";

            //BITMAPINFOHEADER head = new BITMAPINFOHEADER();

            //IntPtr hBmp = LoadImage(IntPtr.Zero, fname, 0, 0, 0, 0x00000010);
            //IntPtr hDCBmp = CreateCompatibleDC(hBmp);
            //SelectObject(hDCBmp, hBmp);

            using(var bmp = new Bitmap(500,500,System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            using(var g = Graphics.FromImage(bmp))
            {
                IntPtr hdc = g.GetHdc();//Bitmapのデバイスコンテキスト
                IntPtr hdcm = CreateCompatibleDC(hdc);
                //IntPtr screendc = GetDC(hdc);
                //IntPtr hcompatibledc = CreateCompatibleDC(screendc);
                //IntPtr 

                IntPtr hBmp = LoadImage(IntPtr.Zero, fname, 0, 0, 0, 0x00000010);
                //IntPtr hcompatibledc = CreateCompatibleDC(hBmp);
                SelectObject(hdcm, hBmp);
                //IntPtr hBmpDC = GetDC(hBmp);
                


                BitBlt(hdc, 0, 0, 500, 500, hdcm, 0, 0, TernaryRasterOperations.SRCCOPY);

                //StretchBlt(hdc, 0, 0, 500, 500, hcompatibledc, 0, 0, 500, 500, (int)TernaryRasterOperations.SRCCOPY);

                var image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Img1.Source = image;
            }



            //DeleteDC(hDCBmp);


        }

        //Bitmap→BitmapSource
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            string fname = @"D:\movie\壁紙\test.bmp";
            Bitmap bmp = new Bitmap(fname);
            var image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            Img1.Source = image;
        }

        //FileStreamとBITMAPINFOHEADERを使ってDIB画像を直接読み込む
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            string fname = @"D:\movie\壁紙\test.bmp";
            var fs = new FileStream(fname, FileMode.Open);

            var reader = new BinaryReader(fs);

            byte[] buf = reader.ReadBytes((int)fs.Length);

            reader.Dispose();

            fs.Close();

            IntPtr ptr = Marshal.AllocCoTaskMem(buf.Length);
            Marshal.Copy(buf, 0, ptr, buf.Length);

            IntPtr tmpptr = new IntPtr(ptr.ToInt32());

            BITMAPFILEHEADER fheader = Marshal.PtrToStructure<BITMAPFILEHEADER>(tmpptr);

            tmpptr = IntPtr.Add(tmpptr, Marshal.SizeOf(fheader));

            BITMAPINFOHEADER iheader = Marshal.PtrToStructure<BITMAPINFOHEADER>(tmpptr);

            tmpptr = IntPtr.Add(tmpptr, Marshal.SizeOf(iheader));

            int stride = iheader.biWidth * (iheader.biBitCount / 8);

            var src = BitmapSource.Create(iheader.biWidth, iheader.biHeight, iheader.biXPelsPerMeter, iheader.biYPelsPerMeter,
                PixelFormats.Bgr24, null, tmpptr, (int)iheader.biSizeImage, stride);

            Img1.Source = src;
        }

        //FileStreamとBITMAPINFOHEADERを使ってDIB画像を保存する(0.0111148)
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            BitmapSource src = Img1.Source as BitmapSource;

            int stride = src.PixelWidth * (src.Format.BitsPerPixel / 8);

            int size = src.PixelHeight * stride;

            byte[] buf = new byte[size];

            src.CopyPixels(buf, stride, 0);

            string savef = "test.bmp";
            using(var fs = new FileStream(savef, FileMode.Create))
            using(var writer = new BinaryWriter(fs))
            {
                BITMAPFILEHEADER fhead = new BITMAPFILEHEADER();
                fhead.bftype = 19778;
                fhead.bfSize = (uint)(size + 54);
                fhead.bfOffBits = 54;

                byte[] buffhead = new byte[Marshal.SizeOf(fhead)];
                IntPtr pfhead = Marshal.AllocCoTaskMem(Marshal.SizeOf(fhead));
                Marshal.StructureToPtr<BITMAPFILEHEADER>(fhead, pfhead, false);
                Marshal.Copy(pfhead, buffhead, 0, buffhead.Length);

                writer.Write(buffhead);

                BITMAPINFOHEADER ihead = new BITMAPINFOHEADER();
                ihead.biSize = 40;
                ihead.biWidth = src.PixelWidth;
                ihead.biHeight = src.PixelHeight;
                ihead.biPlanes = 1;
                ihead.biBitCount = (ushort)src.Format.BitsPerPixel;
                ihead.biSizeImage = (uint)size;
                ihead.biXPelsPerMeter = (int)src.DpiX;
                ihead.biYPelsPerMeter = (int)src.DpiY;

                byte[] bufihead = new byte[Marshal.SizeOf(ihead)];
                IntPtr pihead = Marshal.AllocCoTaskMem(Marshal.SizeOf(ihead));
                Marshal.StructureToPtr<BITMAPINFOHEADER>(ihead, pihead, false);
                Marshal.Copy(pihead, bufihead, 0, bufihead.Length);

                writer.Write(bufihead);

                writer.Write(buf);

            }

            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalSeconds.ToString());
        }

        //BmpEncoderで保存する(0.2341903)
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            BitmapSource src = Img1.Source as BitmapSource;
            using (var fs = new FileStream("test.bmp", FileMode.Create))
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(src));
                enc.Save(fs);
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalSeconds.ToString());
        }
    }
}

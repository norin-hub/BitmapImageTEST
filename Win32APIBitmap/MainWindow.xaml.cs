/*
 * 補足：
 * test1.bmp    1920x1080   24bit
 * test2.bmp    11520x6480  24bit
 * 
 * それぞれ速度計測
 */ 
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

        #region "定義"
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

        public class TM
        {
            private static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            public static void start()
            {
                sw.Restart();
            }

            public static void show(string msg = "")
            {
                Console.WriteLine("{0:0.000} {1}", sw.Elapsed.TotalSeconds,msg);
            }

            public static void stop(string msg = "")
            {
                sw.Stop();
                Console.WriteLine("{0:0.000} {1}", sw.Elapsed.TotalSeconds,msg);
            }
        }
        #endregion

        //TEST1
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

        //TEST2 Bitmap→BitmapSource
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            string fname = @"D:\movie\壁紙\test.bmp";
            Bitmap bmp = new Bitmap(fname);
            var image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            Img1.Source = image;
        }

        //TEST3 FileStreamとBITMAPINFOHEADERを使ってDIB画像を直接読み込む(0.0120069)(2.2738832)
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            string fname = @"D:\movie\壁紙\test2.bmp";
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

            ChangeUpDown(tmpptr, iheader);

            int stride = iheader.biWidth * (iheader.biBitCount / 8);

            var src = BitmapSource.Create(iheader.biWidth, iheader.biHeight, iheader.biXPelsPerMeter, iheader.biYPelsPerMeter,
                PixelFormats.Bgr24, null, tmpptr, (int)iheader.biSizeImage, stride);

            Img1.Source = src;

            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalSeconds.ToString());
        }

        //TEST4 FileStreamとBITMAPINFOHEADERを使ってDIB画像を保存する(0.0111148)(1.6200039)
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
                fhead.bfOffBits = 54;//gary →  1078

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
                ihead.biBitCount = (ushort)src.Format.BitsPerPixel;//gray → 8
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

        //TEST5 BmpEncoderで保存する(24bitは24bit 32bitは32bitで保存)(0.2341903)(8.2563395)
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

        //TEST6 グレースケール画像をBITMAPINFOHEADERを使って読み込む
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            string fname = @"D:\movie\壁紙\test_gray.bmp";
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

            //画像オフセットの位置がカラーと違うので注意
            tmpptr = new IntPtr(ptr.ToInt32() + fheader.bfOffBits);

            ChangeUpDown(tmpptr, iheader);

            int stride = iheader.biWidth * (iheader.biBitCount / 8);
            int size = stride * iheader.biHeight;

            var src = BitmapSource.Create(iheader.biWidth, iheader.biHeight, 600, 600,
                PixelFormats.Gray8, null, tmpptr, size, stride);

            Img1.Source = src;
        }

        //TEST7 BitmapImageで画像を読み込む(0.0239597)(0.0245081) 読み込みはBitmapImageが最高
        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            string fname = @"D:\movie\壁紙\test2.bmp";
            BitmapImage img = new BitmapImage(new Uri(fname));
            Img1.Source = img;

            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalSeconds.ToString());
        }

        //上下反転処理
        private void ChangeUpDown(IntPtr ptr,BITMAPINFOHEADER ihead)
        {
            int pxlsize = (ihead.biBitCount / 8);
            int stride = ihead.biWidth * pxlsize;

            IntPtr p = new IntPtr(ptr.ToInt32());
            IntPtr p2 = new IntPtr(ptr.ToInt32() +  stride * (ihead.biHeight-1));

            int harfy = ihead.biHeight / 2;

            byte[] tmpb = new byte[3];

            for(int y = 0; y < harfy; y++)
            {
                for(int x = 0; x < ihead.biWidth; x++)
                {
                    for(int i = 0; i < pxlsize; i++)
                    {
                        tmpb[i] = Marshal.ReadByte(p, i);
                        Marshal.WriteByte(p, i, Marshal.ReadByte(p2, i));
                        Marshal.WriteByte(p2, i, tmpb[i]);
                    }
                    p = IntPtr.Add(p, pxlsize);
                    p2 = IntPtr.Add(p2, pxlsize);
                }
                p2 = IntPtr.Add(p2, stride * -2);
            }
        }

        //TEST8 BITMAPINFOを使って24bit→32bitフォーマットに変換して読み込む(0.0728768)(2.4672905)
        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            string fname = @"D:\movie\壁紙\test2.bmp";
            var fs = new FileStream(fname, FileMode.Open);

            var reader = new BinaryReader(fs);

            //ヘッダ情報読み取り
            byte[] buf1 = reader.ReadBytes(Marshal.SizeOf<BITMAPFILEHEADER>() + Marshal.SizeOf<BITMAPINFOHEADER>());
            IntPtr ptr = Marshal.AllocCoTaskMem(buf1.Length);
            IntPtr tmpptr = new IntPtr(ptr.ToInt64());
            Marshal.Copy(buf1, 0, ptr, buf1.Length);
            BITMAPFILEHEADER fheader = Marshal.PtrToStructure<BITMAPFILEHEADER>(tmpptr);
            tmpptr = IntPtr.Add(tmpptr, Marshal.SizeOf(fheader));
            BITMAPINFOHEADER iheader = Marshal.PtrToStructure<BITMAPINFOHEADER>(tmpptr);
            Marshal.FreeCoTaskMem(ptr);

            //画像バッファ読み取り 24bit→32bit変換(画素を下から読み取り反転を防ぐ)
            int pxlsize = 4;
            int stride = iheader.biWidth * pxlsize;
            int size = stride * iheader.biHeight;

            byte[] buf2 = new byte[size];
            for(int y = iheader.biHeight-1; y > 0 ; y--)
            {
                for(int x = 0; x < iheader.biWidth; x++)
                {
                    int pos = y * stride + (x * 4);

                    buf2[pos + 0] = reader.ReadByte();
                    buf2[pos + 1] = reader.ReadByte();
                    buf2[pos + 2] = reader.ReadByte();
                    buf2[pos + 3] = 0;
                }
            }

            reader.Dispose();
            fs.Close();

            var src = BitmapSource.Create(iheader.biWidth, iheader.biHeight, 600, 600,
                PixelFormats.Bgr32, null, buf2, stride);

            Img1.Source = src;

            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalSeconds.ToString());
        }

        //TEST9 BITMAPINFOHEADERを使ってDIB画像を保存する(32bitのまま)(1.8004037)
        private void MenuItem_Click_8(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            BitmapSource src = Img1.Source as BitmapSource;

            int stride = src.PixelWidth * (src.Format.BitsPerPixel / 8);

            int size = src.PixelHeight * stride;

            byte[] buf = new byte[size];

            src.CopyPixels(buf, stride, 0);

            string savef = "test.bmp";
            using (var fs = new FileStream(savef, FileMode.Create))
            using (var writer = new BinaryWriter(fs))
            {
                BITMAPFILEHEADER fhead = new BITMAPFILEHEADER();
                fhead.bftype = 19778;
                fhead.bfSize = (uint)(size + 54);
                fhead.bfOffBits = 54;//gary →  1078

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
                ihead.biBitCount = (ushort)src.Format.BitsPerPixel;//gray → 8
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

        //TEST10 BITMAPINFOを使って32bit→24bitフォーマットに変換してさらに上下反転して書き込む(2.161)
        private void MenuItem_Click_9(object sender, RoutedEventArgs e)
        {
            //書き込み用に24bitの画像バッファが必要になったためメモリ消費がやばそう　このPCだと余裕だが？

            MenuItem_Click_6(null, null);//画像を開く

            TM.start();

            BitmapSource src = Img1.Source as BitmapSource;
            int pxlsize = 4;
            int stride = src.PixelWidth * pxlsize;
            int size = src.PixelHeight * stride;

            BITMAPFILEHEADER fhead = new BITMAPFILEHEADER();
            fhead.bftype = 19778;
            fhead.bfSize = (uint)(size + 54);
            fhead.bfOffBits = 54;//gary →  1078
            BITMAPINFOHEADER ihead = new BITMAPINFOHEADER();
            ihead.biSize = 40;
            ihead.biWidth = src.PixelWidth;
            ihead.biHeight = src.PixelHeight;
            ihead.biPlanes = 1;
            ihead.biBitCount = 24;//gray → 8
            ihead.biSizeImage = (uint)size;
            ihead.biXPelsPerMeter = (int)src.DpiX;
            ihead.biYPelsPerMeter = (int)src.DpiY;

            byte[] fheadbuf = new byte[Marshal.SizeOf(fhead)];
            GCHandle gch = GCHandle.Alloc(fheadbuf, GCHandleType.Pinned);
            Marshal.StructureToPtr(fhead, gch.AddrOfPinnedObject(), false);
            gch.Free();

            byte[] iheadbuf = new byte[Marshal.SizeOf(ihead)];
            gch = GCHandle.Alloc(iheadbuf, GCHandleType.Pinned);
            Marshal.StructureToPtr(ihead, gch.AddrOfPinnedObject(), false);
            gch.Free();

            TM.show("1");

            byte[] buf = new byte[size];

            src.CopyPixels(buf, stride, 0);

            TM.show("2");

            int size24 = ihead.biHeight * (ihead.biWidth * 3);
            byte[] wbuf = new byte[size24];
            for (int y = 0,y2 = ihead.biHeight-1; y < ihead.biHeight; y++ , y2--)
            {
                for (int x = 0; x < ihead.biWidth; x++)
                {
                    int pos = y2 * (ihead.biWidth * 3) + (x * 3);
                    int pos2 = y * (ihead.biWidth * 4) + (x * 4);

                    wbuf[pos + 0] = buf[pos2 + 0];
                    wbuf[pos + 1] = buf[pos2 + 1];
                    wbuf[pos + 2] = buf[pos2 + 2];
                }
            }
            TM.show("3");
            using (var fs = new FileStream("test.bmp", FileMode.Create))
            {
                fs.Write(fheadbuf, 0, fheadbuf.Length);
                fs.Write(iheadbuf, 0, iheadbuf.Length);
                fs.Write(wbuf, 0, wbuf.Length);
            }

            TM.stop();
        }

        //TEST10 BITMAPINFOを使って32bit→24bitフォーマットに変換してさらに上下反転して書き込む(改良)(1.851)
        private void MenuItem_Click_10(object sender, RoutedEventArgs e)
        {
            //BITMAPINFOHEADERの高さをマイナスにすることで簡単に反転して保存できる
            //メモリ消費も減った

            TM.start();

            BitmapSource src = Img1.Source as BitmapSource;
            int width = src.PixelWidth;
            int height = src.PixelHeight;
            int pxlsize = 4;
            int stride = width * pxlsize;
            int size = height * stride;

            BITMAPFILEHEADER fhead = new BITMAPFILEHEADER();
            fhead.bftype = 19778;
            fhead.bfSize = (uint)(size + 54);
            fhead.bfOffBits = 54;//gary →  1078
            BITMAPINFOHEADER ihead = new BITMAPINFOHEADER();
            ihead.biSize = 40;
            ihead.biWidth = width;
            ihead.biHeight = -height;
            ihead.biPlanes = 1;
            ihead.biBitCount = 24;//gray → 8
            ihead.biSizeImage = (uint)size;
            ihead.biXPelsPerMeter = 0;
            ihead.biYPelsPerMeter = 0;

            byte[] fheadbuf = new byte[Marshal.SizeOf(fhead)];
            GCHandle gch = GCHandle.Alloc(fheadbuf, GCHandleType.Pinned);
            Marshal.StructureToPtr(fhead, gch.AddrOfPinnedObject(), false);
            gch.Free();

            byte[] iheadbuf = new byte[Marshal.SizeOf(ihead)];
            gch = GCHandle.Alloc(iheadbuf, GCHandleType.Pinned);
            Marshal.StructureToPtr(ihead, gch.AddrOfPinnedObject(), false);
            gch.Free();

            TM.show("1");

            byte[] buf = new byte[size];

            src.CopyPixels(buf, stride, 0);

            TM.show("2");

            int pos, pos2;
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    pos = y * (width * 3) + (x * 3);
                    pos2 = y * (height * 4) + (x * 4);

                    buf[pos + 0] = buf[pos2 + 0];
                    buf[pos + 1] = buf[pos2 + 1];
                    buf[pos + 2] = buf[pos2 + 2];
                }
            }
            TM.show("3");
            using (var fs = new FileStream("test.bmp", FileMode.Create))
            {
                fs.Write(fheadbuf, 0, fheadbuf.Length);
                fs.Write(iheadbuf, 0, iheadbuf.Length);
                fs.Write(buf, 0, src.PixelWidth * src.PixelHeight * 3);
            }
            TM.stop();
        }
    }
}

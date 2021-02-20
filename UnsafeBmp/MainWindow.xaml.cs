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

using BMP;
using System.IO;
using System.Runtime.InteropServices;

namespace UnsafeBmp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //アンセーフを使って画像を開く(反転処理なし) 測定停止 0.0095
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MM2.TStart();

            using (var fs = new FileStream("test.bmp", FileMode.Open))
            using (var reader = new BinaryReader(fs))
            {
                byte[] buf = reader.ReadBytes((int)fs.Length);

                unsafe
                {
                    fixed (byte* pbuf = buf)
                    {
                        byte* pbuf2 = pbuf;

                        BITMAPFILEHEADER* fheader = (BITMAPFILEHEADER*)pbuf2;
                        pbuf2 += sizeof(BITMAPFILEHEADER);
                        BITMAPINFOHEADER* iheader = (BITMAPINFOHEADER*)pbuf2;
                        pbuf2 += sizeof(BITMAPINFOHEADER);

                        //sizeof(BITMAPFILEHEADER)
                        int stride = iheader->biWidth * (iheader->biBitCount / 8);

                        IntPtr tmpptr = (IntPtr)pbuf2;
                        var src = BitmapSource.Create(iheader->biWidth, iheader->biHeight, iheader->biXPelsPerMeter, iheader->biYPelsPerMeter,
                         PixelFormats.Bgr24, null, tmpptr, (int)iheader->biSizeImage, stride);

                        Img1.Source = src;
                    }
                }

                MM2.TStop();
            }
        }

        //アンセーフを使って画像を開く(反転処理あり) 測定停止 0.0214
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            MM2.TStart();

            using (var fs = new FileStream(Define.fname1, FileMode.Open))
            using (var reader = new BinaryReader(fs))
            {
                unsafe
                {
                    //BITMAPヘッダ読み込み
                    int headersize = sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER);
                    byte[] headerbuf = reader.ReadBytes(headersize);

                    BITMAPFILEHEADER* fheader;
                    BITMAPINFOHEADER* iheader;

                    fixed (byte* hbuf = headerbuf)
                    {
                        byte* hbuf2 = hbuf;
                        fheader = (BITMAPFILEHEADER*)hbuf2;
                        hbuf2 += sizeof(BITMAPFILEHEADER);
                        iheader = (BITMAPINFOHEADER*)hbuf2;
                    }

                    //画像データ読み込み
                    int stride = iheader->biWidth * (iheader->biBitCount / 8);

                    byte[] imgbuf = new byte[stride * iheader->biHeight];
                    for (int j = iheader->biHeight - 1; j >= 0; j--)
                    {
                        fs.Read(imgbuf, j * stride, stride);
                    }

                    var src = BitmapSource.Create(iheader->biWidth, iheader->biHeight, iheader->biXPelsPerMeter, iheader->biYPelsPerMeter,
                     PixelFormats.Bgr24, null, imgbuf, stride);

                    Img1.Source = src;
                }

                MM2.TStop();

            }
        }

        //アンセーフを使って画像を開く(反転処理あり32bit変換) 測定停止 0.0895
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            MM2.TStart();

            using (var fs = new FileStream(Define.fname1, FileMode.Open))
            using (var reader = new BinaryReader(fs))
            {
                unsafe
                {
                    //BITMAPヘッダ読み込み
                    int headersize = sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER);
                    byte[] headerbuf = reader.ReadBytes(headersize);

                    BITMAPFILEHEADER* fheader;
                    BITMAPINFOHEADER* iheader;

                    fixed (byte* hbuf = headerbuf)
                    {
                        byte* hbuf2 = hbuf;
                        fheader = (BITMAPFILEHEADER*)hbuf2;
                        hbuf2 += sizeof(BITMAPFILEHEADER);
                        iheader = (BITMAPINFOHEADER*)hbuf2;
                    }

                    //画像データ読み込み
                    int stride = iheader->biWidth * (iheader->biBitCount / 8);
                    int stride2 = iheader->biWidth * 4;

                    int size = stride * iheader->biHeight;
                    int size2 = stride2 * iheader->biHeight;

                    IntPtr imgbufptr = Marshal.AllocCoTaskMem(size2);
                    byte* imgbuf = (byte*)imgbufptr;

                    byte[] tmpbuf = new byte[3];
        
                    for(int y=iheader->biHeight - 1; y >= 0; y--)
                    {
                        for(int x = 0; x < iheader->biWidth; x++)
                        {
                            fs.Read(tmpbuf, 0, 3);
                            int pos = (y * stride2) + (x * 4);

                            imgbuf[pos] = tmpbuf[0];
                            imgbuf[pos+1] = tmpbuf[1];
                            imgbuf[pos+2] = tmpbuf[2];
                            imgbuf[pos+3] = 0;
                        }
                    }

                    var src = BitmapSource.Create(iheader->biWidth, iheader->biHeight, iheader->biXPelsPerMeter, iheader->biYPelsPerMeter,
                     PixelFormats.Bgr32, null, (IntPtr)imgbuf, size2, stride2);

                    Img1.Source = src;

                    Marshal.FreeCoTaskMem(imgbufptr);
                }

                MM2.TStop();

            }
        }

        //保存24bit変換反転(アンセーフ)測定停止 0.0537
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            BitmapSource bmp = Img1.Source as BitmapSource;
            if(bmp == null)
            {
                MessageBox.Show("画像が開かれていません");
                return;
            }

            int stride = bmp.PixelWidth * 4;
            int size = stride * bmp.PixelHeight;
            byte[] ImgBuf = new byte[size];
            bmp.CopyPixels(ImgBuf, stride, 0);

            //Buffer.BlockCopy(ImgBuf, 0, ImgBuf, stride, size);

            MM2.TStart();

            string newfname = "test.bmp";

            int stride2 = bmp.PixelWidth * 3;
            int size2 = stride2 * bmp.PixelHeight;
            BITMAPFILEHEADER fhead = new BITMAPFILEHEADER();
            fhead.bftype = 19778;
            fhead.bfReserved1 = 0;
            fhead.bfReserved2 = 0;
            fhead.bfOffBits = 54;
            fhead.bfSize = (uint)(fhead.bfOffBits + size2);
            BITMAPINFOHEADER ihead = new BITMAPINFOHEADER();
            ihead.biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>();
            ihead.biWidth = bmp.PixelWidth;
            ihead.biHeight = bmp.PixelHeight;
            ihead.biPlanes = 1;
            ihead.biBitCount = 24;
            ihead.biSizeImage = (uint)size2;
            ihead.biXPelsPerMeter = 3780;
            ihead.biYPelsPerMeter = 3780;

            byte[] ImgBuf2 = new byte[fhead.bfSize];
            using (var fs = new FileStream(newfname, FileMode.Create))
            {
                unsafe
                {
                    fixed (byte* pImgBuf = ImgBuf)
                    {
                        fixed (byte* pImgBuf2 = ImgBuf2)
                        {
                            byte* ibuf = pImgBuf;
                            byte* ibuf2 = pImgBuf2;
                            byte* tmpibuf;

                            *((BITMAPFILEHEADER*)ibuf2) = fhead;
                            ibuf2 += sizeof(BITMAPFILEHEADER);
                            *((BITMAPINFOHEADER*)ibuf2) = ihead;
                            ibuf2 += sizeof(BITMAPINFOHEADER);

                            //for (int y = 0; y < bmp.PixelHeight; y++)
                            for (int y = bmp.PixelHeight-1; y >= 0; y--)
                            {
                                tmpibuf = ibuf + (y * stride);
                                for (int x = 0; x < bmp.PixelWidth; x++)
                                {
                                    *ibuf2 = *tmpibuf;
                                    ibuf2++;tmpibuf++;
                                    *ibuf2 = *tmpibuf;
                                    ibuf2++; tmpibuf++;
                                    *ibuf2 = *tmpibuf;
                                    ibuf2++; tmpibuf++;
                                    tmpibuf++;
                                }
                            }
                        }
                    }

                    //writer.Write(ImgBuf);
                    fs.Write(ImgBuf2, 0, (int)fhead.bfSize);
                }
                ImgBuf = null;
            }
            //Marshal.FreeCoTaskMem(ptr);
            MM2.TStop();
        }

        //保存 BitmapSource 測定停止 0.2660
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            BitmapSource bmp = Img1.Source as BitmapSource;
            if (bmp == null)
            {
                MessageBox.Show("画像が開かれていません");
                return;
            }

            MM2.TStart();

            int stride = bmp.PixelWidth * 4;
            int size = stride * bmp.PixelHeight;
            byte[] ImgBuf = new byte[size];
            bmp.CopyPixels(ImgBuf, stride, 0);

            int stride2 = bmp.PixelWidth * 3;
            int size2 = stride2 * bmp.PixelHeight;
            byte[] ImgBuf2 = new byte[size2];

            int pos1 = 0;
            int pos2 = 0;
            for (int y = bmp.PixelHeight - 1; y >= 0; y--)
            {
                //pos1 = (y * stride);
                for (int x = 0; x < bmp.PixelWidth; x++)
                {
                    ImgBuf2[pos2] = ImgBuf[pos1];
                    ImgBuf2[pos2+1] = ImgBuf[pos1+1];
                    ImgBuf2[pos2+2] = ImgBuf[pos1+2];

                    pos1 += 4;
                    pos2 += 3;
                }
            }

            var bmp2 = BitmapSource.Create(bmp.PixelWidth, bmp.PixelHeight, 600, 600, PixelFormats.Bgr24, null, ImgBuf2, stride2);


            using (var fs = new FileStream("test1.bmp", FileMode.Create))
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bmp2));
                enc.Save(fs);
            }

            MM2.TStop();
        }
    }
}

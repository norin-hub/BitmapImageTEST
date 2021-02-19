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

            using (var fs = new FileStream(Define.fname1, FileMode.Open))
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
    }
}

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
    }
}

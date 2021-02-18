using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace BMP
{
    //https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapfileheader
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BITMAPFILEHEADER
    {
        public ushort bftype;
        public uint bfSize;
        public ushort bfReserved1;
        public ushort bfReserved2;
        public uint bfOffBits;
    }

    //BITMAPINFOHEADER  https://docs.microsoft.com/en-us/previous-versions/dd183376(v=vs.85)
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
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

    public static class Define
    {
        //1980 x 1080 24bit Bitmap画像
        public const string fname1 = @"D:\movie\壁紙\test.bmp";


    }
}

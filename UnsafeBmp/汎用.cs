/*
 * 動作検証のための汎用処理クラス
 * 
 * 2021/02/14       新規
 */ 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

public static class MM2
{
    #region "時間測定"
    public static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    //測定開始
    public static void TStart()
    {
        sw.Restart();
        Console.WriteLine("測定開始");
    }
    //経過時間表示
    public static void TCheck()
    {
        Console.WriteLine("経過時間 {0:0.0000}", sw.Elapsed.TotalSeconds);
    }
    //経過時間表示＋タイマーストップ
    public static void TStop()
    {
        sw.Stop();
        Console.WriteLine("測定停止 {0:0.0000}", sw.Elapsed.TotalSeconds);
    }
    #endregion

    #region "バッファ(IntPtr)"
    public static byte[] buf;
    public static GCHandle gh;

    public const int width = 15000;
    public const int height = 20000;
    public const int pxlsize = 4;
    public static int size
    {
        get { return width * height * pxlsize; }
    }

    public static IntPtr ptr
    {
        get { return gh.AddrOfPinnedObject(); }
    }

    public static void Alloc(bool usegh = true)
    {
        buf = new byte[size];
        if (usegh)
        {
            gh = GCHandle.Alloc(buf, GCHandleType.Pinned);
        }

        Console.WriteLine("サイズ {0}", size);
    }

    public static void Free()
    {
        gh.Free();
    }
    #endregion

    #region "使用メモリ"
    private static long startsize = -1;
    //使用メモリ計測開始
    public static void MemStart()
    {
        long currentSet = Environment.WorkingSet;
        startsize = currentSet;
        Console.WriteLine(string.Format("使用メモリ計測開始 ={0}byte", currentSet.ToString("N0")));
    }
    //開始から変化した使用メモリ表示
    public static void MemCheck(string comment="")
    {
        long currentSet = Environment.WorkingSet;
        long keika = currentSet - startsize;
        Console.WriteLine(string.Format("使用メモリ確認 ={0}byte {1} {2}", currentSet.ToString("N0")
            , keika.ToString("N0"),comment));
    }
    #endregion
}

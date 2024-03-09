using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DialogGeneratorServer
{
    internal class ScreenCapture
    {
        #region Win32

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, uint flags);

        private const uint DCX_WINDOW = 0x00000001;

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private const int SRCCOPY = 0xCC0020;

        [DllImport("dwmapi.dll")]
        private static extern long DwmGetWindowAttribute(IntPtr hWnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT rect, int cbAttribute);

        // ウィンドウ属性
        // 列挙値の開始は0だとずれていたので1からにした
        private enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,//ウィンドウのRect
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        };
        #endregion

        /// <summary>
        /// ウィンドウキャプチャ
        /// </summary>
        /// <param name="hWnd">対象ウィンドウ</param>
        /// <returns>キャプチャしたBitmap</returns>
        public static Bitmap CaptureWindow(IntPtr hWnd)
        {
            // ウィンドウを取得
            DwmGetWindowAttribute(hWnd,
                                  DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
                                  out RECT bounds,
                                  Marshal.SizeOf(typeof(RECT)));
            // Rect取得
            GetWindowRect(hWnd, out RECT rect);

            // ウィンドウDCからコピー
            int width = bounds.right - bounds.left;
            int height = bounds.bottom - bounds.top;
            int offsetX = bounds.left - rect.left;
            int offsetY = bounds.top - rect.top;

            //IntPtr winDC = GetWindowDC(hWnd);
            IntPtr winDC = GetDCEx(hWnd, IntPtr.Zero, DCX_WINDOW);
            Bitmap bmp = new Bitmap(width, height);

            using (var g = Graphics.FromImage(bmp))
            {
                IntPtr hDC = g.GetHdc();
                // ビットブロック転送、コピー元からコピー先へ
                BitBlt(hDC, 0, 0, width, height, winDC, offsetX, offsetY, SRCCOPY);
                g.ReleaseHdc(hDC);
            }

            // 後片付け
            ReleaseDC(hWnd, winDC);

            return bmp;
        }
    }
}

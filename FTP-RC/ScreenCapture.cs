using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FTP_RC
{
    public static class ScreenCapture
    {
        [DllImport("GDI32.dll")]
        private static extern bool BitBlt(int hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, int hdcSrc, int nXSrc, int nYSrc, int dwRop);
        [DllImport("GDI32.dll")]
        private static extern int CreateCompatibleBitmap(int hdc, int nWidth, int nHeight);
        [DllImport("GDI32.dll")]
        private static extern int CreateCompatibleDC(int hdc);
        [DllImport("GDI32.dll")]
        private static extern bool DeleteDC(int hdc);
        [DllImport("GDI32.dll")]
        private static extern bool DeleteObject(int hObject);
        [DllImport("GDI32.dll")]
        private static extern int GetDeviceCaps(int hdc, int nIndex);
        [DllImport("GDI32.dll")]
        private static extern int SelectObject(int hdc, int hgdiobj);
        [DllImport("User32.dll")]
        private static extern int GetDesktopWindow();
        [DllImport("User32.dll")]
        private static extern int GetWindowDC(int hWnd);
        [DllImport("User32.dll")]
        private static extern int ReleaseDC(int hWnd, int hDC);

        public static MemoryStream CaptureScreen(ImageFormat imageFormat, int MaxSideSize)
        {
            int hdcSrc = GetWindowDC(GetDesktopWindow()),
            hdcDest = CreateCompatibleDC(hdcSrc),
            hBitmap = CreateCompatibleBitmap(hdcSrc, GetDeviceCaps(hdcSrc, 8), GetDeviceCaps(hdcSrc, 10));
            SelectObject(hdcDest, hBitmap);
            BitBlt(hdcDest, 0, 0, GetDeviceCaps(hdcSrc, 8),
            GetDeviceCaps(hdcSrc, 10), hdcSrc, 0, 0, 0x00CC0020);
            MemoryStream returnme = SaveImageAs(hBitmap, imageFormat, MaxSideSize);
            Cleanup(hBitmap, hdcSrc, hdcDest);
            return returnme;
        }
        private static MemoryStream SaveImageAs(int hBitmap, ImageFormat imageFormat, int MaxSideSize)
        {
            MemoryStream returnme = new MemoryStream();
            int intNewWidth;
            int intNewHeight;
            Image imgInput = Image.FromHbitmap(new IntPtr(hBitmap));

            //get image original width and height
            int intOldWidth = imgInput.Width;
            int intOldHeight = imgInput.Height;

            //determine if landscape or portrait
            int intMaxSide;

            if (intOldWidth >= intOldHeight)
            {
                intMaxSide = intOldWidth;
            }
            else
            {
                intMaxSide = intOldHeight;
            }


            if (intMaxSide > MaxSideSize)
            {
                //set new width and height
                double dblCoef = MaxSideSize / (double)intMaxSide;
                intNewWidth = Convert.ToInt32(dblCoef * intOldWidth);
                intNewHeight = Convert.ToInt32(dblCoef * intOldHeight);
            }
            else
            {
                intNewWidth = intOldWidth;
                intNewHeight = intOldHeight;
            }
            //create new bitmap
            Bitmap bmpResized = new Bitmap(imgInput, intNewWidth, intNewHeight);

            //           byte[] buffer;
            //save bitmap to disk
            bmpResized.Save(returnme, imageFormat);

            //release used resources
            imgInput.Dispose();
            bmpResized.Dispose();
            return returnme;
        }
        private static void Cleanup(int hBitmap, int hdcSrc, int hdcDest)
        {
            ReleaseDC(GetDesktopWindow(), hdcSrc);
            DeleteDC(hdcDest);
            DeleteObject(hBitmap);
        }
    }
}

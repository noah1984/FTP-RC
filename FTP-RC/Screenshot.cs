// MIT License
// 
// Copyright (c) 2017 Noah Allen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace noah1984.FtpRc
{
    // This class contains methods pertaining to capturing a screenshot from the machine
    public static class Screenshot
    {
        // Capture a screenshot from the machine and returns it as a MemoryStram in the JPEG image format
        // This method supports capturing one screenshot containing the image from multiple displays (monitors)
        public static MemoryStream CaptureScreen()
        {
            // Create Bitmap object to store the screenshot with the dimensions of all screens (monitors) on the machine
            Bitmap bitmapOfScreen = new Bitmap(SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
            // Create Graphics object  and pass in the Bitmap object (which will be modified by reference) 
            Graphics screenGraphics = Graphics.FromImage(bitmapOfScreen);
            // Capture the screenshot and transfer it into the Graphics object (which modifies the Bitmap object by reference)
            screenGraphics.CopyFromScreen(SystemInformation.VirtualScreen.Left, SystemInformation.VirtualScreen.Top, 0, 0, bitmapOfScreen.Size);
            // Create MemoryStream for use in return data from the method
            MemoryStream memoryStream = new MemoryStream();
            // Copy the screenshot into "msScreens" as a JPEG image
            bitmapOfScreen.Save(memoryStream, ImageFormat.Jpeg);
            // Return the MemoryStream containing the screenshot
            return memoryStream;
        }
    }
}

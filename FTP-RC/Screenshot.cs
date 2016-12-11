// Copyright (C) 2016 Noah Allen
// This file is part of FTP-RC
//
// FTP-RC is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// FTP-RC is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see http://www.gnu.org/licenses/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace FTP_RC
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

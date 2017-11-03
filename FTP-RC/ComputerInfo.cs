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
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace noah1984.FtpRc
{
    // This class contains methods that retrieve machine-specific information
    public static class ComputerInfo
    {
        // Retrieve the first valid MAC address from the client's network interfaces (NIC card(s))
        // Returns String.Empty if no valid MAC address is located
        public static string GetMacAddress()
        {
            // Convert byte array to string
            return String.Concat(GetMacAddressBytes().Select(h => h.ToString("X2")));
        }
        public static byte[] GetMacAddressBytes()
        {
            // Populate array of all the network interfaces for the machine
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            // Loop through each member of the "networkInterfaces" array
            for (int i = 0; i < networkInterfaces.Length; ++i)
            {
                // Store the MAC address from the current network interface being examined
                // and convert it to a string.
                string currentMac = networkInterfaces[i].GetPhysicalAddress().ToString();
                // If the address retrieved from the device is 12 characters
                if (currentMac.Length == 12)
                {
                    // It is a valid MAC address
                    return networkInterfaces[i].GetPhysicalAddress().GetAddressBytes();
                }
            }
            // This method returns null if no valid MAC address is located
            return null;
        }
    }
}
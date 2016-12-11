// Copyright (C) 2016 Noah Allen
//
// This file is part of FTP-RC.
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
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FTP_RC
{
    // This class contains methods that retrieve machine specific information
    public static class ComputerInfo
    {
        // Retrieve the first valid MAC address from the client's network interfaces (NIC card(s))
        // Returns String.Empty if no valid MAC address is located
        public static string GetMacAddress()
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
                    return currentMac;
                }
            }
            // This method returns String.Empty if no valid MAC address is located
            return String.Empty;
        }
    }
}
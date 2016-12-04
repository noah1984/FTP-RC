using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FTP_RC
{
    public static class ComputerInfo
    {
        public static string GetMacAddress()
        {
            /*
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                PhysicalAddress address = adapter.GetPhysicalAddress();
                if (address.ToString().Length == 12)
                {
                    return "000000000000";
                    //return address.ToString();
                }
            }
            return null;
            */
            return "000000000000";
        }
    }
}

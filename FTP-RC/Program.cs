using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;

namespace FTP_RC
{
    class Program
    {
        static void Main(string[] args)
        {
            FTPClient ftpClient = new FTPClient("127.0.0.1", "root", "password");



            //ftpClient.Upload(@"C:\Users\Nova\Documents\file.txt");

            bool addClient = true;
            
            string macAddress = ComputerInfo.GetMacAddress();
            StreamReader reader = ftpClient.Download("clients.txt");
            if (reader != null)
            {
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();
                    if(line == macAddress)
                    {
                        addClient = false;
                    }
                }
                //Console.WriteLine("Download Complete, status {0}, and {1}", ftpClient.response.StatusDescription, (int)ftpClient.response.StatusCode);
                reader.Close();
                reader.Dispose();
                reader = null;
                ftpClient.response.Close();
            }
            if(addClient)
            {
                
                ftpClient.UploadText("clients.txt", macAddress + "\n");
                //Console.WriteLine("No file.");
            }
            while(true)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Sleeping");
                reader = ftpClient.Download(macAddress + "CMD.txt");
                if (reader != null)
                {
                    while (reader.Peek() >= 0)
                    {
                        string line = reader.ReadLine();
                        if (line == "0")
                        {
                            ftpClient.UploadText("tester.txt", "blah\n");
                        }
                    }
                    //Console.WriteLine("Download Complete, status {0}, and {1}", ftpClient.response.StatusDescription, (int)ftpClient.response.StatusCode);
                    reader.Close();
                    ftpClient.response.Close();
                }
            }
            

            Console.ReadLine();


        }
    }
}

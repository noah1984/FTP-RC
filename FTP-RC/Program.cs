using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace FTP_RC
{
    class Program
    {
        static void Main(string[] args)
        {
            FTPClient ftpClient = new FTPClient("127.0.0.1", "root", "password");
            ftpClient.upload(@"C:\Users\Nova\Documents\file.txt");

            /* Download print up
            StreamReader reader = ftpClient.download("clients.txt");
            Console.WriteLine(reader.ReadToEnd());

            Console.WriteLine("Download Complete, status {0}", ftpClient.response.StatusDescription);

            reader.Close();
            ftpClient.response.Close();

            Console.ReadLine();
            */

            Console.ReadLine();


        }
    }
}

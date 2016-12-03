using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace FTP_RC
{
    public class FTPClient
    {
        public FtpWebResponse response { get; set; }

        private string server;
        private string username;
        private string password;

        

        public FTPClient(string sentServer, string sentUsername, string sentPassword)
        {
            server = sentServer;
            username = sentUsername;
            password = sentPassword;
        }

        public StreamReader download(string filename)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + server + "/" + filename);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            request.Credentials = new NetworkCredential(username, password);

            response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            return new StreamReader(responseStream);
        }

        public void upload(string filename)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + server + "/" + Path.GetFileName(filename));
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(username, password);

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            FileInfo fInfo = new FileInfo(filename);

            long size = fInfo.Length;

            byte[] buffer = new byte[4096];
            int bytes = 0;

            request.ContentLength = fs.Length;

            Stream requestStream = request.GetRequestStream();

            while ((bytes = fs.Read(buffer, 0, buffer.Length)) > 0)
            {
                requestStream.Write(buffer, 0, buffer.Length);
            }
            
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
        }
    }
}

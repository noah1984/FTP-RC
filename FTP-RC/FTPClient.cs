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

        private string _server;
        private string _username;
        private string _password;

        

        public FTPClient(string sentServer, string sentUsername, string sentPassword)
        {
            _server = sentServer;
            _username = sentUsername;
            _password = sentPassword;
        }
        public StreamReader DownloadText(string filename)
        {
            return DownloadText(filename, 0);
        }

        public StreamReader DownloadText(string filename, long offset)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + _server + "/" + filename);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            if (offset > 0)
            {
                request.ContentOffset = offset;
            }
            request.Credentials = new NetworkCredential(_username, _password);

            try
            {
                response = (FtpWebResponse)request.GetResponse();
            }
            catch
            {
                return null;
            }

            Stream responseStream = response.GetResponseStream();
            return new StreamReader(responseStream);
        }

        public void Upload(string filename)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + _server + "/" + Path.GetFileName(filename));
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(_username, _password);

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

        public void UploadMemstream(string filename, MemoryStream ms)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + _server + "/" + filename);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(_username, _password);


            byte[] buffer = new byte[4096];
            int bytes = 0;

            request.ContentLength = ms.Length;

            Stream requestStream = request.GetRequestStream();

            ms.Seek(0, SeekOrigin.Begin);

            while ((bytes = ms.Read(buffer, 0, buffer.Length)) > 0)
            {
                requestStream.Write(buffer, 0, buffer.Length);
            }

            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
        }

        public void UploadText(string filename, string text)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + _server + "/" + Path.GetFileName(filename));
            request.Method = WebRequestMethods.Ftp.AppendFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(_username, _password);


            request.ContentLength = text.Length;

            Stream requestStream = request.GetRequestStream();

            byte[] strBytes = Encoding.ASCII.GetBytes(text);

            requestStream.Write(strBytes, 0, strBytes.Length);

            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
        }
    }
}

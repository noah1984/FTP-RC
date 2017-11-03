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
using System.Net;
using System.IO;

namespace noah1984.FtpRc
{
    // This class is a FTP client and contains custom method to communicate with a FTP server
    public class FTPClient
    {
        // Stores the FTP server's response to the client's request
        // including status code and response description
        public FtpWebResponse Response { get; set; }
        // FTP server IP address or DNS name (can also have ':' and port)
        private string _server;
        // FTP username
        private string _username;
        // FTP password
        private string _password;
        // Class constructor that sets FTP server, FTP username, and FTP password
        public FTPClient(string ftpServer, string ftpUsername, string ftpPassword)
        {
            _server = ftpServer;
            _username = ftpUsername;
            _password = ftpPassword;
        }
        // Download file as binary data using BinaryReader
        public BinaryReader DownloadBinary(string filename)
        {
            // Call overload, specifying size of 0 which indicates to download the
            // file from the beginning
            return DownloadBinary(filename, 0);
        }
        // Download file as binary data using BinaryReader, seeking to specified offset
        public BinaryReader DownloadBinary(string filename, long offset)
        {
            // Create new FTP request
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + _server + "/" + filename);
            // Signal the request to download a file
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            // Set the offset if it is greater than 0
            if (offset > 0)
            {
                ftpRequest.ContentOffset = offset;
            }
            // Set the username and password credentials for the FTP request
            ftpRequest.Credentials = new NetworkCredential(_username, _password);
            // If the file does not exist then the application will throw an error
            // and the method will return null
            try
            {
                // Store the FTP server's response to the client's current request
                Response = (FtpWebResponse)ftpRequest.GetResponse();
            }
            catch
            {
                // Returning null means the file does not exist
                return null;
            }
            // Retrieve the stream for the current response
            Stream responseStream = Response.GetResponseStream();
            // Return BinaryReader of the stream
            return new BinaryReader(responseStream);
        }
        // Download text file using StreamReader
        public StreamReader DownloadText(string filename)
        {
            // Call overload, specifying size of 0 which indicates to download the
            // file from the beginning
            return DownloadText(filename, 0);
        }
        // Download text file using StreamReader, seeking to specified offset
        public StreamReader DownloadText(string filename, long offset)
        {
            // Create new FTP request
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + _server + "/" + filename);
            // Signal the request to download a file
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            // Set the offset if it is greater than 0
            if (offset > 0)
            {
                ftpRequest.ContentOffset = offset;
            }
            // Set the username and password credentials for the FTP request
            ftpRequest.Credentials = new NetworkCredential(_username, _password);
            // If the file does not exist then the application will throw an error
            // and the method will return null
            try
            {
                // Store the FTP server's response to the client's current request
                Response = (FtpWebResponse)ftpRequest.GetResponse();
            }
            catch
            {
                // Returning null means the file does not exist
                return null;
            }
            // Retrieve the stream for the current response
            Stream responseStream = Response.GetResponseStream();
            // Return StreamReader of the stream
            return new StreamReader(responseStream);
        }
        // Determine if client is connected to a valid FTP server and has valid credentials
        public bool IsLoggedIn()
        {
            // This try-catch statement attempts to list the contents of the root directory
            // on the FTP server. The application will throw an error if the client does
            // not have valid credentials for the FTP server (or if it is an invalid server).
            try
            {
                // Create new FTP request
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + _server);
                // Signal the request to list the contents of a Direcotry
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                // Set the username and password credentials for the FTP request
                ftpRequest.Credentials = new NetworkCredential(_username, _password);
                // Store the FTP server's response to the client's current request
                Response = (FtpWebResponse)ftpRequest.GetResponse();
                // Close the stream and free the resources held by the response
                Response.Close();
            }
            catch
            {
                // Returning false indicates there was a problem logging in to the FTP server
                return false;
            }
            // The client has valid credentials for this FTP server
            return true;
        }
        // Upload a MemoryStream to a file on the FTP server
        public void UploadMemoryStream(MemoryStream memoryStream, string filename)
        {
            // Create buffer to store bytes read from the MemoryStream
            byte[] bBuffer = new byte[4096];
            // Stores the number of bytes currently read from the MemoryStream into the buffer
            int bytesRead = 0;
            // Create new FTP request
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + _server + "/" + filename);
            // Signal the request to upload a file
            request.Method = WebRequestMethods.Ftp.UploadFile;
            // Set the username and password credentials for the FTP request
            request.Credentials = new NetworkCredential(_username, _password);
            // Tell the FTP server how many bytes of data to receive
            request.ContentLength = memoryStream.Length;
            // Retrieve the stream used to upload data to the FTP server
            Stream requestStream = request.GetRequestStream();
            // Set the MemoryStream's position back to the beginning
            memoryStream.Seek(0, SeekOrigin.Begin);
            // Loop until all bytes have been read from the MemoryStream
            while ((bytesRead = memoryStream.Read(bBuffer, 0, bBuffer.Length)) > 0)
            {
                // Write the current buffer block to the FTP server
                requestStream.Write(bBuffer, 0, bBuffer.Length);
            }
            // Close the stream and free the resources used by the MemoryStream
            memoryStream.Close();
            // Close the stream and free the resources held by the stream used to upload data to the FTP server
            requestStream.Close();
            // Store the FTP server's response to the client's current request
            Response = (FtpWebResponse)request.GetResponse();
            // Display message informing the user the file was uploaded successfully
            Console.Write("Upload File Complete, status {0}", Response.StatusDescription);
            // Close the stream and free the resources held by the response
            Response.Close();
        }
        // Upload text to the specified file, appending pre-existing files
        public void UploadText(string text, string filename)
        {
            //Create new FTP request
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + _server + "/" + Path.GetFileName(filename));
            // Signal the request to append the file (also creates a file if one does not exist)
            request.Method = WebRequestMethods.Ftp.AppendFile;
            // Set the username and password credentials for the FTP request
            request.Credentials = new NetworkCredential(_username, _password);
            // Tell the FTP server how many bytes of data to receive
            request.ContentLength = text.Length;
            // Retrieve the stream used to upload data to the FTP server
            Stream requestStream = request.GetRequestStream();
            // Write the text to the FTP server
            requestStream.Write(Encoding.ASCII.GetBytes(text), 0, text.Length);
            // Close the stream and free the resources held by the stream used to upload data to the FTP server
            requestStream.Close();
            // Store the FTP server's response to the client's current request
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            // Display message informing the user the file was uploaded successfully
            Console.Write("Upload File Complete, status {0}", response.StatusDescription);
            // Close the stream and free the resources held by the response
            response.Close();
        }
    }
}

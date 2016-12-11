// FTP-RC: Control your PC from your phone
// Copyright (C) 2016 Noah Allen
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
using System.Security.Cryptography;
using System.IO;

namespace FTP_RC
{
    public static class Encryption
    {
        // Initialization vector must be 16 characters
        private const string IV = "1234567890ABCDEF";
        // This can be anything
        private const string password = "password";
        // Key size of the encryption algorithm
        private const int KEY_SIZE = 256;
        // Size of salt
        private const int SALT_SIZE = 4;
        // Used for generating random salt values
        private static RNGCryptoServiceProvider cryptoRandom = new RNGCryptoServiceProvider();
        // Byte array of the initialization vector
        private static byte[] IV_Bytes = Encoding.UTF8.GetBytes(IV);
        // Encryption object
        private static RijndaelManaged rijndael = new RijndaelManaged();
        // Derive key from the password
        private static PasswordDeriveBytes keyFromPassword = new PasswordDeriveBytes(password, null);
        // Byte array of key from password
        private static byte[] keyBytes = keyFromPassword.GetBytes(KEY_SIZE / 8);
        // Cryptographic encryption transformer
        private static ICryptoTransform encryptor = rijndael.CreateEncryptor(keyBytes, IV_Bytes);
        // Cryptographic decryption transformer
        private static ICryptoTransform decryptor = rijndael.CreateDecryptor(keyBytes, IV_Bytes);
        // Encrypt data
        public static byte[] EncryptData(byte[] unencryptedData)
        {
            // Generate random salt
            byte[] saltBytes = GenerateSalt();
            // Declare byte array to hold unencrypted data + salt
            byte[] saltedData = new byte[unencryptedData.Length + saltBytes.Length];
            // Copy unencrypted data into new array
            Buffer.BlockCopy(unencryptedData, 0, saltedData, 0, unencryptedData.Length);
            // Copy salt into new array
            Buffer.BlockCopy(saltBytes, 0, saltedData, unencryptedData.Length, saltBytes.Length);
            // Create MemoryStream to hold cipher
            MemoryStream memoryStream = new MemoryStream();
            // Create CryptoStream for use in transforming the data into cipher
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            // Write the data to encrypt to the stream
            cryptoStream.Write(saltedData, 0, saltedData.Length);
            // Update data and clear buffer
            cryptoStream.FlushFinalBlock();
            // Free the resources used by the CryptoStream
            cryptoStream.Dispose();
            // Transfer MemoryStream data into the byte array
            byte[] cipherBytes = memoryStream.ToArray();
            // Free the resources used by the MemoryStream
            memoryStream.Dispose();
            // Return the cipher
            return cipherBytes;
        }
        // Encrypt text
        public static string EncryptText(string unencryptedText)
        {
            // Extract bytes from text, encrypt these bytes, then convert to base 64 and return
            return Convert.ToBase64String(EncryptData(Encoding.UTF8.GetBytes(unencryptedText)));
        }
        // Decrypt data
        public static byte[] DecryptData(byte[] cipherData)
        {
            // Declare byte array to hold decrypted data
            byte[] decryptedData = new byte[cipherData.Length];
            // Transfer cipher data into MemoryStream
            MemoryStream memoryStream = new MemoryStream(cipherData);
            // Create CryptoStream for use in decrypting the data from cipher
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            // Read the decrypted data from the stream
            int decryptedByteCount = cryptoStream.Read(decryptedData, 0, decryptedData.Length);
            // Free the resources used by the CryptoStream
            cryptoStream.Dispose();
            // Free the resources used by the MemoryStream
            memoryStream.Dispose();
            // Declare byte array to hold decrypted data - salt
            byte[] unsaltedData = new byte[decryptedByteCount - SALT_SIZE];
            // Copy decrypted data into new array
            Buffer.BlockCopy(decryptedData, 0, unsaltedData, 0, unsaltedData.Length);
            // Return decrypted data
            return unsaltedData;
        }
        // Decrypt text
        public static string DecryptText(string cipherText)
        {
            // Convert text to byte array, decrypt these bytes, then decode bytes into string and return
            return Encoding.UTF8.GetString(DecryptData(Convert.FromBase64String(cipherText)));
        }
        // Retrieves a byte array of random values
        private static byte[] GenerateSalt()
        {
            // Declare byte array to the salt
            byte[] randomBytes = new byte[SALT_SIZE];
            // Use the crypto service provider to fill the byte array with random values
            cryptoRandom.GetBytes(randomBytes);
            // Return the salt
            return randomBytes;
        }
        // Set additional salt
        public static void SetExtraSalt(byte[] extraSalt)
        {
            // Derive key from the password + salt
            keyFromPassword = new PasswordDeriveBytes(password, extraSalt);
            // Set the byte array of key from password (now with salt)
            keyBytes = keyFromPassword.GetBytes(KEY_SIZE / 8);
        }
    }
}

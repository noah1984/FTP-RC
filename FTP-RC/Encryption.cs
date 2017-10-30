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
using System.Security.Cryptography;
using System.IO;

namespace FTP_RC
{
    // This class is the parent of the Encryption child classes
    public abstract class Encryption
    {
        // All child classes must implement decrypting data
        public abstract byte[] DecryptData(byte[] data);
        // Decrypting text is universal to all child classes
        public string DecryptText(string text)
        {
            return Encoding.UTF8.GetString(DecryptData(Convert.FromBase64String(text)));
        }
        // All child classes must implement encrypting data
        public abstract byte[] EncryptData(byte[] data);
        // Encrypting text is universal to all child classes
        public string EncryptText(string text)
        {
            return Convert.ToBase64String(EncryptData(Encoding.UTF8.GetBytes(text)));
        }

    }
    // RSA encryption, child class of encryption and inherits from IDisposable interface
    public class RSAEncryption : Encryption, IDisposable
    {
        // For use in IDisposable in tracking the current state of disposal
        private bool _isDisposed;
        // Informs if private key information is available
        public bool PublicOnly
        {
            get { return _rsa.PublicOnly; }
        }
        // RSA encryption object
        private RSACryptoServiceProvider _rsa;
        // Default constructor simply instantiates RSA encryption object
        public RSAEncryption()
        {
            _rsa = new RSACryptoServiceProvider();
        }
        // Overloaded constructor calls the default constructor to instantiate the
        // RSA encryption object then loads key information from XML string.
        public RSAEncryption(string XmlString)
            : this()
        {
            _rsa.FromXmlString(XmlString);
        }
        // Overloaded constructor calls the default constructor to instantiate the 
        // RSA encryption object then loads key information from CSP blob.
        public RSAEncryption(byte[] cspBlob)
            : this()
        {
            _rsa.ImportCspBlob(cspBlob);
        }
        // Decrypt the data, with OAEP padding
        public override byte[] DecryptData(byte[] data)
        {
            return _rsa.Decrypt(data, true);
        }
        // Performs disposing required by IDisposable interface
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // Carries out disposing of RSA object
        protected virtual void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    if (_rsa != null)
                    {
                        _rsa.Clear();
                    }
                }
                _isDisposed = true;
            }
        }
        // Encrypt the data, with OAEP padding
        public override byte[] EncryptData(byte[] data)
        {
            return _rsa.Encrypt(data, true);
        }
        //Retrieve CSP blob which is a byte array containing private and/or public key information
        public byte[] GetCspBlob(bool includePrivate)
        {
            // If the user has requested private key, but it is not available, this method
            // will return null.
            if (PublicOnly && includePrivate)
            {
                return null;
            }
            return _rsa.ExportCspBlob(includePrivate);
        }
        //Retrieve XML string which is a string containing private and/or public key information
        public string GetXmlString(bool includePrivate)
        {
            // If the user has requested private key, but it is not available, this method
            // will return null.
            if (PublicOnly && includePrivate)
            {
                return null;
            }
            return _rsa.ToXmlString(includePrivate);
        }
        // Saves key information in the designated slow of the machine's key store.
        // This version of the method stores it in the user section of the store.
        public void SaveInContainer(string containerName)
        {
            SaveInContainer(containerName, false);
        }
        // Saves key information in the designated slow of the machine's key store.
        // This overloaded version of the method stores the information in the user
        // section if "useMachineStore" is false, otherwise it stores the information
        // in the machine section.
        public void SaveInContainer(string containerName, bool useMachineStore)
        {
            // Create new cryptographic service provider parameters object
            CspParameters cspParameters = new CspParameters();
            // Set the container name, which in this case is the MAC address
            cspParameters.KeyContainerName = containerName;
            // Set the machine flag to switch from the user section to the machine
            // section of the key store.
            if (useMachineStore)
            {
                cspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
            }
            // Create temporary RSA encryption in order to write these changes to the key store
            using (RSACryptoServiceProvider tempRsa = new RSACryptoServiceProvider(cspParameters))
            {
                // Update the temporary object to the current class RSA objects key information.
                // This updates the information in the key store.
                tempRsa.ImportParameters(_rsa.ExportParameters(!PublicOnly));
            }
        }
        // Load private and or public key information from the key store.
        // This version of the method loads the information from the machine section
        // of the key store.
        public bool LoadFromContainer(string containerName)
        {
            return LoadFromContainer(containerName, false);
        }
        // Load private and or public key information from the key store.
        // This overloaded version of the method loads the information from the user
        // section if "useMachineStore" is false, otherwise it loads the information
        // from the machine section. The method will return false if the entry does
        // not exist.
        public bool LoadFromContainer(string containerName, bool useMachineStore)
        {
            // Create new cryptographic service provider parameters object
            CspParameters cspParameters = new CspParameters();
            // Set the container name, which in this case is the MAC address
            cspParameters.KeyContainerName = containerName;
            // This flag indicates that the entry must exist. Without setting this
            // the application will return a random key. This flag is also what causes
            // an error to be thrown in the following try-catch.
            cspParameters.Flags = CspProviderFlags.UseExistingKey;
            // Set the machine store flag if necessary
            if (useMachineStore)
            {
                cspParameters.Flags |= CspProviderFlags.UseMachineKeyStore;
            }
            // If this throws an error then the entry does not exist
            try
            {
                // Create temporary RSA encryption in order to load the information from the
                // key store and import into classes' RSA encryption object.
                using (RSACryptoServiceProvider tempRsa = new RSACryptoServiceProvider(cspParameters))
                {
                    // Import key information from the key store to the current class RSA
                    // encryption object.
                    _rsa.ImportParameters(tempRsa.ExportParameters(!tempRsa.PublicOnly));
                }
            }
            catch
            {
                // The entry does not exist
                return false;
            }
            // The entry was retrieved
            return true;
        }
    }
    // AES encryption, child class of encryption and inherits from IDisposable interface
    public class AESEncryption : Encryption, IDisposable
    {
        // AES encryption object
        protected Aes _aes;
        // For use in IDisposable in tracking the current state of disposal
        private bool _isDisposed;
        // Retrieve and set current AES encryption key
        public byte[] Key
        {
            get { return _aes.Key; }
            set { _aes.Key = value; }
        }
        // Default constructor instantiates AES encryption object and sets encryption to 128-bit
        public AESEncryption()
        {
            _aes = Aes.Create();
            _aes.KeySize = 128;
        }
        // Overloaded constructor calls default constructor and sets key to specified byte array
        public AESEncryption(byte[] key)
            : this()
        {
            _aes.Key = key;
        }
        // Decrypt the data
        public override byte[] DecryptData(byte[] data)
        {
            // Byte array to hold the initialization vector
            byte[] IV = new byte[_aes.BlockSize / 8];
            // Byte array to hold the cipher data
            byte[] cipher = new byte[data.Length - IV.Length];
            // Copy the data passed in into the designated byte arrays
            Buffer.BlockCopy(data, 0, IV, 0, IV.Length);
            Buffer.BlockCopy(data, IV.Length, cipher, 0, cipher.Length);
            // Create cryptographic decryption object, setting the key and initialization vector
            using (ICryptoTransform decryptor = _aes.CreateDecryptor(_aes.Key, IV))
            {
                // Initialize MemoryStream object with the cipher data byte array
                using (MemoryStream memoryStream = new MemoryStream(cipher))
                {
                    // Initialize CryptoStream object with the MemoryStream of the cipher data and
                    // specify the decryption object.
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        // Perform decryption and retrieve the number of decrypted bytes
                        int decryptedBytesCount = cryptoStream.Read(data, 0, data.Length);
                        // Declare byte array to transfer decrypted data
                        byte[] decryptedData = new byte[decryptedBytesCount];
                        // Copy the decrypted to the designated byte array
                        Buffer.BlockCopy(data, 0, decryptedData, 0, decryptedData.Length);
                        // return the decrypted data
                        return decryptedData;
                    }
                }
            }
        }
        // Performs disposing required by IDisposable interface
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // Carries out disposing of AES object
        protected virtual void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    if (_aes != null)
                    {
                        _aes.Clear();
                    }
                }
                _isDisposed = true;
            }
        }
        // Encrypt the data
        public override byte[] EncryptData(byte[] data)
        {
            // This updates the class AES encryption object's "IV" property with a fresh
            // initialization vector.
            _aes.GenerateIV();
            // Create cryptographic encryption object, setting the key and initialization vector
            using (ICryptoTransform encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV))
            {
                // Initialize MemoryStream object to store initialization vector and cipher data
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Write initialization vector to the MemoryStream object
                    memoryStream.Write(_aes.IV, 0, _aes.IV.Length);
                    // Initialize CryptoStream object with the MemoryStream of the cipher data and
                    // specify the encryption object.
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        // Perform the encryption
                        cryptoStream.Write(data, 0, data.Length);
                    }
                    // Return byte array of the MemoryStream object
                    return memoryStream.ToArray();
                }
            }
        }
    }
    // 192-bit variant of the AES encryption class
    public class AES192 : AESEncryption
    {
        // Default constructor calls the AESEncryption default base constructor and
        // sets encryption to 192-bit
        public AES192()
            : base()
        {
            _aes.KeySize = 192;
        }
        // Overloaded constructor calls the AESEncryption overloaded base constructor,
        // setting the key to specified byte array, and sets encryption to 192-bit
        public AES192(byte[] key)
            : base(key)
        {
            _aes.KeySize = 192;
        }
    }
    // 256-bit variant of the AES encryption class
    public class AES256 : AESEncryption
    {
        // Default constructor calls the AESEncryption default base constructor and
        // sets encryption to 256-bit
        public AES256()
            : base()
        {
            _aes.KeySize = 256;
        }
        // Overloaded constructor calls the AESEncryption overloaded base constructor,
        // setting the key to specified byte array, and sets encryption to 192-bit
        public AES256(byte[] key)
            : base(key)
        {
            _aes.KeySize = 256;
        }
    }
    // XOR (flip) encryption, child class of encryption
    // This encryption flips the bits of the data.
    // This encryption is unique in that you use
    // the same method to encrypt and decrypt data.
    // This encryption is not very strong.
    public class XOREncryption : Encryption
    {
        // Byte array of key
        public byte[] Key { get; set; }
        // Default constructor sets key from password
        public XOREncryption() : this("password") {}
        // Overloaded constructor sets the key to the specified byte array
        public XOREncryption(byte[] keyBytes)
        {
            Key = keyBytes;
        }
        // Overloaded constructor sets the key based on a string
        public XOREncryption(string password)
        {
            Key = new byte[16];
            if (password.Length > 0)
            {
                // Loop through each byte of the Key
                for (int x = 0; x < Key.Length; ++x)
                {
                    // This sets the key based on the password
                    // If x >= password length, it resets back to 0 using modulus.
                    Key[x] = (byte)password[x % password.Length];
                }
                Key = Encoding.UTF8.GetBytes(password);
            }
        }
        // Decrypting data and encrypting data uses the same method
        // because the data is flipped.
        public override byte[] DecryptData(byte[] data)
        {
            return EncryptData(data);
        }
        // Perform encrypt/decrypt by value
        public override byte[] EncryptData(byte[] data)
        {
            // Clone the data to a separate byte array
            // otherwise the data would be modified by reference
            byte[] dataClone = data.Clone() as byte[];
            // XOR the byte array against the key by reference
            XORRef(dataClone);
            // Return the flipped data
            return dataClone;
        }

        // Perform encrypt/decrypt by reference
        // This modifies the data array directly
        public void XORRef(byte[] data)
        {
            // Loop through each byte of the data
            for (int x = 0; x < data.Length; ++x)
            {
                // This performs an XOR operation between the data and the
                // key. If x >= key length, it resets back to 0 using modulus.
                data[x] = (byte)(data[x] ^ Key[x % Key.Length]);
            }
        }
    }
}


using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;

using BibleBot.Backend.Models;

namespace BibleBot.Backend.Services
{
    public enum CryptographicAction
    {
        ENCRYPT = 0,
        DECRYPT = 1
    }

    public class CryptographyService
    {
        public byte[] GenerateKeyFromString(string str)
        {
            return SHA256.Create().ComputeHash(Encoding.Unicode.GetBytes(str));
        }

        public void ProcessFile(CryptographicAction cryptoAction, string inputPath, string outputPath, string key)
        {
            var inputFile = new FileStream(inputPath, FileMode.Open);
            var outputFile = new FileStream(outputPath, FileMode.Create);

            Aes crypt = Aes.Create();
            crypt.Key = GenerateKeyFromString(key);
            crypt.BlockSize = 128;
            crypt.IV = new byte[] { 0xB, 0x1, 0xB, 0x1, 0xE, 0xB, 0x0, 0x7, 0xB, 0x1, 0xB, 0x1, 0xE, 0xB, 0x0, 0x7 };

            FileStream fileToRead;
            ICryptoTransform cryptoTransform;
            CryptoStreamMode streamMode;

            if (cryptoAction == CryptographicAction.ENCRYPT)
            {
                fileToRead = outputFile;
                cryptoTransform = crypt.CreateEncryptor();
                streamMode = CryptoStreamMode.Write;
            }
            else
            {
                fileToRead = inputFile;
                cryptoTransform = crypt.CreateDecryptor();
                streamMode = CryptoStreamMode.Read;
            }

            using (CryptoStream cryptoStream = new CryptoStream(fileToRead, cryptoTransform, streamMode))
            {
                if (cryptoAction == CryptographicAction.ENCRYPT)
                {
                    int currentByte;
                    while ((currentByte = inputFile.ReadByte()) != -1)
                    {
                        cryptoStream.WriteByte((byte) currentByte);
                    }
                }
                else
                {
                    int currentByte;
                    while ((currentByte = cryptoStream.ReadByte()) != -1)
                    {
                        outputFile.WriteByte((byte) currentByte);
                    }
                }

                cryptoStream.Close();
            }

            inputFile.Close();
            outputFile.Close();
        }
    }
}
using System;
using System.IO;
using System.Security.Cryptography;

namespace PowerBi.Tests
{
    public static class HashExtension
    {
        public static string HashFile(this Stream stream)
        {
            var sha = new SHA256Managed();
            byte[] checksum = sha.ComputeHash(stream);
            return BitConverter.ToString(checksum).Replace("-", String.Empty);
        }
    }
}
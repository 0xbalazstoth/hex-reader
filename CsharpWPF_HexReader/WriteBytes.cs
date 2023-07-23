using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsharpWPF_HexReader
{
    class WriteBytes
    {
        public void Saved(string fileName, List<string> readContent)
        {
            string hexString = "";
            foreach (var currentHex in readContent)
            {
                hexString += currentHex.Replace("66", "5B");
            }

            File.WriteAllBytes(fileName, StringToByteArray(hexString));
        }

        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}

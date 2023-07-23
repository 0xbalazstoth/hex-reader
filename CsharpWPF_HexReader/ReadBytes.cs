using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsharpWPF_HexReader
{
    class ReadBytes
    {
        private List<string> _readFileContent = new List<string>();
        public List<string> ReadFileContent
        {
            get
            {
                return _readFileContent;
            }
        }
        public void Opened(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    int hexIn;
                    string hex;

                    for (int i = 0; (hexIn = fs.ReadByte()) != -1; i++)
                    {
                        hex = string.Format("{0:X2}", hexIn);
                        _readFileContent.Add(hex);
                    }
                }
            } catch { }
        }
    }
}

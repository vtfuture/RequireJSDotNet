using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor
{
    public class FileSpec
    {
        private string fileName;

        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value.Trim();
            }
        }

        public string CompressionType { get; set; }

        public FileSpec(string fileName, string compressionType)
        {
            FileName = fileName;
            CompressionType = compressionType;
        }
    }
}

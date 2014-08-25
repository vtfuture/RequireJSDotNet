// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

namespace RequireJsNet.Compressor
{
    public class FileSpec
    {
        private string fileName;

        public FileSpec(string fileName, string compressionType)
        {
            FileName = fileName;
            CompressionType = compressionType;
        }

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

        public string FileContent { get; set; }

        public string CompressionType { get; set; }
    }
}

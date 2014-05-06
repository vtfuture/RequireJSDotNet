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

        public string CompressionType { get; set; }
    }
}

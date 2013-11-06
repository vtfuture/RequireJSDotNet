using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Yahoo.Yui.Compressor;

namespace RequireJsNet.Compressor
{
    public class RequireCompressorTask : Task
    {
        private RequireConfigReader ConfigReader;

        public string LoggingType { get; set; }
        public string CompressionType { get; set; }
        public string EncodingType { get; set; }
        public int LineBreakPosition { get; set; }
        [Required]
        public string ProjectPath { get; set; }
        public ITaskItem[] RequireConfigs { get; set; }

        public override bool Execute()
        {
            var files =  new List<string>();
            if (RequireConfigs != null)
            {
                files = RequireConfigs.Select(r => r.GetMetadata("FullPath")).ToList();    
            }
            
            ConfigReader = new RequireConfigReader(ProjectPath, files);
            var bundles = new List<Bundle>();
            try
            {
                bundles = ConfigReader.ParseConfigs();
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }

            if (bundles.Any())
            {
                var compressor = new JavaScriptCompressor();
                foreach (var bundle in bundles)
                {
                    if (!bundle.Files.Any())
                    {
                        continue;
                    }
                    var taskEngine = new CompressorTaskEngine(new MsBuildLogAdapter(Log), compressor)
                    {
                        CompressionType = CompressionType,
                        DeleteSourceFiles = false,
                        EncodingType = EncodingType,
                        LineBreakPosition = -1,
                        LoggingType = LoggingType,
                        OutputFile = bundle.Output,
                        SourceFiles = bundle.Files.ToArray()
                    };
                    taskEngine.Execute();
                }
            }
            
            return true;
        }

        

    }
}

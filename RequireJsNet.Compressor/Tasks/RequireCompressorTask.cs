// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using RequireJsNet.Compressor.Helpers;
using RequireJsNet.Compressor.RequireProcessing;

using Yahoo.Yui.Compressor;

namespace RequireJsNet.Compressor
{
    public class RequireCompressorTask : Task
    {
        private ConfigProcessor configProcessor;

        public string LoggingType { get; set; }

        public string CompressionType { get; set; }

        public string EncodingType { get; set; }

        public int LineBreakPosition { get; set; }

        public bool AutoCompressor { get; set; }

        [Required]
        public string ProjectPath { get; set; }

        public string PackagePath { get; set; }

        public ITaskItem[] RequireConfigs { get; set; }

        public ITaskItem EntryPointOverride { get; set; }


        public override bool Execute()
        {
            var files = new List<string>();
            if (RequireConfigs != null)
            {
                files = RequireConfigs.Select(r => r.GetMetadata("FullPath")).ToList();    
            }

            var entryPointOveride = string.Empty;

            if (EntryPointOverride != null)
            {
                entryPointOveride = EntryPointOverride.GetMetadata("FullPath");
            }

            this.configProcessor = ConfigProcessorFactory.Create(AutoCompressor, ProjectPath, PackagePath, entryPointOveride, files, FileHelpers.ParseEncoding(EncodingType));

            var bundles = new List<Bundle>();
            try
            {
                bundles = this.configProcessor.ParseConfigs();
            }
            catch (Exception ex)
            {
                var isDebugLogging = LoggingType.ToLower() == "debug";
                Log.LogErrorFromException(ex, isDebugLogging, isDebugLogging, null);
                return false;
            }

            if (bundles.Any())
            {
                EnsureOutputDirectoriesExist(bundles);
                var compressor = new JavaScriptCompressor
                                     {
                                         Encoding = FileHelpers.ParseEncoding(EncodingType)
                                     };
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

        private void EnsureOutputDirectoriesExist(List<Bundle> bundles)
        {
            foreach (var bundle in bundles)
            {
                var directoryName = Path.GetDirectoryName(bundle.Output);
                if (directoryName == null)
                {
                    continue;
                }

                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
            }
        }
    }
}

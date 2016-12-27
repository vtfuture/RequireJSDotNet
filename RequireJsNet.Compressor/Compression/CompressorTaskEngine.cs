﻿// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

using EcmaScript.NET;

using RequireJsNet.Compressor.Helpers;

using Yahoo.Yui.Compressor;

namespace RequireJsNet.Compressor
{
    public class CompressorTaskEngine
    {
        private readonly ICompressor compressor;

        private CompressionType compressionType;

        public CompressorTaskEngine(ILog log, ICompressor compressor)
        {
            Log = log;
            this.compressor = compressor;
            this.Encoding = Encoding.Default;
            DeleteSourceFiles = false;
            LineBreakPosition = -1;
            EcmaExceptions = new List<EcmaScriptException>();
        }

        public delegate void Action();

        public string LoggingType { get; set; }

        public FileSpec[] SourceFiles { get; set; }

        public Action SetTaskEngineParameters { get; set; }

        public Action ParseAdditionalTaskParameters { get; set; }

        public Action LogAdditionalTaskParameters { get; set; }

        public Action SetCompressorParameters { get; set; }

        public string OutputFile { get; set; }

        public string CompressionType { get; set; }

        public string EncodingType { get; set; }

        public bool DeleteSourceFiles { get; set; }

        public int LineBreakPosition { get; set; }

        public ILog Log { get; private set; }

        protected internal LoggingType LogType { get; set; }

        protected internal Encoding Encoding { get; set; }

        private List<EcmaScriptException> EcmaExceptions { get; set; }

        public bool Execute()
        {
            try
            {
                if (this.SetTaskEngineParameters != null)
                {
                    this.SetTaskEngineParameters();
                }

                ParseTaskParameters();
                if (this.SetCompressorParameters != null)
                {
                    SetCompressorParameters();
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }

            // Check to make sure we have the bare minimum arguments supplied to the task.
            if (SourceFiles == null || SourceFiles.Length == 0)
            {
                Log.LogError("At least one file is required to be compressed / minified.");
                return false;
            }

            if (string.IsNullOrEmpty(OutputFile))
            {
                Log.LogError("The outfile is required if one or more css input files have been defined.");
                return false;
            }

            foreach (var sourceFile in SourceFiles)
            {
                if (string.Compare(sourceFile.FileName, OutputFile, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    Log.LogError("Output file cannot be the same as source file(s).");
                    return false;
                }
            }

            if (this.LogType == Yahoo.Yui.Compressor.LoggingType.Debug)
            {
                LogTaskParameters();
                if (LogAdditionalTaskParameters != null)
                {
                    LogAdditionalTaskParameters();
                }
            }

            Log.LogMessage("Starting Compression...");

            OutputAssemblyInfo();

            // What is the current thread culture?
            Log.LogMessage(
                string.Format(
                    "Current thread culture / UI culture (before modifying, if requested): {0}/{1}",
                    Thread.CurrentThread.CurrentCulture.EnglishName,
                    Thread.CurrentThread.CurrentUICulture.EnglishName));

            Log.LogMessage(string.Empty); // This, in effect, is a new line.

            var startTime = DateTime.Now;
            var compressedText = CompressFiles();

            if (EcmaExceptions.Count > 0)
            {
                return false;
            }

            // Save this css to the output file, if we have some result text.
            if (!this.SaveCompressedText(compressedText))
            {
                Log.LogMessage("Failed to finish compression - terminating prematurely.");
                return false;
            }

            Log.LogMessage("Finished compression.");
            Log.LogMessage(
                string.Format(
                    CultureInfo.InvariantCulture, "Total time to execute task: {0}", (DateTime.Now - startTime)));
            Log.LogMessage("8< ---------------------------------  ( o Y o )  --------------------------------- >8");
            Log.LogMessage(string.Empty); // This, in effect, is a new line.

            return true;
        }

        public void ParseTaskParameters()
        {
            ParseLoggingType();
            if (string.IsNullOrEmpty(CompressionType))
            {
                LogMessage("No Compression type defined. Defaulting to 'Standard'.");
                compressionType = ParseCompressionType("Standard");
            }
            else
            {
                compressionType = ParseCompressionType(CompressionType);
            }

            this.Encoding = FileHelpers.ParseEncoding(EncodingType);
            if (this.ParseAdditionalTaskParameters != null)
            {
                ParseAdditionalTaskParameters();
            }
        }

        protected internal virtual string Compress(FileSpec file, string originalContent)
        {
            compressor.CompressionType = GetCompressionTypeFor(file);
            compressor.LineBreakPosition = LineBreakPosition;
            return compressor.Compress(originalContent);
        }

        protected void LogMessage(string message, bool isIndented = false)
        {
            if (this.LogType != Yahoo.Yui.Compressor.LoggingType.None)
            {
                Log.LogMessage(string.Format(CultureInfo.InvariantCulture, "{0}{1}", isIndented ? "    " : string.Empty, message));
            }
        }

        protected virtual void LogTaskParameters()
        {
            LogMessage("CompressionType: " + this.CompressionType);
            LogMessage("DeleteSourceFiles: " + DeleteSourceFiles);
            LogMessage("EncodingType: " + this.EncodingType);
            LogMessage("LoggingType: " + this.LoggingType);
        }

        private void OutputAssemblyInfo()
        {
            // Determine and log the Assembly version.
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionAttributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            var assemblyFileVersion = fileVersionAttributes.Length > 0
                                             ? ((AssemblyFileVersionAttribute)fileVersionAttributes[0]).Version
                                             : "Unknown File Version";

            var assemblyTitleAttributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            var assemblyTitle = assemblyTitleAttributes.Length > 0
                                       ? ((AssemblyTitleAttribute)assemblyTitleAttributes[0]).Title
                                       : "Unknown Title";

            Log.LogMessage(string.Format("Using version {0} of {1}.", assemblyFileVersion, assemblyTitle));
        }

        private void ParseLoggingType()
        {
            if (string.IsNullOrEmpty(this.LoggingType))
            {
                this.LogType = Yahoo.Yui.Compressor.LoggingType.Info;
                this.LogMessage("No logging argument defined. Defaulting to 'Info'.");
                return;
            }

            switch (this.LoggingType.ToLowerInvariant())
            {
                case "none":
                    this.LogType = Yahoo.Yui.Compressor.LoggingType.None;
                    break;
                case "debug":
                    this.LogType = Yahoo.Yui.Compressor.LoggingType.Debug;
                    break;
                case "info":
                    this.LogType = Yahoo.Yui.Compressor.LoggingType.Info;
                    break;
                default:
                    throw new ArgumentException("Logging Type: " + LoggingType + " is invalid.", "LoggingType");
            }
        }

        private CompressionType ParseCompressionType(string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "none":
                    return Yahoo.Yui.Compressor.CompressionType.None;
                case "standard":
                    return Yahoo.Yui.Compressor.CompressionType.Standard;
                default:
                    throw new ArgumentException("Compression Type: " + type + " is invalid.", "type");
            }
        }

        private StringBuilder CompressFiles()
        {
            int totalOriginalContentLength = 0;
            StringBuilder finalContent = null;

            if (SourceFiles != null)
            {
                LogMessage(string.Format(CultureInfo.InvariantCulture, "# {0} file{1} requested.", SourceFiles.Length, Extensions.ToPluralString(SourceFiles.Length)));

                // Now compress each file.
                foreach (var file in SourceFiles)
                {
                    var message = "=> " + file.FileName;

                    // Load up the file.
                    try
                    {
                        var originalContent = string.IsNullOrEmpty(file.FileContent) 
                                                    ? File.ReadAllText(file.FileName, this.Encoding) 
                                                    : file.FileContent;
                        totalOriginalContentLength += originalContent.Length;

                        if (string.IsNullOrEmpty(originalContent))
                        {
                            LogMessage(message, true);
                            Log.LogError(string.Format(CultureInfo.InvariantCulture, "There is no data in the file [{0}]. Please check that this is the file you want to compress.", file.FileName));
                        }

                        var compressedContent = Compress(file, originalContent);

                        if (!string.IsNullOrEmpty(compressedContent))
                        {
                            if (finalContent == null)
                            {
                                finalContent = new StringBuilder();
                            }

                            finalContent.AppendLine(compressedContent);
                        }

                        // Try and remove this file, if the user requests to do this.
                        try
                        {
                            if (DeleteSourceFiles)
                            {
                                if (LogType == Yahoo.Yui.Compressor.LoggingType.Debug)
                                {
                                    Log.LogMessage("Deleting source file: " + file.FileName);
                                }

                                File.Delete(file.FileName);
                            }
                        }
                        catch (Exception exception)
                        {
                            Log.LogError(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Failed to delete the path/file [{0}]. It's possible the file is locked?",
                                    file.FileName));
                            Log.LogErrorFromException(exception, false);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (exception is EcmaScriptException)
                        {
                            var ecmaException = exception as EcmaScriptException;
                            Log.LogEcmaError(ecmaException);
                            EcmaExceptions.Add(ecmaException);
                        }

                        if (exception is FileNotFoundException)
                        {
                            Log.LogError(string.Format(CultureInfo.InvariantCulture, "ERROR reading file or path [{0}].", file.FileName));
                        }
                        else
                        {
                            // FFS :( Something bad happened.
                            Log.LogError(string.Format(CultureInfo.InvariantCulture, "Failed to read/parse data in file [{0}].", file.FileName));
                        }

                        Log.LogErrorFromException(exception, false);
                    }
                }

                LogMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Finished compressing all {0} file{1}.",
                        SourceFiles.Length,
                        Extensions.ToPluralString(SourceFiles.Length)),
                    true);

                int finalContentLength = finalContent == null ? 0 : finalContent.ToString().Length;

                LogMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Total original file size: {0}. After compression: {1}. Compressed down to {2}% of original size.",
                        totalOriginalContentLength,
                        finalContentLength,
                        (100 - (totalOriginalContentLength - (float)finalContentLength)) / totalOriginalContentLength * 100));

                LogMessage(string.Format(CultureInfo.InvariantCulture, "Compression Type: {0}.", compressionType));
            }

            return finalContent;
        }

        private CompressionType GetCompressionTypeFor(FileSpec file)
        {
            var message = "=> " + file.FileName;
            var actualCompressionType = this.compressionType;
            var overrideType = file.CompressionType;
            if (!string.IsNullOrEmpty(overrideType))
            {
                actualCompressionType = this.ParseCompressionType(overrideType);
                if (actualCompressionType != this.compressionType)
                {
                    message += string.Format(" (CompressionType: {0})", actualCompressionType.ToString());
                }
            }
            this.LogMessage(message, true);
            return actualCompressionType;
        }

        private bool SaveCompressedText(StringBuilder compressedText)
        {
            // Note: compressedText CAN be null or empty, so no check.
            try
            {
                File.WriteAllText(OutputFile, compressedText == null ? string.Empty : compressedText.ToString(), this.Encoding);
                Log.LogMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Compressed content saved to file [{0}].{1}",
                        OutputFile,
                        Environment.NewLine));
            }
            catch (Exception exception)
            {
                // Most likely cause of this exception would be that the user failed to provide the correct path/file
                // or the file is read only, unable to be written, etc.. 
                Log.LogError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Failed to save the compressed text into the output file [{0}]. Please check the path/file name and make sure the file isn't magically locked, read-only, etc..",
                        OutputFile));
                Log.LogErrorFromException(exception, false);

                return false;
            }

            return true;
        }
    }
}

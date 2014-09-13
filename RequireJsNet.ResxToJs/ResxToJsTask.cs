using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Newtonsoft.Json;

using RequireJsNet.ResxToJs.Models;

namespace RequireJsNet.ResxToJs
{
    public class ResxToJsTask : Task
    {
        [Required]
        public string RootResx { get; set; }
        [Required]
        public string RootJs { get; set; }

        public override bool Execute()
        {
            var sw = new Stopwatch();
            sw.Start();
            if (!CheckMainFilesExistance())
            {
                return false;
            }

            try
            {
                var languages = ReadJSFile(RootJs);
                var cultures = GetAvailableCultures(languages.Where(r => r.Value.ToLower() == "true").Select(r => r.Key).ToList());
                GetResources(cultures);
                WriteJsFiles(cultures);
            }
            catch (Exception ex)
            {
                Log.LogMessage(MessageImportance.High, ex.Message);
                return false;
            }

            sw.Stop();
            Log.LogMessage(MessageImportance.High, "Resource files generated in " + sw.ElapsedMilliseconds + "ms.");
            return true;
        }

        private void GetResources(List<ResourceInfo> resouceInfo)
        {
            var rootResInfo = resouceInfo.FirstOrDefault(r => r.CultureShort.ToLower() == "root");
            if (rootResInfo == null)
            {
                throw new Exception("Could not find root resources");
            }

            var xdoc = XDocument.Load(rootResInfo.ResxPath);
            rootResInfo.Resources = xdoc.Descendants("data")
                    .Where(x => x.Element("comment") != null && x.Element("comment").Value.Trim() == "js")
                    .ToDictionary(x => x.Attribute("name").Value, y => y.Element("value").Value);
            var keys = rootResInfo.Resources.Select(r => r.Key).ToList();

            foreach (var resInfoItem in resouceInfo.Where(r => r.CultureShort.ToLower() != "root"))
            {
                if (!File.Exists(resInfoItem.ResxPath))
                {
                    throw new FileNotFoundException("Could not load file " + resInfoItem.ResxPath, resInfoItem.ResxPath);
                }

                var xdocLocalized = XDocument.Load(resInfoItem.ResxPath);
                resInfoItem.Resources = xdocLocalized.Descendants("data")
                        .Where(x => keys.Contains(x.Attribute("name").Value))
                        .ToDictionary(x => x.Attribute("name").Value, y => y.Element("value").Value);
            }
        }

        private bool CheckMainFilesExistance()
        {
            return CheckFileExistance(RootJs) && CheckFileExistance(RootResx);
        }

        private bool CheckFileExistance(string path)
        {
            if (!File.Exists(path))
            {
                Log.LogMessage(MessageImportance.High, "Could not read file " + path);
                return false;
            }

            return true;
        }

        private Dictionary<string, string> ReadJSFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Could not find file " + path, path);
            }

            var lines = File.ReadAllLines(path).ToList();
            var fileToDict = new JsFileToDictionary(lines);
            return fileToDict.GetDictionary();
        }

        private List<ResourceInfo> GetAvailableCultures(List<string> jsCultures)
        {
            var jsFileInfo = new FileInfo(RootJs); 
            var rootJsPath = jsFileInfo.Directory.FullName;
            var jsFilename = jsFileInfo.Name;

            var rootResxPath = new FileInfo(RootResx).Directory.FullName;
            var allResxFiles = Directory.GetFiles(rootResxPath, "*.resx");
            var availableCultures = new List<ResourceInfo>();

            foreach (string resxFile in allResxFiles)
            {
                var file = new FileInfo(resxFile).Name;
                var match = Regex.Match(file, @"\.([a-z]+?)\-", RegexOptions.IgnoreCase);
                var locale = "Root";
                if (match.Success)
                {
                    locale = match.Groups[1].Value;
                }

                if (jsCultures.Any(r => r.ToLower() == locale.ToLower()))
                {
                    availableCultures.Add(new ResourceInfo
                    {
                        CultureShort = locale,
                        ResxPath = resxFile,
                        ProjectedJsPath = Path.Combine(rootJsPath, locale, jsFilename)
                    });
                }
            }

            return availableCultures;
        }

        private void WriteJsFiles(IEnumerable<ResourceInfo> resInfo)
        {
            foreach (var resourceInfo in resInfo)
            {
                var sb = new StringBuilder();
                sb.Append("// DO NOT EDIT" + Environment.NewLine);
                sb.Append("// Autogenerated on " + DateTime.Now + Environment.NewLine);
                sb.Append("// There are " + resourceInfo.Resources.Keys.Count + " keys" + Environment.NewLine);
                sb.Append(GetDefineCall(resourceInfo.Resources));
                var directory = Path.GetDirectoryName(resourceInfo.ProjectedJsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(resourceInfo.ProjectedJsPath))
                {
                    var fileInfo = new FileInfo(resourceInfo.ProjectedJsPath);
                    fileInfo.IsReadOnly = false;
                    fileInfo.Refresh();
                }

                File.WriteAllText(resourceInfo.ProjectedJsPath, sb.ToString());
            }
        }

        private string GetDefineCall(Dictionary<string, string> resources)
        {
            var sb = new StringBuilder();
            sb.Append("define(");
            var js = JsonConvert.SerializeObject(resources, Formatting.Indented);
            sb.Append(js);
            sb.Append(");");
            return sb.ToString();
        }
    }
}

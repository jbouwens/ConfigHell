using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ConfigHell
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var directory = args[0];
            var test = ReSortAppSettings(directory);
        }

        public static XDocument GetLocalAppSettings(String path)
        {
            var sb = new StringBuilder();
            using (var sr = new StreamReader(path))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            var document = sb.ToString();
            var xmlDocument = XDocument.Parse(document);
            return xmlDocument;
        }

        public static List<AppSetting> GetLocalAppSettingsList(String path)
        {
            var xmlDocument = GetLocalAppSettings(path);
            var appSettings = new List<AppSetting>();
            foreach (var element in xmlDocument.Descendants().Where(p => p.HasElements == false))
            {
                appSettings.Add(new AppSetting
                {
                    Name = element.FirstAttribute.Value,
                    Value = element.LastAttribute.Value
                });
            }
            return appSettings;
        }

        public static bool ReSortAppSettings(string path)
        {
            var files = GetMatchingFilesInDirectory("appsettings.config", path);

            var personalAppSettings =
                ConfigurationManager.AppSettings.AllKeys.Select(
                    key => new AppSetting {Name = key, Value = ConfigurationManager.AppSettings[key]}).ToList();

            foreach (var file in files)
            {
                var localAppSettings = GetLocalAppSettingsList(file);
                var appSettingsDictionary = new Dictionary<string, string>();

                foreach (var personalAppSetting in personalAppSettings)
                {
                    foreach (
                        var localAppSetting in
                            localAppSettings.Where(
                                localAppSetting => localAppSetting.Name.Equals(personalAppSetting.Name)))
                    {
                        localAppSetting.Value = personalAppSetting.Value;
                    }
                }

                appSettingsDictionary = localAppSettings.ToDictionary(x => x.Name, x => x.Value);

                var newXml = new XElement("appSettings",
                    appSettingsDictionary.Select(
                        x => new XElement("add", new XAttribute("key", x.Key), new XAttribute("value", x.Value))));
                newXml.Save(file);
            }
            return true;
        }

        public static List<string> GetMatchingFilesInDirectory(String filename, string path)
        {
            var hdDirectoryInWhichToSearch = new DirectoryInfo(path);
            var filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + filename, SearchOption.AllDirectories);
            var files = new List<string>();

            foreach (var foundFile in filesInDir)
            {
                var fullName = foundFile.FullName;
                Console.WriteLine(fullName);
                files.Add(fullName);
            }
            return files;
        }
    }
}
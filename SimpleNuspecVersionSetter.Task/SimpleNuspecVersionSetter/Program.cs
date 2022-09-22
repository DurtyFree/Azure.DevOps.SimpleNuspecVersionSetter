using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using SimpleNuspecVersionSetter.Extensions;

namespace SimpleNuspecVersionSetter
{
    public class Program
    {
        private static readonly Regex RangedVersionRegex = new Regex("([\\[|\\(])(.+)(,)(.+)([]|)])", RegexOptions.Compiled);
        private static readonly Regex SingleVersionRegex = new Regex("([\\[|\\(])(.+)([]|)])", RegexOptions.Compiled);
        
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("RootDirectory path & version to set must be given.");
            }
            string rootDirectory = args[0];
            string version = args[1];

            if (!Directory.Exists(rootDirectory))
            {
                throw new ArgumentException("Given RootDirectory does not exist.");
            }

            Console.WriteLine($"Collecting nuspec files on Root Directory {rootDirectory}");

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Execute(rootDirectory, version);
            watch.Stop();

            Console.WriteLine($"Done in {watch.Elapsed}.");
        }

        public static bool Execute(string rootDirectoryPath, string version)
        {
            DirectoryInfo root = new DirectoryInfo(rootDirectoryPath);
            List<FileInfo> nuspecFiles = root.GetFiles("*.nuspec", SearchOption.AllDirectories).ToList();
            if (!nuspecFiles.Any())
            {
                Console.WriteLine("Could not find any nuspec files!");
                return false;
            }
            Console.WriteLine($"Found {nuspecFiles.Count} nuspec files.");

            foreach (FileInfo nuspecFile in nuspecFiles)
            {
                if (nuspecFile.Exists)
                {
                    Console.WriteLine($"Setting version for file {nuspecFile.FullName} -> {version}");
                    FileAttributes attributes = nuspecFile.Attributes;
                    nuspecFile.Attributes = FileAttributes.Normal;
                    SetNuspecVersionString(nuspecFile, version);
                    nuspecFile.Attributes = attributes;
                }
                else
                {
                    Console.WriteLine($"File {nuspecFile.FullName} does not exist!");
                }
            }
            return true;
        }

        private static void SetNuspecVersionString(FileInfo file, string versionString)
        {
            XmlDocument document = new XmlDocument();
            document.Load(file.FullName);

            if (TryGetNuspecVersionElement(document, out XmlElement element))
            {
                element.InnerText = versionString;
                document.Save(file.FullName);
            }
        }

        private static bool TryGetNuspecVersionElement(XmlDocument document, out XmlElement versionElement)
        {
            XmlNamespaceManager namespaceManager = document.CreateNuspecNamespaceManager();
            versionElement = document.GetVersionElement(namespaceManager);
            return versionElement != null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SimpleNuspecVersionSetter.Models;

namespace SimpleNuspecVersionSetter.Extensions
{
    internal static class NuspecExtensions
    {
        public const string PackageComponentPath = "/packages/package";

        public static XmlNamespaceManager CreateNuspecNamespaceManager(this XmlDocument document)
        {
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace(NuspecConstants.NuspecPefix, NuspecConstants.Namespace);
            return namespaceManager;
        }

        public static XmlElement GetVersionElement(this XmlDocument document, XmlNamespaceManager namespaceManager)
        {
            return document.SelectSingleNode(NuspecConstants.VersionPath, namespaceManager) as XmlElement;
        }

        public static XmlElement GetIdElement(this XmlDocument document, XmlNamespaceManager namespaceManager)
        {
            return document.SelectSingleNode(NuspecConstants.IdPath, namespaceManager) as XmlElement;
        }

        public static bool IsVersionNewerThan(this string version, string otherVersion)
        {
            ComponentVersion versionA = new ComponentVersion(version);
            ComponentVersion versionB = new ComponentVersion(otherVersion);

            return versionA.CompareTo(versionB) > 0;
        }

        public static IEnumerable<Component> GetComponentsFromPackage(this FileInfo packageFile)
        {
            if (!packageFile.Exists)
            {
                yield break;
            }

            XmlDocument document = new XmlDocument();
            document.Load(packageFile.FullName);
            XmlNodeList componentList = document.SelectNodes(PackageComponentPath);
            if (componentList != null)
            {
                foreach (XmlElement component in componentList.OfType<XmlElement>())
                {
                    yield return new Component(component.GetAttribute(NuspecConstants.IdAttribute), component.GetAttribute(NuspecConstants.VersionAttribute));
                }
            }
        }

        public static bool SyncPackage(this FileInfo packageFile, IEnumerable<Component> components)
        {
            if (!packageFile.Exists)
            {
                return false;
            }

            bool changed = false;
            Dictionary<string, string> componentDictionary = components.ToDictionary(c => c.Id, c => c.Version);
            XmlDocument document = new XmlDocument();
            document.Load(packageFile.FullName);
            XmlNodeList componentList = document.SelectNodes(PackageComponentPath);
            if (componentList != null)
            {
                foreach (XmlElement component in componentList.OfType<XmlElement>())
                {
                    if (componentDictionary.TryGetValue(component.GetAttribute(NuspecConstants.IdAttribute), out string version))
                    {
                        string packageVersion = component.GetAttribute(NuspecConstants.VersionAttribute);
                        if (!string.Equals(version, packageVersion, StringComparison.OrdinalIgnoreCase))
                        {
                            changed = true;
                            component.SetAttribute(NuspecConstants.VersionAttribute, version);
                        }
                    }
                }
            }

            if (changed)
            {
                packageFile.Attributes = FileAttributes.Normal;
                packageFile.Delete();
                document.Save(packageFile.FullName);
            }
            return changed;
        }


        public static Component GetComponentFromNuspec(this FileInfo nuspecFile)
        {
            XmlDocument document = new XmlDocument();
            document.Load(nuspecFile.FullName);
            XmlNamespaceManager namespaceManager = CreateNuspecNamespaceManager(document);
            XmlElement idElement = GetIdElement(document, namespaceManager);
            XmlElement versionElement = GetVersionElement(document, namespaceManager);
            if (idElement == null || versionElement == null)
            {
                return null;
            }
            return new Component(idElement.InnerText, versionElement.InnerText);
        }

        public static XmlElement GetMetaDataElement(this XmlDocument document)
        {
            XmlNamespaceManager namespaceManager = document.CreateNuspecNamespaceManager();
            XmlElement element = document.SelectSingleNode(NuspecConstants.MetaDataPath, namespaceManager) as XmlElement;
            if (element == null)
            {
                throw new InvalidOperationException("The meta data element is missing. The given document might not be a nuspec file.");
            }
            return element;
        }

        public static XmlElement GetOrCreateDependencyRoot(this XmlDocument document)
        {
            XmlElement metaData = document.GetMetaDataElement();
            XmlNamespaceManager namespaceManager = document.CreateNuspecNamespaceManager();
            XmlElement dependencyRoot = document.SelectSingleNode(NuspecConstants.DependencyRootPath, namespaceManager) as XmlElement;
            if (dependencyRoot == null)
            {
                dependencyRoot = document.CreateElement(NuspecConstants.DependencyRootName, NuspecConstants.Namespace);
                metaData.AppendChild(dependencyRoot);
            }
            return dependencyRoot;
        }
    }
}

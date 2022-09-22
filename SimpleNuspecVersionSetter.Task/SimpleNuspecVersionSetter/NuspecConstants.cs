namespace SimpleNuspecVersionSetter
{
    public static class NuspecConstants
    {
        public const string Namespace = "http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd";
        public const string NuspecPefix = "nuspec";
        public const string MetaDataPath = "/nuspec:package/nuspec:metadata";
        public const string VersionPath = MetaDataPath + "/nuspec:version";
        public const string IdPath = MetaDataPath + "/nuspec:id";
        public const string DependencyRootPath = MetaDataPath + "/" + DependencyRootName;
        public const string DependencyRootName = "nuspec:dependencies";
        public const string VersionAttribute = "version";
        public const string IdAttribute = "id";
        public const string DependencyNodeName = "nuspec:dependency";
        public const string DependencyIdQuery = DependencyRootPath + "/" + DependencyNodeName + "[@" + IdAttribute + "='{0}']";


        // package config constants
        public const string PackageComponentPath = "/packages/package";
    }
}

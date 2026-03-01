using System.Xml.Linq;
using Bam;
using Bam.Generators;

namespace BamDb
{
    /// <summary>
    /// Wraps a raw <see cref="DaoRepoGenerationConfig"/> with resolved runtime paths.
    /// The original config (from yaml) is never mutated; all path resolution lives here.
    /// </summary>
    public class DaoRepoGenerationContext
    {
        public DaoRepoGenerationContext(DaoRepoGenerationConfig config, FileInfo configFile)
        {
            OriginalConfig = config;
            ConfigFile = configFile;
            ProjectFile = DiscoverProject(configFile);
            ResolvedWriteSourceTo = ResolveWriteSourceTo(configFile, config.WriteSourceTo);
            ResolvedTypeAssembly = ProjectFile != null
                ? DeriveTypeAssemblyPath(ProjectFile)
                : new HomePath(config.TypeAssembly).Resolve();
        }

        public DaoRepoGenerationConfig OriginalConfig { get; }

        public FileInfo ConfigFile { get; }

        public FileInfo? ProjectFile { get; }

        public string ResolvedWriteSourceTo { get; }

        public string ResolvedTypeAssembly { get; }

        /// <summary>
        /// Creates a new <see cref="DaoRepoGenerationConfig"/> with resolved absolute paths,
        /// suitable for passing to <see cref="HandlebarsSchemaRepositoryGenerator"/>.
        /// The original config is not mutated.
        /// </summary>
        public DaoRepoGenerationConfig ToResolvedConfig()
        {
            return new DaoRepoGenerationConfig
            {
                TypeAssembly = ResolvedTypeAssembly,
                SchemaName = OriginalConfig.SchemaName,
                FromNamespace = OriginalConfig.FromNamespace,
                WriteSourceTo = ResolvedWriteSourceTo,
                CheckForIds = OriginalConfig.CheckForIds,
                UseAsync = OriginalConfig.UseAsync,
                UseInheritanceSchema = OriginalConfig.UseInheritanceSchema,
                WarningsAsErrors = OriginalConfig.WarningsAsErrors
            };
        }

        private static string ResolveWriteSourceTo(FileInfo configFile, string writeSourceTo)
        {
            if (writeSourceTo.StartsWith("./"))
            {
                return Path.Combine(
                    configFile.Directory!.FullName,
                    writeSourceTo.TruncateFront(2)
                );
            }

            if (writeSourceTo.StartsWith("~/"))
            {
                return new HomePath(writeSourceTo).Resolve();
            }

            return writeSourceTo;
        }

        /// <summary>
        /// Derives the TypeAssembly path from the project file's TargetFramework.
        /// Pattern: ~/.bam/build/Debug/{projectName}/{targetFramework}/{projectName}.dll
        /// </summary>
        private static string DeriveTypeAssemblyPath(FileInfo projectFile)
        {
            string projectName = Path.GetFileNameWithoutExtension(projectFile.Name);
            string targetFramework = ParseTargetFramework(projectFile) ?? "net10.0";
            string homePath = $"~/.bam/build/Debug/{projectName}/{targetFramework}/{projectName}.dll";
            return new HomePath(homePath).Resolve();
        }

        private static string? ParseTargetFramework(FileInfo projectFile)
        {
            try
            {
                XDocument doc = XDocument.Load(projectFile.FullName);
                return doc.Descendants("TargetFramework").FirstOrDefault()?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Walks up from the config file directory looking for a .csproj file.
        /// </summary>
        private static FileInfo? DiscoverProject(FileInfo configFile)
        {
            DirectoryInfo? dir = configFile.Directory;
            while (dir != null)
            {
                FileInfo[] csprojFiles = dir.GetFiles("*.csproj");
                if (csprojFiles.Length > 0)
                {
                    return csprojFiles[0];
                }

                dir = dir.Parent;
            }

            return null;
        }
    }
}

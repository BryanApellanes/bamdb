using System.Diagnostics;
using Bam;
using Bam.CommandLine;
using Bam.Console;
using Bam.Generators;
using Bam.Logging;

namespace BamDb
{
    public class DaoRepoGenerationService
    {
        private readonly ILogger _logger;
        private readonly HashSet<string> _builtProjects = new(StringComparer.OrdinalIgnoreCase);

        public DaoRepoGenerationService(ILogger logger)
        {
            _logger = logger;
        }

        public void RegenerateAll(string rootDirectory)
        {
            DirectoryInfo rootDir = new DirectoryInfo(rootDirectory);
            if (!rootDir.Exists)
            {
                Message.PrintLine("Directory not found: {0}", ConsoleColor.Red, rootDir.FullName);
                return;
            }

            Message.PrintLine("Scanning for *.dao-repo-gen.yaml in {0}...", rootDir.FullName);

            FileInfo[] configFiles = rootDir.GetFiles("*.dao-repo-gen.yaml", SearchOption.AllDirectories);
            if (configFiles.Length == 0)
            {
                Message.PrintLine("No *.dao-repo-gen.yaml files found.");
                return;
            }

            Message.PrintLine("Found {0} config file(s).", configFiles.Length);
            Message.PrintLine();

            for (int i = 0; i < configFiles.Length; i++)
            {
                FileInfo configFile = configFiles[i];
                Message.PrintLine("[{0}/{1}] {2}", i + 1, configFiles.Length, configFile.Name);

                try
                {
                    ProcessConfigFile(configFile);
                }
                catch (Exception ex)
                {
                    _logger.Error("Error processing {0}: {1}", ex, configFile.Name);
                    Message.PrintLine("  Error: {0}", ConsoleColor.Red, ex.Message);
                }

                Message.PrintLine();
            }
        }

        private void ProcessConfigFile(FileInfo configFile)
        {
            DaoRepoGenerationConfig config = DaoRepoGenerationConfig.ReadFrom(configFile);
            DaoRepoGenerationContext context = new DaoRepoGenerationContext(config, configFile);

            if (context.ProjectFile == null)
            {
                _logger.Warning("No .csproj found walking up from {0}", configFile.Directory!.FullName);
                Message.PrintLine("  Skipping: no .csproj found.", ConsoleColor.Yellow);
                return;
            }

            Message.PrintLine("  Project: {0}", context.ProjectFile.FullName);

            if (!BuildProject(context.ProjectFile.FullName))
            {
                Message.PrintLine("  Skipping: build failed.", ConsoleColor.Red);
                return;
            }

            if (!File.Exists(context.ResolvedTypeAssembly))
            {
                Message.PrintLine("  Warning: TypeAssembly not found at {0}", ConsoleColor.Yellow, context.ResolvedTypeAssembly);
                Message.PrintLine("  Expected path derived from {0}", ConsoleColor.Yellow, context.ProjectFile.FullName);
                return;
            }

            DaoRepoGenerationConfig resolvedConfig = context.ToResolvedConfig();
            Message.PrintLine("  Generating DAO source to {0}", resolvedConfig.WriteSourceTo);

            HandlebarsSchemaRepositoryGenerator generator = new HandlebarsSchemaRepositoryGenerator(resolvedConfig, _logger);
            generator.GenerateSource();

            OutputWarnings(generator);
            Message.PrintLine("  Done.", ConsoleColor.Green);
        }

        private bool BuildProject(string projectPath)
        {
            if (_builtProjects.Contains(projectPath))
            {
                Message.PrintLine("  Building project... (already built, skipping)");
                return true;
            }

            Message.PrintLine("  Building project...");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            ProcessOutput output = startInfo.Run();

            if (output.ExitCode != 0)
            {
                _logger.Error("Build failed for {0}:\n{1}", projectPath, output.StandardError);
                return false;
            }

            _builtProjects.Add(projectPath);
            Message.PrintLine("  Build succeeded.");
            return true;
        }

        private void OutputWarnings(HandlebarsSchemaRepositoryGenerator generator)
        {
            if (generator.Warnings.MissingKeyColumns.Length > 0)
            {
                Message.PrintLine("  Missing key/id columns:", ConsoleColor.Yellow);
                foreach (var kc in generator.Warnings.MissingKeyColumns)
                {
                    Message.PrintLine("    {0}", ConsoleColor.DarkYellow, kc.TableClassName);
                }
            }

            if (generator.Warnings.MissingForeignKeyColumns.Length > 0)
            {
                Message.PrintLine("  Missing ForeignKey columns:", ConsoleColor.Cyan);
                foreach (var fkc in generator.Warnings.MissingForeignKeyColumns)
                {
                    Message.PrintLine("    {0}.{1}", ConsoleColor.DarkCyan, fkc.TableClassName, fkc.Name);
                }
            }

            if (generator.TypeSchemaWarnings.Count > 0)
            {
                foreach (var warning in generator.TypeSchemaWarnings)
                {
                    Message.PrintLine("  {0}", ConsoleColor.Yellow, warning.ToString());
                }
            }
        }
    }
}

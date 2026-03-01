using Bam.Console;
using Bam;
using Bam.Data.Schema;
using Bam.Generators;
using Bam.Data.Repositories;
using System.Reflection;
using Bam.Data;
using Bam.DependencyInjection;
using Bam.Logging;
using Bam.Services;
using Bam.Shell;

namespace BamDb
{
    /// <summary>
    /// Console menu container that provides bamdb commands for initializing configuration,
    /// generating schema repositories, and generating DAO code from assembly namespaces.
    /// </summary>
    [ConsoleMenu("bamdb options")]
    public class BamDbMenuContainer : ConsoleMenuContainer
    {
        /// <summary>
        /// Initializes a new instance with the specified service registry.
        /// </summary>
        /// <param name="serviceRegistry">The service registry for dependency resolution.</param>
        public BamDbMenuContainer(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        /// <summary>
        /// Configures the service registry with default Handlebars-based DAO generation services.
        /// </summary>
        /// <param name="serviceRegistry">The service registry to configure.</param>
        /// <returns>The configured service registry.</returns>
        public override ServiceRegistry Configure(ServiceRegistry serviceRegistry)
        {
            return serviceRegistry
                .For<IDaoCodeWriter>().Use<HandlebarsCSharpDaoCodeWriter>()
                .For<ISchemaProvider>().Use<SchemaProvider>()
                .For<IDaoGenerator>().Use<DaoGenerator>()
                .For<IWrapperGenerator>().Use<HandlebarsWrapperGenerator>()
                .For<IDaoRepository>().Use<DaoRepository>();
        }
        
        /// <summary>
        /// Creates a default DAO repository generation configuration file and writes it to the default path.
        /// </summary>
        [ConsoleCommand("initConfig")]
        [MenuItem]
        public void InitConfig()
        {
            DaoRepoGenerationConfig config = new DaoRepoGenerationConfig();
            FileInfo file = new FileInfo(DaoRepoGenerationConfig.DefaultFilePath);
            config.ToYaml().SafeAppendToFile(file.FullName);
            Message.PrintLine("Dao repository generation configuration written to: {0}", file.FullName);
        }

        /// <summary>
        /// Generates DAO schema repository source files using configuration from a YAML file.
        /// Reads the config from the --config argument or the default path, and writes output to --output or -o if specified.
        /// </summary>
        [ConsoleCommand("generateSchemaRepository")]
        [MenuItem]
        public void GenerateSchemaRepository()
        {
            IDaoRepoGenerationConfig? config = null; 
            if (BamConsoleContext.Current.Arguments.Contains("config"))
            {
                config = DaoRepoGenerationConfig.ReadFrom(BamConsoleContext.Current.Arguments["config"]);
            }
            else
            {
                config = DaoRepoGenerationConfig.ReadFile();
            }

            if (BamConsoleContext.Current.Arguments.Contains("output"))
            {
                config.WriteSourceTo = BamConsoleContext.Current.Arguments["output"];
            }
            else if (BamConsoleContext.Current.Arguments.Contains("o"))
            {
                config.WriteSourceTo = BamConsoleContext.Current.Arguments["o"];
            }

            HandlebarsSchemaRepositoryGenerator schemaRepositoryGenerator = new HandlebarsSchemaRepositoryGenerator(config);
            schemaRepositoryGenerator.GenerateSource();
            Message.PrintLine("Wrote source to {0}", config.WriteSourceTo);
        }
        
        /// <summary>
        /// Regenerates all DAO code by recursively scanning for *.dao-repo-gen.yaml files,
        /// discovering their owning .csproj, building the project, and generating source.
        /// </summary>
        [ConsoleCommand("regenerateAll")]
        [MenuItem]
        public void RegenerateAll()
        {
            string rootDirectory = ".";
            if (BamConsoleContext.Current.Arguments.Contains("regenerateAll"))
            {
                string argValue = BamConsoleContext.Current.Arguments["regenerateAll"];
                if (!string.IsNullOrEmpty(argValue))
                {
                    rootDirectory = argValue;
                }
            }

            ILogger logger = Log.Default ?? new ConsoleLogger();
            DaoRepoGenerationService service = new DaoRepoGenerationService(logger);
            service.RegenerateAll(rootDirectory);
        }

        /// <summary>
        /// Interactively prompts for an assembly path, namespace, schema name, and output path,
        /// then generates DAO source code for the types in the specified namespace.
        /// </summary>
        [MenuItem]
        public void GenerateDataAccessCodeFromAssemblyNamespace()
        {
            string assemblyPath = Prompt.Show("Enter the path to the assembly.");
            string nameSpace = Prompt.Show("Enter the namespace.");
            string schemaName = BamConsoleContext.Current.Arguments["schemaName"];
            schemaName = schemaName.Or(Prompt.Show("Enter a name for the schema"));
            string output = Prompt.Show("Enter the path to write source to.");
            if (string.IsNullOrEmpty(output))
            {
                output = "./.bam/gen/Dao";
            }
            TypeToDaoGenerator typeToDaoGenerator = new ServiceRegistry()
                .For<IDaoCodeWriter>().Use<HandlebarsCSharpDaoCodeWriter>()
                .For<ISchemaProvider>().Use<SchemaProvider>()
                .For<IDaoGenerator>().Use<DaoGenerator>()
                .For<IWrapperGenerator>().Use<HandlebarsWrapperGenerator>()
                .For<IDaoRepository>().Use<DaoRepository>()
                .For<ILogger>().Use(Log.Default!)
                .Get<TypeToDaoGenerator>();

            Assembly assembly = Assembly.LoadFile(assemblyPath);
            typeToDaoGenerator.SchemaName = schemaName;
            typeToDaoGenerator.AddTypes(assembly.GetTypes().Where(type=> type.Namespace != null && type.Namespace.Equals(nameSpace)));
            typeToDaoGenerator.GenerateSource(output);
        }


    }
}

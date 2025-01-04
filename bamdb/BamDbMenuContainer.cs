using Bam.Console;
using Bam;
using Bam.CoreServices;
using Bam.Data.Schema;
using Bam.Generators;
using Bam.Data.Repositories;
using System.Reflection;
using Bam.Data;
using Bam.Logging;
using Bam.Shell;
using MongoDB.Driver.Linq;

namespace BamDb
{
    [ConsoleMenu("bamdb options")]
    public class BamDbMenuContainer : ConsoleMenuContainer
    {
        public BamDbMenuContainer(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        public override ServiceRegistry Configure(ServiceRegistry serviceRegistry)
        {
            return serviceRegistry
                .For<IDaoCodeWriter>().Use<HandlebarsCSharpDaoCodeWriter>()
                .For<ISchemaProvider>().Use<SchemaProvider>()
                .For<IDaoGenerator>().Use<DaoGenerator>()
                .For<IWrapperGenerator>().Use<HandlebarsWrapperGenerator>()
                .For<IDaoRepository>().Use<DaoRepository>();
        }
        
        [ConsoleCommand("initConfig")]
        [MenuItem]
        public void InitConfig()
        {
            DaoRepoGenerationConfig config = new DaoRepoGenerationConfig();
            FileInfo file = new FileInfo(DaoRepoGenerationConfig.DefaultFilePath);
            config.ToYaml().SafeAppendToFile(file.FullName);
            Message.PrintLine("Dao repository generation configuration written to: {0}", file.FullName);
        }

        [ConsoleCommand("generateSchemaRepository")]
        [MenuItem]
        public void GenerateSchemaRepository()
        {
            // Load the dao-repo-gen.yaml file in the current directory
            IDaoRepoGenerationConfig config = DaoRepoGenerationConfig.LoadDefault();
            HandlebarsSchemaRepositoryGenerator schemaRepositoryGenerator = new HandlebarsSchemaRepositoryGenerator(config);
            schemaRepositoryGenerator.GenerateSource();
        }
        
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
                .For<ILogger>().Use(Log.Default)
                .Get<TypeToDaoGenerator>();

            Assembly assembly = Assembly.LoadFile(assemblyPath);
            typeToDaoGenerator.SchemaName = schemaName;
            typeToDaoGenerator.AddTypes(assembly.GetTypes().Where(type=> type.Namespace != null && type.Namespace.Equals(nameSpace)));
            typeToDaoGenerator.GenerateSource(output);
        }


    }
}

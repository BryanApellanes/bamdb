using Bam.Console;
using Bam;
using Bam.CoreServices;
using Bam.Data.Schema;
using Bam.Generators;
using Bam.Data.Repositories;

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
        
        [ConsoleCommand("generateDaoClassesFromJsLiteral")]
        public void GenerateDaoClassesFromJsLiteralFile()
        {
            Message.PrintLine("Generate from literal file");
            Expect.Fail("This is not fully implemented");
        }

        [ConsoleCommand]
        public void GenerateDaoClassesFromCSharpSource()
        {
            Message.PrintLine("Generate dao classes from CSharp Source");
            Expect.Fail("This is not fully implemented");
        }


    }
}

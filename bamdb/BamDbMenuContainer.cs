using Bam.Console;
using Bam.Net;
using Bam.Net.CoreServices;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Data.Schema;
using Bam.Generators;
using Bam.Net.Data.Repositories;
using Bam.Net.Data.Schema;

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

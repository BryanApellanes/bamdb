using Bam.Console;
using Bam.Net;
using Bam.Net.CoreServices;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BamDb
{
    [ConsoleMenu("bamdb options")]
    public class ConsoleMenuContainer : Bam.Console.ConsoleMenuContainer
    {
        public ConsoleMenuContainer(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
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

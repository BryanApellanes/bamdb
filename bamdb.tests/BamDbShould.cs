using BamDb;
using Bam.DependencyInjection;
using Bam.Test;

namespace BamDb.Tests
{
    [UnitTestMenu("BamDb Should", Selector = "bds")]
    public class BamDbShould : UnitTestMenuContainer
    {
        public BamDbShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        [UnitTest]
        public void HaveExpectedDefaultPorts()
        {
            When.A<DefaultPorts>("has expected port values",
                DefaultPorts.Postgres,
                (port) => port)
            .TheTest
            .ShouldPass(because =>
            {
                because.ItsTrue("MySql port is 3306", (int)DefaultPorts.MySql == 3306);
                because.ItsTrue("Postgres port is 5432", (int)DefaultPorts.Postgres == 5432);
                because.ItsTrue("MsSql port is 1433", (int)DefaultPorts.MsSql == 1433);
                because.ItsTrue("Oracle port is 1521", (int)DefaultPorts.Oracle == 1521);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void DefineContainerDatabaseTypes()
        {
            When.A<ContainerDatabaseTypes>("defines expected database types",
                ContainerDatabaseTypes.MySql,
                (dbType) => dbType)
            .TheTest
            .ShouldPass(because =>
            {
                because.ItsTrue("MySql is defined", Enum.IsDefined(typeof(ContainerDatabaseTypes), ContainerDatabaseTypes.MySql));
                because.ItsTrue("Postgres is defined", Enum.IsDefined(typeof(ContainerDatabaseTypes), ContainerDatabaseTypes.Postgres));
                because.ItsTrue("MsSql is defined", Enum.IsDefined(typeof(ContainerDatabaseTypes), ContainerDatabaseTypes.MsSql));
                because.ItsTrue("Oracle is defined", Enum.IsDefined(typeof(ContainerDatabaseTypes), ContainerDatabaseTypes.Oracle));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }
    }
}

using Bam.Data.Repositories;
using Bam.DependencyInjection;
using Bam.Protocol.Data.Common.Dao.Repository;
using Bam.Test;

namespace BamDb.Tests
{
    [UnitTestMenu("AsyncDaoRepository Should", Selector = "adr")]
    public class AsyncDaoRepositoryShould : UnitTestMenuContainer
    {
        public AsyncDaoRepositoryShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        [UnitTest]
        public void InheritFromDaoRepository()
        {
            When.A<AsyncDaoRepository>("inherits from DaoRepository",
                new AsyncDaoRepository(),
                (repo) => repo)
            .TheTest
            .ShouldPass(because =>
            {
                because.ItsTrue("is DaoRepository", new AsyncDaoRepository() is DaoRepository);
                because.ItsTrue("is IAsyncRepository", new AsyncDaoRepository() is IAsyncRepository);
                because.ItsTrue("is ISchemaRepository", new AsyncDaoRepository() is ISchemaRepository);
                because.ItsTrue("is IAsyncSchemaRepository", new AsyncDaoRepository() is IAsyncSchemaRepository);
                because.ItsTrue("is IDaoRepository", new AsyncDaoRepository() is IDaoRepository);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void BeBaseClassOfGeneratedRepositories()
        {
            When.A<CommonSchemaRepository>("is base class of generated repositories",
                new CommonSchemaRepository(),
                (repo) => repo)
            .TheTest
            .ShouldPass(because =>
            {
                var repo = new CommonSchemaRepository();
                because.ItsTrue("CommonSchemaRepository is AsyncDaoRepository", repo is AsyncDaoRepository);
                because.ItsTrue("CommonSchemaRepository is DaoRepository", repo is DaoRepository);
                because.ItsTrue("CommonSchemaRepository is IAsyncRepository", repo is IAsyncRepository);
                because.ItsTrue("CommonSchemaRepository is ISchemaRepository", repo is ISchemaRepository);
                because.ItsTrue("CommonSchemaRepository is IAsyncSchemaRepository", repo is IAsyncSchemaRepository);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }
    }
}

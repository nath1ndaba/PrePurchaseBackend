
namespace Infrastructure.Unit.Tests.Database
{
    [Collection("Database")]
    public class RepositoryManagerTests
    {
        public class TestModel1 { }
        public class TestModel2 { }
        private readonly RepositoryManager _repositoryManager;
        private readonly DatabaseFixture _databaseFixture;

        public RepositoryManagerTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
            _repositoryManager = databaseFixture.ServiceProvider.GetRequiredService<RepositoryManager>();
        }

        [Theory]
        [InlineData(typeof(IDatabaseContext))]
        public void Can_GetService_ShouldPass(Type expectedService)
        {
            // Arrange

            var methodInfo = typeof(RepositoryManager).GetMethod(nameof(RepositoryManager.GetService));
            var methodDef = methodInfo!.MakeGenericMethod(expectedService);

            // Act

            var service = methodDef.Invoke(_repositoryManager, null);

            // Assert

            service.Should().BeAssignableTo(expectedService);
        }

        [Theory]
        [InlineData(typeof(TestModel1), typeof(IRepository<TestModel1>))]
        [InlineData(typeof(TestModel2), typeof(IRepository<TestModel2>))]
        public void Can_GetRepository_ShouldPass(Type type, Type expectedRepository)
        {
            // Arrange

            var methodInfo = typeof(RepositoryManager).GetMethod(nameof(RepositoryManager.GetRepository));
            var methodDef = methodInfo!.MakeGenericMethod(type);

            // Act

            var repository = methodDef.Invoke(_repositoryManager, null);

            // Assert

            repository.Should().BeAssignableTo(expectedRepository);
        }


        [Theory]
        [InlineData(typeof(TestModel1), typeof(IMongoCollection<TestModel1>))]
        [InlineData(typeof(TestModel2), typeof(IMongoCollection<TestModel2>))]
        public void Can_GetCollection_ShouldPass(Type type, Type expectedCollection)
        {
            // Arrange

            var methodInfo = typeof(RepositoryManager).GetMethod(nameof(RepositoryManager.GetCollection));
            var methodDef = methodInfo!.MakeGenericMethod(type);

            // Act

            var collection = methodDef.Invoke(_repositoryManager, new string?[] {null});

            // Assert

            collection.Should().BeAssignableTo(expectedCollection);
        }
    }
}

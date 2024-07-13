
namespace Infrastructure.Unit.Tests.Database
{
    [Collection("Database")]
    public class MongoDBContextTests
    {
        private readonly DatabaseFixture _databaseFixture;

        public class TestModel1 { }
        public class TestModel2 { }

        public MongoDBContextTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
        }

        [Theory()]
        [InlineData(typeof(TestModel1), "Test")]
        [InlineData(typeof(TestModel2), "Test")]
        public void GetName_FromType_ShouldPass(Type modelType, string expectedName)
        {
            MongoDbContext.GetName(modelType).Should().Be(expectedName);
        }

        [Theory()]
        [InlineData(typeof(TestModel1) , typeof(IMongoCollection<TestModel1>))]
        [InlineData(typeof(TestModel2), typeof(IMongoCollection<TestModel2>))]
        public void GetCollection_ShouldReturnMongoCollection(Type typeParam, Type expectedCollection)
        {
            // Arrange

            var methodName = nameof(MongoDbContext.GetCollection);
            var methodInfo= typeof(MongoDbContext).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            var methodDef = methodInfo!.MakeGenericMethod(typeParam);

            // Act

            var collection = methodDef.Invoke(_databaseFixture.DbContext, new string?[] {null});
            // Assert

            collection.Should().BeAssignableTo(expectedCollection);
        }
    }
}

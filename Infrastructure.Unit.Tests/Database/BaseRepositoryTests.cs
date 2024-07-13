
namespace Infrastructure.Unit.Tests.Database
{
    [TestCaseOrderer("Infrastructure.Unit.Tests.Orderers.PriorityMethodOrderer", "Infrastructure.Unit.Tests")]
    [Collection("Database")]
    public class BaseRepositoryTests
    {
        public record TestModel1
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public string Name { get; set; }

            public TestModel1() { }

            public TestModel1(string id)
            {
                Id = ObjectId.Parse(id);
            }

            public TestModel1(string id, string name)
            {
                Id = ObjectId.Parse(id);
                Name = name;
            }
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly DatabaseFixture _databaseFixture;

        public BaseRepositoryTests(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
            _serviceProvider = _databaseFixture.ServiceProvider;
        }

        [Theory, Priority(0)]
        [InlineData("612c6b058d26f0c9785607dd", "user1")]
        [InlineData("612c6b058d26f0c9785617dd", "user2")]
        [InlineData("612c6b058d26f0c9785627dd", "user3")]
        [InlineData("612c6b058d26f0c9785637dd", "user1")]
        public async Task Can_InsertOne_ShouldPass(string id, string name)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            TestModel1 model = new(id) {Name = name };
            // Act
            await repo.Insert(model);
        }

        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd")]
        [InlineData("612c6b058d26f0c9785617dd")]
        [InlineData("612c6b058d26f0c9785627dd")]
        public async Task Can_FindById_ShouldPass(string id)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            // Act
            var inserted = await repo.FindById(id);

            // Assert

            inserted.Id.Should().Be(ObjectId.Parse(id));
        }

        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd")]
        [InlineData("612c6b058d26f0c9785617dd")]
        [InlineData("612c6b058d26f0c9785627dd")]
        public async Task Can_FindOneUsingExpressionOneParameter_ShouldPass(string expectedId)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            // Act

            var found = await repo.FindOne(x => x.Id == ObjectId.Parse(expectedId));

            // Assert

            found.Id.Should().Be(ObjectId.Parse(expectedId));
        }

        [Theory, Priority]
        [InlineData("User1", "user1")]
        [InlineData("uSer2", "user2")]
        [InlineData("usEr3", "user3")]
        public async Task Can_FindOneUsingExpressionOneParameter_CaseInsesitive_ShouldPass(string name, string expectedName)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            // Act

            var found = await repo.FindOne(x => x.Name == name.ToLowerInvariant());

            // Assert

            found.Name.Should().Be(expectedName);
        }

        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd", "user1")]
        [InlineData("612c6b058d26f0c9785617dd", "user2")]
        [InlineData("612c6b058d26f0c9785627dd", "user3")]
        public async Task Can_FindOneUsingExpressionTwoParameter_AndOperation_ShouldPass(string expectedId, string expectedName)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            TestModel1 expectedModel = new(expectedId) { Name = expectedName };
            // Act

            var found = await repo.FindOne(x => x.Id == ObjectId.Parse(expectedId) && x.Name == expectedName);

            // Assert

            found.Should().Be(expectedModel);
        }

        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd", "user1")]
        [InlineData("612c6b058d26f0c9785617dd", "user2")]
        [InlineData("612c6b058d26f0c9785627dd", "user3")]
        public async Task Can_FindOneUsingExpressionTwoParameter_OrOperation_ShouldPass(string expectedId, string expectedName)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            TestModel1 expectedModel = new(expectedId) { Name = expectedName };
            // Act

            var found = await repo.FindOne(x => x.Id == ObjectId.Parse(expectedId) || x.Name == expectedName);

            // Assert

            found.Should().Be(expectedModel);
        }

        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd")]
        [InlineData("612c6b058d26f0c9785617dd")]
        [InlineData("612c6b058d26f0c9785627dd")]
        public async Task Can_FindOneUsingQueryBuilderOneParameter_ShouldPass(string expectedId)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            var queryBuilder = _serviceProvider.GetRequiredService<IQueryBuilder<TestModel1>>();
            // Act

            var found = await repo.FindOne(queryBuilder.Eq(x => x.Id, ObjectId.Parse(expectedId)));

            // Assert

            found.Id.Should().Be(ObjectId.Parse(expectedId));
        }

        [Theory, Priority]
        [InlineData("User1", "user1")]
        [InlineData("uSer2", "user2")]
        [InlineData("usEr3", "user3")]
        public async Task Can_FindOneUsingQueryBuilderOneParameter_CaseInsesitive_ShouldPass(string name, string expectedName)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            var queryBuilder = _serviceProvider.GetRequiredService<IQueryBuilder<TestModel1>>();
            // Act

            var found = await repo.FindOne(queryBuilder.Eq(x => x.Name, name.ToLowerInvariant()));

            // Assert

            found.Name.Should().Be(expectedName);
        }

        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd", "user1")]
        [InlineData("612c6b058d26f0c9785617dd", "user2")]
        [InlineData("612c6b058d26f0c9785627dd", "user3")]
        [InlineData("612c6b058d26f0c9785637dd", "user1")]
        public async Task Can_FindOneUsingQueryBuilderTwoParameter_AndOperation_ShouldPass(string expectedId, string expectedName)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            var queryBuilder = _serviceProvider.GetRequiredService<IQueryBuilder<TestModel1>>();
            TestModel1 expectedModel = new(expectedId) { Name = expectedName };
            // Act

            var found1Task = repo.FindOne(queryBuilder.Eq(x => x.Name, expectedName).And(x => x.Id, ObjectId.Parse(expectedId)));
            var found2Task = repo.FindOne(queryBuilder.Eq(x => x.Id, ObjectId.Parse(expectedId)).And(x => x.Name, expectedName));

            var result = await Task.WhenAll(found1Task, found2Task);

            // Assert

            result.Select(x => x.Should().Be(expectedModel));
        }

        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd", "user1")]
        [InlineData("612c6b058d26f0c9785617dd", "user2")]
        [InlineData("612c6b058d26f0c9785627dd", "user3")]
        public async Task Can_FindOneUsingQueryBuilderTwoParameter_OrOperation_ShouldPass(string expectedId, string expectedName)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            var queryBuilder = _serviceProvider.GetRequiredService<IQueryBuilder<TestModel1>>();
            TestModel1 expectedModel = new(expectedId) { Name = expectedName };
            // Act

            var found1Task = repo.FindOne(queryBuilder.Eq(x => x.Name, expectedName).Or(x => x.Id, ObjectId.Parse(expectedId)));
            var found2Task = repo.FindOne(queryBuilder.Eq(x => x.Id, ObjectId.Parse(expectedId)).Or(x => x.Name, expectedName));
            var found3Task = repo.FindOne(queryBuilder.Eq(x => x.Name, expectedName));
            var found4Task = repo.FindOne(queryBuilder.Eq(x => x.Name, expectedName));

            var result = await Task.WhenAll(found1Task, found2Task, found3Task, found4Task);

            // Assert

            result.Select(x => x.Should().Be(expectedModel));
        }

        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd")]
        [InlineData("612c6b058d26f0c9785617dd")]
        [InlineData("612c6b058d26f0c9785627dd")]
        public async Task Can_FindUsingExpressionOneParameter_ShouldPass(string expectedId)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            // Act

            var found = await repo.Find(x => x.Id == ObjectId.Parse(expectedId));

            // Assert
            found.Select(x => x.Id.Should().Be(ObjectId.Parse(expectedId)));
        }

        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd", "612c6b058d26f0c9785617dd", "612c6b058d26f0c9785627dd")]
        public async Task Can_FindUsingExpressionMultipleParameters_ShouldPass(params string[] expectedIds)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            // Act
            var expectedBsonIds = expectedIds.Select(ObjectId.Parse);
            var found = await repo.Find(x => expectedBsonIds.Contains(x.Id));

            // Assert
            found.Select(x => expectedBsonIds.Should().Contain(x.Id));
            found.Should().HaveCount(expectedIds.Length);
        }

        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd", "user1")]
        [InlineData("612c6b058d26f0c9785617dd", "user2")]
        [InlineData("612c6b058d26f0c9785627dd", "user3")]
        public async Task Can_FindUsingExpressionTwoParameter_AndOperation_ShouldPass(string expectedId, string expectedName)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            TestModel1 expectedModel = new(expectedId) { Name = expectedName };
            // Act

            var found = await repo.FindOne(x => x.Id == ObjectId.Parse(expectedId) && x.Name == expectedName);

            // Assert

            found.Should().Be(expectedModel);
        }
        
        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd", "user1")]
        [InlineData("612c6b058d26f0c9785617dd", "user2")]
        [InlineData("612c6b058d26f0c9785627dd", "user3")]
        public async Task Can_FindUsingExpressionTwoParameter_OrOperation_ShouldPass(string expectedId, string expectedName)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            TestModel1 expectedModel = new(expectedId) { Name = expectedName };
            // Act

            var found = await repo.FindOne(x => x.Id == ObjectId.Parse(expectedId) || x.Name == expectedName);

            // Assert

            found.Should().Be(expectedModel);
        }

        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd", "612c6b058d26f0c9785617dd", "612c6b058d26f0c9785627dd")]
        public async Task Can_FindUsingQueryBuilder_InList_ShouldPass(params string[] expectedIds)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            var queryBuilder = _serviceProvider.GetRequiredService<IQueryBuilder<TestModel1>>();
            // Act

            var expectedObjIds = expectedIds.Select(ObjectId.Parse);
            var found = await repo.Find(queryBuilder.In(x => x.Id, expectedObjIds));

            // Assert
            found.Select(x => expectedObjIds.Should().Contain(x.Id));
            found.Should().HaveCount(expectedIds.Length);
        }

        [Theory, Priority]
        [InlineData(new string[] { "612c6b058d26f0c9785607dd", "612c6b058d26f0c9785637dd" }, "user1")]
        public async Task Can_FindUsingQueryBuilder_AndInList_ShouldPass(params object[] values)
        {
            // Arrange
            ObjectId[] expectedObjIds = values.OfType<string[]>().First().Select(x => ObjectId.Parse(x)).ToArray();
            string expectedName = values.OfType<string>().First();

            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            var queryBuilder = _serviceProvider.GetRequiredService<IQueryBuilder<TestModel1>>();

            queryBuilder = queryBuilder.Eq(x => x.Name, expectedName)
                .And<CompanyEmployee>(queryBuilder.In(x => x.Id, expectedObjIds));
            // Act

            var found = await repo.Find(queryBuilder);

            // Assert
            found.Select(x => expectedObjIds.Should().Contain(x.Id));
            found.Should().HaveSameCount(expectedObjIds);
        }

        [Theory, Priority]
        [InlineData(new string[] { "User1", "uSer2", "usEr3" }, new string[] { "user1", "user2", "user3" })]
        public void Can_FindUsingQueryable_CaseInsesitive_ShouldPass(params object[] values)
        {
            // Arrange
            var names = (string[])values[0];
            var expectedNames = (string[])values[1];

            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            var queryable = repo.AsQueryable();
            // Act

            names = names.Select(x => x.ToLowerInvariant()).ToArray();
            var found = (from model in queryable
                         select model).Where(x => names.Contains(x.Name));

            // Assert

            found.Select(x => x.Name).Should().Contain(expectedNames);
        }

        [Theory, Priority]
        [InlineData(new string[] { "612c6b058d26f0c9785607dd", "612c6b058d26f0c9785617dd", "612c6b058d26f0c9785627dd" }, new string[] { "user1", "user2", "user3" })]
        public async Task Can_FindUsingQueryBuilder_And_OR_Operation_ShouldPass(params object[] values)
        {
            // Arrange
            (ObjectId, string)[] valuePairs = Enumerable.Range(0,((string[])values[0]).Length)
                .Select(x => (ObjectId.Parse(((string[])values[0])[x]), ((string[])values[1])[x])).ToArray();

            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            var queryBuilder = _serviceProvider.GetRequiredService<IQueryBuilder<TestModel1>>();
            queryBuilder = valuePairs.Select(x => queryBuilder.Eq(y => y.Id, x.Item1).And(y => y.Name, x.Item2))
                .Join((accumulated, current) =>
                {
                    if (accumulated is null)
                        return current;

                    return accumulated.Or<TestModel1>(current);
                });

            // Act

            var found = await repo.Find(queryBuilder);

            // Assert

            found.Should().HaveSameCount(valuePairs);
            found.Select(x => values.Contains((x.Id, x.Name)));
        }

        [Theory, Priority]
        [InlineData("612c6b058d26f0c9785607dd", "test1")]
        [InlineData("612c6b058d26f0c9785617dd", "test2")]
        [InlineData("612c6b058d26f0c9785627dd", "test3")]
        public async Task Can_Update_Single_ShouldPass(string expectedId, string expectedName)
        {
            // Arrange
            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();
            TestModel1 expectedModel = new(expectedId) { Name = expectedName };
            // Act

            var updated = await repo.Update(expectedId, expectedModel);

            // Assert

            updated.Should().Be(expectedModel);
        }

        [Theory, Priority]
        [InlineData(new string[] { "612c6b058d26f0c9785607dd", "612c6b058d26f0c9785617dd", "612c6b058d26f0c9785627dd" }, new string[] { "testuser1", "testuser2", "testuser3" })]
        public async Task Can_Update_Many_ShouldPass(params object[] values)
        {
            // Arrange
            (ObjectId, string)[] valuePairs = Enumerable.Range(0, ((string[])values[0]).Length)
                .Select(x => (ObjectId.Parse(((string[])values[0])[x]), ((string[])values[1])[x])).ToArray();

            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();

            var queryBuilder = _serviceProvider.GetRequiredService<IQueryBuilder<TestModel1>>();
            var updateBuilder = _serviceProvider.GetRequiredService<IUpdateBuilder<TestModel1>>();

            var updatesBuilder = valuePairs.Select(x => (queryBuilder.Eq(y => y.Id, x.Item1), updateBuilder.Set(y => y.Name, x.Item2)))
                .Select(x => KeyValuePair.Create(x.Item1, x.Item2));

            var query = updatesBuilder.Select(x => x.Key)
                .Join((accumulator, current) =>
                {
                    if (accumulator is null)
                        return current;

                    return accumulator.Or<TestModel1>(current);
                });

            // Act

            var result = await repo.Update(updatesBuilder);

            var updated = await repo.Find(query);

            // Assert

            result.Should().Be((true, valuePairs.Length));
            updated.Should().HaveSameCount(valuePairs);
        }

        [Theory, Priority]
        [InlineData(new string[] { "612c6b058d26f0c9785607dd", "612c6b058d26f0c9785617dd" }, new string[] { "testuser", "testuser" })]
        public async Task Can_Update_Many_Sparse_ShouldPass(params object[] values)
        {
            // Arrange
            (ObjectId, string)[] valuePairs = Enumerable.Range(0, ((string[])values[0]).Length)
                .Select(x => (ObjectId.Parse(((string[])values[0])[x]), ((string[])values[1])[x])).ToArray();

            var repo = _serviceProvider.GetRequiredService<IRepository<TestModel1>>();

            var queryBuilder = _serviceProvider.GetRequiredService<IQueryBuilder<TestModel1>>();
            var updateBuilder = _serviceProvider.GetRequiredService<IUpdateBuilder<TestModel1>>();

            var updatesBuilder = valuePairs.Select(x => (queryBuilder.Eq(y => y.Id, x.Item1), updateBuilder.Set(y => y.Name, x.Item2)))
                .Select(x => KeyValuePair.Create(x.Item1, x.Item2));

            var query = updatesBuilder.Select(x => x.Key)
                .Join((accumulator, current) =>
                {
                    if (accumulator is null)
                        return current;

                    return accumulator.Or<TestModel1>(current);
                });

            // Act

            var result = await repo.Update(updatesBuilder);

            var updated = await repo.Find(query,limit: values.Length);

            // Assert

            result.Should().Be((true, valuePairs.Length));
            updated.Should().HaveSameCount(valuePairs);
            updated.Select(x => (x.Id, x.Name)).Select(x => valuePairs.Should().Contain(x));
        }
    }
}


namespace Infrastructure.Unit.Tests.Database.TestDefinitions
{
    [CollectionDefinition("Database")]
    public class DatabaseCollectionDefinition : ICollectionFixture<DatabaseFixture> { }
    public class DatabaseFixture : IDisposable
    {
        private bool _disposedValue;
        public MongoDbRunner MongoServer { get; }
        public IDatabaseContext DbContext { get; }
        public IServiceProvider ServiceProvider { get; }

        public DatabaseFixture()
        {
            MongoServer = MongoDbRunner.Start(dataDirectory: Path.Combine(Environment.CurrentDirectory, "mongotest/"));
            IServiceCollection serviceDescriptors = new ServiceCollection();
            serviceDescriptors.AddSingleton<IDatabaseContext>((sp) => new MongoDbContext("testdb", MongoServer.ConnectionString));
            serviceDescriptors.AddSingleton<RepositoryManager>();
            serviceDescriptors.AddSingleton(typeof(IRepository<>), typeof(BaseRepository<>));
            serviceDescriptors.AddTransient(typeof(IQueryBuilder<>), typeof(BaseQueryBuilder<>));
            serviceDescriptors.AddTransient(typeof(IUpdateBuilder<>), typeof(BaseUpdateBuilder<>));
            serviceDescriptors.AddTransient<IQueryBuilderProvider, QueryBuilderProvider>();
            serviceDescriptors.AddTransient<IUpdateBuilderProvider,UpdateBuilderProvider>();

            ServiceProvider = serviceDescriptors.BuildServiceProvider();
            DbContext = ServiceProvider.GetRequiredService<IDatabaseContext>();
            DbContext.Initialize().Await();
        }

        public string NewId()
            => ObjectId.GenerateNewId().ToString();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    MongoServer.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

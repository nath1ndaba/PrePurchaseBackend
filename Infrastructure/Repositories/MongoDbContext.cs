using BackendServices;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    internal class MongoDbContext : IDatabaseContext
    {
        static readonly SemaphoreSlim _semaphore = new(1);
        internal IMongoDatabase _db;

        private readonly string DATABASE_NAME;
        private readonly string DATABASE_HOST;
        private readonly string DATABASE_AUTH_DB;
        private readonly string DATABASE_USERNAME;
        private readonly string DATABASE_USER_PASSWORD;

        private readonly ILogger<MongoDbContext>? _logger;

        internal MongoDbContext(string databaseName, string databaseHost = "mongodb://localhost:27017", string authDb = "admin", string? databaseUserName = default, string? databaseUserPassword = default, ILogger<MongoDbContext>? logger = default)
        {
            DATABASE_NAME = databaseName;
            DATABASE_HOST = databaseHost;
            DATABASE_AUTH_DB = authDb;
            DATABASE_USERNAME = databaseUserName;
            DATABASE_USER_PASSWORD = databaseUserPassword;
            _logger = logger;
        }

        internal static string GetName<T>() => GetName(typeof(T));

        internal static string GetName(Type type)
        {
            var name = type.Name;
            var index = name.IndexOf("model", StringComparison.InvariantCultureIgnoreCase);
            return index > -1 ? name.Substring(0, index) : name;
        }

        internal IMongoCollection<Entity> GetCollection<Entity>(string? collectionName = default)
        {
            var name =  string.IsNullOrWhiteSpace(collectionName) ? GetName<Entity>() : collectionName;
            return _db.GetCollection<Entity>(name, new MongoCollectionSettings
            {
                AssignIdOnInsert = true
            });
        }

        public Task<IEnumerable<T>> Find<T>(Expression<Func<T, bool>> predicate, string? collectionName = null, int skip = 0, int limit = 10000)
        {
            var col = GetCollection<T>(collectionName);
            var result = col.Find(predicate).Skip(skip).Limit(limit).ToEnumerable();
            return Task.FromResult(result);
        }

        public async Task<T> FindById<T>(string id, string? collectionName = null)
        {
            var col = GetCollection<T>(collectionName);
            var result = await col.FindAsync(new BsonDocument("_id", id));
            return await result.FirstOrDefaultAsync();
        }

        public async Task<T> FindOne<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            var col = GetCollection<T>(collectionName);
            var result = await col.Find(predicate).FirstOrDefaultAsync();
            return result;
        }

        public async Task Initialize()
        {
            await _semaphore.WaitAsync();
            _logger?.LogInformation("Initializing database.");
            if (_db is not null) // not null check
            {
                _semaphore.Release();
                return;
            }

            try
            {
                var mongoSettings = MongoClientSettings.FromConnectionString(DATABASE_HOST);

                // checking that credentials were provided
                if (!string.IsNullOrWhiteSpace(DATABASE_USERNAME) && !string.IsNullOrWhiteSpace(DATABASE_USER_PASSWORD))
                {

                    var mongoCred = MongoCredential.CreateCredential(DATABASE_AUTH_DB, DATABASE_USERNAME, DATABASE_USER_PASSWORD);
                    mongoSettings.Credential = mongoCred;
                }

                var client = new MongoClient(mongoSettings);

                _db = client.GetDatabase(DATABASE_NAME);
                _logger?.LogInformation("Database initialized.");
            }catch(Exception e)
            {
                _logger.LogError(e.StackTrace);
            }
            finally
            {
                _semaphore.Release();
            }

        }

        public Task Insert<T>(T data, string collectionName = null)
        {
            var col = GetCollection<T>(collectionName);
            return col.InsertOneAsync(data);
        }

        public Task Insert<T>(IEnumerable<T> data, string collectionName = null)
        {
            var col = GetCollection<T>(collectionName);
            return col.InsertManyAsync(data);
        }

        public async Task<T> Update<T>(string id, T data, string collectionName = null)
        {
            var col = GetCollection<T>(collectionName);
            var result = await col.FindOneAndReplaceAsync(new BsonDocument("_id", id), data
                , new FindOneAndReplaceOptions<T, T> {
                ReturnDocument = ReturnDocument.After
            });
            return result;
        }

        public async Task<T> Upsert<T>(string id, T data, string collectionName = null)
        {
            var col = GetCollection<T>(collectionName);
            var result = await col.FindOneAndReplaceAsync(new BsonDocument("_id", id), data
                , new FindOneAndReplaceOptions<T, T>
                {
                    ReturnDocument = ReturnDocument.After,
                    IsUpsert = true
                });
            return result;
        }
    }
}

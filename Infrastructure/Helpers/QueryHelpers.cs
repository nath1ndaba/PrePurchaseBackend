using BackendServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    internal static class QueryHelpers
    {
        public static BsonDocument RenderToBsonDocument<T>(this FilterDefinition<T> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<T>();
            return filter.Render(documentSerializer, serializerRegistry);
        }

        public static BsonDocument RenderToBsonDocument<T>(this IQueryBuilder<T> queryBuilder)
        {
            return ((BaseQueryBuilder<T>)queryBuilder).Filter.RenderToBsonDocument();
        }
    }
}

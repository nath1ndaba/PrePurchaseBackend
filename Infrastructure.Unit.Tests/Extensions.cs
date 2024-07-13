namespace Infrastructure.Unit.Tests
{
    public static class Extensions
    {
        public static T? Join<T>(this IEnumerable<T> enumeration, Func<T?, T, T> accumulator)
        {
            T? result = default;
            foreach(var item in enumeration)
            {
                result = accumulator(result,item);
            }

            return result;
        }

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

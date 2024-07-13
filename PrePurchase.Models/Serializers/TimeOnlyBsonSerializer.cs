using MongoDB.Bson.Serialization;
using System;

namespace PrePurchase.Models.Serializers
{
    public class TimeOnlyBsonSerializer : IBsonSerializer<TimeOnly>
    {
        public Type ValueType => typeof(TimeOnly);

        public TimeOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();
            return TimeOnly.Parse(value);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TimeOnly value)
        {
            context.Writer.WriteString(value.ToString());
        }

        void IBsonSerializer.Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            context.Writer.WriteString(value.ToString());
        }

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }
    }
}

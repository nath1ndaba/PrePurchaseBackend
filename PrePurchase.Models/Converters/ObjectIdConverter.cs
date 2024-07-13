using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MongoDB.Bson;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PrePurchase.Models.Converters
{
    public class ObjectIdConverter : JsonConverter<ObjectId>
	{
		public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return ObjectId.Parse(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}

	public class BsonObjectIdBinder : IModelBinder
    {
        public System.Threading.Tasks.Task BindModelAsync(ModelBindingContext bindingContext)
        {
			if (bindingContext == null)
			{
				throw new ArgumentNullException(nameof(bindingContext));
			}

			var provider = bindingContext.ValueProvider;
			var valueProviderResult = provider.GetValue(bindingContext.ModelName);

			if (valueProviderResult == ValueProviderResult.None)
				return System.Threading.Tasks.Task.CompletedTask;

            if (ObjectId.TryParse(valueProviderResult.FirstValue, out var id))
            {
				bindingContext.Result = ModelBindingResult.Success(id);
			}

			return System.Threading.Tasks.Task.CompletedTask;
        }
    }

	public class BsonObjectIdModelBinderProvider : IModelBinderProvider
	{
		private static Type[] SupportedTypes = { typeof(BsonObjectId), typeof(ObjectId) };
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			if (context is null) throw new ArgumentNullException(nameof(context));

			// our binders here
			if (SupportedTypes.Contains(context.Metadata.ModelType))
			{
				return new BinderTypeModelBinder(typeof(BsonObjectIdBinder));
			}

			// your maybe have more binders?
			// ....

			// this provider does not provide any binder for given type
			//   so we return null
			return null;
		}
	}
}

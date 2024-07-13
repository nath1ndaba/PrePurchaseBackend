using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackendServer
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (context == null)
                throw new ArgumentNullException(nameof(context));


            if (context.Type.IsEnum)
            {
                model.Type = "enum";
                model.Description = DescribeEnum(context.Type);
            }
        }

        internal static string DescribeEnum(Type enumType)
        {
            int i = 0;
            var values = Enum.GetValues(enumType);
            StringBuilder sb = new();
            foreach (var n in values)
            {
                sb.Append($"{n} = {(int)n}, ");
                i++;
            }
            if (sb.Length > 2)
                sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        }
    }

    public class EnumDocumentFilter : IDocumentFilter
    {

        /// <inheritdoc />
        public void Apply(OpenApiDocument openApiDoc, DocumentFilterContext context)
        {

            if (openApiDoc.Paths.Count <= 0) return;

            // add enum descriptions to input parameters
            foreach (var pathItem in openApiDoc.Paths.Values)
            {
                DescribeEnumParameters(pathItem.Parameters, openApiDoc.Components);

                var possibleParameterisedOperations = pathItem.Operations.Where(x => x.Value != null)
                    .Select(x => x.Value);

                foreach(var operation in possibleParameterisedOperations)
                {
                    DescribeEnumParameters(operation.Parameters, openApiDoc.Components);
                }
            }
        }

        private static void DescribeEnumParameters(IList<OpenApiParameter> parameters, OpenApiComponents components)
        {
            if (parameters == null) return;

            foreach (var param in parameters)
            {
                if (string.IsNullOrWhiteSpace(param.Schema.Description) is false)
                {
                    param.Description += param.Schema.Description;
                }else
                {
                    param.Description += DescribeEnumFromReference(param, components);
                }
            }
        }

        private static string DescribeEnumFromReference(OpenApiParameter parameter, OpenApiComponents components)
        {
            if (parameter.Schema is null || parameter.Schema.Reference is null)
                return string.Empty;

            if (components.Schemas.TryGetValue(parameter.Schema.Reference.Id, out var schema) is false)
                return string.Empty;

            return schema.Description;
        }
    }
}
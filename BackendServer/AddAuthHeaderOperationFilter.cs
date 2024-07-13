using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendServer
{
    internal partial class AddAuthHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!IsAuthorized(context)) return;

            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

            OpenApiSecurityScheme jwtBearerScheme = new()
            {
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, 
                    Id = JwtBearerDefaults.AuthenticationScheme 
                }
            };

            operation.Security = new List<OpenApiSecurityRequirement> { new() { [jwtBearerScheme] = Array.Empty<string>() } };
        }

        private static bool IsAuthorized(OperationFilterContext context)
        {
            var methodAttributes = context.MethodInfo.GetCustomAttributes(true);
            var classAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true);

            var methodHasAuthorize = methodAttributes.OfType<AuthorizeAttribute>().Any();
            var methodHasAnonymous = methodAttributes.OfType<AllowAnonymousAttribute>().Any();

            var methodAuth = methodHasAuthorize && !methodHasAnonymous;
            var classAuth = classAttributes.OfType<AuthorizeAttribute>().Any()
                && !classAttributes.OfType<AllowAnonymousAttribute>().Any();

            if(classAuth) // there is authorize on class level
            {
                if (methodAuth || (!methodHasAuthorize && !methodHasAnonymous)) // and authorize on method level or no auth attributes
                    return true;

                return false; // method has authorize and anonymous
            }
            else
            {
                // there is no authorize on class level
                if (methodAuth || (!methodHasAuthorize && !methodHasAnonymous)) // and authorize on method level or no auth attributes
                    return true;

                return false; // method has authorize and anonymous
            }
        }
    }
}
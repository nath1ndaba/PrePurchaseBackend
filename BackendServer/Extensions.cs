using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using MongoDB.Bson;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace BackendServer;

public static class Extensions
{
    public static T UserData<T>(this ClaimsPrincipal user)
    {
        return JsonSerializer.Deserialize<T>(user.Claims.First(c => c.Type == ClaimTypes.UserData).Type);
    }

    public static T UserData<T>(this ControllerBase controller)
    {
        return controller.User.UserData<T>();
    }

    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pre Purchase Api", Version = "v1" });
            c.MapType<ObjectId>(() => new OpenApiSchema
                { Type = "string", Example = new OpenApiString(ObjectId.GenerateNewId().ToString()) });
            c.MapType<TimeSpan>(() => new OpenApiSchema
                { Type = "string", Example = new OpenApiString((DateTime.UtcNow - DateTime.Today).ToString()) });
            c.MapType<TimeOnly>(() => new OpenApiSchema
            {
                Type = "string", Example = new OpenApiString(TimeOnly.FromDateTime(DateTime.UtcNow).ToString())
            });
            c.OperationFilter<AddAuthHeaderOperationFilter>();
            c.SchemaFilter<EnumSchemaFilter>();
            c.DocumentFilter<EnumDocumentFilter>();
            c.IgnoreObsoleteProperties();
            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = @"Enter JWT Bearer token ***only***",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            });
        });

        services.AddMvc(c => c.Conventions.Add(new ApiExplorerGroupPerVersionConvention()));

        return services;
    }

    public static IApplicationBuilder UseSwagger(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseSwagger(c => c.RouteTemplate = "api-docs/{documentName}/swagger.json");
        applicationBuilder.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/api-docs/v1/swagger.json", "Pre Purchase Api v1");
            c.RoutePrefix = string.Empty;
            c.DisplayRequestDuration();
        });
        return applicationBuilder;
    }

    internal static string ToCamelCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
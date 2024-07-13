using BackendServices;
using BackendServices.Actions;
using BackendServices.Actions.Admin;
using BackendServices.Actions.Admin.RealTimeServices;
using BackendServices.Actions.Inventory;
using BackendServices.JWT;
using BackendServices.Validators;
using BackendServices.Validators.ValidationData;
using Infrastructure.Actions;
using Infrastructure.Actions.Admin;
using Infrastructure.Actions.Admin.RealTime;
using Infrastructure.Helpers;
using Infrastructure.JWT;
using Infrastructure.Repositories;
using Infrastructure.Validators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PrePurchase.Models;
using System;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using BackendServices.Actions.PrePurchase.AdminPortal;
using Infrastructure.Actions.PrePurchase.Admin;
using ILoginActions = BackendServices.Actions.Admin.ILoginActions;

namespace Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddDatabase();
            services.AddAuth();
            services.AddPasswordManager();
            services.AddActions();
            services.AddSingleton<IEmployeeIdGenerator, EmployeeIdGenerator>();
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddSingleton<ITimeZoneProvider, TimeZoneProvider>();
            services.AddSingleton<ICommon, Common>();
            services.AddValidators();

            return services;
        }

        public static IApplicationBuilder UseDatabase(this IApplicationBuilder builder)
        {
            var databaseContext = builder.ApplicationServices.GetService<IDatabaseContext>();
            databaseContext.Initialize().Await();

            return builder;
        }

        private static IServiceCollection AddDatabase(this IServiceCollection services)
        {
            services.AddSingleton(sp => new RepositoryManager(sp));
            services.AddSingleton<IDatabaseContext>(sp => new MongoDbContext(
                PrePurchaseConfig.DATABASE_NAME
                , PrePurchaseConfig.DATABASE_HOST
                , PrePurchaseConfig.DATABASE_AUTH_DB
                , PrePurchaseConfig.DATABASE_USERNAME
                , PrePurchaseConfig.DATABASE_USER_PASSWORD
                , sp.GetService<ILogger<MongoDbContext>>()));
            services.AddSingleton(typeof(IRepository<>), typeof(BaseRepository<>));
            services.AddSingleton<IQueryBuilderProvider, QueryBuilderProvider>();
            services.AddTransient(typeof(IQueryBuilder<>), typeof(BaseQueryBuilder<>));
            services.AddSingleton<ITimeSummaryRepository, TimeSummaryRepository>();
            services.AddTransient(typeof(IUpdateBuilderProvider), typeof(UpdateBuilderProvider));
            services.AddTransient(typeof(IUpdateBuilder<>), typeof(BaseUpdateBuilder<>));
            return services;
        }

        public static async Task<string> CreateBatchCode(this IRepository<History> histories,
            IQueryBuilderProvider builder)
        {
            string _batchCode = $"B-{DateTime.UtcNow:yyMMdd}-" + await Nanoid.Nanoid.GenerateAsync(size: 10);

            return _batchCode;
        }

        private static IServiceCollection AddAuth(this IServiceCollection services)
        {
            services.AddSingleton<IAuthService>(sp => new JWTAuthService(
                PrePurchaseConfig.JWT_SECRET_KEY
                , PrePurchaseConfig.JWT_SECURITY_ALGORITHM
                , PrePurchaseConfig.JWT_TOKEN_EXPIRES_IN
                , PrePurchaseConfig.JWT_ISSUER
                , PrePurchaseConfig.JWT_AUDIENCE
                , PrePurchaseConfig.JWT_VALIDATE_ISSUER
                , PrePurchaseConfig.JWT_VALIDATE_AUDIENCE));
            services.AddTransient<IAuthContainerModel, JWTContainerModel>();

            services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtAuthOptions>();
            services.ConfigJwtAuth();


            services.AddAuthorization(config =>
            {
                config.AddPolicy(AuthPolicies.Company, policyBuilder =>
                {
                    policyBuilder.RequireClaim(ClaimTypes.Role);
                    policyBuilder.RequireRole(AuthRoles.Manager, AuthRoles.Owner);
                });
            });

            return services;
        }

        private static IServiceCollection AddPasswordManager(this IServiceCollection services)
        {
            services.AddScoped<IPasswordManager, PasswordManager>();
            return services;
        }

        private static IServiceCollection AddActions(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeActions, EmployeeActions>();
            services.AddScoped<IAllActions, AllActions>();
            services.AddScoped<IUniversalActions, UniversalActions>();
            services.AddScoped<ISimunyeActions, SimunyeActions>();
            services.AddScoped<IPayment, PeachPayment>();

            services.AddScoped<ILoginActions, LoginActions>();
            services.AddScoped<IEmployeesActions, EmployeesActions>();
            services.AddScoped<ICompanyActions, CompanyActions>();
            services.AddScoped<IUserActions, UserActions>();
            services.AddScoped<IDepartmentsActions, DepartmentsActions>();
            services.AddScoped<ILoansActions, LoansActions>();
            services.AddScoped<IPayrollActions, PayrollActions>();
            services.AddScoped<IPositionsNewActions, PositionsNewActions>();
            services.AddScoped<IDepartmentsNewActions, DepartmentsnewActions>();
            services.AddScoped<IRealTimeDashBoardUpdate, RealTimeDashBoardUpdate>();
            services.AddScoped<IRealTimePayroll, RealTimePayroll>();
            services.AddScoped<ICategoriesActions, CategoriesActions>();
            services.AddScoped<IOrderItemsActions, OrderItemsActions>();
            services.AddScoped<IProductsActions, ProductsActions>();
            services.AddScoped<IStockCountsActions, StockCountsActions>();
            services.AddScoped<ISuppliersActions, SuppliersActions>();

            //Pre -Purchase
            services.AddScoped<IRegisterAdminActions, RegisterAdminActions>();

            return services;
        }

        private static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services
                .AddSingleton<IValidator<CompanyEmployeeValidationData, CompanyEmployeeValidationResult>,
                    CompanyEmployeeValidator>();
            services.AddSingleton<IValidator<DayOfWeekValidationData, DayOfWeekValidationResult>, DayOfWeekValidator>();
            services.AddSingleton<IValidator<DistanceValidationData, DistanceValidationResult>, DistanceValidator>();
            services.AddSingleton<IValidator<QrCodeValidationData, QrCodeValidationResult>, QrCodeValidator>();
            services.AddSingleton<IValidator<ShiftValidationData, ShiftValidationResult>, ShiftValidator>();
            return services;
        }

        private static IServiceCollection AddEncryptionManger(this IServiceCollection services)
        {
            services.AddSingleton<IEncryptionManager, EncryptionManager>();

            return services;
        }

        private static AuthenticationBuilder ConfigJwtAuth(this IServiceCollection services)
        {
            return services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, default);
        }

        public static JwtToken ToJwtToken(this SecurityToken securityToken)
        {
            var token = JWTAuthService.SecurityTokenToString(securityToken);
            return new(token, securityToken.ValidTo);
        }

        public static async Task<T?> ToJson<T>(this Stream stream)
        {
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
            };
            var data = await JsonSerializer.DeserializeAsync<T>(stream, options);
            return data;
        }
    }
}
using System.Reflection;
using System.Text.Json.Serialization;
using BridgeTask.Api.Common.Validation;
using BridgeTask.Api.Data;
using BridgeTask.Api.Middleware;
using BridgeTask.Api.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BridgeTask.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers(options => options.Filters.Add<ValidationFilter>())
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = ctx =>
                ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier);
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(xmlFile))
                options.IncludeXmlComments(xmlFile);
        });

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")
                              ?? throw new InvalidOperationException("Connection string 'Default' is missing.")));

        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<ICityService, CityService>();

        return services;
    }
}

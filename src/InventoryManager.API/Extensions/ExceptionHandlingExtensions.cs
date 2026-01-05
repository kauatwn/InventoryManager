using InventoryManager.API.ExceptionHandlers;
using System.Diagnostics.CodeAnalysis;

namespace InventoryManager.API.Extensions;

[ExcludeFromCodeCoverage(Justification = "Pure dependency injection configuration")]
public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddApiExceptionHandlers(this IServiceCollection services)
    {
        services.AddExceptionHandler<ValidationExceptionHandler>();

        services.AddExceptionHandler<DomainExceptionHandler>();
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<ConflictExceptionHandler>();

        services.AddExceptionHandler<UnhandledExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
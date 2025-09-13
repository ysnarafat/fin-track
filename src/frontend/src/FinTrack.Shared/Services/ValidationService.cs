using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Shared.Services;

/// <summary>
/// Service for handling validation operations
/// </summary>
public interface IValidationService
{
    Task<FluentValidation.Results.ValidationResult> ValidateAsync<T>(T instance, CancellationToken cancellationToken = default);
    Task<FluentValidation.Results.ValidationResult> ValidateAsync<T>(T instance, IValidator<T> validator, CancellationToken cancellationToken = default);
    Task<bool> IsValidAsync<T>(T instance, CancellationToken cancellationToken = default);
    Task ThrowIfInvalidAsync<T>(T instance, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of validation service using FluentValidation
/// </summary>
public class ValidationService : IValidationService
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<FluentValidation.Results.ValidationResult> ValidateAsync<T>(T instance, CancellationToken cancellationToken = default)
    {
        var validator = _serviceProvider.GetService<IValidator<T>>();
        if (validator == null)
        {
            throw new InvalidOperationException($"No validator registered for type {typeof(T).Name}");
        }

        return await validator.ValidateAsync(instance, cancellationToken);
    }

    public async Task<FluentValidation.Results.ValidationResult> ValidateAsync<T>(T instance, IValidator<T> validator, CancellationToken cancellationToken = default)
    {
        return await validator.ValidateAsync(instance, cancellationToken);
    }

    public async Task<bool> IsValidAsync<T>(T instance, CancellationToken cancellationToken = default)
    {
        var result = await ValidateAsync(instance, cancellationToken);
        return result.IsValid;
    }

    public async Task ThrowIfInvalidAsync<T>(T instance, CancellationToken cancellationToken = default)
    {
        var result = await ValidateAsync(instance, cancellationToken);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}


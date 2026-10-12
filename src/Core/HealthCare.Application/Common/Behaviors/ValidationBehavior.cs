using FluentValidation;
using HealthCare.Application.Common.Models;
using MediatR;

namespace HealthCare.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .Select(f => f.ErrorMessage)
                .Distinct()
                .ToList();
            if (failures.Count == 0)
            {
                return await next();
            }
            // Return a failed Result with the validation errors
            return CreateValidationResult(failures);
        }
        private static TResponse CreateValidationResult(List<string> errors)
        {
            var responseType = typeof(TResponse);
            // If TResponse is non-generic Result
            if (responseType == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(errors);
            }
            // If TResponse is generic Result<TValue>
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var valueType = responseType.GetGenericArguments()[0];
                var failureMethod = typeof(Result)
                    .GetMethods()
                    .First(m => m.Name == nameof(Result.Failure) &&
                                m.IsGenericMethod &&
                                m.GetParameters().Length == 1 &&
                                m.GetParameters()[0].ParameterType == typeof(IEnumerable<string>))
                    .MakeGenericMethod(valueType);
                return (TResponse)failureMethod.Invoke(null, new object[] { errors })!;
            }
            throw new InvalidOperationException(
                $"Validation failed for '{typeof(TRequest).Name}', but response type '{responseType.Name}' does not inherit from Result.");
        }
    }
}
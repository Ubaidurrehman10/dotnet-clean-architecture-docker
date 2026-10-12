namespace HealthCare.Application.Common.Models
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }
        public IReadOnlyList<string> ValidationErrors { get; }
        protected Result(bool isSuccess, Error error, IEnumerable<string>? validationErrors = null)
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException("Successful result cannot contain an error.");
            if (!isSuccess && error == Error.None && (validationErrors == null || !validationErrors.Any()))
                throw new InvalidOperationException("Failed result must contain an error or validation messages.");
            IsSuccess = isSuccess;
            Error = error;
            ValidationErrors = validationErrors?.ToList().AsReadOnly() ?? (IReadOnlyList<string>)Array.Empty<string>();
        }
        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);
        public static Result Failure(IEnumerable<string> validationErrors) =>
            new(false, Error.Validation("Validation.General", "One or more validation failures occurred."), validationErrors);
        public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
        public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
        public static Result<TValue> Failure<TValue>(IEnumerable<string> validationErrors) =>
            new(default, false, Error.Validation("Validation.General", "One or more validation failures occurred."), validationErrors);
    }
    public class Result<TValue> : Result
    {
        private readonly TValue? _value;
        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access the value of a failed result.");
        protected internal Result(TValue? value, bool isSuccess, Error error, IEnumerable<string>? validationErrors = null)
            : base(isSuccess, error, validationErrors)
        {
            _value = value;
        }
        public static implicit operator Result<TValue>(TValue? value) =>
            value is not null ? Success(value) : Failure<TValue>(Error.NotFound("Resource.NotFound", "Resource not found."));
    }
}
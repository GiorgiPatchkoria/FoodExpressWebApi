namespace FoodExpress.Services.Services.Implementations
{
    public class ServiceException(ServiceError error, string message, Dictionary<string, string[]>? errors = null) : Exception(message)
    {
        public ServiceError Error { get; } = error;
        public Dictionary<string, string[]>? Errors { get; } = errors;
    }

    public enum ServiceError
    {
        Validation,
        NotFound,
        Conflict,
        Unauthorized,
        Forbidden
    }
}
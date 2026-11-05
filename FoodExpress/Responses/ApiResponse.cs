namespace FoodExpress.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Request completed successfully.")
        {
            return new() { Success = true, Message = message, Data = data };
        }
    }
}

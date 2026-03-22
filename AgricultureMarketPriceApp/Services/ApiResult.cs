namespace AgricultureMarketPriceApp.Services
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public int? StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ResponseContent { get; set; }
        public string ReasonPhrase { get; set; }

        public static ApiResult<T> FromSuccess(T data)
            => new ApiResult<T> { Success = true, Data = data };

        public static ApiResult<T> FromError(int? statusCode, string reason, string content, string message = null)
            => new ApiResult<T>
            {
                Success = false,
                StatusCode = statusCode,
                ReasonPhrase = reason,
                ResponseContent = content,
                ErrorMessage = message ?? reason
            };
    }
}

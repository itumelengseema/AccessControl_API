using AccessControl_API.Utilities.Enums;

namespace AccessControl_API.Models.DTO
{
    public class ApiResponse<TData>
    {
        public bool Success { get; set; }
        public ApiStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public TData? Data { get; set; }
        public object? Errors { get; set; }

        public int StatusCode => (int)Status;

        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        // Factory Methods for Success Responses
        public static ApiResponse<TData> SuccessResponse(TData data, string message = "Request successful")
        {
            return new ApiResponse<TData>
            {
                Success = true,
                Status = ApiStatus.ok,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<TData> CreatedResponse(TData data, string message = "Resource created successfully")
        {
            return new ApiResponse<TData>
            {
                Success = true,
                Status = ApiStatus.created,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<TData> NoContentResponse(string message = "No content")
        {
            return new ApiResponse<TData>
            {
                Success = true,
                Status = ApiStatus.noContent,
                Message = message,
                Data = default
            };
        }

        // Factory Methods for Error Responses
        public static ApiResponse<TData> BadRequestResponse(string message = "Bad request", object? errors = null)
        {
            return new ApiResponse<TData>
            {
                Success = false,
                Status = ApiStatus.badRequest,
                Message = message,
                Data = default,
                Errors = errors
            };
        }

        public static ApiResponse<TData> UnauthorizedResponse(string message = "Unauthorized access")
        {
            return new ApiResponse<TData>
            {
                Success = false,
                Status = ApiStatus.unauthorized,
                Message = message,
                Data = default
            };
        }

        public static ApiResponse<TData> ForbiddenResponse(string message = "Access forbidden")
        {
            return new ApiResponse<TData>
            {
                Success = false,
                Status = ApiStatus.forbidden,
                Message = message,
                Data = default
            };
        }

        public static ApiResponse<TData> NotFoundResponse(string message = "Resource not found")
        {
            return new ApiResponse<TData>
            {
                Success = false,
                Status = ApiStatus.notFound,
                Message = message,
                Data = default
            };
        }

        public static ApiResponse<TData> ConflictResponse(string message = "Conflict occurred", object? errors = null)
        {
            return new ApiResponse<TData>
            {
                Success = false,
                Status = ApiStatus.conflict,
                Message = message,
                Data = default,
                Errors = errors
            };
        }

        public static ApiResponse<TData> InternalServerErrorResponse(string message = "Internal server error", object? errors = null)
        {
            return new ApiResponse<TData>
            {
                Success = false,
                Status = ApiStatus.internalServerError,
                Message = message,
                Data = default,
                Errors = errors
            };
        }
    }
}

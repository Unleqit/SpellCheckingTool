using System.Net;

namespace SpellCheckingTool.Presentation.ConsoleClient
{
    public class ApiResult<T>
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
        public string? ErrorMessage { get; init; }
        public HttpStatusCode StatusCode { get; init; }

    }
}

namespace SpellCheckingTool.Application.Common;
    public class OperationResult<T>
    {
        public bool Success { get; }
        public string? ErrorMessage { get; }
        public T? Value { get; }

    private OperationResult(bool success, T? value, string? error)
        {
            Success = success;
            ErrorMessage = error;
            Value = value;
        }

        public static OperationResult<T> Ok(T value) => new(true, value, null);
        public static OperationResult<T> Fail(string error) => new(false, default, error);
    }

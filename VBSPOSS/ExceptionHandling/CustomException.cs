namespace VBSPOSS.ExceptionHandling
{
    public class CustomException : Exception
    {
        public int ErrorCode { get; }

        public CustomException() { }

        public CustomException(string message)
            : base(message) { }

        public CustomException(string message, Exception innerException)
            : base(message, innerException) { }

        public CustomException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        // Optional: Override ToString for detailed error information
        public override string ToString()
        {
            return $"{base.ToString()}, ErrorCode: {ErrorCode}";
        }
    }
}

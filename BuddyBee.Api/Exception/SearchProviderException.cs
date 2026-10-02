namespace BuddyBee.Api.Exceptions
{
    public class SearchProviderException : Exception
    {
        public string Provider { get; }

        public bool IsTemporary { get; }

        public int? StatusCode { get; }

        public SearchProviderException(
            string provider,
            string message,
            bool isTemporary = false,
            int? statusCode = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            Provider = provider;
            IsTemporary = isTemporary;
            StatusCode = statusCode;
        }
    }
}

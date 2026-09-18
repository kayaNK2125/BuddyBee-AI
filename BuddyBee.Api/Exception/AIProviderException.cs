namespace BuddyBee.Api.Exceptions
{
    public class AIProviderException : Exception
    {
        public string Provider { get; }
        //we are finding if provider API is raised any exception , we are capturing     that exceptionn
        public AIProviderException(
            string provider,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            Provider = provider;
        }
    }
}
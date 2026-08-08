namespace BuddyBee.Api.Interfaces
{
    public interface IAIService
    {
        //string GenerateReply(string message); //GenerateReply method takes a string message as input and returns a string reply
        Task<string> GenerateReply(string message); //This method will eventually give me a string
    }
}
using BuddyBee.Api.Models;

namespace BuddyBee.Api.Interfaces
{
    public interface IAIProvider
    {
        //Every AI provider BuddyBee uses must know how to receive the message + conversation history and return a reply
        Task<string> GenerateReply(
            string message,
            List<Message> history);
    }
}
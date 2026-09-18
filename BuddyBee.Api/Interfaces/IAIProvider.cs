using BuddyBee.Api.Models;

namespace BuddyBee.Api.Interfaces
{
    public interface IAIProvider
    {
        //Every AI provider BuddyBee uses must know how to receive the message + conversation history and return a reply
        Task<string> GenerateReply(
     string message,
     List<Message> history,
     string memoryContext); //The memoryContext is a string that contains the context of the conversation, which can be used to provide more relevant responses.
    }
    ///Whenever an AI provider generates a response, it receives the conversation history and the user's long-term memory context
}
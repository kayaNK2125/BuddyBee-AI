using BuddyBee.Api.Models;

namespace BuddyBee.Api.Interfaces
{
    public interface IConversationStore
    {
        Task SaveMessage(Message message);

        Task<List<Message>> GetConversationMessages(
            string conversationId);
    }
}
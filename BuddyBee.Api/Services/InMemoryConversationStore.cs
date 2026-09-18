using System.Collections.Concurrent;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;

namespace BuddyBee.Api.Services
{
    public class InMemoryConversationStore : IConversationStore
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<Message>> _conversations = new();

        public Task SaveMessage(Message message)
        {
            var messages = _conversations.GetOrAdd(
                message.ConversationId,
                _ => new ConcurrentQueue<Message>());

            messages.Enqueue(message);

            return Task.CompletedTask;
        }

        public Task<List<Message>> GetConversationMessages(string conversationId)
        {
            if (_conversations.TryGetValue(conversationId, out var messages))
            {
                return Task.FromResult(
                    messages.OrderBy(message => message.Time).ToList());
            }

            return Task.FromResult(new List<Message>());
        }
    }
}
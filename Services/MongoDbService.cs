using BuddyBee.Api.Configuration;
using BuddyBee.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BuddyBee.Api.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<Message> _messages;
        private readonly IMongoDatabase _database;

        public MongoDbService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
            _messages = _database.GetCollection<Message>("Messages");
        }
        public async Task SaveMessage(Message message)
        {
            await _messages.InsertOneAsync(message);
        }

        public async Task<List<Message>> GetConversationMessages(string conversationId)
        {
            return await _messages
                .Find(message => message.ConversationId == conversationId) //From MongoDB, give me only messages belonging to this conversation
                .SortBy(message => message.Time) //correct order
                .ToListAsync();
        }
    }
}
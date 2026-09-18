using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;
using MongoDB.Driver;

namespace BuddyBee.Api.Services
{
    public class MemoryService : IMemoryService
    {
        private readonly IMongoCollection<Memory> _memories;

        public MemoryService(IMongoDatabase database)
        //When ASP.NET creates my MemoryService, give me an IMongoDatabase
        {
            _memories = database.GetCollection<Memory>("Memories");  //Use/create a collection called Memories containing Memory documents.
        }

        public async Task SaveMemory(Memory memory)
        {
            await _memories.InsertOneAsync(memory);
        }

        public async Task<List<Memory>> GetMemories(string userId)
        {
            return await _memories
                .Find(memory => memory.UserId == userId)
                .SortBy(memory => memory.CreatedAt)
                .ToListAsync();
        }
    }
}
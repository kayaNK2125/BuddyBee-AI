using System.Collections.Concurrent;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;

namespace BuddyBee.Api.Services
{
    public class InMemoryMemoryService : IMemoryService
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<Memory>> _memories = new();

        public Task SaveMemory(Memory memory)
        {
            var memories = _memories.GetOrAdd(
                memory.UserId,
                _ => new ConcurrentBag<Memory>());

            memories.Add(memory);

            return Task.CompletedTask;
        }

        public Task<List<Memory>> GetMemories(string userId)
        {
            if (_memories.TryGetValue(userId, out var memories))
            {
                return Task.FromResult(
                    memories.OrderBy(memory => memory.CreatedAt).ToList());
            }

            return Task.FromResult(new List<Memory>());
        }
    }
}
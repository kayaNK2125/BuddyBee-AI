using BuddyBee.Api.Models;

namespace BuddyBee.Api.Interfaces
{
    public interface IMemoryService
    {
        //Any service that is responsible for Memory must provide these two abilities that is to save a memory and to get all memories for a user   
        Task SaveMemory(Memory memory); //Take a Memory object and save it

        Task<List<Memory>> GetMemories(string userId); //Give me all memories belonging to this particular user if GetMemories("xyz") is called, it should return all memories that have the userId of "xyz"
    }
}
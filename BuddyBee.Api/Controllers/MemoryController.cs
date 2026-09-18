using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BuddyBee.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemoryController : ControllerBase
    {
        private readonly IMemoryService _memoryService;

        public MemoryController(IMemoryService memoryService)
        {
            _memoryService = memoryService;
        }

        [HttpPost]  // POST api/memory
        public async Task<IActionResult> SaveMemory(Memory memory)
        {
            memory.Id = Guid.NewGuid().ToString();
            memory.CreatedAt = DateTime.UtcNow;

            await _memoryService.SaveMemory(memory);

            return Ok(memory);
        }

        [HttpGet("{userId}")] // GET api/memory/{userId}
        public async Task<IActionResult> GetMemories(string userId)
        {
            var memories = await _memoryService.GetMemories(userId);

            return Ok(memories);
        }
    }
}
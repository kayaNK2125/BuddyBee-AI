using BuddyBee.Api.Models;

namespace BuddyBee.Api.Models
{
    public class Memory
    {
        public string Id { get; set; } = string.Empty;

        public string UserId { get; set; }

        public string Text { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

//Message = something BuddyBee said or the user said in a conversation.
//Memory = information BuddyBee should retain beyond that conversation
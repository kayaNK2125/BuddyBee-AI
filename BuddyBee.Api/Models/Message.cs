namespace BuddyBee.Api.Models
{
    public class Message
    {
        public string Id { get; set; }

        public string ConversationId { get; set; }
        public string Text { get; set; }
        public string Sender { get; set; }
        public DateTime Time { get; set; }
    }
}

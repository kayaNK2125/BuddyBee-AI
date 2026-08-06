namespace BuddyBee.Api.Models
{
    public class Message
    {
        public int id { get; set; }
        public string Text { get; set; }
        public string Sender { get; set; }
        public DateTime Time { get; set; }
    }
}

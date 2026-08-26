namespace BuddyBee.Api.Models
{
    public class SearchResult
    {
        public string Query { get; set; } = "";

        public string? Answer { get; set; }

        public List<SearchResultItem> Results { get; set; } = new();
    }

    public class SearchResultItem
    {
        public string Title { get; set; } = "";

        public string Url { get; set; } = "";

        public string Content { get; set; } = "";

        public double Score { get; set; }

        public string? RawContent { get; set; }
    }
}
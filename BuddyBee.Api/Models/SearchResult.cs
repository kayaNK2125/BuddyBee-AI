namespace BuddyBee.Api.Models
{
    public class SearchResultItem
    {
        public string Title { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public double? Score { get; set; }
    }

    public class SearchResult
    {
        public string Query { get; set; } = string.Empty;

        public string Provider { get; set; } = string.Empty;

        public List<SearchResultItem> Results { get; set; } = new();
    }
}

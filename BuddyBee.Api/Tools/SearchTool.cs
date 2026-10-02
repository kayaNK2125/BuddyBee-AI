using System.Text;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;

namespace BuddyBee.Api.Tools
{
    public class SearchTool : ITool
    {
        private readonly ISearchService _searchService;
        private int _searchCallCount = 0;
        private const int MaxSearchesPerRequest = 3;
        private readonly Dictionary<string, ToolResult> _cache = new(StringComparer.OrdinalIgnoreCase);

        public SearchTool(ISearchService searchService)
        {
            _searchService = searchService;
        }

        public string Name => "search";

        public string Description =>
            "Searches the live web for current information, recent events, or fresh documentation. Requires a 'query' string parameter.";

        public async Task<ToolResult> ExecuteAsync(Dictionary<string, object> arguments)
        {
            object? queryObj = null;

            if (arguments.TryGetValue("query", out var qVal) && qVal != null)
            {
                queryObj = qVal;
            }
            else if (arguments.TryGetValue("q", out var altVal) && altVal != null)
            {
                queryObj = altVal;
            }

            if (queryObj == null)
            {
                return new ToolResult
                {
                    Success = false,
                    Output = "",
                    Error = "Missing 'query' parameter for search."
                };
            }

            var query = queryObj.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(query))
            {
                return new ToolResult
                {
                    Success = false,
                    Output = "",
                    Error = "Search query cannot be empty."
                };
            }

            if (_cache.TryGetValue(query, out var cachedResult))
            {
                return cachedResult;
            }

            if (_searchCallCount >= MaxSearchesPerRequest)
            {
                return new ToolResult
                {
                    Success = false,
                    Output = "",
                    Error = $"Search call limit reached ({MaxSearchesPerRequest} searches per request). Please answer using the evidence already retrieved."
                };
            }

            _searchCallCount++;

            try
            {
                var searchResult = await _searchService.SearchAsync(query);

                if (searchResult.Results == null || searchResult.Results.Count == 0)
                {
                    var emptyResult = new ToolResult
                    {
                        Success = true,
                        Output = $"No web search results found for query: \"{query}\".",
                        Error = ""
                    };

                    _cache[query] = emptyResult;
                    return emptyResult;
                }

                var sb = new StringBuilder();
                sb.AppendLine($"Web search results for \"{query}\" (Provider: {searchResult.Provider}):");

                int index = 1;
                foreach (var item in searchResult.Results.Take(5))
                {
                    sb.AppendLine();
                    sb.AppendLine($"[{index}] {item.Title}");
                    sb.AppendLine($"URL: {item.Url}");
                    sb.AppendLine($"Snippet: {item.Content}");
                    sb.AppendLine($"Source: {item.Source}");
                    index++;
                }

                var toolResult = new ToolResult
                {
                    Success = true,
                    Output = sb.ToString().TrimEnd(),
                    Error = ""
                };

                _cache[query] = toolResult;
                return toolResult;
            }
            catch (Exception ex)
            {
                return new ToolResult
                {
                    Success = false,
                    Output = "",
                    Error = $"Web search failed: {ex.Message}"
                };
            }
        }
    }
}

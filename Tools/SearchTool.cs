using System.Text;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;

namespace BuddyBee.Api.Tools
{
    public class SearchTool : ITool
    {
        private readonly ISearchService _searchService;

        public SearchTool(ISearchService searchService)
        {
            _searchService = searchService;
        }

        public string Name => "search";

        public string Description =>
            "Searches the internet for current, recent, external, " +
            "or factual information.";

        public async Task<ToolResult> ExecuteAsync(
            Dictionary<string, object> arguments)
        {
            // -----------------------------------------------------
            // 1. Check that Gemini provided a search query.
            // -----------------------------------------------------

            if (!arguments.TryGetValue(
                    "query",
                    out var queryValue))
            {
                return new ToolResult
                {
                    Success = false,
                    Error = "Search query is required."
                };
            }

            var query = queryValue?.ToString();

            // -----------------------------------------------------
            // 2. Make sure the query isn't empty.
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(query))
            {
                return new ToolResult
                {
                    Success = false,
                    Error = "Search query cannot be empty."
                };
            }

            try
            {
                // -------------------------------------------------
                // 3. Ask the configured search service to search.
                //
                // SearchTool does NOT know that Tavily is being used.
                // It only knows about ISearchService.
                // -------------------------------------------------

                var result =
                    await _searchService.SearchAsync(query);

                // -------------------------------------------------
                // 4. Convert the search result into clean evidence
                //    that Gemini can understand easily.
                // -------------------------------------------------

                var output = new StringBuilder();

                output.AppendLine(
                    $"Search query: {result.Query}");

                // -------------------------------------------------
                // Tavily's generated summary.
                // -------------------------------------------------

                if (!string.IsNullOrWhiteSpace(result.Answer))
                {
                    output.AppendLine();
                    output.AppendLine("Search summary:");
                    output.AppendLine(result.Answer);
                }

                // -------------------------------------------------
                // Individual web sources.
                // -------------------------------------------------

                output.AppendLine();
                output.AppendLine("Web sources:");

                foreach (var item in result.Results)
                {
                    output.AppendLine();
                    output.AppendLine(
                        $"Title: {item.Title}");

                    output.AppendLine(
                        $"URL: {item.Url}");

                    output.AppendLine(
                        $"Relevance score: {item.Score}");

                    output.AppendLine(
                        $"Content: {item.Content}");
                }

                // -------------------------------------------------
                // 5. Return the formatted evidence to Gemini.
                // -------------------------------------------------

                return new ToolResult
                {
                    Success = true,
                    Output = output.ToString()
                };
            }
            catch (Exception ex)
            {
                // -------------------------------------------------
                // 6. Convert search failures into a normal
                //    ToolResult instead of crashing the provider.
                // -------------------------------------------------

                return new ToolResult
                {
                    Success = false,
                    Error = $"Search failed: {ex.Message}"
                };
            }
        }
    }
}
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;

namespace BuddyBee.Api.Services
{
    public class TavilySearchService : ISearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public TavilySearchService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;

            _apiKey = configuration["TAVILY_API_KEY"]
                ?? throw new Exception(
                    "Tavily API key is missing.");
        }

        public async Task<SearchResult> SearchAsync(
            string query,
            bool includeRawContent = false)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException(
                    "Search query cannot be empty.");
            }

            var requestBody = new
            {
                api_key = _apiKey,
                query = query,
                search_depth = "basic",
                include_answer = true,
                include_raw_content = includeRawContent,
                max_results = 5
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                "https://api.tavily.com/search",
                content);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Tavily search failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Response: {responseBody}");
            }

            var tavilyResponse =
                JsonSerializer.Deserialize<TavilyResponse>(
                    responseBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (tavilyResponse == null)
            {
                throw new Exception(
                    "Tavily returned an empty response.");
            }

            return new SearchResult
            {
                Query = tavilyResponse.Query ?? query,
                Answer = tavilyResponse.Answer,
                Results = tavilyResponse.Results
                    .Select(result => new SearchResultItem
                    {
                        Title = result.Title ?? "",
                        Url = result.Url ?? "",
                        Content = result.Content ?? "",
                        Score = result.Score,
                        RawContent = result.RawContent
                    })
                    .ToList()
            };
        }

        private class TavilyResponse
        {
            public string? Query { get; set; }

            public string? Answer { get; set; }

            public List<TavilyResult> Results { get; set; }
                = new();
        }

        private class TavilyResult
        {
            public string? Title { get; set; }

            public string? Url { get; set; }

            public string? Content { get; set; }

            public double Score { get; set; }

            public string? RawContent { get; set; }
        }
    }
}
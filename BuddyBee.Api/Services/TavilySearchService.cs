using System.Net.Http.Json;
using System.Text.Json;
using BuddyBee.Api.Exceptions;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;

namespace BuddyBee.Api.Services
{
    public class TavilySearchService : ISearchService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TavilySearchService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<SearchResult> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["Tavily:ApiKey"] ?? _configuration["TAVILY_API_KEY"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Tavily API key is not configured.");
            }

            var payload = new
            {
                api_key = apiKey,
                query = query,
                search_depth = "basic",
                include_answer = false,
                max_results = 5
            };

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync("https://api.tavily.com/search", payload, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new SearchProviderException("Tavily", "Network error while connecting to Tavily search service.", isTemporary: true, innerException: ex);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new SearchProviderException("Tavily", "Tavily search request timed out.", isTemporary: true, innerException: ex);
            }

            var statusCode = (int)response.StatusCode;

            if (statusCode == 429 || statusCode == 432)
            {
                throw new SearchProviderException("Tavily", "Tavily search quota exhausted or rate limit reached.", isTemporary: true, statusCode: statusCode);
            }

            if (statusCode >= 500)
            {
                throw new SearchProviderException("Tavily", $"Tavily service returned server error {statusCode}.", isTemporary: true, statusCode: statusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new SearchProviderException("Tavily", $"Tavily search request failed with status {statusCode}: {errorText}", isTemporary: false, statusCode: statusCode);
            }

            var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);

            var searchResult = new SearchResult
            {
                Query = query,
                Provider = "Tavily",
                Results = new List<SearchResultItem>()
            };

            if (doc.RootElement.TryGetProperty("results", out var resultsElement) && resultsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in resultsElement.EnumerateArray())
                {
                    var title = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "" : "";
                    var url = item.TryGetProperty("url", out var urlProp) ? urlProp.GetString() ?? "" : "";
                    var snippet = item.TryGetProperty("content", out var contentProp) ? contentProp.GetString() ?? "" : "";
                    double? score = item.TryGetProperty("score", out var scoreProp) && scoreProp.TryGetDouble(out var scoreVal) ? scoreVal : null;

                    searchResult.Results.Add(new SearchResultItem
                    {
                        Title = title,
                        Url = url,
                        Content = snippet,
                        Source = "Tavily",
                        Score = score
                    });
                }
            }

            return searchResult;
        }
    }
}

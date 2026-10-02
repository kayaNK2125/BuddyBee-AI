using BuddyBee.Api.Models;

namespace BuddyBee.Api.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResult> SearchAsync(string query, CancellationToken cancellationToken = default);
    }
}

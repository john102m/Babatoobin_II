using Examine;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Babatoobin_II.Services
{
    public interface ISearchService
    {
        IEnumerable<IPublishedContent> SearchContentNames(string query);
        IEnumerable<IPublishedContent> SearchResults(string query, out long itemCount, string[]? docTypeAliases = null);
    }
}

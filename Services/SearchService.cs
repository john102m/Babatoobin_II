using Examine;
using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Examine.Search;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;

namespace Babatoobin_II.Services
{
    public class SearchService(IExamineManager examineManager, UmbracoHelper umbracoHelper) : ISearchService
    {
        private readonly IExamineManager _examineManager = examineManager;
        private readonly UmbracoHelper _umbracoHelper = umbracoHelper;

        public IEnumerable<IPublishedContent> SearchContentNames(string query)
        {
            var textFields = new[] { "headline", "detailDescription", "detailSummary" };
            IEnumerable<string> ids = [];
            if (!string.IsNullOrEmpty(query) && _examineManager.TryGetIndex("ExternalIndex", out IIndex? index))
            {
                ids = index
                    .Searcher
                    .CreateQuery("content", BooleanOperation.Or)//.GroupedOr(new[] { "nodeName", "pageTitle", "metaDescription", "bodyText" })
                    .NodeTypeAlias("productBlock")
                    .Or().NodeTypeAlias("homePage")
                    .And().Field("headline", query)
                    .Or().Field("detailDescription", query)
                    .Or().Field("title", query)
                    .Execute()
                    .Select(x => x.Id);
            }
            List<IPublishedContent> things = new();

            foreach (string? id in ids)
            {
                things.Add(_umbracoHelper.Content(id)!);
            }
           
            return [];
        }

        public IEnumerable<IPublishedContent> SearchResults(string query, out long itemCount, string[]? docTypeAliases = null)
        {                
            List<IPublishedContent> items = [];

            if (!string.IsNullOrEmpty(query) && _examineManager.TryGetIndex("ExternalIndex", out IIndex? index))
            {
                
                var searchFields = new List<string>() { "detailDescription", "headline", "detailHeading", "detailSummary", "projectTitle", "tagLine", "category", "title", "bulletPoints" };
                var searcher = (BaseLuceneSearcher)index.Searcher;
                var criteria = searcher.CreateQuery("content", BooleanOperation.Or, searcher.LuceneAnalyzer, new LuceneSearchOptions() { AllowLeadingWildcard = true });
                var examineQuery = criteria.NodeTypeAlias("homePage"); 
                var nodes = new[] { "productBlock", "standardPage"};
                foreach(var node in nodes)
                {
                    examineQuery.Or().NodeTypeAlias(node);
                }

                examineQuery.And().GroupedOr(searchFields, query);//.MultipleCharacterWildcard());
                ISearchResults searchResult = examineQuery.Execute();
                var results = examineQuery.Execute().Select(x => x.Id);

                foreach(var id in results)
                {
                    items.Add(_umbracoHelper.Content(id)!);
                }
            }

            itemCount = items.Count;
            return items;
        }

        private void  DoRawQuery(string query)
        {
            var searchFields = new List<string>();
            var rawQuery = "";
            var terms = query.Trim().Split(' ');
            var searchTerms = new List<string>();
            foreach (var term in terms.Where(x => !string.IsNullOrWhiteSpace(x.Trim())))
            {
                searchTerms.Add(term);
            }
            string model = "allWords";

            if (model == "exactPhrase")
            {
                //Exact
                rawQuery += GetExactPhraseRawFilter(model, searchFields);
            }
            else if (model == "allWords")
            {
                //All
                rawQuery += GetAllWordsRawFilter(searchTerms, searchFields);
            }
            else
            {
                //Any
                rawQuery += GetAnyWordRawFilter(searchTerms, searchFields);
            }

            rawQuery += " NOT hideFromSearch:1";

            //string terms =!string.IsNullOrEmpty(searchTerm)
            //int skip = pageNumber > 1 ? (pageNumber - 1) * pageSize : 0;
            //if(ExamineManager//Instance.TryGetIndex(32))
        }

        protected string GetAnyWordRawFilter(List<string> searchTerms, List<string> searchFields)
        {
            var termsIndex = 0;
            var searchTermCount = searchTerms.Count;
            var any = "(";
            foreach (var term in searchTerms)
            {
                var fieldIndex = 0;
                if (termsIndex > 0 && termsIndex < searchTermCount)
                {
                    any += " OR ";
                }
                foreach (var field in searchFields)
                {
                    if (fieldIndex > 0 && fieldIndex < searchFields.Count)
                    {
                        any += " OR ";
                    }
                    var exactBoost = field == "nodeName" || field == "keywordTags" ? "^3" : "";
                    any += field + ":" + term + exactBoost + " OR ";
                    //Reduce boost for fuzzy terms
                    var fuzzyBoost = field == "nodeName" || field == "keywordTags" ? "^2" : "";
                    any += field + ":" + term + fuzzyBoost + "~0.5";// boost +
                    fieldIndex++;
                }
                termsIndex++;
            }
            any += ")";
            return any;
        }
        protected string GetAllWordsRawFilter(List<string> searchTerms, List<string> searchFields)
        {
            var all = "";
            var termIndex = 0;
            var searchTermCount = searchTerms.Count;
            if (searchTermCount > 1)
            {
                all += "(";
            }
            foreach (var term in searchTerms)
            {
                var fieldIndex = 0;
                if (termIndex > 0 && termIndex < searchTermCount)
                {
                    all += " AND ";
                }

                all += "(";
                foreach (var field in searchFields)
                {
                    if (fieldIndex > 0 && fieldIndex < searchFields.Count)
                    {
                        all += " OR ";
                    }
                    var boost = field == "nodeName" || field == "keywordTags" ? "^3" : "";
                    all += field + ":" + term + boost;// + "~0.5";
                    fieldIndex++;
                }
                all += ")";

                termIndex++;
            }
            if (searchTermCount > 1)
            {
                all += ")";
            }
            return all;
        }

        protected string GetExactPhraseRawFilter(string searchTerm, List<string> searchFields)
        {
            var fieldIndex = 0;
            var exact = "(";
            foreach (var field in searchFields)
            {
                if (fieldIndex > 0 && fieldIndex < searchFields.Count)
                {
                    exact += " OR ";
                }
                var boost = field == "nodeName" || field == "keywordTags" ? "^3" : "";
                exact += field + ":\"" + searchTerm + "\"" + boost;
                fieldIndex++;
            }
            exact += ")";

            return exact;
        }
    }
}

using ODataQueryHelper.Core.Extensions;

namespace ODataQueryHelper.Core
{
    /// <summary>
    /// OData query parser for Document
    /// </summary>
    /// <typeparam name="T">Document of type <typeparamref name="T"/></typeparam>
    public class ODataParser<T> : IODataParser<T> where T : class
    {
        private static readonly string FILTER = "$filter";
        private static readonly string ORDERBY = "$orderby";
        private static readonly string SKIP = "$skip";
        private static readonly string TOP = "$top";
        /// <summary>
        /// Try and Parse OData Query expression
        /// </summary>
        /// <param name="oDataExpression">OData standard query expression</param>
        /// <returns>Parser Document Query of type <typeparamref name="T"/></returns>
        public DocumentQuery<T> TryParse(string oDataExpression)
        {
            var model = new DocumentQuery<T>();
            if (string.IsNullOrEmpty(oDataExpression))
                throw new ArgumentNullException(nameof(oDataExpression));

            var queryStrings = HttpUtility.ParseQueryString(oDataExpression);
            if (!queryStrings.HasKeys())
                return model;
            var filterQuery = queryStrings[FILTER];
            if (!string.IsNullOrEmpty(filterQuery))
                model.Filter.TryParseFilter(filterQuery);

            var orderByQuery = queryStrings[ORDERBY];
            if (!string.IsNullOrEmpty(orderByQuery))
                model.OrderBy.TryParseOrderBy(orderByQuery);

            var skip = queryStrings[SKIP];
            if (!string.IsNullOrEmpty(skip))
                model.TryParseSkip(skip);

            var top = queryStrings[TOP];
            if (!string.IsNullOrEmpty(top))
                model.TryParseTop(top);

            return model;
        }

        public DocumentQuery<T> TryParse(IQueryCollection queryCollection)
        {
            DocumentQuery<T>? model = new DocumentQuery<T>();
            if (queryCollection == null || !queryCollection.Any())
                return model;
            return TryParse(queryCollection.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString())));
        }

        public DocumentQuery<T> TryParse(IEnumerable<KeyValuePair<string, string>> paramList)
        {
            DocumentQuery<T>? model = new DocumentQuery<T>();
            if (paramList == null || !paramList.Any())
                return model;

            if (paramList.Any(x => x.Has(FILTER)))
                model.Filter.TryParseFilter(GetValue(paramList, FILTER));
            if (paramList.Any(x => x.Has(ORDERBY)))
                model.OrderBy.TryParseOrderBy(GetValue(paramList, ORDERBY));
            if (paramList.Any(x => x.Has(SKIP)))
                model.TryParseSkip(GetValue(paramList, SKIP));
            if (paramList.Any(x => x.Has(TOP)))
                model.TryParseTop(GetValue(paramList, TOP));

            return model;
        }

        private static string GetValue(IEnumerable<KeyValuePair<string, string>> paramList, string key) =>
            paramList.First(x => x.Has(key)).Value;
    }
}

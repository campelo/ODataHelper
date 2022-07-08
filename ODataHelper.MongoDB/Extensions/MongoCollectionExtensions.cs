namespace ODataHelper.MongoDB.Extensions
{
    public static class MongoCollectionExtensions
    {
        public static Task<IEnumerable<T>> GetAllAsync<T>(this IMongoCollection<T> mongoCollection, IQueryCollection queryCollection)
            where T : class => mongoCollection.GetAllAsync(queryCollection.ToList().Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString())));

        public static async Task<IEnumerable<T>> GetAllAsync<T>(this IMongoCollection<T> mongoCollection, IEnumerable<KeyValuePair<string, string>> queryList)
            where T : class
        {
            MongoDBQueryRunner<T> runner = new();
            ODataParser<T> docQuery = new();
            DocumentQuery<T> query = docQuery.TryParse(queryList);
            runner.Create(query);
            IList<T>? list = await runner.QueryAsync(mongoCollection);
            return list;
        }
    }
}

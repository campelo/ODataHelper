namespace ODataHelper.MongoDB.Extensions
{
    public static class MongoCollectionExtensions
    {
        public static async Task<IEnumerable<T>> GetAllAsync<T>(this IMongoCollection<T> mongoCollection, IQueryCollection queryCollection)
            where T : class
        {
            MongoDBQueryRunner<T> runner = new();
            ODataParser<T> docQuery = new();
            DocumentQuery<T> query = docQuery.TryParse(queryCollection);
            runner.Create(query);
            IList<T>? list = await runner.QueryAsync(mongoCollection);
            return list;
        }
    }
}

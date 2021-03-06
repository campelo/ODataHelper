namespace ODataQueryRunner.MongoDB
{
    /// <summary>
    /// Creates and Runs OData query on MongoDB Collection
    /// </summary>
    /// <typeparam name="T">Document Entity</typeparam>
    public interface IMongoDBQueryRunner<T> where T : class
    {
        /// <summary>
        /// MongoDB interpreted filter criteria
        /// </summary>
        FilterDefinition<T> FilterDefinition { get; set; }

        /// <summary>
        /// MongoDB interpreted sort criteria
        /// </summary>
        SortDefinition<T> SortDefinition { get; set; }

        /// <summary>
        /// Documents to skip
        /// </summary>
        int Skip { get; set; }

        /// <summary>
        /// Documents to limit in collection
        /// </summary>
        int Limit { get; set; }

        /// <summary>
        /// Creates MongoDB friendly filter, sort , skip and limt options from <paramref name="documentQuery"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <param name="documentQuery">Document Query generated from ODATA expression query</param>
        void Create(DocumentQuery<T> parser);

        /// <summary>
        /// Queries on mongoDB collection and returns list of documents.
        /// </summary>
        /// <param name="mongoCollection">MongoDB Collection</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Document List based on filter and sort definition.</returns>
        Task<IList<T>> QueryAsync(IMongoCollection<T> mongoCollection);
    }
}
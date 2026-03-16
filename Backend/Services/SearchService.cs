using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;


public class SearchService
{
    private readonly SearchClient _searchClient;
    private readonly SearchIndexClient _indexClient;
    private readonly string _indexName;

    public SearchService(string serviceName, string indexName, string apiKey)
    {
        var endpoint = new Uri($"https://{serviceName}.search.windows.net");
        var credential = new AzureKeyCredential(apiKey);
        _indexClient = new SearchIndexClient(endpoint, credential);
        _indexName = indexName;
        _searchClient = new SearchClient(endpoint, indexName, credential);
    }

    public async Task<SearchResults<SearchDocument>> SearchAsync(string query)
    {
        var options = new SearchOptions
        {
            IncludeTotalCount = true
        };
        return await _searchClient.SearchAsync<SearchDocument>(query, options);
    }

    public async Task EnsureIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _indexClient.GetIndexAsync(_indexName, cancellationToken);
            return;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Index does not exist yet; create it below.
        }

        var fieldBuilder = new FieldBuilder();
        var fields = fieldBuilder.Build(typeof(MovieSearchDocument));

        var index = new SearchIndex(_indexName, fields)
        {
            VectorSearch = new VectorSearch
            {
                Algorithms =
                {
                    new HnswAlgorithmConfiguration("movie-vector-hnsw")
                },
                Profiles =
                {
                    new VectorSearchProfile("movie-vector-profile", "movie-vector-hnsw")
                }
            },
            ScoringProfiles =
            {
                new ScoringProfile("movie-text-profile")
                {
                    TextWeights = new TextWeights(MovieSearchDocument.FieldWeights)
                }
            }
        };

        await _indexClient.CreateOrUpdateIndexAsync(index, cancellationToken: cancellationToken);
    }

    public async Task<List<MovieSearchDocument>> VectorSearchAsync(float[] vector, int count = 10)
    {
        var vectorQuery = new VectorizedQuery(vector)
        {
            KNearestNeighborsCount = count,
        };
        vectorQuery.Fields.Add(nameof(MovieSearchDocument.DescriptionVector));

        var options = new SearchOptions();
        options.VectorSearch = new VectorSearchOptions();
        options.VectorSearch.Queries.Add(vectorQuery);

        var response = await _searchClient.SearchAsync<MovieSearchDocument>(searchText: null, options);

        var results = new List<MovieSearchDocument>();
        await foreach (var result in response.Value.GetResultsAsync())
            results.Add(result.Document);

        return results;
    }

    public async Task UpsertMovieAsync(MovieSearchDocument document, CancellationToken cancellationToken = default)
    {
        var batch = IndexDocumentsBatch.Create(IndexDocumentsAction.MergeOrUpload(document));
        await _searchClient.IndexDocumentsAsync(batch, cancellationToken: cancellationToken);
    }

}
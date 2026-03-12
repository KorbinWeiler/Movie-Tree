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

}
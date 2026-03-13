using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

public class MovieSearchDocument
{
    // Field weights used in the Azure Search ScoringProfile (TextWeights).
    // Only searchable text fields are eligible for weights.
    public static readonly Dictionary<string, double> FieldWeights = new()
    {
        { nameof(Title),       1.0 },
        { nameof(Description), 3.0 },
        { nameof(Genres),      0.1 },
    };


    [SimpleField(IsKey = true, IsFilterable = true)]
    public string Id { get; set; } = null!;

    [SearchableField(IsSortable = true)]
    public string Title { get; set; } = null!;

    [SearchableField]
    public string? Description { get; set; }

    [SimpleField]
    public string? PosterUrl { get; set; }

    [SearchableField(IsFilterable = true, IsFacetable = true)]
    public string[] Genres { get; set; } = [];

    [VectorSearchField(VectorSearchDimensions = 1024, VectorSearchProfileName = "movie-vector-profile")]
    public float[]? DescriptionVector { get; set; }
}

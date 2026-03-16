using System.Text.Json;
using System.Text.Json.Serialization;

namespace Backend.Test;

public class ReviewJsonTests
{
    [Fact]
    public void CreateReviewRequest_Deserializes_StringVisibility()
    {
        var json = """
        {
          "movieId": 1,
          "rating": 8,
          "reviewText": "Great movie!",
          "visibility": "Public"
        }
        """;

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());

        var req = JsonSerializer.Deserialize<CreateReviewRequest>(json, options);

        Assert.NotNull(req);
        Assert.Equal(1, req!.MovieId);
        Assert.Equal((byte)8, req.Rating);
        Assert.Equal(ReviewVisibility.Public, req.Visibility);
    }
}

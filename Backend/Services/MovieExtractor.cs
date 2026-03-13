using System.Text.Json;

public class MovieExtractor
{
    public static List<Movie> ExtractMovieIdsFromFile(string filePath)
    {
        var movies = new List<Movie>();

        try
        {
            string jsonData = File.ReadAllText(filePath);
            var jsonDocuments = JsonSerializer.Deserialize<List<JsonElement>>(jsonData);
            if (jsonDocuments != null)
            {
                foreach (var doc in jsonDocuments)
                {
                    var movie = new Movie
                    {
                        Id = doc.GetProperty("id").GetInt32(),
                        Title = doc.GetProperty("original_title").GetString()!
                    };
                    movies.Add(movie);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading movie IDs from file: {ex.Message}");
        }

        return movies;
    }
}
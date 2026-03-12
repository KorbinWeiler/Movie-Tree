using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class TMDBService
{
    private const string BaseUrl = "https://api.themoviedb.org/3";
    private const string PosterBaseUrl = "https://image.tmdb.org/t/p/w500";

    private readonly HttpClient _httpClient;
    private readonly AppDbContext _db;

    public TMDBService(AppDbContext db)
    {
        _db = db;

        var apiKey = Environment.GetEnvironmentVariable("TMDB_READ_API_KEY")
            ?? throw new InvalidOperationException("TMDB_READ_API_KEY environment variable is not set.");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<MovieDetailDto?> GetMovieByIdAsync(int id)
    {
        Console.WriteLine($"Fetching movie {id} from TMDB...");
        var response = await _httpClient.GetAsync($"{BaseUrl}/movie/{id}");
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        if (root.TryGetProperty("original_language", out var lang) &&
            lang.GetString() != "en")
            return null;

        var posterPath = root.TryGetProperty("poster_path", out var pp) ? pp.GetString() : null;
        var posterUrl = posterPath is not null ? $"{PosterBaseUrl}{posterPath}" : null;

        DateOnly? releaseDate = null;
        if (root.TryGetProperty("release_date", out var rd) &&
            DateOnly.TryParse(rd.GetString(), out var parsed))
            releaseDate = parsed;

        var existing = await _db.Movies.FindAsync(id);
        if (existing is not null)
        {
            existing.Title = root.GetProperty("title").GetString()!;
            existing.Description = root.TryGetProperty("overview", out var ov2) ? ov2.GetString() : null;
            existing.ReleaseDate = releaseDate;
            existing.RuntimeMinutes = root.TryGetProperty("runtime", out var rt2) && rt2.ValueKind == JsonValueKind.Number ? rt2.GetInt32() : null;
            existing.PosterUrl = posterUrl;
        }
        else
        {
            var movie = new Movie
            {
                Id = id,
                Title = root.GetProperty("title").GetString()!,
                Description = root.TryGetProperty("overview", out var ov) ? ov.GetString() : null,
                ReleaseDate = releaseDate,
                RuntimeMinutes = root.TryGetProperty("runtime", out var rt) && rt.ValueKind == JsonValueKind.Number ? rt.GetInt32() : null,
                PosterUrl = posterUrl,
            };
            _db.Movies.Add(movie);
        }

        if (root.TryGetProperty("genres", out var genresEl))
        {
            var genreIds = genresEl.EnumerateArray()
                .Select(g => g.GetProperty("id").GetInt32())
                .ToList();

            var validIds = await _db.Genres
                .Where(g => genreIds.Contains(g.Id))
                .Select(g => g.Id)
                .ToListAsync();

            // Only add genre links that don't already exist
            var existingGenreIds = await _db.MovieGenres
                .Where(mg => mg.MovieId == id)
                .Select(mg => mg.GenreId)
                .ToHashSetAsync();

            foreach (var genreId in validIds.Where(g => !existingGenreIds.Contains(g)))
                _db.MovieGenres.Add(new MovieGenre { MovieId = id, GenreId = genreId });
        }

        await _db.SaveChangesAsync();

        var saved = await _db.Movies.FindAsync(id);
        var genres = await _db.MovieGenres
            .Where(mg => mg.MovieId == id)
            .Select(mg => new GenreDto(mg.GenreId, mg.Genre.Name))
            .ToListAsync();

        return new MovieDetailDto(
            Id: saved!.Id,
            Title: saved.Title,
            Description: saved.Description,
            ReleaseDate: saved.ReleaseDate,
            RuntimeMinutes: saved.RuntimeMinutes,
            PosterUrl: saved.PosterUrl,
            AverageRating: null,
            ReviewCount: 0,
            Genres: genres,
            IsVisible: saved.IsVisible
        );
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class GenerateController(
    AppDbContext db,
    ChatService chatService,
    EmbeddedService embeddedService,
    SearchService searchService,
    ILogger<GenerateController> logger) : ControllerBase
{
    private const int DefaultRecommendationCount = 30;
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private static MovieSummaryDto ToSummary(Movie m) => new(
        m.Id, m.Title, m.PosterUrl, m.ReleaseDate,
        m.Reviews.Count > 0 ? Math.Round(m.Reviews.Average(r => (double)r.Rating), 1) : null,
        m.Reviews.Count,
        m.MovieGenres.Select(mg => new GenreDto(mg.Genre.Id, mg.Genre.Name))
    );

    private static RecommendResultDto ToRecommendResult(MovieSearchDocument document) => new(
        document.Id,
        document.Title,
        document.PosterUrl,
        document.Description,
        document.Genres
    );

    private static RecommendResultDto ToRecommendResult(Movie movie) => new(
        movie.Id.ToString(),
        movie.Title,
        movie.PosterUrl,
        movie.Description,
        movie.MovieGenres.Select(mg => mg.Genre.Name).ToArray()
    );

    [HttpPost("recommend")]
    [Authorize]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest req)
    {
        return await RecommendFromMovieIdsAsync(req.MovieIds, req.Count);
    }

    [HttpPost("recommend/all")]
    [Authorize]
    public async Task<IActionResult> RecommendFromAllReviewed([FromBody] RecommendCountRequest? req)
    {
        var reviewedMovieIds = await db.Reviews
            .Where(r => r.UserId == CurrentUserId && r.Movie.IsVisible)
            .Select(r => r.MovieId)
            .Distinct()
            .ToListAsync();

        if (reviewedMovieIds.Count == 0)
            return NotFound("You have not reviewed any visible movies yet.");

        return await RecommendFromMovieIdsAsync(reviewedMovieIds, req?.Count ?? DefaultRecommendationCount);
    }

    [HttpPost("recommend/ai")]
    [Authorize]
    public async Task<IActionResult> RecommendFromAi([FromBody] RecommendCountRequest? req)
    {
        var count = NormalizeCount(req?.Count ?? DefaultRecommendationCount);

        string generatedDescription;
        try
        {
            generatedDescription = await chatService.GenerateRandomMovieTasteDescription();
        }
        catch (Exception ex)
        {
            var fallbackDescriptions = await db.Movies
                .Where(m => m.IsVisible && m.Description != null)
                .OrderBy(_ => Guid.NewGuid())
                .Take(3)
                .Select(m => m.Description!)
                .ToListAsync();

            generatedDescription = BuildFallbackDescription(fallbackDescriptions);

            if (string.IsNullOrWhiteSpace(generatedDescription))
                return StatusCode(502, $"AI generation failed: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(generatedDescription))
            return StatusCode(502, "AI returned an empty description.");

        try
        {
            var results = await SearchAndFillAsync(generatedDescription, new HashSet<int>(), count);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(502, ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Generate([FromQuery] int count = DefaultRecommendationCount)
    {
        var clampedCount = NormalizeCount(count);

        var seedIds = await db.Movies
            .Where(m => m.IsVisible && m.Description != null)
            .OrderBy(_ => Guid.NewGuid())
            .Take(3)
            .Select(m => m.Id)
            .ToListAsync();

        if (seedIds.Count == 0)
            return Ok(Array.Empty<RecommendResultDto>());

        return await RecommendFromMovieIdsAsync(seedIds, clampedCount);
    }

    private async Task<IActionResult> RecommendFromMovieIdsAsync(IReadOnlyCollection<int>? movieIds, int requestedCount)
    {
        try
        {
            if (movieIds is null || movieIds.Count == 0)
                return BadRequest("At least one movie ID is required.");

            var count = NormalizeCount(requestedCount);
            var movieIdSet = movieIds.ToHashSet();

            var descriptions = await db.Movies
                .Where(m => movieIdSet.Contains(m.Id) && m.IsVisible && m.Description != null)
                .Select(m => m.Description!)
                .ToListAsync();

            if (descriptions.Count == 0)
                return NotFound("No descriptions found for the provided movie IDs.");

            string generalizedDescription;
            try
            {
                generalizedDescription = await chatService.GeneralizeMovieDescriptions(descriptions.ToArray());
            }
            catch (Exception ex)
            {
                generalizedDescription = BuildFallbackDescription(descriptions);

                if (string.IsNullOrWhiteSpace(generalizedDescription))
                    return StatusCode(502, $"AI generalization failed: {ex.Message}");
            }

            if (string.IsNullOrWhiteSpace(generalizedDescription))
                return StatusCode(502, "AI returned an empty description.");

            try
            {
                var results = await SearchAndFillAsync(generalizedDescription, movieIdSet, count);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(502, ex.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Generate pipeline failed in RecommendFromMovieIdsAsync for movieIds={MovieIds}", movieIds);
            return StatusCode(500, new { error = "Generate pipeline failed", message = ex.Message });
        }
    }

    private async Task<List<RecommendResultDto>> SearchAndFillAsync(
        string seedDescription,
        HashSet<int> excludedMovieIds,
        int count)
    {
        float[] vector;
        try
        {
            vector = await embeddedService.VectorizeTextAsync(seedDescription);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Vectorization failed: {ex.Message}", ex);
        }

        List<MovieSearchDocument> searchResults;
        try
        {
            searchResults = await searchService.VectorSearchAsync(vector, count);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Search failed: {ex.Message}", ex);
        }

        var seenMovieIds = new HashSet<int>(excludedMovieIds);
        var results = new List<RecommendResultDto>();

        foreach (var document in searchResults)
        {
            if (!int.TryParse(document.Id, out var movieId))
                continue;

            if (!seenMovieIds.Add(movieId))
                continue;

            results.Add(ToRecommendResult(document));

            if (results.Count == count)
                return results;
        }

        if (results.Count < count)
        {
            var fallbackMovies = await db.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Where(m => m.IsVisible && !seenMovieIds.Contains(m.Id))
                .OrderBy(_ => Guid.NewGuid())
                .Take(count - results.Count)
                .ToListAsync();

            foreach (var movie in fallbackMovies)
            {
                if (!seenMovieIds.Add(movie.Id))
                    continue;

                results.Add(ToRecommendResult(movie));

                if (results.Count == count)
                    break;
            }
        }

        return results;
    }

    private static int NormalizeCount(int requestedCount) => Math.Clamp(requestedCount, 1, 50);

    private static string BuildFallbackDescription(IEnumerable<string> descriptions)
    {
        return string.Join("\n\n", descriptions
            .Where(description => !string.IsNullOrWhiteSpace(description))
            .Select(description => description.Trim())
            .Distinct(StringComparer.Ordinal)
            .Take(5));
    }
}

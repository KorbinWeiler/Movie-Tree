using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class GenerateController(
    AppDbContext db,
    ChatService chatService,
    EmbeddedService embeddedService,
    SearchService searchService) : ControllerBase
{
    private static MovieSummaryDto ToSummary(Movie m) => new(
        m.Id, m.Title, m.PosterUrl, m.ReleaseDate,
        m.Reviews.Count > 0 ? Math.Round(m.Reviews.Average(r => (double)r.Rating), 1) : null,
        m.Reviews.Count,
        m.MovieGenres.Select(mg => new GenreDto(mg.Genre.Id, mg.Genre.Name))
    );

    // POST /api/generate/recommend
    // Body: { "movieIds": [1,2,3], "count": 10 }
    // Flow: fetch descriptions → generalize via Gemini → vectorize via Azure Vision → vector search
    [HttpPost("recommend")]
    [Authorize]
    public async Task<IActionResult> Recommend([FromBody] RecommendRequest req)
    {
        if (req.MovieIds is null || req.MovieIds.Count == 0)
            return BadRequest("At least one movie ID is required.");

        var count = Math.Clamp(req.Count, 1, 50);

        var descriptions = await db.Movies
            .Where(m => req.MovieIds.Contains(m.Id) && m.IsVisible && m.Description != null)
            .Select(m => m.Description!)
            .ToListAsync();

        if (descriptions.Count == 0)
            return NotFound("No descriptions found for the provided movie IDs.");

        string generalizedDescription;
        try
        {
            generalizedDescription = await chatService.GeneralizeMovieDescriptions([.. descriptions]);
        }
        catch (Exception ex)
        {
            return StatusCode(502, $"AI generalization failed: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(generalizedDescription))
            return StatusCode(502, "AI returned an empty description.");

        float[] vector;
        try
        {
            vector = await embeddedService.VectorizeTextAsync(generalizedDescription);
        }
        catch (Exception ex)
        {
            return StatusCode(502, $"Vectorization failed: {ex.Message}");
        }

        List<MovieSearchDocument> searchResults;
        try
        {
            searchResults = await searchService.VectorSearchAsync(vector, count);
        }
        catch (Exception ex)
        {
            return StatusCode(502, $"Search failed: {ex.Message}");
        }

        var inputIds = req.MovieIds.ToHashSet();
        var results = searchResults
            .Where(d => !inputIds.Contains(int.TryParse(d.Id, out var id) ? id : -1))
            .Select(d => new RecommendResultDto(d.Id, d.Title, d.PosterUrl, d.Description, d.Genres));

        return Ok(results);
    }

    // GET /api/generate — 9 random movies from the database
    [HttpGet]
    public async Task<IActionResult> Generate()
    {
        var totalCount = await db.Movies.CountAsync(m => m.IsVisible);
        if (totalCount == 0) return Ok(Array.Empty<MovieSummaryDto>());

        var skip = new Random().Next(0, Math.Max(0, totalCount - 9));

        var movies = await db.Movies
            .Include(m => m.Reviews)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Where(m => m.IsVisible)
            .Skip(skip)
            .Take(9)
            .ToListAsync();

        return Ok(movies.Select(ToSummary));
    }
}

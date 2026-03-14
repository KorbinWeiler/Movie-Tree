using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class GenerateController(AppDbContext db) : ControllerBase
{
    private static MovieSummaryDto ToSummary(Movie m) => new(
        m.Id, m.Title, m.PosterUrl, m.ReleaseDate,
        m.Reviews.Count > 0 ? Math.Round(m.Reviews.Average(r => (double)r.Rating), 1) : null,
        m.Reviews.Count,
        m.MovieGenres.Select(mg => new GenreDto(mg.Genre.Id, mg.Genre.Name))
    );

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

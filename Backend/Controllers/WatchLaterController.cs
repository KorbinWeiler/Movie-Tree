using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WatchLaterController(AppDbContext db) : ControllerBase
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET /api/watchlater
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await db.WatchLaterItems
            .Include(w => w.Movie).ThenInclude(m => m.Reviews)
            .Include(w => w.Movie).ThenInclude(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Where(w => w.UserId == CurrentUserId)
            .OrderByDescending(w => w.AddedAt)
            .ToListAsync();

        return Ok(items.Select(w => new WatchLaterDto(
            w.Id,
            new MovieSummaryDto(
                w.Movie.Id, w.Movie.Title, w.Movie.PosterUrl, w.Movie.ReleaseDate,
                w.Movie.Reviews.Count > 0 ? Math.Round(w.Movie.Reviews.Average(r => (double)r.Rating), 1) : null,
                w.Movie.Reviews.Count,
                w.Movie.MovieGenres.Select(mg => new GenreDto(mg.Genre.Id, mg.Genre.Name))
            ),
            w.AddedAt
        )));
    }

    // POST /api/watchlater/{movieId}
    [HttpPost("{movieId:int}")]
    public async Task<IActionResult> Add(int movieId)
    {
        var movieExists = await db.Movies.AnyAsync(m => m.Id == movieId);
        if (!movieExists) return NotFound("Movie not found.");

        var existing = await db.WatchLaterItems.AnyAsync(w => w.UserId == CurrentUserId && w.MovieId == movieId);
        if (existing) return Conflict("Movie is already in your Watch Later list.");

        db.WatchLaterItems.Add(new WatchLaterItem { UserId = CurrentUserId, MovieId = movieId });
        await db.SaveChangesAsync();
        return Ok();
    }

    // DELETE /api/watchlater/{movieId}
    [HttpDelete("{movieId:int}")]
    public async Task<IActionResult> Remove(int movieId)
    {
        var item = await db.WatchLaterItems
            .FirstOrDefaultAsync(w => w.UserId == CurrentUserId && w.MovieId == movieId);

        if (item is null) return NotFound();

        db.WatchLaterItems.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

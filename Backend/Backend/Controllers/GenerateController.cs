using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class GenerateController(AppDbContext db) : ControllerBase
{
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static MovieSummaryDto ToSummary(Movie m) => new(
        m.Id, m.Title, m.PosterUrl, m.ReleaseDate,
        m.Reviews.Count > 0 ? Math.Round(m.Reviews.Average(r => (double)r.Rating), 1) : null,
        m.Reviews.Count,
        m.MovieGenres.Select(mg => new GenreDto(mg.Genre.Id, mg.Genre.Name))
    );

    // GET /api/generate/ai-picks — latest global AI picks (home page)
    [HttpGet("ai-picks")]
    public async Task<IActionResult> GlobalPicks()
    {
        var list = await db.AiPickLists
            .Include(l => l.Items).ThenInclude(i => i.Movie).ThenInclude(m => m.Reviews)
            .Include(l => l.Items).ThenInclude(i => i.Movie).ThenInclude(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Where(l => l.UserId == null && l.GenerationMode == AiGenerationMode.Global)
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync();

        if (list is null) return Ok(new GeneratedPickDto(0, []));

        return Ok(new GeneratedPickDto(
            list.Id,
            list.Items.OrderBy(i => i.Position).Select(i => new PickedMovieDto(i.Position, ToSummary(i.Movie)))
        ));
    }

    // POST /api/generate — generate a list for the current user
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Generate([FromBody] GenerateRequest req)
    {
        var userId = CurrentUserId!;

        // Collect already-reviewed movie IDs for this user (to exclude from results)
        var reviewedIds = await db.Reviews
            .Where(r => r.UserId == userId)
            .Select(r => r.MovieId)
            .ToListAsync();

        IQueryable<Movie> candidateQuery = db.Movies
            .Include(m => m.Reviews)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Where(m => m.Reviews.Count > 0);  // only movies with at least one review

        List<Movie> candidates;

        switch (req.Mode)
        {
            case AiGenerationMode.AllHistory:
            {
                // Seed genres from user's entire review history
                var genreIds = await db.Reviews
                    .Where(r => r.UserId == userId)
                    .SelectMany(r => r.Movie.MovieGenres.Select(mg => mg.GenreId))
                    .Distinct()
                    .ToListAsync();

                candidates = await candidateQuery
                    .Where(m => !reviewedIds.Contains(m.Id) &&
                                m.MovieGenres.Any(mg => genreIds.Contains(mg.GenreId)))
                    .OrderByDescending(m => m.Reviews.Average(r => (double)r.Rating))
                    .Take(50)
                    .ToListAsync();
                break;
            }
            case AiGenerationMode.Selected:
            {
                if (req.MovieIds is null || req.MovieIds.Length == 0)
                    return BadRequest("MovieIds is required for Selected mode.");

                var genreIds = await db.MovieGenres
                    .Where(mg => req.MovieIds.Contains(mg.MovieId))
                    .Select(mg => mg.GenreId)
                    .Distinct()
                    .ToListAsync();

                candidates = await candidateQuery
                    .Where(m => !reviewedIds.Contains(m.Id) &&
                                !req.MovieIds.Contains(m.Id) &&
                                m.MovieGenres.Any(mg => genreIds.Contains(mg.GenreId)))
                    .OrderByDescending(m => m.Reviews.Average(r => (double)r.Rating))
                    .Take(50)
                    .ToListAsync();
                break;
            }
            case AiGenerationMode.Genre:
            {
                if (!req.GenreId.HasValue)
                    return BadRequest("GenreId is required for Genre mode.");

                candidates = await candidateQuery
                    .Where(m => !reviewedIds.Contains(m.Id) &&
                                m.MovieGenres.Any(mg => mg.GenreId == req.GenreId.Value))
                    .OrderByDescending(m => m.Reviews.Average(r => (double)r.Rating))
                    .Take(50)
                    .ToListAsync();
                break;
            }
            case AiGenerationMode.FullAI:
            default:
            {
                // Diverse pick: highest-rated unseen movies
                candidates = await candidateQuery
                    .Where(m => !reviewedIds.Contains(m.Id))
                    .OrderByDescending(m => m.Reviews.Average(r => (double)r.Rating))
                    .ThenByDescending(m => m.Reviews.Count)
                    .Take(100)
                    .ToListAsync();
                break;
            }
        }

        // Pick up to 10, diversifying genres where possible
        var picked = PickDiverse(candidates, 10);

        var pickList = new AiPickList
        {
            UserId = userId,
            GenerationMode = req.Mode,
            GenreId = req.Mode == AiGenerationMode.Genre ? req.GenreId : null,
        };

        db.AiPickLists.Add(pickList);
        await db.SaveChangesAsync();

        byte position = 1;
        foreach (var movie in picked)
        {
            db.AiPickListItems.Add(new AiPickListItem
            {
                AiPickListId = pickList.Id,
                MovieId = movie.Id,
                Position = position++,
            });
        }
        await db.SaveChangesAsync();

        return Ok(new GeneratedPickDto(
            pickList.Id,
            picked.Select((m, i) => new PickedMovieDto((byte)(i + 1), ToSummary(m)))
        ));
    }

    // Picks up to `count` movies, preferring genre variety
    private static List<Movie> PickDiverse(List<Movie> candidates, int count)
    {
        if (candidates.Count <= count) return candidates;

        var result = new List<Movie>(count);
        var usedGenres = new HashSet<int>();

        // First pass: one movie per genre
        foreach (var movie in candidates)
        {
            if (result.Count >= count) break;
            var genres = movie.MovieGenres.Select(mg => mg.GenreId).ToList();
            if (!genres.Any(g => usedGenres.Contains(g)))
            {
                result.Add(movie);
                foreach (var g in genres) usedGenres.Add(g);
            }
        }

        // Second pass: fill remaining slots
        foreach (var movie in candidates)
        {
            if (result.Count >= count) break;
            if (!result.Contains(movie))
                result.Add(movie);
        }

        return result;
    }
}

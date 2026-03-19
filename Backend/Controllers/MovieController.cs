using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class MovieController(AppDbContext db) : ControllerBase
{
    private static MovieSummaryDto ToSummary(Movie m) => new(
        m.Id, m.Title, m.PosterUrl, m.ReleaseDate,
        m.Reviews.Count > 0 ? Math.Round(m.Reviews.Average(r => (double)r.Rating), 1) : null,
        m.Reviews.Count,
        m.MovieGenres.Select(mg => new GenreDto(mg.Genre.Id, mg.Genre.Name))
    );

    private static MovieDetailDto ToDetail(Movie m) => new(
        m.Id, m.Title, m.Description, m.ReleaseDate, m.RuntimeMinutes, m.PosterUrl,
        m.Reviews.Count > 0 ? Math.Round(m.Reviews.Average(r => (double)r.Rating), 1) : null,
        m.Reviews.Count,
        m.MovieGenres.Select(mg => new GenreDto(mg.Genre.Id, mg.Genre.Name)),
        m.IsVisible
    );

    // GET /api/movie?q=title&genreId=1&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] int? genreId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageSize > 100) pageSize = 100;

        var query = db.Movies
            .Include(m => m.Reviews)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Where(m => m.IsVisible)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(m => m.Title.Contains(q));

        if (genreId.HasValue)
            query = query.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId.Value));

        // Special-case: if the client requested the custom "Josh" genre, return a
        // hardcoded list of movie IDs instead of filtering by MovieGenres.
        if (genreId.HasValue && genreId.Value == 99999)
        {
            var joshIds = new[] { 6439, 157763, 538854, 18980, 15922, 21599, 77716, 61828, 9023, 603206, 57212 };// hardcoded movie IDs for "Josh"
            var joshMovies = await db.Movies
                .Include(m => m.Reviews)
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Where(m => joshIds.Contains(m.Id) && m.IsVisible)
                .OrderBy(m => m.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(joshMovies.Select(ToSummary));
        }

        var movies = await query
            .OrderBy(m => m.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(movies.Select(ToSummary));
    }

    // GET /api/movie/trending
    [HttpGet("trending")]
    public async Task<IActionResult> Trending([FromQuery] int count = 20)
    {
        if (count > 50) count = 50;

        var cutoff = DateTime.UtcNow.AddDays(-30);
        var movies = await db.Movies
            .Include(m => m.Reviews)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Where(m => m.IsVisible && m.Reviews.Any(r => r.CreatedAt >= cutoff))
            .OrderByDescending(m => m.Reviews.Count(r => r.CreatedAt >= cutoff))
            .ThenByDescending(m => m.Reviews.Average(r => (double)r.Rating))
            .Take(count)
            .ToListAsync();

        return Ok(movies.Select(ToSummary));
    }

    // GET /api/movie/temp-trending — random movies, used until real trending data exists
    [HttpGet("temp-trending")]
    public async Task<IActionResult> TempTrending([FromQuery] int count = 30)
    {
        if (count > 30) count = 30;
        var total = await db.Movies.CountAsync(m => m.IsVisible);
        if (total == 0) return Ok(Array.Empty<MovieSummaryDto>());

        var skip = new Random().Next(0, Math.Max(0, total - count));
        var movies = await db.Movies
            .Include(m => m.Reviews)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Where(m => m.IsVisible)
            .Skip(skip)
            .Take(count)
            .ToListAsync();

        return Ok(movies.Select(ToSummary));
    }

    // GET /api/movie/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        // Admins can fetch hidden movies (to unhide from the modal)
        var isAdmin = User.IsInRole("Admin");
        var movie = await db.Movies
            .Include(m => m.Reviews)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(m => m.Id == id && (m.IsVisible || isAdmin));

        if (movie is null)
        {
            try
            {
                var tmdb = new TMDBService(db);
                var detail = await tmdb.GetMovieByIdAsync(id);
                if (detail is null) return NotFound($"Movie with ID {id} not found.");
                return Ok(detail);
            }
            catch (Exception ex)
            {
                return NotFound($"Movie with ID {id} not found. Error: {ex.Message}");
            }
        }

        if (movie.Description is null)
        {
            try
            {
                var tmdb = new TMDBService(db);
                var detail = await tmdb.GetMovieByIdAsync(id);
                if (detail is not null) return Ok(detail);
            }
            catch
            {
                // Ignore TMDB failures and fall back to the DB record.
            }
        }

        return Ok(ToDetail(movie));
    }

    // GET /api/movie/batch?ids=1,2,3
    [HttpGet("batch")]
    public async Task<IActionResult> GetBatch([FromQuery] string ids)
    {
        if (string.IsNullOrWhiteSpace(ids)) return Ok(Array.Empty<MovieSummaryDto>());

        var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var n) ? (int?)n : null)
            .Where(n => n.HasValue)
            .Select(n => n!.Value)
            .Distinct()
            .Take(50)
            .ToList();

        var movies = await db.Movies
            .Include(m => m.Reviews)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Where(m => idList.Contains(m.Id) && m.IsVisible)
            .ToListAsync();

        // Return in the same order as the requested ids
        var ordered = idList
            .Select(id => movies.FirstOrDefault(m => m.Id == id))
            .Where(m => m is not null)
            .Select(m => ToSummary(m!));

        return Ok(ordered);
    }

    // GET /api/movie/genres
    [HttpGet("genres")]
    public async Task<IActionResult> GetGenres()
    {
        var genres = await db.Genres.OrderBy(g => g.Name).ToListAsync();
        var dtos = genres.Select(g => new GenreDto(g.Id, g.Name)).ToList();
        // Append a custom genre called "Josh" at the end of the results
        dtos.Add(new GenreDto(99999, "Josh"));
        return Ok(dtos);
    }

    [HttpPost("list/upload")]
    [Authorize (Roles = "Admin")]
    public async Task<IActionResult> UploadMovieList(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var tempFilePath = Path.GetTempFileName();
        try
        {
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var movies = MovieExtractor.ExtractMovieIdsFromFile(tempFilePath);
            if (movies.Count == 0)
                return BadRequest("No valid movie data found in the file.");

            foreach (var movie in movies)
            {
                if (!await db.Movies.AnyAsync(m => m.Id == movie.Id))
                {
                    db.Movies.Add(movie);
                }
            }
            await db.SaveChangesAsync();

            return Ok(new { AddedCount = movies.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error processing file: {ex.Message}");
        }
        finally
        {
            System.IO.File.Delete(tempFilePath);
        }
    }
}
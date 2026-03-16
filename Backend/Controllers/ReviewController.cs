using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ReviewController(
    AppDbContext db,
    EmbeddedService embeddedService,
    SearchService searchService,
    ILogger<ReviewController> logger) : ControllerBase
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    private static ReviewDto ToDto(Review r) => new(
        r.Id, r.UserId, r.User.UserName!, r.User.ProfilePictureUrl,
        r.MovieId, r.Movie.Title, r.Movie.PosterUrl,
        r.Rating, r.ReviewText, r.Visibility, r.CreatedAt, r.UpdatedAt
    );

    // GET /api/review/movie/{movieId} — all public reviews for a movie
    [HttpGet("movie/{movieId:int}")]
    public async Task<IActionResult> GetForMovie(int movieId)
    {
        var reviews = await db.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .Where(r => r.MovieId == movieId && r.Visibility == ReviewVisibility.Public && r.Movie.IsVisible)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(reviews.Select(ToDto));
    }

    // POST /api/review
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest req)
    {
        if (req.Rating < 1 || req.Rating > 10)
            return BadRequest("Rating must be between 1 and 10.");

        var movieExists = await db.Movies.AnyAsync(m => m.Id == req.MovieId && m.IsVisible);
        if (!movieExists) return NotFound("Movie not found.");

        var existing = await db.Reviews.AnyAsync(r => r.UserId == CurrentUserId && r.MovieId == req.MovieId);
        if (existing) return Conflict("You have already reviewed this movie.");

        var review = new Review
        {
            UserId = CurrentUserId,
            MovieId = req.MovieId,
            Rating = req.Rating,
            ReviewText = req.ReviewText,
            Visibility = req.Visibility,
        };

        db.Reviews.Add(review);
        await db.SaveChangesAsync();

        // Keep the search index refreshed with the latest movie/review context.
        try
        {
            await SyncMovieDocumentToSearchAsync(req.MovieId);
        }
        catch (Exception ex)
        {
            // Review creation should not fail if external embedding/search services are unavailable.
            logger.LogWarning(ex, "Failed to sync movie {MovieId} to search index after review creation.", req.MovieId);
        }

        await db.Entry(review).Reference(r => r.User).LoadAsync();
        await db.Entry(review).Reference(r => r.Movie).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = review.Id }, ToDto(review));
    }

    // GET /api/review/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var review = await db.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null) return NotFound();

        // Only return private/friends reviews to the owner
        if (review.Visibility == ReviewVisibility.Private && review.UserId != CurrentUserId)
            return NotFound();

        return Ok(ToDto(review));
    }

    // PUT /api/review/{id}
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewRequest req)
    {
        var review = await db.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null) return NotFound();
        if (review.UserId != CurrentUserId) return Forbid();

        if (req.Rating.HasValue)
        {
            if (req.Rating.Value < 1 || req.Rating.Value > 10)
                return BadRequest("Rating must be between 1 and 10.");
            review.Rating = req.Rating.Value;
        }
        if (req.ReviewText is not null) review.ReviewText = req.ReviewText;
        if (req.Visibility.HasValue) review.Visibility = req.Visibility.Value;
        review.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(ToDto(review));
    }

    // DELETE /api/review/{id}
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await db.Reviews.FindAsync(id);
        if (review is null) return NotFound();

        // Owner or moderator/admin can delete
        var isPrivileged = User.IsInRole("Moderator") || User.IsInRole("Admin");
        if (review.UserId != CurrentUserId && !isPrivileged) return Forbid();

        db.Reviews.Remove(review);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/review/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId)
    {
        var isOwner = User.Identity?.IsAuthenticated == true && CurrentUserId == userId;

        var query = db.Reviews
            .Include(r => r.User)
            .Include(r => r.Movie)
            .Where(r => r.UserId == userId);

        if (!isOwner)
            query = query.Where(r => r.Visibility == ReviewVisibility.Public);

        var reviews = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return Ok(reviews.Select(ToDto));
    }

    private async Task SyncMovieDocumentToSearchAsync(int movieId)
    {
        var movie = await db.Movies
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == movieId && m.IsVisible);

        if (movie is null)
            return;

        var genres = movie.MovieGenres
            .Select(mg => mg.Genre.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var reviewSnippets = movie.Reviews
            .Where(r => !string.IsNullOrWhiteSpace(r.ReviewText))
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(r => r.ReviewText!.Trim());

        var combinedText = string.Join("\n", new[]
        {
            movie.Title,
            movie.Description,
            genres.Length > 0 ? $"Genres: {string.Join(", ", genres)}" : null,
            reviewSnippets.Any() ? $"Recent reviews: {string.Join(" | ", reviewSnippets)}" : null,
        }.Where(s => !string.IsNullOrWhiteSpace(s)));

        if (string.IsNullOrWhiteSpace(combinedText))
            return;

        var vector = await embeddedService.VectorizeTextAsync(combinedText);

        var document = new MovieSearchDocument
        {
            Id = movie.Id.ToString(),
            Title = movie.Title,
            Description = movie.Description,
            PosterUrl = movie.PosterUrl,
            Genres = genres,
            DescriptionVector = vector,
        };

        await searchService.UpsertMovieAsync(document);
    }
}
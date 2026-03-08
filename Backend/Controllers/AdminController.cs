using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext db) : ControllerBase
{
    // POST /api/admin/import-movies
    [HttpPost("import-movies")]
    [RequestSizeLimit(250_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 250_000_000)]
    public async Task<IActionResult> ImportMovies(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        var existingIds = await db.Movies.Select(m => m.Id).ToHashSetAsync();

        var toAdd = new List<Movie>();
        int added = 0;
        int skipped = 0;

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;

                if (!root.TryGetProperty("id", out var idProp) ||
                    !root.TryGetProperty("original_title", out var titleProp))
                    continue;

                int id = idProp.GetInt32();
                string title = titleProp.GetString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(title)) continue;

                if (existingIds.Contains(id))
                {
                    skipped++;
                    continue;
                }

                toAdd.Add(new Movie { Id = id, Title = title });
                existingIds.Add(id);
                added++;

                if (toAdd.Count >= 500)
                {
                    db.Movies.AddRange(toAdd);
                    await db.SaveChangesAsync();
                    toAdd.Clear();
                }
            }
            catch (JsonException) { /* skip malformed lines */ }
        }

        if (toAdd.Count > 0)
        {
            db.Movies.AddRange(toAdd);
            await db.SaveChangesAsync();
        }

        return Ok(new { added, skipped });
    }
}

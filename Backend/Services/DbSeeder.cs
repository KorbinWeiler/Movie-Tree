using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public static class DbSeeder
{
    private static readonly Genre[] Genres =
    [
        new() { Id = 28,    Name = "Action" },
        new() { Id = 12,    Name = "Adventure" },
        new() { Id = 16,    Name = "Animation" },
        new() { Id = 35,    Name = "Comedy" },
        new() { Id = 80,    Name = "Crime" },
        new() { Id = 99,    Name = "Documentary" },
        new() { Id = 18,    Name = "Drama" },
        new() { Id = 10751, Name = "Family" },
        new() { Id = 14,    Name = "Fantasy" },
        new() { Id = 36,    Name = "History" },
        new() { Id = 27,    Name = "Horror" },
        new() { Id = 10402, Name = "Music" },
        new() { Id = 9648,  Name = "Mystery" },
        new() { Id = 10749, Name = "Romance" },
        new() { Id = 878,   Name = "Science Fiction" },
        new() { Id = 10770, Name = "TV Movie" },
        new() { Id = 53,    Name = "Thriller" },
        new() { Id = 10752, Name = "War" },
        new() { Id = 37,    Name = "Western" },
    ];

    public static async Task SeedGenresAsync(IServiceProvider services, ILogger logger)
    {
        var db = services.GetRequiredService<AppDbContext>();

        var existingIds = await db.Genres.Select(g => g.Id).ToHashSetAsync();
        var toAdd = Genres.Where(g => !existingIds.Contains(g.Id)).ToList();

        if (toAdd.Count == 0)
        {
            logger.LogInformation("Genres already seeded — skipping.");
            return;
        }

        db.Genres.AddRange(toAdd);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} genres.", toAdd.Count);
    }

    public static async Task SeedMoviesAsync(IServiceProvider services, ILogger logger)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var filePath = config["Seeder:MovieFilePath"];

        if (string.IsNullOrWhiteSpace(filePath))
        {
            logger.LogInformation("Seeder:MovieFilePath not configured — skipping movie seed.");
            return;
        }

        if (!File.Exists(filePath))
        {
            logger.LogWarning("Movie seed file not found at '{Path}' — skipping.", filePath);
            return;
        }

        var db = services.GetRequiredService<AppDbContext>();

        var existingIds = await db.Movies.Select(m => m.Id).ToHashSetAsync();
        logger.LogInformation("Seeding movies from '{Path}' ({Existing} already in DB)...", filePath, existingIds.Count);

        var toAdd = new List<Movie>(1000);
        int added = 0;
        int skipped = 0;

        await using var stream = File.OpenRead(filePath);
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

                if (existingIds.Contains(id)) { skipped++; continue; }

                toAdd.Add(new Movie { Id = id, Title = title });
                existingIds.Add(id);
                added++;

                if (toAdd.Count >= 1000)
                {
                    try
                    {
                        db.Movies.AddRange(toAdd);
                        await db.SaveChangesAsync();
                        db.ChangeTracker.Clear();
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Batch save failed: {Msg} — skipping batch.", ex.Message);
                        db.ChangeTracker.Clear();
                    }
                    toAdd.Clear();

                    if (added % 50_000 == 0)
                        logger.LogInformation("  {Added} movies added so far...", added);
                }
            }
            catch (JsonException) { /* skip malformed lines */ }
        }

        if (toAdd.Count > 0)
        {
            try
            {
                db.Movies.AddRange(toAdd);
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();
            }
            catch (Exception ex)
            {
                logger.LogWarning("Final batch save failed: {Msg}", ex.Message);
            }
        }

        logger.LogInformation("Seed complete — {Added} added, {Skipped} skipped.", added, skipped);
    }
}

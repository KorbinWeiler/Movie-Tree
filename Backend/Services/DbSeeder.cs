using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public static class DbSeeder
{
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
        if (existingIds.Count > 0)
        {
            logger.LogInformation("Movies table already has {Count} entries — skipping seed.", existingIds.Count);
            return;
        }

        logger.LogInformation("Seeding movies from '{Path}'...", filePath);

        var toAdd = new List<Movie>(500);
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

        logger.LogInformation("Seed complete — {Added} added, {Skipped} skipped.", added, skipped);
    }
}

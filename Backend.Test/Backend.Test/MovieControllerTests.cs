using Microsoft.AspNetCore.Mvc;

namespace Backend.Test;

public class MovieControllerTests
{
    [Fact]
    public async Task Search_Returns_Only_Visible_Matching_Movies()
    {
        await using var db = TestHelpers.CreateDbContext();

        var genre = new Genre { Id = 10, Name = "Sci-Fi" };
        var visibleMovie = new Movie { Id = 1, Title = "Star Quest", IsVisible = true };
        var hiddenMovie = new Movie { Id = 2, Title = "Star Secret", IsVisible = false };

        db.Genres.Add(genre);
        db.Movies.AddRange(visibleMovie, hiddenMovie);
        db.MovieGenres.AddRange(
            new MovieGenre { MovieId = 1, GenreId = 10 },
            new MovieGenre { MovieId = 2, GenreId = 10 }
        );
        await db.SaveChangesAsync();

        var controller = new MovieController(db);
        var action = await controller.Search("Star", null, 1, 20);

        var ok = Assert.IsType<OkObjectResult>(action);
        var payload = Assert.IsAssignableFrom<IEnumerable<MovieSummaryDto>>(ok.Value);
        var items = payload.ToList();

        Assert.Single(items);
        Assert.Equal(1, items[0].Id);
        Assert.Equal("Star Quest", items[0].Title);
    }

    [Fact]
    public async Task GetBatch_Preserves_Request_Order_And_Filters_Invalid_Duplicates_And_Hidden()
    {
        await using var db = TestHelpers.CreateDbContext();

        db.Movies.AddRange(
            new Movie { Id = 1, Title = "One", IsVisible = true },
            new Movie { Id = 2, Title = "Two", IsVisible = true },
            new Movie { Id = 3, Title = "Three", IsVisible = false }
        );
        await db.SaveChangesAsync();

        var controller = new MovieController(db);
        var action = await controller.GetBatch("2,1,2,3,abc");

        var ok = Assert.IsType<OkObjectResult>(action);
        var payload = Assert.IsAssignableFrom<IEnumerable<MovieSummaryDto>>(ok.Value);
        var ids = payload.Select(x => x.Id).ToList();

        Assert.Equal(new[] { 2, 1 }, ids);
    }
}

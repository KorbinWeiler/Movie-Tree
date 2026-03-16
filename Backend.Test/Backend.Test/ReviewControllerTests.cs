using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Backend.Test;

public class ReviewControllerTests
{
    private static ReviewController CreateController(AppDbContext db)
    {
        var embeddedService = new EmbeddedService("https://example.cognitiveservices.azure.com", "test-key");
        var searchService = new SearchService("example-service", "test-index", "test-key");
        return new ReviewController(db, embeddedService, searchService, NullLogger<ReviewController>.Instance);
    }

    [Fact]
    public async Task Create_Returns_BadRequest_When_Rating_Is_Out_Of_Range()
    {
        await using var db = TestHelpers.CreateDbContext();
        db.Movies.Add(new Movie { Id = 1, Title = "Movie", IsVisible = true });
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        TestHelpers.SetUser(controller, "user-1");

        var action = await controller.Create(new CreateReviewRequest(1, 0, "too low"));
        Assert.IsType<BadRequestObjectResult>(action);
    }

    [Fact]
    public async Task Create_Returns_Conflict_When_User_Already_Reviewed_Movie()
    {
        await using var db = TestHelpers.CreateDbContext();

        var user = new ApplicationUser { Id = "user-1", UserName = "alice", Email = "alice@example.com" };
        var movie = new Movie { Id = 1, Title = "Movie", IsVisible = true };

        db.Users.Add(user);
        db.Movies.Add(movie);
        db.Reviews.Add(new Review
        {
            UserId = "user-1",
            MovieId = 1,
            Rating = 8,
            ReviewText = "already reviewed",
            Visibility = ReviewVisibility.Public,
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        TestHelpers.SetUser(controller, "user-1");

        var action = await controller.Create(new CreateReviewRequest(1, 9, "new one"));
        Assert.IsType<ConflictObjectResult>(action);
    }

    [Fact]
    public async Task GetByUser_Returns_Only_Public_Reviews_For_NonOwner()
    {
        await using var db = TestHelpers.CreateDbContext();

        var user = new ApplicationUser { Id = "user-1", UserName = "alice", Email = "alice@example.com" };
        db.Users.Add(user);
        db.Movies.AddRange(
            new Movie { Id = 1, Title = "Public Movie", IsVisible = true },
            new Movie { Id = 2, Title = "Private Movie", IsVisible = true }
        );
        db.Reviews.AddRange(
            new Review { UserId = "user-1", MovieId = 1, Rating = 7, Visibility = ReviewVisibility.Public },
            new Review { UserId = "user-1", MovieId = 2, Rating = 9, Visibility = ReviewVisibility.Private }
        );
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };

        var action = await controller.GetByUser("user-1");

        var ok = Assert.IsType<OkObjectResult>(action);
        var payload = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(ok.Value);
        var items = payload.ToList();

        Assert.Single(items);
        Assert.Equal(ReviewVisibility.Public, items[0].Visibility);
    }

    [Fact]
    public async Task Create_PublicReview_Appears_In_PublicFeed()
    {
        await using var db = TestHelpers.CreateDbContext();

        db.Users.Add(new ApplicationUser { Id = "user-1", UserName = "alice", Email = "alice@example.com" });
        db.Movies.Add(new Movie { Id = 1, Title = "Movie", IsVisible = true });
        await db.SaveChangesAsync();

        var reviewController = CreateController(db);
        TestHelpers.SetUser(reviewController, "user-1");

        var createResult = await reviewController.Create(new CreateReviewRequest(1, 8, "Great movie!", ReviewVisibility.Public));
        Assert.IsType<CreatedAtActionResult>(createResult);

        var feedController = new FeedController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            }
        };

        var feedResult = await feedController.Public();
        var ok = Assert.IsType<OkObjectResult>(feedResult);
        var payload = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(ok.Value);
        var items = payload.ToList();

        Assert.Single(items);
        Assert.Equal(1, items[0].MovieId);
        Assert.Equal("Great movie!", items[0].ReviewText);
        Assert.Equal(ReviewVisibility.Public, items[0].Visibility);
    }
}

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
    public async Task GetByUser_Returns_Only_Public_Reviews_For_Anonymous_Viewer()
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
    public async Task GetByUser_Returns_Public_And_Friends_Reviews_For_Accepted_Friend()
    {
        await using var db = TestHelpers.CreateDbContext();

        db.Users.AddRange(
            new ApplicationUser { Id = "user-1", UserName = "alice", Email = "alice@example.com" },
            new ApplicationUser { Id = "user-2", UserName = "bob", Email = "bob@example.com" }
        );
        db.Movies.AddRange(
            new Movie { Id = 1, Title = "Public Movie", IsVisible = true },
            new Movie { Id = 2, Title = "Friends Movie", IsVisible = true },
            new Movie { Id = 3, Title = "Private Movie", IsVisible = true }
        );
        db.Friendships.Add(new Friendship { RequesterId = "user-1", AddresseeId = "user-2", Status = FriendshipStatus.Accepted });
        db.Reviews.AddRange(
            new Review { UserId = "user-1", MovieId = 1, Rating = 7, Visibility = ReviewVisibility.Public },
            new Review { UserId = "user-1", MovieId = 2, Rating = 8, Visibility = ReviewVisibility.Friends },
            new Review { UserId = "user-1", MovieId = 3, Rating = 9, Visibility = ReviewVisibility.Private }
        );
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        TestHelpers.SetUser(controller, "user-2");

        var action = await controller.GetByUser("user-1");

        var ok = Assert.IsType<OkObjectResult>(action);
        var payload = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(ok.Value);
        var items = payload.ToList();

        Assert.Equal(2, items.Count);
        Assert.Contains(items, item => item.Visibility == ReviewVisibility.Public);
        Assert.Contains(items, item => item.Visibility == ReviewVisibility.Friends);
        Assert.DoesNotContain(items, item => item.Visibility == ReviewVisibility.Private);
    }

    [Fact]
    public async Task GetById_Returns_NotFound_For_Friends_Review_To_NonFriend()
    {
        await using var db = TestHelpers.CreateDbContext();

        db.Users.AddRange(
            new ApplicationUser { Id = "user-1", UserName = "alice", Email = "alice@example.com" },
            new ApplicationUser { Id = "user-2", UserName = "bob", Email = "bob@example.com" }
        );
        db.Movies.Add(new Movie { Id = 1, Title = "Friends Movie", IsVisible = true });
        db.Reviews.Add(new Review { Id = 1, UserId = "user-1", MovieId = 1, Rating = 8, Visibility = ReviewVisibility.Friends });
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        TestHelpers.SetUser(controller, "user-2");

        var action = await controller.GetById(1);

        Assert.IsType<NotFoundResult>(action);
    }

    [Fact]
    public async Task GetForMovie_Returns_Friends_Reviews_For_Accepted_Friends_Only()
    {
        await using var db = TestHelpers.CreateDbContext();

        db.Users.AddRange(
            new ApplicationUser { Id = "user-1", UserName = "alice", Email = "alice@example.com" },
            new ApplicationUser { Id = "user-2", UserName = "bob", Email = "bob@example.com" },
            new ApplicationUser { Id = "user-3", UserName = "cara", Email = "cara@example.com" }
        );
        db.Movies.Add(new Movie { Id = 1, Title = "Shared Movie", IsVisible = true });
        db.Friendships.Add(new Friendship { RequesterId = "user-1", AddresseeId = "user-2", Status = FriendshipStatus.Accepted });
        db.Reviews.AddRange(
            new Review { UserId = "user-1", MovieId = 1, Rating = 7, ReviewText = "public", Visibility = ReviewVisibility.Public },
            new Review { UserId = "user-1", MovieId = 1, Rating = 8, ReviewText = "friends", Visibility = ReviewVisibility.Friends }
        );
        await db.SaveChangesAsync();

        var friendController = CreateController(db);
        TestHelpers.SetUser(friendController, "user-2");

        var friendAction = await friendController.GetForMovie(1);
        var friendOk = Assert.IsType<OkObjectResult>(friendAction);
        var friendPayload = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(friendOk.Value);
        var friendItems = friendPayload.ToList();

        Assert.Equal(2, friendItems.Count);

        var nonFriendController = CreateController(db);
        TestHelpers.SetUser(nonFriendController, "user-3");

        var nonFriendAction = await nonFriendController.GetForMovie(1);
        var nonFriendOk = Assert.IsType<OkObjectResult>(nonFriendAction);
        var nonFriendPayload = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(nonFriendOk.Value);
        var nonFriendItems = nonFriendPayload.ToList();

        Assert.Single(nonFriendItems);
        Assert.All(nonFriendItems, item => Assert.Equal(ReviewVisibility.Public, item.Visibility));
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

    [Fact]
    public async Task Public_Excludes_Current_Users_Reviews_When_Authenticated()
    {
        await using var db = TestHelpers.CreateDbContext();

        db.Users.AddRange(
            new ApplicationUser { Id = "user-1", UserName = "alice", Email = "alice@example.com" },
            new ApplicationUser { Id = "user-2", UserName = "bob", Email = "bob@example.com" }
        );
        db.Movies.AddRange(
            new Movie { Id = 1, Title = "Movie 1", IsVisible = true },
            new Movie { Id = 2, Title = "Movie 2", IsVisible = true }
        );
        db.Reviews.AddRange(
            new Review { UserId = "user-1", MovieId = 1, Rating = 7, ReviewText = "mine", Visibility = ReviewVisibility.Public },
            new Review { UserId = "user-2", MovieId = 2, Rating = 9, ReviewText = "theirs", Visibility = ReviewVisibility.Public }
        );
        await db.SaveChangesAsync();

        var feedController = new FeedController(db);
        TestHelpers.SetUser(feedController, "user-1");

        var feedResult = await feedController.Public();
        var ok = Assert.IsType<OkObjectResult>(feedResult);
        var payload = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(ok.Value);
        var items = payload.ToList();

        Assert.Single(items);
        Assert.Equal("user-2", items[0].UserId);
        Assert.Equal("theirs", items[0].ReviewText);
    }

    [Fact]
    public async Task Friends_Returns_Accepted_Friends_NonPrivate_Reviews_Only()
    {
        await using var db = TestHelpers.CreateDbContext();

        db.Users.AddRange(
            new ApplicationUser { Id = "user-1", UserName = "alice", Email = "alice@example.com" },
            new ApplicationUser { Id = "user-2", UserName = "bob", Email = "bob@example.com" },
            new ApplicationUser { Id = "user-3", UserName = "cara", Email = "cara@example.com" },
            new ApplicationUser { Id = "user-4", UserName = "drew", Email = "drew@example.com" }
        );
        db.Movies.AddRange(
            new Movie { Id = 1, Title = "Movie 1", IsVisible = true },
            new Movie { Id = 2, Title = "Movie 2", IsVisible = true },
            new Movie { Id = 3, Title = "Movie 3", IsVisible = true },
            new Movie { Id = 4, Title = "Movie 4", IsVisible = true },
            new Movie { Id = 5, Title = "Movie 5", IsVisible = true }
        );
        db.Friendships.AddRange(
            new Friendship { RequesterId = "user-1", AddresseeId = "user-2", Status = FriendshipStatus.Accepted },
            new Friendship { RequesterId = "user-1", AddresseeId = "user-3", Status = FriendshipStatus.Pending },
            new Friendship { RequesterId = "user-1", AddresseeId = "user-4", Status = FriendshipStatus.Accepted }
        );
        db.Reviews.AddRange(
            new Review { UserId = "user-1", MovieId = 1, Rating = 8, ReviewText = "self", Visibility = ReviewVisibility.Public },
            new Review { UserId = "user-2", MovieId = 2, Rating = 9, ReviewText = "friend public", Visibility = ReviewVisibility.Public },
            new Review { UserId = "user-2", MovieId = 3, Rating = 7, ReviewText = "friend friends", Visibility = ReviewVisibility.Friends },
            new Review { UserId = "user-3", MovieId = 4, Rating = 6, ReviewText = "pending friend", Visibility = ReviewVisibility.Public },
            new Review { UserId = "user-4", MovieId = 5, Rating = 10, ReviewText = "friend private", Visibility = ReviewVisibility.Private }
        );
        await db.SaveChangesAsync();

        var feedController = new FeedController(db);
        TestHelpers.SetUser(feedController, "user-1");

        var feedResult = await feedController.Friends();
        var ok = Assert.IsType<OkObjectResult>(feedResult);
        var payload = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(ok.Value);
        var items = payload.ToList();

        Assert.Equal(2, items.Count);
        Assert.All(items, item => Assert.Equal("user-2", item.UserId));
        Assert.DoesNotContain(items, item => item.ReviewText == "self");
        Assert.DoesNotContain(items, item => item.ReviewText == "pending friend");
        Assert.DoesNotContain(items, item => item.ReviewText == "friend private");
    }
}

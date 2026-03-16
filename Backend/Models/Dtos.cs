// ── Movie DTOs ──────────────────────────────────────────────────────────────

public record GenreDto(int Id, string Name);

public record MovieSummaryDto(
    int Id,
    string Title,
    string? PosterUrl,
    DateOnly? ReleaseDate,
    double? AverageRating,
    int ReviewCount,
    IEnumerable<GenreDto> Genres
);

public record MovieDetailDto(
    int Id,
    string Title,
    string? Description,
    DateOnly? ReleaseDate,
    int? RuntimeMinutes,
    string? PosterUrl,
    double? AverageRating,
    int ReviewCount,
    IEnumerable<GenreDto> Genres,
    bool IsVisible
);

// ── Review DTOs ─────────────────────────────────────────────────────────────

public record ReviewDto(
    int Id,
    string UserId,
    string UserName,
    string? UserAvatar,
    int MovieId,
    string MovieTitle,
    string? MoviePoster,
    byte Rating,
    string? ReviewText,
    ReviewVisibility Visibility,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateReviewRequest(
    int MovieId,
    byte Rating,
    string? ReviewText,
    ReviewVisibility Visibility = ReviewVisibility.Public
);

public record UpdateReviewRequest(
    byte? Rating,
    string? ReviewText,
    ReviewVisibility? Visibility
);

// ── Friend DTOs ─────────────────────────────────────────────────────────────

public record FriendRequestDto(string AddresseeId);

public record FriendshipDto(
    int Id,
    string UserId,
    string UserName,
    string? UserAvatar,
    FriendshipStatus Status,
    DateTime CreatedAt
);

// ── WatchLater DTOs ─────────────────────────────────────────────────────────

public record WatchLaterDto(
    int Id,
    MovieSummaryDto Movie,
    DateTime AddedAt
);

// ── Generate DTOs ────────────────────────────────────────────────────────────

public record PickedMovieDto(byte Position, MovieSummaryDto Movie);

public record RecommendRequest(List<int> MovieIds, int Count = 10);

public record RecommendResultDto(
    string Id,
    string Title,
    string? PosterUrl,
    string? Description,
    string[] Genres
);

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WatchLaterItem> WatchLaterItems => Set<WatchLaterItem>();
    public DbSet<Friendship> Friendships => Set<Friendship>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Movie.Id comes from TMDB — never auto-generate
        builder.Entity<Movie>()
            .Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Entity<Movie>()
            .Property(m => m.IsVisible)
            .HasDefaultValue(true);

        builder.Entity<ApplicationUser>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // MovieGenre composite PK
        builder.Entity<MovieGenre>()
            .HasKey(mg => new { mg.MovieId, mg.GenreId });

        builder.Entity<MovieGenre>()
            .HasOne(mg => mg.Movie)
            .WithMany(m => m.MovieGenres)
            .HasForeignKey(mg => mg.MovieId);

        builder.Entity<MovieGenre>()
            .HasOne(mg => mg.Genre)
            .WithMany(g => g.MovieGenres)
            .HasForeignKey(mg => mg.GenreId);

        // WatchLaterItem: one per user+movie
        builder.Entity<WatchLaterItem>()
            .HasIndex(w => new { w.UserId, w.MovieId })
            .IsUnique();

        builder.Entity<WatchLaterItem>()
            .Property(w => w.AddedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Review: one per user+movie; rating 1-10
        builder.Entity<Review>()
            .HasIndex(r => new { r.UserId, r.MovieId })
            .IsUnique();

        builder.Entity<Review>()
            .ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "[Rating] BETWEEN 1 AND 10"));

        builder.Entity<Review>()
            .Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Friendship: unique pair, no self-friending
        builder.Entity<Friendship>()
            .HasIndex(f => new { f.RequesterId, f.AddresseeId })
            .IsUnique();

        builder.Entity<Friendship>()
            .ToTable(t => t.HasCheckConstraint("CK_Friendship_NoSelf", "[RequesterId] <> [AddresseeId]"));

        builder.Entity<Friendship>()
            .HasOne(f => f.Requester)
            .WithMany(u => u.SentFriendRequests)
            .HasForeignKey(f => f.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Friendship>()
            .HasOne(f => f.Addressee)
            .WithMany(u => u.ReceivedFriendRequests)
            .HasForeignKey(f => f.AddresseeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Friendship>()
            .Property(f => f.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

    }
}

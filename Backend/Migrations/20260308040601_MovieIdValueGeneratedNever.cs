using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class MovieIdValueGeneratedNever : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK constraints that reference Movies.Id
            migrationBuilder.Sql("ALTER TABLE [MovieGenres] DROP CONSTRAINT [FK_MovieGenres_Movies_MovieId];");
            migrationBuilder.Sql("ALTER TABLE [Reviews] DROP CONSTRAINT [FK_Reviews_Movies_MovieId];");
            migrationBuilder.Sql("ALTER TABLE [WatchLaterItems] DROP CONSTRAINT [FK_WatchLaterItems_Movies_MovieId];");
            migrationBuilder.Sql("ALTER TABLE [AiPickListItems] DROP CONSTRAINT [FK_AiPickListItems_Movies_MovieId];");

            // Add plain (non-identity) replacement column, copy data, drop old identity column
            migrationBuilder.Sql("ALTER TABLE [Movies] ADD [Id_new] int NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("UPDATE [Movies] SET [Id_new] = [Id];");
            migrationBuilder.Sql("ALTER TABLE [Movies] DROP CONSTRAINT [PK_Movies];");
            migrationBuilder.Sql("ALTER TABLE [Movies] DROP COLUMN [Id];");
            migrationBuilder.Sql("EXEC sp_rename N'Movies.Id_new', N'Id', N'COLUMN';");
            migrationBuilder.Sql("ALTER TABLE [Movies] ADD CONSTRAINT [PK_Movies] PRIMARY KEY ([Id]);");

            // Recreate FK constraints
            migrationBuilder.Sql("ALTER TABLE [MovieGenres] ADD CONSTRAINT [FK_MovieGenres_Movies_MovieId] FOREIGN KEY ([MovieId]) REFERENCES [Movies] ([Id]) ON DELETE CASCADE;");
            migrationBuilder.Sql("ALTER TABLE [Reviews] ADD CONSTRAINT [FK_Reviews_Movies_MovieId] FOREIGN KEY ([MovieId]) REFERENCES [Movies] ([Id]) ON DELETE CASCADE;");
            migrationBuilder.Sql("ALTER TABLE [WatchLaterItems] ADD CONSTRAINT [FK_WatchLaterItems_Movies_MovieId] FOREIGN KEY ([MovieId]) REFERENCES [Movies] ([Id]) ON DELETE CASCADE;");
            migrationBuilder.Sql("ALTER TABLE [AiPickListItems] ADD CONSTRAINT [FK_AiPickListItems_Movies_MovieId] FOREIGN KEY ([MovieId]) REFERENCES [Movies] ([Id]) ON DELETE CASCADE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [MovieGenres] DROP CONSTRAINT [FK_MovieGenres_Movies_MovieId];");
            migrationBuilder.Sql("ALTER TABLE [Reviews] DROP CONSTRAINT [FK_Reviews_Movies_MovieId];");
            migrationBuilder.Sql("ALTER TABLE [WatchLaterItems] DROP CONSTRAINT [FK_WatchLaterItems_Movies_MovieId];");
            migrationBuilder.Sql("ALTER TABLE [AiPickListItems] DROP CONSTRAINT [FK_AiPickListItems_Movies_MovieId];");

            migrationBuilder.Sql("ALTER TABLE [Movies] ADD [Id_new] int NOT NULL IDENTITY(1,1);");
            migrationBuilder.Sql("ALTER TABLE [Movies] DROP CONSTRAINT [PK_Movies];");
            migrationBuilder.Sql("ALTER TABLE [Movies] DROP COLUMN [Id];");
            migrationBuilder.Sql("EXEC sp_rename N'Movies.Id_new', N'Id', N'COLUMN';");
            migrationBuilder.Sql("ALTER TABLE [Movies] ADD CONSTRAINT [PK_Movies] PRIMARY KEY ([Id]);");

            migrationBuilder.Sql("ALTER TABLE [MovieGenres] ADD CONSTRAINT [FK_MovieGenres_Movies_MovieId] FOREIGN KEY ([MovieId]) REFERENCES [Movies] ([Id]) ON DELETE CASCADE;");
            migrationBuilder.Sql("ALTER TABLE [Reviews] ADD CONSTRAINT [FK_Reviews_Movies_MovieId] FOREIGN KEY ([MovieId]) REFERENCES [Movies] ([Id]) ON DELETE CASCADE;");
            migrationBuilder.Sql("ALTER TABLE [WatchLaterItems] ADD CONSTRAINT [FK_WatchLaterItems_Movies_MovieId] FOREIGN KEY ([MovieId]) REFERENCES [Movies] ([Id]) ON DELETE CASCADE;");
            migrationBuilder.Sql("ALTER TABLE [AiPickListItems] ADD CONSTRAINT [FK_AiPickListItems_Movies_MovieId] FOREIGN KEY ([MovieId]) REFERENCES [Movies] ([Id]) ON DELETE CASCADE;");
        }
    }
}
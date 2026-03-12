using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class GenreIdValueGeneratedNever : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL Server cannot remove IDENTITY via ALTER COLUMN.
            // Recreate the table without IDENTITY on Id.
            migrationBuilder.Sql(@"
                -- Drop ALL FK constraints referencing Genres.Id
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovieGenres_Genres_GenreId')
                    ALTER TABLE [MovieGenres] DROP CONSTRAINT [FK_MovieGenres_Genres_GenreId];

                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AiPickList_Genres_GenreId')
                    ALTER TABLE [AiPickList] DROP CONSTRAINT [FK_AiPickList_Genres_GenreId];

                -- Rename old table and its PK (constraint names don't change with sp_rename)
                EXEC sp_rename 'Genres', 'Genres_Old';
                EXEC sp_rename N'dbo.PK_Genres', N'PK_Genres_Old', N'OBJECT';

                -- Create new table without IDENTITY
                CREATE TABLE [Genres] (
                    [Id]   int          NOT NULL,
                    [Name] nvarchar(max) NOT NULL,
                    CONSTRAINT [PK_Genres] PRIMARY KEY ([Id])
                );

                -- Copy data
                INSERT INTO [Genres] ([Id], [Name])
                SELECT [Id], [Name] FROM [Genres_Old];

                -- Drop old table
                DROP TABLE [Genres_Old];

                -- Restore FKs
                ALTER TABLE [MovieGenres] ADD CONSTRAINT [FK_MovieGenres_Genres_GenreId]
                    FOREIGN KEY ([GenreId]) REFERENCES [Genres]([Id]) ON DELETE CASCADE;

                ALTER TABLE [AiPickList] ADD CONSTRAINT [FK_AiPickList_Genres_GenreId]
                    FOREIGN KEY ([GenreId]) REFERENCES [Genres]([Id]);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore IDENTITY on Id
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MovieGenres_Genres_GenreId')
                    ALTER TABLE [MovieGenres] DROP CONSTRAINT [FK_MovieGenres_Genres_GenreId];

                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AiPickList_Genres_GenreId')
                    ALTER TABLE [AiPickList] DROP CONSTRAINT [FK_AiPickList_Genres_GenreId];

                EXEC sp_rename 'Genres', 'Genres_Old';
                EXEC sp_rename N'dbo.PK_Genres', N'PK_Genres_Old', N'OBJECT';

                CREATE TABLE [Genres] (
                    [Id]   int          NOT NULL IDENTITY(1,1),
                    [Name] nvarchar(max) NOT NULL,
                    CONSTRAINT [PK_Genres] PRIMARY KEY ([Id])
                );

                SET IDENTITY_INSERT [Genres] ON;
                INSERT INTO [Genres] ([Id], [Name])
                SELECT [Id], [Name] FROM [Genres_Old];
                SET IDENTITY_INSERT [Genres] OFF;

                DROP TABLE [Genres_Old];

                ALTER TABLE [MovieGenres] ADD CONSTRAINT [FK_MovieGenres_Genres_GenreId]
                    FOREIGN KEY ([GenreId]) REFERENCES [Genres]([Id]) ON DELETE CASCADE;

                ALTER TABLE [AiPickList] ADD CONSTRAINT [FK_AiPickList_Genres_GenreId]
                    FOREIGN KEY ([GenreId]) REFERENCES [Genres]([Id]);
            ");
        }
    }
}

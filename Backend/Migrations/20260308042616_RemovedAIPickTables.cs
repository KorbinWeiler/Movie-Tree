using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemovedAIPickTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiPickListItems_AiPickLists_AiPickListId",
                table: "AiPickListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AiPickListItems_Movies_MovieId",
                table: "AiPickListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_AiPickLists_AspNetUsers_UserId",
                table: "AiPickLists");

            migrationBuilder.DropForeignKey(
                name: "FK_AiPickLists_Genres_GenreId",
                table: "AiPickLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AiPickLists",
                table: "AiPickLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AiPickListItems",
                table: "AiPickListItems");

            migrationBuilder.DropIndex(
                name: "IX_AiPickListItems_AiPickListId_MovieId",
                table: "AiPickListItems");

            migrationBuilder.DropIndex(
                name: "IX_AiPickListItems_AiPickListId_Position",
                table: "AiPickListItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AiPickListItem_Position",
                table: "AiPickListItems");

            migrationBuilder.RenameTable(
                name: "AiPickLists",
                newName: "AiPickList");

            migrationBuilder.RenameTable(
                name: "AiPickListItems",
                newName: "AiPickListItem");

            migrationBuilder.RenameIndex(
                name: "IX_AiPickLists_UserId",
                table: "AiPickList",
                newName: "IX_AiPickList_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AiPickLists_GenreId",
                table: "AiPickList",
                newName: "IX_AiPickList_GenreId");

            migrationBuilder.RenameIndex(
                name: "IX_AiPickListItems_MovieId",
                table: "AiPickListItem",
                newName: "IX_AiPickListItem_MovieId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AiPickList",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AiPickList",
                table: "AiPickList",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AiPickListItem",
                table: "AiPickListItem",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AiPickListItem_AiPickListId",
                table: "AiPickListItem",
                column: "AiPickListId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiPickList_AspNetUsers_UserId",
                table: "AiPickList",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AiPickList_Genres_GenreId",
                table: "AiPickList",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AiPickListItem_AiPickList_AiPickListId",
                table: "AiPickListItem",
                column: "AiPickListId",
                principalTable: "AiPickList",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AiPickListItem_Movies_MovieId",
                table: "AiPickListItem",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiPickList_AspNetUsers_UserId",
                table: "AiPickList");

            migrationBuilder.DropForeignKey(
                name: "FK_AiPickList_Genres_GenreId",
                table: "AiPickList");

            migrationBuilder.DropForeignKey(
                name: "FK_AiPickListItem_AiPickList_AiPickListId",
                table: "AiPickListItem");

            migrationBuilder.DropForeignKey(
                name: "FK_AiPickListItem_Movies_MovieId",
                table: "AiPickListItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AiPickListItem",
                table: "AiPickListItem");

            migrationBuilder.DropIndex(
                name: "IX_AiPickListItem_AiPickListId",
                table: "AiPickListItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AiPickList",
                table: "AiPickList");

            migrationBuilder.RenameTable(
                name: "AiPickListItem",
                newName: "AiPickListItems");

            migrationBuilder.RenameTable(
                name: "AiPickList",
                newName: "AiPickLists");

            migrationBuilder.RenameIndex(
                name: "IX_AiPickListItem_MovieId",
                table: "AiPickListItems",
                newName: "IX_AiPickListItems_MovieId");

            migrationBuilder.RenameIndex(
                name: "IX_AiPickList_UserId",
                table: "AiPickLists",
                newName: "IX_AiPickLists_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AiPickList_GenreId",
                table: "AiPickLists",
                newName: "IX_AiPickLists_GenreId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AiPickLists",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AiPickListItems",
                table: "AiPickListItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AiPickLists",
                table: "AiPickLists",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AiPickListItems_AiPickListId_MovieId",
                table: "AiPickListItems",
                columns: new[] { "AiPickListId", "MovieId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiPickListItems_AiPickListId_Position",
                table: "AiPickListItems",
                columns: new[] { "AiPickListId", "Position" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_AiPickListItem_Position",
                table: "AiPickListItems",
                sql: "[Position] BETWEEN 1 AND 10");

            migrationBuilder.AddForeignKey(
                name: "FK_AiPickListItems_AiPickLists_AiPickListId",
                table: "AiPickListItems",
                column: "AiPickListId",
                principalTable: "AiPickLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AiPickListItems_Movies_MovieId",
                table: "AiPickListItems",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AiPickLists_AspNetUsers_UserId",
                table: "AiPickLists",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AiPickLists_Genres_GenreId",
                table: "AiPickLists",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id");
        }
    }
}

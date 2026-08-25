using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class AddUserAvatarTable : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
      name: "avatars",
      schema: "identity",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
        user_id = table.Column<Guid>(type: "uuid", nullable: false),
        is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
        created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        CreatedBy = table.Column<string>(type: "text", nullable: true),
        update_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        UpdatedBy = table.Column<string>(type: "text", nullable: true)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_avatars", x => x.Id);
        table.ForeignKey(
          name: "FK_avatars_users_user_id",
          column: x => x.user_id,
          principalSchema: "identity",
          principalTable: "users",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);
      });

    migrationBuilder.CreateIndex(
      name: "IX_avatars_user_id",
      schema: "identity",
      table: "avatars",
      column: "user_id",
      unique: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
      name: "avatars",
      schema: "identity");
  }
}

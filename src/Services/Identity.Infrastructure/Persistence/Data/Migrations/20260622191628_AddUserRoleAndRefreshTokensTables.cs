using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class AddUserRoleAndRefreshTokensTables : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
      name: "Roles",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        role_name = table.Column<string>(type: "text", nullable: false),
        role_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
        created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
        created_by = table.Column<string>(type: "text", nullable: false),
        updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
        updated_by = table.Column<string>(type: "text", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_Roles", x => x.Id);
      });

    migrationBuilder.CreateTable(
      name: "Users",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
        role_id = table.Column<Guid>(type: "uuid", nullable: false),
        is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
        first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
        last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
        password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
        created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
        created_by = table.Column<string>(type: "text", nullable: false, defaultValue: "user"),
        updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
        updated_by = table.Column<string>(type: "text", nullable: false, defaultValue: "user")
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_Users", x => x.Id);
        table.ForeignKey(
          name: "FK_Users_Roles_role_id",
          column: x => x.role_id,
          principalTable: "Roles",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);
      });

    migrationBuilder.CreateTable(
      name: "RefreshTokens",
      columns: table => new
      {
        Id = table.Column<Guid>(type: "uuid", nullable: false),
        user_id = table.Column<Guid>(type: "uuid", nullable: false),
        expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        is_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
        RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
        token_value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_RefreshTokens", x => x.Id);
        table.ForeignKey(
          name: "FK_RefreshTokens_Users_user_id",
          column: x => x.user_id,
          principalTable: "Users",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);
      });

    migrationBuilder.CreateIndex(
      name: "IX_RefreshTokens_token_value",
      table: "RefreshTokens",
      column: "token_value");

    migrationBuilder.CreateIndex(
      name: "IX_RefreshTokens_user_id",
      table: "RefreshTokens",
      column: "user_id");

    migrationBuilder.CreateIndex(
      name: "IX_Roles_role_name",
      table: "Roles",
      column: "role_name",
      unique: true);

    migrationBuilder.CreateIndex(
      name: "IX_Users_email",
      table: "Users",
      column: "email",
      unique: true);

    migrationBuilder.CreateIndex(
      name: "IX_Users_role_id",
      table: "Users",
      column: "role_id");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
      name: "RefreshTokens");

    migrationBuilder.DropTable(
      name: "Users");

    migrationBuilder.DropTable(
      name: "Roles");
  }
}

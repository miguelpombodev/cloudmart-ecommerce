using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class AlterUsersRolesAndRefreshTokensTablesNames : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropForeignKey(
      name: "FK_RefreshTokens_Users_user_id",
      table: "RefreshTokens");

    migrationBuilder.DropForeignKey(
      name: "FK_Users_Roles_role_id",
      table: "Users");

    migrationBuilder.DropPrimaryKey(
      name: "PK_Users",
      table: "Users");

    migrationBuilder.DropPrimaryKey(
      name: "PK_Roles",
      table: "Roles");

    migrationBuilder.DropPrimaryKey(
      name: "PK_RefreshTokens",
      table: "RefreshTokens");

    migrationBuilder.EnsureSchema(
      name: "identity");

    migrationBuilder.RenameTable(
      name: "Users",
      newName: "users",
      newSchema: "identity");

    migrationBuilder.RenameTable(
      name: "Roles",
      newName: "roles",
      newSchema: "identity");

    migrationBuilder.RenameTable(
      name: "RefreshTokens",
      newName: "refresh_tokens",
      newSchema: "identity");

    migrationBuilder.RenameIndex(
      name: "IX_Users_role_id",
      schema: "identity",
      table: "users",
      newName: "IX_users_role_id");

    migrationBuilder.RenameIndex(
      name: "IX_Users_email",
      schema: "identity",
      table: "users",
      newName: "IX_users_email");

    migrationBuilder.RenameIndex(
      name: "IX_Roles_role_name",
      schema: "identity",
      table: "roles",
      newName: "IX_roles_role_name");

    migrationBuilder.RenameIndex(
      name: "IX_RefreshTokens_user_id",
      schema: "identity",
      table: "refresh_tokens",
      newName: "IX_refresh_tokens_user_id");

    migrationBuilder.RenameIndex(
      name: "IX_RefreshTokens_token_value",
      schema: "identity",
      table: "refresh_tokens",
      newName: "IX_refresh_tokens_token_value");

    migrationBuilder.AddPrimaryKey(
      name: "PK_users",
      schema: "identity",
      table: "users",
      column: "Id");

    migrationBuilder.AddPrimaryKey(
      name: "PK_roles",
      schema: "identity",
      table: "roles",
      column: "Id");

    migrationBuilder.AddPrimaryKey(
      name: "PK_refresh_tokens",
      schema: "identity",
      table: "refresh_tokens",
      column: "Id");

    migrationBuilder.AddForeignKey(
      name: "FK_refresh_tokens_users_user_id",
      schema: "identity",
      table: "refresh_tokens",
      column: "user_id",
      principalSchema: "identity",
      principalTable: "users",
      principalColumn: "Id",
      onDelete: ReferentialAction.Cascade);

    migrationBuilder.AddForeignKey(
      name: "FK_users_roles_role_id",
      schema: "identity",
      table: "users",
      column: "role_id",
      principalSchema: "identity",
      principalTable: "roles",
      principalColumn: "Id",
      onDelete: ReferentialAction.Cascade);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropForeignKey(
      name: "FK_refresh_tokens_users_user_id",
      schema: "identity",
      table: "refresh_tokens");

    migrationBuilder.DropForeignKey(
      name: "FK_users_roles_role_id",
      schema: "identity",
      table: "users");

    migrationBuilder.DropPrimaryKey(
      name: "PK_users",
      schema: "identity",
      table: "users");

    migrationBuilder.DropPrimaryKey(
      name: "PK_roles",
      schema: "identity",
      table: "roles");

    migrationBuilder.DropPrimaryKey(
      name: "PK_refresh_tokens",
      schema: "identity",
      table: "refresh_tokens");

    migrationBuilder.RenameTable(
      name: "users",
      schema: "identity",
      newName: "Users");

    migrationBuilder.RenameTable(
      name: "roles",
      schema: "identity",
      newName: "Roles");

    migrationBuilder.RenameTable(
      name: "refresh_tokens",
      schema: "identity",
      newName: "RefreshTokens");

    migrationBuilder.RenameIndex(
      name: "IX_users_role_id",
      table: "Users",
      newName: "IX_Users_role_id");

    migrationBuilder.RenameIndex(
      name: "IX_users_email",
      table: "Users",
      newName: "IX_Users_email");

    migrationBuilder.RenameIndex(
      name: "IX_roles_role_name",
      table: "Roles",
      newName: "IX_Roles_role_name");

    migrationBuilder.RenameIndex(
      name: "IX_refresh_tokens_user_id",
      table: "RefreshTokens",
      newName: "IX_RefreshTokens_user_id");

    migrationBuilder.RenameIndex(
      name: "IX_refresh_tokens_token_value",
      table: "RefreshTokens",
      newName: "IX_RefreshTokens_token_value");

    migrationBuilder.AddPrimaryKey(
      name: "PK_Users",
      table: "Users",
      column: "Id");

    migrationBuilder.AddPrimaryKey(
      name: "PK_Roles",
      table: "Roles",
      column: "Id");

    migrationBuilder.AddPrimaryKey(
      name: "PK_RefreshTokens",
      table: "RefreshTokens",
      column: "Id");

    migrationBuilder.AddForeignKey(
      name: "FK_RefreshTokens_Users_user_id",
      table: "RefreshTokens",
      column: "user_id",
      principalTable: "Users",
      principalColumn: "Id",
      onDelete: ReferentialAction.Cascade);

    migrationBuilder.AddForeignKey(
      name: "FK_Users_Roles_role_id",
      table: "Users",
      column: "role_id",
      principalTable: "Roles",
      principalColumn: "Id",
      onDelete: ReferentialAction.Cascade);
  }
}

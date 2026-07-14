using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class AddUserAuthenticationProvidersTable : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AlterColumn<string>(
      name: "password_hash",
      schema: "identity",
      table: "users",
      type: "character varying(500)",
      maxLength: 500,
      nullable: true,
      oldClrType: typeof(string),
      oldType: "character varying(500)",
      oldMaxLength: 500);

    migrationBuilder.CreateTable(
      name: "user_oauth_providers",
      columns: table => new
      {
        id = table.Column<Guid>(type: "uuid", nullable: false),
        user_id = table.Column<Guid>(type: "uuid", nullable: false),
        VARCHAR20 = table.Column<string>(name: "VARCHAR(20)", type: "text", nullable: false),
        provider_user_id = table.Column<string>(type: "text", nullable: false),
        CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
        CreatedBy = table.Column<string>(type: "text", nullable: true),
        UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
        UpdatedBy = table.Column<string>(type: "text", nullable: true)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_user_oauth_providers", x => x.id);
        table.ForeignKey(
          name: "FK_user_oauth_providers_users_user_id",
          column: x => x.user_id,
          principalSchema: "identity",
          principalTable: "users",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);
      });

    migrationBuilder.CreateIndex(
      name: "IX_user_oauth_providers_user_id",
      table: "user_oauth_providers",
      column: "user_id");

    migrationBuilder.CreateIndex(
      name: "IX_UserAuthenticationProvider_ProviderName",
      table: "user_oauth_providers",
      column: "VARCHAR(20)",
      unique: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
      name: "user_oauth_providers");

    migrationBuilder.AlterColumn<string>(
      name: "password_hash",
      schema: "identity",
      table: "users",
      type: "character varying(500)",
      maxLength: 500,
      nullable: false,
      defaultValue: "",
      oldClrType: typeof(string),
      oldType: "character varying(500)",
      oldMaxLength: 500,
      oldNullable: true);
  }
}

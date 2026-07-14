using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class AddEmailColumnIntoUserAuthenticationProvidersTable : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<string>(
      name: "email",
      table: "user_oauth_providers",
      type: "character varying(320)",
      maxLength: 320,
      nullable: false,
      defaultValue: "");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
      name: "email",
      table: "user_oauth_providers");
  }
}

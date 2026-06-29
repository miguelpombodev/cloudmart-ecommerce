using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class AddRoleNameInEntity : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AlterColumn<string>(
      name: "role_name",
      table: "Roles",
      type: "character varying(30)",
      maxLength: 30,
      nullable: false,
      oldClrType: typeof(string),
      oldType: "text");

    migrationBuilder.AddColumn<string>(
      name: "role_type",
      table: "Roles",
      type: "text",
      nullable: false,
      defaultValue: "");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
      name: "role_type",
      table: "Roles");

    migrationBuilder.AlterColumn<string>(
      name: "role_name",
      table: "Roles",
      type: "text",
      nullable: false,
      oldClrType: typeof(string),
      oldType: "character varying(30)",
      oldMaxLength: 30);
  }
}

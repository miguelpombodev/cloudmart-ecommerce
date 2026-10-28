using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class AddImageUrlToBrandTable : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<string>(
      name: "brand_image_url",
      schema: "catalog",
      table: "brands",
      type: "character varying(2048)",
      maxLength: 2048,
      nullable: false,
      defaultValue: "");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
      name: "brand_image_url",
      schema: "catalog",
      table: "brands");
  }
}

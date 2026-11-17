using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class DropSlugAndAddImageForCategory : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
      name: "Slug",
      schema: "catalog",
      table: "categories");

    migrationBuilder.AddColumn<string>(
      name: "category_image_alt_text",
      schema: "catalog",
      table: "categories",
      type: "character varying(500)",
      maxLength: 500,
      nullable: false,
      defaultValue: "");

    migrationBuilder.AddColumn<bool>(
      name: "category_image_is_primary",
      schema: "catalog",
      table: "categories",
      type: "boolean",
      nullable: false,
      defaultValue: false);

    migrationBuilder.AddColumn<string>(
      name: "category_image_url",
      schema: "catalog",
      table: "categories",
      type: "character varying(2048)",
      maxLength: 2048,
      nullable: false,
      defaultValue: "");

    migrationBuilder.CreateIndex(
      name: "IX_categories_category_image_url",
      schema: "catalog",
      table: "categories",
      column: "category_image_url");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropIndex(
      name: "IX_categories_category_image_url",
      schema: "catalog",
      table: "categories");

    migrationBuilder.DropColumn(
      name: "category_image_alt_text",
      schema: "catalog",
      table: "categories");

    migrationBuilder.DropColumn(
      name: "category_image_is_primary",
      schema: "catalog",
      table: "categories");

    migrationBuilder.DropColumn(
      name: "category_image_url",
      schema: "catalog",
      table: "categories");

    migrationBuilder.AddColumn<string>(
      name: "Slug",
      schema: "catalog",
      table: "categories",
      type: "text",
      nullable: false,
      defaultValue: "");
  }
}

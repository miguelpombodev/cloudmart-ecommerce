using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class AlterNameColumnsInProductsAndChangeNumericAmountForRating : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropForeignKey(
      name: "FK_products_brands_BrandId",
      schema: "catalog",
      table: "products");

    migrationBuilder.RenameColumn(
      name: "StockQuantity",
      schema: "catalog",
      table: "products",
      newName: "product_stock_quantity");

    migrationBuilder.RenameColumn(
      name: "BrandId",
      schema: "catalog",
      table: "products",
      newName: "product_brand_id");

    migrationBuilder.RenameIndex(
      name: "IX_products_BrandId",
      schema: "catalog",
      table: "products",
      newName: "IX_products_product_brand_id");

    migrationBuilder.AlterColumn<int>(
      name: "product_stock_quantity",
      schema: "catalog",
      table: "products",
      type: "INTEGER",
      nullable: false,
      oldClrType: typeof(int),
      oldType: "integer");

    migrationBuilder.AlterColumn<decimal>(
      name: "review_rating",
      schema: "catalog",
      table: "product_review",
      type: "numeric(2,1)",
      nullable: false,
      oldClrType: typeof(decimal),
      oldType: "numeric(1,1)");

    migrationBuilder.AddColumn<string>(
      name: "Slug",
      schema: "catalog",
      table: "categories",
      type: "text",
      nullable: false,
      defaultValue: "");

    migrationBuilder.AddForeignKey(
      name: "FK_products_brands_product_brand_id",
      schema: "catalog",
      table: "products",
      column: "product_brand_id",
      principalSchema: "catalog",
      principalTable: "brands",
      principalColumn: "id",
      onDelete: ReferentialAction.Cascade);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropForeignKey(
      name: "FK_products_brands_product_brand_id",
      schema: "catalog",
      table: "products");

    migrationBuilder.DropColumn(
      name: "Slug",
      schema: "catalog",
      table: "categories");

    migrationBuilder.RenameColumn(
      name: "product_stock_quantity",
      schema: "catalog",
      table: "products",
      newName: "StockQuantity");

    migrationBuilder.RenameColumn(
      name: "product_brand_id",
      schema: "catalog",
      table: "products",
      newName: "BrandId");

    migrationBuilder.RenameIndex(
      name: "IX_products_product_brand_id",
      schema: "catalog",
      table: "products",
      newName: "IX_products_BrandId");

    migrationBuilder.AlterColumn<int>(
      name: "StockQuantity",
      schema: "catalog",
      table: "products",
      type: "integer",
      nullable: false,
      oldClrType: typeof(int),
      oldType: "INTEGER");

    migrationBuilder.AlterColumn<decimal>(
      name: "review_rating",
      schema: "catalog",
      table: "product_review",
      type: "numeric(1,1)",
      nullable: false,
      oldClrType: typeof(decimal),
      oldType: "numeric(2,1)");

    migrationBuilder.AddForeignKey(
      name: "FK_products_brands_BrandId",
      schema: "catalog",
      table: "products",
      column: "BrandId",
      principalSchema: "catalog",
      principalTable: "brands",
      principalColumn: "id",
      onDelete: ReferentialAction.Cascade);
  }
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class AddProductProductReviewBrandAndCategoryTables : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.EnsureSchema(
      name: "catalog");

    migrationBuilder.CreateTable(
      name: "brands",
      schema: "catalog",
      columns: table => new
      {
        id = table.Column<Guid>(type: "UUID", nullable: false),
        brand_title = table.Column<string>(type: "VARCHAR(100)", nullable: false),
        created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        created_by = table.Column<string>(type: "text", nullable: false),
        updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        updated_by = table.Column<string>(type: "text", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_brands", x => x.id);
      });

    migrationBuilder.CreateTable(
      name: "categories",
      schema: "catalog",
      columns: table => new
      {
        id = table.Column<Guid>(type: "UUID", nullable: false),
        category_title = table.Column<string>(type: "VARCHAR(100)", nullable: false),
        created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        created_by = table.Column<string>(type: "text", nullable: false),
        updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        updated_by = table.Column<string>(type: "text", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_categories", x => x.id);
      });

    migrationBuilder.CreateTable(
      name: "product_review",
      schema: "catalog",
      columns: table => new
      {
        id = table.Column<Guid>(type: "UUID", nullable: false),
        review_product_id = table.Column<Guid>(type: "UUID", nullable: false),
        review_rating = table.Column<decimal>(type: "numeric(1,1)", nullable: false),
        review_contents = table.Column<string[]>(type: "text[]", nullable: false),
        review_description = table.Column<string>(type: "VARCHAR(800)", nullable: false),
        review_user_id = table.Column<Guid>(type: "UUID", nullable: false),
        created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_product_review", x => x.id);
      });

    migrationBuilder.CreateTable(
      name: "products",
      schema: "catalog",
      columns: table => new
      {
        id = table.Column<Guid>(type: "UUID", nullable: false),
        product_title = table.Column<string>(type: "VARCHAR(100)", nullable: false),
        product_sku = table.Column<string>(type: "VARCHAR(12)", nullable: false),
        product_description = table.Column<string>(type: "VARCHAR(1200)", nullable: false),
        product_tags = table.Column<List<string>>(type: "text[]", nullable: false),
        product_category_id = table.Column<Guid>(type: "UUID", nullable: false),
        BrandId = table.Column<Guid>(type: "UUID", nullable: false),
        StockQuantity = table.Column<int>(type: "integer", nullable: false),
        is_active = table.Column<bool>(type: "BOOLEAN", nullable: false),
        price_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
        price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
        created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        created_by = table.Column<string>(type: "text", nullable: false),
        updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
        updated_by = table.Column<string>(type: "text", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_products", x => x.id);
        table.ForeignKey(
          name: "FK_products_brands_BrandId",
          column: x => x.BrandId,
          principalSchema: "catalog",
          principalTable: "brands",
          principalColumn: "id",
          onDelete: ReferentialAction.Cascade);
        table.ForeignKey(
          name: "FK_products_categories_product_category_id",
          column: x => x.product_category_id,
          principalSchema: "catalog",
          principalTable: "categories",
          principalColumn: "id",
          onDelete: ReferentialAction.Cascade);
      });

    migrationBuilder.CreateTable(
      name: "product_images",
      schema: "catalog",
      columns: table => new
      {
        product_id = table.Column<Guid>(type: "UUID", nullable: false),
        Id = table.Column<int>(type: "integer", nullable: false)
          .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
        url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
        alt_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
        is_primary = table.Column<bool>(type: "boolean", nullable: false)
      },
      constraints: table =>
      {
        table.PrimaryKey("PK_product_images", x => new { x.product_id, x.Id });
        table.ForeignKey(
          name: "FK_product_images_products_product_id",
          column: x => x.product_id,
          principalSchema: "catalog",
          principalTable: "products",
          principalColumn: "id",
          onDelete: ReferentialAction.Cascade);
      });

    migrationBuilder.CreateIndex(
      name: "IX_brands_brand_title",
      schema: "catalog",
      table: "brands",
      column: "brand_title",
      unique: true);

    migrationBuilder.CreateIndex(
      name: "IX_brands_id",
      schema: "catalog",
      table: "brands",
      column: "id");

    migrationBuilder.CreateIndex(
      name: "IX_categories_category_title",
      schema: "catalog",
      table: "categories",
      column: "category_title",
      unique: true);

    migrationBuilder.CreateIndex(
      name: "IX_product_review_review_rating",
      schema: "catalog",
      table: "product_review",
      column: "review_rating",
      descending: new bool[0]);

    migrationBuilder.CreateIndex(
      name: "IX_product_review_review_user_id_review_product_id",
      schema: "catalog",
      table: "product_review",
      columns: new[] { "review_user_id", "review_product_id" });

    migrationBuilder.CreateIndex(
      name: "IX_products_BrandId",
      schema: "catalog",
      table: "products",
      column: "BrandId");

    migrationBuilder.CreateIndex(
      name: "IX_products_product_category_id",
      schema: "catalog",
      table: "products",
      column: "product_category_id");

    migrationBuilder.CreateIndex(
      name: "IX_products_product_sku",
      schema: "catalog",
      table: "products",
      column: "product_sku",
      unique: true);

    migrationBuilder.CreateIndex(
      name: "IX_products_product_title",
      schema: "catalog",
      table: "products",
      column: "product_title",
      unique: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
      name: "product_images",
      schema: "catalog");

    migrationBuilder.DropTable(
      name: "product_review",
      schema: "catalog");

    migrationBuilder.DropTable(
      name: "products",
      schema: "catalog");

    migrationBuilder.DropTable(
      name: "brands",
      schema: "catalog");

    migrationBuilder.DropTable(
      name: "categories",
      schema: "catalog");
  }
}

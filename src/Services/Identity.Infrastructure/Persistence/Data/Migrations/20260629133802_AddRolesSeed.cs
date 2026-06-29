using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Identity.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class AddRolesSeed : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.InsertData(
      schema: "identity",
      table: "roles",
      columns: new[] { "Id", "created_at", "created_by", "role_description", "role_name", "role_type", "updated_at", "updated_by" },
      values: new object[,]
      {
        { new Guid("1629dfbc-df35-4aed-a071-08755df25354"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", "Admin role", "Admin", "Admin", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system" },
        { new Guid("19804ec2-fe97-4a75-b084-035eb84fdeb5"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", "Member role", "Member", "Member", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system" },
        { new Guid("37d43ef9-6739-40d9-b876-6ff9e92e68d4"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", "Manager role", "Manager", "Manager", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system" },
        { new Guid("b97dd358-e092-43c4-91c4-f34f2b04927c"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", "Support role", "Support", "Support", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system" },
        { new Guid("d7d42ee4-3899-4630-9ecc-6eccc8af04a7"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", "Seller role", "Seller", "Seller", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system" },
        { new Guid("f8f4e2b8-7351-4e7f-a5b0-69f8f66940e4"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system", "Customer role", "Customer", "Customer", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "system" }
      });
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DeleteData(
      schema: "identity",
      table: "roles",
      keyColumn: "Id",
      keyValue: new Guid("1629dfbc-df35-4aed-a071-08755df25354"));

    migrationBuilder.DeleteData(
      schema: "identity",
      table: "roles",
      keyColumn: "Id",
      keyValue: new Guid("19804ec2-fe97-4a75-b084-035eb84fdeb5"));

    migrationBuilder.DeleteData(
      schema: "identity",
      table: "roles",
      keyColumn: "Id",
      keyValue: new Guid("37d43ef9-6739-40d9-b876-6ff9e92e68d4"));

    migrationBuilder.DeleteData(
      schema: "identity",
      table: "roles",
      keyColumn: "Id",
      keyValue: new Guid("b97dd358-e092-43c4-91c4-f34f2b04927c"));

    migrationBuilder.DeleteData(
      schema: "identity",
      table: "roles",
      keyColumn: "Id",
      keyValue: new Guid("d7d42ee4-3899-4630-9ecc-6eccc8af04a7"));

    migrationBuilder.DeleteData(
      schema: "identity",
      table: "roles",
      keyColumn: "Id",
      keyValue: new Guid("f8f4e2b8-7351-4e7f-a5b0-69f8f66940e4"));
  }
}

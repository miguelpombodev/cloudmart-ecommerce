using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class AlterProviderNameAndTimestampsColumnNames : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.RenameColumn(
      name: "VARCHAR(20)",
      table: "user_oauth_providers",
      newName: "provider_name");

    migrationBuilder.RenameColumn(
      name: "UpdatedBy",
      table: "user_oauth_providers",
      newName: "updated_by");

    migrationBuilder.RenameColumn(
      name: "UpdatedAt",
      table: "user_oauth_providers",
      newName: "updated_at");

    migrationBuilder.RenameColumn(
      name: "CreatedBy",
      table: "user_oauth_providers",
      newName: "created_by");

    migrationBuilder.RenameColumn(
      name: "CreatedAt",
      table: "user_oauth_providers",
      newName: "created_at");

    migrationBuilder.AlterColumn<string>(
      name: "provider_name",
      table: "user_oauth_providers",
      type: "VARCHAR(20)",
      nullable: false,
      oldClrType: typeof(string),
      oldType: "text");

    migrationBuilder.AlterColumn<string>(
      name: "updated_by",
      table: "user_oauth_providers",
      type: "text",
      nullable: false,
      defaultValue: "user",
      oldClrType: typeof(string),
      oldType: "text",
      oldNullable: true);

    migrationBuilder.AlterColumn<DateTimeOffset>(
      name: "updated_at",
      table: "user_oauth_providers",
      type: "timestamp with time zone",
      nullable: false,
      defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
      oldClrType: typeof(DateTimeOffset),
      oldType: "timestamp with time zone",
      oldNullable: true);

    migrationBuilder.AlterColumn<string>(
      name: "created_by",
      table: "user_oauth_providers",
      type: "text",
      nullable: false,
      defaultValue: "user",
      oldClrType: typeof(string),
      oldType: "text",
      oldNullable: true);

    migrationBuilder.AlterColumn<DateTimeOffset>(
      name: "created_at",
      table: "user_oauth_providers",
      type: "timestamp with time zone",
      nullable: false,
      defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
      oldClrType: typeof(DateTimeOffset),
      oldType: "timestamp with time zone",
      oldNullable: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.RenameColumn(
      name: "updated_by",
      table: "user_oauth_providers",
      newName: "UpdatedBy");

    migrationBuilder.RenameColumn(
      name: "updated_at",
      table: "user_oauth_providers",
      newName: "UpdatedAt");

    migrationBuilder.RenameColumn(
      name: "provider_name",
      table: "user_oauth_providers",
      newName: "VARCHAR(20)");

    migrationBuilder.RenameColumn(
      name: "created_by",
      table: "user_oauth_providers",
      newName: "CreatedBy");

    migrationBuilder.RenameColumn(
      name: "created_at",
      table: "user_oauth_providers",
      newName: "CreatedAt");

    migrationBuilder.AlterColumn<string>(
      name: "UpdatedBy",
      table: "user_oauth_providers",
      type: "text",
      nullable: true,
      oldClrType: typeof(string),
      oldType: "text",
      oldDefaultValue: "user");

    migrationBuilder.AlterColumn<DateTimeOffset>(
      name: "UpdatedAt",
      table: "user_oauth_providers",
      type: "timestamp with time zone",
      nullable: true,
      oldClrType: typeof(DateTimeOffset),
      oldType: "timestamp with time zone");

    migrationBuilder.AlterColumn<string>(
      name: "VARCHAR(20)",
      table: "user_oauth_providers",
      type: "text",
      nullable: false,
      oldClrType: typeof(string),
      oldType: "VARCHAR(20)");

    migrationBuilder.AlterColumn<string>(
      name: "CreatedBy",
      table: "user_oauth_providers",
      type: "text",
      nullable: true,
      oldClrType: typeof(string),
      oldType: "text",
      oldDefaultValue: "user");

    migrationBuilder.AlterColumn<DateTimeOffset>(
      name: "CreatedAt",
      table: "user_oauth_providers",
      type: "timestamp with time zone",
      nullable: true,
      oldClrType: typeof(DateTimeOffset),
      oldType: "timestamp with time zone");
  }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Data.Migrations;

/// <inheritdoc />
public partial class AvatarImageNameAndAvatarContentTypeColumnsInUserAvatarTable : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<string>(
      name: "avatar_content_type",
      schema: "identity",
      table: "avatars",
      type: "character varying(100)",
      maxLength: 100,
      nullable: false,
      defaultValue: "");

    migrationBuilder.AddColumn<string>(
      name: "avatar_image_name",
      schema: "identity",
      table: "avatars",
      type: "character varying(150)",
      maxLength: 150,
      nullable: false,
      defaultValue: "");
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
      name: "avatar_content_type",
      schema: "identity",
      table: "avatars");

    migrationBuilder.DropColumn(
      name: "avatar_image_name",
      schema: "identity",
      table: "avatars");
  }
}

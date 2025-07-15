using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ApiKeyEnabled",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApiKeyGeneratedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApiKeyLastUsedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                columns: new[] { "ApiKey", "ApiKeyEnabled", "ApiKeyGeneratedAt", "ApiKeyLastUsedAt", "PasswordHash" },
                values: new object[] { null, true, null, null, "AQAAAAIAAYagAAAAEI3r0ttRj6VjVIAlER6AY2o2tdhPKeNLjXvh7JpCpmYIbtwR+5tWk73EWSkbq0oHcQ==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApiKeyEnabled",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApiKeyGeneratedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApiKeyLastUsedAt",
                table: "AspNetUsers");

            // Admin user password reset (only password)
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEFCKAnQwD9LG8JEi6fw2INwOK/bIIhJHOQUILhEK8I+z3CuCruLwKe5go1i7n+B2cw==");
        }
    }
}
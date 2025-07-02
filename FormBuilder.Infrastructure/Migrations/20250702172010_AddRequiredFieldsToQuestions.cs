using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormBuilder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiredFieldsToQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CustomCheckbox1Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomCheckbox2Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomCheckbox3Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomCheckbox4Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomInt1Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomInt2Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomInt3Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomInt4Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomString1Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomString2Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomString3Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomString4Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomText1Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomText2Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomText3Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CustomText4Required",
                table: "Templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEDr2Vsu4PgZrSlwjgU1xSFFsbT7QwYj4uAyFRkU0QnQ2rBwqNtoeZjwJtZTYQuUfUQ==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomCheckbox1Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomCheckbox2Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomCheckbox3Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomCheckbox4Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomInt1Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomInt2Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomInt3Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomInt4Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomString1Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomString2Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomString3Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomString4Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomText1Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomText2Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomText3Required",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CustomText4Required",
                table: "Templates");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "admin-user-id",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAECsIkzteEzSY+3G/HqGkrqQEDRduM2/yKK082R9Tt5iW4tqG6ctSgJefy/ZY6Ytdmw==");
        }
    }
}

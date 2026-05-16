using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DamiFYP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixBloodType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "DonationRequest");

            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "DonationPosts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "BloodTypes");

            migrationBuilder.AddColumn<int>(
                name: "BloodTypeName",
                table: "DonationRequest",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BloodTypeName",
                table: "DonationPosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BloodTypeName",
                table: "BloodTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloodTypeName",
                table: "DonationRequest");

            migrationBuilder.DropColumn(
                name: "BloodTypeName",
                table: "DonationPosts");

            migrationBuilder.DropColumn(
                name: "BloodTypeName",
                table: "BloodTypes");

            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "DonationRequest",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "DonationPosts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "BloodTypes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

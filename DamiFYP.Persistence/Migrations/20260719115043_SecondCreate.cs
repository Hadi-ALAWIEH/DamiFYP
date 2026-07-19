using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DamiFYP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SecondCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloodTypes_DamiUser_UserId",
                table: "BloodTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_DonationRequest_DamiUser_UserId",
                table: "DonationRequest");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DonationPosts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ConversationParticipants");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "DonationRequest",
                newName: "DamiUserId");

            migrationBuilder.RenameIndex(
                name: "IX_DonationRequest_UserId",
                table: "DonationRequest",
                newName: "IX_DonationRequest_DamiUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "BloodTypes",
                newName: "DamiUserId");

            migrationBuilder.RenameIndex(
                name: "IX_BloodTypes_UserId",
                table: "BloodTypes",
                newName: "IX_BloodTypes_DamiUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BloodTypes_DamiUser_DamiUserId",
                table: "BloodTypes",
                column: "DamiUserId",
                principalTable: "DamiUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonationRequest_DamiUser_DamiUserId",
                table: "DonationRequest",
                column: "DamiUserId",
                principalTable: "DamiUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloodTypes_DamiUser_DamiUserId",
                table: "BloodTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_DonationRequest_DamiUser_DamiUserId",
                table: "DonationRequest");

            migrationBuilder.RenameColumn(
                name: "DamiUserId",
                table: "DonationRequest",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DonationRequest_DamiUserId",
                table: "DonationRequest",
                newName: "IX_DonationRequest_UserId");

            migrationBuilder.RenameColumn(
                name: "DamiUserId",
                table: "BloodTypes",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_BloodTypes_DamiUserId",
                table: "BloodTypes",
                newName: "IX_BloodTypes_UserId");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "DonationPosts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "ConversationParticipants",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddForeignKey(
                name: "FK_BloodTypes_DamiUser_UserId",
                table: "BloodTypes",
                column: "UserId",
                principalTable: "DamiUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonationRequest_DamiUser_UserId",
                table: "DonationRequest",
                column: "UserId",
                principalTable: "DamiUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

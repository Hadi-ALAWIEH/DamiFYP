using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DamiFYP.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDamiBadgeAndDonationPostStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DonationPosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DamiBadge",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DamiUserId = table.Column<long>(type: "bigint", nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DonationPoints = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamiBadge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DamiBadge_DamiUser_DamiUserId",
                        column: x => x.DamiUserId,
                        principalTable: "DamiUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DamiBadge_DamiUserId",
                table: "DamiBadge",
                column: "DamiUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DamiBadge");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DonationPosts");
        }
    }
}
